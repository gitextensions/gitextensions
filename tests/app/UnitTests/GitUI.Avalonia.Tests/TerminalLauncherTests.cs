using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using NSubstitute;
using ResourceManager;

namespace GitExtensionsTests;

[TestFixture]
public sealed class TerminalLauncherTests
{
    [Test]
    public void Launch_should_prefer_the_configured_linux_terminal()
    {
        ProcessStartInfo? captured = null;
        TerminalLauncher launcher = new(
            name => name == "TERMINAL" ? "kitty" : null,
            executable => executable == "kitty" ? "/usr/bin/kitty" : null,
            startInfo => captured = startInfo,
            TerminalPlatform.Linux);

        launcher.Launch("/work/repository");

        captured.Should().NotBeNull();
        captured!.FileName.Should().Be("/usr/bin/kitty");
        captured.WorkingDirectory.Should().Be("/work/repository");
        captured.ArgumentList.Should().BeEmpty();
    }

    [Test]
    public void Launch_should_use_linux_fallback_order()
    {
        List<string> candidates = [];
        ProcessStartInfo? captured = null;
        TerminalLauncher launcher = new(
            _ => null,
            executable =>
            {
                candidates.Add(executable);
                return executable == "gnome-terminal" ? "/usr/bin/gnome-terminal" : null;
            },
            startInfo => captured = startInfo,
            TerminalPlatform.Linux);

        launcher.Launch("/work/repository");

        candidates.Should().Equal("x-terminal-emulator", "gnome-terminal");
        captured!.FileName.Should().Be("/usr/bin/gnome-terminal");
    }

    [Test]
    public void SanitizeLinuxEnvironment_should_remove_snap_toolkit_overrides_and_restore_XDG_paths()
    {
        Dictionary<string, string?> environment = new()
        {
            ["SNAP"] = "/snap/code/1",
            ["SNAP_NAME"] = "code",
            ["GTK_PATH"] = "/snap/code/1/usr/lib/gtk-3.0",
            ["LD_LIBRARY_PATH"] = "/usr/local/lib",
            ["XDG_CONFIG_DIRS"] = "/snap/code/1/etc/xdg",
            ["XDG_CONFIG_DIRS_VSCODE_SNAP_ORIG"] = "/etc/xdg",
            ["XDG_DATA_DIRS"] = "/snap/code/1/usr/share",
            ["XDG_DATA_DIRS_VSCODE_SNAP_ORIG"] = "/usr/local/share:/usr/share",
        };

        TerminalLauncher.SanitizeLinuxEnvironment(environment);

        environment.Should().NotContainKeys("SNAP", "SNAP_NAME", "GTK_PATH");
        environment["LD_LIBRARY_PATH"].Should().Be("/usr/local/lib");
        environment["XDG_CONFIG_DIRS"].Should().Be("/etc/xdg");
        environment["XDG_DATA_DIRS"].Should().Be("/usr/local/share:/usr/share");
        environment.Should().NotContainKeys("XDG_CONFIG_DIRS_VSCODE_SNAP_ORIG", "XDG_DATA_DIRS_VSCODE_SNAP_ORIG");
    }

    [Test]
    public void Launch_should_open_windows_terminal_in_the_working_directory()
    {
        ProcessStartInfo? captured = null;
        TerminalLauncher launcher = new(
            _ => null,
            executable => executable == "wt.exe" ? "C:\\WindowsApps\\wt.exe" : null,
            startInfo => captured = startInfo,
            TerminalPlatform.Windows);

        launcher.Launch("C:\\work\\repository");

        captured!.FileName.Should().Be("C:\\WindowsApps\\wt.exe");
        captured.ArgumentList.Should().Equal("-d", "C:\\work\\repository");
    }

    [Test]
    public void Launch_should_fall_back_to_cmd_on_windows()
    {
        ProcessStartInfo? captured = null;
        TerminalLauncher launcher = new(
            _ => null,
            executable => executable == "cmd.exe" ? "C:\\Windows\\System32\\cmd.exe" : null,
            startInfo => captured = startInfo,
            TerminalPlatform.Windows);

        launcher.Launch("C:\\work\\repository");

        captured!.FileName.Should().EndWith("cmd.exe");
        captured.ArgumentList.Should().BeEmpty();
    }

    [Test]
    public void Launch_should_use_Terminal_app_on_macOS()
    {
        ProcessStartInfo? captured = null;
        TerminalLauncher launcher = new(
            _ => null,
            executable => executable == "open" ? "/usr/bin/open" : null,
            startInfo => captured = startInfo,
            TerminalPlatform.MacOS);

        launcher.Launch("/work/repository");

        captured!.FileName.Should().Be("/usr/bin/open");
        captured.ArgumentList.Should().Equal("-a", "Terminal", "/work/repository");
    }

    [AvaloniaTest]
    public void FormBrowse_terminal_menu_should_launch_in_the_repository_working_directory()
    {
        RecordingTerminalLauncher launcher = new();
        FormBrowse form = CreateBrowseForm(launcher);
        MenuItem userShell = form.FindControl<MenuItem>("userShell")
            ?? throw new InvalidOperationException("Terminal menu item was not created.");

        userShell.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        launcher.WorkingDirectory.Should().Be("/work/repository");
    }

    [AvaloniaTest]
    public void FormBrowse_terminal_menu_should_reuse_the_userShell_translation_key()
    {
        FormBrowse form = new();
        ITranslation translation = Substitute.For<ITranslation>();
        translation.TranslateItem(
                nameof(FormBrowse),
                "userShell",
                "ToolTipText",
                Arg.Any<Func<string?>>())
            .Returns("Translated terminal");

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "userShell", "ToolTipText", "Git bash");
        ((TextBlock)form.FindControl<MenuItem>("userShell")!.Header!).Text.Should().Be("Translated terminal");
    }

    private static FormBrowse CreateBrowseForm(ITerminalLauncher launcher)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns("/work/repository");
        module.IsValidGitWorkingDir().Returns(false);

        IAppTitleGenerator appTitleGenerator = Substitute.For<IAppTitleGenerator>();
        appTitleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(typeof(IAppTitleGenerator)).Returns(appTitleGenerator);
        commands.GetService(typeof(ITerminalLauncher)).Returns(launcher);

        return new FormBrowse(commands);
    }

    private sealed class RecordingTerminalLauncher : ITerminalLauncher
    {
        public string? WorkingDirectory { get; private set; }

        public void Launch(string workingDirectory)
        {
            WorkingDirectory = workingDirectory;
        }
    }
}
