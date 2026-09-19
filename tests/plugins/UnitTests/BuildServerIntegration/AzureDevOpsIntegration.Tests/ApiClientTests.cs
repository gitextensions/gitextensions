using System.Diagnostics;
using AzureDevOpsIntegration;

namespace AzureDevOpsIntegrationTests;

public class ApiClientTests
{
    [Test]
    public void CreateCredentialFillStartInfo_should_not_let_the_credential_helper_prompt()
    {
        ProcessStartInfo startInfo = ApiClient.CreateCredentialFillStartInfo(gitExecutable: null);

        startInfo.Arguments.Should().Be("-c credential.interactive=false credential fill");
        startInfo.Environment["GIT_TERMINAL_PROMPT"].Should().Be("0");
        startInfo.Environment["GCM_INTERACTIVE"].Should().Be("never");
    }

    [Test]
    public void CreateCredentialFillStartInfo_should_leave_the_ssh_askpass_variables_alone()
    {
        // Prompting for an ssh passphrase is forced deliberately for interactive git operations
        ProcessStartInfo startInfo = ApiClient.CreateCredentialFillStartInfo(gitExecutable: null);

        startInfo.Environment.Keys.Should().NotContain(["SSH_ASKPASS", "SSH_ASKPASS_REQUIRE", "DISPLAY"]);
    }

    [TestCase(null, "git")]
    [TestCase(@"C:\Program Files\Git\cmd\git.exe", @"C:\Program Files\Git\cmd\git.exe")]
    public void CreateCredentialFillStartInfo_should_use_the_configured_git_executable(string? gitExecutable, string expected)
    {
        ApiClient.CreateCredentialFillStartInfo(gitExecutable).FileName.Should().Be(expected);
    }

    [Test]
    public void CreateCredentialFillStartInfo_should_not_show_a_window()
    {
        ProcessStartInfo startInfo = ApiClient.CreateCredentialFillStartInfo(gitExecutable: null);

        startInfo.CreateNoWindow.Should().BeTrue();
        startInfo.UseShellExecute.Should().BeFalse();
        startInfo.RedirectStandardInput.Should().BeTrue();
        startInfo.RedirectStandardOutput.Should().BeTrue();
    }
}
