using System.Net;
using GitExtensions.Extensibility.Settings;

namespace GitExtensions.ExtensibilityTests;

[NonParallelizable]
public sealed class CredentialsManagerNonWindowsTests
{
    [Test]
    public void Fill_should_return_credential_without_exposing_the_setting_name()
    {
        RecordingCredentialProcess process = new()
        {
            Handler = (_, _, _) => new GitCredentialProcessResult(
                0,
                "protocol=https\nhost=example.invalid\nusername=nikola\npassword=p=a=s=s\n\n"),
        };
        string workingDirectory = Path.GetTempPath();
        CredentialsManager manager = new(() => workingDirectory, process);
        string settingName = $"test-{Guid.NewGuid():N}";

        NetworkCredential result = manager.GetCredentialOrDefault(
            SettingLevel.Global,
            settingName,
            new NetworkCredential("default", "default"));

        result.UserName.Should().Be("nikola");
        result.Password.Should().Be("p=a=s=s");
        process.Calls.Should().ContainSingle();
        process.Calls[0].Operation.Should().Be("fill");
        process.Calls[0].Input.Should().Contain("protocol=https\n");
        process.Calls[0].Input.Should().Contain(".credentials.gitextensions\n");
        process.Calls[0].Input.Should().NotContain(settingName);
        process.Calls[0].WorkingDirectory.Should().Be(workingDirectory);
    }

    [Test]
    public void Pending_changes_should_be_visible_and_saved_with_approve_and_reject()
    {
        RecordingCredentialProcess process = new();
        CredentialsManager manager = new(() => Path.GetTempPath(), process);
        string approveName = $"approve-{Guid.NewGuid():N}";
        string rejectName = $"reject-{Guid.NewGuid():N}";
        NetworkCredential expected = new("user", "secret");

        manager.SetCredentials(SettingLevel.Global, approveName, expected);
        manager.SetCredentials(SettingLevel.Global, rejectName, value: null);

        manager.GetCredentialOrDefault(
                SettingLevel.Global,
                approveName,
                new NetworkCredential())
            .Should().BeSameAs(expected);
        manager.GetCredentialOrDefault(
                SettingLevel.Global,
                rejectName,
                new NetworkCredential("default", "default"))
            .UserName.Should().Be("default");
        process.Calls.Should().BeEmpty();

        CredentialsManager settingsContainerManager = new(getWorkingDir: null, process);
        settingsContainerManager.Save();

        process.Calls.Should().HaveCount(2);
        CredentialProcessCall approve = process.Calls.Single(call => call.Operation == "approve");
        approve.Input.Should().Contain("username=user\n");
        approve.Input.Should().Contain("password=secret\n");
        approve.WorkingDirectory.Should().Be(Path.GetTempPath());
        CredentialProcessCall reject = process.Calls.Single(call => call.Operation == "reject");
        reject.Input.Should().NotContain("username=");
        reject.Input.Should().NotContain("password=");
    }

    [Test]
    public void Missing_local_working_directory_or_failed_fill_should_return_default()
    {
        RecordingCredentialProcess process = new()
        {
            Handler = (_, _, _) => new GitCredentialProcessResult(1, string.Empty),
        };
        NetworkCredential expected = new("default", "secret");
        CredentialsManager missingDirectoryManager = new(() => null, process);

        missingDirectoryManager.GetCredentialOrDefault(SettingLevel.Local, "local", expected)
            .Should().BeSameAs(expected);
        process.Calls.Should().BeEmpty();

        CredentialsManager failedFillManager = new(() => Path.GetTempPath(), process);
        failedFillManager.GetCredentialOrDefault(SettingLevel.Global, "global", expected)
            .Should().BeSameAs(expected);
        process.Calls.Should().ContainSingle();
    }

    private sealed class RecordingCredentialProcess : IGitCredentialProcess
    {
        internal Func<string, string, string?, GitCredentialProcessResult> Handler { get; init; }
            = (_, _, _) => new GitCredentialProcessResult(0, string.Empty);

        internal List<CredentialProcessCall> Calls { get; } = [];

        public GitCredentialProcessResult Run(string operation, string input, string? workingDirectory)
        {
            Calls.Add(new CredentialProcessCall(operation, input, workingDirectory));
            return Handler(operation, input, workingDirectory);
        }
    }

    private sealed record CredentialProcessCall(string Operation, string Input, string? WorkingDirectory);
}
