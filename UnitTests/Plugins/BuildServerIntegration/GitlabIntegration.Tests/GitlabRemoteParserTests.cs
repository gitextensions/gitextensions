using FluentAssertions;
using GitExtensions.Plugins.GitlabIntegration.Settings;

namespace GitlabIntegrationTests
{
    [TestFixture]
    internal class GitlabRemoteParserTests
    {
        [TestCase("https://github.com/owner/repo.git/")]
        [TestCase("http://github.com/owner/repo.git/")]
        [TestCase("https://github.com/owner/repo/")]
        [TestCase("ssh://git@github.com/owner/repo.git/")]
        [TestCase("git@github.com/owner/repo.git/")]
        public void TryExtractGitHubDataFromRemoteUrl(string url)
        {
            new GitlabRemoteParser().TryExtractGitlabDataFromRemoteUrl(url, out var host, out var owner, out var repository).Should().BeTrue();
            host.Should().Be("github.com");
            owner.Should().Be("owner");
            repository.Should().Be("repo");
        }

        [Test]
        public void Should_fail_in_parsing_invalid_url()
        {
            GitlabRemoteParser gitHubRemoteParser = new();
            var url = "https://owner@dev.bad.com/owner/project/_git/repo";
            gitHubRemoteParser.TryExtractGitlabDataFromRemoteUrl(url, out var host, out var owner, out var repository).Should().BeFalse();
            owner.Should().BeNull();
            repository.Should().BeNull();

            gitHubRemoteParser.IsValidRemoteUrl(url).Should().BeFalse();
        }
    }
}
