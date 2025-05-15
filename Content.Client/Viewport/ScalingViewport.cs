using System.Numerics;
using Content.Client.UserInterface.Systems.Viewport;
using Content.KayMisaZlevels.Client;
using Content.KayMisaZlevels.Shared.Systems;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.ContentPack;
using Robust.Shared.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Client.Viewport;

/// <summary>
///     Viewport control that has a fixed viewport size and scales it appropriately.
///     Z Level aware. You have to use this for rendering Z levels, or at least consult its drawing implementation.
/// </summary>
public sealed class ScalingViewport : Control, IViewportControl
{
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IResourceManager _resManager = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    //[Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private ZStackSystem? _zStack = default!;

    private EntityQuery<TransformComponent>? _xformQuery;

    // Drawing Shader
    public ShaderInstance? Shader;

    public bool CanMouseClick = true;

    // Internal viewport creation is deferred.
    private IClydeViewport? _viewport;
    private IEye? _eye;
    private IEye? _fallbackEye;
    private Vector2i _viewportSize;
    private int _curRenderScale;
    private ScalingViewportStretchMode _stretchMode = ScalingViewportStretchMode.Bilinear;
    private ScalingViewportRenderScaleMode _renderScaleMode = ScalingViewportRenderScaleMode.Fixed;
    private ScalingViewportIgnoreDimension _ignoreDimension = ScalingViewportIgnoreDimension.None;
    private int _fixedRenderScale = 1;

    private readonly List<CopyPixelsDelegate<Rgba32>> _queuedScreenshots = new();

    /// <summary>
    /// Viewport sized in tiles
    /// </summary>
    public Vector2i SizeInTiles { get; set; } = new Vector2i(ViewportUIController.ViewportHeight, ViewportUIController.ViewportHeight);
    public Vector2i OffsetSize { get; set; } = Vector2i.Zero;
    public Vector2i OffsetPosition { get; set; } = Vector2i.Zero;

    public int CurrentRenderScale => _curRenderScale;

    /// <summary>
    ///     The eye to render.
    /// </summary>
    public IEye? Eye
    {
        get => _eye;
        set
        {
            _eye = value;

            if (_viewport != null)
                _viewport.Eye = value;
        }
    }

    /// <summary>
    ///     The size, in unscaled pixels, of the internal viewport.
    /// </summary>
    /// <remarks>
    ///     The actual viewport may have render scaling applied based on parameters.
    /// </remarks>
    public Vector2i ViewportSize
    {
        get => _viewportSize;
        set
        {
            _viewportSize = value;
            InvalidateViewport();
        }
    }

    // Do not need to InvalidateViewport() since it doesn't affect viewport creation.

    [ViewVariables(VVAccess.ReadWrite)] public Vector2i? FixedStretchSize { get; set; }

    [ViewVariables(VVAccess.ReadWrite)]
    public ScalingViewportStretchMode StretchMode
    {
        get => _stretchMode;
        set
        {
            _stretchMode = value;
            InvalidateViewport();
        }
    }

    [ViewVariables(VVAccess.ReadWrite)]
    public ScalingViewportRenderScaleMode RenderScaleMode
    {
        get => _renderScaleMode;
        set
        {
            _renderScaleMode = value;
            InvalidateViewport();
        }
    }

    [ViewVariables(VVAccess.ReadWrite)]
    public int FixedRenderScale
    {
        get => _fixedRenderScale;
        set
        {
            _fixedRenderScale = value;
            InvalidateViewport();
        }
    }

    [ViewVariables(VVAccess.ReadWrite)]
    public ScalingViewportIgnoreDimension IgnoreDimension
    {
        get => _ignoreDimension;
        set
        {
            _ignoreDimension = value;
            InvalidateViewport();
        }
    }

