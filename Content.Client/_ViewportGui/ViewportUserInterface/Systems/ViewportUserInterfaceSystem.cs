
using Content.Client._ViewportGui.ViewportUserInterface.Overlays;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Shared.CCVar;
using Content.Shared.Ghost;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Client._ViewportGui.ViewportUserInterface.Systems;
public sealed class ViewportUserInterfaceSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public event Action? PlayerAttachedEvent;
    public event Action? PlayerDetachedEvent;
    public override void Initialize()
    {
        _overlay.AddOverlay(new ViewportUserInterfaceOverlay());

        SubscribeLocalEvent<LocalPlayerAttachedEvent>(LocalPlayerAttached);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(LocalPlayerDetached);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<ViewportUserInterfaceOverlay>();
    }

    private void LocalPlayerAttached(LocalPlayerAttachedEvent ev)
        => PlayerAttachedEvent?.Invoke();

    private void LocalPlayerDetached(LocalPlayerDetachedEvent ev)
        => PlayerDetachedEvent?.Invoke();
}
