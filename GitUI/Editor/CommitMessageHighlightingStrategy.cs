using System.Collections.Generic;
using System.Drawing;
using GitCommands;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor
{
    internal sealed class CommitMessageHighlightingStrategy : GitHighlightingStrategyBase
    {
        private static HighlightColor ColorSummary { get; } = new HighlightColor(Color.Black, bold: true, italic: false);

        private readonly List<TextMarker> _overlengthDescriptionMarkers = new List<TextMarker>();

        private readonly TextMarker _markerSummaryTooLong = new TextMarker(0, 0, TextMarkerType.WaveLine, Color.Red) { ToolTip = "Summary line is too long." };
        private readonly TextMarker _markerSpacerNeeded = new TextMarker(0, 0, TextMarkerType.WaveLine, Color.Red) { ToolTip = "There must be a blank line after the summary." };

        public CommitMessageHighlightingStrategy(GitModule module)
            : base("GitCommitMessage", module)
        {
        }

        // TODO pending issue is that when text is pasted into the editor, validation markers are not updated until their lines are modified (seems like a bug in the editor)

        protected override void MarkTokens(IDocument document, IList<LineSegment> lines)
        {
            var summaryLineNumber = -1;
            var seenDividingSpace = false;
            var descriptionStartLineNumber = -1;

            foreach (var line in document.LineSegmentCollection)
            {
                if (summaryLineNumber == -1)
                {
                    if (!IsEmptyOrWhiteSpace(document, line) && !IsComment(document, line))
                    {
                        summaryLineNumber = line.LineNumber;
                    }
                }
                else if (!seenDividingSpace)
                {
                    if (IsEmptyOrWhiteSpace(document, line))
                    {
                        seenDividingSpace = true;
                    }
                    else if (!IsComment(document, line))
                    {
                        descriptionStartLineNumber = line.LineNumber;
                        break;
                    }
                }
                else if (descriptionStartLineNumber != -1)
                {
                    if (!IsEmptyOrWhiteSpace(document, line) && !IsComment(document, line))
                    {
                        descriptionStartLineNumber = line.LineNumber;
                        break;
                    }
                }
            }

            const int maxSummaryLength = 50;
            const int maxDescriptionLength = 80;

            // NOTE the pattern of removing then adding markers might look suboptimal, but tracking their presence isn't reliable as they can be removed without warning

            foreach (var line in lines)
            {
                var lineNumber = line.LineNumber;

                if (TryHighlightComment(document, line))
                {
                }
                else
                {
                    var color = lineNumber == summaryLineNumber ? ColorSummary : ColorNormal;

                    line.Words = new List<TextWord>(capacity: 1)
                        { new TextWord(document, line, 0, line.Length, color, hasDefaultColor: false) };

                    if (lineNumber == summaryLineNumber)
                    {
                        document.MarkerStrategy.RemoveMarker(_markerSummaryTooLong);

                        if (line.Length > maxSummaryLength)
                        {
                            _markerSummaryTooLong.Offset = line.Offset + maxSummaryLength;
                            _markerSummaryTooLong.Length = line.Length - maxSummaryLength;
                            document.MarkerStrategy.AddMarker(_markerSummaryTooLong);
                        }
                    }
                    else if (lineNumber == descriptionStartLineNumber)
                    {
                        document.MarkerStrategy.RemoveMarker(_markerSpacerNeeded);

                        if (!seenDividingSpace)
                        {
                            _markerSpacerNeeded.Offset = line.Offset;
                            _markerSpacerNeeded.Length = line.Length;
                            document.MarkerStrategy.AddMarker(_markerSpacerNeeded);
                        }
                    }
                }

                document.RequestUpdate(
                    new TextAreaUpdate(TextAreaUpdateType.SingleLine, lineNumber));
            }

            if (descriptionStartLineNumber == -1)
            {
                if (_overlengthDescriptionMarkers.Count != 0)
                {
                    foreach (var marker in _overlengthDescriptionMarkers)
                    {
                        document.MarkerStrategy.RemoveMarker(marker);
                    }

                    _overlengthDescriptionMarkers.Clear();
                }

                return;
            }

            var markerIndex = 0;

            foreach (var line in document.LineSegmentCollection)
            {
                if (line.LineNumber < descriptionStartLineNumber)
                {
                    continue;
                }

                if (line.Length > maxDescriptionLength)
                {
                    var markerOffset = line.Offset + maxDescriptionLength;
                    var markerLength = line.Length - maxDescriptionLength;

                    TextMarker overlengthMarker;
                    if (markerIndex < _overlengthDescriptionMarkers.Count)
                    {
                        overlengthMarker = _overlengthDescriptionMarkers[markerIndex];
                    }
                    else
                    {
                        overlengthMarker = new TextMarker(markerOffset, markerLength, TextMarkerType.WaveLine, Color.Red) { ToolTip = "Line is too long." };
                        _overlengthDescriptionMarkers.Add(overlengthMarker);
                        document.MarkerStrategy.AddMarker(overlengthMarker);
                    }

                    overlengthMarker.Offset = markerOffset;
                    overlengthMarker.Length = markerLength;

                    markerIndex++;
                }
            }

            var toRemove = _overlengthDescriptionMarkers.Count - markerIndex;

            if (toRemove > 0)
            {
                for (var i = 0; i < toRemove; i++)
                {
                    var marker = _overlengthDescriptionMarkers[markerIndex + i];
                    document.MarkerStrategy.RemoveMarker(marker);
                }

                _overlengthDescriptionMarkers.RemoveRange(markerIndex, toRemove);
            }
        }
    }
}
