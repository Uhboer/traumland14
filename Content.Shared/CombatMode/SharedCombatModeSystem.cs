using Content.Shared.Actions;
using Content.Shared.Damage.Systems;
using Content.Shared.MouseRotator;
using Content.Shared.Movement.Components;
using Content.Shared.Popups;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared.CombatMode;

public abstract class SharedCombatModeSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private   readonly INetManager _netMan = default!;
    [Dependency] private   readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private   readonly SharedPopupSystem _popup = default!;
    [Dependency] private   readonly StaminaSystem _stamina = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CombatModeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CombatModeComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CombatModeComponent, ToggleCombatActionEvent>(OnActionPerform);
    }

    private void OnMapInit(EntityUid uid, CombatModeComponent component, MapInitEvent args)
    {
        //_actionsSystem.AddAction(uid, ref component.CombatToggleActionEntity, component.CombatToggleAction);
        Dirty(uid, component);
    }

    public virtual void OnShutdown(EntityUid uid, CombatModeComponent component, ComponentShutdown args)
    {
        //_actionsSystem.RemoveAction(uid, component.CombatToggleActionEntity);

        // Don't use it, because we already has FixedEye button
        //SetMouseRotatorComponents(uid, false);
    }

    private void OnActionPerform(EntityUid uid, CombatModeComponent component, ToggleCombatActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        PerformAction(uid, component, args.Performer);
    }

    public void PerformAction(EntityUid uid, CombatModeComponent component, EntityUid performer)
    {
        SetInCombatMode(uid, !component.IsInCombatMode, component);

        // TODO better handling of predicted pop-ups.
        // This probably breaks if the client has prediction disabled.

        if (!_netMan.IsClient || !Timing.IsFirstTimePredicted)
            return;

        var msg = component.IsInCombatMode ? "action-popup-combat-enabled" : "action-popup-combat-disabled";
        _popup.PopupEntity(Loc.GetString(msg), performer, performer);
    }

    public bool IsInCombatMode(EntityUid? entity, CombatModeComponent? component = null)
    {
        return entity != null && Resolve(entity.Value, ref component, false) && component.IsInCombatMode;
    }

    public virtual void SetInCombatMode(EntityUid entity, bool value, CombatModeComponent? component = null)
    {
        if (!Resolve(entity, ref component))
            return;

        if (component.IsInCombatMode == value)
            return;

        // Apply stamina draining on entity, when combat mode is enabled.
        // If disabled - remove draining
        if (!value && component.StaminaDrainSource is not null)
        {
            _stamina.ToggleStaminaDrain(entity, component.StaminaDrainRate, false, false, source: component.StaminaDrainSource);
            QueueDel(component.StaminaDrainSource);
            component.StaminaDrainSource = null;
        }
        else if (component.IsDrainable)
        {
            var drainEnt = Spawn(null, MapCoordinates.Nullspace);
            _stamina.ToggleStaminaDrain(entity, component.StaminaDrainRate, true, false, source: drainEnt);
            component.StaminaDrainSource = drainEnt;
        }

        component.IsInCombatMode = value;
        Dirty(entity, component);

        //if (component.CombatToggleActionEntity != null)
        //    _actionsSystem.SetToggled(component.CombatToggleActionEntity, component.IsInCombatMode);

        // Change mouse rotator comps if flag is set
        if (!component.ToggleMouseRotator || IsNpc(entity))
            return;

        // Don't use it, because we already has FixedEye button
        //SetMouseRotatorComponents(entity, value);
    }

    private void SetMouseRotatorComponents(EntityUid uid, bool value)
    {
        if (value)
        {
            var rot = EnsureComp<MouseRotatorComponent>(uid);
            // BACKMEN EDIT START
            if (TryComp<CombatModeComponent>(uid, out var comp) && comp.SmoothRotation) // no idea under which (intended) circumstances this can fail (if any), so i'll avoid Comp<>().
            {
                rot.AngleTolerance = Angle.FromDegrees(1); // arbitrary
                rot.Simple4DirMode = false;
            }
            // BACKMEN EDIT END
            EnsureComp<NoRotateOnMoveComponent>(uid);
        }
        else
        {
            RemComp<MouseRotatorComponent>(uid);
            RemComp<NoRotateOnMoveComponent>(uid);
        }
    }

    // todo: When we stop making fucking garbage abstract shared components, remove this shit too.
    protected abstract bool IsNpc(EntityUid uid);
}

public sealed partial class ToggleCombatActionEvent : InstantActionEvent
{

}
