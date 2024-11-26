using Content.Server.Station.Systems;

namespace Content.Server.Station.Components;

/// <summary>
/// Ignore randomize transformation of station grid. It useful for planet maps, where applying rotation an offset causing game crush
/// </summary>
[RegisterComponent, Access(typeof(StationSystem))]
public sealed partial class StationIgnoreTransformComponent : Component
{
}
