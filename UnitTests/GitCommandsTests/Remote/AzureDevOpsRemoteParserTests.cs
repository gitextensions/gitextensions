using FluentAssertions;
using GitCommands.Remotes;
using NUnit.Framework;

namespace GitCommandsTests.Remote
{
    [TestFixture]
    public class AzureDevOpsRemoteParserTests
    {
        [TestCase("https://owner@dev.azure.com/owner/project/_git/repo")]
        [TestCase("git@ssh.dev.azure.com:v3/owner/project/repo")]
        [TestCase("https://owner.visualstudio.com/project/_git/repo")]
        [TestCase("owner@vs-ssh.visualstudio.com:v3/owner/project/repo")]
        public void Should_succeed_in_parsing_valid_url(string url)
        {
            var azureDevOpsRemoteParser = new AzureDevOpsRemoteParser();
            azureDevOpsRemoteParser.TryExtractAzureDevopsDataFromRemoteUrl(url, out var owner, out var project, out var repository).Should().BeTrue();
            owner.Should().Be("owner");
            project.Should().Be("project");
            repository.Should().Be("repo");

            azureDevOpsRemoteParser.IsValidRemoteUrl(url).Should().BeTrue();
        }

        [Test]
        public void Should_fail_in_parsing_invalid_url()
        {
            var azureDevOpsRemoteParser = new AzureDevOpsRemoteParser();
            var url = "https://owner@dev.bad.com/owner/project/_git/repo";
            azureDevOpsRemoteParser.TryExtractAzureDevopsDataFromRemoteUrl(url, out var owner, out var project, out var repository).Should().BeFalse();
            owner.Should().BeNull();
            project.Should().BeNull();
            repository.Should().BeNull();

            azureDevOpsRemoteParser.IsValidRemoteUrl(url).Should().BeFalse();
        }
    }
}
