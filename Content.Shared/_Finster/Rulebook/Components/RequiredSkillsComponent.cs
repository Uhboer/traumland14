using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Finster.Rulebook;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RequiredSkillsComponent : Component
{
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
    public Dictionary<ProtoId<SkillPrototype>, Enum> Skills = new();
}
