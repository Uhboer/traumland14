using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<bool> ParallaxEnabled =
        CVarDef.Create("finster_parallax.enabled", true, CVar.CLIENTONLY);

    public static readonly CVarDef<bool> ParallaxDebug =
        CVarDef.Create("finster_parallax.debug", false, CVar.CLIENTONLY);

    public static readonly CVarDef<bool> ParallaxLowQuality =
        CVarDef.Create("finster_parallax.low_quality", false, CVar.ARCHIVE | CVar.CLIENTONLY);
}
