using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._Finster.Teleporter;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedTeleporterSystem))]
public sealed partial class TeleporterComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2 Adjust;
}
