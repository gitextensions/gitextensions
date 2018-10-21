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
        private static readonly IReadOnlyDictionary<int, DiffLineInfo> Empty = new Dictionary<int, DiffLineInfo>();
        private IReadOnlyDictionary<int, DiffLineInfo> _diffLines = Empty;
        private bool _visible = true;

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
                    return textArea.TextView.WideSpaceWidth * ((int)Math.Log10(MaxLineNumber) + TextHorizontalMargin + 3);
                }

                return 0;
            }
        }

        /// <summary>
        /// returns the according line numbers or null if the caretLine is not mapped
        /// </summary>
        /// <param name="caretLine">0-based (in contrast to the displayed line numbers which are 1-based)</param>
        public DiffLineInfo GetLineInfo(int caretLine)
        {
            DiffLineInfo diffLine;
            _diffLines.TryGetValue(caretLine + 1, out diffLine);
            return diffLine;
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

                if (!_diffLines.ContainsKey(curLine + 1))
                {
                    continue;
                }

                var diffLine = _diffLines[curLine + 1];
                if (diffLine.LineType != DiffLineType.Context)
                {
                    var brush = default(Brush);
                    switch (diffLine.LineType)
                    {
                        case DiffLineType.Context:
                            break;
                        case DiffLineType.Plus:
                            brush = new SolidBrush(AppSettings.DiffAddedColor);
                            break;
                        case DiffLineType.Minus:
                            brush = new SolidBrush(AppSettings.DiffRemovedColor);
                            break;
                        case DiffLineType.Header:
                            brush = new SolidBrush(AppSettings.DiffSectionColor);
                            break;
                    }

                    Debug.Assert(brush != null, string.Format("brush != null, unknow diff line style {0}", diffLine.LineType));
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
                        new Point(TextHorizontalMargin + (totalWidth / 2), backgroundRectangle.Top));
                }
            }
        }

        public void DisplayLineNumFor(string diff)
        {
            var result = new DiffLineNumAnalyzer().Analyze(diff);
            _diffLines = result.DiffLines;
            MaxLineNumber = result.MaxLineNumber;
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
}
