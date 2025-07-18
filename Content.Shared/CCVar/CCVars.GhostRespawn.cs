using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<double> GhostRespawnTime =
        CVarDef.Create("finster_ghost.respawn_time", 15d, CVar.SERVERONLY);

    public static readonly CVarDef<int> GhostRespawnMaxPlayers =
        CVarDef.Create("finster_ghost.respawn_max_players", 40, CVar.SERVERONLY);

    public static readonly CVarDef<bool> GhostAllowSameCharacter =
        CVarDef.Create("finster_ghost.allow_same_character", false, CVar.SERVERONLY);
}
