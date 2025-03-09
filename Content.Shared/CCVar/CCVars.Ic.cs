using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Restricts IC character names to alphanumeric chars.
    /// </summary>
    public static readonly CVarDef<bool> RestrictedNames =
        CVarDef.Create("finster_ic.restricted_names", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Allows flavor text (character descriptions)
    /// </summary>
    public static readonly CVarDef<bool> FlavorText =
        CVarDef.Create("finster_ic.flavor_text", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Adds a period at the end of a sentence if the sentence ends in a letter.
    /// </summary>
    public static readonly CVarDef<bool> ChatPunctuation =
        CVarDef.Create("finster_ic.punctuation", true, CVar.SERVER);

    /// <summary>
    ///     Enables automatically forcing IC name rules. Uppercases the first letter of the first and last words of the name
    /// </summary>
    public static readonly CVarDef<bool> ICNameCase =
        CVarDef.Create("finster_ic.name_case", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Whether or not players' characters are randomly generated rather than using their selected characters in the creator.
    /// </summary>
    public static readonly CVarDef<bool> ICRandomCharacters =
        CVarDef.Create("finster_ic.random_characters", false, CVar.SERVER);

    /// <summary>
    ///     A weighted random prototype used to determine the species selected for random characters.
    /// </summary>
    public static readonly CVarDef<string> ICRandomSpeciesWeights =
        CVarDef.Create("finster_ic.random_species_weights", "SpeciesWeights", CVar.SERVER);

    /// <summary>
    ///     Control displaying SSD indicators near players
    /// </summary>
    public static readonly CVarDef<bool> ICShowSSDIndicator =
        CVarDef.Create("finster_ic.show_ssd_indicator", false, CVar.CLIENTONLY);
}
