using Content.Shared.Standing;
using Content.Shared.DoAfter;
using System.Linq;
using Content.Server.Standing;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators;

/// <summary>
/// Пытаемся заставить NPC встать, если он еще не стоит.
/// </summary>
public sealed partial class StandUpOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private Shared.Standing.StandingStateSystem _standing = default!;
    private LayingDownSystem _laying = default!;

    [DataField("shutdownState")]
    public HTNPlanState ShutdownState { get; private set; } = HTNPlanState.TaskFinished;

    private EntityQuery<StandingStateComponent> _standingQuery;
    private EntityQuery<DoAfterComponent> _doAfterQuery;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _standing = sysManager.GetEntitySystem<Shared.Standing.StandingStateSystem>();
        _laying = sysManager.GetEntitySystem<LayingDownSystem>();
        _standingQuery = _entManager.GetEntityQuery<StandingStateComponent>();
        _doAfterQuery = _entManager.GetEntityQuery<DoAfterComponent>();
    }

    public override void Startup(NPCBlackboard blackboard)
    {
        base.Startup(blackboard);
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!_standingQuery.TryGetComponent(owner, out var standing) ||
            standing.CurrentState == StandingState.Standing)
            return;

        if (_doAfterQuery.TryGetComponent(owner, out var doAfter) &&
            doAfter.DoAfters.Values.Any(x => x.Args.Event is StandingUpDoAfterEvent && !x.Cancelled && !x.Completed))
            return;

        _entManager.Dirty(owner, standing);
        _standing.Down(owner);
        _laying.TryStandUp(owner, standingState: standing);
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!_standingQuery.TryGetComponent(owner, out var standing) ||
            standing.CurrentState == StandingState.Standing)
            return HTNOperatorStatus.Finished;

        if (_doAfterQuery.TryGetComponent(owner, out var doAfter) &&
            doAfter.DoAfters.Values.Any(x => x.Args.Event is StandingUpDoAfterEvent && !x.Cancelled && !x.Completed))
            return HTNOperatorStatus.Continuing;

        _entManager.Dirty(owner, standing);
        _standing.Down(owner);
        _laying.TryStandUp(owner, standingState: standing);

        return HTNOperatorStatus.Continuing;
    }
}