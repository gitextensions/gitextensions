using GitCommands.Git;

namespace GitCommandsTests.Git;

[TestFixture]
public class DetachedHeadParserTests
{
    [Test]
    public void ShouldExtractOldVersionOfDetachedHeadOutput()
    {
        ClassicAssert.True(DetachedHeadParser.TryParse("(detached from c299581)", out string? sha1));
        ClassicAssert.AreEqual("c299581", sha1);
    }

    [Test]
    public void ShouldExtractNewVersionOfDetachedHeadOutput()
    {
        ClassicAssert.True(DetachedHeadParser.TryParse("(HEAD detached at c299582)", out string? sha1));
        ClassicAssert.AreEqual("c299582", sha1);
    }
}
