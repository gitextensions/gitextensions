using System.ComponentModel.Design;
using System.Diagnostics;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using CommonTestUtils;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Extensibility.Translations.Xliff;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class FlatpakHostGitTests
{
    private string _root = null!;
    private string _originalGit = null!;
    private Dictionary<string, string?> _environment = null!;
    private Func<bool> _isFlatpak = null!;
    private Func<string> _launcherPath = null!;
    private Func<string> _bundledPath = null!;
    private Func<ProcessStartInfo, Task<(int ExitCode, string Output, string Error)>> _query = null!;
    private JoinableTaskContext? _previousContext;
    private JoinableTaskContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        Avalonia.Threading.AvaloniaSynchronizationContext.InstallIfNeeded();
        _root = Path.Combine(Path.GetTempPath(), "GitExtensions.HostGit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
        string config = Path.Combine(_root, "global.config");
        File.WriteAllText(config, string.Empty);
        string[] names = ["GIT_CONFIG_GLOBAL", "GIT_CONFIG_SYSTEM", "GIT_CONFIG_NOSYSTEM", "GITEXTENSIONS_HOST_GIT_MIN_VERSION",
            "GITEXTENSIONS_HOST_GIT_MISSING_MESSAGE", "GITEXTENSIONS_HOST_GIT_OLD_MESSAGE", "GITEXTENSIONS_HOST_GIT_UNAVAILABLE_MESSAGE",
            "GITEXTENSIONS_HOST_GIT_SELECTED", "GITEXTENSIONS_HOST_GIT_FALLBACK",
            "LOCALAPPDATA", "ProgramFiles", "ProgramW6432", "ProgramFiles(x86)"];
        _environment = names.ToDictionary(name => name, Environment.GetEnvironmentVariable);
        Environment.SetEnvironmentVariable(names[0], config);
        Environment.SetEnvironmentVariable(names[1], config);
        Environment.SetEnvironmentVariable(names[2], "1");
        _originalGit = AppSettings.GitCommandValue;
        _isFlatpak = FlatpakHostGit.TestAccessor.IsFlatpak;
        _launcherPath = FlatpakHostGit.TestAccessor.LauncherPath;
        _bundledPath = FlatpakHostGit.TestAccessor.BundledPath;
        _query = FlatpakHostGit.TestAccessor.Query;
        FlatpakHostGit.TestAccessor.Query = _ => Task.FromResult((0, "git version 2.53.0", string.Empty));
        _previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        _context = new JoinableTaskContext();
        ThreadHelper.JoinableTaskContext = _context;
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.GitCommandValue = _originalGit;
        FlatpakHostGit.TestAccessor.IsFlatpak = _isFlatpak;
        FlatpakHostGit.TestAccessor.LauncherPath = _launcherPath;
        FlatpakHostGit.TestAccessor.BundledPath = _bundledPath;
        FlatpakHostGit.TestAccessor.Query = _query;
        foreach ((string name, string? value) in _environment)
        {
            Environment.SetEnvironmentVariable(name, value);
        }

        ThreadHelper.JoinableTaskContext = _previousContext!;
        _context.Dispose();
        TestDirectory.Delete(_root);
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Paths_page_should_offer_host_Git_only_in_Flatpak_and_route_the_existing_executor_through_the_choice(bool flatpak)
    {
        using TranslationTestScope language = new("");
        PathUtil.TryFindFullPath(OperatingSystem.IsWindows() ? "git.exe" : "git", out string? bundled).Should().BeTrue();
        string host = OperatingSystem.IsWindows()
            ? Path.GetFullPath(Path.Combine(Path.GetDirectoryName(bundled)!, "..", "mingw64", "bin", "git.exe"))
            : Path.Combine(_root, "host-git");
        if (!OperatingSystem.IsWindows())
        {
            File.CreateSymbolicLink(host, bundled!);
        }

        File.Exists(host).Should().BeTrue();
        host.Should().NotBe(bundled);
        FlatpakHostGit.TestAccessor.IsFlatpak = () => flatpak;
        FlatpakHostGit.TestAccessor.LauncherPath = () => host;
        FlatpakHostGit.TestAccessor.BundledPath = () => bundled!;
        int queries = 0;
        FlatpakHostGit.TestAccessor.Query = _ =>
        {
            queries++;
            return Task.FromResult((0, "git version 2.53.0", string.Empty));
        };
        AppSettings.GitCommandValue = bundled!;
        using GitModuleTestHelper repository = new(nameof(FlatpakHostGitTests));
        using FormSettings form = new();
        form.GetTestAccessor().InitializePages();
        GitSettingsPage page = form.GetTestAccessor().SettingsTreeView.SettingsPages.OfType<GitSettingsPage>().Single();
        page.LoadSettings();
        CheckBox? option = page.GetLogicalDescendants().OfType<CheckBox>()
            .SingleOrDefault(control => control.Name == "_NO_TRANSLATE_chkUseHostGit");
        repository.Module.GitExecutable.Command.Should().Be(bundled);
        if (!flatpak)
        {
            option.Should().BeNull();
            return;
        }

        option.Should().NotBeNull();
        AutomationProperties.GetName(option!).Should().Be("Use Git installed on the host");
        AutomationProperties.GetAutomationId(option!).Should().Be("_NO_TRANSLATE_chkUseHostGit");
        KeyboardNavigation.GetTabIndex(option!).Should().BeGreaterThan(KeyboardNavigation.GetTabIndex(page.FindControl<Button>("BrowseGitPath")!));
        option!.IsChecked.Should().BeFalse();
        option.IsChecked = true;
        page.SaveSettings();
        AppSettings.GitCommandValue.Should().Be(host);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        Environment.GetEnvironmentVariable("GITEXTENSIONS_HOST_GIT_SELECTED").Should().Be("1");
        queries.Should().Be(1, "selecting host Git must check availability before the next application start");
        repository.Module.GitExecutable.Command.Should().Be(host);
        repository.Module.GitExecutable.GetOutput("--version").Should().StartWith("git version ");
        page.LoadSettings();
        option.IsChecked.Should().BeTrue();
        option.IsChecked = false;
        page.SaveSettings();
        AppSettings.GitCommandValue.Should().Be(bundled);
        Environment.GetEnvironmentVariable("GITEXTENSIONS_HOST_GIT_SELECTED").Should().Be("0");
        repository.Module.GitExecutable.Command.Should().Be(bundled);
    }

    [TestCase(44, "", "missing")]
    [TestCase(1, "", "unavailable")]
    [TestCase(0, "git version 2.42.9", "old")]
    [TestCase(0, "git version 2.43.0", null)]
    [TestCase(0, "git version 2.53.0", null)]
    public async Task Fallback_notice_should_follow_the_original_minimum_version_and_host_service_result(int status, string version, string? expected)
    {
        using HostGitMessages messages = new();
        string? result = await FlatpakHostGit.GetFallbackMessageAsync(request =>
        {
            request.FileName.Should().Be("/usr/bin/flatpak-spawn");
            request.ArgumentList.Should().Contain("--host").And.Contain("--watch-bus");
            return Task.FromResult((status, version, string.Empty));
        }, messages);

        result.Should().Be(expected switch
        {
            "missing" => messages.Missing,
            "unavailable" => messages.Unavailable,
            "old" => messages.Old,
            _ => null,
        });
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Settings_should_keep_editor_and_tool_choices_accessible_when_host_permission_is_revoked(bool configuredTool)
    {
        foreach (string name in new[] { "LOCALAPPDATA", "ProgramFiles", "ProgramW6432", "ProgramFiles(x86)" })
        {
            Environment.SetEnvironmentVariable(name, Path.Combine(_root, "NoOriginalTools"));
        }

        PlatformToolLocator previous = PlatformToolLocator.TestAccessor.Current;
        int lookups = 0;
        PlatformToolLocator.TestAccessor.Current = new(TerminalPlatform.Linux, File.Exists, _ =>
        {
            lookups++;
            throw new PlatformNotSupportedException("Host tools are not available in this Flatpak installation.");
        }, _ => [], [], [], isFlatpak: true);
        try
        {
            using GitModuleTestHelper repository = new(nameof(FlatpakHostGitTests));
            if (configuredTool)
            {
                repository.Module.SetSetting("diff.guitool", "meld");
                repository.Module.SetSetting("merge.tool", "meld");
            }

            using ServiceContainer services = new();
            GitExtensions.ServiceContainerRegistry.RegisterServices(services);
            GitUICommands commands = new(services, repository.Module);
            using FormSettings form = new(commands);
            form.GetTestAccessor().InitializePages();
            GitConfigSettingsPage page = form.GetTestAccessor().SettingsTreeView.SettingsPages.OfType<GitConfigSettingsPage>().Single();
            page.LoadSettings();

            page.GetTestAccessor().Editor.Items.OfType<string>().Should().Contain("code --new-window --wait");
            lookups.Should().BeGreaterThan(0);
            if (configuredTool)
            {
                page.GetTestAccessor().DiffTool.Text.Should().Be("meld");
                page.FindControl<TextBox>("txtDiffToolPath")!.Text.Should().NotBeNullOrWhiteSpace();
            }
        }
        finally
        {
            PlatformToolLocator.TestAccessor.Current = previous;
        }
    }

    [AvaloniaTest]
    public void Translation_should_reach_the_option_and_every_launcher_fallback_message_from_the_additive_catalog()
    {
        using TranslationTestScope language = new("OwnedHostGit");
        TranslationFile catalog = TranslationSerializer.Deserialize(Path.Combine(AppContext.BaseDirectory, "Translation", "English.Avalonia.xlf"))!;
        catalog.Should().NotBeNull();
        using HostGitMessages messages = new();
        TranslationFile neutral = new();
        messages.AddTranslationItems(neutral);
        catalog.TranslationCategories.Single().Body.TranslationItems.Select(item => (item.Id, item.Source))
            .Should().BeEquivalentTo(neutral.TranslationCategories.Single().Body.TranslationItems.Select(item => (item.Id, item.Source)));
        foreach (TranslationItem item in catalog.TranslationCategories.Single().Body.TranslationItems)
        {
            item.Value = "Translated " + item.Source;
        }

        Translator.GetTranslation("OwnedHostGit")[".Avalonia"] = catalog;
        FlatpakHostGit.TestAccessor.IsFlatpak = () => true;
        using GitSettingsPage page = new();
        CheckBox? option = page.GetLogicalDescendants().OfType<CheckBox>()
            .SingleOrDefault(control => control.Name == "_NO_TRANSLATE_chkUseHostGit");
        option.Should().NotBeNull();
        option!.Content.Should().Be("Translated Use Git installed on the host");
        FlatpakHostGit.InitializeEnvironment();
        Environment.GetEnvironmentVariable("GITEXTENSIONS_HOST_GIT_MIN_VERSION").Should().Be(GitVersion.LastSupportedVersion.ToString());
        Environment.GetEnvironmentVariable("GITEXTENSIONS_HOST_GIT_MISSING_MESSAGE").Should().Be("Translated " + messages.Missing);
        Environment.GetEnvironmentVariable("GITEXTENSIONS_HOST_GIT_OLD_MESSAGE").Should().Be("Translated " + messages.Old);
        Environment.GetEnvironmentVariable("GITEXTENSIONS_HOST_GIT_UNAVAILABLE_MESSAGE").Should().Be("Translated " + messages.Unavailable);
    }
}
