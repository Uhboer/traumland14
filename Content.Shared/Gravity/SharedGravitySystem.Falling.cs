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
        SubscribeLocalEvent<PhysicsComponent, IsGravityAffectedEvent>(OnGravityAffected);
        SubscribeLocalEvent<PhysicsComponent, ZLevelDroppingEvent>(OnDropping);
        SubscribeLocalEvent<IsGravitySource>(OnGravitySource);
    }

    private void OnDropping(Entity<PhysicsComponent> ent, ref ZLevelDroppingEvent args)
    {
        // Flying object should not dropped on bottom levels.
        if (ent.Comp.BodyStatus == BodyStatus.InAir)
            args.Handled = true;
    }

    private void OnGravityAffected(Entity<PhysicsComponent> ent, ref IsGravityAffectedEvent args)
    {
        // FIXME: By some reasons if we try to set position from client - it cause crash.
        // So, until i can find a way to fix - it would be disabled on client.
        if (_net.IsClient)
            return;

        if (!_xformQuery.TryComp(ent, out var xform))
            return;

        ContentTileDefinition? tileDef = null;

        // Don't bother getting the tiledef here if we're weightless or in-air
        // since no tile-based modifiers should be applying in that situation
        if (xform.MapUid != null &&
            _mapManager.TryFindGridAt(xform.MapUid.Value, xform.WorldPosition, out var _, out var gridComp) &&
            _mapSystem.TryGetTile(gridComp, xform.Coordinates.ToVector2i(EntityManager, _mapManager, _xform), out var tile))
        {
            tileDef = (ContentTileDefinition) _tileDefinitionManager[tile.TypeId];
        }

        // We can drop only on empty tiles.
        if (tileDef is null || tileDef.ID == ContentTileDefinition.SpaceID)
            args.Affected = true;
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
