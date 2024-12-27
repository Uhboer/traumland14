using Content.Shared.Alert;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Intent;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class IntentComponent : Component
{
    [DataField, AutoNetworkedField]
    public Intent Intent;

    #region Grab

    [DataField]
    public bool CanGrab = true;

    #endregion

    #region Disarm

    [DataField]
    public bool CanDisarm = true;

    [DataField]
    public SoundSpecifier DisarmSuccessSound = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg");

    [DataField]
    public float BaseDisarmFailChance = 0.75f;

    #endregion

    #region Actions

    [DataField]
    public string HelpAction = "ActionHelpToggle";

    [DataField, AutoNetworkedField]
    public EntityUid? HelpActionEntity;

    [DataField]
    public string DisarmAction = "ActionDisarmToggle";

    [DataField, AutoNetworkedField]
    public EntityUid? DisarmActionEntity;

    [DataField]
    public string GrabAction = "ActionGrabToggle";

    [DataField, AutoNetworkedField]
    public EntityUid? GrabActionEntity;

    [DataField]
    public string HarmAction = "ActionHarmToggle";

    [DataField, AutoNetworkedField]
    public EntityUid? HarmActionEntity;

    #endregion

    #region Alerts

    [DataField]
    public ProtoId<AlertPrototype> IntentHelpAlert = "IntentHelp";

    [DataField]
    public ProtoId<AlertCategoryPrototype> IntentHelpCategory = "IntentHelp";

    [DataField]
    public ProtoId<AlertPrototype> IntentDisarmAlert = "IntentDisarm";

    [DataField]
    public ProtoId<AlertCategoryPrototype> IntentDisarmCategory = "IntentDisarm";

    [DataField]
    public ProtoId<AlertPrototype> IntentGrabAlert = "IntentGrab";

    [DataField]
    public ProtoId<AlertCategoryPrototype> IntentGrabCategory = "IntentGrab";

    [DataField]
    public ProtoId<AlertPrototype> IntentHarmAlert = "IntentHarm";

    [DataField]
    public ProtoId<AlertCategoryPrototype> IntentHarmCategory = "IntentHarm";

    #endregion
}

[Serializable, NetSerializable]
public sealed class ToggleNetIntentEvent(Intent intent) : CancellableEntityEventArgs
{
    public Intent Intent = intent;
}

[Serializable, NetSerializable]
public enum Intent : byte
{
    Help,
    Disarm,
    Grab,
    Harm
}
