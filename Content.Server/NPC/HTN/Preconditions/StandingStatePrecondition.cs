using Content.Shared.Standing;

namespace Content.Server.NPC.HTN.Preconditions;

public sealed partial class StandingStatePrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("isStanding")]
    public bool IsStanding = true;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!_entManager.TryGetComponent<StandingStateComponent>(owner, out var standing))
            return false;

        return IsStanding && standing.CurrentState == StandingState.Standing ||
               !IsStanding && standing.CurrentState != StandingState.Standing;
    }
}