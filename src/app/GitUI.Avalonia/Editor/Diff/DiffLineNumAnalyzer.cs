using System.Text.RegularExpressions;
using AvaloniaEdit.Document;

namespace GitUI.Editor.Diff;

public partial class DiffLineNumAnalyzer
{
    [GeneratedRegex(@"\-(?<leftStart>\d+)(?:,(?<leftCount>\d*))?\s+\+(?<rightStart>\d+)(?:,(?<rightCount>\d*))?", RegexOptions.ExplicitCapture)]
    private static partial Regex DiffRegex { get; }

    internal static DiffLinesInfo Analyze(
        string text,
        IReadOnlyList<DiffTextMarker> allTextMarkers,
        bool isCombinedDiff,
        bool isGitWordDiff = false)
    {
        DiffLinesInfo result = new();
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
                continue;
            }

            IReadOnlyList<DiffTextMarker> lineMarkers =
                [.. allTextMarkers.Where(marker => marker.Offset < line.EndOffset && marker.EndOffset >= line.Offset)];

            DiffLineInfo info;
            if (isCombinedDiff)
            {
                info = CreateCombinedInfo(line, lineText, ref rightLineNumber);
            }
            else if (lineText.StartsWith('\\'))
            {
                info = CreateInfo(line, DiffLineType.Header);
            }
            else if (isGitWordDiff)
            {
                info = CreateWordDiffInfo(line, lineMarkers, ref leftLineNumber, ref rightLineNumber);
            }
            else
            {
                info = CreateOrdinaryInfo(line, lineText, lineMarkers, ref leftLineNumber, ref rightLineNumber);
            }

            result.Add(info);
        }

        return result;
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

    private static DiffLineInfo CreateWordDiffInfo(
        DocumentLine line,
        IReadOnlyList<DiffTextMarker> lineMarkers,
        ref int leftLineNumber,
        ref int rightLineNumber)
    {
        bool hasRemoved = lineMarkers.Any(marker => marker.Kind is DiffMarkerKind.Removed or DiffMarkerKind.MovedRemoved);
        bool hasAdded = lineMarkers.Any(marker => marker.Kind is DiffMarkerKind.Added or DiffMarkerKind.MovedAdded);
        DiffLineType type = (hasRemoved, hasAdded) switch
        {
            (true, true) => DiffLineType.MinusPlus,
            (true, false) => DiffLineType.MinusLeft,
            (false, true) => DiffLineType.PlusRight,
            _ => DiffLineType.Context,
        };

        DiffLineInfo info = CreateInfo(line, type);
        if (hasRemoved || !hasAdded)
        {
            info.LeftLineNumber = leftLineNumber++;
        }

        if (hasAdded || !hasRemoved)
        {
            info.RightLineNumber = rightLineNumber++;
        }

        return info;
    }

    private static DiffLineInfo CreateOrdinaryInfo(
        DocumentLine line,
        string lineText,
        IReadOnlyList<DiffTextMarker> lineMarkers,
        ref int leftLineNumber,
        ref int rightLineNumber)
    {
        if (lineText.StartsWith("-", StringComparison.Ordinal))
        {
            DiffLineInfo removed = CreateInfo(line, DiffLineType.Minus);
            removed.LeftLineNumber = leftLineNumber++;
            removed.LineSegment = new SimpleSegment(line.Offset, line.Length);
            removed.IsMovedLine = lineMarkers.Any(marker => marker.Kind == DiffMarkerKind.MovedRemoved);
            return removed;
        }

        if (lineText.StartsWith("+", StringComparison.Ordinal))
        {
            DiffLineInfo added = CreateInfo(line, DiffLineType.Plus);
            added.RightLineNumber = rightLineNumber++;
            added.LineSegment = new SimpleSegment(line.Offset, line.Length);
            added.IsMovedLine = lineMarkers.Any(marker => marker.Kind == DiffMarkerKind.MovedAdded);
            return added;
        }

        DiffLineInfo context = CreateInfo(line, DiffLineType.Context);
        context.LeftLineNumber = leftLineNumber++;
        context.RightLineNumber = rightLineNumber++;
        return context;
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
