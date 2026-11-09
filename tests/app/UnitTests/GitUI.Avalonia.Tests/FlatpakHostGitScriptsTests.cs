using System.Diagnostics;
using System.Runtime.CompilerServices;
using CommonTestUtils;
using GitCommands;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FlatpakHostGitScriptsTests
{
    [TestCase("forward")]
    [TestCase("missing")]
    [TestCase("old")]
    [TestCase("denied")]
    [TestCase("minimum")]
    [TestCase("failed-command")]
    [TestCase("git-error")]
    [TestCase("cached-bundled-path")]
    public async Task Packaged_launcher_should_forward_Git_or_fall_back_only_before_it_starts(string scenario)
    {
        string root = Path.Combine(Path.GetTempPath(), "GitExtensions.HostGit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            PathUtil.TryFindFullPath(OperatingSystem.IsWindows() ? "git.exe" : "git", out string? git).Should().BeTrue();
            string bash = OperatingSystem.IsWindows()
                ? Path.GetFullPath(Path.Combine(Path.GetDirectoryName(git)!, "..", "bin", "bash.exe")) : "/bin/bash";
            string source = GetSourceDirectory();
            ProcessStartInfo request = new(bash)
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            request.ArgumentList.Add(Path.Combine(source, "FlatpakHostGitScriptsTests.sh"));
            request.ArgumentList.Add(root);
            request.ArgumentList.Add(Path.GetFullPath(Path.Combine(source, "..", "..", "..", "..", "eng", "avalonia", "flatpak")));
            request.ArgumentList.Add(git!);
            request.ArgumentList.Add(scenario);
            using Process process = Process.Start(request)!;
            process.StandardInput.Close();
            Task<string> output = process.StandardOutput.ReadToEndAsync();
            Task<string> error = process.StandardError.ReadToEndAsync();
            using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(45));
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
                throw;
            }

            process.ExitCode.Should().Be(0, "{0}\n{1}", await output, await error);
        }
        finally
        {
            Path.GetDirectoryName(root).Should().Be(Path.TrimEndingDirectorySeparator(Path.GetTempPath()));
            Path.GetFileName(root).Should().StartWith("GitExtensions.HostGit-");
            TestDirectory.Delete(root);
        }
    }

    private static string GetSourceDirectory([CallerFilePath] string path = "")
        => Path.GetDirectoryName(path)!;
}
