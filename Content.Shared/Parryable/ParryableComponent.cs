using Robust.Shared.GameStates;

namespace Content.Shared._Finster.Rulebook;

/// <summary>
/// All attributes from the character for using role/dice systems.
/// It also can be applied for the another entity, if we wanna use RolePlay mechanics on them.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ParryableComponent : Component
{
    public TimeSpan NextParry;

    [DataField, AutoNetworkedField]
    public float ParryRate = 1.5f;
}
