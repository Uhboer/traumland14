using Content.Client._Finster.Lookup;
using Content.Client._Finster.Misc;
using Content.Client._Finster.Rulebook;
using Content.Client._Finster.UserInterface.RichText;
using Content.Client._Shitmed.UserInterface.Systems.Targeting;
using Content.Client._Shitmed.UserInterface.Systems.Targeting.Controls;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Client.UserInterface.Systems.Inventory;
using Content.Client.UserInterface.Systems.Inventory.Controls;
using Content.Client.UserInterface.Systems.Viewport;
using Content.Shared.CCVar;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;

namespace Content.Client._ViewportGui.ViewportUserInterface.UI;

/// <summary>
/// GameplayState screen HUD, contains all needed elements.
/// </summary>
public class HUDGameplayState : HUDRoot
{
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!;

    public HUDGameplayType Type { get; set; }

    public IInventoryPanel InventoryPanel { get; set; }

    public HUDAlertsPanel AlertsPanel { get; set; }
    public HUDTargetDoll TargetingControl { get; set; }
    public HUDAttributeStats AttributeStats { get; set; }

    public HUDBuildInfoLabel BuildInfoLable { get; set; }

    public HUDLookupLabel LookupLabel { get; set; }

    public HUDGameplayState(HUDGameplayType hudType)
    {
        IoCManager.InjectDependencies(this);
        Type = hudType;

        var invController = _uiManager.GetUIController<HUDInventoryUIController>();
        var targetingController = _uiManager.GetUIController<TargetingUIController>();

        if (hudType == HUDGameplayType.Lifeweb)
        {
            var viewportSize = new Vector2i(CCVars.ViewportWidth.DefaultValue, ViewportUIController.ViewportHeight);
            var drawingInfo = new ViewportDrawingInfo(
                (-3, 0),
                viewportSize,
                ((viewportSize.X + 3 + 1) * EyeManager.PixelsPerMeter, viewportSize.Y * EyeManager.PixelsPerMeter),
                (3 + 1, 0),
                (3, 0)
            );
            DrawingInfo = drawingInfo;

            var textureInv = _vpUIManager.GetThemeTexture("left_panel_background");
            var inventoryPanel = new HUDInventoryPanel(invController, textureInv.Size);
            inventoryPanel.Texture = textureInv;
            inventoryPanel.Position = (0, 0); // fucking calculus

            var textureAlerts = _vpUIManager.GetThemeTexture("right_panel_background");
            AlertsPanel = new HUDAlertsPanel(textureAlerts.Size);
            AlertsPanel.Texture = textureAlerts;
            AlertsPanel.Position = (
                EyeManager.PixelsPerMeter * (3 + ViewportUIController.ViewportHeight), 0); // fucking calculus

            BuildInfoLable = new HUDBuildInfoLabel();
            BuildInfoLable.Alignment = HUDBuildInfoAlignment.Center;

            LookupLabel = new HUDLookupLabel();
            LookupLabel.Alignment = LookupAlignment.Bottom;
            LookupLabel.TextPositionX = 336;

            TargetingControl = new HUDTargetDoll(targetingController);
            AttributeStats = new HUDAttributeStats();

            InventoryPanel = inventoryPanel;
            AddChild(inventoryPanel);
            AddChild(AlertsPanel);
            AddChild(BuildInfoLable);
            AddChild(LookupLabel);
            AlertsPanel.AddChild(TargetingControl);
            AlertsPanel.AddChild(AttributeStats);
        }
        else // By default - Interbay
        {
            var viewportSize = new Vector2i(CCVars.ViewportWidth.DefaultValue, ViewportUIController.ViewportHeight);
            var drawingInfo = new ViewportDrawingInfo(
                (0, 0),
                viewportSize,
                ((viewportSize.X + 1) * EyeManager.PixelsPerMeter, (viewportSize.Y + 1) * EyeManager.PixelsPerMeter),
                (1, 1),
                (0, 0)
            );
            DrawingInfo = drawingInfo;

            var texture = _vpUIManager.GetThemeTexture("down_panel_background");
            var inventoryPanel = new HUDInventoryPanelLegacy(invController, texture.Size);
            inventoryPanel.Texture = texture;
            inventoryPanel.Position = (0, EyeManager.PixelsPerMeter * ViewportUIController.ViewportHeight); // fucking calculus

            var textureAlerts = _vpUIManager.GetThemeTexture("right_panel_background_alt");
            AlertsPanel = new HUDAlertsPanel(textureAlerts.Size);
            AlertsPanel.Texture = textureAlerts;
            AlertsPanel.Position = (EyeManager.PixelsPerMeter * ViewportUIController.ViewportHeight, 0); // fucking calculus

            BuildInfoLable = new HUDBuildInfoLabel();

            LookupLabel = new HUDLookupLabel();

            TargetingControl = new HUDTargetDoll(targetingController);
            AttributeStats = new HUDAttributeStats();

            InventoryPanel = inventoryPanel;
            AddChild(inventoryPanel);
            AddChild(AlertsPanel);
            AddChild(BuildInfoLable);
            AddChild(LookupLabel);
            AlertsPanel.AddChild(TargetingControl);
            AlertsPanel.AddChild(AttributeStats);
        }
    }
}

public enum HUDGameplayType : int
{
    Interbay,
    Lifeweb
}
