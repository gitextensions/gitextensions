using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.CommandsDialogs.Menus;
using GitUI.Compat;
using GitUI.Shells;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class TerminalLauncherTests
{
    [SetUp]
    public void SetUp()
        => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

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
    public void Launch_should_use_the_linux_desktop_default_terminal()
    {
        ProcessStartInfo? captured = null;
        TerminalLauncher launcher = new(
            _ => null,
            executable => executable == "xdg-terminal-exec" ? "/usr/bin/xdg-terminal-exec" : null,
            startInfo => captured = startInfo,
            TerminalPlatform.Linux);

        launcher.Launch("/work/repository with spaces");

        captured!.FileName.Should().Be("/usr/bin/xdg-terminal-exec");
        captured.ArgumentList.Should().Equal("--dir=/work/repository with spaces");
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

        candidates.Should().Equal("xdg-terminal-exec", "x-terminal-emulator", "gnome-terminal");
        captured!.FileName.Should().Be("/usr/bin/gnome-terminal");
    }

    [Test]
    public void Launch_should_report_external_terminal_as_unavailable_in_Flatpak()
    {
        TerminalLauncher launcher = new(
            _ => null,
            _ => throw new InvalidOperationException("Executable resolution must not run inside Flatpak."),
            _ => throw new InvalidOperationException("Process launch must not run inside Flatpak."),
            TerminalPlatform.Linux,
            () => true);

        Action action = () => launcher.Launch("/work/repository");

        action.Should().Throw<PlatformNotSupportedException>()
            .WithMessage("*not available in this Flatpak installation*");
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

    [Test]
    public void LaunchShell_should_start_the_selected_Windows_shell_in_the_repository()
    {
        ProcessStartInfo? captured = null;
        TerminalLauncher launcher = new(
            _ => null,
            _ => null,
            startInfo => captured = startInfo,
            TerminalPlatform.Windows);

        launcher.LaunchShell("C:\\work\\repository", "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe");

        captured!.FileName.Should().EndWith("powershell.exe");
        captured.WorkingDirectory.Should().Be("C:\\work\\repository");
        captured.UseShellExecute.Should().BeFalse();
        captured.CreateNoWindow.Should().BeFalse();
        captured.RedirectStandardOutput.Should().BeFalse();
        captured.RedirectStandardError.Should().BeFalse();
    }

    [Test]
    public void LaunchShell_should_reject_a_non_graphical_Unix_shell()
    {
        TerminalLauncher launcher = new(
            _ => null,
            _ => null,
            _ => throw new AssertionException("Should not start a process"),
            TerminalPlatform.Linux);

        Action action = () => launcher.LaunchShell("/work/repository", "/bin/bash");

        action.Should().Throw<PlatformNotSupportedException>();
    }

    [AvaloniaTest]
    public void FormBrowse_terminal_toolbar_button_should_launch_in_the_repository_working_directory()
    {
        RecordingTerminalLauncher launcher = new();
        FormBrowse form = CreateBrowseForm(launcher);
        IconSplitButton userShell = form.FindControl<IconSplitButton>("userShell")
            ?? throw new InvalidOperationException("Terminal toolbar button was not created.");

        userShell.RaiseEvent(new RoutedEventArgs(SplitButton.ClickEvent));

        launcher.WorkingDirectory.Should().Be("/work/repository");
    }

    [AvaloniaTest]
    public void FormBrowse_terminal_hotkey_should_launch_the_shell_selected_by_initialization()
    {
        RecordingTerminalLauncher launcher = new();
        using FormBrowse form = CreateBrowseForm(launcher);
        IconSplitButton userShell = form.FindControl<IconSplitButton>("userShell")!;

        form.ExecuteCommand(FormBrowse.Command.GitBash).Should().BeTrue();

        launcher.WorkingDirectory.Should().Be("/work/repository");
        if (OperatingSystem.IsWindows())
        {
            IShellDescriptor shell = (IShellDescriptor)userShell.Tag!;
            shell.Should().BeSameAs(new ShellProvider().GetShell(BashShell.ShellName));
            shell.ExecutablePath.Should().NotBeNullOrEmpty();
            launcher.ShellLaunches.Should().Be(1);
            launcher.GenericLaunches.Should().Be(0);
            launcher.ExecutablePath.Should().Be(shell.ExecutablePath);
        }
        else
        {
            launcher.GenericLaunches.Should().Be(1);
            launcher.ShellLaunches.Should().Be(0);
        }
    }

    [AvaloniaTest]
    public void FormBrowse_terminal_menu_and_toolbar_should_preserve_their_original_translation_keys()
    {
        FormBrowse form = CreateBrowseForm(new RecordingTerminalLauncher());
        ITranslation translation = Substitute.For<ITranslation>();
        translation.TranslateItem(
                nameof(FormBrowse),
                "userShell",
                "ToolTipText",
                Arg.Any<Func<string?>>())
            .Returns("Translated terminal");
        translation.TranslateItem(
                nameof(FormBrowse),
                "gitBashToolStripMenuItem",
                "Text",
                Arg.Any<Func<string?>>())
            .Returns("Translated Git bash menu");

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "userShell", "ToolTipText", "Git bash");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "gitBashToolStripMenuItem", "Text", "Git &bash");
        ToolTip.GetTip(form.FindControl<IconSplitButton>("userShell")!).Should().Be("Translated terminal");
        form.toolsToolStripMenuItem.GetTestAccessor().GitBashMenuItem.Header.Should().Be("Translated Git bash menu");
    }

    [AvaloniaTest]
    public void Tools_terminal_menu_should_launch_the_selected_shell_in_the_repository()
    {
        RecordingTerminalLauncher launcher = new();
        ToolsToolStripMenuItem menu = CreateToolsMenu(launcher);
        MenuItem item = menu.GetTestAccessor().GitBashMenuItem;
        IShellDescriptor shell = new ShellProvider().GetShell(BashShell.ShellName);
        item.Tag.Should().BeSameAs(shell);
        shell.ExecutablePath.Should().NotBeNullOrEmpty();

        item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        launcher.WorkingDirectory.Should().Be("/work/repository");
        if (OperatingSystem.IsWindows())
        {
            launcher.ShellLaunches.Should().Be(1);
            launcher.GenericLaunches.Should().Be(0);
            launcher.ExecutablePath.Should().Be(shell.ExecutablePath);
        }
        else
        {
            launcher.GenericLaunches.Should().Be(1);
            launcher.ShellLaunches.Should().Be(0);
            launcher.ExecutablePath.Should().BeNull();
        }
    }

    [AvaloniaTest]
    public void Tools_terminal_menu_should_preserve_the_platform_route_when_no_shell_is_configured()
    {
        RecordingTerminalLauncher launcher = new();
        IShellProvider provider = Substitute.For<IShellProvider>();
        provider.GetShell(BashShell.ShellName).Returns((IShellDescriptor)null!);
        ToolsToolStripMenuItem menu = CreateToolsMenu(launcher, provider);
        MenuItem item = menu.GetTestAccessor().GitBashMenuItem;

        item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        launcher.ShellLaunches.Should().Be(0);
        if (OperatingSystem.IsWindows())
        {
            launcher.GenericLaunches.Should().Be(0);
            launcher.WorkingDirectory.Should().BeNull();
        }
        else
        {
            launcher.GenericLaunches.Should().Be(1);
            launcher.WorkingDirectory.Should().Be("/work/repository");
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    [TestCase(false)]
    [TestCase(true)]
    public void Tools_terminal_menu_should_name_the_selected_shell_when_it_cannot_launch(bool missingExecutable)
    {
        RecordingTerminalLauncher launcher = new() { Failure = new InvalidOperationException("synthetic launch failure") };
        IShellDescriptor shell = Substitute.For<IShellDescriptor>();
        shell.Name.Returns("Configured shell");
        shell.ExecutablePath.Returns(missingExecutable ? null : "C:\\portable\\selected-shell.exe");
        IShellProvider provider = Substitute.For<IShellProvider>();
        provider.GetShell(BashShell.ShellName).Returns(shell);
        ToolsToolStripMenuItem menu = CreateToolsMenu(launcher, provider);
        MenuItem item = menu.GetTestAccessor().GitBashMenuItem;
        WinFormsShims.IMessageBoxHost? previous;
        try
        {
            previous = WinFormsShims.ShimHost.MessageBoxHost;
        }
        catch (InvalidOperationException)
        {
            previous = null;
        }

        WinFormsShims.IMessageBoxHost messageBoxes = Substitute.For<WinFormsShims.IMessageBoxHost>();
        string? message = null;
        messageBoxes.Show(Arg.Any<WinFormsShims.IWin32Window?>(), Arg.Do<string?>(value => message = value), Arg.Any<string?>(),
            Arg.Any<WinFormsShims.MessageBoxButtons>(), Arg.Any<WinFormsShims.MessageBoxIcon>(), Arg.Any<WinFormsShims.MessageBoxDefaultButton>())
            .Returns(WinFormsShims.DialogResult.OK);
        WinFormsShims.ShimHost.MessageBoxHost = messageBoxes;
        try
        {
            item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            message.Should().Contain(OperatingSystem.IsWindows() ? shell.Name : "Git bash");
            if (OperatingSystem.IsWindows() && missingExecutable)
            {
                launcher.ShellLaunches.Should().Be(0);
                launcher.GenericLaunches.Should().Be(0);
            }
            else
            {
                message.Should().Contain("synthetic launch failure");
            }
        }
        finally
        {
            WinFormsShims.ShimHost.MessageBoxHost = previous!;
        }
    }

    private static ToolsToolStripMenuItem CreateToolsMenu(ITerminalLauncher launcher, IShellProvider? provider = null)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns("/work/repository");
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(typeof(ITerminalLauncher)).Returns(launcher);
        commands.GetService(typeof(IShellProvider)).Returns(provider ?? new ShellProvider());
        ToolsToolStripMenuItem menu = new();
        menu.Initialize(() => commands);
        return menu;
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
        commands.RepoChangedNotifier.Returns(Substitute.For<ILockableNotifier>());
        commands.GetService(typeof(IAppTitleGenerator)).Returns(appTitleGenerator);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());
        commands.GetService(typeof(IRepositoryHistoryUIService)).Returns(RepositoryHistoryTestHelper.CreateEmptyService());
        commands.GetService(typeof(ILinkFactory)).Returns(Substitute.For<ILinkFactory>());
        IUserRepositoriesListController repositoriesController = Substitute.For<IUserRepositoriesListController>();
        repositoriesController.PreRenderRepositories(Arg.Any<string>()).Returns((
            Array.Empty<RecentRepoInfo>(),
            Array.Empty<RecentRepoInfo>()));
        commands.GetService(typeof(IUserRepositoriesListController)).Returns(repositoriesController);
        commands.GetService(typeof(ITerminalLauncher)).Returns(launcher);
        commands.GetService(typeof(IShellProvider)).Returns(new ShellProvider());

        return new FormBrowse(commands);
    }

    private sealed class RecordingTerminalLauncher : ITerminalLauncher
    {
        public string? WorkingDirectory { get; private set; }
        public string? ExecutablePath { get; private set; }
        public int GenericLaunches { get; private set; }
        public int ShellLaunches { get; private set; }
        public Exception? Failure { get; init; }

        public void Launch(string workingDirectory)
        {
            WorkingDirectory = workingDirectory;
            GenericLaunches++;
            if (Failure is not null)
            {
                throw Failure;
            }
        }

        public void LaunchShell(string workingDirectory, string executablePath)
        {
            WorkingDirectory = workingDirectory;
            ExecutablePath = executablePath;
            ShellLaunches++;
            if (Failure is not null)
            {
                throw Failure;
            }
        }
    }
}
