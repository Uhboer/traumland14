using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Time that players have to wait before rules can be accepted.
    /// </summary>
    public static readonly CVarDef<float> RulesWaitTime =
        CVarDef.Create("finster_rules.time", 0f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Don't show rules to localhost/loopback interface.
    /// </summary>
    public static readonly CVarDef<bool> RulesExemptLocal =
        CVarDef.Create("finster_rules.exempt_local", false, CVar.SERVERONLY);
}
