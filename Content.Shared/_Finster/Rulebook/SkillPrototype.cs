using Robust.Shared.Prototypes;

namespace Content.Shared._Finster.Rulebook;

[Prototype("skill")]
public sealed partial class SkillPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = string.Empty;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public Color Color { get; set; } = new Color(255, 255, 255);

    [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
    public ProtoId<SkillCategoryPrototype> Category = default!;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public List<Enum> Blocked = new();
}
