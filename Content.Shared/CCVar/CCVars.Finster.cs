using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    # region White Dream (OLD)

    public static readonly CVarDef<bool> LogChatActions =
        CVarDef.Create("finster_white.log_to_chat", true, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);

    # endregion

    #region Development

    /// <summary>
    /// Run ShaderViewer only.
    /// </summary>
    public static readonly CVarDef<bool> LaunchShaderViewer =
        CVarDef.Create("finster_launch.shader_viewer", false, CVar.CLIENTONLY);

    #endregion

    # region Z Levels

    public static readonly CVarDef<bool> ZLayersBackgroundShader =
        CVarDef.Create("finster_zlayers.background_shader", true, CVar.CLIENT | CVar.REPLICATED);

    # endregion

    # region HUD

    public static readonly CVarDef<bool> ShowLookupHint =
        CVarDef.Create("finster_hud.show_lookup_hint", true, CVar.ARCHIVE | CVar.CLIENTONLY);

    # endregion
}
