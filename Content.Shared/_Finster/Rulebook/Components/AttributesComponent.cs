using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Finster.Rulebook;

/// <summary>
/// All attributes from the character for using role/dice systems.
/// It also can be applied for the another entity, if we wanna use RolePlay mechanics on them.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AttributesComponent : Component
{
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public Dictionary<ProtoId<AttributePrototype>, Enum> Attributes { get; set; } = new();

    /// <summary>
    /// Calculate modifier, given by the attribute.
    /// </summary>
    /// <param name="attributeValue">Attribute?</param>
    /// <returns></returns>
    public int GetModifier(int attributeValue)
    {
        return (attributeValue - 10) / 2;
    }
}
