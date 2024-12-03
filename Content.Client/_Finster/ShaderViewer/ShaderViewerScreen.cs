using System.Text.RegularExpressions;
using Content.Client._Finster.ShaderViewer.UI;
using Content.Client.UserInterface.Systems.EscapeMenu;
using Robust.Client;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Utility;
using UsernameHelpers = Robust.Shared.AuthLib.UsernameHelpers;

namespace Content.Client._Finster.ShaderViewer
{
    /// <summary>
    ///     Main menu screen that is the first screen to be displayed when the game starts.
    /// </summary>
    // Instantiated dynamically through the StateManager, Dependencies will be resolved.
    public sealed class ShaderViewerScreen : Robust.Client.State.State
    {
        [Dependency] private readonly IBaseClient _client = default!;
        [Dependency] private readonly IClientNetManager _netManager = default!;
        [Dependency] private readonly IConfigurationManager _configurationManager = default!;
        [Dependency] private readonly IGameController _controllerProxy = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;

        private ShaderViewerControl _shaderViewerControl = default!;

        /// <inheritdoc />
        protected override void Startup()
        {
            _shaderViewerControl = new ShaderViewerControl(_resourceCache, _configurationManager);
            _userInterfaceManager.StateRoot.AddChild(_shaderViewerControl);

            //_shaderViewerControl.QuitButton.OnPressed += QuitButtonPressed;
            //_shaderViewerControl.OptionsButton.OnPressed += OptionsButtonPressed;
            //_shaderViewerControl.DirectConnectButton.OnPressed += DirectConnectButtonPressed;
            //_shaderViewerControl.AddressBox.OnTextEntered += AddressBoxEntered;
            //_shaderViewerControl.ChangelogButton.OnPressed += ChangelogButtonPressed;

            //_client.RunLevelChanged += RunLevelChanged;
        }

        /// <inheritdoc />
        protected override void Shutdown()
        {
            _shaderViewerControl.Dispose();
        }

/*
        private void ChangelogButtonPressed(BaseButton.ButtonEventArgs args)
        {
            _userInterfaceManager.GetUIController<ChangelogUIController>().ToggleWindow();
        }

        private void OptionsButtonPressed(BaseButton.ButtonEventArgs args)
        {
            _userInterfaceManager.GetUIController<OptionsUIController>().ToggleWindow();
        }

        private void QuitButtonPressed(BaseButton.ButtonEventArgs args)
        {
            _controllerProxy.Shutdown();
        }

        private void DirectConnectButtonPressed(BaseButton.ButtonEventArgs args)
        {
            var input = _shaderViewerControl.AddressBox;
            TryConnect(input.Text);
        }

        private void AddressBoxEntered(LineEdit.LineEditEventArgs args)
        {
        }
*/
    }
}
