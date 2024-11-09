namespace Content.Shared.DA.TimeCycle;

[RegisterComponent]
public sealed partial class TimeCycleComponent : Component
{
    public TimeSpan? DelayTime;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool SpeedUp = false;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool Paused = false;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string Palette = "DefaultTimeCycle";

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan TimeLength { get; set; } = TimeSpan.FromSeconds(4);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan CurrentTime { get; set; } = TimeSpan.FromHours(12);
}
