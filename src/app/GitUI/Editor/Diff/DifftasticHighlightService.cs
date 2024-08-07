using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands;
using GitExtensions.Extensibility;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

/// <summary>
/// Highlight difftastic diff
/// </summary>
public partial class DifftasticHighlightService : TextHighlightService
{
    protected readonly List<TextMarker> _textMarkers = [];
    private DiffLinesInfo _matchInfos = new();

    [GeneratedRegex(@"^(\s*(?<matchStart>(?<lineNo>\d+)|(\.+)) )", RegexOptions.ExplicitCapture)]
    private static partial Regex LineNoRegex();

    public DifftasticHighlightService(ref string text)
    {
        if (!int.TryParse(new EnvironmentAbstraction().GetEnvironmentVariable("DFT_WIDTH"), out int column))
        {
            column = 80;
        }

        StringBuilder sb = new(text.Length);
        StringBuilder lineBuilder = column > 0 ? new(column) : new();
        List<TextMarker> textMarkers = [];
        int halfColumn = column / 2;
        int rightColumnStart = 0;
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
                Debug.WriteLineIf(debugPrinted, $"Unexpected Difftastic output, no left line. This could occur for last line and may be OK if another difftool is used. ({_matchInfos.DiffLines.Count}) {lineBuilder}");
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

            if (leftLen > 0)
            {
                lineBuilder = lineBuilder.Remove(0, leftLen);
                int i = 0;
                while (i < textMarkers.Count && textMarkers[i].Offset < leftLen)
                {
                    if (textMarkers[i].Offset < leftLen)
                    {
                        // Remove markers in lineno part
                        textMarkers.RemoveAt(i);
                        --i;
                    }
                    else
                    {
                        textMarkers[i].Offset = leftLen;
                    }

                    ++i;
                }

                foreach (TextMarker tm in textMarkers)
                {
                    tm.Offset = Math.Max(0, tm.Offset - leftLen);
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
                    Debug.WriteLineIf(debugPrinted, $"Unexpected Difftastic no right lineno. This is OK if another difftool is used. ({_matchInfos.DiffLines.Count}) {lineBuilder}");
                    debugPrinted = true;
                }
            }
            else
            {
                if (lineType == DiffLineType.PlusRight)
                {
                    Debug.WriteLineIf(debugPrinted, $"Unexpected Difftastic has PlusRight and right lineno. This is OK if another difftool is used. ({_matchInfos.DiffLines.Count}) {lineBuilder}");
                    debugPrinted = true;
                }

                if (matchRight.Groups["lineNo"].Success && int.TryParse(matchRight.Groups["lineNo"].ValueSpan, out int lineNo))
                {
                    rightLineNo = lineNo;
                }

                int rightLen = matchRight.Length;
                lineBuilder = lineBuilder.Remove(rightStartOffset, rightLen);

                int i = 0;
                bool first = true;
                while (i < textMarkers.Count)
                {
                    if (textMarkers[i].EndOffset < rightStartOffset)
                    {
                        ++i;
                        continue;
                    }

                    if (textMarkers[i].Offset >= rightStartOffset + rightLen)
                    {
                        break;
                    }

                    if (first)
                    {
                        first = false;

                        Color c = AppSettings.ReverseGitColoring.Value ? textMarkers[i].Color : textMarkers[i].ForeColor;
                        if (c.R != c.G)
                        {
                            // Merge line type with existing left line type
                            lineType = lineType == DiffLineType.Context ? DiffLineType.PlusRight : DiffLineType.MinusPlus;
                        }
                    }

                    if (textMarkers[i].Offset > rightStartOffset)
                    {
                        textMarkers[i].Length -= textMarkers[i].Offset - rightStartOffset;
                        textMarkers[i].Offset = rightStartOffset;
                    }

                    if (textMarkers[i].EndOffset < rightStartOffset + rightLen - 1)
                    {
                        // textMarkers[i].Length += rightStartOffset + rightLen - textMarkers[i].EndOffset;
                    }

                    if (textMarkers[i].Length <= 0
                        || (rightStartOffset >= textMarkers[i].Offset && textMarkers[i].EndOffset <= rightStartOffset + rightLen))
                    {
                        textMarkers.RemoveAt(i);
                        --i;
                    }

                    ++i;
                }

                if (rightStartOffset > 0)
                {
                    if (rightColumnStart == 0)
                    {
                        rightColumnStart = rightStartOffset;
                    }

                    int columnOffset = rightColumnStart - rightStartOffset;

                    // Add spaces so both-sides markers are aligned
                    // It would be nicer to set VRulerRow at rightColumnStart, but that need to be reset for next file
                    lineBuilder = lineBuilder.Insert(rightStartOffset, new string(' ', columnOffset) + '|');
                    rightLen -= columnOffset + 1;
                }

                foreach (TextMarker tm in textMarkers)
                {
                    if (tm.Offset >= rightStartOffset)
                    {
                        tm.Offset = Math.Max(0, tm.Offset - rightLen);
                    }
                }
            }

            AddInfo(leftLineNo, rightLineNo, lineType, textMarkers, lineBuilder);
        }

        text = sb.ToString();
        return;

        void AddInfo(int leftLineNo, int rightLineNo, DiffLineType lineType, List<TextMarker> textMarkers, StringBuilder lineBuilder)
        {
            _matchInfos.Add(
                new()
                {
                    LineNumInDiff = _matchInfos.DiffLines.Count + 1,
                    LeftLineNumber = leftLineNo,
                    RightLineNumber = rightLineNo,
                    LineType = lineType,
                });
            foreach (TextMarker tm in textMarkers)
            {
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

    public override void SetLineControl(DiffViewerLineNumberControl lineNumbersControl, TextEditorControl textEditor)
    {
        lineNumbersControl.DisplayLineNum(_matchInfos, showLeftColumn: true);
    }
}
