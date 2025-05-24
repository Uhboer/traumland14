using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;

namespace Content.Client._ViewportGui.ViewportUserInterface.UI;

public class HUDLabel : HUDControl
{
    [Dependency] private readonly IResourceCache _cache = default!;

    private string _fontPath = "/Fonts/Bedstead/Bedstead.otf";
    private Font? _font;
    private string _text = string.Empty;

    /// <summary>
    /// Text's font scale.
    /// </summary>
    public int Scale { get; set; } = 8;

    public Vector2 TextPosition { get; set; } = Vector2.Zero;

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

    public Font? Font
    {
        get => _font;
        set
        {
            _font = value;
        }
    }

    /// <summary>
    /// Return label text
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
        }
    }

    public Color Color { get; set; } = Color.White;

    public HUDLabel()
    {
        IoCManager.InjectDependencies(this);

        _font = new VectorFont(_cache.GetResource<FontResource>(_fontPath), Scale);
    }

    public override void Draw(in ViewportUIDrawArgs args)
    {
        var handle = args.ScreenHandle;

        if (_font is null)
            return;

        handle.DrawString(_font,
            new Vector2(TextPosition.X, TextPosition.Y),
            _text,
            1f,
            Color);

        // Because next texts children
        base.Draw(args);
    }
}
