using System.Diagnostics.CodeAnalysis;
using Content.KayMisaZlevels.Server.Components;
using Content.KayMisaZlevels.Shared.Components;
using Content.KayMisaZlevels.Shared.Systems;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.KayMisaZlevels.Server.Systems;

public sealed partial class ZDefinedStackSystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly ZStackSystem _zStack = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ZDefinedStackComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<ZDefinedStackComponent> initialMapUid, ref MapInitEvent args)
    {
        // We should use our initial map as stack for Z levels
        var stackLoc = (EntityUid?) initialMapUid;
        if (!TryComp<ZStackTrackerComponent>(initialMapUid, out var zStackTrackerComp))
            AddComp<ZStackTrackerComponent>(initialMapUid);

        // Load levels downer
        foreach (var path in initialMapUid.Comp.DownLevels)
        {
            LoadLevel(stackLoc, path);
        }

        // Add initial map as lowest level in the world
        _zStack.AddToStack(initialMapUid, ref stackLoc);

        // Load level upper
        foreach (var path in initialMapUid.Comp.UpLevels)
        {
            LoadLevel(stackLoc, path);
        }
    }

    private void LoadLevel(EntityUid? stackLoc, ResPath path)
    {
        var mapUid = _map.CreateMap(out var mapId);
        var options = new MapLoadOptions { LoadMap = true };

        if (_mapLoader.TryLoad(mapId, path.ToString(), out var roots, options))
        {
            _zStack.AddToStack(roots[0], ref stackLoc);
            Log.Info($"Created map {mapId} for ZDefinedStackSystem system");
        }
        else
        {
            Log.Error($"Failed to load map from {path}!");
            Del(mapUid);
            return;
        }
    }
}
