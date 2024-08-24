using FluentAssertions;
using GitUI.Editor.Diff;
using ICSharpCode.TextEditor.Document;

namespace GitUITests.Editor.Diff;

[TestFixture]
public class LinesMatcherTests
{
    [Test]
    public void GetAllCombinations([Range(1, 4)] int firstEnd, [Range(1, 4)] int secondEnd)
    {
        bool[,] visited = new bool[firstEnd, secondEnd];
        foreach ((int firstIndex, int secondIndex) in LinesMatcher.GetAllCombinations(firstEnd, secondEnd))
        {
            firstIndex.Should().BeLessThan(firstEnd);
            secondIndex.Should().BeLessThan(secondEnd);
            visited[firstIndex, secondIndex].Should().BeFalse();
            visited[firstIndex, secondIndex] = true;
        }

        for (int i = 0; i < firstEnd; ++i)
        {
            for (int j = 0; j < secondEnd; ++j)
            {
                visited[i, j].Should().BeTrue($"everywhere including at {i}, {j}");
            }
        }
    }

    [Test]
    [TestCase("", new string[] { })]
    [TestCase("a", new string[] { "a" })]
    [TestCase("a-", new string[] { "a" })]
    [TestCase("-a", new string[] { "a" })]
    [TestCase("a bc", new string[] { "a", "bc" })]
    [TestCase("-a bc", new string[] { "a", "bc" })]
    [TestCase("a bc-", new string[] { "a", "bc" })]
    [TestCase("---abc---123---def_7---", new string[] { "abc", "123", "def_7" })]
    public void GetWords(string text, string[] words)
    {
        LinesMatcher.GetWords(text, TextUtilities.IsLetterDigitOrUnderscore)
            .Should().BeEquivalentTo(words);
    }

    [Test]
    public void FindLinePairs_shall_not_search_in_too_large_blocks()
    {
        const int removedCount = 10;
        const int maxCombinations = 100 * 100;
        LineSegment[] removedLines = CreateLines(removedCount);
        LineSegment[] addedLines = CreateLines((maxCombinations / removedCount) + 1);
        Dictionary<ISegment, string> lineTexts = new();
        for (int index = 0; index < removedCount; ++index)
        {
            string lineText = $"line{index}";
            lineTexts[removedLines[index]] = lineText;
            lineTexts[addedLines[index + removedCount]] = lineText;
        }

        IEnumerable<(ISegment RemovedLine, ISegment AddedLine)> pairs = LinesMatcher.FindLinePairs(GetText, removedLines, addedLines);

        int pairIndex = 0;
        foreach ((ISegment removedLine, ISegment addedLine) in pairs)
        {
            removedLine.Should().Be(removedLines[pairIndex]);
            addedLine.Should().Be(addedLines[pairIndex]);
            ++pairIndex;
        }

        return;

        string GetText(ISegment line) => lineTexts.GetValueOrDefault(line, "other line");
    }

    [Test]
    public void FindLinePairs_shall_match_trimmed_lines()
    {
        LineSegment[] removedLines = CreateLines(3);
        LineSegment[] addedLines = CreateLines(5);
        Dictionary<ISegment, string> lineTexts = new()
        {
            { removedLines[0], "r0" },
            { removedLines[1], " trimmed line\t" },
            { removedLines[2], "r2" },
            { addedLines[0], "a0" },
            { addedLines[1], "a1" },
            { addedLines[2], "a2" },
            { addedLines[3], "trimmed line" },
            { addedLines[4], "a4" },
        };

        IEnumerable<(ISegment RemovedLine, ISegment AddedLine)> pairs = LinesMatcher.FindLinePairs(GetText, removedLines, addedLines);

        (ISegment RemovedLine, ISegment AddedLine)[] expectedPairs =
        [
            (removedLines[0], addedLines[0]),
            (removedLines[1], addedLines[3]),
            (removedLines[2], addedLines[4]),
        ];
        pairs.Should().BeEquivalentTo(expectedPairs);

        return;

        string GetText(ISegment line) => lineTexts.GetValueOrDefault(line, "other line");
    }

    [Test]
    public void FindLinePairs_shall_match_lines_whose_common_words_have_maximum_summedup_length()
    {
        LineSegment[] removedLines = CreateLines(4);
        LineSegment[] addedLines = CreateLines(5);
        Dictionary<ISegment, string> lineTexts = new()
        {
            { removedLines[0], "line 0 had some words" },
            { removedLines[1], "line 1 had some more common words" },
            { removedLines[2], " trimmed line\t" },
            { removedLines[3], "r2" },
            { addedLines[0], "some words" },
            { addedLines[1], "line 1 has some words" },
            { addedLines[2], "line 2 has some common words" },
            { addedLines[3], "trimmed line" },
            { addedLines[4], "a4" },
        };

        IEnumerable<(ISegment RemovedLine, ISegment AddedLine)> pairs = LinesMatcher.FindLinePairs(GetText, removedLines, addedLines);

        (ISegment RemovedLine, ISegment AddedLine)[] expectedPairs =
        [
            (removedLines[0], addedLines[1]), // longer common words
            (removedLines[1], addedLines[2]), // most long common words
            (removedLines[2], addedLines[3]), // trimmed
            (removedLines[3], addedLines[4]), // single remaining lines after best match
        ];
        pairs.Should().BeEquivalentTo(expectedPairs);

        return;

        string GetText(ISegment line) => lineTexts.GetValueOrDefault(line, "other line");
    }

    private static LineSegment[] CreateLines(int count)
    {
        LineSegment[] lines = new LineSegment[count];
        for (int index = 0; index < count; ++index)
        {
            lines[index] = new LineSegment();
        }

        return lines;
    }
}
