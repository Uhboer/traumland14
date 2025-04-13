namespace Content.Server.Power.Components;

[RegisterComponent]
public sealed partial class PowerVerticalAdapterComponent : Component
{
    /// <summary>
    ///     In joule
    /// </summary>
    [DataField("maxPowerTransfer")]
    public float MaxPowerTransfer = 10000f;
}
