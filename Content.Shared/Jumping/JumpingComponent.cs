using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared.Jumping;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class JumpingComponent : Component
{
    [DataField, AutoNetworkedField]
    public float JumpRange = 5f;

    [DataField, AutoNetworkedField]
    public float JumpSpeed = 3f;

    /// <summary>
    ///     Modifier for collide damage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DamageModifier = 1f;

    /// <summary>
    ///     Amount of damage, when character was collided by the wall or
    ///     another objects.
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier BaseCollideDamage = new DamageSpecifier();

    /// <summary>
    ///     Ignore collide damage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IgnoreDamage;

    [DataField, AutoNetworkedField]
    public TimeSpan JumpCooldown = TimeSpan.FromSeconds(2.5);

    /// <summary>
    ///     Used as checking - if we fail the jumping. If tru - then... Wellh
    /// </summary>
    //public bool IsFailed;

    /*
    [DataField]
    public TimeSpan LandingStunTime = TimeSpan.FromSeconds(0.5);
    */

    public TimeSpan? LastJump = null;
}
