using Content.Shared._ES14.Weight;
using Robust.Shared.GameStates;

namespace Content.Shared._ES14.Weight.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ESWeightComponent : Component
{
    [ViewVariables]
    public float Total => ModifiedSelf + InsideWeight;

    [DataField]
    public float Self = 0.05f;

    [DataField]
    public bool HideInExamine = false;

    [ViewVariables]
    public float ModifiedSelf;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public float InsideWeight;
}