using System.Numerics;
using Content.Shared.Strip;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared.Inventory;

[Prototype("inventoryTemplate")]
public sealed partial class InventoryTemplatePrototype : IPrototype
{
    [IdDataField] public string ID { get; } = string.Empty;

    [DataField("slots")] public SlotDefinition[] Slots { get; private set; } = Array.Empty<SlotDefinition>();
}

[DataDefinition]
public sealed partial class SlotDefinition
{
    [DataField("name", required: true)] public string Name { get; private set; } = string.Empty;
    [DataField("slotTexture")] public string TextureName { get; private set; } = "pocket";
    /// <summary>
    /// The texture displayed in a slot when it has an item inside of it.
    /// </summary>
    [DataField] public string FullTextureName { get; private set; } = "SlotBackground";
    [DataField("slotFlags")] public SlotFlags SlotFlags { get; private set; } = SlotFlags.PREVENTEQUIP;
    [DataField("showInWindow")] public bool ShowInWindow { get; private set; } = true;
    [DataField("slotGroup")] public string SlotGroup { get; private set; } = "Default";
    /// <summary>
    /// Like 'slotGroup', but for HUD slots.
    /// </summary>
    [DataField("hudSlotGroup")] public string HUDSlotGroup { get; private set; } = "Default";
    [DataField("stripTime")] public TimeSpan StripTime { get; private set; } = TimeSpan.FromSeconds(4f);

    [DataField("uiWindowPos", required: true)]
    public Vector2i UIWindowPosition { get; private set; }
    /// <summary>
    /// Like 'uiWindowPos', but for HUD.
    /// </summary>
    [DataField("hudWindowPos")]
    public Vector2i HUDWindowPosition { get; private set; } = Vector2i.Zero;
    [DataField("altHudWindowPos")]
    public Vector2i AltHUDWindowPosition { get; private set; } = Vector2i.Zero;

    [DataField("strippingWindowPos", required: true)]
    public Vector2i StrippingWindowPos { get; private set; }

    [DataField("dependsOn")] public string? DependsOn { get; private set; }

    [DataField("dependsOnComponents")] public ComponentRegistry? DependsOnComponents { get; private set; }

    [DataField("displayName", required: true)]
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>
    ///     Whether or not this slot will have its item hidden in the strip menu, and block interactions.
    ///     <seealso cref="SharedStrippableSystem.IsStripHidden"/>
    /// </summary>
    [DataField("stripHidden")] public bool StripHidden { get; private set; }

    /// <summary>
    ///     Offset for the clothing sprites.
    /// </summary>
    [DataField("offset")] public Vector2 Offset { get; private set; } = Vector2.Zero;

    /// <summary>
    ///     Entity whitelist for CanEquip checks.
    /// </summary>
    [DataField("whitelist")] public EntityWhitelist? Whitelist = null;

    /// <summary>
    ///     Entity blacklist for CanEquip checks.
    /// </summary>
    [DataField("blacklist")] public EntityWhitelist? Blacklist = null;

    /// <summary>
    ///     Entity whitelist for CanEquip checks, on spawn only.
    /// </summary>
    [DataField("spawnWhitelist")] public EntityWhitelist? SpawnWhitelist = null;

    /// <summary>
    ///     Entity blacklist for CanEquip checks, on spawn only.
    /// </summary>
    [DataField("spawnBlacklist")] public EntityWhitelist? SpawnBlacklist = null;

    /// <summary>
    ///     Is this slot disabled? Could be due to severing or other reasons.
    /// </summary>
    [DataField] public bool Disabled;
}
