using AvaloniaEdit.Document;
using GitUI.Editor.Diff;

namespace GitExtensionsTests;

[TestFixture]
public sealed class LinesMatcherParityTests
{
    [Test]
    public void Word_and_subword_helpers_should_preserve_the_original_contract()
    {
        (string Word, int StartIndex)[] words = [.. LinesMatcher.GetWords("---abc---123---def_7---", LinesMatcher.IsWordChar)];
        words.Select(LinesMatcher.SelectWord).Should().Equal("abc", "123", "def_7");
        words.Select(LinesMatcher.SelectStartIndex).Should().Equal(3, 9, 15);

        (string Word, int StartIndex)[] subwords = [.. LinesMatcher.GetSubwords("CAPITALSwithsuffixNext_Upper_lower")];
        subwords.Select(LinesMatcher.SelectWord).Should().Equal("CAPITALSwithsuffix", "Next", "Upper", "lower");
        subwords.Select(LinesMatcher.SelectStartIndex).Should().Equal(0, 18, 23, 29);
    }

    [Test]
    public void Pairing_should_match_trimmed_lines_before_word_scores()
    {
        ISegment[] removed = [new SimpleSegment(0, 1), new SimpleSegment(1, 1), new SimpleSegment(2, 1)];
        ISegment[] added = [new SimpleSegment(3, 1), new SimpleSegment(4, 1), new SimpleSegment(5, 1), new SimpleSegment(6, 1), new SimpleSegment(7, 1)];
        Dictionary<ISegment, string> text = new()
        {
            [removed[0]] = "r0",
            [removed[1]] = " trimmed line\t",
            [removed[2]] = "r2",
            [added[0]] = "a0",
            [added[1]] = "a1",
            [added[2]] = "a2",
            [added[3]] = "trimmed line",
            [added[4]] = "a4",
        };

        (ISegment RemovedLine, ISegment AddedLine)[] pairs =
            [.. LinesMatcher.FindLinePairs(segment => text.GetValueOrDefault(segment, "other line"), removed, added)];

        pairs.Should().Equal((removed[0], added[0]), (removed[1], added[3]), (removed[2], added[4]));
    }
}
