using Robust.Shared.Prototypes;

namespace Content.Shared._Finster.Rulebook;

[Prototype("attribute")]
public sealed partial class AttributePrototype : IPrototype
{
    [IdDataField] public string ID { get; } = string.Empty;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public Color Color { get; set; } = new Color(255, 255, 255);
}
