using Content.Server.Cuffs;
using Content.Shared.ActionBlocker;
using Content.Shared.Alert;
using Content.Shared.Buckle;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using JetBrains.Annotations;

namespace Content.Server._Finster.Alert.Click;

/// <summary>
/// Stop pulling something
/// </summary>
[UsedImplicitly]
[DataDefinition]
public sealed partial class TryResist : IAlertClick
{
    public void AlertClicked(EntityUid player)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var buckleSystem = entityManager.System<SharedBuckleSystem>();
        var pullingSystem = entityManager.System<PullingSystem>();

        // Unbuckle
        var unbuckled = buckleSystem.TryUnbuckle(player, player);
        // Unbuckle - End

        // Cuffed
        var cuffableSys = entityManager.System<CuffableSystem>();
        cuffableSys.TryUncuff(player, player);
        // Cuffed - End

        // TODO: Should be reworked. Because entity can garb another entity
        // Pulled
        if (!entityManager.System<ActionBlockerSystem>().CanInteract(player, null))
            return;

        if (entityManager.TryGetComponent(player, out PullableComponent? playerPullable))
            pullingSystem.TryStopPull(player, playerPullable, user: player);
        // Pulled - End
    }
}
