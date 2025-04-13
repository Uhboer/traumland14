using Content.KayMisaZlevels.Shared.Miscellaneous;
using Content.KayMisaZlevels.Shared.Systems;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;

namespace Content.Shared.Gravity;

public abstract partial class SharedGravitySystem
{
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;

    public void InitializeFalling()
    {
        SubscribeLocalEvent<IsGravityAffectedEvent>(OnGravityAffected);
        SubscribeLocalEvent<IsGravitySource>(OnGravitySource);
    }

    private void OnGravityAffected(ref IsGravityAffectedEvent args)
    {
        // FIXME: By some reasons if we try to set position from client - it cause crash.
        // So, until i can find a way to fix - it would be disabled on client.
        if (_net.IsClient)
            return;

        if (!TryComp<TransformComponent>(args.Target, out var xform))
            return;

        if (!TryComp<PhysicsComponent>(args.Target, out var physicsComponent))
            return;

        ContentTileDefinition? tileDef = null;

        // Don't bother getting the tiledef here if we're weightless or in-air
        // since no tile-based modifiers should be applying in that situation
        if (TryComp(xform.GridUid, out MapGridComponent? gridComp)
            && _mapSystem.TryGetTileRef(xform.GridUid.Value, gridComp, xform.Coordinates, out var tile))
        {
            tileDef = (ContentTileDefinition) _tileDefinitionManager[tile.Tile.TypeId];
        }

        if ((tileDef is null || tileDef.ID == ContentTileDefinition.SpaceID) && physicsComponent.BodyStatus != BodyStatus.InAir)
            args.Affected = true;
    }

    private void OnGravitySource(ref IsGravitySource args)
    {
        // FIXME: By some reasons if we try to set position from client - it cause crash.
        // So, until i can find a way to fix - it would be disabled on client.
        if (_net.IsClient)
            return;

        if (_zPhysics.TryGetTileWithEntity(args.Entity, ZDirection.Down, out var _, out var _, out var targetMap) &&
            TryComp<GravityComponent>(targetMap, out var comp))
            args.Handled = comp.Enabled;
        else // not founded
            args.Handled = false;
    }
}
