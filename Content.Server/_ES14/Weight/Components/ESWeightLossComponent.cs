namespace Content.Server._ES14.Weight.Components;

[RegisterComponent]
public sealed partial class ESWeightLossComponent : Component
{
    [DataField]
    public float InsideWeightLossModifier = 0.9f;
}