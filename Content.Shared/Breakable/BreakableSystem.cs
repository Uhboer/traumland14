using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Interaction.Events;
using Content.Shared.NameModifier.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;

namespace Content.Shared.Breakable;

/// <summary>
/// This handles guns and melee weapon break. Also changing metadata.
/// <seealso cref="BreakableComponent"/>>
/// </summary>
public sealed class BreakableSystem : EntitySystem
{
    [Dependency] private readonly NameModifierSystem _nameMod = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private EntityQuery<BreakableComponent> _breakableQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<BreakableComponent, BreakageEventArgs>(OnBreak);
        SubscribeLocalEvent<BreakableComponent, RefreshNameModifiersEvent>(OnRefreshNameModifiers);
        SubscribeLocalEvent<BreakableComponent, AttemptShootEvent>(OnShotAttempt);
        SubscribeLocalEvent<CombatModeComponent, AttackAttemptEvent>(OnAttackAttempt);

        SubscribeLocalEvent<BreakableComponent, MeleeHitEvent>(OnHit);
        SubscribeLocalEvent<BreakableComponent, GunShotEvent>(OnGunShoot);

        _breakableQuery = GetEntityQuery<BreakableComponent>();
    }

    private void OnHit(Entity<BreakableComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0 || !args.IsHit)
            return;
        _damageable.TryChangeDamage(ent, ent.Comp.Damage);
    }

    private void OnGunShoot(Entity<BreakableComponent> ent, ref GunShotEvent args)
    {
        _damageable.TryChangeDamage(ent, ent.Comp.Damage);
    }

    private void OnAttackAttempt(EntityUid uid, CombatModeComponent comp, AttackAttemptEvent ev)
    {
        if (!_breakableQuery.TryComp(ev.Weapon, out var breakableComponent))
            return;
        if (!breakableComponent.IsBroken)
            return;
        ev.Cancel();
    }

    private void OnShotAttempt(Entity<BreakableComponent> ent, ref AttemptShootEvent args)
    {
        if (!ent.Comp.IsBroken)
            return;
        args.Message = Loc.GetString("breakable-weapon-is-broken");
        args.Cancelled = true;
        args.ThrowItems = true;
    }

    private void OnRefreshNameModifiers(Entity<BreakableComponent> ent, ref RefreshNameModifiersEvent args)
    {
        if (ent.Comp.IsBroken)
            args.AddModifier("broken-name-prefix");
    }

    private void OnBreak(Entity<BreakableComponent> ent, ref BreakageEventArgs args)
    {
        ent.Comp.IsBroken = true;
        Dirty(ent, ent.Comp);
        _audio.PlayPvs(ent.Comp.BreakSound, ent);
        _nameMod.RefreshNameModifiers(ent.Owner);
        _popup.PopupEntity(
            Loc.GetString("breakable-breaks", ("target", ent.Owner)),
            ent,
            PopupType.LargeCaution);
    }
}
