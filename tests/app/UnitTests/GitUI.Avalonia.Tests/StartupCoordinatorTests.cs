using Avalonia.Headless.NUnit;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class StartupCoordinatorTests
{
    private bool _originalCheckSettings;
    private string _originalRecentWorkingDir = string.Empty;
    private bool _originalStartWithRecentWorkingDir;

    [SetUp]
    public void SetUp()
    {
        _originalCheckSettings = AppSettings.CheckSettings;
        _originalRecentWorkingDir = AppSettings.RecentWorkingDir ?? string.Empty;
        _originalStartWithRecentWorkingDir = AppSettings.StartWithRecentWorkingDir;
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.CheckSettings = _originalCheckSettings;
        AppSettings.RecentWorkingDir = _originalRecentWorkingDir;
        AppSettings.StartWithRecentWorkingDir = _originalStartWithRecentWorkingDir;
    }

    [Test]
    public void Valid_startup_settings_should_skip_repairs_and_settings_dialog()
    {
        AppSettings.CheckSettings = true;
        List<string> calls = [];
        GitExtensions.StartupCoordinator coordinator = CreateCoordinator(
            calls,
            solveResults: [true],
            checkResults: [true]);

        bool result = coordinator.EnsurePrerequisites(CreateCommands(), ["app"]);

        result.Should().BeTrue();
        calls.Should().Equal("solve-git", "check");
    }

    [Test]
    public void Unresolved_startup_settings_should_try_repairs_then_open_the_checklist()
    {
        AppSettings.CheckSettings = true;
        List<string> calls = [];
        List<Type> openedPages = [];
        GitExtensions.StartupCoordinator coordinator = CreateCoordinator(
            calls,
            solveResults: [true, true],
            checkResults: [false, false],
            autoSolveResult: true,
            openedPages);

        bool result = coordinator.EnsurePrerequisites(CreateCommands(), ["app"]);

        result.Should().BeTrue();
        calls.Should().Equal("solve-git", "check", "auto-solve", "check", "settings", "solve-git");
        openedPages.Should().Equal(typeof(ChecklistSettingsPage));
    }

    [Test]
    public void Cancelling_git_location_without_a_valid_git_command_should_stop_startup()
    {
        AppSettings.CheckSettings = false;
        List<string> calls = [];
        List<Type> openedPages = [];
        GitExtensions.StartupCoordinator coordinator = CreateCoordinator(
            calls,
            solveResults: [false, false],
            checkResults: [],
            autoSolveResult: false,
            openedPages);

        bool result = coordinator.EnsurePrerequisites(CreateCommands(), ["app"]);

        result.Should().BeFalse();
        calls.Should().Equal("solve-git", "settings", "solve-git");
        openedPages.Should().Equal(typeof(GitSettingsPage));
    }

    [Test]
    public void Uninstall_should_bypass_git_and_settings_prerequisites()
    {
        AppSettings.CheckSettings = true;
        List<string> calls = [];
        GitExtensions.StartupCoordinator coordinator = CreateCoordinator(
            calls,
            solveResults: [],
            checkResults: []);

        bool result = coordinator.EnsurePrerequisites(CreateCommands(), ["app", "uninstall"]);

        result.Should().BeTrue();
        calls.Should().BeEmpty();
    }

    [AvaloniaTest]
    public void Checklist_should_report_invalid_settings_without_throwing_when_git_is_unavailable()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.GitExecutable.Returns(Substitute.For<IExecutable>());
        CommonLogic commonLogic = new(module);
        SettingsPageHostMock pageHost = new(new CheckSettingsLogic(commonLogic));
        ChecklistSettingsPage page = SettingsPageBase.Create<ChecklistSettingsPage>(
            pageHost,
            Substitute.For<IServiceProvider>());

        bool result = page.CheckSettings();

        result.Should().BeFalse();
        string[] windowsOnlyRows =
        [
            "ShellExtensionsRegistered",
            "GitBinFound",
            "GitExtensionsInstall",
            "SshConfig",
        ];
        windowsOnlyRows
            .Select(name => Avalonia.Controls.ControlExtensions
                .FindControl<Avalonia.Controls.Button>(page, name)!.IsVisible)
            .Should().OnlyContain(isVisible => isVisible == OperatingSystem.IsWindows());
    }

    [Test]
    public void Working_directory_selection_should_distinguish_dashboard_recent_and_explicit_launches()
    {
        string repository = FindRepositoryRoot();

        AppSettings.StartWithRecentWorkingDir = false;
        GitExtensions.App.GetWorkingDir(["app"]).Should().BeNull();

        AppSettings.RecentWorkingDir = repository;
        AppSettings.StartWithRecentWorkingDir = true;
        NormalizePath(GitExtensions.App.GetWorkingDir(["app"])!)
            .Should().Be(NormalizePath(repository));

        AppSettings.StartWithRecentWorkingDir = false;
        NormalizePath(GitExtensions.App.GetWorkingDir(["app", "browse", repository])!)
            .Should().Be(NormalizePath(repository));
    }

    private static GitExtensions.StartupCoordinator CreateCoordinator(
        ICollection<string> calls,
        IReadOnlyList<bool> solveResults,
        IReadOnlyList<bool> checkResults,
        bool autoSolveResult = false,
        ICollection<Type>? openedPages = null)
    {
        Queue<bool> solve = new(solveResults);
        Queue<bool> check = new(checkResults);
        return new GitExtensions.StartupCoordinator(
            () =>
            {
                calls.Add("solve-git");
                return solve.Dequeue();
            },
            _ =>
            {
                calls.Add("auto-solve");
                return autoSolveResult;
            },
            _ =>
            {
                calls.Add("check");
                return check.Dequeue();
            },
            (_, page) =>
            {
                calls.Add("settings");
                openedPages?.Add(((SettingsPageReferenceByType)page).SettingsPageType);
                return true;
            });
    }

    private static IGitUICommands CreateCommands()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.GitExecutable.Returns(Substitute.For<IExecutable>());
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        return commands;
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !Directory.Exists(Path.Join(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("The test checkout root was not found.");
    }

    private static string NormalizePath(string path)
        => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
}
