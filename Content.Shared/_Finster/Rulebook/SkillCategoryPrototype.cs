using Robust.Shared.Prototypes;

namespace Content.Shared._Finster.Rulebook;

[Prototype("skillCategory")]
public sealed partial class SkillCategoryPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = string.Empty;
}
