using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using CommonTestUtils;
using GitCommands;
using GitCommands.DiffMergeTools;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class PlatformToolLocatorTests
{
    private string _root = null!;
    private PlatformToolLocator _previous = null!;

    [SetUp]
    public void SetUp()
    {
        _root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), $"GitExtensions.ToolLocator-{Guid.NewGuid():N}"));
        Directory.CreateDirectory(_root);
        _previous = PlatformToolLocator.TestAccessor.Current;
    }

    [TearDown]
    public void TearDown()
    {
        PlatformToolLocator.TestAccessor.Current = _previous;
        Path.GetDirectoryName(_root).Should().Be(Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath())));
        Path.GetFileName(_root).Should().StartWith("GitExtensions.ToolLocator-");
        TestDirectory.Delete(_root);
    }

    [TestCase("meld", "Meld.app/Contents/MacOS/Meld")]
    [TestCase("code", "Visual Studio Code.app/Contents/Resources/app/bin/code")]
    [TestCase("code", "VSCodium.app/Contents/Resources/app/bin/codium")]
    [TestCase("kdiff3", "kdiff3.app/Contents/MacOS/kdiff3")]
    [TestCase("sublime_text.exe", "Sublime Text.app/Contents/SharedSupport/bin/subl")]
    [TestCase("zed.exe", "Zed.app/Contents/MacOS/cli")]
    [TestCase("bcomp.exe", "Beyond Compare.app/Contents/MacOS/bcomp")]
    [TestCase("compare.exe", "Araxis Merge.app/Contents/Utilities/compare")]
    [TestCase("smerge.exe", "Sublime Merge.app/Contents/SharedSupport/bin/smerge")]
    [TestCase("p4merge.exe", "p4merge.app/Contents/Resources/launchp4merge")]
    [TestCase("diffmerge", "DiffMerge.app/Contents/MacOS/DiffMerge")]
    public void Find_should_use_the_executable_inside_a_known_Mac_bundle(string executable, string relative)
    {
        string path = CreateFile("Applications", relative);
        PlatformToolLocator locator = CreateLocator(TerminalPlatform.MacOS);

        LocatedTool? result = locator.Find(executable);

        result.Should().NotBeNull();
        result!.Executable.Should().Be(path);
        result.PrefixArguments.Should().BeEmpty();
    }

    [Test]
    public void Find_should_prefer_system_bundles_then_user_bundles_then_Homebrew()
    {
        string system = CreateFile("Applications", "Meld.app/Contents/MacOS/Meld");
        string user = CreateFile("UserApplications", "Meld.app/Contents/MacOS/Meld");
        string armBrew = CreateFile("Homebrew", "meld");
        string intelBrew = CreateFile("LocalBin", "meld");
        PlatformToolLocator locator = CreateLocator(TerminalPlatform.MacOS);

        locator.Find("meld")!.Executable.Should().Be(system);
        File.Delete(system);
        locator.Find("meld")!.Executable.Should().Be(user);
        File.Delete(user);
        locator.Find("meld")!.Executable.Should().Be(armBrew);
        File.Delete(armBrew);
        locator.Find("meld")!.Executable.Should().Be(intelBrew);
    }

    [Test]
    public void GetToolPath_and_command_should_keep_the_original_shared_lookup_first()
    {
        string sharedPath = CreateFile("Original  location", "meld");
        int sharedLookups = 0;
        DiffMergeToolConfigurationManager manager = new(() => null, (_, _) =>
        {
            sharedLookups++;
            return sharedPath;
        });
        PlatformToolLocator locator = CreateLocator(TerminalPlatform.Linux,
            _ => throw new AssertionException("A successful shared lookup must win."));

        locator.GetToolPath(manager, "meld", DiffMergeToolType.Diff, null).Should().Be(sharedPath.ToPosixPath());
        locator.GetToolCommand(manager, "meld", DiffMergeToolType.Diff, null).Should().Be($"\"{sharedPath.ToPosixPath()}\" \"$LOCAL\" \"$REMOTE\"");
        sharedLookups.Should().BeGreaterThan(0);
    }

    [Test]
    public void Find_should_use_Linux_PATH_before_querying_Flatpak_apps()
    {
        string native = CreateFile("Path", "meld");
        PlatformToolLocator locator = CreateLocator(TerminalPlatform.Linux,
            name => name == "meld" ? native : null,
            () => throw new AssertionException("An installed PATH executable must win."));

        locator.Find("meld")!.Executable.Should().Be(native);
        locator.Find("meld")!.PrefixArguments.Should().BeEmpty();
    }

    [TestCase("code", "com.visualstudio.code")]
    [TestCase("code", "com.vscodium.codium")]
    [TestCase("meld", "org.gnome.meld")]
    [TestCase("kdiff3", "org.kde.kdiff3")]
    [TestCase("sublime_text.exe", "com.sublimehq.SublimeText")]
    [TestCase("sublime_text.exe", "com.sublimetext.three")]
    [TestCase("smerge.exe", "com.sublimemerge.App")]
    [TestCase("zed.exe", "dev.zed.Zed")]
    [TestCase("konsole", "org.kde.konsole")]
    public void Find_should_use_only_a_verified_installed_Flatpak_app_ID(string executable, string id)
    {
        string flatpak = CreateFile("Path", "flatpak");
        PlatformToolLocator locator = CreateLocator(TerminalPlatform.Linux,
            name => name == "flatpak" ? flatpak : null, () => ["org.example.Unknown", id]);

        LocatedTool? result = locator.Find(executable);

        result.Should().NotBeNull();
        result!.Executable.Should().Be(flatpak);
        result.PrefixArguments.Should().Equal("run", id);
    }

    [Test]
    public void Find_should_respect_the_real_PATH_without_probing_the_working_directory_first()
    {
        string installed = CreateFile("Installed", "code");
        string shadow = CreateFile("Working", "code");
        string previousDirectory = Environment.CurrentDirectory;
        string? previousPath = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(shadow)!;
            Environment.SetEnvironmentVariable("PATH", Path.GetDirectoryName(installed));
            PlatformToolLocator locator = new(TerminalPlatform.Linux);

            locator.Find("code")!.Executable.Should().Be(installed);
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
            Environment.SetEnvironmentVariable("PATH", previousPath);
        }
    }

    [Test]
    public void GetToolCommand_should_keep_Git_placeholders_and_a_host_valid_Flatpak_command()
    {
        string flatpak = CreateFile("Path", "flatpak");
        DiffMergeToolConfigurationManager manager = new(() => null, (_, _) => string.Empty);
        PlatformToolLocator locator = CreateLocator(TerminalPlatform.Linux,
            name => name == "flatpak" ? flatpak : null, () => ["org.gnome.meld"]);

        string command = locator.GetToolCommand(manager, "meld", DiffMergeToolType.Merge, null);

        command.Should().Be($"\"{flatpak.ToPosixPath()}\" \"run\" \"--file-forwarding\" \"org.gnome.meld\" @@ \"$LOCAL\" @@ @@ \"$BASE\" @@ @@ \"$REMOTE\" @@ --output @@ \"$MERGED\" @@");
        command.Should().NotContain("flatpak-spawn").And.NotContain("/app/");
    }

    [Test]
    public void GetToolPath_and_command_should_preserve_explicit_user_values()
    {
        string explicitPath = CreateFile("Custom", "my-meld");
        IConfigValueStore settings = Substitute.For<IConfigValueStore>();
        settings.GetValue("mergetool.meld.path").Returns(explicitPath);
        settings.GetValue("mergetool.meld.cmd").Returns("my wrapper \"$MERGED\"");
        DiffMergeToolConfigurationManager manager = new(() => settings, (_, _) => string.Empty);
        PlatformToolLocator locator = CreateLocator(TerminalPlatform.Linux,
            _ => throw new AssertionException("Explicit user values must be preserved."));

        locator.GetToolPath(manager, "meld", DiffMergeToolType.Merge, settings).Should().Be(explicitPath);
        locator.GetToolCommand(manager, "meld", DiffMergeToolType.Merge, settings).Should().Be("my wrapper \"$MERGED\"");
    }

    [Test]
    public void GetToolCommand_should_not_search_again_for_an_explicit_command_matching_the_default()
    {
        string executable = OperatingSystem.IsWindows() ? "meld.exe" : "meld";
        string command = $"\"{executable}\" \"$LOCAL\" \"$REMOTE\"";
        IConfigValueStore settings = Substitute.For<IConfigValueStore>();
        settings.GetValue("difftool.meld.cmd").Returns(command);
        DiffMergeToolConfigurationManager manager = new(() => settings,
            (_, _) => throw new AssertionException("The shared lookup already found an explicit command."));
        PlatformToolLocator locator = CreateLocator(TerminalPlatform.Linux,
            _ => throw new AssertionException("An explicit command must win."));

        locator.GetToolCommand(manager, "meld", DiffMergeToolType.Diff, settings).Should().Be(command);
    }

    [Test]
    public void GetToolPath_should_not_search_again_for_an_explicit_bare_executable()
    {
        string executable = OperatingSystem.IsWindows() ? "meld.exe" : "meld";
        IConfigValueStore settings = Substitute.For<IConfigValueStore>();
        settings.GetValue("difftool.meld.path").Returns(executable);
        DiffMergeToolConfigurationManager manager = new(() => settings,
            (_, _) => throw new AssertionException("The shared lookup already found an explicit path."));
        PlatformToolLocator locator = CreateLocator(TerminalPlatform.Linux,
            _ => throw new AssertionException("An explicit path must win."));

        locator.GetToolPath(manager, "meld", DiffMergeToolType.Diff, settings).Should().Be(executable);
    }

    [Test]
    public void Find_should_not_discover_an_unknown_tool_or_guess_an_uninstalled_app()
    {
        PlatformToolLocator locator = CreateLocator(TerminalPlatform.Linux,
            name => name == "flatpak" ? "/usr/bin/flatpak" : null, () => ["org.example.Unknown"]);

        locator.Find("unknown-tool").Should().BeNull();
        locator.Find("meld").Should().BeNull();
    }

    [Test]
    public void Find_should_use_host_answers_instead_of_sandbox_paths_when_confined()
    {
        PlatformToolLocator locator = CreateLocator(TerminalPlatform.Linux,
            name => name == "meld" ? "/host/tools/meld" : null, isFlatpak: true);

        locator.Find("meld")!.Executable.Should().Be("/host/tools/meld");
    }

    [Test]
    public void Find_should_use_installed_host_Flatpak_apps_after_host_PATH_when_confined()
    {
        PlatformToolLocator locator = CreateLocator(TerminalPlatform.Linux,
            name => name == "flatpak" ? "/host/bin/flatpak" : null,
            () => ["org.gnome.meld"], isFlatpak: true);

        locator.Find("meld").Should().BeEquivalentTo(new LocatedTool("/host/bin/flatpak", ["run", "org.gnome.meld"]));
    }

    [AvaloniaTest]
    public void GetEditors_should_use_the_portable_lookup_without_collapsing_spaces_in_the_executable_path()
    {
        string[] variables = ["LOCALAPPDATA", "ProgramFiles", "ProgramW6432", "ProgramFiles(x86)"];
        Dictionary<string, string?> prior = variables.ToDictionary(name => name, Environment.GetEnvironmentVariable);
        try
        {
            foreach (string variable in variables)
            {
                Environment.SetEnvironmentVariable(variable, Path.Combine(_root, "MissingOriginalTools"));
            }

            string code = CreateFile("Tools  with  spaces", "code");
            PlatformToolLocator.TestAccessor.Current = CreateLocator(TerminalPlatform.Linux,
                name => name == "code" ? code : null);

            string[] editors = EditorHelper.GetEditors();

            editors.Should().Contain($"{code.Quote()} --new-window --wait");
            editors.Should().Contain("subl --new-window --wait");
        }
        finally
        {
            foreach ((string name, string? value) in prior)
            {
                Environment.SetEnvironmentVariable(name, value);
            }
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void GitConfigSettingsPage_should_load_the_tool_name_and_paths_through_its_real_initialization(bool flatpakTool)
    {
        string[] variables = ["LOCALAPPDATA", "ProgramFiles", "ProgramW6432", "ProgramFiles(x86)", "GIT_CONFIG_GLOBAL", "GIT_CONFIG_NOSYSTEM", "GIT_CONFIG_SYSTEM"];
        Dictionary<string, string?> prior = variables.ToDictionary(name => name, Environment.GetEnvironmentVariable);
        string globalConfig = CreateFile("Config", "global.config");
        JoinableTaskContext? previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        using JoinableTaskContext context = new();
        ThreadHelper.JoinableTaskContext = context;
        try
        {
            foreach (string variable in variables.Take(4))
            {
                Environment.SetEnvironmentVariable(variable, Path.Combine(_root, "MissingOriginalTools"));
            }

            Environment.SetEnvironmentVariable("GIT_CONFIG_GLOBAL", globalConfig);
            Environment.SetEnvironmentVariable("GIT_CONFIG_SYSTEM", globalConfig);
            Environment.SetEnvironmentVariable("GIT_CONFIG_NOSYSTEM", "1");
            string meld = flatpakTool ? CreateFile("Path", "flatpak") : CreateFile("Applications", "Meld.app/Contents/MacOS/Meld");
            PlatformToolLocator.TestAccessor.Current = flatpakTool
                ? CreateLocator(TerminalPlatform.Linux, name => name == "flatpak" ? meld : null, () => ["org.gnome.meld"])
                : CreateLocator(TerminalPlatform.MacOS);
            using GitModuleTestHelper repository = new(nameof(PlatformToolLocatorTests));
            repository.Module.SetSetting("diff.guitool", "meld");
            using CommonLogic common = new(repository.Module);
            SettingsPageHostMock host = new(new CheckSettingsLogic(common));
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(repository.Module);
            using GitConfigSettingsPage page = SettingsPageBase.Create<GitConfigSettingsPage>(host, commands);

            page.LoadSettings();

            page.GetTestAccessor().DiffTool.Text.Should().Be("meld");
            page.FindControl<TextBox>("txtDiffToolPath")!.Text.Should().Be(meld.ToPosixPath());
            string prefix = flatpakTool ? "\"run\" \"--file-forwarding\" \"org.gnome.meld\" " : string.Empty;
            string files = flatpakTool ? "@@ \"$LOCAL\" @@ @@ \"$REMOTE\" @@" : "\"$LOCAL\" \"$REMOTE\"";
            string expected = $"\"{meld.ToPosixPath()}\" {prefix}{files}";
            page.FindControl<TextBox>("txtDiffToolCommand")!.Text.Should().Be(expected);
            page.FindControl<Button>("btnDiffToolCommandSuggest")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            page.FindControl<TextBox>("txtDiffToolCommand")!.Text.Should().Be(expected);

            repository.Module.SetSetting("merge.tool", "meld");
            repository.Module.SetSetting("mergetool.meld.path", meld.ToPosixPath());
            repository.Module.SetSetting("mergetool.meld.cmd", expected);
            using FormResolveConflicts conflicts = new(commands);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(FormResolveConflicts).GetMethod("InitMergetool", flags)!.Invoke(conflicts, null).Should().Be(true);
            typeof(FormResolveConflicts).GetField("_mergetoolCmd", flags)!.GetValue(conflicts)
                .Should().Be($"{prefix}{files}");
        }
        finally
        {
            foreach ((string name, string? value) in prior)
            {
                Environment.SetEnvironmentVariable(name, value);
            }

            ThreadHelper.JoinableTaskContext = previousContext!;
        }
    }

    [AvaloniaTest]
    public async Task GetEditors_should_forward_the_file_Git_supplies_to_a_Flatpak_editor()
    {
        string[] variables = ["LOCALAPPDATA", "ProgramFiles", "ProgramW6432", "ProgramFiles(x86)", "GIT_CONFIG_GLOBAL", "GIT_CONFIG_NOSYSTEM", "GIT_CONFIG_SYSTEM", "GITEXTENSIONS_LOCATOR_EDITOR_LOG"];
        Dictionary<string, string?> prior = variables.ToDictionary(name => name, Environment.GetEnvironmentVariable);
        string globalConfig = CreateFile("Config", "global.config");
        string log = Path.Combine(_root, "editor-arguments.txt");
        JoinableTaskContext? previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        using JoinableTaskContext context = new();
        ThreadHelper.JoinableTaskContext = context;
        try
        {
            foreach (string variable in variables.Take(4))
            {
                Environment.SetEnvironmentVariable(variable, Path.Combine(_root, "MissingOriginalTools"));
            }

            Environment.SetEnvironmentVariable("GIT_CONFIG_GLOBAL", globalConfig);
            Environment.SetEnvironmentVariable("GIT_CONFIG_SYSTEM", globalConfig);
            Environment.SetEnvironmentVariable("GIT_CONFIG_NOSYSTEM", "1");
            Environment.SetEnvironmentVariable("GITEXTENSIONS_LOCATOR_EDITOR_LOG", log.ToPosixPath());
            string flatpak = CreateFile("Flatpak with 'quotes' and spaces", "flatpak");
            File.WriteAllText(flatpak, """
                #!/bin/sh
                printf '%s\0' "$@" > "$GITEXTENSIONS_LOCATOR_EDITOR_LOG"
                forwarding=0
                edited=0
                for argument do
                    case "$argument" in
                        @@) forwarding=$((1 - forwarding));;
                        *) if test "$forwarding" = 1; then
                               printf 'Owned editor forwarding regression\n' > "$argument"
                               edited=1
                           fi;;
                    esac
                done
                test "$edited" = 1
                """.Replace("\r\n", "\n") + "\n");
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(flatpak, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            }

            PlatformToolLocator.TestAccessor.Current = CreateLocator(TerminalPlatform.Linux,
                name => name == "flatpak" ? flatpak.ToPosixPath() : null, () => ["com.visualstudio.code"]);
            string editor = EditorHelper.GetEditors().Single(value => value.Contains("com.visualstudio.code", StringComparison.Ordinal));
            using GitModuleTestHelper repository = new(nameof(PlatformToolLocatorTests));
            repository.Module.SetSetting("core.editor", editor);
            repository.Module.SetSetting("commit.gpgsign", "false");

            ExecutionResult result = await repository.Module.GitExecutable.ExecuteAsync(new GitArgumentBuilder("commit") { "--allow-empty" }, throwOnErrorExit: false);

            string[] arguments = File.ReadAllText(log).Split('\0', StringSplitOptions.RemoveEmptyEntries);
            arguments.Should().ContainInOrder("run", "--file-forwarding", "com.visualstudio.code", "--new-window", "--wait", "@@");
            arguments.Count(argument => argument == "@@").Should().Be(2);
            arguments.Should().Contain(argument => argument.EndsWith("COMMIT_EDITMSG", StringComparison.Ordinal));
            result.ExitedSuccessfully.Should().BeTrue(result.AllOutput);
            repository.Module.GitExecutable.GetOutput("log -1 --format=%s").Trim().Should().Be("Owned editor forwarding regression");
        }
        finally
        {
            foreach ((string name, string? value) in prior)
            {
                Environment.SetEnvironmentVariable(name, value);
            }

            ThreadHelper.JoinableTaskContext = previousContext!;
        }
    }

    [AvaloniaTest]
    [TestCase("meld", "org.gnome.meld")]
    [TestCase("kdiff3", "org.kde.kdiff3")]
    [TestCase("vscode", "com.visualstudio.code")]
    [TestCase("smerge", "com.sublimemerge.App")]
    public async Task GetToolCommand_should_forward_the_files_Git_supplies_to_an_installed_Flatpak_tool(string tool, string id)
    {
        string[] variables = ["GIT_CONFIG_GLOBAL", "GIT_CONFIG_NOSYSTEM", "GIT_CONFIG_SYSTEM", "GITEXTENSIONS_LOCATOR_TOOL_LOG"];
        Dictionary<string, string?> prior = variables.ToDictionary(name => name, Environment.GetEnvironmentVariable);
        string globalConfig = CreateFile("Config", "global.config");
        string log = Path.Combine(_root, "tool-arguments.txt");
        JoinableTaskContext? previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        using JoinableTaskContext context = new();
        ThreadHelper.JoinableTaskContext = context;
        try
        {
            Environment.SetEnvironmentVariable("GIT_CONFIG_GLOBAL", globalConfig);
            Environment.SetEnvironmentVariable("GIT_CONFIG_SYSTEM", globalConfig);
            Environment.SetEnvironmentVariable("GIT_CONFIG_NOSYSTEM", "1");
            Environment.SetEnvironmentVariable("GITEXTENSIONS_LOCATOR_TOOL_LOG", log.ToPosixPath());
            string flatpak = CreateFile("Flatpak with 'quotes' and spaces", "flatpak");
            File.WriteAllText(flatpak, "#!/bin/sh\nprintf '%s\\0' \"$@\" > \"$GITEXTENSIONS_LOCATOR_TOOL_LOG\"\n");
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(flatpak, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            }

            PlatformToolLocator locator = CreateLocator(TerminalPlatform.Linux,
                name => name == "flatpak" ? flatpak.ToPosixPath() : null, () => [id]);
            DiffMergeToolConfigurationManager manager = new(() => null, (_, _) => string.Empty);
            string command = locator.GetToolCommand(manager, tool, DiffMergeToolType.Diff, null);
            using GitModuleTestHelper repository = new(nameof(PlatformToolLocatorTests));
            repository.Module.SetSetting($"difftool.{tool}.cmd", command);
            const string left = "before with spaces.txt";
            const string right = "after with spaces.txt";
            File.WriteAllText(Path.Combine(repository.Module.WorkingDir, left), "before\n");
            File.WriteAllText(Path.Combine(repository.Module.WorkingDir, right), "after\n");

            ExecutionResult result = await repository.Module.GitExecutable.ExecuteAsync(
                new GitArgumentBuilder("difftool") { "--no-index", "--no-prompt", $"--tool={tool}", "--", left.Quote(), right.Quote() }, throwOnErrorExit: false);

            string[] arguments = File.ReadAllText(log).Split('\0', StringSplitOptions.RemoveEmptyEntries);
            arguments.Should().ContainInOrder("run", "--file-forwarding", id);
            arguments.Count(argument => argument == "@@").Should().Be(tool == "smerge" ? 6 : 4);
            if (tool == "smerge")
            {
                arguments.Should().Contain("-o");
                arguments.Should().NotContain(argument => argument.StartsWith("-o=", StringComparison.Ordinal));
            }

            result.ExitCode.Should().Be(1, "--no-index reports the different file contents; {0}", result.AllOutput);
            repository.Module.GetSetting($"difftool.{tool}.cmd").Should().Be(command);
        }
        finally
        {
            foreach ((string name, string? value) in prior)
            {
                Environment.SetEnvironmentVariable(name, value);
            }

            ThreadHelper.JoinableTaskContext = previousContext!;
        }
    }

    private PlatformToolLocator CreateLocator(TerminalPlatform platform, Func<string, string?>? findOnPath = null,
        Func<IReadOnlyCollection<string>>? listApps = null, bool isFlatpak = false)
        => new(platform, File.Exists, findOnPath ?? (_ => null), _ => listApps?.Invoke() ?? [],
            [Path.Combine(_root, "Applications"), Path.Combine(_root, "UserApplications")],
            [Path.Combine(_root, "Homebrew"), Path.Combine(_root, "LocalBin")], isFlatpak);

    private string CreateFile(string directory, string relative)
    {
        string path = Path.Combine(_root, directory, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, string.Empty);
        return path;
    }
}
