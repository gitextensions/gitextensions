using GitExtUtils;

namespace GitExtUtilsTests;

[TestFixture]
public sealed class DisplayWithSuffixUpdaterTests
{
    [Test]
    public void FromInitialValue()
    {
        ClassicAssert.AreEqual(DisplayWithSuffixUpdater.UpdateSuffixWithinParenthesis("a", "b"), "a\u00A0(b)");
    }

    [Test]
    public void FromValueContainingAlreadyTheSuffix()
    {
        ClassicAssert.AreEqual(DisplayWithSuffixUpdater.UpdateSuffixWithinParenthesis("a (b)", "c"), "a\u00A0(c)");
    }
}
