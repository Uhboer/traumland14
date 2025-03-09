using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// Enables the automatic voting system.
    public static readonly CVarDef<bool> AutoVoteEnabled =
        CVarDef.Create("finster_vote.autovote_enabled", false, CVar.SERVERONLY);

    /// Automatically starts a map vote when returning to the lobby.
    /// Requires auto voting to be enabled.
    public static readonly CVarDef<bool> MapAutoVoteEnabled =
        CVarDef.Create("finster_vote.map_autovote_enabled", true, CVar.SERVERONLY);

    /// Automatically starts a gamemode vote when returning to the lobby.
    /// Requires auto voting to be enabled.
    public static readonly CVarDef<bool> PresetAutoVoteEnabled =
        CVarDef.Create("finster_vote.preset_autovote_enabled", true, CVar.SERVERONLY);
}
