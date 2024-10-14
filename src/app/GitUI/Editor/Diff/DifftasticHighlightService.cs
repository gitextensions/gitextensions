using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands;
using GitExtensions.Extensibility;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

/// <summary>
/// Highlight difftastic diff
/// </summary>
public partial class DifftasticHighlightService : TextHighlightService
{
    protected readonly List<TextMarker> _textMarkers = [];
    private DiffLinesInfo _diffLinesInfo = new();

    [GeneratedRegex(@"^(\s*(?<matchStart>(?<lineNo>\d+)|(\.+)) )", RegexOptions.ExplicitCapture)]
    private static partial Regex LineNoRegex();

    public DifftasticHighlightService(ref string text, DiffViewerLineNumberControl lineNumbersControl, out int rightColumnStart)
    {
        // Hide VRulerPos by default
        rightColumnStart = 0;
        if (!int.TryParse(new EnvironmentAbstraction().GetEnvironmentVariable("DFT_WIDTH"), out int column))
        {
            column = 80;
        }

        StringBuilder sb = new(text.Length);
        StringBuilder lineBuilder = column > 0 ? new(column) : new();
        List<TextMarker> textMarkers = [];
        int halfColumn = column / 2;
        bool nextIsHeader = true;
        bool debugPrinted = false;

        foreach (string rawLine in text.LazySplit('\n'))
        {
            if (string.IsNullOrWhiteSpace(rawLine))
            {
                // Empty rawLine before header
                nextIsHeader = true;
                continue;
            }

            lineBuilder.Clear();
            textMarkers.Clear();
            AnsiEscapeUtilities.ParseEscape(rawLine, lineBuilder, textMarkers, themeColors: AppSettings.ReverseGitColoring.Value);

            int leftLineNo = DiffLineInfo.NotApplicableLineNum;
            int rightLineNo = DiffLineInfo.NotApplicableLineNum;

            if (nextIsHeader)
            {
                nextIsHeader = false;
                AddInfo(leftLineNo, rightLineNo, DiffLineType.Header, textMarkers, lineBuilder);
                continue;
            }

            DiffLineType lineType = DiffLineType.Context;

            Match matchLeft = LineNoRegex().Match(lineBuilder.ToString());
            if (!matchLeft.Success)
            {
                // This could also be a side-by-diff with zero context where ... are not printed
                Debug.WriteLineIf(debugPrinted, $"Unexpected Difftastic output, no left line. This could occur for last line and may be OK if another difftool is used. ({_diffLinesInfo.DiffLines.Count}) {lineBuilder}");
                debugPrinted = true;
                AddInfo(leftLineNo, rightLineNo, lineType, textMarkers, lineBuilder);
                continue;
            }

            int leftLen;
            if (matchLeft.Groups["matchStart"].Index >= halfColumn)
            {
                // No left line number (occurs if zero context), ignore (unexpected) markers
                leftLen = 0;
                if (rightColumnStart > 0)
                {
                    // Keep a consistent start for the second column
                    leftLen = halfColumn - rightColumnStart;
                }
            }
            else
            {
                leftLen = matchLeft.Length;
                if (matchLeft.Groups["lineNo"].Success && int.TryParse(matchLeft.Groups["lineNo"].ValueSpan, out int lineNo))
                {
                    leftLineNo = lineNo;
                }

                if (textMarkers.Count > 0 && textMarkers[0].Offset < leftLen)
                {
                    // GE theme colors sets reverse colors
                    Color c = AppSettings.ReverseGitColoring.Value ? textMarkers[0].Color : textMarkers[0].ForeColor;
                    if (c.R != c.G)
                    {
                        // Assume the theme in Diffstatic sets gray and the GE theme has the same value for red/green when context
                        if (c.R > c.G)
                        {
                            lineType = DiffLineType.MinusLeft;
                        }
                        else
                        {
                            lineType = DiffLineType.PlusRight;
                            rightLineNo = leftLineNo;
                            leftLineNo = DiffLineInfo.NotApplicableLineNum;
                        }
                    }
                }
            }

            // Trim left lineno from text, marker
            if (leftLen > 0)
            {
                lineBuilder = lineBuilder.Remove(0, leftLen);
                foreach (TextMarker tm in textMarkers)
                {
                    RemoveLineNoPart(tm, 0, leftLen);
                }
            }

            // Where to try parse for next line number, if both-sides is displayed
            int rightStartOffset = halfColumn - leftLen;
            Match matchRight;
            if (lineBuilder.Length > rightStartOffset
                && LineNoRegex().Match(lineBuilder.ToString()[rightStartOffset..]) is Match match
                && match.Success)
            {
                matchRight = match;
                if (rightColumnStart == 0)
                {
                    // Keep a consistent start for the second column
                    rightColumnStart = rightStartOffset;
                }
            }
            else
            {
                // Lineno assumed in start of line
                rightStartOffset = 0;
                matchRight = LineNoRegex().Match(lineBuilder.ToString());
            }

            if (!matchRight.Success)
            {
                if (lineType != DiffLineType.PlusRight)
                {
                    Debug.WriteLineIf(debugPrinted, $"Unexpected Difftastic no right lineno. This is OK if another difftool is used. ({_diffLinesInfo.DiffLines.Count}) {lineBuilder}");
                    debugPrinted = true;
                }

                AddInfo(leftLineNo, rightLineNo, lineType, textMarkers, lineBuilder);
                continue;
            }

            if (lineType == DiffLineType.PlusRight)
            {
                Debug.WriteLineIf(debugPrinted, $"Unexpected Difftastic has PlusRight and right lineno. This is OK if another difftool is used. ({_diffLinesInfo.DiffLines.Count}) {lineBuilder}");
                debugPrinted = true;
            }

            if (matchRight.Groups["lineNo"].Success && int.TryParse(matchRight.Groups["lineNo"].ValueSpan, out int rightNo))
            {
                rightLineNo = rightNo;
            }

            // Remove right line no from text, markers
            int rightLen = matchRight.Length;
            lineBuilder = lineBuilder.Remove(rightStartOffset, rightLen);
            int columnGap = 0;

            if (rightStartOffset > 0)
            {
                if (rightColumnStart == 0)
                {
                    rightColumnStart = rightStartOffset;
                }

                // Add spaces so both-sides markers are aligned
                columnGap = rightColumnStart - rightStartOffset;
                if (columnGap > 0)
                {
                    lineBuilder = lineBuilder.Insert(rightStartOffset, new string(' ', columnGap));
                }
            }

            bool first = true;
            foreach (TextMarker tm in textMarkers)
            {
                if (tm.EndOffset < rightStartOffset)
                {
                    continue;
                }

                if (first)
                {
                    first = false;

                    Color c = AppSettings.ReverseGitColoring.Value ? tm.Color : tm.ForeColor;
                    if (c.R != c.G)
                    {
                        // Merge line type with existing left line type
                        lineType = lineType == DiffLineType.Context ? DiffLineType.PlusRight : DiffLineType.MinusPlus;
                    }
                }

                RemoveLineNoPart(tm, rightStartOffset, rightLen);
                if (tm.Offset >= rightStartOffset)
                {
                    tm.Offset += columnGap;
                }
            }

            AddInfo(leftLineNo, rightLineNo, lineType, textMarkers, lineBuilder);
        }

        text = sb.ToString();
        lineNumbersControl.DisplayLineNum(_diffLinesInfo, showLeftColumn: true);

        return;

        static void RemoveLineNoPart(TextMarker tm, int offset, int length)
        {
            if (tm.Offset + tm.Length <= offset)
            {
                // All is before the gap
                return;
            }

            if (tm.Offset >= offset + length)
            {
                // All is after the gap
                tm.Offset -= length;
                return;
            }

            if (tm.Offset <= offset && offset + length <= tm.Offset + tm.Length)
            {
                // Gap is covered
                tm.Length -= length;
                return;
            }

            if (tm.Offset > offset)
            {
                // Remove the start in the gap
                tm.Length -= tm.Offset - offset;
                tm.Offset = offset;
                return;
            }

            // the end part of the gap
            tm.Length -= tm.Offset + tm.Length - offset;
        }

        void AddInfo(int leftLineNo, int rightLineNo, DiffLineType lineType, List<TextMarker> textMarkers, StringBuilder lineBuilder)
        {
            _diffLinesInfo.Add(
                new()
                {
                    LineNumInDiff = _diffLinesInfo.DiffLines.Count + 1,
                    LeftLineNumber = leftLineNo,
                    RightLineNumber = rightLineNo,
                    LineType = lineType,
                    LineSegment = null,
                    IsMovedLine = false,
                });
            for (int i = 0; i < textMarkers.Count; ++i)
            {
                TextMarker tm = textMarkers[i];
                if (tm.Length <= 0)
                {
                    textMarkers.RemoveAt(i);
                    --i;
                    continue;
                }

                tm.Offset += sb.Length;
            }

            _textMarkers.AddRange(textMarkers);
            sb.Append(lineBuilder);
            sb.Append('\n');
        }
    }

    public override bool IsSearchMatch(DiffViewerLineNumberControl lineNumbersControl, int indexInText)
        => lineNumbersControl.GetLineInfo(indexInText)?.LineType is DiffLineType.Plus or DiffLineType.Minus or DiffLineType.MinusPlus or DiffLineType.MinusLeft or DiffLineType.PlusRight;

    public override void AddTextHighlighting(IDocument document)
    {
        foreach (TextMarker tm in _textMarkers)
        {
            document.MarkerStrategy.AddMarker(tm);
        }
    }
}
