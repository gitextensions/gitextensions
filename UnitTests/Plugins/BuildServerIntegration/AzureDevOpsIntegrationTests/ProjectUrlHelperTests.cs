using FluentAssertions;
using NUnit.Framework;

namespace AzureDevOpsIntegrationTests
{
    [TestFixture]
    public class ProjectUrlHelperTests
    {
        [TestCase("https://user.visualstudio.com/DefaultCollection/MyProject/_git/MyProject", "https://user.visualstudio.com/MyProject")]
        [TestCase("http://user.visualstudio.com/DefaultCollection/MyProject/_git/MyProject", "http://user.visualstudio.com/MyProject")]
        [TestCase("https://user.visualstudio.com/MyProject/_git/MyProject", "https://user.visualstudio.com/MyProject")]
        [TestCase("http://user.visualstudio.com/MyProject/_git/MyProject", "http://user.visualstudio.com/MyProject")]
        [TestCase("user@vs-ssh.visualstudio.com:v3/user/MyProject/MyProject", "https://user.visualstudio.com/MyProject")]
        [TestCase("https://user@dev.azure.com/user/MyProject/_git/MyProject", "https://dev.azure.com/user/MyProject")]
        [TestCase("http://user@dev.azure.com/user/MyProject/_git/MyProject", "http://dev.azure.com/user/MyProject")]
        [TestCase("git@ssh.dev.azure.com:v3/user/MyProject/MyProject", "https://dev.azure.com/user/MyProject")]
        [TestCase("https://somehost:8080/tfs/DefaultCollection/_git/MyProject", "https://somehost:8080/tfs/DefaultCollection/MyProject")]
        [TestCase("http://somehost:8080/tfs/DefaultCollection/_git/MyProject", "http://somehost:8080/tfs/DefaultCollection/MyProject")]
        [TestCase("https://somehost:8080/tfs/DefaultCollection/MyProject/_git/SecondaryRepo", "https://somehost:8080/tfs/DefaultCollection/MyProject")]
        [TestCase("http://somehost:8080/tfs/DefaultCollection/MyProject/_git/SecondaryRepo", "http://somehost:8080/tfs/DefaultCollection/MyProject")]
        public void TryDetectProjectFromRemoteUrl_should_succeed_with_expected_url_from_valid_remote(string remoteUrl, string expectedProjectUrl)
        {
            var (success, projectUrl) = AzureDevOpsIntegration.Settings.ProjectUrlHelper.TryDetectProjectFromRemoteUrl(remoteUrl);
            success.Should().Be(true);
            projectUrl.Should().BeEquivalentTo(expectedProjectUrl);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("notAurl")]
        [TestCase("https://www.google.de")]
        [TestCase("https://github.com/example/project.git")]
        [TestCase("git@github.com:example/project.git")]
        [TestCase("https://user@bitbucket.org/example/project.git")]
        [TestCase("git@bitbucket.org:example/project.git")]
        [TestCase(@"[TestCase(""http://somehost:8080/tfs/DefaultCollection/MyProject/_git/SecondaryRepo"", ""http://somehost:8080/tfs/DefaultCollection/MyProject"")]")]
        public void TryDetectProjectFromRemoteUrl_should_fail_with_invalid_remote(string remoteUrl)
        {
            var (success, projectUrl) = AzureDevOpsIntegration.Settings.ProjectUrlHelper.TryDetectProjectFromRemoteUrl(remoteUrl);
            success.Should().Be(false);
            projectUrl.Should().BeNullOrEmpty();
        }

