using Content.Server.NPC.HTN;
using Content.Shared.Alert;
using Content.Shared.CombatMode;
using JetBrains.Annotations;

namespace Content.Server.CombatMode;

public sealed class CombatModeSystem : SharedCombatModeSystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CombatModeComponent, ComponentStartup>(OnInit);
        SubscribeNetworkEvent<ToggleCombatModeEvent>(OnToggleCombatMode);

    }

    private void OnInit(EntityUid uid, CombatModeComponent component, ComponentStartup args)
    {
        RefreshAlert(uid, component);
    }

    public override void OnShutdown(EntityUid uid, CombatModeComponent component, ComponentShutdown args)
    {
        base.OnShutdown(uid, component, args);

        _alerts.ClearAlertCategory(uid, component.CombatModeCategory);
    }

    public void OnToggleCombatMode(ToggleCombatModeEvent ev, EntitySessionEventArgs args)
    {
        if (!args.SenderSession.AttachedEntity.HasValue)
            return;

        var uid = args.SenderSession.AttachedEntity.Value;
        if (!TryComp(uid, out CombatModeComponent? comp))
            return;

        ToggleCombatMode(uid, comp);
    }

    public void ToggleCombatMode(EntityUid uid, CombatModeComponent comp)
    {
        PerformAction(uid, comp, uid);
        RefreshAlert(uid, comp);
    }

    public void RefreshAlert(EntityUid uid, CombatModeComponent comp)
    {
        _alerts.ShowAlert(uid, comp.CombatModeAlert, comp.IsInCombatMode ? (short) 1 : (short) 0);
    }

    protected override bool IsNpc(EntityUid uid)
    {
        return HasComp<HTNComponent>(uid);
    }
}

[UsedImplicitly, DataDefinition]
public sealed partial class ToggleCombatModeAlert : IAlertClick
{
    public void AlertClicked(EntityUid uid)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();

        if (!entityManager.TryGetComponent(uid, out CombatModeComponent? comp))
            return;

        entityManager.System<CombatModeSystem>().ToggleCombatMode(uid, comp);
    }
}
