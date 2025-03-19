using Content.Client.Inventory;
using Content.Shared.Hands.Components;

namespace Content.Client.UserInterface.Systems.Inventory.Controls;

public interface IInventoryPanel
{
    /// <summary>
    /// Set active hand and visualioze slot
    /// </summary>
    /// <param name="handName">Hand name</param>
    void SetActiveHand(string? handName);

    /// <summary>
    /// Set entity into hand slot.
    /// </summary>
    /// <param name="handName">Hand name</param>
    /// <param name="ent">Entity</param>
    /// <param name="blocked">Should we block the slot (and visialize it)</param>
    /// <param name="doNotSetEntity">Do not try set entity and only try block slot</param>
    void SetHandEntity(
        string? handName,
        EntityUid? ent,
        bool blocked = false,
        bool doNotSetEntity = false);

    /// <summary>
    /// Set entity into inv's slot
    /// </summary>
    /// <param name="slotName">Slot name</param>
    /// <param name="ent">Entity</param>
    /// <param name="blocked">Should we block the slot (and visialize it)</param>
    void SetSlotEntity(
        string? slotName,
        EntityUid? ent,
        bool blocked = false);

    /// <summary>
    /// Update inventory slots and recreate buttons.
    /// </summary>
    /// <param name="invComp"></param>
    void UpdateSlots(InventorySlotsComponent? invComp);

    /// <summary>
    /// Update hand slots and recreate buttons.
    /// </summary>
    /// <param name="handsComp"></param>
    /// <param name="ignoreHand"></param>
    void UpdateHands(HandsComponent? handsComp, string? ignoreHand = null);

    /// <summary>
    /// Clear all slot buttons from UI
    /// </summary>
    void ClearSlots();
}