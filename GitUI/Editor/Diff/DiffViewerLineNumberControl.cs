using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ICSharpCode.TextEditor;

namespace GitUI.Editor.Diff;

public class DiffViewerLineNumberControl : AbstractMargin
{
    private const int TextHorizontalMargin = 4;
    private static readonly IReadOnlyDictionary<int, DiffLineInfo> Empty = new Dictionary<int, DiffLineInfo>();
    private IReadOnlyDictionary<int, DiffLineInfo> _diffLines = Empty;
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
            if (_visible && _diffLines.Any())
            {
                // add a space behind each number
                int maxDigits = MaxLineNumber > 0 ? ((int)Math.Log10(MaxLineNumber) + 1) : 0;
                int length = (_showLeftColumn ? 2 : 1) * (1 + maxDigits);
                return TextHorizontalMargin + (textArea.TextView.WideSpaceWidth * length);
            }

            return 0;
        }
    }

    /// <summary>
    /// returns the according line numbers or null if the caretLine is not mapped.
    /// </summary>
    /// <param name="caretLine">0-based (in contrast to the displayed line numbers which are 1-based).</param>
    public DiffLineInfo GetLineInfo(int caretLine)
    {
        _diffLines.TryGetValue(caretLine + 1, out DiffLineInfo diffLine);
        return diffLine;
    }

    public override void Paint(Graphics g, Rectangle rect)
    {
        int numbersWidth = Width - TextHorizontalMargin;
        int leftWidth = _showLeftColumn ? TextHorizontalMargin + (numbersWidth / 2) : 0;
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

            if (curLine >= textArea.Document.TotalNumberOfLines)
            {
                continue;
            }

            if (!_diffLines.ContainsKey(curLine + 1))
            {
                continue;
            }

            DiffLineInfo diffLine = _diffLines[curLine + 1];
            if (diffLine.LineType != DiffLineType.Context)
            {
                using Brush brush = diffLine.LineType switch
                {
                    DiffLineType.Plus => new SolidBrush(AppColor.DiffAdded.GetThemeColor()),
                    DiffLineType.Minus => new SolidBrush(AppColor.DiffRemoved.GetThemeColor()),
                    DiffLineType.Header => new SolidBrush(AppColor.DiffSection.GetThemeColor()),
                    DiffLineType.Grep => new SolidBrush(AppColor.DiffRemoved.GetThemeColor()),
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
                    new Point(TextHorizontalMargin, backgroundRectangle.Top));
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

    public void Clear(bool forDiff)
    {
        _diffLines = Empty;
    }

    public void SetVisibility(bool visible)
    {
        _visible = visible;
    }
}
