using System.Globalization;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using GitUI.Theming;
using MediaColor = Avalonia.Media.Color;

namespace GitUI.Editor.Diff;

public class DiffViewerLineNumberControl : AbstractMargin
{
    private const double _textHorizontalMargin = 4;
    private static readonly IReadOnlyDictionary<int, DiffLineInfo> _empty = new Dictionary<int, DiffLineInfo>();

    private readonly TextEditor _editor;
    private IReadOnlyDictionary<int, DiffLineInfo> _diffLines = _empty;
    private bool _visible = true;
    private bool _showLeftColumn = true;

    public DiffViewerLineNumberControl(TextEditor editor)
    {
        _editor = editor;
    }

    /// <summary>
    /// Gets the maximum line number from either left or right version.
    /// </summary>
    public int MaxLineNumber { get; private set; }

    /// <summary>
    /// returns the according line numbers or null if the caretLine is not mapped.
    /// </summary>
    /// <param name="caretLine">0-based (in contrast to the displayed line numbers which are 1-based).</param>
    public DiffLineInfo? GetLineInfo(int caretLine)
    {
        _diffLines.TryGetValue(caretLine + 1, out DiffLineInfo? info);
        return info;
    }

    public void DisplayLineNum(DiffLinesInfo result, bool showLeftColumn)
    {
        _diffLines = result.DiffLines;
        MaxLineNumber = result.MaxLineNumber;
        _showLeftColumn = showLeftColumn;
        InvalidateMeasure();
        InvalidateVisual();
    }

    public void Clear()
    {
        _diffLines = _empty;
        MaxLineNumber = 0;
        InvalidateMeasure();
        InvalidateVisual();
    }

    protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
    {
        if (!_visible || _diffLines.Count == 0)
        {
            return default;
        }

        int digits = MaxLineNumber > 0 ? ((int)Math.Log10(MaxLineNumber) + 1) : 1;
        double digitWidth = CreateFormattedText("0", bold: false, Brushes.Black).Width;

        // add a space behind each number
        int columnCount = _showLeftColumn ? 2 : 1;
        return new Avalonia.Size(_textHorizontalMargin + (columnCount * digitWidth * (digits + 1)), 0);
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
        if (!_visible || textView is null || !textView.VisualLinesValid)
        {
            return;
        }

        IBrush background = GetBrush("GitExtensionsDiffLineNumberBackgroundBrush", GetAppColor(AppColor.LineNumberBackground));
        IBrush numberBrush = GetBrush("GitExtensionsDiffLineNumberBrush", GetSystemColor(System.Drawing.KnownColor.GrayText));
        IBrush selectedBrush = GetBrush("GitExtensionsDiffLineNumberSelectedBrush", GetSystemColor(System.Drawing.KnownColor.WindowText));
        context.FillRectangle(background, new Avalonia.Rect(Bounds.Size));

        (double backgroundSplit, double rightNumberX) = _showLeftColumn
            ? GetTwoColumnGeometry(Bounds.Width)
            : (0, _textHorizontalMargin);
        foreach (VisualLine visualLine in textView.VisualLines)
        {
            int documentLine = visualLine.FirstDocumentLine.LineNumber;
            if (!_diffLines.TryGetValue(documentLine, out DiffLineInfo? info))
            {
                continue;
            }

            double y = visualLine.VisualTop - textView.ScrollOffset.Y;
            Avalonia.Rect row = new(0, y, Bounds.Width, visualLine.Height);
            DrawSemanticBackground(context, row, backgroundSplit, info);

            bool current = documentLine == _editor.TextArea.Caret.Line;
            IBrush textBrush = current ? selectedBrush : numberBrush;
            if (info.LeftLineNumber != DiffLineInfo.NotApplicableLineNum)
            {
                DrawNumber(context, info.LeftLineNumber, _textHorizontalMargin, y, current, textBrush);
            }

            if (info.RightLineNumber != DiffLineInfo.NotApplicableLineNum)
            {
                DrawNumber(context, info.RightLineNumber, rightNumberX, y, current, textBrush);
            }
        }
    }

    internal static (double BackgroundSplit, double RightNumberX) GetTwoColumnGeometry(double width)
    {
        double numbersWidth = width - _textHorizontalMargin;
        return (width / 2, _textHorizontalMargin + (numbersWidth / 2));
    }

    private void DrawSemanticBackground(DrawingContext context, Avalonia.Rect row, double leftWidth, DiffLineInfo info)
    {
        IBrush removed = GetBrush("GitExtensionsDiffRemovedBrush", GetAppColor(AppColor.AnsiTerminalRedBackNormal));
        IBrush added = GetBrush("GitExtensionsDiffAddedBrush", GetAppColor(AppColor.AnsiTerminalGreenBackNormal));
        IBrush section = GetBrush("GitExtensionsDiffSectionBrush", GetAppColor(AppColor.DiffSection));
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

    private static MediaColor GetAppColor(AppColor name)
        => AvaloniaThemeResources.ToMediaColor(AvaloniaThemeResources.ResolveAppColor(ThemeModule.Settings, name));

    private static MediaColor GetSystemColor(System.Drawing.KnownColor name)
        => AvaloniaThemeResources.ToMediaColor(AvaloniaThemeResources.ResolveSystemColor(ThemeModule.Settings, name));

    public void SetVisibility(bool visible)
    {
        _visible = visible;
        IsVisible = visible;
        InvalidateMeasure();
        InvalidateVisual();
    }

    private void TextView_VisualLinesChanged(object? sender, EventArgs e) => InvalidateVisual();
}
