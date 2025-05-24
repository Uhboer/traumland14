using Content.Client._Finster.Misc;
using Content.Client.UserInterface.Systems.Viewport;
using Content.Shared.CCVar;
using Robust.Client.Graphics;

namespace Content.Client._ViewportGui.ViewportUserInterface.UI;

public class HUDGhostState : HUDRoot
{
    public HUDBuildInfoLabel BuildInfoLable { get; set; }

    public HUDGhostState()
    {
        var viewportSize = new Vector2i(CCVars.ViewportWidth.DefaultValue, ViewportUIController.ViewportHeight);
        var drawingInfo = new ViewportDrawingInfo(
            (0, 0),
            viewportSize,
            (viewportSize.X * EyeManager.PixelsPerMeter, viewportSize.Y * EyeManager.PixelsPerMeter),
            (0, 0),
            (0, 0)
        );
        DrawingInfo = drawingInfo;

        BuildInfoLable = new HUDBuildInfoLabel();

        AddChild(BuildInfoLable);
    }
}
