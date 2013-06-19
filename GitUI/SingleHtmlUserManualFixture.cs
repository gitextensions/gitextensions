using GitUI.UserManual;
using NUnit.Framework;
using FluentAssertions;

namespace GitExtensionsTest.GitUI
{
    [TestFixture]
    public class SingleHtmlUserManualFixture
    {
        [TestCase(null, "file:///D:/data2/projects/gitextensions/GitExtensionsDoc/build/singlehtml/index.html")]
        [TestCase("merge-conflicts", "file:///D:/data2/projects/gitextensions/GitExtensionsDoc/build/singlehtml/index.html#merge-conflicts")]
        public void GetUrl(string anchor, string expected)
        {
            var sut = new SingleHtmlUserManual(anchor);

            sut.GetUrl().Should().Be(expected);
        }
    }
}
