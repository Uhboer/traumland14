using Content.Shared.Alert;
using Content.Shared.Inventory;
using Content.Shared.Movement.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Content.Shared.Flight;
using Content.KayMisaZlevels.Shared.Systems;
using Content.KayMisaZlevels.Shared.Miscellaneous;

namespace Content.Shared.Gravity
{
    public abstract partial class SharedGravitySystem : EntitySystem
    {
        [Dependency] protected readonly IGameTiming Timing = default!;
        [Dependency] private readonly AlertsSystem _alerts = default!;
        [Dependency] private readonly SharedZStackSystem _zStack = default!;
        [Dependency] private readonly ZPhysicsSystem _zPhysics = default!;

        [ValidatePrototypeId<AlertPrototype>]
        //public const string WeightlessAlert = "Weightless";

        private EntityQuery<GravityComponent> _gravityQuery;

        public bool IsWeightless(EntityUid uid, PhysicsComponent? body = null, TransformComponent? xform = null)
        {
            Resolve(uid, ref body, false);

            if ((body?.BodyType & (BodyType.Static | BodyType.Kinematic)) != 0)
                return false;

            if (TryComp<FlightComponent>(uid, out var flying) && flying.On)
                return true;

            if (TryComp<MovementIgnoreGravityComponent>(uid, out var ignoreGravityComponent))
                return ignoreGravityComponent.Weightless;

            var ev = new IsWeightlessEvent(uid);
            RaiseLocalEvent(uid, ref ev);
            if (ev.Handled)
                return ev.IsWeightless;

            if (!Resolve(uid, ref xform))
                return true;

            // If grid / map has gravity
            if (EntityGridOrMapHaveGravity((uid, xform)))
                return false;

            return true;
        }

        /// <summary>
        /// Checks if a given entity is currently standing on a grid or map that supports having gravity at all.
        /// </summary>
        public bool EntityOnGravitySupportingGridOrMap(Entity<TransformComponent?> entity)
        {
            entity.Comp ??= Transform(entity);

            return _gravityQuery.HasComp(entity.Comp.GridUid) ||
                   _gravityQuery.HasComp(entity.Comp.MapUid);
        }


        /// <summary>
        /// Checks if a given entity is currently standing on a grid or map that has gravity of some kind.
        /// </summary>
        public bool EntityGridOrMapHaveGravity(Entity<TransformComponent?> entity)
        {
            entity.Comp ??= Transform(entity);

            if (_zStack.TryGetZStack(entity, out var stack))
            {
                if (!_zPhysics.TryGetTileWithEntity(entity, ZDirection.Down, out var tile, out var _, out var targetMap) ||
                    tile is null)
                    return false;

                return _gravityQuery.TryComp(targetMap, out var gravity) && gravity.Enabled;
            }
            else
            {
                return _gravityQuery.TryComp(entity.Comp.GridUid, out var gravity) && gravity.Enabled ||
                    _gravityQuery.TryComp(entity.Comp.MapUid, out var mapGravity) && mapGravity.Enabled;
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            InitializeFalling();

            SubscribeLocalEvent<GridInitializeEvent>(OnGridInit);
            SubscribeLocalEvent<AlertSyncEvent>(OnAlertsSync);
            SubscribeLocalEvent<AlertsComponent, EntParentChangedMessage>(OnAlertsParentChange);
            SubscribeLocalEvent<GravityChangedEvent>(OnGravityChange);
            SubscribeLocalEvent<GravityComponent, ComponentGetState>(OnGetState);
            SubscribeLocalEvent<GravityComponent, ComponentHandleState>(OnHandleState);

            _gravityQuery = GetEntityQuery<GravityComponent>();
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            UpdateShake();
        }

        private void OnHandleState(EntityUid uid, GravityComponent component, ref ComponentHandleState args)
        {
            if (args.Current is not GravityComponentState state)
                return;

            if (component.EnabledVV == state.Enabled)
                return;
            component.EnabledVV = state.Enabled;
            var ev = new GravityChangedEvent(uid, component.EnabledVV);
            RaiseLocalEvent(uid, ref ev, true);
        }

        private void OnGetState(EntityUid uid, GravityComponent component, ref ComponentGetState args)
        {
            args.State = new GravityComponentState(component.EnabledVV);
        }

        private void OnGravityChange(ref GravityChangedEvent ev)
        {
            var alerts = AllEntityQuery<AlertsComponent, TransformComponent>();
            while(alerts.MoveNext(out var uid, out _, out var xform))
            {
                if (xform.GridUid != ev.ChangedGridIndex)
                    continue;
                // FINSTER EDIT
                /*
                if (!ev.HasGravity)
                {
                    _alerts.ShowAlert(uid, WeightlessAlert);
                }
                else
                {
                    _alerts.ClearAlert(uid, WeightlessAlert);
                }
                */
                // FINSTER EDIT END
            }
        }

        private void OnAlertsSync(AlertSyncEvent ev)
        {
            // FINSTER EDIT
            /*
            if (IsWeightless(ev.Euid))
            {
                _alerts.ShowAlert(ev.Euid, WeightlessAlert);
            }
            else
            {
                _alerts.ClearAlert(ev.Euid, WeightlessAlert);
            }
            */
            // FINSTER EDIT END
        }

        private void OnAlertsParentChange(EntityUid uid, AlertsComponent component, ref EntParentChangedMessage args)
        {
            // FINSTER EDIT
            /*
            if (IsWeightless(uid))
            {
                _alerts.ShowAlert(uid, WeightlessAlert);
            }
            else
            {
                _alerts.ClearAlert(uid, WeightlessAlert);
            }
            */
            // FINSTER EDIT END
        }

        private void OnGridInit(GridInitializeEvent ev)
        {
            EntityManager.EnsureComponent<GravityComponent>(ev.EntityUid);
        }

        [Serializable, NetSerializable]
        private sealed class GravityComponentState : ComponentState
        {
            public bool Enabled { get; }

            public GravityComponentState(bool enabled)
            {
                Enabled = enabled;
            }
        }
    }

    [ByRefEvent]
    public record struct IsWeightlessEvent(EntityUid Entity, bool IsWeightless = false, bool Handled = false) : IInventoryRelayEvent
    {
        SlotFlags IInventoryRelayEvent.TargetSlots => ~SlotFlags.POCKET;
    }
}
