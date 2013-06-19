using FluentAssertions;
using GitUI.UserManual;
using NUnit.Framework;

namespace GitExtensionsTest.GitUI
{
    [TestFixture]
    public class StandardHtmlUserManualFixture
    {
        [TestCase(null, null, "https://gitextensions.readthedocs.org/en/latest//")] // both null makes no sense atm
        [TestCase("merge_conflicts", null, "https://gitextensions.readthedocs.org/en/latest/merge_conflicts/")]
        [TestCase("merge_conflicts", "merge-conflicts", "https://gitextensions.readthedocs.org/en/latest/merge_conflicts/#merge-conflicts")]
        public void GetUrl(string subFolder, string anchor, string expected)
        {
            var sut = new StandardHtmlUserManual(subFolder, anchor);

            sut.GetUrl().Should().Be(expected);
        }
    }
}