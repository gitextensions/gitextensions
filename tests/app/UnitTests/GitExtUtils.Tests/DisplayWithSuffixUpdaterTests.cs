using GitExtUtils;

namespace GitExtUtilsTests;
public sealed class DisplayWithSuffixUpdaterTests
{
    [Test]
    public void FromInitialValue()
    {
        "a\u00A0(b)".Should().Be(DisplayWithSuffixUpdater.UpdateSuffixWithinParenthesis("a", "b"));
    }

    [Test]
    public void FromValueContainingAlreadyTheSuffix()
    {
        "a\u00A0(c)".Should().Be(DisplayWithSuffixUpdater.UpdateSuffixWithinParenthesis("a (b)", "c"));
    }
}
