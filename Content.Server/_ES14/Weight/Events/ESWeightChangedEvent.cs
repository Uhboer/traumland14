namespace Content.Server._ES14.Weight.Events;

public sealed class ESWeightChangedEvent(float weight, float oldWeight) : EntityEventArgs
{
    public float Weight = weight;
    public float OldWeight = oldWeight;
}