﻿using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Chat window opacity slider, controlling the alpha of the chat window background.
    ///     Goes from to 0 (completely transparent) to 1 (completely opaque)
    /// </summary>
    public static readonly CVarDef<float> ChatWindowOpacity =
        CVarDef.Create("finster_accessibility.chat_window_transparency", 0.85f, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    ///     Toggle for visual effects that may potentially cause motion sickness.
    ///     Where reasonable, effects affected by this CVar should use an alternate effect.
    ///     Please do not use this CVar as a bandaid for effects that could otherwise be made accessible without issue.
    /// </summary>
    public static readonly CVarDef<bool> ReducedMotion =
        CVarDef.Create("finster_accessibility.reduced_motion", false, CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<bool> SpeechBubbles =
        CVarDef.Create("finster_accessibility.speech_bubbles", false, CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<bool> PopupMessages =
        CVarDef.Create("finster_accessibility.popup_messages", false, CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<bool> ChatEnableColorName =
        CVarDef.Create("finster_accessibility.enable_color_name",
            true,
            CVar.REPLICATED | CVar.SERVER,
            "Toggles displaying names with individual colors.");

    /// <summary>
    ///     Screen shake intensity slider, controlling the intensity of the CameraRecoilSystem.
    ///     Goes from 0 (no recoil at all) to 1 (regular amounts of recoil)
    /// </summary>
    public static readonly CVarDef<float> ScreenShakeIntensity =
        CVarDef.Create("finster_accessibility.screen_shake_intensity", 0.10f, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     A generic toggle for various visual effects that are color sensitive.
    ///     As of 2/16/24, only applies to progress bar colors.
    /// </summary>
    public static readonly CVarDef<bool> AccessibilityColorblindFriendly =
        CVarDef.Create("finster_accessibility.colorblind_friendly", false, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    /// Disables all vision filters for species like Vulpkanin or Harpies. There are good reasons someone might want to disable these.
    /// </summary>
    public static readonly CVarDef<bool> NoVisionFilters =
        CVarDef.Create("finster_accessibility.no_vision_filters", false, CVar.REPLICATED | CVar.SERVER);
}
