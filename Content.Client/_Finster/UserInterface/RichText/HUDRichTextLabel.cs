using System.Numerics;
using Content.Client._Finster.UserInterface.Controls;
using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Utility;

namespace Content.Client._Finster.UserInterface.RichText;

public class HUDRichTextLabel : HUDControl
{
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly MarkupTagManager _tagManager = default!;

    private string _fontPath = "/Fonts/Bedstead/Bedstead.otf";
    private Font _font;

    private FormattedMessage? _message;
    private HUDRichTextEntry _entry;
    private float _lineHeightScale = 1;
    private bool _lineHeightOverride;

    public Color DefaultColor
    {
        get => _entry.DefaultColor;
        set => _entry.DefaultColor = value;
    }

    public string? Text
    {
        get => _message?.ToMarkup();
        set
        {
            if (value == null)
            {
                _message?.Clear();
                return;
            }

            SetMessage(FormattedMessage.FromMarkupPermissive(value));
        }
    }

    /// <summary>
    /// Text's font scale.
    /// </summary>
    public int Scale { get; set; } = 6;

    /// <summary>
    /// Return current font path or set a new font with the path.
    /// </summary>
    public string FontPath
    {
        get => _fontPath;
        set
        {
            _fontPath = value;
            _font = new VectorFont(_cache.GetResource<FontResource>(_fontPath), Scale);
        }
    }

    public HUDRichTextLabel()
    {
        IoCManager.InjectDependencies(this);

        Size = (120, 60);

        _font = new VectorFont(_cache.GetResource<FontResource>(_fontPath), Scale);
    }

    public void SetMessage(FormattedMessage message, Type[]? tagsAllowed = null, Color? defaultColor = null)
    {
        _message = message;
        _entry = new HUDRichTextEntry(_message, _tagManager, tagsAllowed, defaultColor);
        _entry.Update(_tagManager, _font, Size.X, Size.Y);
    }

    public void SetMessage(string message, Type[]? tagsAllowed = null, Color? defaultColor = null)
    {
        var msg = new FormattedMessage();
        msg.AddText(message);
        SetMessage(msg, tagsAllowed, defaultColor);
    }

    public string? GetMessage() => _message?.ToMarkup();

    public override void Draw(in ViewportUIDrawArgs args)
    {
        var handle = args.ScreenHandle;

        base.Draw(args);

        if (_message == null)
        {
            return;
        }

        _entry.Draw(
            _tagManager,
            handle,
            _font,
            new UIBox2(GlobalPosition, GlobalPosition + Size),
            0, new MarkupDrawingContext());
    }
}
