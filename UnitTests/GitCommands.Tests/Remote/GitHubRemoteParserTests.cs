using FluentAssertions;
using GitCommands.Remotes;
using NUnit.Framework;

namespace GitCommandsTests.Remote
{
    [TestFixture]
    public class GitHubRemoteParserTests
    {
        [TestCase("https://github.com/owner/repo.git")]
        [TestCase("http://github.com/owner/repo.git")]
        [TestCase("https://github.com/owner/repo")]
        [TestCase("ssh://git@github.com/owner/repo.git")]
        [TestCase("git@github.com/owner/repo.git")]
        public void TryExtractGitHubDataFromRemoteUrl(string url)
        {
            new GitHubRemoteParser().TryExtractGitHubDataFromRemoteUrl(url, out var owner, out var repository).Should().BeTrue();
            owner.Should().Be("owner");
            repository.Should().Be("repo");
        }

        [Test]
        public void Should_fail_in_parsing_invalid_url()
        {
            var gitHubRemoteParser = new GitHubRemoteParser();
            var url = "https://owner@dev.bad.com/owner/project/_git/repo";
            gitHubRemoteParser.TryExtractGitHubDataFromRemoteUrl(url, out var owner, out var repository).Should().BeFalse();
            owner.Should().BeNull();
            repository.Should().BeNull();

            gitHubRemoteParser.IsValidRemoteUrl(url).Should().BeFalse();
        }
    }
}