    public ScalingViewport()
    {
        IoCManager.InjectDependencies(this);
        RectClipContent = true;

        //ZLayerShader = _prototypeManager.Index<ShaderPrototype>("ZLayer").InstanceUnique();
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (!CanMouseClick)
            return;

        if (args.Handled)
            return;

        _inputManager.ViewportKeyEvent(this, args);
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (!CanMouseClick)
            return;

        if (args.Handled)
            return;

        _inputManager.ViewportKeyEvent(this, args);
    }

    protected override void Draw(IRenderHandle renderHandle)
    {
        _fallbackEye = _eye;

        var handle = renderHandle.DrawingHandleScreen;
        EnsureViewportCreated();
        DebugTools.AssertNotNull(_viewport);

        // Cache frequently accessed components/systems
        _xformQuery ??= _entityManager.GetEntityQuery<TransformComponent>();
        var viewport = _viewport!; // Local reference to avoid null checks
        var drawBox = GetDrawBox();
        var drawBoxGlobal = drawBox.Translated(GlobalPixelPosition);

        if (_eye is null)
        {
            RenderFinalOutput(renderHandle, viewport, drawBox, drawBoxGlobal);
            return;
        }

        // Cache systems and components
        _zStack ??= _entityManager.System<ZStackSystem>();
        var mapId = _eye.Position.MapId;
        var mapEntityId = _mapManager.GetMapEntityIdOrThrow(mapId);

        // If map is not multi-Z
        if (!_zStack.TryGetZStack(mapEntityId, out var stack))
        {
            viewport.Eye = Eye;
            viewport.Render();
            RenderFinalOutput(renderHandle, viewport, drawBox, drawBoxGlobal);
            return;
        }

        // Process multi-Z rendering
        var stackMaps = stack.Value.Comp.Maps;
        GetDrawingMapList(stackMaps, mapEntityId, out var drawMaps);

        var depth = drawMaps.Count;
        var idx = stackMaps.Count - depth;
        var preLastMap = EntityUid.Invalid;
        if (depth >= 2)
            preLastMap = drawMaps[depth - 2];
        var firstLayer = true;

        foreach (var toDraw in drawMaps)
        {
            // Configure viewport for this layer
            viewport.ClearColor = firstLayer ? Robust.Shared.Maths.Color.Magenta : null;

            var mapComp = _entityManager.GetComponent<MapComponent>(toDraw);
            var pos = new MapCoordinates(_eye.Position.Position, mapComp.MapId);

            // Render with appropriate effects
            if (toDraw != mapEntityId)
            {
                var zeye = ConfigureZEye(viewport, pos, idx, depth, toDraw == preLastMap);
                if (zeye is not null)
                    zeye.DrawFov = false;
                //var previousFov = SetEyeFov(viewport.Eye, false);
                viewport.Render();
                //SetEyeFov(viewport.Eye, previousFov);
            }
            else
            {
                FallbackDefaultEye();
                viewport.Render();
                break; // Exit early after rendering main layer
            }

            firstLayer = false;
            idx++;
            depth--;
        }

        RenderFinalOutput(renderHandle, viewport, drawBox, drawBoxGlobal);
    }

    private IEye? ConfigureZEye(IClydeViewport viewport, MapCoordinates pos, int idx, int depth, bool isTop)
    {
        if (_eye is null)
            return null;

        var eye = new ZEye()
        {
            Position = pos,
            DrawFov = _eye.DrawFov,
            DrawLight = _eye.DrawLight,
            Offset = _eye.Offset,
            Rotation = _eye.Rotation,
            Scale = _eye.Scale - new Vector2(0.03f * depth, 0.03f * depth),
            Depth = idx,
            Top = isTop
        };

        viewport.Eye = eye;
        return eye;
    }

