using Content.Shared.CCVar;
using Content.Shared.Popups;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Player;

namespace Content.Server._White.Notice
{

    //^.^
    public sealed class NoticeSystem : EntitySystem
    {
        [Dependency] private readonly IConsoleHost _consoleHost = default!;
        [Dependency] private readonly INetConfigurationManager _netConfigManager = default!;

        public void SendNoticeMessage(EntityUid uid, string message, PopupType? type = null)
        {
            if (!TryComp(uid, out ActorComponent? actor))
                return;

            var should = _netConfigManager.GetClientCVar(actor.PlayerSession.Channel, CCVars.LogChatActions);

            if (should)
            {
                var fontSizeDict = new Dictionary<PopupType, string>
                {
                  { PopupType.Medium, "12" },
                  { PopupType.MediumCaution, "12" },
                  { PopupType.Large, "15" },
                  { PopupType.LargeCaution, "15" }
                };

                var fontsize = "10";
                if (type != null)
                    fontsize = fontSizeDict.ContainsKey((PopupType) type) ? fontSizeDict[(PopupType) type] : "10";
                var fontcolor = (type == PopupType.LargeCaution || type == PopupType.MediumCaution || type == PopupType.SmallCaution) ? "c62828" : "aeabc4";

                var formatedMessage = Loc.GetString("notice-command", ("fontsize", fontsize), ("fontcolor", fontcolor), ("message", message));
                _consoleHost.RemoteExecuteCommand(actor.PlayerSession, $"notice {formatedMessage}");
            }
        }

    }
}
