using Content.Client.GameTicking.Managers;
using Content.Shared.CCVar;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;

namespace Content.Client.DynamicWindowTitle;

public sealed class DynamicWindowTitleSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly ClientGameTicker _gameTicker = default!;
    [Dependency] private readonly IClyde _clyde = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_configuration, CCVars.GameHostName, OnValueChanged);

        UpdateWindowTitle();
    }

    private void OnValueChanged(string newValue)
    {
        UpdateWindowTitle(newValue);
    }

    public void UpdateWindowTitle(string? name = null)
    {
        _clyde.SetWindowTitle(name != null ? name : _configuration.GetCVar(CCVars.GameHostName));
    }
}
