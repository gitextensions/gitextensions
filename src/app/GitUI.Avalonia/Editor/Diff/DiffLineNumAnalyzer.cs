using System.Text.RegularExpressions;
using AvaloniaEdit.Document;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;

namespace GitUI.Editor.Diff;

public partial class DiffLineNumAnalyzer
{
    [GeneratedRegex(@"\-(?<leftStart>\d+)(?:,(?<leftCount>\d*))?\s+\+(?<rightStart>\d+)(?:,(?<rightCount>\d*))?", RegexOptions.ExplicitCapture)]
    private static partial Regex DiffRegex { get; }

    public static DiffLinesInfo Analyze(string text, IReadOnlyList<TextMarker> allTextMarkers, bool isCombinedDiff, bool isGitWordDiff = false)
    {
        DiffLinesInfo result = new();
        bool reverseGitColoring = AppSettings.ReverseGitColoring.Value;
        int leftLineNumber = DiffLineInfo.NotApplicableLineNum;
        int rightLineNumber = DiffLineInfo.NotApplicableLineNum;
        bool foundHunk = false;

        TextDocument document = new(text);
        foreach (DocumentLine line in document.Lines)
        {
            if (line.Length == 0 && line.Offset == document.TextLength && text.EndsWith('\n'))
            {
                break;
            }

            string lineText = document.GetText(line.Offset, line.Length);
            if (lineText.StartsWith("@@", StringComparison.Ordinal))
            {
                if (!TryCreateHunkInfo(
                        line,
                        lineText,
                        out DiffLineInfo hunk,
                        out leftLineNumber,
                        out rightLineNumber))
                {
                    continue;
                }

                foundHunk = true;
                result.Add(hunk);
                continue;
            }

            if (!foundHunk)
            {
                // No marker to add
                continue;
            }

            List<TextMarker> lineMarkers =
                [.. allTextMarkers.Where(marker => marker.Offset < line.EndOffset && marker.EndOffset >= line.Offset)];

            DiffLineInfo info;
            if (isCombinedDiff)
            {
                // left line is from two documents, so undefined
                info = CreateCombinedInfo(line, lineText, ref rightLineNumber);
            }
            else if (lineText.StartsWith('\\'))
            {
                // git-diff has inserted this line, present it as a header
                // The only known string is GitModule.NoNewLineAtTheEnd
                info = CreateInfo(line, DiffLineType.Header);
            }
            else if (isGitWordDiff)
            {
                info = CreateWordDiffInfo(line, lineText, lineMarkers, ref leftLineNumber, ref rightLineNumber);
            }
            else
            {
                info = CreateOrdinaryInfo(line, lineText, lineMarkers, ref leftLineNumber, ref rightLineNumber);
            }

            result.Add(info);
        }

        return result;

        // git-diff colors moved lines in other than red green
        // However, Git may mark trailing whitespaces (diff.colormovedws is ignored)
        bool IsMovedLine(List<TextMarker> textMarkers, DiffLineInfo meta)
            => textMarkers.Count > 0
                && !MarkerColorMatch(textMarkers[0], meta.LineType)
                && (textMarkers.Count <= 1 || !MarkerColorMatch(textMarkers[^1], meta.LineType) || text.AsSpan()[textMarkers[^1].Offset..textMarkers[^1].EndOffset].IsWhiteSpace())
                && (textMarkers.Count <= 2 || !textMarkers[1..^1].All(m => MarkerColorMatch(m, meta.LineType)));

        bool IsGitWordMatch(DiffLineType lineType, string lineText, DocumentLine line, List<TextMarker> textMarkers)
        {
            // Heuristics (or wild guessing): For GitWordDiff find if the line is exclusive (otherwise DiffLineType.MinusPlus).
            // If the marker covers the line this should be true.
            // Git output is impossible to parse, some guesses are done.
            // Whitespace only lines are still incorrect (no marker at all in Git),
            // as well as some other situations.

            int firstNonWhiteSpace = lineText.Length - lineText.AsSpan().TrimStart().Length;

            return textMarkers.Count == 1

                    // start may be indented, if a new block of changes starts with white spaces
                    && (textMarkers[0].Offset <= line.Offset || (firstNonWhiteSpace > 0 && textMarkers[0].Offset <= line.Offset + firstNonWhiteSpace))

                    // Compensate length->ending and remove the trailing newline chars (no check for \r\n vs \n)
                    && (textMarkers[0].EndOffset >= line.Offset + line.Length - 3)

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

        DiffLineInfo CreateWordDiffInfo(
            DocumentLine line,
            string lineText,
            List<TextMarker> lineMarkers,
            ref int currentLeftLineNumber,
            ref int currentRightLineNumber)
        {
            bool isRemoved = IsGitWordMatch(DiffLineType.MinusLeft, lineText, line, lineMarkers);
            bool isAdded = IsGitWordMatch(DiffLineType.PlusRight, lineText, line, lineMarkers);
            DiffLineType type = isRemoved
                ? DiffLineType.MinusLeft
                : isAdded
                    ? DiffLineType.PlusRight
                    : lineMarkers.Count > 0
                        ? DiffLineType.MinusPlus
                        : DiffLineType.Context;

            DiffLineInfo info = CreateInfo(line, type);
            if (isRemoved || !isAdded)
            {
                info.LeftLineNumber = currentLeftLineNumber++;
            }

            if (isAdded || !isRemoved)
            {
                info.RightLineNumber = currentRightLineNumber++;
            }

            return info;
        }

        DiffLineInfo CreateOrdinaryInfo(
            DocumentLine line,
            string lineText,
            List<TextMarker> lineMarkers,
            ref int currentLeftLineNumber,
            ref int currentRightLineNumber)
        {
            if (lineText.StartsWith("-", StringComparison.Ordinal))
            {
                DiffLineInfo removed = CreateInfo(line, DiffLineType.Minus);
                removed.LeftLineNumber = currentLeftLineNumber++;
                removed.LineSegment = new SimpleSegment(line.Offset, line.Length);
                removed.IsMovedLine = IsMovedLine(lineMarkers, removed);
                return removed;
            }

            if (lineText.StartsWith("+", StringComparison.Ordinal))
            {
                DiffLineInfo added = CreateInfo(line, DiffLineType.Plus);
                added.RightLineNumber = currentRightLineNumber++;
                added.LineSegment = new SimpleSegment(line.Offset, line.Length);
                added.IsMovedLine = IsMovedLine(lineMarkers, added);
                return added;
            }

            DiffLineInfo context = CreateInfo(line, DiffLineType.Context);
            context.LeftLineNumber = currentLeftLineNumber++;
            context.RightLineNumber = currentRightLineNumber++;
            return context;
        }
    }

    private static bool TryCreateHunkInfo(
        DocumentLine line,
        string lineText,
        out DiffLineInfo info,
        out int leftLineNumber,
        out int rightLineNumber)
    {
        Match match = DiffRegex.Match(lineText);
        if (!match.Success)
        {
            info = null!;
            leftLineNumber = DiffLineInfo.NotApplicableLineNum;
            rightLineNumber = DiffLineInfo.NotApplicableLineNum;
            return false;
        }

        leftLineNumber = int.Parse(match.Groups["leftStart"].ValueSpan);
        rightLineNumber = int.Parse(match.Groups["rightStart"].ValueSpan);
        info = CreateInfo(line, DiffLineType.Header);
        return true;
    }

    private static DiffLineInfo CreateCombinedInfo(DocumentLine line, string lineText, ref int rightLineNumber)
    {
        DiffLineInfo info = CreateInfo(line, DiffLineType.Context);
        if (IsMinusLineInCombinedDiff(lineText))
        {
            info.LineType = DiffLineType.Minus;
            info.LineSegment = new SimpleSegment(line.Offset, line.Length);
            return info;
        }

        info.RightLineNumber = rightLineNumber++;
        if (IsPlusLineInCombinedDiff(lineText))
        {
            info.LineType = DiffLineType.Plus;
            info.LineSegment = new SimpleSegment(line.Offset, line.Length);
        }

        return info;
    }

    private static DiffLineInfo CreateInfo(DocumentLine line, DiffLineType type)
        => new()
        {
            LineNumInDiff = line.LineNumber,
            LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
            RightLineNumber = DiffLineInfo.NotApplicableLineNum,
            LineType = type,
        };

    private static bool IsPlusLineInCombinedDiff(string line)
        => line.StartsWith("++", StringComparison.Ordinal)
            || line.StartsWith("+ ", StringComparison.Ordinal)
            || line.StartsWith(" +", StringComparison.Ordinal);

    private static bool IsMinusLineInCombinedDiff(string line)
        => line.StartsWith("--", StringComparison.Ordinal)
            || line.StartsWith("- ", StringComparison.Ordinal)
            || line.StartsWith(" -", StringComparison.Ordinal);
}
