using Content.Server._ES14.Weight.Events;
using Content.Shared.Stacks;

namespace Content.Server._ES14.Weight.EntitySystems;

public sealed partial class ESWeightSystem
{
    private void InitializeStack()
    {
        SubscribeLocalEvent<StackComponent, ESWeightGetModifierEvent>(OnGetWeightModifiers);
    }

    private void OnGetWeightModifiers(Entity<StackComponent> ent, ref ESWeightGetModifierEvent args)
    {
        args.SelfWeightModifier *= ent.Comp.Count;
    }
}