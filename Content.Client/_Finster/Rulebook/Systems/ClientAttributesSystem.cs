using Content.Shared._Finster.Rulebook;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Random;

public sealed class ClientAttributesSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public event Action<AttributesComponent>? AttributesStartup;
    public event Action? AttributesShutdown;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AttributesComponent, LocalPlayerAttachedEvent>(HandlePlayerAttached);
        SubscribeLocalEvent<AttributesComponent, LocalPlayerDetachedEvent>(HandlePlayerDetached);
        SubscribeLocalEvent<AttributesComponent, ComponentStartup>(OnAttributesStartup);
        SubscribeLocalEvent<AttributesComponent, ComponentShutdown>(OnAttributesShutdown);
    }

    private void HandlePlayerAttached(EntityUid uid, AttributesComponent component, LocalPlayerAttachedEvent args)
    {
        AttributesStartup?.Invoke(component);
    }

    private void HandlePlayerDetached(EntityUid uid, AttributesComponent component, LocalPlayerDetachedEvent args)
    {
        AttributesShutdown?.Invoke();
    }

    private void OnAttributesStartup(EntityUid uid, AttributesComponent component, ComponentStartup args)
    {
        if (_playerManager.LocalEntity != uid)
            return;

        AttributesStartup?.Invoke(component);
    }

    private void OnAttributesShutdown(EntityUid uid, AttributesComponent component, ComponentShutdown args)
    {
        if (_playerManager.LocalEntity != uid)
            return;

        AttributesShutdown?.Invoke();
    }
}
