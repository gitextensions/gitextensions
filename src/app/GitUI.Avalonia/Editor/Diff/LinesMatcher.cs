using System.Diagnostics;
using AvaloniaEdit.Document;
using GitExtensions.Extensibility;

namespace GitUI.Editor.Diff;

internal static class LinesMatcher
{
    internal static IEnumerable<(ISegment RemovedLine, ISegment AddedLine)> FindLinePairs(
        Func<ISegment, string> getText,
        IReadOnlyList<ISegment> removedLines,
        IReadOnlyList<ISegment> addedLines)
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
            for (int index = 0; index < Math.Min(removedLines.Count, addedLines.Count); ++index)
            {
                yield return (removedLines[index], addedLines[index]);
            }

            yield break;
        }

        LineData[] removed = [.. removedLines.Select(line => new LineData(line, getText(line)))];
        LineData[] added = [.. addedLines.Select(line => new LineData(line, getText(line)))];
        foreach ((ISegment, ISegment) pair in FindLinePairs(removed, added))
        {
            yield return pair;
        }
    }

    private static IEnumerable<(ISegment RemovedLine, ISegment AddedLine)> FindLinePairs(LineData[] removed, LineData[] added)
    {
        (int removedIndex, int addedIndex) = FindBestMatch(removed, added);
        if (removedIndex > 0 && addedIndex > 0)
        {
            foreach ((ISegment, ISegment) pair in FindLinePairs(removed[..removedIndex], added[..addedIndex]))
            {
                yield return pair;
            }
        }

        yield return (removed[removedIndex].Line, added[addedIndex].Line);

        ++removedIndex;
        ++addedIndex;
        if (removedIndex < removed.Length && addedIndex < added.Length)
        {
            foreach ((ISegment, ISegment) pair in FindLinePairs(removed[removedIndex..], added[addedIndex..]))
            {
                yield return pair;
            }
        }
    }

    private static (int RemovedIndex, int AddedIndex) FindBestMatch(LineData[] removed, LineData[] added)
    {
        // first, search longest match of trimmed lines, i.e. detect indented lines
        (LineData longestMatchingRemoved, int matchingAddedIndex)
            = removed.Select(r => (r, addedIndex: added.IndexOf(a => a.Trimmed == r.Trimmed)))
                     .MaxBy(pair => pair.addedIndex < 0 ? -1 : pair.r.Trimmed.Length);
        if (matchingAddedIndex >= 0)
        {
            return (Array.IndexOf(removed, longestMatchingRemoved), matchingAddedIndex);
        }

        // then match lines whose common words have the maximum summed-up length
        int bestRemovedIndex = 0;
        int bestAddedIndex = 0;
        float bestScore = -1;
        foreach ((int removedIndex, int addedIndex) in GetAllCombinations(removed.Length, added.Length))
        {
            float score = GetWordMatchScore(removed[removedIndex], added[addedIndex]);
            if (score > bestScore)
            {
                bestScore = score;
                bestRemovedIndex = removedIndex;
                bestAddedIndex = addedIndex;
                if (score >= 1)
                {
                    break;
                }
            }
        }

        const float insignificantWordMatchScore = 0.1f;
        return bestScore <= insignificantWordMatchScore ? (0, 0) : (bestRemovedIndex, bestAddedIndex);

        static float GetWordMatchScore(LineData removedLine, LineData addedLine)
        {
            if (removedLine.Words.Count == 0 || addedLine.Words.Count == 0)
            {
                return -1;
            }

            return (float)removedLine.Words.Intersect(addedLine.Words).Sum(word => word.Length)
                / Math.Max(removedLine.WordsTotalLength, addedLine.WordsTotalLength);
        }
    }

    internal static (string? CommonWord, int StartIndexRemoved, int StartIndexAdded) FindBestMatch(
        string removed,
        string added)
    {
        (string Word, int StartIndex) notFound = ("", -1);
        (string Word, int StartIndex)[] removedWords = [.. GetWords(removed)];
        (string Word, int StartIndex)[] addedWords = [.. GetWords(added)];
        (string? commonWord, int addedStart) = addedWords
            .IntersectBy(removedWords.Select(SelectWord), SelectWord)
            .Union([notFound])
            .MaxBy(pair => pair.Word.Length);
        if (addedStart != notFound.StartIndex)
        {
            return (commonWord, removedWords.First(pair => pair.Word == commonWord).StartIndex, addedStart);
        }

        (string Word, int StartIndex)[] removedSubwords = [.. GetSubwords(removedWords)];
        (commonWord, addedStart) = GetSubwords(addedWords)
            .IntersectBy(removedSubwords.Select(SelectWord), SelectWord)
            .Union([notFound])
            .MaxBy(pair => pair.Word.Length);
        return addedStart == notFound.StartIndex
            ? (null, 0, 0)
            : (commonWord, removedSubwords.First(pair => pair.Word == commonWord).StartIndex, addedStart);
    }

    /// <summary>
    ///  Iterates all combinations of indices - starting with (0,0), (1,0), (0,1), (2,0), (1,1), ...
    /// </summary>
    /// <returns>an enumeration of the index pairs</returns>
    internal static IEnumerable<(int FirstIndex, int SecondIndex)> GetAllCombinations(int firstEnd, int secondEnd)
    {
        // upper left half including prinicipal diagonal
        for (int diagonalIndex = 0; diagonalIndex < firstEnd; ++diagonalIndex)
        {
            int diagonalEnd = Math.Min(diagonalIndex + 1, secondEnd);
            for (int secondIndex = 0; secondIndex < diagonalEnd; ++secondIndex)
            {
                yield return (diagonalIndex - secondIndex, secondIndex);
            }
        }

        // lower right half
        for (int diagonalIndex = 1; diagonalIndex < secondEnd; ++diagonalIndex)
        {
            int diagonalEnd = Math.Min(firstEnd + diagonalIndex, secondEnd);
            for (int secondIndex = diagonalIndex; secondIndex < diagonalEnd; ++secondIndex)
            {
                yield return (firstEnd - 1 + diagonalIndex - secondIndex, secondIndex);
            }
        }
    }

    internal static IEnumerable<(string Word, int StartIndex)> GetSubwords(string word)
    {
        if (word.Length == 0)
        {
            yield break;
        }

        int start = 0;
        bool previousUpper = char.IsUpper(word[0]);
        for (int index = 0; index < word.Length; ++index)
        {
            bool currentUpper = char.IsUpper(word[index]);
            if (previousUpper != currentUpper)
            {
                previousUpper = currentUpper;
                if (currentUpper)
                {
                    // emit previous word, but no single '_'
                    if (!(index == 1 && !char.IsLetterOrDigit(word[0])))
                    {
                        yield return (word[start..index], start);
                    }

                    start = index;
                }
            }

            // end word at '_', but join preceding '_' to first word
            if (index > 0 && !char.IsLetterOrDigit(word[index]))
            {
                if (start < index && char.IsLetterOrDigit(word[index - 1]))
                {
                    yield return (word[start..index], start);
                }

                start = index + 1;
                previousUpper = true;
            }
        }

        if (start < word.Length && !(word.Length == 1 && !char.IsLetterOrDigit(word[0])))
        {
            yield return (word[start..], start);
        }
    }

    internal static IEnumerable<(string Word, int StartIndex)> GetSubwords(IEnumerable<(string Word, int StartIndex)> words)
    {
        foreach ((string Word, int StartIndex) word in words)
        {
            foreach ((string Word, int StartIndex) subword in GetSubwords(word.Word))
            {
                yield return (subword.Word, subword.StartIndex + word.StartIndex);
            }
        }
    }

    internal static IEnumerable<(string Word, int StartIndex)> GetWords(string text) => GetWords(text, IsWordChar);

    internal static IEnumerable<(string Word, int StartIndex)> GetWords(string text, Func<char, bool> isWordChar)
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
                    yield return (text[start..end], start);
                    start = end + 1;
                    break;
                }
            }
        }
    }

    // Cross-platform framework constraint: the original TextUtilities helper belongs to WinForms.
    internal static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    internal static string SelectWord((string Word, int StartIndex) pair) => pair.Word;

    internal static int SelectStartIndex((string Word, int StartIndex) pair) => pair.StartIndex;

    [DebuggerDisplay("{Line.Offset}: {Trimmed}")]
    private readonly struct LineData
    {
        internal ISegment Line { get; }
        internal string Full { get; }
        internal string Trimmed { get; }
        internal IReadOnlySet<string> Words { get; }
        internal int WordsTotalLength { get; }

        internal LineData(ISegment line, string text)
        {
            Line = line;
            Full = text;
            Trimmed = text.Trim();
            Words = GetWords(Trimmed).Select(SelectWord).ToHashSet();
            WordsTotalLength = Words.Sum(word => word.Length);
        }
    }
}
