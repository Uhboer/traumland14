using System.Linq;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Client.Inventory;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.Hands.Components;
using Robust.Client.UserInterface;
using Robust.Shared.Timing;
using static Content.Client.Inventory.ClientInventorySystem;

namespace Content.Client.UserInterface.Systems.Inventory.Controls;

/// <summary>
/// Left panel of the HUD.
/// </summary>
public class HUDInventoryPanel : HUDTextureRect, IInventoryPanel
{
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    private HUDInventoryUIController _controller;
    private HUDHandButton? _activeHand;

    /// <summary>
    /// DON'T USE IT WITH inventoryTemplate! It is only for hands definition on UIControler's layer.
    /// </summary>
    public const string HandsGroup = "Hands";

    public static int DefaultButtonSize => HUDSlotControl.DefaultButtonSize;

    /// <summary>
    /// Contain all slots from <seealso cref="HUDBoxContainer"'s in single list./>
    /// </summary>
    public List<HUDSlotControl> Slots { get; private set; } = new();

    /// <summary>
    /// Slots, what should be visible everytime.
    /// </summary>
    public HUDBoxContainer SlotsContainer;

    /// <summary>
    /// For hands!
    /// </summary>
    public HUDBoxContainer HandsContainer;

    public HUDInventoryPanel(HUDInventoryUIController controller, Vector2i size)
    {
        IoCManager.InjectDependencies(this);

        _controller = controller;
        Size = size;
        Name = "InventoryPanel";

        SlotsContainer = new();
        SlotsContainer.OnChildAdded += AddChildToSlots;
        SlotsContainer.OnChildRemoved += RemoveChildFromSlots;
        AddChild(SlotsContainer);

        HandsContainer = new();
        HandsContainer.OnChildAdded += AddChildToSlots;
        HandsContainer.OnChildRemoved += RemoveChildFromSlots;
        AddChild(HandsContainer);
    }

    private void AddChildToSlots(HUDControl child)
    {
        var slot = child as HUDSlotControl;
        if (slot != null)
            Slots.Add(slot);
    }

    private void RemoveChildFromSlots(HUDControl child)
    {
        HUDSlotControl? childToRemove = null;
        foreach (var slot in Slots)
        {
            if (slot.Name == child.Name)
            {
                childToRemove = slot;
                break;
            }
        }
        if (childToRemove != null)
            Slots.Remove(childToRemove);
    }

    public void UpdateSlots(InventorySlotsComponent? invComp)
    {
        ClearSlots();

        // Inventory slots
        if (invComp is not null)
        {
            foreach (var (_, data) in invComp.SlotData)
            {
                CreateSlotButton(data);
            }

            SlotsContainer.Position = (0, DefaultButtonSize);
        }
    }

    public void ClearSlots()
    {
        SlotsContainer.RemoveAllChildren();
        SlotsContainer.Size = (0, 0);
        SlotsContainer.Position = (0, 0);
    }

    public void UpdateHands(HandsComponent? handsComp, string? ignoreHand = null)
    {
        ClearHands();

        // Hand slots
        if (handsComp is not null)
        {
            foreach (var (name, data) in handsComp.Hands)
            {
                if (name != ignoreHand)
                    CreateHandButton(name, data, handsComp);
            }

            HandsContainer.Position = (0, DefaultButtonSize + (DefaultButtonSize * 3));

            if (handsComp.ActiveHand != null)
                SetActiveHand(handsComp.ActiveHand.Name);
        }
    }

    public void ClearHands()
    {
        HandsContainer.RemoveAllChildren();
        HandsContainer.Size = (0, 0);
        HandsContainer.Position = (0, 0);
    }

    private HUDSlotButton CreateSlotButton(SlotData data)
    {
        var slotDef = data.SlotDef;

        var slotButton = new HUDSlotButton(data);
        slotButton.Position = slotDef.HUDWindowPosition * DefaultButtonSize;
        slotButton.Pressed += _controller.ItemPressed;

        SlotsContainer.AddChild(slotButton);

        return slotButton;
    }

    private HUDHandButton CreateHandButton(string name, Hand data, HandsComponent? handsComp = null)
    {
        var parentChildCount = HandsContainer.ChildCount;
        var handButton = new HUDHandButton(name, data.Location);
        handButton.Position = (parentChildCount * DefaultButtonSize, 0);
        handButton.Pressed += _controller.HandPressed;

        HandsContainer.AddChild(handButton);

        // TODO: Move to another UI's button. Because it works sucks
        // Add switch hands button. It only shows when entity has 2 hands (left and right)
        if (handsComp != null &&
            parentChildCount <= 0 &&
            handsComp.Hands.Count == 2) // because two hands
        {
            var switchHandsButton = new HUDHandButton("switchbutton", data.Location);
            switchHandsButton.Position = ((parentChildCount + 1) * DefaultButtonSize, DefaultButtonSize);
            switchHandsButton.OnPressed += (_) =>
            {
                SwitchHands(switchHandsButton);
            };

            // Force texture update
            if (handsComp.ActiveHand != null)
            {
                if (handsComp.ActiveHand.Location == HandLocation.Right)
                    switchHandsButton.ButtonTexturePath = "Buttons/hands_right";
                else
                    switchHandsButton.ButtonTexturePath = "Buttons/hands_left";
            }

            HandsContainer.AddChild(switchHandsButton);
        }

        return handButton;
    }

    // Use only with switch hand button
    private void SwitchHands(HUDHandButton switchButton)
    {
        if (_activeHand is null)
            return;

        var curIdx = _activeHand.GetPositionInParent();

        if (curIdx + 2 >= HandsContainer.ChildCount)
            curIdx = 0;
        else
            curIdx += 2;

        var childsList = HandsContainer.Children.ToArray();
        var hand = childsList[curIdx] as HUDHandButton;
        if (hand is null || hand.Name == switchButton.Name)
            return;

        _controller.SetHand(hand.HandName);
    }

    public void SetActiveHand(string? handName)
    {
        if (_activeHand is not null)
            _activeHand.Highlight = false;

        HUDHandButton? switchHandsButton = null;

        foreach (var child in HandsContainer.Children)
        {
            var hand = child as HUDHandButton;

            if (hand is null)
                continue;

            // For updating switch button
            if (hand.HandName == "switchbutton")
                switchHandsButton = hand;

            if (hand.HandName == handName)
            {
                _activeHand = hand;
                _activeHand.Highlight = true;
            }
        }

        if (switchHandsButton is null || _activeHand is null)
            return;

        if (_activeHand.HandLocation == HandLocation.Right)
            switchHandsButton.ButtonTexturePath = "Buttons/hands_right";
        else
            switchHandsButton.ButtonTexturePath = "Buttons/hands_left";
    }

    public void SetHandEntity(string? handName, EntityUid? ent, bool blocked = false, bool doNotSetEntity = false)
    {
        if (handName == null)
            return;

        HUDHandButton? handButton = null;
        foreach (var child in HandsContainer.Children)
        {
            var childButton = child as HUDHandButton;
            if (childButton is null || childButton.HandName != handName)
                continue;
            else
                handButton = childButton;
        }
        if (handButton == null)
            return;

        if (!doNotSetEntity)
            handButton.SetEntity(ent);
        handButton.Blocked = blocked;
    }

    public void SetSlotEntity(string? slotName, EntityUid? ent, bool blocked = false)
    {
        if (slotName == null)
            return;

        var slotButton = Slots.FirstOrDefault(control => control.SlotName == slotName);
        if (slotButton == null)
            return;

        slotButton.SetEntity(ent);
        slotButton.Blocked = blocked;
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
    }
}
