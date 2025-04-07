using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Server.GameObjects;
using Content.Shared.Maps;
using Content.Shared.Movement.Components;
using Content.Shared.Standing;
using Robust.Shared.Input;
using Content.KayMisaZlevels.Server.Systems;
using Content.Server.DoAfter;
using Content.Shared.DoAfter;
using Content.KayMisaZlevels.Shared.Miscellaneous;
using System.Numerics;

namespace Content.Server.Descending;

public sealed class DescendingSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly TransformSystem _transformSys = default!;
    [Dependency] private readonly ZStackSystem _zStack = default!;
    [Dependency] private readonly MapSystem _mapSys = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        //SubscribeLocalEvent<ClimbingComponent, DescendingDoAfterEvent>(OnDescendingDoAfter);
        /*
        CommandBinds.Builder
            .Bind(
        EngineKeyFunctions.Use,
        new PointerInputCmdHandler(Click)).Register<ClimbingSystem>();
        */
    }

    /*
    private void OnDescendingDoAfter(Entity<ClimbingComponent> entity, ref DescendingDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || entity.Comp.DescendCoords == null)
            return;

        TryDescend(entity.Owner, entity.Comp.DescendCoords.Value, doAftered: true);

        args.Handled = true;
    }
    */

    /*
    public bool Click(ICommonSession? session, EntityCoordinates coords, EntityUid uid)
    {
        if (session?.AttachedEntity != null && TryClimb(session.AttachedEntity.Value, coords))
            return true;
        return false;
    }
    */

    /*
    public bool TryClimb(
        EntityUid uid,
        Vector2 targetPosition,
        ZDirection direction,
        bool doAftered = false,
        ClimbingComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return false;

        // If it is doAftered - then just move the player on position.
        if (doAftered)
        {
            if (comp.DescendCoords == null)
                return false;
            _transformSys.SetCoordinates(uid, comp.DescendCoords.Value);
            return true;
        }

        var xform = Transform(uid);
        if (xform.MapUid == null)
            return false;

        if (!_zStack.TryGetZStack(xform.MapUid.Value, out var zStack))
            return false;

        var maps = zStack.Value.Comp.Maps;
        var mapIdx = maps.IndexOf(xform.MapUid.Value);
        int targetMapIdx = -1;
        if (direction == ZDirection.Down)
        {
            targetMapIdx = mapIdx - 1;

            // Because there is no bottom levels
            if (targetMapIdx < 0)
                return false;
        }
        else
        {
            targetMapIdx = mapIdx + 1;

            // Because there is no top levels
            if (targetMapIdx >= maps.Count)
                return false;
        }

        var userTransf = Transform(uid);
        if ((userTransf.WorldPosition - targetPosition).Length() > comp.DescendRange)
            return false;

        if ((TryComp<InputMoverComponent>(uid, out var inputMoverComp) && !inputMoverComp.CanMove) ||
            (TryComp<StandingStateComponent>(uid, out var standingComp) && standingComp.CurrentState < StandingState.Standing))
            return false;

        // Check grids and another world objects.
        if (direction == ZDirection.Down)
        {
            // No grids founded
            if (!_mapManager.TryFindGridAt(maps[mapIdx], targetPosition, out _, out var grid))
                return false;

            // Check if there is a bottom grid.
            if (!_mapManager.TryFindGridAt(maps[targetMapIdx], targetPosition, out _, out var bottomGrid))
                return false;

            var currentEntityCoords = new EntityCoordinates(maps[mapIdx], targetPosition);
            var descendEntityCoords = new EntityCoordinates(maps[targetMapIdx], targetPosition);

            // Check grid on current map.
            _mapSys.TryGetTile(grid, currentEntityCoords.ToVector2i(EntityManager, _mapManager, _transformSys), out var tile);
            // Tile should be empty.
            if (!tile.IsSpace())
                return false;

            // Check for grid on bottom map. Because we can't descend on the empty tile.
            _mapSys.TryGetTile(grid, descendEntityCoords.ToVector2i(EntityManager, _mapManager, _transformSys), out var bottomTile);
            if (bottomTile.IsSpace())
                return false;

            var doAfterEventArgs = new DoAfterArgs(
                EntityManager,
                uid,
                comp.DoAfterDelay,
                new DescendingDoAfterEvent(),
                uid)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
                NeedHand = true,
                BreakOnWeightlessMove = true,
            };
            _doAfter.TryStartDoAfter(doAfterEventArgs);

            comp.DescendCoords = new EntityCoordinates(maps[mapIdx - 1], coords.Position);
        }
        else
        {

        }

        /*
        if (!_mapManager.TryFindGridAt(maps[mapIdx], coords.Position, out _, out var zGrid))
            return false;

        _mapSys.TryGetTile(zGrid, coords.ToVector2i(EntityManager, IoCManager.Resolve<IMapManager>(), _transformSys), out var tile);
        if (!tile.IsSpace())
            return false;

        var doAfterEventArgs = new DoAfterArgs(
            EntityManager,
            uid,
            comp.DoAfterDelay,
            new DescendingDoAfterEvent(),
            uid)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
            BreakOnWeightlessMove = true,
        };
        _doAfter.TryStartDoAfter(doAfterEventArgs);

        comp.DescendCoords = new EntityCoordinates(maps[mapIdx - 1], coords.Position);

        return true;
    }
    */
}