        [TestCase("https://user.visualstudio.com/MyProject", "https://user.visualstudio.com/_details/security/tokens")]
        [TestCase("http://user.visualstudio.com/MyProject", "http://user.visualstudio.com/_details/security/tokens")]
        [TestCase("https://dev.azure.com/user/MyProject", "https://dev.azure.com/user/_details/security/tokens")]
        [TestCase("http://dev.azure.com/user/MyProject", "http://dev.azure.com/user/_details/security/tokens")]
        [TestCase("https://somehost:8080/tfs/DefaultCollection/MyProject", "https://somehost:8080/tfs/DefaultCollection/_details/security/tokens")]
        [TestCase("http://somehost:8080/tfs/DefaultCollection/MyProject", "http://somehost:8080/tfs/DefaultCollection/_details/security/tokens")]
        public void TryGetTokenManagementUrlFromProject_should_succeed_with_expected_url_from_valid_projecturl(string projectUrl, string expectedTokenManagementUrl)
        {
            var (success, tokenManagementUrl) = AzureDevOpsIntegration.Settings.ProjectUrlHelper.TryGetTokenManagementUrlFromProject(projectUrl);
            success.Should().Be(true);
            tokenManagementUrl.Should().BeEquivalentTo(expectedTokenManagementUrl);
        }

        /// <remarks>
        /// TryGetTokenManagementUrlFromProject will happlily convert anything that somewhat looks like a project url
        /// in favor of better availability for on premise installations of TFS
        /// </remarks>
        [TestCase(null)]
        [TestCase("")]
        [TestCase("notAurl")]
        [TestCase("https://www.google.de")]
        [TestCase(@"[TestCase(""http://somehost:8080/tfs/DefaultCollection/MyProject"", ""http://somehost:8080/tfs/DefaultCollection/_details/security/tokens"")]")]
        public void TryGetTokenManagementUrlFromProject_should_fail_with_url_that_does_not_look_like_a_project_url(string projectUrl)
        {
            var (success, tokenManagementUrl) = AzureDevOpsIntegration.Settings.ProjectUrlHelper.TryGetTokenManagementUrlFromProject(projectUrl);
            success.Should().Be(false);
            tokenManagementUrl.Should().BeNullOrEmpty();
        }

        [TestCase("https://user.visualstudio.com/MyProject/_build/results?buildId=42&view=results", "https://user.visualstudio.com/MyProject", 42)]
        [TestCase("http://user.visualstudio.com/MyProject/_build/results?buildId=42&view=results", "http://user.visualstudio.com/MyProject", 42)]
        [TestCase("https://dev.azure.com/user/MyProject/_build/results?buildId=42&view=results", "https://dev.azure.com/user/MyProject", 42)]
        [TestCase("http://dev.azure.com/user/MyProject/_build/results?buildId=42&view=results", "http://dev.azure.com/user/MyProject", 42)]
        [TestCase("https://somehost:8080/tfs/DefaultCollection/MyProject/_build/index?buildId=1&view=summary", "https://somehost:8080/tfs/DefaultCollection/MyProject", 1)]
        [TestCase("http://somehost:8080/tfs/DefaultCollection/MyProject/_build/index?buildId=987&view=summary", "http://somehost:8080/tfs/DefaultCollection/MyProject", 987)]
        public void TryParseBuildUrl_should_succeed_with_expected_build_info_from_valid_buildurl(string buildUrl, string expectedProjectUrl, int expectedBuildId)
        {
            var (success, projectUrl, buildId) = AzureDevOpsIntegration.Settings.ProjectUrlHelper.TryParseBuildUrl(buildUrl);
            success.Should().Be(true);
            projectUrl.Should().BeEquivalentTo(expectedProjectUrl);
            buildId.Should().Be(expectedBuildId);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("notAurl")]
        [TestCase("https://www.google.de")]
        [TestCase(@"[TestCase(""https://somehost:8080/tfs/DefaultCollection/MyProject/_build/index?buildId=42&view=summary"", ""https://somehost:8080/tfs/DefaultCollection/MyProject"", 42)]")]
        public void TryParseBuildUrl_should_fail_with_invalid_buildurl(string buildUrl)
        {
            var (success, projectUrl, buildId) = AzureDevOpsIntegration.Settings.ProjectUrlHelper.TryParseBuildUrl(buildUrl);
            success.Should().Be(false);
            projectUrl.Should().BeNullOrEmpty();
            buildId.Should().BeLessThan(0);
        }
    }
}
