using Robust.Shared.Serialization;

namespace Content.Shared._Finster.Rulebook;

[Serializable, NetSerializable]
public enum SkillLevel : int
{
    NonSkilled,
    Basic,
    Trained,
    Experienced,
    Master,
    Legendary // Fuck them all!
}
