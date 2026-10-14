using System.Text.RegularExpressions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

public partial class DiffLineNumAnalyzer
{
    [GeneratedRegex(@"\-(?<leftStart>\d{1,})\,{0,}(?<leftCount>\d{0,})\s\+(?<rightStart>\d{1,})\,{0,}(?<rightCount>\d{0,})", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture)]
    private static partial Regex DiffRegex { get; }

    public static DiffLinesInfo Analyze(string text, IReadOnlyList<TextMarker> allTextMarkers, bool isCombinedDiff, bool isGitWordDiff = false)
    {
        DiffLinesInfo ret = new();
        bool reverseGitColoring = AppSettings.ReverseGitColoring.Value;
        int lineNumInDiff = 0;
        int leftLineNum = DiffLineInfo.NotApplicableLineNum;
        int rightLineNum = DiffLineInfo.NotApplicableLineNum;
        bool isHeaderLineLocated = false;
        string[] lines = text.Split(Delimiters.LineFeed);
        int textOffset = 0;
        int lastLine = lines.Length - 1;
        LineMarkerReader markerReader = new(allTextMarkers);
        for (int i = 0; i <= lastLine; i++)
        {
            string line = lines[i];
            int lineLength = line.Length;
            int lineEndLength = Math.Min(1, text.Length - (textOffset + lineLength));
            if (line.EndsWith(Delimiters.CarriageReturn))
            {
                ++lineEndLength;
                --lineLength;
            }

            if (i == lastLine && lineLength == 0)
            {
                break;
            }

            Lazy<List<TextMarker>> textMarkers = new(()
                => markerReader.GetMarkers(textOffset, lineLength));

            lineNumInDiff++;
            if (line.StartsWith("@@"))
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                    RightLineNumber = DiffLineInfo.NotApplicableLineNum,
                    LineType = DiffLineType.Header
                };

                Match lineNumbers = DiffRegex.Match(line);
                leftLineNum = int.Parse(lineNumbers.Groups["leftStart"].ValueSpan);
                rightLineNum = int.Parse(lineNumbers.Groups["rightStart"].ValueSpan);

                ret.Add(meta);
                isHeaderLineLocated = true;
            }
            else if (!isHeaderLineLocated)
            {
                // No marker to add
            }
            else if (isCombinedDiff)
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                    RightLineNumber = DiffLineInfo.NotApplicableLineNum,
                    LineSegment = new Segment() { Offset = textOffset, Length = lineLength },
                    IsMovedLine = false,
                };

                if (IsMinusLineInCombinedDiff(line))
                {
                    // left line is from two documents, so undefined
                    meta.LineType = DiffLineType.Minus;
                }
                else if (IsPlusLineInCombinedDiff(line))
                {
                    meta.LineType = DiffLineType.Plus;
                    meta.RightLineNumber = rightLineNum;
                    rightLineNum++;
                }
                else
                {
                    meta.LineType = DiffLineType.Context;
                    meta.RightLineNumber = rightLineNum;
                    rightLineNum++;
                }

                ret.Add(meta);
            }
            else if (!isGitWordDiff ? IsMinusLine(line) : IsGitWordMatch(DiffLineType.MinusLeft, line, textOffset, lineLength, textMarkers.Value))
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = leftLineNum,
                    RightLineNumber = DiffLineInfo.NotApplicableLineNum,
                    LineType = isGitWordDiff ? DiffLineType.MinusLeft : DiffLineType.Minus,
                    LineSegment = new Segment() { Offset = textOffset, Length = lineLength },
                };
                meta.IsMovedLine = IsMovedLine(textMarkers.Value, meta);
                ret.Add(meta);

                leftLineNum++;
            }
            else if (!isGitWordDiff ? IsPlusLine(line) : IsGitWordMatch(DiffLineType.PlusRight, line, textOffset, lineLength, textMarkers.Value))
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                    RightLineNumber = rightLineNum,
                    LineType = isGitWordDiff ? DiffLineType.PlusRight : DiffLineType.Plus,
                    LineSegment = new Segment() { Offset = textOffset, Length = lineLength },
                };
                meta.IsMovedLine = IsMovedLine(textMarkers.Value, meta);
                ret.Add(meta);
                rightLineNum++;
            }
            else if (isGitWordDiff && textMarkers.Value.Count > 0)
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = leftLineNum,
                    RightLineNumber = rightLineNum,
                    LineType = DiffLineType.MinusPlus,
                };
                ret.Add(meta);
                leftLineNum++;
                rightLineNum++;
            }
            else if (!isGitWordDiff && line.StartsWith('\\'))
            {
                // git-diff has inserted this line, present it as a header
                // The only known string is GitModule.NoNewLineAtTheEnd
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                    RightLineNumber = DiffLineInfo.NotApplicableLineNum,
                    LineType = DiffLineType.Header
                };
                ret.Add(meta);
            }
            else
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = leftLineNum,
                    RightLineNumber = rightLineNum,
                    LineType = DiffLineType.Context,
                };
                ret.Add(meta);

                leftLineNum++;
                rightLineNum++;
            }

            textOffset += Math.Min(lineLength + lineEndLength, text.Length - textOffset);
        }

        return ret;

        // git-diff colors moved lines in other than red green
        // However, Git may mark trailing whitespaces (diff.colormovedws is ignored)
        bool IsMovedLine(List<TextMarker> textMarkers, DiffLineInfo meta)
            => textMarkers.Count > 0
                && !MarkerColorMatch(textMarkers[0], meta.LineType)
                && (textMarkers.Count <= 1 || !MarkerColorMatch(textMarkers[^1], meta.LineType) || text.AsSpan()[textMarkers[^1].Offset..textMarkers[^1].EndOffset].IsWhiteSpace())
                && (textMarkers.Count <= 2 || !textMarkers[1..^1].All(m => MarkerColorMatch(m, meta.LineType)));

        bool IsGitWordMatch(DiffLineType lineType, string line, int textOffset, int textLength, List<TextMarker> textMarkers)
        {
            // Heuristics (or wild guessing): For GitWordDiff find if the line is exclusive (otherwise DiffLineType.MinusPlus).
            // If the marker covers the line this should be true.
            // Git output is impossible to parse, some guesses are done.
            // Whitespace only lines are still incorrect (no marker at all in Git),
            // as well as some other situations.

            int firstNonWhiteSpace = line.Length - line.AsSpan().TrimStart().Length;

            return textMarkers.Count == 1

                    // start may be indented, if a new block of changes starts with white spaces
                    && (textMarkers[0].Offset <= textOffset || (firstNonWhiteSpace > 0 && textMarkers[0].Offset <= textOffset + firstNonWhiteSpace))

                    // Compensate length->ending and remove the trailing newline chars (no check for \r\n vs \n)
                    && (textMarkers[0].EndOffset >= textOffset + textLength - 3)

                    // Assume the user has not overridden colors
                    && MarkerColorMatch(textMarkers[0], lineType);
        }

        bool MarkerColorMatch(TextMarker textMarker, DiffLineType lineType)
        {
            // The expected marker color for a line type, for heuristics.

            return lineType is (DiffLineType.Minus or DiffLineType.MinusLeft)
                ? (reverseGitColoring
                    ? textMarker.Color == AppColor.AnsiTerminalRedBackNormal.GetThemeColor()
                    : textMarker.ForeColor == AppColor.AnsiTerminalRedForeNormal.GetThemeColor())
                : (reverseGitColoring
                    ? textMarker.Color == AppColor.AnsiTerminalGreenBackNormal.GetThemeColor()
                    : textMarker.ForeColor == AppColor.AnsiTerminalGreenForeNormal.GetThemeColor());
        }

        static bool IsMinusLine(string line)
        {
            return line.StartsWith('-');
        }

        static bool IsPlusLine(string line)
        {
            return line.StartsWith('+');
        }

        static bool IsPlusLineInCombinedDiff(string line)
        {
            return line.StartsWith("++") || line.StartsWith("+ ") || line.StartsWith(" +");
        }

        static bool IsMinusLineInCombinedDiff(string line)
        {
            return line.StartsWith("--") || line.StartsWith("- ") || line.StartsWith(" -");
        }
    }

    /// <summary>
    /// Reads overlapping markers for successive lines without searching the whole document each time.
    /// Marker order is significant to the moved-line heuristics, so retain the input order even when
    /// markers overlap or are supplied out of offset order.
    /// </summary>
    internal sealed class LineMarkerReader(IReadOnlyList<TextMarker> markers)
    {
        private readonly (TextMarker Marker, int Index)[] _byOffset = markers
            .Select((marker, index) => (Marker: marker, Index: index))
            .OrderBy(entry => entry.Marker.Offset)
            .ToArray();
        private readonly SortedDictionary<int, TextMarker> _active = [];
        private int _next;

        public List<TextMarker> GetMarkers(int offset, int length)
        {
            foreach (int index in _active.Where(entry => entry.Value.EndOffset < offset).Select(entry => entry.Key).ToArray())
            {
                _active.Remove(index);
            }

            while (_next < _byOffset.Length && _byOffset[_next].Marker.Offset < offset + length)
            {
                (TextMarker marker, int index) = _byOffset[_next++];
                if (marker.EndOffset >= offset)
                {
                    _active.Add(index, marker);
                }
            }

            return [.. _active.Values];
        }
    }
}
