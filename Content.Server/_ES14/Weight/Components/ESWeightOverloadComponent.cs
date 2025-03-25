using Content.Shared.Alert;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Server._ES14.Weight.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class ESWeightOverloadComponent : Component
{
    [DataField]
    public float Overload = 90f;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public ESOverloadLevel OverloadLevel = ESOverloadLevel.NoOverload;

    #region Overload Modifiers

    [DataField]
    public float SlightOverloadModifier = 0.85f;

    [DataField]
    public float SevereOverloadModifier = 1.15f;

    [ViewVariables]
    public float SlightOverload => Overload * SlightOverloadModifier;

    [ViewVariables]
    public float SevereOverload => Overload * SevereOverloadModifier;

    #endregion


    #region Modifiers

    [DataField]
    public float MovementSpeedModifier = 1f;

    [DataField]
    public float ThirstModifier;

    [DataField]
    public float HungerModifier;

    /*
    [DataField]
    public float OverloadDamageModifier;

    [DataField]
    public DamageSpecifier OverloadDamage;
    */

    #endregion

    #region Timing

    [DataField]
    public TimeSpan UpdateRate = TimeSpan.FromSeconds(1);

    [DataField]
    [AutoPausedField]
    public TimeSpan NextUpdateTime;

    #endregion
}