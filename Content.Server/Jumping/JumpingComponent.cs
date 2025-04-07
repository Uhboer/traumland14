using Content.Shared.Damage;

namespace Content.Server.Jumping;

[RegisterComponent]
public sealed partial class JumpingComponent : Component
{
    [DataField]
    public float JumpRange = 5f;

    [DataField]
    public float JumpSpeed = 3f;

    /// <summary>
    ///     Modifier for collide damage.
    /// </summary>
    [DataField]
    public float DamageModifier = 1f;

    /// <summary>
    ///     Amount of damage, when character was collided by the wall or
    ///     another objects.
    /// </summary>
    [DataField]
    public DamageSpecifier BaseCollideDamage = new DamageSpecifier();

    /// <summary>
    ///     Ignore collide damage.
    /// </summary>
    [DataField]
    public bool IgnoreDamage;

    [DataField]
    public TimeSpan JumpCooldown = TimeSpan.FromSeconds(2.5);

    /*
    [DataField]
    public TimeSpan LandingStunTime = TimeSpan.FromSeconds(0.5);
    */

    public bool IsFailed = false;

    public TimeSpan? LastJump = null;
}
