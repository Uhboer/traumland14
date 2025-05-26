using System.Linq;
using System.Numerics;
using Content.Client.Animations;
using Content.Shared.Hands;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Robust.Client.Player;
using Robust.Shared.Collections;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client.Storage.Systems;

public sealed class StorageSystem : SharedStorageSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly EntityPickupAnimationSystem _entityPickupAnimation = default!;

    private Entity<StorageComponent>? _openedStorage;
    private readonly List<StorageBoundUserInterface> _storagesToClose = new();
    public int OpenStorageAmount = 1;

    public event Action<Entity<StorageComponent>>? StorageUpdated;
    public event Action<Entity<StorageComponent>?>? StorageOrderChanged;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StorageComponent, ComponentShutdown>(OnShutdown);
        SubscribeNetworkEvent<PickupAnimationEvent>(HandlePickupAnimation);
        SubscribeAllEvent<AnimateInsertingEntitiesEvent>(HandleAnimatingInsertingEntities);
    }

    public override void UpdateUI(Entity<StorageComponent?> entity)
    {
        if (Resolve(entity.Owner, ref entity.Comp))
            StorageUpdated?.Invoke((entity, entity.Comp));
    }

    public void OpenStorageWindow(Entity<StorageComponent> entity)
    {
        // Close existing window
        if (_openedStorage is not null)
            CloseStorageWindow(_openedStorage.Value.Owner, true);

        _openedStorage = entity;
        StorageOrderChanged?.Invoke(_openedStorage);
    }

    public void CloseStorageWindow(Entity<StorageComponent?> entity, bool onOpening = false)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        if (_openedStorage is null)
            return;
        if (_openedStorage.Value.Owner != entity.Owner)
            return;

        CloseStorageBoundUserInterface(_openedStorage.Value.Owner);
        _openedStorage = null;
        if (!onOpening)
            StorageOrderChanged?.Invoke(_openedStorage);
    }

    private void CloseStorageBoundUserInterface(Entity<UserInterfaceComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        if (!UI.TryGetOpenUi<StorageBoundUserInterface>(entity, StorageComponent.StorageUiKey.Key, out var storageBui))
            return;

        // TODO: It can'not be queued. Need fix it or refactor by self for the game.
        //bui.Close();
        // EE ebanie gandonia nahui suka DEATH4DEATH or how to blyat suka ti uebok, kak i
        // vse iz consoula, suka ubeites ya nimogu.
        // 26.05.2025 - kazhetsa ya nashel fix, no on koncheniy.
        // EE uebany ebanie, suka, blyat. nahui, blayt.
        if (!_storagesToClose.Contains(storageBui))
            _storagesToClose.Add(storageBui);
    }

    private void OnShutdown(Entity<StorageComponent> ent, ref ComponentShutdown args)
    {
        CloseStorageWindow((ent, ent.Comp));
    }

    /// <inheritdoc />
    public override void PlayPickupAnimation(EntityUid uid, EntityCoordinates initialCoordinates, EntityCoordinates finalCoordinates,
        Angle initialRotation, EntityUid? user = null)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        PickupAnimation(uid, initialCoordinates, finalCoordinates, initialRotation);
    }

    private void HandlePickupAnimation(PickupAnimationEvent msg)
    {
        PickupAnimation(GetEntity(msg.ItemUid), GetCoordinates(msg.InitialPosition), GetCoordinates(msg.FinalPosition), msg.InitialAngle);
    }

    public void PickupAnimation(EntityUid item, EntityCoordinates initialCoords, EntityCoordinates finalCoords, Angle initialAngle)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (finalCoords.InRange(EntityManager, TransformSystem, initialCoords, 0.1f) ||
            !Exists(initialCoords.EntityId) || !Exists(finalCoords.EntityId))
        {
            return;
        }

        var finalMapPos = finalCoords.ToMapPos(EntityManager, TransformSystem);
        var finalPos = Vector2.Transform(finalMapPos, TransformSystem.GetInvWorldMatrix(initialCoords.EntityId));

        _entityPickupAnimation.AnimateEntityPickup(item, initialCoords, finalPos, initialAngle);
    }

    public override void Update(float frameTime)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        foreach (var storageBui in _storagesToClose)
        {
            storageBui.Close();
        }
        _storagesToClose.Clear();

        base.Update(frameTime);
    }

    /// <summary>
    /// Animate the newly stored entities in <paramref name="msg"/> flying towards this storage's position
    /// </summary>
    /// <param name="msg"></param>
    public void HandleAnimatingInsertingEntities(AnimateInsertingEntitiesEvent msg)
    {
        TryComp(GetEntity(msg.Storage), out TransformComponent? transformComp);

        for (var i = 0; msg.StoredEntities.Count > i; i++)
        {
            var entity = GetEntity(msg.StoredEntities[i]);

            var initialPosition = msg.EntityPositions[i];
            if (Exists(entity) && transformComp != null)
            {
                _entityPickupAnimation.AnimateEntityPickup(entity, GetCoordinates(initialPosition), transformComp.LocalPosition, msg.EntityAngles[i]);
            }
        }
    }
}
