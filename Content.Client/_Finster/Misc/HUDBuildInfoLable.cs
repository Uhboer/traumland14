using System.Numerics;
using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Client.Changelog;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Client._Finster.Misc;

public class HUDBuildInfoLabel : HUDLabel
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly ChangelogManager _changelog = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public HUDBuildInfoAlignment Alignment { get; set; } = HUDBuildInfoAlignment.Edge;

    public HUDBuildInfoLabel()
    {
        IoCManager.InjectDependencies(this);

        _cfg.OnValueChanged(CCVars.ShowBuildInfo, (toggle) =>
        {
            OnConfigChanged(toggle);
        }, true);

        OnConfigChanged(_cfg.GetCVar(CCVars.ShowBuildInfo));
        Color = Color.Gainsboro.WithAlpha(0.25f);
        Scale = 6;
    }

    public void OnConfigChanged(bool value)
    {
        if (_cfg.GetCVar(CCVars.ShowBuildInfoForce))
        {
            Visible = true;
            return;
        }

        Visible = value;
        return;
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
    }

    public override void Draw(in ViewportUIDrawArgs args)
    {
        Text = _changelog.GetClientVersion().ToUpper();

        if (Alignment == HUDBuildInfoAlignment.Center)
        {
            var dimensions = args.ScreenHandle.GetDimensions(Font!, Text, 1f);
            TextPosition = new Vector2(336, 0) - new Vector2(dimensions.X / 2, 0);
        }

        base.Draw(args);
    }
}

public enum HUDBuildInfoAlignment
{
    Edge,
    Center
}
