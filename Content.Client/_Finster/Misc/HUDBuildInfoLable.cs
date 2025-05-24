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

    public HUDBuildInfoLabel()
    {
        IoCManager.InjectDependencies(this);

        _cfg.OnValueChanged(CCVars.ShowBuildInfo, (toggle) =>
        {
            OnConfigChanged(toggle);
        }, true);

        OnConfigChanged(_cfg.GetCVar(CCVars.ShowBuildInfo));
        Color = Color.Gainsboro.WithAlpha(0.25f);
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
        base.Draw(args);
    }
}
