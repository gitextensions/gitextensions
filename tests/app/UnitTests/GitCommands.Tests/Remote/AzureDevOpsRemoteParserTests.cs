using GitCommands.Remotes;

namespace GitCommandsTests.Remote;
public class AzureDevOpsRemoteParserTests
{
    [TestCase("https://owner@dev.azure.com/owner/project/_git/repo")]
    [TestCase("git@ssh.dev.azure.com:v3/owner/project/repo")]
    [TestCase("https://owner.visualstudio.com/project/_git/repo")]
    [TestCase("owner@vs-ssh.visualstudio.com:v3/owner/project/repo")]
    public void Should_succeed_in_parsing_valid_url(string url)
    {
        AzureDevOpsRemoteParser azureDevOpsRemoteParser = new();
        azureDevOpsRemoteParser.TryExtractAzureDevopsDataFromRemoteUrl(url, out string? owner, out string? project, out string? repository).Should().BeTrue();
        owner.Should().Be("owner");
        project.Should().Be("project");
        repository.Should().Be("repo");

        azureDevOpsRemoteParser.IsValidRemoteUrl(url).Should().BeTrue();
    }

    [Test]
    public void Should_succeed_in_parsing_visualstudio_url_with_DefaultCollection()
    {
        AzureDevOpsRemoteParser azureDevOpsRemoteParser = new();
        string url = "https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_git/vs-green";
        azureDevOpsRemoteParser.TryExtractAzureDevopsDataFromRemoteUrl(url, out string? owner, out string? project, out string? repository).Should().BeTrue();
        owner.Should().Be("devdiv");
        project.Should().Be("DevDiv");
        repository.Should().Be("vs-green");
    }

    [Test]
    public void Should_fail_in_parsing_invalid_url()
    {
        AzureDevOpsRemoteParser azureDevOpsRemoteParser = new();
        string url = "https://owner@dev.bad.com/owner/project/_git/repo";
        azureDevOpsRemoteParser.TryExtractAzureDevopsDataFromRemoteUrl(url, out string? owner, out string? project, out string? repository).Should().BeFalse();
        owner.Should().BeNull();
        project.Should().BeNull();
        repository.Should().BeNull();

        azureDevOpsRemoteParser.IsValidRemoteUrl(url).Should().BeFalse();
    }

    [TestCase("https://owner@dev.azure.com/myorg/myproject/_git/myrepo", "myorg", "myproject", "https://dev.azure.com/myorg/myproject")]
    [TestCase("git@ssh.dev.azure.com:v3/myorg/myproject/myrepo", "myorg", "myproject", "https://dev.azure.com/myorg/myproject")]
    [TestCase("https://myorg.visualstudio.com/myproject/_git/myrepo", "myorg", "myproject", "https://myorg.visualstudio.com/myproject")]
    [TestCase("myorg@vs-ssh.visualstudio.com:v3/myorg/myproject/myrepo", "myorg", "myproject", "https://myorg.visualstudio.com/myproject")]
    public void BuildProjectUrl_should_return_expected_url(string remoteUrl, string owner, string project, string expectedUrl)
    {
        AzureDevOpsRemoteParser.BuildProjectUrl(remoteUrl, owner, project).Should().Be(expectedUrl);
    }

    [Test]
    public void BuildProjectUrl_should_return_null_for_unknown_host()
    {
        AzureDevOpsRemoteParser.BuildProjectUrl("https://github.com/owner/repo.git", "owner", "repo").Should().BeNull();
    }

    [TestCase("https://owner@dev.azure.com/myorg/myproject/_git/myrepo", "myorg", "myproject", "myrepo", "https://dev.azure.com/myorg/myproject/_git/myrepo")]
    [TestCase("git@ssh.dev.azure.com:v3/myorg/myproject/myrepo", "myorg", "myproject", "myrepo", "https://dev.azure.com/myorg/myproject/_git/myrepo")]
    [TestCase("https://myorg.visualstudio.com/myproject/_git/myrepo", "myorg", "myproject", "myrepo", "https://myorg.visualstudio.com/myproject/_git/myrepo")]
    [TestCase("myorg@vs-ssh.visualstudio.com:v3/myorg/myproject/myrepo", "myorg", "myproject", "myrepo", "https://myorg.visualstudio.com/myproject/_git/myrepo")]
    public void BuildRepositoryUrl_should_return_expected_url(string remoteUrl, string owner, string project, string repository, string expectedUrl)
    {
        AzureDevOpsRemoteParser.BuildRepositoryUrl(remoteUrl, owner, project, repository).Should().Be(expectedUrl);
    }

    [Test]
    public void BuildRepositoryUrl_should_return_null_for_unknown_host()
    {
        AzureDevOpsRemoteParser.BuildRepositoryUrl("https://github.com/owner/repo.git", "owner", "project", "repo").Should().BeNull();
    }
}
