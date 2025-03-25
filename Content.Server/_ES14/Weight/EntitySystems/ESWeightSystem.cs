using System.Diagnostics.CodeAnalysis;
using Content.Shared._ES14.Weight.Components;
using Content.Server._ES14.Weight.Events;
using Content.Shared.Throwing;
using JetBrains.Annotations;
using Content.Shared.Movement.Components;
using Robust.Shared.Physics.Components;

namespace Content.Server._ES14.Weight.EntitySystems;

public sealed partial class ESWeightSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<ESWeightComponent, EntParentChangedMessage>(OnParentChanged);
        SubscribeLocalEvent<ESWeightComponent, ComponentStartup>(OnComponentStartup);

        InitializeStack();
        InitializeExamine();
    }

    private void OnComponentStartup(Entity<ESWeightComponent> ent, ref ComponentStartup args)
    {
        // For characters and mobs
        if (TryComp<MobMoverComponent>(ent, out var _) &&
            TryComp<PhysicsComponent>(ent, out var physics))
            ent.Comp.Self = physics.Mass; // Average weight is 71f

        UpdateWeight(ent, updateParent: false);
        // Компонент веса инициализирован, скажем об этом другим системам
        RaiseLocalEvent(ent, new ESWeightChangedEvent(ent.Comp.Total, ent.Comp.Total));
    }

    private void OnParentChanged(Entity<ESWeightComponent> ent, ref EntParentChangedMessage args)
    {
        UpdateWeight(ent);
        if (args.OldParent != null)
            TryUpdateWeight((args.OldParent.Value, null));
    }


    private void UpdateWeight(Entity<ESWeightComponent> ent, bool updateParent = true)
    {
        var transform = Transform(ent);

        float newInside = 0;
        var enumerator = transform.ChildEnumerator;
        while (enumerator.MoveNext(out var uid))
        {
            if (!TryGetWeight(uid, out var childWeight))
                continue;

            newInside += childWeight.Value;
        }

        ent.Comp.ModifiedSelf = ent.Comp.Self;

        var ev = new ESWeightGetModifierEvent();
        RaiseLocalEvent(ent, ev);
        ent.Comp.InsideWeight *= ev.InsideWeightModifier;
        ent.Comp.ModifiedSelf *= ev.SelfWeightModifier;

        if (newInside != ent.Comp.InsideWeight)
        {
            var oldTotal = ent.Comp.Total;
            ent.Comp.InsideWeight = newInside;
            RaiseLocalEvent(ent, new ESWeightChangedEvent(ent.Comp.Total, oldTotal));
        }

        if (updateParent)
            UpdateParent(ent);
    }

    [PublicAPI]
    public bool TryUpdateWeight(Entity<ESWeightComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, logMissing: false))
            return false;


        UpdateWeight((ent, ent.Comp));
        return true;
    }

    [PublicAPI]
    public bool TryGetWeight(Entity<ESWeightComponent?> entity, [NotNullWhen(true)] out float? weight)
    {
        weight = null;
        if (!Resolve(entity.Owner, ref entity.Comp, logMissing: false))
            return false;

        weight = entity.Comp.Total;
        return true;
    }

    [PublicAPI]
    public bool TryChangeWeight(Entity<ESWeightComponent?> entity, float value)
    {
        if (!Resolve(entity.Owner, ref entity.Comp, logMissing: false))
            return false;
        if (value <= 0)
            return false;

        var oldTotal = entity.Comp.Total;

        entity.Comp.Self += value;

        UpdateWeight(entity!);

        RaiseLocalEvent(entity, new ESWeightChangedEvent(entity.Comp.Total, oldTotal));
        return true;
    }

    private void UpdateParent(EntityUid entityUid)
    {
        var parent = Transform(entityUid).ParentUid;
        if (!TryComp<ESWeightComponent>(parent, out var comp))
            return;
        UpdateWeight((parent, comp));
    }
}