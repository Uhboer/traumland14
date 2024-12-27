using System.Numerics;
using Content.Client._White.Intent;
using Content.Client.CombatMode;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Client.UserInterface.Systems.NativeActions.Controls;
using Content.Client.UserInterface.Systems.NativeActions.Widgets;
using Content.Shared.CombatMode;
using Content.Shared.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using static Robust.Shared.Input.Binding.PointerInputCmdHandler;

namespace Content.Client.UserInterface.Systems;

public sealed class NativeActionsUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>, IOnSystemChanged<CombatModeSystem>
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [UISystemDependency] private readonly CombatModeSystem _combatSystem = default!;
    [UISystemDependency] private readonly IntentSystem _intent = default!;

    private NativeActionsGui? _nativeActionsGui;

    public override void Initialize()
    {
        base.Initialize();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
    }

    private void OnScreenLoad()
    {
        if (UIManager.ActiveScreen == null)
            return;

        ReloadActions();
    }

    public void OnStateEntered(GameplayState state)
    {
        //bind enable/disable combat mode key to ToggleCombatMenu;
        CommandBinds.Builder
           .Bind(ContentKeyFunctions.ToggleCombatMode, InputCmdHandler.FromDelegate(_ => ToggleCombatMode()))
           .Register<CombatModeSystem>();

        var builder = CommandBinds.Builder;

        // Binds for intents
        var hotbarKeys = ContentKeyFunctions.GetIntentsBoundKeys();
        for (var i = 0; i < hotbarKeys.Length; i++)
        {
            var boundId = i; // This is needed, because the lambda captures it.
            var boundKey = hotbarKeys[i];
            builder = builder.Bind(boundKey, new PointerInputCmdHandler((in PointerInputCmdArgs args) =>
            {
                if (args.State != BoundKeyState.Up)
                    return false;

                TriggerIntent(boundId);
                return true;
            }, false, true));
        }

        builder.Register<IntentSystem>();
    }

    public void OnStateExited(GameplayState state)
    {
        CommandBinds.Unregister<CombatModeSystem>();
        CommandBinds.Unregister<IntentSystem>();
    }

    public void OnPlayerAttached(EntityUid uid)
    {
    }

    public void TriggerIntent(int id)
    {
        if (_nativeActionsGui == null)
            return;

        var uid = _playerManager.LocalEntity;
        if (uid == null)
            return;

        if (!_intent.HasIntents((EntityUid) uid))
            return;

        _intent.LocalToggleIntent((Shared._White.Intent.Intent) id);

        var intentsContainer = _nativeActionsGui.IntentsContainer;
        var intentsContainer2 = _nativeActionsGui.IntentsContainer2;

        // FIXME: Looks not good...
        if (id <= 1)
        {
            SetClickIntentButton(intentsContainer, id);
        }
        else
        {
            SetClickIntentButton(intentsContainer2, id);
        }
    }

    private void SetClickIntentButton(BoxContainer container, int id)
    {
        if (container.ChildCount <= 0)
            return;

        foreach (var item in container.Children)
        {
            var button = (AlertControl) item;

            if (button.Name == id.ToString() && button != null)
            {
                button.SetClickPressed(true);
                return;
            }
        }
    }

    public void ReloadActions()
    {
        if (UIManager.ActiveScreen == null)
            return;

        if (UIManager.ActiveScreen.GetWidget<NativeActionsGui>() is { } nativeActions)
        {
            if (_nativeActionsGui == null)
                _nativeActionsGui = nativeActions;
        }
    }

    public void ToggleCombatMode(bool ignoreButton = false)
    {
        //if (_nativeActionsGui != null && ignoreButton != true)
        //    _nativeActionsGui.CombatModeButton.Toggle();

        _combatSystem.LocalToggleCombatMode();
    }
/*
    public void ToggleLayingMode()
    {

    }

    public void ToggleWalkMode()
    {

    }
*/

    public void OnPlayerDetached(EntityUid uid)
    {
    }

    public void OnSystemLoaded(CombatModeSystem system)
    {
        system.LocalPlayerAttached += OnPlayerAttached;
        system.LocalPlayerDetached += OnPlayerDetached;
    }

    public void OnSystemUnloaded(CombatModeSystem system)
    {
        system.LocalPlayerAttached -= OnPlayerAttached;
        system.LocalPlayerDetached -= OnPlayerDetached;
    }
}
