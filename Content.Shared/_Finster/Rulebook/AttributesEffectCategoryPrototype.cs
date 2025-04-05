using Robust.Shared.Prototypes;

namespace Content.Shared._Finster.Rulebook;

[Prototype]
public sealed partial class AttributesEffectCategoryPrototype : IPrototype
{
    /// <summary>
    ///     The ID of the moodlet to use.
    /// </summary>
    [IdDataField]
    public string ID { get; } = default!;
    public string Description => Loc.GetString($"attributes-effect-category-{ID}");
}
