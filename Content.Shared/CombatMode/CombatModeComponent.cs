using Content.Shared.Alert;
using Content.Shared.MouseRotator;
using Content.Shared.Movement.Components;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.CombatMode
{
    /// <summary>
    ///     Stores whether an entity is in "combat mode"
    ///     This is used to differentiate between regular item interactions or
    ///     using *everything* as a weapon.
    /// </summary>
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
    [Access(typeof(SharedCombatModeSystem))]
    public sealed partial class CombatModeComponent : Component
    {
        #region Disarm

        /// <summary>
        /// Whether we are able to disarm. This requires our active hand to be free.
        /// False if it's toggled off for whatever reason, null if it's not possible.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), DataField("canDisarm")]
        public bool? CanDisarm;

        [DataField("disarmSuccessSound")]
        public SoundSpecifier DisarmSuccessSound = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg");

        [DataField("disarmFailChance")]
        public float BaseDisarmFailChance = 0.75f;

        #endregion

        [DataField("combatToggleAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string CombatToggleAction = "ActionCombatModeToggle";

        [DataField, AutoNetworkedField]
        public EntityUid? CombatToggleActionEntity;

        [DataField]
        public ProtoId<AlertPrototype> CombatModeAlert = "CombatMode";

        [DataField]
        public ProtoId<AlertCategoryPrototype> CombatModeCategory = "NativeActions";

        [ViewVariables(VVAccess.ReadWrite), DataField("isInCombatMode"), AutoNetworkedField]
        public bool IsInCombatMode;

        /// <summary>
        ///     Will add <see cref="MouseRotatorComponent"/> and <see cref="NoRotateOnMoveComponent"/>
        ///     to entities with this flag enabled that enter combat mode, and vice versa for removal.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool ToggleMouseRotator = true;

        // BACKMEN START
        /// <summary>
        ///     If true, sets <see cref="MouseRotatorComponent.AngleTolerance"/> to 1 degree and <see cref="MouseRotatorComponent.Simple4DirMode"/>
        ///     to false when the owner enters combatmode. This is currently being tested as of 06.12.24,
        ///     so a simple bool switch should suffice.
        ///     Leaving AutoNetworking just in case shitmins need to disable it for someone. Will only take effect when re-enabling combat mode.
        /// </summary>
        /// <remarks>
        ///     No effect if <see cref="ToggleMouseRotator"/> is false.
        /// </remarks>
        [DataField, AutoNetworkedField]
        public bool SmoothRotation = true;
        // BACKMEN END

        /// <summary>
        ///     Set the defense style for the character.
        ///     If parry - character will try to parry attacks using his weapon.
        ///     If dodge - character will try dodge attacks using his dexterity attribute.
        /// </summary>
        [DataField, AutoNetworkedField]
        public DefenseMode DefenseStyle = DefenseMode.Dodge;

        /// <summary>
        ///     Combat style. <seealso cref="CombatIntent"/>
        /// </summary>
        [DataField, AutoNetworkedField]
        public CombatIntent Style = CombatIntent.Feint;

        // Show popup combat intents list.
        public bool ShowCombatStyles = false;
    }

    public enum DefenseMode : uint
    {
        Parry,
        Dodge
    }

    /// <summary>
    ///     Hardcoded combat styles.
    ///     It should be hardcoded, because there is no reasons to seperate it
    ///     or make modulary.
    /// </summary>
    [Serializable, NetSerializable]
    public enum CombatIntent : uint
    {
        /// <summary>
        ///     Deal weak damage, like for friedly fire
        /// </summary>
        Weak,

        /// <summary>
        ///     Deal accurate attacks to oponent (based on Dexterity).
        ///     But increase melee attacking cooldown.
        /// </summary>
        Aimed,

        /// <summary>
        ///     Fury attack add debuff for oponent parrity.
        ///     But also add debuff on aiming for attacker.
        /// </summary>
        Furious,

        /// <summary>
        ///     Make strong attack (from Strength attribute).
        ///     But cost stamina (or, on heavy AOE attack - it costs more).
        /// </summary>
        Strong,

        /// <summary>
        ///     Increase parry bonus, debuff to attacking, reduce movement speed.
        ///     It also increase modifier for attacker on feint.
        /// </summary>
        Defend,

        /// <summary>
        ///     Make able to use two attacks (from all hands).
        ///     Debuff to aiming.
        /// </summary>
        Dual,

        /// <summary>
        ///     On succesful parrying from oponent - add on him effect to make
        ///     ignore parrying on next attacking from attacker.
        ///
        ///     Do not deal damage on succesful feint. But can deal, when oponent under effect (generic damage).
        /// </summary>
        Feint,

        // He-he, thats not intent. Do not use :eyes:
        Max
    }

    [Serializable, NetSerializable]
    public sealed class ToggleCombatModeEvent : CancellableEntityEventArgs;
}
