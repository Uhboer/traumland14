using Content.Server.Maps;
using Content.Shared.GridPreloader.Prototypes;
using Content.Shared.Storage;
using Content.Shared.Whitelist;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// This is used for a game rule that loads a map when activated.
/// </summary>
[RegisterComponent]
public sealed partial class LoadMapRuleComponent : Component
{
    [DataField]
    public MapId? Map;

    [DataField]
    public ProtoId<GameMapPrototype>? GameMap;

    /// <summary>
    /// A map to load.
    /// </summary>
    [DataField]
    public ResPath? MapPath;

    /// <summary>
    /// A <see cref="PreloadedGridPrototype"/> to move to a new map.
    /// If there are no instances left nothing is done.
    /// </summary>
    [DataField]
    public ResPath? GridPath;

    [DataField]
    public ProtoId<PreloadedGridPrototype>? PreloadedGrid;

    [DataField]
    public List<EntityUid> MapGrids = new();

    [DataField]
    public EntityWhitelist? SpawnerWhitelist;
}
