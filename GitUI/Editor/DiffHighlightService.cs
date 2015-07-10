using System;
using System.Collections.Generic;
using System.Drawing;
using GitCommands;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor
{
    public class DiffHighlightService
    {
        private static List<LineSegment> GetLinesStartingWith(IDocument document, ref int beginIndex, char startingChar, ref bool found)
        {
            List<LineSegment> result = new List<LineSegment>();

            while (beginIndex < document.TotalNumberOfLines)
            {
                var lineSegment = document.GetLineSegment(beginIndex);

                if (lineSegment.Length > 0 && document.GetCharAt(lineSegment.Offset) == startingChar)
                {
                    found = true;
                    result.Add(lineSegment);
                    beginIndex++;
                }
                else
                {
                    if (found)
                        break;
                    else
                        beginIndex++;
                }
            }

            return result;
        }

        private void MarkDifference(IDocument document, List<LineSegment> linesRemoved, List<LineSegment> linesAdded, int beginOffset)
        {
            int count = Math.Min(linesRemoved.Count, linesAdded.Count);

            for (int i = 0; i < count; i++)
                MarkDifference(document, linesRemoved[i], linesAdded[i], beginOffset);
        }

        private void MarkDifference(IDocument document, LineSegment lineRemoved, LineSegment lineAdded, int beginOffset)
        {
            var lineRemovedEndOffset = lineRemoved.Length;
            var lineAddedEndOffset = lineAdded.Length;
            var endOffsetMin = Math.Min(lineRemovedEndOffset, lineAddedEndOffset);
            var reverseOffset = 0;

            while (beginOffset < endOffsetMin)
            {
                if (!document.GetCharAt(lineAdded.Offset + beginOffset).Equals(
                        document.GetCharAt(lineRemoved.Offset + beginOffset)))
                    break;

                beginOffset++;
            }

            while (lineAddedEndOffset > beginOffset && lineRemovedEndOffset > beginOffset)
            {
                reverseOffset = lineAdded.Length - lineAddedEndOffset;

                if (!document.GetCharAt(lineAdded.Offset + lineAdded.Length - 1 - reverseOffset).
                         Equals(document.GetCharAt(lineRemoved.Offset + lineRemoved.Length - 1 -
                                                   reverseOffset)))
                    break;

                lineRemovedEndOffset--;
                lineAddedEndOffset--;
            }

            Color color;
            var markerStrategy = document.MarkerStrategy;

            if (lineAdded.Length - beginOffset - reverseOffset > 0)
            {
                color = AppSettings.DiffAddedExtraColor;
                markerStrategy.AddMarker(new TextMarker(lineAdded.Offset + beginOffset,
                                                        lineAdded.Length - beginOffset - reverseOffset,
                                                        TextMarkerType.SolidBlock, color,
                                                        ColorHelper.GetForeColorForBackColor(color)));
            }

            if (lineRemoved.Length - beginOffset - reverseOffset > 0)
            {
                color = AppSettings.DiffRemovedExtraColor;
                markerStrategy.AddMarker(new TextMarker(lineRemoved.Offset + beginOffset,
                                                        lineRemoved.Length - beginOffset - reverseOffset,
                                                        TextMarkerType.SolidBlock, color,
                                                        ColorHelper.GetForeColorForBackColor(color)));
            }

        }

        private void AddExtraPatchHighlighting(IDocument document)
        {
            var line = 0;

            bool found = false;
            int numberOfParents;
            var linesRemoved = GetLinesStartingWith(document, ref line, '-', ref found);
            var linesAdded = GetLinesStartingWith(document, ref line, '+', ref found);
            if (linesAdded.Count == 1 && linesRemoved.Count == 1)
            {
                var lineA = linesRemoved[0];
                var lineB = linesAdded[0];
                if (lineA.Length > 4 && lineB.Length > 4 &&
                    document.GetCharAt(lineA.Offset + 4) == 'a' &&
                    document.GetCharAt(lineB.Offset + 4) == 'b')
                    numberOfParents = 5;
                else
                    numberOfParents = 4;

                MarkDifference(document, linesRemoved, linesAdded, numberOfParents);
            }

            numberOfParents = 1;
            while (line < document.TotalNumberOfLines)
            {
                found = false;
                linesRemoved = GetLinesStartingWith(document, ref line, '-', ref found);
                linesAdded = GetLinesStartingWith(document, ref line, '+', ref found);

                MarkDifference(document, linesRemoved, linesAdded, numberOfParents);
            }
        }

        private void ProcessLineSegment(IDocument document, ref int line, LineSegment lineSegment, char signChar, Color color)
        {
            if (document.GetCharAt(lineSegment.Offset) == signChar)
            {
                var endLine = document.GetLineSegment(line);

                for (; line < document.TotalNumberOfLines && document.GetCharAt(endLine.Offset) == signChar; line++)
                {
                    endLine = document.GetLineSegment(line);
                }
                line--;
                line--;
                endLine = document.GetLineSegment(line);

                document.MarkerStrategy.AddMarker(new TextMarker(lineSegment.Offset,
                                                        (endLine.Offset + endLine.TotalLength) -
                                                        lineSegment.Offset, TextMarkerType.SolidBlock, color,
                                                        ColorHelper.GetForeColorForBackColor(color)));
            }
        }

        public void AddPatchHighlighting(IDocument document)
        {
            var markerStrategy = document.MarkerStrategy;
            markerStrategy.RemoveAll(m => true);
            bool forceAbort = false;

            AddExtraPatchHighlighting(document);

            for (var line = 0; line < document.TotalNumberOfLines && !forceAbort; line++)
            {
                var lineSegment = document.GetLineSegment(line);

                if (lineSegment.TotalLength == 0)
                    continue;

                if (line == document.TotalNumberOfLines - 1)
                    forceAbort = true;

                ProcessLineSegment(document, ref line, lineSegment, '+', AppSettings.DiffAddedColor);
                ProcessLineSegment(document, ref line, lineSegment, '-', AppSettings.DiffRemovedColor);
                ProcessLineSegment(document, ref line, lineSegment, '@', AppSettings.DiffSectionColor);
                ProcessLineSegment(document, ref line, lineSegment, '\\', AppSettings.DiffSectionColor);
            }
        }

        public void HighlightLine(IDocument document, int line, Color color)
        {
            if (line >= document.TotalNumberOfLines)
                return;

            var markerStrategy = document.MarkerStrategy;
            var lineSegment = document.GetLineSegment(line);
            markerStrategy.AddMarker(new TextMarker(lineSegment.Offset,
                                                    lineSegment.Length, TextMarkerType.SolidBlock, color
                                                    ));
        }

        public void HighlightLines(IDocument document, int startLine, int endLine, Color color)
        {
            if (startLine > endLine || endLine >= document.TotalNumberOfLines)
                return;

            var markerStrategy = document.MarkerStrategy;
            var startLineSegment = document.GetLineSegment(startLine);
            var endLineSegment = document.GetLineSegment(endLine);
            markerStrategy.AddMarker(new TextMarker(startLineSegment.Offset,
                                                    endLineSegment.Offset - startLineSegment.Offset + endLineSegment.Length,
                                                    TextMarkerType.SolidBlock, color
                                                    ));
        }
    }
}
