using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Client.Gameplay;
using Content.Shared._Finster.Rulebook;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client._Finster.Rulebook;

public sealed class AttributesUIController : UIController, IOnStateEntered<GameplayState>, IOnSystemChanged<ClientAttributesSystem>
{
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!; // VPGui edit

    private AttributesComponent? _attributesComponent;

    public HUDAttributeStats? AttributesControl { get; private set; }

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

        AttributesControl = hudGameplay.AttributeStats;
        UpdateAttributesControl();
    }

    private void OnHudScreenUnload(HUDRoot hud)
    {
        AttributesControl = null;
    }

    public void OnSystemLoaded(ClientAttributesSystem system)
    {
        system.AttributesStartup += AddAttributesControl;
        system.AttributesShutdown += RemoveAttributesControl;
    }

    public void OnSystemUnloaded(ClientAttributesSystem system)
    {
        system.AttributesStartup -= AddAttributesControl;
        system.AttributesShutdown -= RemoveAttributesControl;
    }

    public void OnStateEntered(GameplayState state)
    {
        UpdateAttributesControl();
    }

    public void AddAttributesControl(AttributesComponent component)
    {
        _attributesComponent = component;
        UpdateAttributesControl();
    }

    public void RemoveAttributesControl()
    {
        _attributesComponent = null;
        UpdateAttributesControl();
    }

    public void UpdateAttributesControl()
    {
        if (AttributesControl == null)
            return;

        AttributesControl.Visible = _attributesComponent != null;
    }
}
