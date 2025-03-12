using Content.Client.Verbs;
using Content.Shared.Eye.Blinding;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Input;
using Content.Shared.Verbs;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Utility;
using System.Linq;
using System.Numerics;
using System.Threading;
using Content.Shared.Eye.Blinding.Components;
using Robust.Client;
using static Content.Shared.Interaction.SharedInteractionSystem;
using static Robust.Client.UserInterface.Controls.BoxContainer;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Direction = Robust.Shared.Maths.Direction;
using Content.Shared.Chat;
using Content.Client.UserInterface.Systems.Chat;

namespace Content.Client.Examine
{
    [UsedImplicitly]
    public sealed class ExamineSystem : ExamineSystemShared
    {
        [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IUserInterfaceManager _ui = default!;
        [Dependency] private readonly IEyeManager _eyeManager = default!;
        [Dependency] private readonly VerbSystem _verbSystem = default!;

        public const string StyleClassEntityTooltip = "entity-tooltip";

        private EntityUid _examinedEntity;
        private EntityUid _lastExaminedEntity;
        private EntityUid _playerEntity;
        private Popup? _examineTooltipOpen;
        private ScreenCoordinates _popupPos;
        private CancellationTokenSource? _requestCancelTokenSource;
        private int _idCounter;

        public override void Initialize()
        {
            UpdatesOutsidePrediction = true;

            SubscribeLocalEvent<GetVerbsEvent<ExamineVerb>>(AddExamineVerb);

            SubscribeNetworkEvent<ExamineSystemMessages.ExamineInfoResponseMessage>(OnExamineInfoResponse);

            CommandBinds.Builder
                .Bind(ContentKeyFunctions.ExamineEntity, new PointerInputCmdHandler(HandleExamine, outsidePrediction: true))
                .Register<ExamineSystem>();

            _idCounter = 0;
        }

        public override void Shutdown()
        {
            CommandBinds.Unregister<ExamineSystem>();
            base.Shutdown();
        }

        public override bool CanExamine(EntityUid examiner, MapCoordinates target, Ignored? predicate = null, EntityUid? examined = null, ExaminerComponent? examinerComp = null)
        {
            if (!Resolve(examiner, ref examinerComp, false))
                return false;

            if (examinerComp.SkipChecks)
                return true;

            if (examinerComp.CheckInRangeUnOccluded)
            {
                // TODO fix this. This should be using the examiner's eye component, not eye manager.
                var b = _eyeManager.GetWorldViewbounds();
                if (!b.Contains(target.Position))
                    return false;
            }

            return base.CanExamine(examiner, target, predicate, examined, examinerComp);
        }

        private bool HandleExamine(in PointerInputCmdHandler.PointerInputCmdArgs args)
        {
            var entity = args.EntityUid;

            if (!args.EntityUid.IsValid() || !EntityManager.EntityExists(entity))
            {
                return false;
            }

            _playerEntity = _playerManager.LocalEntity ?? default;

            if (_playerEntity == default || !CanExamine(_playerEntity, entity))
            {
                return false;
            }

            DoExamine(entity);
            return true;
        }

        private void AddExamineVerb(GetVerbsEvent<ExamineVerb> args)
        {
            if (!CanExamine(args.User, args.Target))
                return;

            // Basic examine verb.
            ExamineVerb verb = new();
            verb.Category = VerbCategory.Examine;
            verb.Priority = 10;
            // Center it on the entity if they use the verb instead.
            verb.Act = () => DoExamine(args.Target, false);
            verb.Text = Loc.GetString("examine-verb-name");
            verb.Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/examine.svg.192dpi.png"));
            verb.ShowOnExamineTooltip = false;
            verb.ClientExclusive = true;
            args.Verbs.Add(verb);
        }

        private void OnExamineInfoResponse(ExamineSystemMessages.ExamineInfoResponseMessage ev)
        {
            var player = _playerManager.LocalEntity;
            if (player == null)
                return;

            // Prevent updating a new tooltip.
            if (ev.Id != 0 && ev.Id != _idCounter)
                return;

            // Tooltips coming in from the server generally prioritize
            // opening at the old tooltip rather than the cursor/another entity,
            // since there's probably one open already if it's coming in from the server.
            var entity = GetEntity(ev.EntityUid);

            UpdateTooltipInfo(player.Value, entity, ev.Message);
        }

        public override void SendExamineMessage(
                EntityUid player,
                EntityUid target,
                FormattedMessage message,
                bool getVerbs,
                bool centerAtCursor,
                string? titleName = null)
        {
            //UpdateTooltipInfo(player, target, message);
        }

        /// <summary>
        ///     Fills the examine tooltip with a message.
        /// </summary>
        public void UpdateTooltipInfo(EntityUid player, EntityUid target, FormattedMessage message)
        {
            var chatMsg = new ChatMessage(ChatChannel.Emotes,
                message.ToString(),
                message.ToMarkup(),
                NetEntity.Invalid,
                null);

            // TODO: For now BaseTextureTag is broken with chat stack.
            // But also, for now i don't wanna use, so if i want use textures
            // then i should fix it, instead of ignoring stacks
            //chatMsg.IgnoreChatStack = true;

            _ui.GetUIController<ChatUIController>().ProcessChatMessage(chatMsg);
        }

        public void DoExamine(EntityUid entity, bool centeredOnCursor = true, EntityUid? userOverride = null)
        {
            var playerEnt = userOverride ?? _playerManager.LocalEntity;
            if (playerEnt == null)
                return;

            FormattedMessage message;

            // Basically this just predicts that we can't make out the entity if we have poor vision.
            var canSeeClearly = !HasComp<BlurryVisionComponent>(playerEnt);

            // Always update tooltip info from client first.
            // If we get it wrong, server will correct us later anyway.
            // This will usually be correct (barring server-only components, which generally only adds, not replaces text)
            //message = GetExamineText(entity, playerEnt);
            //UpdateTooltipInfo(playerEnt.Value, entity, message);

            if (!IsClientSide(entity))
            {
                // Ask server for extra examine info.
                if (entity != _lastExaminedEntity)
                    _idCounter += 1;
                if (_idCounter == int.MaxValue)
                    _idCounter = 0;
                RaiseNetworkEvent(new ExamineSystemMessages.RequestExamineInfoMessage(GetNetEntity(entity), _idCounter, true));
            }

            RaiseLocalEvent(entity, new ClientExaminedEvent(entity, playerEnt.Value));
            _lastExaminedEntity = entity;
        }
    }

    /// <summary>
    /// An entity was examined on the client.
    /// </summary>
    public sealed class ClientExaminedEvent : EntityEventArgs
    {
        /// <summary>
        ///     The entity performing the examining.
        /// </summary>
        public readonly EntityUid Examiner;

        /// <summary>
        ///     Entity being examined, for broadcast event purposes.
        /// </summary>
        public readonly EntityUid Examined;

        public ClientExaminedEvent(EntityUid examined, EntityUid examiner)
        {
            Examined = examined;
            Examiner = examiner;
        }
    }
}
