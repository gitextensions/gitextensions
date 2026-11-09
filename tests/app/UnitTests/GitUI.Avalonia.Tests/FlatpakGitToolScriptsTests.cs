using System.Diagnostics;
using System.Runtime.CompilerServices;
using CommonTestUtils;
using GitCommands;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FlatpakGitToolScriptsTests
{
    [TestCase("diff-command")]
    [TestCase("diff-path")]
    [TestCase("editor")]
    [TestCase("sequence-editor")]
    [TestCase("denied")]
    public async Task Packaged_scripts_should_translate_actual_Git_tool_launches_without_changing_configuration(string scenario)
    {
        string root = Path.Combine(Path.GetTempPath(), "GitExtensions.ToolScripts-" + Guid.NewGuid().ToString("N"));
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
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            request.ArgumentList.Add(Path.Combine(source, "FlatpakGitToolScriptsTests.sh"));
            request.ArgumentList.Add(root);
            request.ArgumentList.Add(Path.GetFullPath(Path.Combine(source, "..", "..", "..", "..", "eng", "avalonia", "flatpak")));
            request.ArgumentList.Add(git!);
            request.ArgumentList.Add(scenario);
            using Process process = Process.Start(request)!;
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
            Path.GetFileName(root).Should().StartWith("GitExtensions.ToolScripts-");
            TestDirectory.Delete(root);
        }
    }

    private static string GetSourceDirectory([CallerFilePath] string path = "")
        => Path.GetDirectoryName(path)!;
}
