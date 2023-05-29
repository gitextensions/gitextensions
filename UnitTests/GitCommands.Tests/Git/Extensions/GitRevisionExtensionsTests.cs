using FluentAssertions;
using GitCommands.Git.Extensions;
using GitUIPluginInterfaces;

namespace GitCommandsTests.Git.Extensions
{
    [TestFixture]
    public class GitRevisionExtensionsTests
    {
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase(" ", false)]
        [TestCase("0000", false)]
        [TestCase(GitRevision.WorkTreeGuid, true)]
        [TestCase(GitRevision.IndexGuid, true)]
        [TestCase(GitRevision.CombinedDiffGuid, true)]
        public void IsArtificial_tests(string sha1, bool expected)
        {
            sha1.IsArtificial().Should().Be(expected);
        }
    }
}
