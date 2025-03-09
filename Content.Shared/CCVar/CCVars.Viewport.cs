using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<bool> ViewportStretch =
        CVarDef.Create("finster_viewport.stretch", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<int> ViewportFixedScaleFactor =
        CVarDef.Create("finster_viewport.fixed_scale_factor", 2, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<int> ViewportSnapToleranceMargin =
        CVarDef.Create("finster_viewport.snap_tolerance_margin", 64, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<int> ViewportSnapToleranceClip =
        CVarDef.Create("finster_viewport.snap_tolerance_clip", 32, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> ViewportScaleRender =
        CVarDef.Create("finster_viewport.scale_render", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<int> ViewportMinimumWidth =
        CVarDef.Create("finster_viewport.minimum_width", 15, CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<int> ViewportMaximumWidth =
        CVarDef.Create("finster_viewport.maximum_width", 15, CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<int> ViewportWidth =
        CVarDef.Create("finster_viewport.width", 15, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> ViewportVerticalFit =
        CVarDef.Create("finster_viewport.vertical_fit", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    //public static readonly CVarDef<string> ViewportFilter =
    //    CVarDef.Create("viewport.filter", "None", CVar.CLIENTONLY | CVar.ARCHIVE);
}
