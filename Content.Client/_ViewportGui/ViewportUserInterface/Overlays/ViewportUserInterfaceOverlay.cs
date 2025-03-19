using System.Numerics;
using Content.Client._ViewportGui.ViewportUserInterface.Systems;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Client.Examine;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Viewport;
using Content.Client.Viewport;
using Content.KayMisaZlevels.Client;
using Content.Shared.CCVar;
using Content.Shared.Ghost;
using Content.Shared.Maps;
using Content.Shared.Parallax.Biomes;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client._ViewportGui.ViewportUserInterface.Overlays;

public sealed class ViewportUserInterfaceOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IClyde _clyde = default!;

    private ViewportUserInterfaceSystem _vpUISystem;
    private ViewportUIController _viewportUIController;
    private ScalingViewport? _viewport;

    private IRenderTexture? _buffer;

    public ViewportUserInterfaceOverlay()
    {
        IoCManager.InjectDependencies(this);

        _vpUISystem = _entManager.System<ViewportUserInterfaceSystem>();
        _viewportUIController = _uiManager.GetUIController<ViewportUIController>();

        _cfg.OnValueChanged(CCVars.HudType, (_) =>
        {
            RestoreHud();
        });

        _vpUISystem.PlayerAttachedEvent += () =>
        {
            RestoreHud();
        };
        _vpUISystem.PlayerDetachedEvent += () =>
        {
            _vpUIManager.UnloadScreen();
        };

        ResolveViewport();
    }

    private void RestoreHud()
    {
        var hudType = _cfg.GetCVar(CCVars.HudType);
        HUDRoot gameplayHud = new HUDGameplayState((HUDGameplayType) hudType);

        if (_player.LocalEntity is not null &&
            _entManager.TryGetComponent<GhostComponent>(_player.LocalEntity.Value, out var ghostComp) &&
            ghostComp.EnableGhostOverlay)
            gameplayHud = new HUDGhostState();

        _vpUIManager.ReloadScreen(gameplayHud);
        ResolveViewport();
    }

    private void RestoreBuffer(Vector2i contentSize)
    {
        _buffer?.Dispose();
        _buffer = _clyde.CreateRenderTarget(
            contentSize,
            RenderTargetColorFormat.Rgba8Srgb);
    }

    public void ResolveViewport(ScalingViewport? control = null)
    {
        // What the fuck i am douing?
        var mainViewport = _uiManager.ActiveScreen?.GetWidget<MainViewport>();
        if (mainViewport is null ||
            _vpUIManager.DrawingInfo is null)
            return;

        if (control is null)
            _viewport = mainViewport.Viewport;
        else
            _viewport = control;

        _vpUIManager.Viewport = _viewport;
        _vpUIManager.Viewport.OffsetSize = _vpUIManager.DrawingInfo.Value.OffsetSize;
        _vpUIManager.Viewport.OffsetPosition = _vpUIManager.DrawingInfo.Value.OffsetPosition;

        RestoreBuffer(_vpUIManager.DrawingInfo.Value.ContentSize);
    }

    protected override void DisposeBehavior()
    {
        base.DisposeBehavior();

        _buffer?.Dispose();
    }

    /*
    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return true;
    }
    */

    protected override void FrameUpdate(FrameEventArgs args)
    {
        _vpUIManager.FrameUpdate(args);

        base.FrameUpdate(args);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.ScreenHandle;
        var viewport = (args.ViewportControl as ScalingViewport);
        var uiScale = (args.ViewportControl as ScalingViewport)?.UIScale ?? 1f;

        if (viewport is null)
            return;
        if (_viewport is null)
        {
            ResolveViewport(viewport);
            return; // Because we can draw in next frame
        }

        var drawBounds = _vpUIManager.GetDrawingBounds();
        if (drawBounds is null ||
            _vpUIManager.DrawingInfo is null ||
            _vpUIManager.Root is null ||
            _buffer is null)
            return;

        var drawingArgs = new ViewportUIDrawArgs(
                _buffer,
                _vpUIManager.DrawingInfo.Value.ContentSize,
                drawBounds.Value.DrawBox, drawBounds.Value.Scale, viewport, handle);
        _vpUIManager.Draw(drawingArgs);

        handle.DrawTextureRect(_buffer.Texture, drawBounds.Value.DrawBox);
    }
}
