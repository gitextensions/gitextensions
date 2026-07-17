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
        int lineNumberInDiff = 0;
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

            ++lineNumberInDiff;
            string lineText = document.GetText(line.Offset, line.Length);
            if (lineText.StartsWith("@@", StringComparison.Ordinal))
            {
                Match match = DiffRegex.Match(lineText);
                if (!match.Success)
                {
                    continue;
                }

                leftLineNumber = int.Parse(match.Groups["leftStart"].ValueSpan);
                rightLineNumber = int.Parse(match.Groups["rightStart"].ValueSpan);
                foundHunk = true;
                result.Add(CreateInfo(line, DiffLineType.Header));
                continue;
            }

            if (!foundHunk)
            {
                continue;
            }

            IReadOnlyList<DiffTextMarker> lineMarkers =
                [.. allTextMarkers.Where(marker => marker.Offset < line.EndOffset && marker.EndOffset >= line.Offset)];

            if (isCombinedDiff)
            {
                DiffLineInfo combined = CreateInfo(line, DiffLineType.Context);
                if (IsMinusLineInCombinedDiff(lineText))
                {
                    combined.LineType = DiffLineType.Minus;
                    combined.LineSegment = new SimpleSegment(line.Offset, line.Length);
                }
                else
                {
                    combined.RightLineNumber = rightLineNumber++;
                    if (IsPlusLineInCombinedDiff(lineText))
                    {
                        combined.LineType = DiffLineType.Plus;
                        combined.LineSegment = new SimpleSegment(line.Offset, line.Length);
                    }
                }

                result.Add(combined);
                continue;
            }

            if (lineText.StartsWith('\\'))
            {
                result.Add(CreateInfo(line, DiffLineType.Header));
                continue;
            }

            if (isGitWordDiff)
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

                DiffLineInfo word = CreateInfo(line, type);
                if (hasRemoved)
                {
                    word.LeftLineNumber = leftLineNumber++;
                }

                if (hasAdded)
                {
                    word.RightLineNumber = rightLineNumber++;
                }

                if (!hasRemoved && !hasAdded)
                {
                    word.LeftLineNumber = leftLineNumber++;
                    word.RightLineNumber = rightLineNumber++;
                }

                result.Add(word);
                continue;
            }

            if (lineText.StartsWith("-", StringComparison.Ordinal))
            {
                DiffLineInfo removed = CreateInfo(line, DiffLineType.Minus);
                removed.LeftLineNumber = leftLineNumber++;
                removed.LineSegment = new SimpleSegment(line.Offset, line.Length);
                removed.IsMovedLine = lineMarkers.Any(marker => marker.Kind == DiffMarkerKind.MovedRemoved);
                result.Add(removed);
            }
            else if (lineText.StartsWith("+", StringComparison.Ordinal))
            {
                DiffLineInfo added = CreateInfo(line, DiffLineType.Plus);
                added.RightLineNumber = rightLineNumber++;
                added.LineSegment = new SimpleSegment(line.Offset, line.Length);
                added.IsMovedLine = lineMarkers.Any(marker => marker.Kind == DiffMarkerKind.MovedAdded);
                result.Add(added);
            }
            else
            {
                DiffLineInfo context = CreateInfo(line, DiffLineType.Context);
                context.LeftLineNumber = leftLineNumber++;
                context.RightLineNumber = rightLineNumber++;
                result.Add(context);
            }
        }

        return result;

        static DiffLineInfo CreateInfo(DocumentLine line, DiffLineType type)
            => new()
            {
                LineNumInDiff = line.LineNumber,
                LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                RightLineNumber = DiffLineInfo.NotApplicableLineNum,
                LineType = type,
            };

        static bool IsPlusLineInCombinedDiff(string line)
            => line.StartsWith("++", StringComparison.Ordinal)
                || line.StartsWith("+ ", StringComparison.Ordinal)
                || line.StartsWith(" +", StringComparison.Ordinal);

        static bool IsMinusLineInCombinedDiff(string line)
            => line.StartsWith("--", StringComparison.Ordinal)
                || line.StartsWith("- ", StringComparison.Ordinal)
                || line.StartsWith(" -", StringComparison.Ordinal);
    }
}
