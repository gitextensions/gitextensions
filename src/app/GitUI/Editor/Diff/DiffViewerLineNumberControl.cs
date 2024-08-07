using GitExtensions.Extensibility;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ICSharpCode.TextEditor;

namespace GitUI.Editor.Diff;

public class DiffViewerLineNumberControl : AbstractMargin
{
    private const int _textHorizontalMargin = 4;
    private static readonly IReadOnlyDictionary<int, DiffLineInfo> _empty = new Dictionary<int, DiffLineInfo>();
    private IReadOnlyDictionary<int, DiffLineInfo> _diffLines = _empty;
    private bool _visible = true;
    private bool _showLeftColumn = true;

    public DiffViewerLineNumberControl(TextArea textArea)
        : base(textArea)
    {
    }

    /// <summary>
    /// Gets the maximum line number from either left or right version.
    /// </summary>
    public int MaxLineNumber { get; private set; }

    public override int Width
    {
        get
        {
            if (_visible && _diffLines.Count != 0)
            {
                // add a space behind each number
                int maxDigits = MaxLineNumber > 0 ? ((int)Math.Log10(MaxLineNumber) + 1) : 0;
                int length = (_showLeftColumn ? 2 : 1) * (1 + maxDigits);
                return _textHorizontalMargin + (textArea.TextView.WideSpaceWidth * length);
            }

            return 0;
        }
    }

    /// <summary>
    /// returns the according line numbers or null if the caretLine is not mapped.
    /// </summary>
    /// <param name="caretLine">0-based (in contrast to the displayed line numbers which are 1-based).</param>
    public DiffLineInfo? GetLineInfo(int caretLine)
    {
        _diffLines.TryGetValue(caretLine + 1, out DiffLineInfo? diffLine);
        return diffLine;
    }

    public override void Paint(Graphics g, Rectangle rect)
    {
        int numbersWidth = Width - _textHorizontalMargin;
        int leftWidth = _showLeftColumn ? _textHorizontalMargin + (numbersWidth / 2) : 0;
        int rightWidth = rect.Width - leftWidth;

        int fontHeight = textArea.TextView.FontHeight;
        ICSharpCode.TextEditor.Document.HighlightColor lineNumberPainterColor = textArea.Document.HighlightingStrategy.GetColorFor("LineNumbers");
        Brush fillBrush = textArea.Enabled ? BrushRegistry.GetBrush(lineNumberPainterColor.BackgroundColor) : SystemBrushes.InactiveBorder;
        Brush drawBrush = BrushRegistry.GetBrush(lineNumberPainterColor.Color);

        for (int y = 0; y < ((DrawingPosition.Height + textArea.TextView.VisibleLineDrawingRemainder) / fontHeight) + 1; ++y)
        {
            int ypos = drawingPosition.Y + (fontHeight * y) - textArea.TextView.VisibleLineDrawingRemainder;
            Rectangle backgroundRectangle = new(drawingPosition.X, ypos, drawingPosition.Width, fontHeight);
            if (!rect.IntersectsWith(backgroundRectangle))
            {
                continue;
            }

            g.FillRectangle(fillBrush, backgroundRectangle);
            int curLine = textArea.Document.GetFirstLogicalLine(textArea.Document.GetVisibleLine(textArea.TextView.FirstVisibleLine) + y);

            if (curLine >= textArea.Document.TotalNumberOfLines
                || !_diffLines.TryGetValue(curLine + 1, out DiffLineInfo? diffLine))
            {
                continue;
            }

            if (diffLine.LineType is DiffLineType.MinusPlus or DiffLineType.MinusLeft or DiffLineType.PlusRight)
            {
                if (diffLine.LineType is not DiffLineType.PlusRight)
                {
                    using Brush leftBrush = new SolidBrush(AppColor.AnsiTerminalRedBackNormal.GetThemeColor());
                    g.FillRectangle(leftBrush, new Rectangle(0, backgroundRectangle.Top, backgroundRectangle.Width / 2, backgroundRectangle.Height));
                }

                if (diffLine.LineType is not DiffLineType.MinusLeft)
                {
                    using Brush rightBrush = new SolidBrush(AppColor.AnsiTerminalGreenBackNormal.GetThemeColor());
                    g.FillRectangle(rightBrush, new Rectangle(backgroundRectangle.Width / 2, backgroundRectangle.Top, rightWidth, backgroundRectangle.Height));
                }
            }
            else if (diffLine.LineType != DiffLineType.Context)
            {
                using Brush brush = diffLine.LineType switch
                {
                    DiffLineType.Plus => new SolidBrush(AppColor.AnsiTerminalGreenBackNormal.GetThemeColor()),
                    DiffLineType.Minus => new SolidBrush(AppColor.AnsiTerminalRedBackNormal.GetThemeColor()),
                    DiffLineType.Header => new SolidBrush(AppColor.DiffSection.GetThemeColor()),
                    DiffLineType.Grep => new SolidBrush(AppColor.AnsiTerminalRedBackNormal.GetThemeColor()),
                    _ => default(Brush)
                };

                DebugHelpers.Assert(brush is not null, string.Format("brush is not null, unknow diff line style {0}", diffLine.LineType));
                g.FillRectangle(brush, new Rectangle(0, backgroundRectangle.Top, leftWidth, backgroundRectangle.Height));
                g.FillRectangle(brush, new Rectangle(leftWidth, backgroundRectangle.Top, rightWidth, backgroundRectangle.Height));
            }

            if (diffLine.LeftLineNumber != DiffLineInfo.NotApplicableLineNum)
            {
                g.DrawString(diffLine.LeftLineNumber.ToString(),
                    lineNumberPainterColor.GetFont(TextEditorProperties.FontContainer),
                    drawBrush,
                    new Point(_textHorizontalMargin, backgroundRectangle.Top));
            }

            if (diffLine.RightLineNumber != DiffLineInfo.NotApplicableLineNum)
            {
                g.DrawString(diffLine.RightLineNumber.ToString(),
                    lineNumberPainterColor.GetFont(TextEditorProperties.FontContainer),
                    drawBrush,
                    new Point(leftWidth, backgroundRectangle.Top));
            }
        }
    }

    public void DisplayLineNum(DiffLinesInfo result, bool showLeftColumn)
    {
        _diffLines = result.DiffLines;
        MaxLineNumber = result.MaxLineNumber;
        _showLeftColumn = showLeftColumn;
    }

    public void Clear()
    {
        _diffLines = _empty;
        MaxLineNumber = 0;
    }

    public void SetVisibility(bool visible)
    {
        _visible = visible;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly DiffViewerLineNumberControl _diffViewerLineNumberControl;

        public TestAccessor(DiffViewerLineNumberControl diffViewerLineNumberControl)
        {
            _diffViewerLineNumberControl = diffViewerLineNumberControl;
        }

        public DiffLinesInfo Result
        {
            get
            {
                DiffLinesInfo diffLinesInfo = new();
                foreach (KeyValuePair<int, DiffLineInfo> kvp in _diffViewerLineNumberControl._diffLines)
                {
                    diffLinesInfo.Add(kvp.Value);
                }

                return diffLinesInfo;
            }
        }
    }
}
