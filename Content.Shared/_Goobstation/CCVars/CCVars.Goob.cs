using Robust.Shared.Configuration;

namespace Content.Shared._Goobstation.CCVar;

[CVarDefs]
public sealed partial class GoobCVars
{
    #region Mechs

    /// <summary>
    ///     Whether or not players can use mech guns outside of mechs.
    /// </summary>
    public static readonly CVarDef<bool> MechGunOutsideMech =
        CVarDef.Create("finster_mech.gun_outside_mech", true, CVar.SERVER | CVar.REPLICATED);

    #endregion

    public static readonly CVarDef<bool> GhostBarEnabled =
        CVarDef.Create("finster_map.ghost_bar_enabled", false, CVar.SERVER | CVar.REPLICATED);
}