    private void RenderFinalOutput(IRenderHandle renderHandle, IClydeViewport viewport, UIBox2 drawBox, UIBox2i drawBoxGlobal)
    {
        var handle = renderHandle.DrawingHandleScreen;

        // Restore the Eye
        FallbackDefaultEye();
        // Setup the shader
        handle.UseShader(Shader);

        // Process screenshots if any
        if (_queuedScreenshots.Count > 0)
        {
            var callbacks = _queuedScreenshots.ToArray();
            _queuedScreenshots.Clear();

            viewport.RenderTarget.CopyPixelsToMemory<Rgba32>(image =>
            {
                foreach (var callback in callbacks)
                {
                    callback(image);
                }
            });
        }

        viewport.RenderScreenOverlaysBelow(renderHandle, this, drawBoxGlobal);
        handle.DrawTextureRect(viewport.RenderTarget.Texture, drawBox);
        viewport.RenderScreenOverlaysAbove(renderHandle, this, drawBoxGlobal);
    }

    public void GetDrawingMapList(List<EntityUid> sourceList, EntityUid currentMap, out List<EntityUid> mapList)
    {
        mapList = new List<EntityUid>(sourceList.Count);

        if (_eye is null)
            return;

        var mapIdx = sourceList.IndexOf(currentMap);
        if (mapIdx < 0)
            return;

        for (int i = mapIdx; i >= 0; i--)
        {
            var targetMap = sourceList[i];
            mapList.Add(targetMap);

            if (!TryFindEmptyTiles(targetMap))
                break;
        }

        // Reverse a new list
        if (mapList.Count > 0)
        {
            var tempList = new List<EntityUid>(mapList.Count);
            for (int i = mapList.Count - 1; i >= 0; i--)
            {
                tempList.Add(mapList[i]);
            }
            mapList = tempList;
        }
    }

