using System.Numerics;
using Content.Client.Parallax.Managers;
using Content.Client.Viewport;
using Content.KayMisaZlevels.Shared.Components;
using Content.Shared.CCVar;
using Content.Shared.Parallax.Biomes;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._Finster.ZLayer;

public sealed class ZLayerOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IParallaxManager _manager = default!;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private bool _drawBackgroundLayer = false;

    private ShaderInstance? _blurShader;

    public ZLayerOverlay()
    {
        ZIndex = 0;
        IoCManager.InjectDependencies(this);

        _drawBackgroundLayer = _configurationManager.GetCVar(CCVars.ZLayersBackgroundShader);
        _configurationManager.OnValueChanged(CCVars.ZLayersBackgroundShader, (val) =>
        {
            _drawBackgroundLayer = val;
        });

        _blurShader = _prototypeManager.Index<ShaderPrototype>("ZBlur").InstanceUnique();
        ZIndex = 102;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (args.MapId != MapId.Nullspace && _entManager.HasComponent<ZStackMemberComponent>(_mapManager.GetMapEntityId(args.MapId)))
            return true;

        return false;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.MapId == MapId.Nullspace)
            return;

        if (ScreenTexture == null)
            return;

        if (!_drawBackgroundLayer)
            return;

        var worldHandle = args.WorldHandle;

        worldHandle.DrawRect(args.WorldAABB, Color.Black.WithAlpha(0.5f));

        var zeye = args.Viewport.Eye as ZEye;
        if (zeye is null || !zeye.Top)
            return;

        _blurShader?.SetParameter("BLUR_AMOUNT", -1f);
        _blurShader?.SetParameter("SCREEN_TEXTURE", ScreenTexture);

        worldHandle.UseShader(_blurShader);
        worldHandle.DrawRect(args.WorldAABB, Color.White);
        worldHandle.UseShader(null);
    }
}

