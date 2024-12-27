using Content.Server.NPC.HTN;
using Content.Shared._White.Intent;
using Content.Shared.Alert;
using JetBrains.Annotations;

namespace Content.Server._White.Intent;

public sealed class IntentSystem : SharedIntentSystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<ToggleNetIntentEvent>(OnIntentState);
    }

    public override void OnStartup(EntityUid uid, IntentComponent component, ComponentStartup args)
    {
        base.OnStartup(uid, component, args);

        RefreshAlert(uid, component);
    }

    public override void OnShutdown(EntityUid uid, IntentComponent component, ComponentShutdown args)
    {
        base.OnShutdown(uid, component, args);

        _alerts.ClearAlertCategory(uid, component.IntentHelpCategory);
        _alerts.ClearAlertCategory(uid, component.IntentDisarmCategory);
        _alerts.ClearAlertCategory(uid, component.IntentGrabCategory);
        _alerts.ClearAlertCategory(uid, component.IntentHarmCategory);
    }

    private void OnIntentState(ToggleNetIntentEvent ev, EntitySessionEventArgs args)
    {
        if (!args.SenderSession.AttachedEntity.HasValue)
            return;

        var uid = args.SenderSession.AttachedEntity.Value;
        SetIntent(uid, ev.Intent);
    }

    protected override bool IsNpc(EntityUid uid)
    {
        return HasComp<HTNComponent>(uid);
    }

    public override void SetIntent(EntityUid uid, Content.Shared._White.Intent.Intent intent = Content.Shared._White.Intent.Intent.Help, IntentComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        base.SetIntent(uid, intent, component);
        RefreshAlert(uid, component);

        //UpdateActions(uid, component);
    }

    public void RefreshAlert(EntityUid uid, IntentComponent comp)
    {
        _alerts.ShowAlert(uid, comp.IntentHelpAlert, comp.Intent == Content.Shared._White.Intent.Intent.Help ? (short) 1 : (short) 0);
        _alerts.ShowAlert(uid, comp.IntentDisarmAlert, comp.Intent == Content.Shared._White.Intent.Intent.Disarm ? (short) 1 : (short) 0);
        _alerts.ShowAlert(uid, comp.IntentGrabAlert, comp.Intent == Content.Shared._White.Intent.Intent.Grab ? (short) 1 : (short) 0);
        _alerts.ShowAlert(uid, comp.IntentHarmAlert, comp.Intent == Content.Shared._White.Intent.Intent.Harm ? (short) 1 : (short) 0);
    }
}

// TODO: Shieeeet... I need refactor this later

[UsedImplicitly, DataDefinition]
public sealed partial class ToggleIntentHelpAlert : IAlertClick
{
    public void AlertClicked(EntityUid uid)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();

        if (!entityManager.TryGetComponent(uid, out IntentComponent? comp))
            return;

        entityManager.System<IntentSystem>().SetIntent(uid, Content.Shared._White.Intent.Intent.Help, comp);
    }
}

[UsedImplicitly, DataDefinition]
public sealed partial class ToggleIntentDisarmAlert : IAlertClick
{
    public void AlertClicked(EntityUid uid)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();

        if (!entityManager.TryGetComponent(uid, out IntentComponent? comp))
            return;

        entityManager.System<IntentSystem>().SetIntent(uid, Content.Shared._White.Intent.Intent.Disarm, comp);
    }
}

[UsedImplicitly, DataDefinition]
public sealed partial class ToggleIntentGrabAlert : IAlertClick
{
    public void AlertClicked(EntityUid uid)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();

        if (!entityManager.TryGetComponent(uid, out IntentComponent? comp))
            return;

        entityManager.System<IntentSystem>().SetIntent(uid, Content.Shared._White.Intent.Intent.Grab, comp);
    }
}

[UsedImplicitly, DataDefinition]
public sealed partial class ToggleIntentHarmAlert : IAlertClick
{
    public void AlertClicked(EntityUid uid)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();

        if (!entityManager.TryGetComponent(uid, out IntentComponent? comp))
            return;

        entityManager.System<IntentSystem>().SetIntent(uid, Content.Shared._White.Intent.Intent.Harm, comp);
    }
}
