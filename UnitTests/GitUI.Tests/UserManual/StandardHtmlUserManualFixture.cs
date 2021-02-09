using FluentAssertions;
using GitUI.UserManual;
using NUnit.Framework;

namespace GitUITests.UserManual
{
    [TestFixture]
    public class StandardHtmlUserManualFixture
    {
        [TestCase(null, null, "https://git-extensions-documentation.readthedocs.org/")] // both null makes no sense atm
        [TestCase("merge_conflicts", null, "https://git-extensions-documentation.readthedocs.org/merge_conflicts.html")]
        [TestCase("merge_conflicts", "merge-conflicts", "https://git-extensions-documentation.readthedocs.org/merge_conflicts.html#merge-conflicts")]
        public void GetUrl(string subFolder, string anchor, string expected)
        {
            var sut = new StandardHtmlUserManual(subFolder, anchor);

            sut.GetUrl().Should().Be(expected);
        }
    }
}
