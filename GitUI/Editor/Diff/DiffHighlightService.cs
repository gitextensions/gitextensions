using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff
{
    public class DiffHighlightService : TextHighlightService
    {
        // Patterns to check for patches in diff files
        private static readonly string[] _diffFullPrefixes = { " ", "+", "-" };
        private static readonly string[] _diffSearchPrefixes = { "+", "-" };

        public static new DiffHighlightService Instance { get; } = new();

        protected readonly LinePrefixHelper LinePrefixHelper = new(new LineSegmentGetter());

        protected DiffHighlightService()
        {
        }

        protected virtual int GetDiffContentOffset()
        {
            return 1;
        }

        public virtual string[] GetFullDiffPrefixes()
        {
            return _diffFullPrefixes;
        }

        public virtual bool IsSearchMatch(string line)
        {
            return line.StartsWithAny(_diffSearchPrefixes);
        }

        private static void MarkDifference(IDocument document, List<ISegment> linesRemoved, List<ISegment> linesAdded, int beginOffset)
        {
            int count = Math.Min(linesRemoved.Count, linesAdded.Count);

            for (int i = 0; i < count; i++)
            {
                MarkDifference(document, linesRemoved[i], linesAdded[i], beginOffset);
            }
        }

        private static void MarkDifference(IDocument document, ISegment lineRemoved,
            ISegment lineAdded, int beginOffset)
        {
            int lineRemovedEndOffset = lineRemoved.Length;
            int lineAddedEndOffset = lineAdded.Length;
            int endOffsetMin = Math.Min(lineRemovedEndOffset, lineAddedEndOffset);
            int reverseOffset = 0;

            while (beginOffset < endOffsetMin)
            {
                char a = document.GetCharAt(lineAdded.Offset + beginOffset);
                char r = document.GetCharAt(lineRemoved.Offset + beginOffset);

                if (a != r)
                {
                    break;
                }

                beginOffset++;
            }

            while (lineAddedEndOffset > beginOffset && lineRemovedEndOffset > beginOffset)
            {
                reverseOffset = lineAdded.Length - lineAddedEndOffset;

                char a = document.GetCharAt(lineAdded.Offset + lineAdded.Length - 1 - reverseOffset);
                char r = document.GetCharAt(lineRemoved.Offset + lineRemoved.Length - 1 - reverseOffset);

                if (a != r)
                {
                    break;
                }

                lineRemovedEndOffset--;
                lineAddedEndOffset--;
            }

            Color color;
            MarkerStrategy markerStrategy = document.MarkerStrategy;

            if (lineAdded.Length - beginOffset - reverseOffset > 0)
            {
                color = AppColor.DiffAddedExtra.GetThemeColor();
                markerStrategy.AddMarker(new TextMarker(lineAdded.Offset + beginOffset,
                                                        lineAdded.Length - beginOffset - reverseOffset,
                                                        TextMarkerType.SolidBlock, color,
                                                        ColorHelper.GetForeColorForBackColor(color)));
            }

            if (lineRemoved.Length - beginOffset - reverseOffset > 0)
            {
                color = AppColor.DiffRemovedExtra.GetThemeColor();
                markerStrategy.AddMarker(new TextMarker(lineRemoved.Offset + beginOffset,
                                                        lineRemoved.Length - beginOffset - reverseOffset,
                                                        TextMarkerType.SolidBlock, color,
                                                        ColorHelper.GetForeColorForBackColor(color)));
            }
        }

        private void AddExtraPatchHighlighting(IDocument document)
        {
            int line = 0;

            bool found = false;
            int diffContentOffset;
            List<ISegment> linesRemoved = GetRemovedLines(document, ref line, ref found);
            List<ISegment> linesAdded = GetAddedLines(document, ref line, ref found);
            if (linesAdded.Count == 1 && linesRemoved.Count == 1)
            {
                ISegment lineA = linesRemoved[0];
                ISegment lineB = linesAdded[0];
                if (lineA.Length > 4 && lineB.Length > 4 &&
                    document.GetCharAt(lineA.Offset + 4) == 'a' &&
                    document.GetCharAt(lineB.Offset + 4) == 'b')
                {
                    diffContentOffset = 5;
                }
                else
                {
                    diffContentOffset = 4;
                }

                MarkDifference(document, linesRemoved, linesAdded, diffContentOffset);
            }

            diffContentOffset = GetDiffContentOffset();
            while (line < document.TotalNumberOfLines)
            {
                found = false;
                linesRemoved = GetRemovedLines(document, ref line, ref found);
                linesAdded = GetAddedLines(document, ref line, ref found);

                MarkDifference(document, linesRemoved, linesAdded, diffContentOffset);
            }
        }

        protected virtual List<ISegment> GetAddedLines(IDocument document, ref int line, ref bool found)
        {
            return LinePrefixHelper.GetLinesStartingWith(document, ref line, "+", ref found);
        }

        protected virtual List<ISegment> GetRemovedLines(IDocument document, ref int line, ref bool found)
        {
            return LinePrefixHelper.GetLinesStartingWith(document, ref line, "-", ref found);
        }

        protected void ProcessLineSegment(IDocument document, ref int line,
            LineSegment lineSegment, string prefixStr, Color color, bool invertMatch = false)
        {
            if (!DoesLineStartWith(document, lineSegment.Offset, prefixStr, invertMatch))
            {
                return;
            }

            LineSegment endLine = document.GetLineSegment(line);

            for (;
                line < document.TotalNumberOfLines
                && DoesLineStartWith(document, endLine.Offset, prefixStr, invertMatch);
                line++)
            {
                endLine = document.GetLineSegment(line);
            }

            line = Math.Max(0, line - 2);
            endLine = document.GetLineSegment(line);

            document.MarkerStrategy.AddMarker(new TextMarker(lineSegment.Offset,
                (endLine.Offset + endLine.TotalLength) -
                lineSegment.Offset, TextMarkerType.SolidBlock, color,
                ColorHelper.GetForeColorForBackColor(color)));

            return;

            bool DoesLineStartWith(IDocument document, int offset, string prefixStr, bool invertMatch)
                => invertMatch ^ LinePrefixHelper.DoesLineStartWith(document, offset, prefixStr);
        }

        public override void AddTextHighlighting(IDocument document)
        {
            bool forceAbort = false;

            AddExtraPatchHighlighting(document);

            for (int line = 0; line < document.TotalNumberOfLines && !forceAbort; line++)
            {
                LineSegment lineSegment = document.GetLineSegment(line);

                if (lineSegment.TotalLength == 0)
                {
                    continue;
                }

                if (line == document.TotalNumberOfLines - 1)
                {
                    forceAbort = true;
                }

                line = TryHighlightAddedAndDeletedLines(document, line, lineSegment);

                ProcessLineSegment(document, ref line, lineSegment, "@", AppColor.DiffSection.GetThemeColor());
                ProcessLineSegment(document, ref line, lineSegment, "\\", AppColor.DiffSection.GetThemeColor());
            }
        }

        protected virtual int TryHighlightAddedAndDeletedLines(IDocument document, int line, LineSegment lineSegment)
        {
            ProcessLineSegment(document, ref line, lineSegment, "+", AppColor.DiffAdded.GetThemeColor());
            ProcessLineSegment(document, ref line, lineSegment, "-", AppColor.DiffRemoved.GetThemeColor());
            return line;
        }
    }
}
