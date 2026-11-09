using Avalonia.Headless.NUnit;
using CommonTestUtils;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtUtils;
using GitUI;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class PortableSequenceEditorTests
{
    [TestCase("e")]
    [TestCase("r")]
    public void Non_Mac_command_should_retain_the_original_GNU_sed_operation(string action)
        => PortableSequenceEditor.GetCommand(action, isMacOS: false).Should().Be($"sed -i -re '0,/pick/s//{action}/'");

    [AvaloniaTest]
    [TestCase("e")]
    [TestCase("r")]
    public async Task Mac_command_should_change_only_the_first_pick_in_an_actual_Git_rebase(string action)
    {
        string root = Path.Combine(Path.GetTempPath(), "GitExtensions.PortableRebase-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string config = Path.Combine(root, "global.config");
        File.WriteAllText(config, string.Empty);
        string[] names = ["GIT_CONFIG_GLOBAL", "GIT_CONFIG_SYSTEM", "GIT_CONFIG_NOSYSTEM", "GIT_SEQUENCE_EDITOR", "GIT_EDITOR"];
        Dictionary<string, string?> previous = names.ToDictionary(name => name, Environment.GetEnvironmentVariable);
        JoinableTaskContext? previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        using JoinableTaskContext context = new();
        ThreadHelper.JoinableTaskContext = context;
        try
        {
            Environment.SetEnvironmentVariable(names[0], config);
            Environment.SetEnvironmentVariable(names[1], config);
            Environment.SetEnvironmentVariable(names[2], "1");
            string marker = Path.Combine(root, "editor-calls.txt");
            string editor = Path.Combine(root, "editor.sh");
            File.WriteAllText(editor, "printf '%s\\n' \"$1\" >> " + PlatformToolLocator.QuoteShell(marker.ToPosixPath()) + "\n");
            Environment.SetEnvironmentVariable(names[3], PortableSequenceEditor.GetCommand(action, isMacOS: true));
            Environment.SetEnvironmentVariable(names[4], "sh " + PlatformToolLocator.QuoteShell(editor.ToPosixPath()));
            using GitModuleTestHelper repository = new(nameof(PortableSequenceEditorTests));
            GitModule module = repository.Module;
            module.SetSetting("commit.gpgsign", "false");
            module.GitExecutable.RunCommand("commit --allow-empty -qm first").Should().BeTrue();
            string first = module.GitExecutable.GetOutput("rev-parse HEAD").Trim();
            module.GitExecutable.RunCommand("commit --allow-empty -qm second").Should().BeTrue();
            string second = module.GitExecutable.GetOutput("rev-parse HEAD").Trim();

            ExecutionResult result = await module.GitExecutable.ExecuteAsync("rebase --interactive --root", throwOnErrorExit: false);

            result.ExitedSuccessfully.Should().BeTrue(result.AllOutput);
            if (action == "e")
            {
                string rebase = Path.Combine(module.WorkingDir, ".git", "rebase-merge");
                File.ReadAllText(Path.Combine(rebase, "stopped-sha")).Trim().Should().Be(first);
                File.ReadLines(Path.Combine(rebase, "git-rebase-todo"))
                    .Where(line => line.Length > 0 && !line.StartsWith('#'))
                    .Should().ContainSingle().Which.Should().StartWith("pick " + second);
                module.GitExecutable.RunCommand("rebase --continue").Should().BeTrue();
                File.Exists(marker).Should().BeFalse();
            }
            else
            {
                File.ReadAllLines(marker).Should().ContainSingle().Which.Should().EndWith("COMMIT_EDITMSG");
            }

            module.GitExecutable.GetOutput("log --reverse --format=%s").Replace("\r\n", "\n").Should().Be("first\nsecond\n");
            Directory.Exists(Path.Combine(module.WorkingDir, ".git", "rebase-merge")).Should().BeFalse();
        }
        finally
        {
            foreach ((string name, string? value) in previous)
            {
                Environment.SetEnvironmentVariable(name, value);
            }

            ThreadHelper.JoinableTaskContext = previousContext!;
            TestDirectory.Delete(root);
        }
    }
}
