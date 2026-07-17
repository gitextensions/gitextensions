using AvaloniaEdit.Document;

namespace GitUI.Editor.Diff;

/// <summary>
/// Analyzes a diff document and produces the semantic line and inline ranges used by the viewer.
/// </summary>
public abstract class DiffHighlightService
{
    private readonly List<DiffInlineMarker> _inlineMarkers = [];
    private readonly List<DiffTextMarker> _textMarkers = [];

    protected DiffHighlightService(ref string text, bool useGitColoring)
    {
        UseGitColoring = useGitColoring;
        if (useGitColoring)
        {
            text = AnsiDiffTextParser.Parse(text, _textMarkers);
        }
    }

    public DiffLinesInfo LinesInfo { get; protected set; } = new();

    internal IReadOnlyList<DiffInlineMarker> InlineMarkers => _inlineMarkers;

    internal IReadOnlyList<DiffTextMarker> TextMarkers => _textMarkers;

    internal bool UseGitColoring { get; }

    internal bool UseBackgroundColoring
        => !UseGitColoring || GitCommands.AppSettings.ReverseGitColoring.Value;

    protected void SetHighlighting(string text)
    {
        AddInlineDifferenceMarkers(text);
    }

    private void AddInlineDifferenceMarkers(string text)
    {
        DiffLineInfo[] lines = [.. LinesInfo.DiffLines.Values.OrderBy(line => line.LineNumInDiff)];
        int index = 0;
        while (index < lines.Length)
        {
            List<ISegment> removed = GetBlock(lines, DiffLineType.Minus, ref index, found: false);
            if (removed.Count == 0)
            {
                continue;
            }

            List<ISegment> added = GetBlock(lines, DiffLineType.Plus, ref index, found: true);
            if (added.Count == 0)
            {
                continue;
            }

            foreach ((ISegment removedLine, ISegment addedLine) in LinesMatcher.FindLinePairs(GetContent, removed, added))
            {
                AddCommonRanges(removedLine, addedLine, GetContent(removedLine), GetContent(addedLine));
            }
        }

        string GetContent(ISegment segment)
        {
            int contentOffset = Math.Min(1, segment.Length);
            return text.Substring(segment.Offset + contentOffset, segment.Length - contentOffset);
        }
    }

    private void AddCommonRanges(ISegment removedLine, ISegment addedLine, string removed, string added)
    {
        const int contentOffset = 1;
        const int maxLength = 2000;
        ReadOnlySpan<char> removedText = removed.AsSpan(0, Math.Min(removed.Length, maxLength));
        ReadOnlySpan<char> addedText = added.AsSpan(0, Math.Min(added.Length, maxLength));
        int removedOffset = removedLine.Offset + contentOffset;
        int addedOffset = addedLine.Offset + contentOffset;
        (int identicalAtStart, int identicalAtEnd) = AddDifferenceMarkers(
            removedText,
            addedText,
            removedOffset,
            addedOffset);

        AddPair(identicalAtStart, removedOffset, addedOffset);
        AddPair(
            identicalAtEnd,
            removedOffset + removedText.Length - identicalAtEnd,
            addedOffset + addedText.Length - identicalAtEnd);

        void AddPair(int length, int removedMarkerOffset, int addedMarkerOffset)
        {
            if (length <= 0)
            {
                return;
            }

            _inlineMarkers.Add(new DiffInlineMarker(removedMarkerOffset, length, IsRemoved: true));
            _inlineMarkers.Add(new DiffInlineMarker(addedMarkerOffset, length, IsRemoved: false));
        }
    }

    private (int IdenticalAtStart, int IdenticalAtEnd) AddDifferenceMarkers(
        ReadOnlySpan<char> removed,
        ReadOnlySpan<char> added,
        int removedOffset,
        int addedOffset)
    {
        int identicalAtStart = 0;
        int identicalAtEnd = 0;
        if (removed.Length == added.Length && removed.SequenceEqual(added))
        {
            return (removed.Length, 0);
        }

        (string? commonWord, int removedCommonStart, int addedCommonStart) = LinesMatcher.FindBestMatch(
            removed.ToString(),
            added.ToString());
        if (commonWord is not null)
        {
            int identicalLength = commonWord.Length;
            int removedRightStart = removedCommonStart + identicalLength;
            int addedRightStart = addedCommonStart + identicalLength;
            (int rightStartIdentical, identicalAtEnd) = AddDifferenceMarkers(
                removed[removedRightStart..],
                added[addedRightStart..],
                removedOffset + removedRightStart,
                addedOffset + addedRightStart);
            identicalLength += rightStartIdentical;

            (identicalAtStart, int leftEndIdentical) = AddDifferenceMarkers(
                removed[..removedCommonStart],
                added[..addedCommonStart],
                removedOffset,
                addedOffset);
            identicalLength += leftEndIdentical;
            removedCommonStart -= leftEndIdentical;
            addedCommonStart -= leftEndIdentical;

            if (removedCommonStart == identicalAtStart && addedCommonStart == identicalAtStart)
            {
                identicalAtStart += identicalLength;
            }
            else if (removedCommonStart + identicalLength + identicalAtEnd == removed.Length
                     && addedCommonStart + identicalLength + identicalAtEnd == added.Length)
            {
                identicalAtEnd += identicalLength;
            }
            else
            {
                _inlineMarkers.Add(new DiffInlineMarker(removedOffset + removedCommonStart, identicalLength, IsRemoved: true));
                _inlineMarkers.Add(new DiffInlineMarker(addedOffset + addedCommonStart, identicalLength, IsRemoved: false));
            }
        }
        else
        {
            int minimumLength = Math.Min(removed.Length, added.Length);
            while (identicalAtStart < minimumLength && removed[identicalAtStart] == added[identicalAtStart])
            {
                ++identicalAtStart;
            }

            int removedEnd = removed.Length;
            int addedEnd = added.Length;
            while (removedEnd > identicalAtStart
                   && addedEnd > identicalAtStart
                   && removed[removedEnd - 1] == added[addedEnd - 1])
            {
                --removedEnd;
                --addedEnd;
                ++identicalAtEnd;
            }

            int removedDifferentLength = removedEnd - identicalAtStart;
            int addedDifferentLength = addedEnd - identicalAtStart;
            if (removedDifferentLength == 0 && addedDifferentLength > 0)
            {
                _inlineMarkers.Add(new DiffInlineMarker(
                    removedOffset + identicalAtStart,
                    Length: 0,
                    IsRemoved: false,
                    IsAnchor: true));
            }
            else if (removedDifferentLength > 0 && addedDifferentLength == 0)
            {
                _inlineMarkers.Add(new DiffInlineMarker(
                    addedOffset + identicalAtStart,
                    Length: 0,
                    IsRemoved: true,
                    IsAnchor: true));
            }
        }

        return (identicalAtStart, identicalAtEnd);
    }

    private static List<ISegment> GetBlock(
        DiffLineInfo[] lines,
        DiffLineType type,
        ref int index,
        bool found)
    {
        List<ISegment> result = [];
        int gapLines = 0;
        for (; index < lines.Length; ++index)
        {
            DiffLineInfo line = lines[index];
            if (line.LineType != type)
            {
                if (!found)
                {
                    continue;
                }

                if (line.LineType == DiffLineType.Context && gapLines++ < 5)
                {
                    continue;
                }

                break;
            }

            gapLines = 0;
            found = true;
            if (!line.IsMovedLine && line.LineSegment is not null)
            {
                result.Add(line.LineSegment);
            }
        }

        return result;
    }
}
