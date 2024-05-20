using FluentAssertions;
using GitCommands.Remotes;

namespace GitCommandsTests.Remote
{
    [TestFixture]
    public class GitHostingRemoteParserTests
    {
        [TestCase("https://github.com/owner/repo.git", "github.com")]
        [TestCase("http://github.com/owner/repo.git", "github.com")]
        [TestCase("https://github.com/owner/repo", "github.com")]
        [TestCase("ssh://git@github.com/owner/repo.git", "github.com")]
        [TestCase("git@github.com/owner/repo.git", "github.com")]
        [TestCase("git@framagit.org:owner/repo.git", "framagit.org")]
        [TestCase("https://framagit.org/owner/repo.git", "framagit.org")]
        [TestCase("https://github.com/owner/repo.git", "github.com")]
        [TestCase("git@github.com:owner/repo.git", "github.com")]
        [TestCase("https://git.sr.ht/owner/repo", "git.sr.ht")]
        [TestCase("git@git.sr.ht:owner/repo", "git.sr.ht")]
        [TestCase("https://gitlab.freedesktop.org/owner/repo.git", "gitlab.freedesktop.org")]
        [TestCase("git@gitlab.freedesktop.org:owner/repo.git", "gitlab.freedesktop.org")]
        public void TryExtractGitHubDataFromRemoteUrl(string url, string expectedGitHosting)
        {
            new GitHostingRemoteParser().TryExtractGitHostingDataFromRemoteUrl(url, out string? gitHosting, out string? owner, out string? repository).Should().BeTrue();
            gitHosting.Should().Be(expectedGitHosting);
            owner.Should().Be("owner");
            repository.Should().Be("repo");
        }

        [Test]
        public void Should_fail_in_parsing_invalid_url()
        {
            GitHostingRemoteParser gitHubRemoteParser = new();
            string url = "https://owner@dev.bad.com/owner/project/_git/repo";
            gitHubRemoteParser.TryExtractGitHostingDataFromRemoteUrl(url, out string? gitHosting, out string? owner, out string? repository).Should().BeFalse();
            gitHosting.Should().BeNull();
            owner.Should().BeNull();
            repository.Should().BeNull();

            gitHubRemoteParser.IsValidRemoteUrl(url).Should().BeFalse();
        }
    }
}
