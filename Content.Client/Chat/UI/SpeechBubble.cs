using System.Numerics;
using Content.Client._Finster.UserInterface.RichText;
using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Client.Chat.Managers;
using Content.Client.Viewport;
using Content.KayMisaZlevels.Client;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Speech;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.Chat.UI
{
    public abstract class SpeechBubble : HUDControl
    {
        [Dependency] private readonly IEyeManager _eyeManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] protected readonly IConfigurationManager ConfigManager = default!;
        [Dependency] protected readonly IUserInterfaceManager UiManager = default!;
        [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!;

        private readonly SharedTransformSystem _transformSystem;

        public enum SpeechType : byte
        {
            Emote,
            Say,
            Whisper,
            Looc
        }

        /// <summary>
        ///     The total time a speech bubble stays on screen.
        /// </summary>
        private const float TotalTime = 4;

        /// <summary>
        ///     The amount of time at the end of the bubble's life at which it starts fading.
        /// </summary>
        private const float FadeTime = 0.25f;

        /// <summary>
        ///     The distance in world space to offset the speech bubble from the center of the entity.
        ///     i.e. greater -> higher above the mob's head.
        /// </summary>
        private const float EntityVerticalOffset = 0.5f;

        /// <summary>
        ///     The default maximum width for speech bubbles.
        /// </summary>
        public const float SpeechMaxWidth = 256;

        private readonly EntityUid _senderEntity;

        private float _timeLeft = TotalTime;

        public float VerticalOffset { get; set; }
        private float _verticalOffsetAchieved;

        private Vector2 _worldPos = Vector2.Zero;

        public Vector2 ContentSize { get; private set; }
        public readonly HUDRichTextLabel BubbleControl;

        // man down
        public event Action<EntityUid, SpeechBubble>? OnDied;

        public static SpeechBubble CreateSpeechBubble(SpeechType type, ChatMessage message, EntityUid senderEntity)
        {
            switch (type)
            {
                case SpeechType.Emote:
                    return new TextSpeechBubble(message, senderEntity, "emoteBox");

                case SpeechType.Say:
                    return new FancyTextSpeechBubble(message, senderEntity, "sayBox");

                case SpeechType.Whisper:
                    return new FancyTextSpeechBubble(message, senderEntity, "whisperBox");

                case SpeechType.Looc:
                    return new TextSpeechBubble(message, senderEntity, "emoteBox", Color.FromHex("#48d1cc"));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public SpeechBubble(ChatMessage message, EntityUid senderEntity, string speechStyleClass, Color? fontColor = null)
        {
            IoCManager.InjectDependencies(this);
            _senderEntity = senderEntity;
            _transformSystem = _entityManager.System<SharedTransformSystem>();

            BubbleControl = BuildBubble(message, speechStyleClass, fontColor);
            AddChild(BubbleControl);
            ContentSize = BubbleControl.Size;
            _verticalOffsetAchieved = -ContentSize.Y;
        }

        protected abstract HUDRichTextLabel BuildBubble(ChatMessage message, string speechStyleClass, Color? fontColor = null);

        public override void FrameUpdate(FrameEventArgs args)
        {
            base.FrameUpdate(args);

            _timeLeft -= args.DeltaSeconds;
            if (_entityManager.Deleted(_senderEntity) || _timeLeft <= 0)
            {
                // Timer spawn to prevent concurrent modification exception.
                Timer.Spawn(0, Die);
                return;
            }

            // Lerp to our new vertical offset if it's been modified.
            if (MathHelper.CloseToPercent(_verticalOffsetAchieved - VerticalOffset, 0, 0.1))
            {
                _verticalOffsetAchieved = VerticalOffset;
            }
            else
            {
                _verticalOffsetAchieved = MathHelper.Lerp(_verticalOffsetAchieved, VerticalOffset, 10 * args.DeltaSeconds);
            }

            if (!_entityManager.TryGetComponent<TransformComponent>(_senderEntity, out var xform) || xform.MapID != _eyeManager.CurrentMap)
            {
                BubbleControl.DefaultColor = BubbleControl.DefaultColor.WithAlpha(0);
                return;
            }

            if (_timeLeft <= FadeTime)
            {
                // Update alpha if we're fading.
                BubbleControl.DefaultColor = BubbleControl.DefaultColor.WithAlpha(_timeLeft / FadeTime);
            }
            /*
            else
            {
                // Make opaque otherwise, because it might have been hidden before
                BubbleControl.DefaultColor = Color.White;
            }
            */

            var baseOffset = 0f;

            if (_entityManager.TryGetComponent<SpeechComponent>(_senderEntity, out var speech))
                baseOffset = speech.SpeechBubbleOffset;

            var offset = (-_eyeManager.CurrentEye.Rotation).ToWorldVec() * -(EntityVerticalOffset + baseOffset);
            _worldPos = _transformSystem.GetWorldPosition(xform) + offset;

            var screenPos = _eyeManager.WorldToScreen(_worldPos);
            var localPos = _vpUIManager.ConvertGlobalToLocal(screenPos);
            if (localPos is null)
                return;

            Position = localPos.Value - (BubbleControl.Size / 2);

            /*

            var lowerCenter = _eyeManager.WorldToScreen(worldPos) / UIScale;
            if (_eyeManager.MainViewport is not ScalingViewport svp)
                return;

            var screenPos = lowerCenter - new Vector2(ContentSize.X / 2, ContentSize.Y + _verticalOffsetAchieved);
            // Round to nearest 0.5
            screenPos = (screenPos * 2).Rounded() / 2;
            LayoutContainer.SetPosition(this, screenPos - svp.GlobalPosition);

            var height = MathF.Ceiling(MathHelper.Clamp(lowerCenter.Y - screenPos.Y, 0, ContentSize.Y));
            SetHeight = height;
            */
        }

        public override void Draw(in ViewportUIDrawArgs args)
        {
            base.Draw(args);
        }

        private void Die()
        {
            if (Disposed)
            {
                return;
            }

            OnDied?.Invoke(_senderEntity, this);
        }

        /// <summary>
        ///     Causes the speech bubble to start fading IMMEDIATELY.
        /// </summary>
        public void FadeNow()
        {
            if (_timeLeft > FadeTime)
            {
                _timeLeft = FadeTime;
            }
        }

        protected FormattedMessage FormatSpeech(string message, Color? fontColor = null)
        {
            var msg = new FormattedMessage();
            if (fontColor != null)
                msg.PushColor(fontColor.Value);
            msg.AddMarkup(message);
            return msg;
        }

        protected FormattedMessage ExtractAndFormatSpeechSubstring(ChatMessage message, string tag, Color? fontColor = null)
        {
            return FormatSpeech(SharedChatSystem.GetStringInsideTag(message, tag), fontColor);
        }

    }

    public sealed class TextSpeechBubble : SpeechBubble
    {
        public TextSpeechBubble(ChatMessage message, EntityUid senderEntity, string speechStyleClass, Color? fontColor = null)
            : base(message, senderEntity, speechStyleClass, fontColor)
        {
        }

        protected override HUDRichTextLabel BuildBubble(ChatMessage message, string speechStyleClass, Color? fontColor = null)
        {
            var label = new HUDRichTextLabel();
            /*
            {
                MaxWidth = SpeechMaxWidth,
            };
            */

            label.SetMessage(FormatSpeech(message.WrappedMessage, fontColor));

            return label;
        }
    }

    public sealed class FancyTextSpeechBubble : SpeechBubble
    {

        public FancyTextSpeechBubble(ChatMessage message, EntityUid senderEntity, string speechStyleClass, Color? fontColor = null)
            : base(message, senderEntity, speechStyleClass, fontColor)
        {
        }

        protected override HUDRichTextLabel BuildBubble(ChatMessage message, string speechStyleClass, Color? fontColor = null)
        {
            if (!ConfigManager.GetCVar(CCVars.ChatEnableFancyBubbles))
            {
                var label = new HUDRichTextLabel();
                /*
                {
                    MaxWidth = SpeechMaxWidth
                };
                */

                label.SetMessage(ExtractAndFormatSpeechSubstring(message, "BubbleContent", fontColor));
                return label;
            }

            var bubbleContent = new HUDRichTextLabel();
            bubbleContent.SetMessage(ExtractAndFormatSpeechSubstring(message, "BubbleContent", fontColor));

            return bubbleContent;
        }
    }
}
