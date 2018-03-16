using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using GitCommands;
using ICSharpCode.TextEditor;

namespace GitUI.Editor.Diff
{
    internal class DiffViewerLineNumberCtrl : AbstractMargin
    {
        private const int TextHorizontalMargin = 4;

        private int _maxValueOfLineNum;
        private bool _visible = true;
        private Size _lastSize = new Size(0, 0);

        public DiffViewerLineNumberCtrl(TextArea textArea) : base(textArea)
        {
            DiffLines = new Dictionary<int, DiffLineNum>();
        }

        public override Size Size
        {
            get
            {
                if (!_visible)
                {
                    _lastSize = new Size(0, 0);
                }
                else if (DiffLines.Any())
                {
                    var size = Graphics.FromHwnd(textArea.Handle).MeasureString(_maxValueOfLineNum.ToString(), textArea.Font);
                    _lastSize = new Size(((int)size.Width * 2) + TextHorizontalMargin, 0);
                }

                return _lastSize;
            }
        }

        public override void Paint(Graphics g, Rectangle rect)
        {
            var totalWidth = Size.Width;
            var leftWidth = (int)(totalWidth / 2.0);
            var rightWidth = rect.Width - leftWidth;

            var fontHeight = textArea.TextView.FontHeight;
            var lineNumberPainterColor = textArea.Document.HighlightingStrategy.GetColorFor("LineNumbers");
            var fillBrush = textArea.Enabled ? BrushRegistry.GetBrush(lineNumberPainterColor.BackgroundColor) : SystemBrushes.InactiveBorder;
            var drawBrush = BrushRegistry.GetBrush(lineNumberPainterColor.Color);

            for (var y = 0; y < ((DrawingPosition.Height + textArea.TextView.VisibleLineDrawingRemainder) / fontHeight) + 1; ++y)
            {
                var ypos = drawingPosition.Y + (fontHeight * y) - textArea.TextView.VisibleLineDrawingRemainder;
                var backgroundRectangle = new Rectangle(drawingPosition.X, ypos, drawingPosition.Width, fontHeight);
                if (!rect.IntersectsWith(backgroundRectangle))
                {
                    continue;
                }

                g.FillRectangle(fillBrush, backgroundRectangle);
                var curLine = textArea.Document.GetFirstLogicalLine(textArea.Document.GetVisibleLine(textArea.TextView.FirstVisibleLine) + y);

                if (curLine >= textArea.Document.TotalNumberOfLines)
                {
                    continue;
                }

                if (!DiffLines.ContainsKey(curLine + 1))
                {
                    continue;
                }

                var diffLine = DiffLines[curLine + 1];
                if (diffLine.Style != DiffLineNum.DiffLineStyle.Context)
                {
                    var brush = default(Brush);
                    switch (diffLine.Style)
                    {
                        case DiffLineNum.DiffLineStyle.Plus:
                            brush = new SolidBrush(AppSettings.DiffAddedColor);
                            break;
                        case DiffLineNum.DiffLineStyle.Minus:
                            brush = new SolidBrush(AppSettings.DiffRemovedColor);
                            break;
                        case DiffLineNum.DiffLineStyle.Header:
                            brush = new SolidBrush(AppSettings.DiffSectionColor);
                            break;
                    }

                    Debug.Assert(brush != null, string.Format("brush != null, unknow diff line style {0}", diffLine.Style));
                    g.FillRectangle(brush, new Rectangle(0, backgroundRectangle.Top, leftWidth, backgroundRectangle.Height));

                    g.FillRectangle(brush, new Rectangle(leftWidth, backgroundRectangle.Top, rightWidth, backgroundRectangle.Height));
                }

                if (diffLine.LeftLineNum != DiffLineNum.NotApplicableLineNum)
                {
                    g.DrawString(diffLine.LeftLineNum.ToString(),
                        lineNumberPainterColor.GetFont(TextEditorProperties.FontContainer),
                        drawBrush,
                        new Point(TextHorizontalMargin, backgroundRectangle.Top));
                }

                if (diffLine.RightLineNum != DiffLineNum.NotApplicableLineNum)
                {
                    g.DrawString(diffLine.RightLineNum.ToString(),
                        lineNumberPainterColor.GetFont(TextEditorProperties.FontContainer),
                        drawBrush,
                        new Point(TextHorizontalMargin + (totalWidth / 2), backgroundRectangle.Top));
                }
            }
        }

        private Dictionary<int, DiffLineNum> DiffLines { get; set; }

        public void DisplayLineNumFor(string diff)
        {
            var result = new DiffLineNumAnalyzer().Analyze(diff);
            DiffLines = result.LineNumbers;
            _maxValueOfLineNum = result.MaxLineNumber;
        }

        public void Clear(bool forDiff)
        {
            if (!forDiff)
            {
                _lastSize = new Size(0, 0);
            }

            DiffLines.Clear();
        }

        public void SetVisibility(bool visible)
        {
            _visible = visible;
        }
    }
}
