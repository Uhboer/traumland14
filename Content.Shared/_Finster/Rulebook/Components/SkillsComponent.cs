using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Finster.Rulebook;

/// <summary>
/// All skills for entity, what should be used for any game mechanics.
/// Based on dices!
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SkillsComponent : Component
{
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public Dictionary<ProtoId<SkillPrototype>, Enum> Skills = new();

    /// <summary>
    ///  Take 2d(sides) by skill level.
    /// </summary>
    /// <param name="level">Skill level lol.</param>
    /// <returns>Sides for dice.</returns>
    public Dice GetDiceSides(SkillLevel level)
    {
        switch (level)
        {
            case SkillLevel.NonSkilled:
                return Dice.D4;
            case SkillLevel.Basic:
                return Dice.D6;
            case SkillLevel.Trained:
                return Dice.D8;
            case SkillLevel.Experienced:
                return Dice.D10;
            case SkillLevel.Master:
                return Dice.D12;
            case SkillLevel.Legendary:
                return Dice.D20;
            default:
                return Dice.D6;
        }
    }
}
