using FluentAssertions;
using GitExtensions.Plugins.GitlabIntegration.Settings;

namespace GitlabIntegrationTests
{
    [TestFixture]
    internal class GitlabRemoteParserTests
    {
        [TestCase("https://gitlab.com/owner/repo.git/")]
        [TestCase("http://gitlab.com/owner/repo.git/")]
        [TestCase("https://gitlab.com/owner/repo/")]
        [TestCase("ssh://git@gitlab.com/owner/repo.git/")]
        [TestCase("git@gitlab.com/owner/repo.git/")]
        public void TryExtractGitlabDataFromGitlabComRemoteUrl(string url)
        {
            new GitlabRemoteParser().TryExtractGitlabDataFromRemoteUrl(url, out var host, out var owner, out var repository).Should().BeTrue();
            host.Should().Be("gitlab.com");
            owner.Should().Be("owner");
            repository.Should().Be("repo");
        }

        [TestCase("https://example.com/owner/repo.git/", "example.com")]
        [TestCase("http://gitlab.example.com/owner/repo.git/", "gitlab.example.com")]
        [TestCase("https://repo.example.com/owner/repo/", "repo.example.com")]
        [TestCase("https://192.168.1.11/owner/repo/", "192.168.1.11")]
        [TestCase("https://repo.192.168.1.11:5896/owner/repo/", "repo.192.168.1.11:5896")]
        public void TryExtractGitlabDataFromSelfHostedRemoteUrl(string url, string expectedHost)
        {
            new GitlabRemoteParser().TryExtractGitlabDataFromRemoteUrl(url, out var host, out var owner, out var repository).Should().BeTrue();
            host.Should().Be(expectedHost);
            owner.Should().Be("owner");
            repository.Should().Be("repo");
        }

        [Test]
        public void Should_fail_in_parsing_invalid_url()
        {
            GitlabRemoteParser gitlabRemoteParser = new();
            string url = "https://owner@dev.bad.com/owner/project/_git/repo";
            gitlabRemoteParser.TryExtractGitlabDataFromRemoteUrl(url, out var host, out var owner, out var repository).Should().BeFalse();
            owner.Should().BeNull();
            repository.Should().BeNull();

            gitlabRemoteParser.IsValidRemoteUrl(url).Should().BeFalse();
        }
    }
}
