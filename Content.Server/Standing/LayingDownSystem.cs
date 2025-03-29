using Content.Shared.Standing;
using Content.Shared.Body.Components; // Goobstation
using Content.Shared.Body.Part; // Goobstation
using Content.Shared.CCVar;
using Content.Shared.Input;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Shared.Configuration;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Content.Shared.Alert;
using JetBrains.Annotations;

namespace Content.Server.Standing;

public sealed class LayingDownSystem : SharedLayingDownSystem
{
    [Dependency] private readonly INetConfigurationManager _cfg = default!;
    [Dependency] private readonly EntityManager _entMan = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StandingStateComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<StandingStateComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<StandingStateComponent, DownedEvent>(OnDowned);
        SubscribeLocalEvent<StandingStateComponent, StoodEvent>(OnStood);
        SubscribeNetworkEvent<CheckAutoGetUpEvent>(OnCheckAutoGetUp);
    }

    private void OnInit(EntityUid uid, StandingStateComponent component, ComponentStartup args)
    {
        RefreshAlert(uid, component);
    }

    private void OnDowned(EntityUid uid, StandingStateComponent component, ref DownedEvent args)
    {
        RefreshAlert(uid, component);
    }

    private void OnStood(EntityUid uid, StandingStateComponent component, ref StoodEvent args)
    {
        RefreshAlert(uid, component);
    }

    public void OnShutdown(EntityUid uid, StandingStateComponent component, ComponentShutdown args)
    {
        _alerts.ClearAlertCategory(uid, component.LayingCategory);
    }

    public void RefreshAlert(EntityUid uid, StandingStateComponent comp)
    {
        var state = 0;
        if (comp.CurrentState == StandingState.Lying)
            state = 1;
        _alerts.ShowAlert(uid, comp.LayingAlert, (short) state);
    }

    public void ToggleStanding(EntityUid uid)
    {
        if (!TryComp(uid, out StandingStateComponent? standing)
            || !TryComp(uid, out LayingDownComponent? layingDown))
            return;

        if (_standing.IsDown(uid, standing))
            TryStandUp(uid, layingDown, standing);
        else
            TryLieDown(uid, layingDown, standing);
    }

    private void OnCheckAutoGetUp(CheckAutoGetUpEvent ev, EntitySessionEventArgs args)
    {
        var uid = GetEntity(ev.User);

        if (!TryComp(uid, out LayingDownComponent? layingDown))
            return;

        // Goobstation start
        bool fullyParalyzed = false;

        if (_entMan.TryGetComponent<BodyComponent>(uid, out var body))
        {
            foreach (var legEntity in body.LegEntities)
            {
                if (_entMan.TryGetComponent<BodyPartComponent>(legEntity, out var partCmp))
                {
                    if (partCmp.Enabled != true)
                    {
                        fullyParalyzed = true;
                        continue;
                    } else if (partCmp.Enabled == true)
                    {
                        fullyParalyzed = false;
                        break;
                    }
                }
            }
        }

        if (fullyParalyzed)
        {
            layingDown.AutoGetUp = false;
            Dirty(uid, layingDown);
            return;
        }
        // Goobstation end

        layingDown.AutoGetUp = _cfg.GetClientCVar(args.SenderSession.Channel, CCVars.AutoGetUp);
        Dirty(uid, layingDown);
    }
}

[UsedImplicitly, DataDefinition]
public sealed partial class ToggleLayingModeAlert : IAlertClick
{
    public void AlertClicked(EntityUid uid)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        entityManager.System<LayingDownSystem>().ToggleStanding(uid);
    }
}
