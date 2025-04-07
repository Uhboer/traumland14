using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Server._KMZLevels.Falling;

[RegisterComponent]
public sealed partial class FallingComponent : Component
{
    /// <summary>
    /// How many seconds the mob will be paralyzed for.
    /// </summary>
    [DataField]
    public TimeSpan LandingStunTime = TimeSpan.FromSeconds(5);

    [DataField]
    public float DamageModifier = 1f;

    /// <summary>
    ///  Base damage from falling. It can be increase by falling distance.
    /// </summary>
    [DataField]
    public DamageSpecifier BaseDamage = new DamageSpecifier();

    [DataField]
    public bool IgnoreDamage;
}
