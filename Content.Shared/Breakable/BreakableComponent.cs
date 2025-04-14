using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Breakable;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BreakableComponent : Component
{
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public bool IsBroken;

    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 0.9f },
            { "Structural", 0.1f },
        },
    };
}
