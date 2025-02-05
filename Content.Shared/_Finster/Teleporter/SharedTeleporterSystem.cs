
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;

namespace Content.Shared._Finster.Teleporter;

public abstract class SharedTeleporterSystem : EntitySystem
{
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<ActorComponent> _actorQuery;
    private EntityQuery<MapGridComponent> _mapGridQuery;

    public override void Initialize()
    {
        _actorQuery = GetEntityQuery<ActorComponent>();
        _mapGridQuery = GetEntityQuery<MapGridComponent>();

        SubscribeLocalEvent<TeleporterComponent, StartCollideEvent>(OnTeleportStartCollide);
    }

    private void OnTeleportStartCollide(Entity<TeleporterComponent> ent, ref StartCollideEvent args)
    {
        var other = args.OtherEntity;
        if (_mapGridQuery.HasComp(other))
        {
            return;
        }

        var otherCoords = _transform.GetMapCoordinates(other);
        var teleporter = _transform.GetMapCoordinates(ent);
        if (otherCoords.MapId != teleporter.MapId)
            return;

        var diff = otherCoords.Position - teleporter.Position;
        if (diff.Length() > 10)
            return;

        teleporter = teleporter.Offset(diff);
        teleporter = teleporter.Offset(ent.Comp.Adjust);

        // TODO: todo...
        //HandlePulling(other, teleporter);
    }

    public void HandlePulling(EntityUid user, EntityUid mapTarget, MapCoordinates teleport)
    {
        if (TryComp(user, out PullableComponent? otherPullable) &&
            otherPullable.Puller != null)
        {
            _pulling.TryStopPull(user, otherPullable, otherPullable.Puller.Value);
        }

        if (TryComp(user, out PullerComponent? puller) &&
            TryComp(puller.Pulling, out PullableComponent? pullable))
        {
            if (TryComp(puller.Pulling, out PullerComponent? otherPullingPuller) &&
                TryComp(otherPullingPuller.Pulling, out PullableComponent? otherPullingPullable))
            {
                _pulling.TryStopPull(otherPullingPuller.Pulling.Value, otherPullingPullable, puller.Pulling);
            }

            var pulling = puller.Pulling.Value;
            _pulling.TryStopPull(pulling, pullable, user);
            _transform.SetCoordinates(user, new EntityCoordinates(mapTarget, teleport.Position));
            _transform.SetCoordinates(pulling, new EntityCoordinates(mapTarget, teleport.Position));
            _pulling.TryStartPull(user, pulling);
        }
        else
        {
            _transform.SetCoordinates(user, new EntityCoordinates(mapTarget, teleport.Position));
        }
    }
}
