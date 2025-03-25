namespace Content.Server._ES14.Weight.Events;

public sealed class ESWeightGetModifierEvent : EntityEventArgs
{
    public float InsideWeightModifier = 1f;
    public float SelfWeightModifier = 1f;
}