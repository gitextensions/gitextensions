using System.Diagnostics;
using GitExtensions.Extensibility;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

internal static class LinesMatcher
{
    internal static IEnumerable<(ISegment RemovedLine, ISegment AddedLine)> FindLinePairs(
        Func<ISegment, string> getText, IReadOnlyList<ISegment> removedLines, IReadOnlyList<ISegment> addedLines)
    {
        int numberOfCombinations = removedLines.Count * addedLines.Count;
        if (numberOfCombinations < 1)
        {
            yield break;
        }

        // Do not try to match more lines than usually visible at the same time, because it costs O(n^2) operations
        const int maxCombinations = 100 * 100;
        if (numberOfCombinations == 1 || numberOfCombinations > maxCombinations)
        {
            int minCount = Math.Min(removedLines.Count, addedLines.Count);
            for (int i = 0; i < minCount; ++i)
            {
                yield return (removedLines[i], addedLines[i]);
            }

            yield break;
        }

        LineData[] removed = removedLines.Select(line => new LineData(line, getText(line))).ToArray();
        LineData[] added = addedLines.Select(line => new LineData(line, getText(line))).ToArray();

        foreach ((ISegment, ISegment) linePair in FindLinePairs(removed, added))
        {
            yield return linePair;
        }
    }

    private static IEnumerable<(ISegment RemovedLine, ISegment AddedLine)> FindLinePairs(LineData[] removed, LineData[] added)
    {
        (int removedIndex, int addedIndex) = FindBestMatch(removed, added);

        if (removedIndex > 0 && addedIndex > 0)
        {
            foreach ((ISegment, ISegment) linePair in FindLinePairs(removed[0..removedIndex], added[0..addedIndex]))
            {
                yield return linePair;
            }
        }

        yield return (removed[removedIndex].Line, added[addedIndex].Line);

        ++removedIndex;
        ++addedIndex;
        if (removedIndex < removed.Length && addedIndex < added.Length)
        {
            foreach ((ISegment, ISegment) linePair in FindLinePairs(removed[removedIndex..], added[addedIndex..]))
            {
                yield return linePair;
            }
        }
    }

    private static (int RemovedIndex, int AddedIndex) FindBestMatch(LineData[] removed, LineData[] added)
    {
        return (0, 0);
    }

    internal static IEnumerable<string> GetWords(string text, Func<char, bool> isWordChar)
    {
        int length = text.Length;
        int start = 0;
        while (true)
        {
            for (; ; ++start)
            {
                if (start >= length)
                {
                    // no (more) word found, exit function
                    yield break;
                }

                if (isWordChar(text[start]))
                {
                    break;
                }
            }

            // start of word found

            for (int end = start + 1; ; ++end)
            {
                if (end >= length || !isWordChar(text[end]))
                {
                    // word end found, yield and find next word
                    yield return text[start..end];
                    start = end + 1;
                    break;
                }
            }
        }
    }

    [DebuggerDisplay("{Line.Offset}: {Trimmed}")]
    private readonly struct LineData
    {
        internal ISegment Line { get; }
        internal string Full { get; }
        internal string Trimmed { get; }
        internal IReadOnlySet<string> Words { get; }

        internal LineData(ISegment line, string text)
        {
            Line = line;
            Full = text;
            Trimmed = text.Trim();
            Words = GetWords(Trimmed, TextUtilities.IsLetterDigitOrUnderscore).ToHashSet();
        }
    }
}