    public bool TryFindEmptyTiles(EntityUid mapUid)
    {
        if (_xformQuery is null || !_xformQuery.Value.TryComp(mapUid, out var xform))
            return true;

        var drawBox = GetDrawBox();
        var eyeManager = _eyeManager;
        var mapManager = _mapManager;

        var bottomLeftPos = eyeManager.ScreenToMap(drawBox.BottomLeft).Position;
        var topRightPos = eyeManager.ScreenToMap(drawBox.TopRight).Position;
        var mapId = xform.MapID;

        var mapCoordsBottomLeft = new MapCoordinates(bottomLeftPos, mapId);
        var mapCoordsTopRight = new MapCoordinates(topRightPos, mapId);

        if (!mapManager.TryFindGridAt(mapUid, mapCoordsBottomLeft.Position, out _, out var grid))
            return true;

        var tileBottomLeft = grid.TileIndicesFor(mapCoordsBottomLeft);
        var tileTopRight = grid.TileIndicesFor(mapCoordsTopRight);

        int minX = tileBottomLeft.X - 1;
        int maxX = tileTopRight.X + 1;
        int minY = tileBottomLeft.Y - 1;
        int maxY = tileTopRight.Y + 1;

        Vector2i tilePos = default;

        for (tilePos.X = minX; tilePos.X <= maxX; tilePos.X++)
        {
            for (tilePos.Y = minY; tilePos.Y <= maxY; tilePos.Y++)
            {
                var tile = grid.GetTileRef(tilePos);

                if (tile.Tile.IsEmpty)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="eye"></param>
    /// <returns>Previous Fov state</returns>
    private bool SetEyeFov(IEye? eye, bool state)
    {
        if (eye is null)
            return false;

        var previousFovState = eye.DrawFov;
        eye.DrawFov = state;

        // If we don't want draw fov by admin menu
        if (previousFovState == state)
            return false;

        return previousFovState;
    }

    private void FallbackDefaultEye()
    {
        Eye = _fallbackEye;
    }

    public void Screenshot(CopyPixelsDelegate<Rgba32> callback)
    {
        _queuedScreenshots.Add(callback);
    }

    // Draw box in pixel coords to draw the viewport at.
    public UIBox2i GetDrawBox()
    {
        DebugTools.AssertNotNull(_viewport);

        var baseHeight = EyeManager.PixelsPerMeter * SizeInTiles.Y;
        var baseWidth = EyeManager.PixelsPerMeter * SizeInTiles.Y;

        var vpSize = _viewport!.Size;
        var vpSizeSized = new Vector2i(vpSize.X, vpSize.Y);
        vpSizeSized.X = (baseWidth + (OffsetSize.X * EyeManager.PixelsPerMeter)) * (vpSize.X / baseWidth); // Only testing
        vpSizeSized.Y = (baseHeight + (OffsetSize.Y * EyeManager.PixelsPerMeter)) * (vpSize.Y / baseHeight); // Only testing
        var ourSize = (Vector2) PixelSize;

        if (FixedStretchSize == null)
        {
            var (ratioX, ratioY) = ourSize / new Vector2(vpSize.X, vpSizeSized.Y);
            var ratio = 1f;
            switch (_ignoreDimension)
            {
                case ScalingViewportIgnoreDimension.None:
                    ratio = Math.Min(ratioX, ratioY);
                    break;
                case ScalingViewportIgnoreDimension.Vertical:
                    ratio = ratioX;
                    break;
                case ScalingViewportIgnoreDimension.Horizontal:
                    ratio = ratioY;
                    break;
            }

            var size = vpSize * ratio;
            var sizeSized = vpSizeSized * ratio;
            // Size
            var pos = (ourSize - sizeSized) / 2;
            var offsetInPixels = new Vector2(
                OffsetPosition.X * EyeManager.PixelsPerMeter * (vpSize.X / baseWidth) * ratio,
                OffsetPosition.Y * EyeManager.PixelsPerMeter * (vpSize.Y / baseHeight) * ratio
            );
            pos += offsetInPixels;

            return (UIBox2i) UIBox2.FromDimensions(pos, size);
        }
        else
        {
            var fixedStretchSize = FixedStretchSize.Value;
            var fixedStretchSizeSized = new Vector2i(fixedStretchSize.X, fixedStretchSize.Y);
            fixedStretchSizeSized.X = (baseWidth + (OffsetSize.X * EyeManager.PixelsPerMeter)) * (fixedStretchSize.X / baseWidth);
            fixedStretchSizeSized.Y = (baseHeight + (OffsetSize.Y * EyeManager.PixelsPerMeter)) * (fixedStretchSize.Y / baseHeight);

            // Center only, no scaling.
            var pos = (ourSize - fixedStretchSizeSized) / 2;
            var offsetInPixels = new Vector2(
                OffsetPosition.X * EyeManager.PixelsPerMeter * (fixedStretchSize.X / baseWidth),
                OffsetPosition.Y * EyeManager.PixelsPerMeter * (fixedStretchSize.Y / baseHeight)
            );
            pos += offsetInPixels;

            return (UIBox2i) UIBox2.FromDimensions(pos, fixedStretchSize);
        }
    }

    private void RegenerateViewport()
    {
        DebugTools.AssertNull(_viewport);

        var vpSizeBase = ViewportSize;
        var ourSize = PixelSize;
        var (ratioX, ratioY) = ourSize / (Vector2) vpSizeBase;
        var ratio = Math.Min(ratioX, ratioY);
        var renderScale = 1;
        switch (_renderScaleMode)
        {
            case ScalingViewportRenderScaleMode.CeilInt:
                renderScale = (int) Math.Ceiling(ratio);
                break;
            case ScalingViewportRenderScaleMode.FloorInt:
                renderScale = (int) Math.Floor(ratio);
                break;
            case ScalingViewportRenderScaleMode.Fixed:
                renderScale = _fixedRenderScale;
                break;
        }

        // Always has to be at least one to avoid passing 0,0 to the viewport constructor
        renderScale = Math.Max(1, renderScale);

        _curRenderScale = renderScale;

        _viewport = _clyde.CreateViewport(
            ViewportSize * renderScale,
            new TextureSampleParameters
            {
                Filter = StretchMode == ScalingViewportStretchMode.Bilinear,
            });

        _viewport.RenderScale = new Vector2(renderScale, renderScale);

        _viewport.Eye = _eye;
    }

    protected override void Resized()
    {
        base.Resized();

        InvalidateViewport();
    }

    private void InvalidateViewport()
    {
        _viewport?.Dispose();
        _viewport = null;
    }

    public MapCoordinates ScreenToMap(Vector2 coords)
    {
        if (_eye == null)
            return default;

        EnsureViewportCreated();

        Matrix3x2.Invert(GetLocalToScreenMatrix(), out var matrix);
        coords = Vector2.Transform(coords, matrix);

        return _viewport!.LocalToWorld(coords);
    }

    /// <inheritdoc/>
    public MapCoordinates PixelToMap(Vector2 coords)
    {
        if (_eye == null)
            return default;

        EnsureViewportCreated();

        Matrix3x2.Invert(GetLocalToScreenMatrix(), out var matrix);
        coords = Vector2.Transform(coords, matrix);

        var ev = new PixelToMapEvent(coords, this, _viewport!);
        _entityManager.EventBus.RaiseEvent(EventSource.Local, ref ev);

        return _viewport!.LocalToWorld(ev.VisiblePosition);
    }

    public Vector2 WorldToScreen(Vector2 map)
    {
        if (_eye == null)
            return default;

        EnsureViewportCreated();

        var vpLocal = _viewport!.WorldToLocal(map);

        var matrix = GetLocalToScreenMatrix();

        return Vector2.Transform(vpLocal, matrix);
    }

    public Matrix3x2 GetWorldToScreenMatrix()
    {
        EnsureViewportCreated();
        return _viewport!.GetWorldToLocalMatrix() * GetLocalToScreenMatrix();
    }

    public Matrix3x2 GetLocalToScreenMatrix()
    {
        EnsureViewportCreated();

        var drawBox = GetDrawBox();
        var scaleFactor = drawBox.Size / (Vector2) _viewport!.Size;

        if (scaleFactor.X == 0 || scaleFactor.Y == 0)
            // Basically a nonsense scenario, at least make sure to return something that can be inverted.
            return Matrix3x2.Identity;

        return Matrix3Helpers.CreateTransform(GlobalPixelPosition + drawBox.TopLeft, 0, scaleFactor);
    }

    private void EnsureViewportCreated()
    {
        if (_viewport == null)
        {
            RegenerateViewport();
        }

        DebugTools.AssertNotNull(_viewport);
    }
}

/// <summary>
///     Defines how the viewport is stretched if it does not match the size of the control perfectly.
/// </summary>
public enum ScalingViewportStretchMode
{
    /// <summary>
    ///     Bilinear sampling is used.
    /// </summary>
    Bilinear = 0,

    /// <summary>
    ///     Nearest neighbor sampling is used.
    /// </summary>
    Nearest,
}

/// <summary>
///     Defines how the base render scale of the viewport is selected.
/// </summary>
public enum ScalingViewportRenderScaleMode
{
    /// <summary>
    ///     <see cref="ScalingViewport.FixedRenderScale"/> is used.
    /// </summary>
    Fixed = 0,

    /// <summary>
    ///     Floor to the closest integer scale possible.
    /// </summary>
    FloorInt,

    /// <summary>
    ///     Ceiling to the closest integer scale possible.
    /// </summary>
    CeilInt
}

/// <summary>
///     If the viewport is allowed to freely scale, this determines which dimensions should be ignored while fitting the viewport
/// </summary>
public enum ScalingViewportIgnoreDimension
{
    /// <summary>
    ///     The viewport won't ignore any dimension.
    /// </summary>
    None = 0,

    /// <summary>
    ///     The viewport will ignore the horizontal dimension, and will exclusively consider the vertical dimension for scaling.
    /// </summary>
    Horizontal,

    /// <summary>
    ///     The viewport will ignore the vertical dimension, and will exclusively consider the horizontal dimension for scaling.
    /// </summary>
    Vertical
}

//FIXME: This is nasty!
public sealed class ZEye : Robust.Shared.Graphics.Eye
{
    public int Depth;
    public bool Top;
}
