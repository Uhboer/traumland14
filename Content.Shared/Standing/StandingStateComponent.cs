using Content.Shared.Alert;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Standing;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StandingStateComponent : Component
{
    [DataField]
    public SoundSpecifier DownSound { get; private set; } = new SoundCollectionSpecifier("BodyFall");

    [DataField, AutoNetworkedField]
    public StandingState CurrentState { get; set; } = StandingState.Standing;

    [DataField, AutoNetworkedField]
    public bool Standing { get; set; } = true;

    [DataField]
    public ProtoId<AlertCategoryPrototype> LayingCategory = "NALaying";
    [DataField]
    public ProtoId<AlertPrototype> LayingAlert = "Laying";

    /// <summary>
    ///     List of fixtures that had their collision mask changed when the entity was downed.
    ///     Required for re-adding the collision mask.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<string> ChangedFixtures = new();
}

public enum StandingState
{
    Lying,
    GettingUp,
    Standing,
}
