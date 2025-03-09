using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<string> UIClickSound =
        CVarDef.Create("finster_interface.click_sound", "/Audio/UserInterface/click.ogg", CVar.REPLICATED);

    public static readonly CVarDef<string> UIHoverSound =
        CVarDef.Create("finster_interface.hover_sound", "/Audio/UserInterface/hover.ogg", CVar.REPLICATED);

    public static readonly CVarDef<string> UICombatModeOnSound =
            CVarDef.Create("finster_interface.combat_on_sound", "/Audio/_Finster/UserInterface/ui_togglecombat.ogg", CVar.REPLICATED);

    public static readonly CVarDef<string> UICombatModeOffSound =
        CVarDef.Create("finster_interface.combat_off_sound", "/Audio/_Finster/UserInterface/ui_toggleoffcombat.ogg", CVar.REPLICATED);

    public static readonly CVarDef<string> UILayout =
        CVarDef.Create("finster_ui.layout", "Separated", CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<string> OverlayScreenChatSize =
        CVarDef.Create("finster_ui.overlay_chat_size", "", CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<string> DefaultScreenChatSize =
        CVarDef.Create("finster_ui.default_chat_size", "", CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<string> SeparatedScreenChatSize =
        CVarDef.Create("finster_ui.separated_chat_size", "0.6,0", CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> OutlineEnabled =
        CVarDef.Create("finster_outline.enabled", true, CVar.CLIENTONLY);
}
