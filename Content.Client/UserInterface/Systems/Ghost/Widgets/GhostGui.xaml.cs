using Content.Client.Stylesheets;
using Content.Client.UserInterface.Systems.Ghost.Controls;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Content.Client._Goobstation.UserInterface.Systems.Ghost.Controls;

namespace Content.Client.UserInterface.Systems.Ghost.Widgets;

[GenerateTypedNameReferences]
public sealed partial class GhostGui : UIWidget
{
    public GhostTargetWindow TargetWindow { get; }
    public GhostBarRulesWindow GhostBarWindow { get; }

    public event Action? RequestWarpsPressed;
    public event Action? ReturnToBodyPressed;
    public event Action? GhostRolesPressed;
    public event Action? GhostBarPressed; // Goobstation - Ghost Bar
    public event Action? ReturnToRoundPressed;

    public GhostGui()
    {
        RobustXamlLoader.Load(this);

        TargetWindow = new GhostTargetWindow();

        GhostBarWindow = new GhostBarRulesWindow();

        MouseFilter = MouseFilterMode.Ignore;

        GhostWarpButton.OnPressed += _ => RequestWarpsPressed?.Invoke();
        ReturnToBodyButton.OnPressed += _ => ReturnToBodyPressed?.Invoke();
        GhostRolesButton.OnPressed += _ => GhostRolesPressed?.Invoke();
        //GhostBarButton.OnPressed += _ => GhostBarPressed?.Invoke(); // Goobstation - Ghost Bar
        ReturnToRound.OnPressed += _ => ReturnToRoundPressed?.Invoke();
    }

    public void Hide()
    {
        TargetWindow.Close();
        GhostBarWindow.Close(); // Goobstation - Ghost Bar
        Visible = false;
    }

    public void Update(int? roles, bool? canReturnToBody)
    {
        ReturnToBodyButton.Disabled = !canReturnToBody ?? true;

        if (roles != null)
        {
            GhostRolesButton.Text = Loc.GetString("ghost-gui-ghost-roles-button", ("count", roles));
            if (roles > 0)
            {
                GhostRolesButton.StyleClasses.Add(StyleBase.ButtonDanger);
            }
            else
            {
                GhostRolesButton.StyleClasses.Remove(StyleBase.ButtonDanger);
            }
        }

        TargetWindow.Populate();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            TargetWindow.Dispose();
            GhostBarWindow.Dispose(); // Goobstation - Ghost Bar
        }
    }
}
