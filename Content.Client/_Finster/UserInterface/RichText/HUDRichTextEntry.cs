using System.Numerics;
using System.Text;
using Content.Client._Finster.UserInterface.Controls;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Collections;
using Robust.Shared.Utility;

namespace Content.Client._Finster.UserInterface.RichText;

internal struct HUDRichTextEntry
{
    private Color _defaultColor;
    private readonly Type[]? _tagsAllowed;

    public readonly FormattedMessage Message;

    /// <summary>
    ///     The vertical size of this entry, in pixels.
    /// </summary>
    public int Height;

    /// <summary>
    ///     The horizontal size of this entry, in pixels.
    /// </summary>
    public int Width;

    /// <summary>
    ///     The combined text indices in the message's text tags to put line breaks.
    /// </summary>
    public ValueList<int> LineBreaks;

    public Color DefaultColor
    {
        get => _defaultColor;
        set => _defaultColor = value;
    }

    public HUDRichTextEntry(FormattedMessage message, MarkupTagManager tagManager, Type[]? tagsAllowed = null, Color? defaultColor = null)
    {
        Message = message;
        Height = 0;
        Width = 0;
        LineBreaks = default;
        _defaultColor = defaultColor ?? new(200, 200, 200);
        _tagsAllowed = tagsAllowed;

        var nodeIndex = -1;
        foreach (var node in Message)
        {
            nodeIndex++;

            if (node.Name == null)
                continue;
        }
    }

    /// <summary>
    ///     Recalculate line dimensions and where it has line breaks for word wrapping.
    /// </summary>
    /// <param name="defaultFont">The font being used for display.</param>
    /// <param name="maxSizeX">The maximum horizontal size of the container of this entry.</param>
    /// <param name="uiScale"></param>
    /// <param name="lineHeightScale"></param>
    public void Update(MarkupTagManager tagManager, Font defaultFont, float maxSizeX = 120, float maxSizeY = 60, float uiScale = 1.0f, float lineHeightScale = 1)
    {
        // This method is gonna suck due to complexity.
        // Bear with me here.
        // I am so deeply sorry for the person adding stuff to this in the future.

        Height = defaultFont.GetHeight(uiScale);
        LineBreaks.Clear();

        int? breakLine;
        var wordWrap = new CustomWordWrap(maxSizeX);
        var context = new MarkupDrawingContext();
        context.Font.Push(defaultFont);
        context.Color.Push(_defaultColor);

        // Go over every node.
        // Nodes can change the markup drawing context and return additional text.
        // It's also possible for nodes to return inline controls. They get treated as one large rune.
        var nodeIndex = -1;
        foreach (var node in Message)
        {
            nodeIndex++;
            var text = ProcessNode(tagManager, node, context);

            if (!context.Font.TryPeek(out var font))
                font = defaultFont;

            // And go over every character.
            foreach (var rune in text.EnumerateRunes())
            {
                if (ProcessRune(ref this, rune, out breakLine))
                    continue;

                // Uh just skip unknown characters I guess.
                if (!font.TryGetCharMetrics(rune, uiScale, out var metrics))
                    continue;

                if (ProcessMetric(ref this, metrics, out breakLine))
                    return;
            }

            var desiredSize = new Vector2i((int) maxSizeX, (int) maxSizeY);
            var controlMetrics = new CharMetrics(
                0, 0,
                desiredSize.X,
                desiredSize.X,
                desiredSize.Y);

            if (ProcessMetric(ref this, controlMetrics, out breakLine))
                return;
        }

        Width = wordWrap.FinalizeText(out breakLine);
        CheckLineBreak(ref this, breakLine);

        bool ProcessRune(ref HUDRichTextEntry src, Rune rune, out int? outBreakLine)
        {
            wordWrap.NextRune(rune, out breakLine, out var breakNewLine, out var skip);
            CheckLineBreak(ref src, breakLine);
            CheckLineBreak(ref src, breakNewLine);
            outBreakLine = breakLine;
            return skip;
        }

        bool ProcessMetric(ref HUDRichTextEntry src, CharMetrics metrics, out int? outBreakLine)
        {
            wordWrap.NextMetrics(metrics, out breakLine, out var abort);
            CheckLineBreak(ref src, breakLine);
            outBreakLine = breakLine;
            return abort;
        }

        void CheckLineBreak(ref HUDRichTextEntry src, int? line)
        {
            if (line is { } l)
            {
                src.LineBreaks.Add(l);
                if (!context.Font.TryPeek(out var font))
                    font = defaultFont;

                src.Height += GetLineHeight(font, uiScale, lineHeightScale);
            }
        }
    }

    public readonly void Draw(
        MarkupTagManager tagManager,
        DrawingHandleBase handle,
        Font defaultFont,
        UIBox2 drawBox,
        float verticalOffset,
        MarkupDrawingContext context,
        float uiScale = 1.0f,
        float lineHeightScale = 1)
    {
        context.Clear();
        context.Color.Push(_defaultColor);
        context.Font.Push(defaultFont);

        var globalBreakCounter = 0;
        var lineBreakIndex = 0;
        var baseLine = drawBox.TopLeft + new Vector2(0, defaultFont.GetAscent(uiScale) + verticalOffset);
        var controlYAdvance = 0f;

        var nodeIndex = -1;
        foreach (var node in Message)
        {
            nodeIndex++;
            var text = ProcessNode(tagManager, node, context);
            if (!context.Color.TryPeek(out var color) || !context.Font.TryPeek(out var font))
            {
                color = _defaultColor;
                font = defaultFont;
            }

            foreach (var rune in text.EnumerateRunes())
            {
                if (lineBreakIndex < LineBreaks.Count &&
                    LineBreaks[lineBreakIndex] == globalBreakCounter)
                {
                    baseLine = new Vector2(drawBox.Left, baseLine.Y + GetLineHeight(font, uiScale, lineHeightScale) + controlYAdvance);
                    controlYAdvance = 0;
                    lineBreakIndex += 1;
                }

                var advance = font.DrawChar(handle, rune, baseLine, uiScale, color);
                baseLine += new Vector2(advance, 0);

                globalBreakCounter += 1;
            }
        }
    }

    private readonly string ProcessNode(MarkupTagManager tagManager, MarkupNode node, MarkupDrawingContext context)
    {
        // If a nodes name is null it's a text node.
        if (node.Name == null)
            return node.Value.StringValue ?? "";

        //Skip the node if there is no markup tag for it.
        if (!tagManager.TryGetMarkupTag(node.Name, _tagsAllowed, out var tag))
            return "";

        if (!node.Closing)
        {
            tag.PushDrawContext(node, context);
            return tag.TextBefore(node);
        }

        tag.PopDrawContext(node, context);
        return tag.TextAfter(node);
    }

    private static int GetLineHeight(Font font, float uiScale, float lineHeightScale)
    {
        var height = font.GetLineHeight(uiScale);
        return (int)(height * lineHeightScale);
    }
}
