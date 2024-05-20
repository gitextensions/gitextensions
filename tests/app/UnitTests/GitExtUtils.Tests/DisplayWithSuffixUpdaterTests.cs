using GitExtUtils;

namespace GitExtUtilsTests
{
    [TestFixture]
    public sealed class DisplayWithSuffixUpdaterTests
    {
        [Test]
        public void FromInitialValue()
        {
            Assert.AreEqual(DisplayWithSuffixUpdater.UpdateSuffixWithinParenthesis("a", "b"), "a\u00A0(b)");
        }

        [Test]
        public void FromValueContainingAlreadyTheSuffix()
        {
            Assert.AreEqual(DisplayWithSuffixUpdater.UpdateSuffixWithinParenthesis("a (b)", "c"), "a\u00A0(c)");
        }
    }
}
