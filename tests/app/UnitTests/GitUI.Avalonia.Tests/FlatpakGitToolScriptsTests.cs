using System.Diagnostics;
using System.Runtime.CompilerServices;
using Avalonia.Headless.NUnit;
using CommonTestUtils;
using GitCommands;
using GitUI.Compat;
using GitUI.Editor;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FlatpakGitToolScriptsTests
{
    [TestCase("diff-command")]
    [TestCase("diff-environment")]
    [TestCase("diff-path")]
    [TestCase("diff-managed-proxy")]
    [TestCase("editor")]
    [TestCase("sequence-editor")]
    [TestCase("denied")]
    [AvaloniaTest]
    public async Task Packaged_scripts_should_translate_actual_Git_tool_launches_without_changing_configuration(string scenario)
    {
        string root = Path.Combine(Path.GetTempPath(), "GitExtensions.ToolScripts-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Dictionary<string, string?> originalEnvironment = Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>()
            .Where(entry => ((string)entry.Key).StartsWith("DFT_", StringComparison.Ordinal) || (string)entry.Key == "WSLENV")
            .ToDictionary(entry => (string)entry.Key, entry => (string?)entry.Value);
        try
        {
            string? diffArguments = null;
            if (scenario == "diff-environment")
            {
                using FileViewer viewer = new();
                diffArguments = viewer.GetDifftasticArguments().Args.Arguments;
            }

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
            if (diffArguments is not null)
            {
                request.Environment["GITEXTENSIONS_TEST_DIFF_ARGUMENTS"] = diffArguments;
            }

            if (scenario == "diff-managed-proxy")
            {
                string cache = Path.Combine(root, "cache");
                Dictionary<string, string?> environment = [];
                (await FlatpakToolEnvironment.InitializeAsync(true,
                    name => name == "XDG_CACHE_HOME" ? cache : name == "FLATPAK_ID" ? "com.github.gitextensions.Parity.Test" : null,
                    (name, value) => environment[name] = value,
                    hostRequest =>
                    {
                        if (hostRequest.ArgumentList.Contains("/bin/cat"))
                        {
                            return Task.FromResult((0, File.ReadAllText(hostRequest.ArgumentList.Last()), string.Empty));
                        }

                        return Task.FromResult((0, hostRequest.ArgumentList.Contains("gitextensions-installed-host-apps")
                            ? "/host/flatpak\0org.gnome.meld\n" : cache + "\0" + cache + "\0", string.Empty));
                    })).Should().BeTrue();
                request.Environment["GITEXTENSIONS_TEST_PROXY_DIR"] = environment[FlatpakToolEnvironment.ProxyDirectoryVariable]!;
            }

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
            if (scenario == "diff-managed-proxy")
            {
                string[] arguments = File.ReadAllText(Path.Combine(root, "host.log")).Split('\0', StringSplitOptions.RemoveEmptyEntries);
                arguments.Should().ContainInOrder("run", "--file-forwarding", "org.gnome.meld", "@@");
                arguments.Count(argument => argument == "@@").Should().Be(4);
            }

            if (scenario == "diff-environment")
            {
                string[] forwarded = File.ReadAllText(Path.Combine(root, "host.log.spawn")).Split('\0', StringSplitOptions.RemoveEmptyEntries);
                foreach (string name in new[] { "DFT_COLOR", "DFT_BACKGROUND", "DFT_SYNTAX_HIGHLIGHT", "DFT_CONTEXT", "DFT_STRIP_CR", "DFT_WIDTH" })
                {
                    forwarded.Should().Contain("--env=" + name + "=" + Environment.GetEnvironmentVariable(name));
                }
            }
        }
        finally
        {
            if (scenario == "diff-environment")
            {
                foreach (string name in Environment.GetEnvironmentVariables().Keys.Cast<string>()
                    .Where(name => name.StartsWith("DFT_", StringComparison.Ordinal) || name == "WSLENV").ToArray())
                {
                    Environment.SetEnvironmentVariable(name, originalEnvironment.GetValueOrDefault(name));
                }
            }

            Path.GetDirectoryName(root).Should().Be(Path.TrimEndingDirectorySeparator(Path.GetTempPath()));
            Path.GetFileName(root).Should().StartWith("GitExtensions.ToolScripts-");
            TestDirectory.Delete(root);
        }
    }

    private static string GetSourceDirectory([CallerFilePath] string path = "")
        => Path.GetDirectoryName(path)!;
}
