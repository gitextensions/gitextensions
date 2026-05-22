using GitCommands.Git;

namespace GitCommandsTests.Git;
public class DetachedHeadParserTests
{
    [Test]
    public void ShouldExtractOldVersionOfDetachedHeadOutput()
    {
        DetachedHeadParser.TryParse("(detached from c299581)", out string? sha1).Should().BeTrue();
        sha1.Should().Be("c299581");
    }

    [Test]
    public void ShouldExtractNewVersionOfDetachedHeadOutput()
    {
        DetachedHeadParser.TryParse("(HEAD detached at c299582)", out string? sha1).Should().BeTrue();
        sha1.Should().Be("c299582");
    }
}
