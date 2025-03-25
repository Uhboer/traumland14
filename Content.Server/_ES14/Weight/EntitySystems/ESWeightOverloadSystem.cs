using Content.Server._ES14.Weight.Components;
using Content.Server._ES14.Weight.Events;
using Content.Server.Alert;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Timing;

namespace Content.Server._ES14.Weight.EntitySystems;

public sealed class ESWeightOverloadSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ThirstSystem _thirst = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ESWeightOverloadComponent, ESWeightChangedEvent>(OnWeightChanged);
        SubscribeLocalEvent<ESWeightOverloadComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeedModifiers);
    }

    private void OnRefreshSpeedModifiers(Entity<ESWeightOverloadComponent> ent,
        ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(ent.Comp.MovementSpeedModifier, ent.Comp.MovementSpeedModifier);
    }

    private static ESOverloadLevel CalculateOverloadLevel(ESWeightOverloadComponent comp, float weight)
    {
        if (comp.SlightOverload > weight)
            return ESOverloadLevel.NoOverload;
        if (weight < comp.Overload)
            return ESOverloadLevel.SlightOverload;
        return weight <= comp.SevereOverload ? ESOverloadLevel.Overload : ESOverloadLevel.SevereOverload;
    }


    private void OnWeightChanged(Entity<ESWeightOverloadComponent> ent, ref ESWeightChangedEvent args)
    {
        var currentLevel = CalculateOverloadLevel(ent, args.Weight);
        if (currentLevel == ent.Comp.OverloadLevel)
            return;

        ent.Comp.OverloadLevel = currentLevel;

        ent.Comp.MovementSpeedModifier =
            Math.Clamp(1 - (args.Weight - ent.Comp.Overload) / (ent.Comp.Overload * 0.75f), 0.25f, 1f);
        ent.Comp.ThirstModifier =
            Math.Clamp((args.Weight - ent.Comp.SlightOverload) / ent.Comp.SlightOverload * 8, 0f, 4f);
        ent.Comp.HungerModifier =
            Math.Clamp((args.Weight - ent.Comp.SlightOverload) / ent.Comp.SlightOverload * 8, 0f, 4f);
        //ent.Comp.OverloadDamageModifier =
        //    Math.Clamp((args.Weight - ent.Comp.SevereOverload) / ent.Comp.SevereOverload * 6, 0f, 4f);
        _movementSpeedModifier.RefreshMovementSpeedModifiers(ent);
    }

    private void DealEffects(Entity<ESWeightOverloadComponent> ent)
    {
        if (TryComp<MobStateComponent>(ent, out var mobState))
        {
            if (mobState.CurrentState == MobState.Dead)
                return;
        }

        if (ent.Comp.OverloadLevel == ESOverloadLevel.NoOverload)
            return;
        DealHungerAndThirst(ent);
        // Im not sure with it.
        //if (ent.Comp.OverloadLevel == ESOverloadLevel.SevereOverload)
        //    DealDamage(ent);
    }

    private void DealHungerAndThirst(Entity<ESWeightOverloadComponent> ent)
    {
        if (TryComp<ThirstComponent>(ent, out var thirst))
            _thirst.ModifyThirst(ent, thirst, -(thirst.ActualDecayRate * ent.Comp.ThirstModifier));
        if (TryComp<HungerComponent>(ent, out var hunger))
            _hunger.ModifyHunger(ent, -(hunger.ActualDecayRate * ent.Comp.ThirstModifier), hunger);
    }

    /*
    private void DealDamage(Entity<ESWeightOverloadComponent> ent)
    {
        _damageable.TryChangeDamage(ent, ent.Comp.OverloadDamage * ent.Comp.OverloadDamageModifier);
    }
    */

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ESWeightOverloadComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextUpdateTime)
                continue;
            DealEffects((uid, comp));
            comp.NextUpdateTime = _timing.CurTime + comp.UpdateRate;
        }
    }
}