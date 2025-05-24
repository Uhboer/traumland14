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

    public static readonly CVarDef<bool> ShowBuildInfo =
        CVarDef.Create("finster_hud.show_build_info", false, CVar.ARCHIVE | CVar.CLIENTONLY);

    public static readonly CVarDef<bool> ShowBuildInfoForce =
        CVarDef.Create("finster_hud.show_build_info_force", false, CVar.SERVER | CVar.REPLICATED);

    #endregion

    /// <summary>
    /// Toggle for non-gameplay-affecting or otherwise status indicative post-process effects, such additive lighting.
    /// In the future, this could probably be turned into an enum that allows only disabling more expensive post-process shaders.
    /// However, for now (mid-July of 2024), this only applies specifically to a particularly cheap shader: additive lighting.
    /// </summary>
    public static readonly CVarDef<bool> PostProcess =
        CVarDef.Create("finster_graphics.post_process", true, CVar.CLIENTONLY | CVar.ARCHIVE);
}
