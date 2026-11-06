using System.Diagnostics;
using GitCommands;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs.SettingsDialog;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class CheckSettingsLogicTests
{
    private string _directory = null!;
    private string _gitCommand = null!;
    private string? _gitRoot;
    private string? _junction;

    [SetUp]
    public void SetUp()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        _directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.GitDiscovery-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_directory);
        _gitCommand = AppSettings.GitCommandValue;
        _gitRoot = Environment.GetEnvironmentVariable("GITEXT_GIT");
        _junction = null;
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.GitCommandValue = _gitCommand;
        Environment.SetEnvironmentVariable("GITEXT_GIT", _gitRoot);
        if (_junction is not null && Directory.Exists(_junction))
        {
            Directory.Delete(_junction);
        }

        TestDirectory.Delete(_directory);
    }

    [Test]
    public void SolveGitCommand_should_consume_the_installation_environment_and_prefer_bin_before_cmd()
    {
        string executable = FindGit();
        string expected;
        if (OperatingSystem.IsWindows())
        {
            string root = FindInstallationRoot(executable);
            _junction = Path.Combine(_directory, "selected Git");
            ProcessStartInfo link = new("cmd.exe")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            link.ArgumentList.Add("/c");
            link.ArgumentList.Add("mklink");
            link.ArgumentList.Add("/J");
            link.ArgumentList.Add(_junction);
            link.ArgumentList.Add(root);
            using Process process = Process.Start(link)!;
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            process.ExitCode.Should().Be(0, output + error);
            Environment.SetEnvironmentVariable("GITEXT_GIT", _junction + Path.DirectorySeparatorChar);
            expected = Path.Combine(_junction, "bin", "git.exe");
        }
        else
        {
            Environment.SetEnvironmentVariable("GITEXT_GIT", Path.Combine(_directory, "unused Windows installation"));
            expected = executable;
        }

        AppSettings.GitCommandValue = Path.Combine(_directory, "missing-git");
        bool found = CheckSettingsLogic.SolveGitCommand(OperatingSystem.IsWindows() ? null : executable);

        found.Should().BeTrue();
        AppSettings.GitCommandValue.Should().Be(expected);
        new Executable(AppSettings.GitCommand).GetOutput("--version").Should().StartWith("git version ");
    }

    [Test]
    public void SolveGitCommand_should_prefer_the_explicit_existing_executable()
    {
        string executable = FindGit();
        Environment.SetEnvironmentVariable("GITEXT_GIT", Path.Combine(_directory, "missing installation") + Path.DirectorySeparatorChar);
        AppSettings.GitCommandValue = Path.Combine(_directory, "missing-git");

        CheckSettingsLogic.SolveGitCommand(executable).Should().BeTrue();

        AppSettings.GitCommandValue.Should().Be(executable);
        new Executable(AppSettings.GitCommand).GetOutput("--version").Should().StartWith("git version ");
    }

    private static string FindGit()
    {
        PathUtil.TryFindFullPath(OperatingSystem.IsWindows() ? "git.exe" : "git", out string? path)
            .Should().BeTrue("the application test assembly requires Git");
        return path!;
    }

    private static string FindInstallationRoot(string executable)
    {
        DirectoryInfo? directory = new FileInfo(executable).Directory;
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "bin", "git.exe"))
                && File.Exists(Path.Combine(directory.FullName, "cmd", "git.exe")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException($"Git for Windows installation root was not found above {executable}.");
    }
}
