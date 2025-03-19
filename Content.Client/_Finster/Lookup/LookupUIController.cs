using Content.Client._Finster.UserActions.Tabs;
using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client._Finster.Lookup;

public sealed class LookupUIController : UIController
{
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!;

    public HUDLookupLabel? LookupLabel;

    public override void Initialize()
    {
        base.Initialize();

        _vpUIManager.OnScreenLoad += OnHudScreenLoad;
        _vpUIManager.OnScreenUnload += OnHudScreenUnload;
    }

    private void OnHudScreenLoad(HUDRoot hud)
    {
        var hudGameplay = hud as HUDGameplayState;
        if (hudGameplay is null)
            return;

        LookupLabel = hudGameplay.LookupLabel;
    }

    private void OnHudScreenUnload(HUDRoot hud)
    {
        LookupLabel = null;
    }

    private void OnAttached()
    {
    }

    private void OnDetached()
    {
    }
}
