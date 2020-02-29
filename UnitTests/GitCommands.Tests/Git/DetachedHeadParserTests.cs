using GitCommands.Git;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class DetachedHeadParserTests
    {
        [Test]
        public void ShouldExtractOldVersionOfDetachedHeadOutput()
        {
            Assert.True(DetachedHeadParser.TryParse("(detached from c299581)", out var sha1));
            Assert.AreEqual("c299581", sha1);
        }

        [Test]
        public void ShouldExtractNewVersionOfDetachedHeadOutput()
        {
            Assert.True(DetachedHeadParser.TryParse("(HEAD detached at c299582)", out var sha1));
            Assert.AreEqual("c299582", sha1);
        }
    }
}