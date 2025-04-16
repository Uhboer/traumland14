using Content.Shared.Alert;
using Content.Shared.Input;
using Content.Shared.Jumping;
using Content.Shared.MiddleClick;
using JetBrains.Annotations;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.MiddleClick;

public sealed class MiddleClickSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly JumpingSystem _jumping = default!;

    private readonly ProtoId<AlertPrototype> _kickIntentAlert = "KickIntent";
    private readonly ProtoId<AlertPrototype> _climbIntentAlert = "ClimbIntent";
    private readonly ProtoId<AlertPrototype> _jumpIntentAlert = "JumpIntent";

    private EntityQuery<MiddleClickIntentComponent> _mIntentQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MiddleClickIntentComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MiddleClickIntentComponent, ComponentShutdown>(OnShutdown);

        _mIntentQuery = GetEntityQuery<MiddleClickIntentComponent>();

        CommandBinds.Builder
                .Bind(ContentKeyFunctions.MouseMiddle, new PointerInputCmdHandler(HandleMiddleButton))
                .Register<MiddleClickSystem>();
    }

    public bool HandleMiddleButton(ICommonSession? session, EntityCoordinates coords, EntityUid uid)
    {
        if (session == null || session.AttachedEntity == null ||
            !_mIntentQuery.TryComp(session.AttachedEntity, out var mIntentComp))
            return false;

        switch (mIntentComp.ClickType)
        {
            case MiddleClickType.Kick:
                return false;
            case MiddleClickType.Climb:
                return false;
            case MiddleClickType.Jump:
                return _jumping.TryJump(session.AttachedEntity.Value, coords.Position);
        }

        return true;
    }

    private void OnStartup(Entity<MiddleClickIntentComponent> ent, ref ComponentStartup args)
    {
        RefreshAlert(ent, ent.Comp);
    }

    private void OnShutdown(Entity<MiddleClickIntentComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(ent, _kickIntentAlert);
        _alerts.ClearAlert(ent, _climbIntentAlert);
        _alerts.ClearAlert(ent, _jumpIntentAlert);
    }

    public void RefreshAlert(EntityUid uid, MiddleClickIntentComponent comp)
    {
        _alerts.ShowAlert(uid, _kickIntentAlert, comp.ClickType == MiddleClickType.Kick ? (short) 1 : (short) 0);
        _alerts.ShowAlert(uid, _climbIntentAlert, comp.ClickType == MiddleClickType.Climb ? (short) 1 : (short) 0);
        _alerts.ShowAlert(uid, _jumpIntentAlert, comp.ClickType == MiddleClickType.Jump ? (short) 1 : (short) 0);
    }

    public void SetIntent(EntityUid uid, MiddleClickType? clickType, bool disableIfSame = false, MiddleClickIntentComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        if (clickType is null ||
            clickType == comp.ClickType && disableIfSame)
            comp.ClickType = MiddleClickType.Generic;
        else
            comp.ClickType = clickType.Value;

        Dirty(uid, comp);
        RefreshAlert(uid, comp);
    }
}

[UsedImplicitly, DataDefinition]
public sealed partial class SetAlternativeIntentAlert : IAlertClick
{
    [DataField("intent", required: true)]
    public MiddleClickType Intent { get; set; }

    public void AlertClicked(EntityUid uid)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        entityManager.System<MiddleClickSystem>().SetIntent(uid, Intent, disableIfSame: true);
    }
}
