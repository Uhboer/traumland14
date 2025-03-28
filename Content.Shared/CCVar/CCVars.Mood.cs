using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /*
        * Mood System
        */

    /*
    public static readonly CVarDef<bool> MoodEnabled =
#if RELEASE
        CVarDef.Create("mood.enabled", true, CVar.SERVER);
#else
        CVarDef.Create("mood.enabled", false, CVar.SERVER);
#endif
    */
    public static readonly CVarDef<bool> MoodEnabled =
        CVarDef.Create("finster_mood.enabled", true, CVar.SERVER);

    public static readonly CVarDef<bool> MoodIncreasesSpeed =
        CVarDef.Create("finster_mood.increases_speed", true, CVar.SERVER);

    public static readonly CVarDef<bool> MoodDecreasesSpeed =
        CVarDef.Create("finster_mood.decreases_speed", true, CVar.SERVER);

    public static readonly CVarDef<bool> MoodModifiesThresholds =
        CVarDef.Create("finster_mood.modify_thresholds", false, CVar.SERVER);

    public static readonly CVarDef<bool> MoodVisualEffects =
        CVarDef.Create("finster_mood.visual_effects", true, CVar.REPLICATED);
}
