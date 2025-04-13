using Content.KayMisaZlevels.Shared.Miscellaneous;
using Content.KayMisaZlevels.Shared.Systems;
using Content.Shared.Jumping;
using Content.Shared.Maps;
using Content.Shared.Projectiles;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;

namespace Content.Shared.Gravity;

public abstract partial class SharedGravitySystem
{
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

        if (!_xformQuery.TryComp(args.Target, out var xform))
            return;

        if (!_physicsQuery.TryComp(args.Target, out var physicsComponent))
            return;

        ContentTileDefinition? tileDef = null;

        // Don't bother getting the tiledef here if we're weightless or in-air
        // since no tile-based modifiers should be applying in that situation
        if (xform.MapUid != null &&
            _mapManager.TryFindGridAt(xform.MapUid.Value, xform.WorldPosition, out var uid, out var gridComp) &&
            _mapSystem.TryGetTile(gridComp, xform.Coordinates.ToVector2i(EntityManager, _mapManager, _xform), out var tile))
        {
            tileDef = (ContentTileDefinition) _tileDefinitionManager[tile.TypeId];
        }

        if (tileDef is null || tileDef.ID == ContentTileDefinition.SpaceID)
        {
            if (physicsComponent.BodyStatus == BodyStatus.InAir)
            {
                if (_projectileQuery.TryComp(args.Target, out var proj))
                {
                    if (proj.TargetMap == Transform(args.Target).MapUid)
                        return;
                }
                //else if (!TryComp<JumpingComponent>(args.Target, out var jumpComponent) || !jumpComponent.IsFailed)
                //    return;
            }
            args.Affected = true;
        }
    }

    private void OnGravitySource(ref IsGravitySource args)
    {
        // FIXME: By some reasons if we try to set position from client - it cause crash.
        // So, until i can find a way to fix - it would be disabled on client.
        if (_net.IsClient)
            return;

        if (_zPhysics.TryGetTileWithEntity(args.Entity, ZDirection.Down, out var _, out var _, out var targetMap) &&
            _gravityQuery.TryComp(targetMap, out var comp))
            args.Handled = comp.Enabled;
        else // not founded
            args.Handled = false;
    }
}
