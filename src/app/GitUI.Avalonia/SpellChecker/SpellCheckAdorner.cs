using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.VisualTree;
using Color = Avalonia.Media.Color;
using Point = Avalonia.Point;

namespace GitUI.SpellChecker;

internal sealed class SpellCheckAdorner : Control
{
    public TextBox? TextBox { get; set; }

    public List<TextPos> IllFormedLines { get; } = [];

    public List<TextPos> MisspelledWords { get; } = [];

    public bool MarkFirstLineBlank { get; set; }

    internal int RenderedMisspellingCount { get; private set; }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        RenderedMisspellingCount = 0;

        if (!TryGetTextLayout(out TextPresenter? presenter, out TextLayout? layout, out Point origin))
        {
            return;
        }

        using (context.PushClip(new Rect(Bounds.Size)))
        {
            IBrush markBrush = new SolidColorBrush(Color.FromArgb(70, 255, 255, 0));
            foreach (TextPos range in IllFormedLines)
            {
                DrawRange(context, layout, origin, range, markBrush, drawWave: false);
            }

            if (MarkFirstLineBlank)
            {
                Rect firstPosition = layout.HitTestTextPosition(0);
                context.DrawRectangle(markBrush, null, new Rect(origin.X, origin.Y, Bounds.Width - origin.X, firstPosition.Height));
            }

            IPen spellingPen = new Pen(Brushes.Red, 1);
            foreach (TextPos range in MisspelledWords)
            {
                DrawRange(context, layout, origin, range, spellingPen.Brush!, drawWave: true);
                RenderedMisspellingCount++;
            }
        }
    }

    public int GetTextIndex(Point textBoxPoint)
    {
        TextBox? textBox = TextBox;
        if (textBox is null
            || !TryGetTextLayout(out TextPresenter? presenter, out TextLayout? layout, out _)
            || presenter.TranslatePoint(new Point(), textBox) is not Point presenterOrigin)
        {
            return TextBox?.CaretIndex ?? 0;
        }

        Point point = new(textBoxPoint.X - presenterOrigin.X, textBoxPoint.Y - presenterOrigin.Y);
        return Math.Clamp(layout.HitTestPoint(point).TextPosition, 0, TextBox?.Text?.Length ?? 0);
    }

    private static void DrawRange(
        DrawingContext context,
        TextLayout layout,
        Point origin,
        TextPos range,
        IBrush brush,
        bool drawWave)
    {
        int start = Math.Clamp(range.Start, 0, layout.TextLines.Sum(line => line.Length));
        int end = Math.Clamp(range.End, start, layout.TextLines.Sum(line => line.Length));
        if (start == end)
        {
            return;
        }

        foreach (TextLine line in layout.TextLines)
        {
            int lineStart = line.FirstTextSourceIndex;
            int lineEnd = lineStart + line.Length;
            int segmentStart = Math.Max(start, lineStart);
            int segmentEnd = Math.Min(end, lineEnd);
            if (segmentStart >= segmentEnd)
            {
                continue;
            }

            Rect startRect = layout.HitTestTextPosition(segmentStart);
            double endX = segmentEnd >= lineEnd
                ? line.WidthIncludingTrailingWhitespace
                : layout.HitTestTextPosition(segmentEnd).X;

            DrawSegment(
                context,
                origin.X + startRect.X,
                origin.X + endX,
                origin.Y + startRect.Bottom,
                startRect.Height,
                brush,
                drawWave);
        }
    }

    private static void DrawSegment(
        DrawingContext context,
        double start,
        double end,
        double baseline,
        double lineHeight,
        IBrush brush,
        bool drawWave)
    {
        if (end <= start)
        {
            return;
        }

        if (!drawWave)
        {
            context.DrawRectangle(brush, null, new Rect(start, baseline - lineHeight, end - start, lineHeight));
            return;
        }

        IPen pen = new Pen(brush, 1);
        double x = start;
        bool down = false;
        while (x < end)
        {
            double next = Math.Min(x + 2, end);
            context.DrawLine(pen, new Point(x, baseline + (down ? 1 : -1)), new Point(next, baseline + (down ? -1 : 1)));
            down = !down;
            x = next;
        }
    }

    private bool TryGetTextLayout(
        [NotNullWhen(true)] out TextPresenter? presenter,
        [NotNullWhen(true)] out TextLayout? layout,
        out Point origin)
    {
        presenter = TextBox?.GetVisualDescendants().OfType<TextPresenter>().FirstOrDefault();
        layout = presenter?.TextLayout;
        Point? translated = presenter?.TranslatePoint(new Point(), this);
        origin = translated ?? default;
        return presenter is not null && layout is not null && translated is not null;
    }
}
