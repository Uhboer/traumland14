using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Weighted list of announcers to choose from
    /// </summary>
    public static readonly CVarDef<string> AnnouncerList =
        CVarDef.Create("finster_announcer.list", "RandomAnnouncers", CVar.REPLICATED);

    /// <summary>
    ///     Optionally force set an announcer
    /// </summary>
    public static readonly CVarDef<string> Announcer =
        CVarDef.Create("finster_announcer.announcer", "", CVar.SERVERONLY);

    /// <summary>
    ///     Optionally blacklist announcers
    ///     List of IDs separated by commas
    /// </summary>
    public static readonly CVarDef<string> AnnouncerBlacklist =
        CVarDef.Create("finster_announcer.blacklist", "", CVar.SERVERONLY);

    /// <summary>
    ///     Changes how loud the announcers are for the client
    /// </summary>
    public static readonly CVarDef<float> AnnouncerVolume =
        CVarDef.Create("finster_announcer.volume", 0.5f, CVar.ARCHIVE | CVar.CLIENTONLY);

    /// <summary>
    ///     Disables multiple announcement sounds from playing at once
    /// </summary>
    public static readonly CVarDef<bool> AnnouncerDisableMultipleSounds =
        CVarDef.Create("finster_announcer.disable_multiple_sounds", false, CVar.ARCHIVE | CVar.CLIENTONLY);
}
