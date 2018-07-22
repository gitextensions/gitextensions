using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using GitCommands;
using ICSharpCode.TextEditor;

namespace GitUI.Editor.Diff
{
    internal class DiffViewerLineNumberControl : AbstractMargin
    {
        private const int TextHorizontalMargin = 4;

        private Dictionary<int, DiffLineNum> DiffLines { get; set; }

        private int _maxValueOfLineNum;
        private bool _visible = true;

        public DiffViewerLineNumberControl(TextArea textArea) : base(textArea)
        {
            DiffLines = new Dictionary<int, DiffLineNum>();
        }

        public override int Width
        {
            get
            {
                if (_visible && DiffLines.Any())
                {
                    return textArea.TextView.WideSpaceWidth * ((int)Math.Log10(_maxValueOfLineNum) + TextHorizontalMargin + 3);
                }

                return 0;
            }
        }

        public override void Paint(Graphics g, Rectangle rect)
        {
            var totalWidth = Width;
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

        public void DisplayLineNumFor(string diff)
        {
            var result = new DiffLineNumAnalyzer().Analyze(diff);
            DiffLines = result.LineNumbers;
            _maxValueOfLineNum = result.MaxLineNumber;
        }

        public void Clear(bool forDiff)
        {
            DiffLines.Clear();
        }

        public void SetVisibility(bool visible)
        {
            _visible = visible;
        }
    }
}
