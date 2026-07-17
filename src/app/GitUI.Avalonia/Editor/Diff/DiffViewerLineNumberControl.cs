using System.Globalization;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using MediaColor = Avalonia.Media.Color;

namespace GitUI.Editor.Diff;

public class DiffViewerLineNumberControl : AbstractMargin
{
    private const double TextHorizontalMargin = 4;
    private static readonly IReadOnlyDictionary<int, DiffLineInfo> _empty = new Dictionary<int, DiffLineInfo>();

    private readonly TextEditor _editor;
    private IReadOnlyDictionary<int, DiffLineInfo> _diffLines = _empty;
    private bool _showLeftColumn = true;

    public DiffViewerLineNumberControl(TextEditor editor)
    {
        _editor = editor;
    }

    /// <summary>
    /// Gets the maximum line number from either side of the diff.
    /// </summary>
    public int MaxLineNumber { get; private set; }

    public DiffLineInfo? GetLineInfo(int zeroBasedDocumentLine)
    {
        _diffLines.TryGetValue(zeroBasedDocumentLine + 1, out DiffLineInfo? info);
        return info;
    }

    public void DisplayLineNum(DiffLinesInfo result, bool showLeftColumn)
    {
        _diffLines = result.DiffLines;
        MaxLineNumber = result.MaxLineNumber;
        _showLeftColumn = showLeftColumn;
        IsVisible = true;
        InvalidateMeasure();
        InvalidateVisual();
    }

    public void Clear()
    {
        _diffLines = _empty;
        MaxLineNumber = 0;
        IsVisible = false;
        InvalidateMeasure();
        InvalidateVisual();
    }

    protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
    {
        if (!IsVisible || _diffLines.Count == 0)
        {
            return default;
        }

        int digits = MaxLineNumber > 0 ? ((int)Math.Log10(MaxLineNumber) + 1) : 1;
        double digitWidth = CreateFormattedText("0", bold: false, Brushes.Black).Width;
        int columnCount = _showLeftColumn ? 2 : 1;
        return new Avalonia.Size(TextHorizontalMargin + (columnCount * ((digitWidth * digits) + TextHorizontalMargin)), 0);
    }

    protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
    {
        if (oldTextView is not null)
        {
            oldTextView.VisualLinesChanged -= TextView_VisualLinesChanged;
        }

        base.OnTextViewChanged(oldTextView, newTextView);
        if (newTextView is not null)
        {
            newTextView.VisualLinesChanged += TextView_VisualLinesChanged;
        }
    }

    public override void Render(DrawingContext context)
    {
        TextView? textView = TextView;
        if (!IsVisible || textView is null || !textView.VisualLinesValid)
        {
            return;
        }

        IBrush background = GetBrush("GitExtensionsDiffLineNumberBackgroundBrush", Colors.Gainsboro);
        IBrush numberBrush = GetBrush("GitExtensionsDiffLineNumberBrush", Colors.DimGray);
        IBrush selectedBrush = GetBrush("GitExtensionsDiffLineNumberSelectedBrush", Colors.Black);
        context.FillRectangle(background, new Avalonia.Rect(Bounds.Size));

        double leftWidth = _showLeftColumn ? Bounds.Width / 2 : 0;
        foreach (VisualLine visualLine in textView.VisualLines)
        {
            int documentLine = visualLine.FirstDocumentLine.LineNumber;
            if (!_diffLines.TryGetValue(documentLine, out DiffLineInfo? info))
            {
                continue;
            }

            double y = visualLine.VisualTop - textView.ScrollOffset.Y;
            Avalonia.Rect row = new(0, y, Bounds.Width, visualLine.Height);
            DrawSemanticBackground(context, row, leftWidth, info);

            bool current = documentLine == _editor.TextArea.Caret.Line;
            IBrush textBrush = current ? selectedBrush : numberBrush;
            if (info.LeftLineNumber != DiffLineInfo.NotApplicableLineNum)
            {
                DrawNumber(context, info.LeftLineNumber, TextHorizontalMargin, y, current, textBrush);
            }

            if (info.RightLineNumber != DiffLineInfo.NotApplicableLineNum)
            {
                DrawNumber(context, info.RightLineNumber, leftWidth + TextHorizontalMargin, y, current, textBrush);
            }
        }
    }

    private void DrawSemanticBackground(DrawingContext context, Avalonia.Rect row, double leftWidth, DiffLineInfo info)
    {
        IBrush removed = GetBrush("GitExtensionsDiffRemovedBrush", MediaColor.Parse("#FFC8C8"));
        IBrush added = GetBrush("GitExtensionsDiffAddedBrush", MediaColor.Parse("#C8FFC8"));
        IBrush section = GetBrush("GitExtensionsDiffSectionBrush", MediaColor.Parse("#E6E6E6"));
        switch (info.LineType)
        {
            case DiffLineType.Header:
                context.FillRectangle(section, row);
                break;
            case DiffLineType.Minus:
            case DiffLineType.Grep:
                context.FillRectangle(removed, row);
                break;
            case DiffLineType.Plus:
                context.FillRectangle(added, row);
                break;
            case DiffLineType.MinusLeft:
                context.FillRectangle(removed, new Avalonia.Rect(row.X, row.Y, leftWidth, row.Height));
                break;
            case DiffLineType.PlusRight:
                context.FillRectangle(added, new Avalonia.Rect(leftWidth, row.Y, row.Width - leftWidth, row.Height));
                break;
            case DiffLineType.MinusPlus:
                context.FillRectangle(removed, new Avalonia.Rect(row.X, row.Y, leftWidth, row.Height));
                context.FillRectangle(added, new Avalonia.Rect(leftWidth, row.Y, row.Width - leftWidth, row.Height));
                break;
        }
    }

    private void DrawNumber(DrawingContext context, int number, double x, double y, bool bold, IBrush brush)
        => context.DrawText(CreateFormattedText(number.ToString(CultureInfo.CurrentCulture), bold, brush), new Avalonia.Point(x, y));

    private FormattedText CreateFormattedText(string text, bool bold, IBrush brush)
        => new(
            text,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            new Typeface(_editor.FontFamily, _editor.FontStyle, bold ? FontWeight.Bold : _editor.FontWeight),
            _editor.FontSize,
            brush);

    private IBrush GetBrush(string key, MediaColor fallback) => DiffBrushes.Get(this, key, fallback);

    private void TextView_VisualLinesChanged(object? sender, EventArgs e) => InvalidateVisual();
}
