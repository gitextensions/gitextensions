using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class SettingsDialogTests
{
    [AvaloniaTest]
    public void FormBrowse_should_expose_the_live_settings_toolbar_action()
    {
        GitUI.ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        FormBrowse form = new();

        Button settings = form.FindControl<Button>("EditSettings")
            ?? throw new InvalidOperationException("Settings toolbar button was not created.");
        settings.Content.Should().Be("Settings");
        settings.IsVisible.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormSettings_should_host_the_placeholder_and_preserve_dialog_chrome()
    {
        FormSettings form = new();
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            FormSettings.TestAccessor accessor = form.GetTestAccessor();
            accessor.CurrentPage.Should().BeOfType<SettingsPlaceholderPage>();
            accessor.OkButton.IsDefault.Should().BeTrue();
            accessor.CancelButton.Content.Should().Be("Cancel");
            form.Title.Should().Be($"Settings - {GitCommands.AppSettings.ApplicationName}");
            form.CaptureRenderedFrame().Should().NotBeNull();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormSettings_should_register_and_navigate_to_the_general_page()
    {
        FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();

        GeneralSettingsPage page = accessor.SettingsTreeView.SettingsPages
            .OfType<GeneralSettingsPage>()
            .Single();
        form.GotoPage(GeneralSettingsPage.GetPageReference());

        SettingsPageHeader header = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        SettingsPageHeader.TestAccessor headerAccessor = header.GetTestAccessor();
        headerAccessor.Page.Should().BeSameAs(page);
        headerAccessor.Global.IsChecked.Should().BeTrue();
        headerAccessor.Effective.IsVisible.Should().BeFalse();
        page.GetTitle().Should().Be("General");
    }

    [AvaloniaTest]
    public void General_settings_should_preserve_special_load_and_save_mappings()
    {
        int originalMaxCommits = AppSettings.MaxRevisionGraphCommits;
        bool originalToolbarStatus = AppSettings.ShowGitStatusInBrowseToolbar;
        bool originalArtificialStatus = AppSettings.ShowGitStatusForArtificialCommits;
        bool originalSubmoduleStatus = AppSettings.ShowSubmoduleStatus;
        bool? originalUpdateModules = AppSettings.UpdateSubmodulesOnCheckout;
        GitPullAction originalPullAction = AppSettings.DefaultPullAction;
        string originalCloneDestination = AppSettings.DefaultCloneDestinationPath;
        try
        {
            AppSettings.MaxRevisionGraphCommits = 42000;
            AppSettings.ShowGitStatusInBrowseToolbar = false;
            AppSettings.ShowGitStatusForArtificialCommits = false;
            AppSettings.ShowSubmoduleStatus = true;
            AppSettings.UpdateSubmodulesOnCheckout = null;
            AppSettings.DefaultPullAction = GitPullAction.FetchAll;
            AppSettings.DefaultCloneDestinationPath = @"D:\Repositories";

            FormSettings form = new();
            FormSettings.TestAccessor formAccessor = form.GetTestAccessor();
            formAccessor.InitializePages();
            GeneralSettingsPage page = formAccessor.SettingsTreeView.SettingsPages
                .OfType<GeneralSettingsPage>()
                .Single();
            page.LoadSettings();

            GeneralSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            accessor.LimitCommits.IsChecked.Should().BeTrue();
            accessor.MaxCommits.Value.Should().Be(42000);
            accessor.UpdateModules.IsChecked.Should().BeNull();
            accessor.ShowSubmoduleStatusInBrowse.IsEnabled.Should().BeFalse();
            accessor.ShowSubmoduleStatusInBrowse.IsChecked.Should().BeFalse();
            accessor.DefaultPullAction.SelectedItem!.ToString().Should().Be("Fetch all");
            accessor.DefaultCloneDestination.Text.Should().Be(@"D:\Repositories");

            accessor.ShowGitStatusInToolbar.IsChecked = true;
            accessor.ShowSubmoduleStatusInBrowse.IsChecked = true;
            accessor.LimitCommits.IsChecked = false;
            accessor.UpdateModules.IsChecked = false;
            accessor.DefaultPullAction.SelectedItem = accessor.DefaultPullAction.Items
                .Cast<object>()
                .Single(item => item.ToString() == "Pull - rebase");
            accessor.DefaultCloneDestination.Text = @"C:\Source";
            page.SaveSettings();

            AppSettings.ShowSubmoduleStatus.Should().BeTrue();
            AppSettings.MaxRevisionGraphCommits.Should().Be(0);
            AppSettings.UpdateSubmodulesOnCheckout.Should().BeFalse();
            AppSettings.DefaultPullAction.Should().Be(GitPullAction.Rebase);
            AppSettings.DefaultCloneDestinationPath.Should().Be(@"C:\Source");
        }
        finally
        {
            AppSettings.MaxRevisionGraphCommits = originalMaxCommits;
            AppSettings.ShowGitStatusInBrowseToolbar = originalToolbarStatus;
            AppSettings.ShowGitStatusForArtificialCommits = originalArtificialStatus;
            AppSettings.ShowSubmoduleStatus = originalSubmoduleStatus;
            AppSettings.UpdateSubmodulesOnCheckout = originalUpdateModules;
            AppSettings.DefaultPullAction = originalPullAction;
            AppSettings.DefaultCloneDestinationPath = originalCloneDestination;
        }
    }

    [AvaloniaTest]
    public void FormSettings_should_register_and_navigate_to_the_colors_page()
    {
        FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();

        ColorsSettingsPage page = accessor.SettingsTreeView.SettingsPages
            .OfType<ColorsSettingsPage>()
            .Single();
        form.GotoPage(ColorsSettingsPage.GetPageReference());

        SettingsPageHeader header = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        header.GetTestAccessor().Page.Should().BeSameAs(page);
        page.GetTitle().Should().Be("Colors");
    }

    [AvaloniaTest]
    public void Colors_settings_should_map_graph_options_and_supported_themes()
    {
        bool originalMulticolorBranches = AppSettings.MulticolorBranches;
        bool originalAlternateBackColor = AppSettings.RevisionGraphDrawAlternateBackColor;
        bool originalNonRelativesGray = AppSettings.RevisionGraphDrawNonRelativesGray;
        bool originalNonRelativesTextGray = AppSettings.RevisionGraphDrawNonRelativesTextGray;
        bool originalHighlightAuthored = AppSettings.HighlightAuthoredRevisions;
        bool originalFillRefLabels = AppSettings.FillRefLabels;
        ThemeId originalThemeId = AppSettings.ThemeId;
        string[] originalThemeVariations = AppSettings.ThemeVariations;
        bool originalUseSystemVisualStyle = AppSettings.UseSystemVisualStyle;
        try
        {
            ThemeId customTheme = new("custom-review-theme");
            AppSettings.MulticolorBranches = true;
            AppSettings.RevisionGraphDrawAlternateBackColor = false;
            AppSettings.RevisionGraphDrawNonRelativesGray = true;
            AppSettings.RevisionGraphDrawNonRelativesTextGray = false;
            AppSettings.HighlightAuthoredRevisions = true;
            AppSettings.FillRefLabels = false;
            AppSettings.ThemeId = customTheme;
            AppSettings.ThemeVariations = [ThemeVariations.Colorblind];
            AppSettings.UseSystemVisualStyle = false;

            FormSettings form = new();
            FormSettings.TestAccessor formAccessor = form.GetTestAccessor();
            formAccessor.InitializePages();
            ColorsSettingsPage page = formAccessor.SettingsTreeView.SettingsPages
                .OfType<ColorsSettingsPage>()
                .Single();
            page.LoadSettings();

            ColorsSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            accessor.MulticolorBranches.IsChecked.Should().BeTrue();
            accessor.DrawAlternateBackColor.IsChecked.Should().BeFalse();
            accessor.DrawNonRelativesGray.IsChecked.Should().BeTrue();
            accessor.DrawNonRelativesTextGray.IsChecked.Should().BeFalse();
            accessor.HighlightAuthored.IsChecked.Should().BeTrue();
            accessor.FillRefLabels.IsChecked.Should().BeFalse();
            page.SelectedThemeId.Should().Be(customTheme);
            accessor.Colorblind.IsChecked.Should().BeTrue();
            accessor.Colorblind.IsVisible.Should().BeFalse();
            accessor.UseSystemVisualStyle.IsVisible.Should().BeFalse();
            accessor.RestartNeeded.IsVisible.Should().BeFalse();

            page.SaveSettings();
            AppSettings.ThemeId.Should().Be(customTheme);

            accessor.MulticolorBranches.IsChecked = false;
            accessor.DrawAlternateBackColor.IsChecked = true;
            accessor.DrawNonRelativesGray.IsChecked = false;
            accessor.DrawNonRelativesTextGray.IsChecked = true;
            accessor.HighlightAuthored.IsChecked = false;
            accessor.FillRefLabels.IsChecked = true;
            page.SelectedThemeId = ThemeId.DefaultLight;

            accessor.RestartNeeded.IsVisible.Should().BeTrue();
            accessor.Colorblind.IsChecked.Should().BeFalse();
            page.SaveSettings();

            AppSettings.MulticolorBranches.Should().BeFalse();
            AppSettings.RevisionGraphDrawAlternateBackColor.Should().BeTrue();
            AppSettings.RevisionGraphDrawNonRelativesGray.Should().BeFalse();
            AppSettings.RevisionGraphDrawNonRelativesTextGray.Should().BeTrue();
            AppSettings.HighlightAuthoredRevisions.Should().BeFalse();
            AppSettings.FillRefLabels.Should().BeTrue();
            AppSettings.ThemeId.Should().Be(ThemeId.DefaultLight);
            AppSettings.ThemeVariations.Should().BeEmpty();
            AppSettings.UseSystemVisualStyle.Should().BeFalse();
        }
        finally
        {
            AppSettings.MulticolorBranches = originalMulticolorBranches;
            AppSettings.RevisionGraphDrawAlternateBackColor = originalAlternateBackColor;
            AppSettings.RevisionGraphDrawNonRelativesGray = originalNonRelativesGray;
            AppSettings.RevisionGraphDrawNonRelativesTextGray = originalNonRelativesTextGray;
            AppSettings.HighlightAuthoredRevisions = originalHighlightAuthored;
            AppSettings.FillRefLabels = originalFillRefLabels;
            AppSettings.ThemeId = originalThemeId;
            AppSettings.ThemeVariations = originalThemeVariations;
            AppSettings.UseSystemVisualStyle = originalUseSystemVisualStyle;
        }
    }

    [AvaloniaTest]
    public void Colors_settings_should_preserve_original_translation_keys()
    {
        ColorsSettingsPage page = new();
        ITranslation translation = Substitute.For<ITranslation>();

        page.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(ColorsSettingsPage), "gbRevisionGraph", "Text", "Revision graph");
        translation.Received(1).AddTranslationItem(
            nameof(ColorsSettingsPage), "MulticolorBranches", "Text", "Multicolor branches");
        translation.Received(1).AddTranslationItem(
            nameof(ColorsSettingsPage), "lblRestartNeeded", "Text", "Restart required to apply changes");
        translation.Received(1).AddTranslationItem(
            nameof(ColorsSettingsPage), "chkUseSystemVisualStyle", "Text", "Use system-defined visual style (looks bad with dark colors)");
    }

    [AvaloniaTest]
    public void FormSettings_should_register_and_navigate_to_the_git_paths_page()
    {
        FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();

        accessor.SettingsTreeView.SettingsPages.OfType<GitSettingsGroup>().Should().ContainSingle();
        GitSettingsPage page = accessor.SettingsTreeView.SettingsPages.OfType<GitSettingsPage>().Single();
        form.GotoPage(GitSettingsPage.GetPageReference());

        SettingsPageHeader header = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        header.GetTestAccessor().Page.Should().BeSameAs(page);
        page.GetTitle().Should().Be("Paths");
    }

    [AvaloniaTest]
    public void Git_settings_should_load_save_and_hide_windows_only_tools_on_native_unix()
    {
        string originalGitCommand = AppSettings.GitCommandValue;
        string originalLinuxToolsDirectory = AppSettings.LinuxToolsDir;
        string? originalPath = Environment.GetEnvironmentVariable("PATH");
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), $"gitext-git-settings-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);
        File.WriteAllText(Path.Join(temporaryDirectory, OperatingSystem.IsWindows() ? "sh.exe" : "sh"), string.Empty);
        try
        {
            string gitExecutableName = OperatingSystem.IsWindows() ? "git.exe" : "git";
            PathUtil.TryFindFullPath(gitExecutableName, out string? gitPath).Should().BeTrue();
            AppSettings.GitCommandValue = gitPath!;
            if (OperatingSystem.IsWindows())
            {
                AppSettings.LinuxToolsDir = temporaryDirectory;
            }

            FormSettings form = new();
            FormSettings.TestAccessor formAccessor = form.GetTestAccessor();
            formAccessor.InitializePages();
            GitSettingsPage page = formAccessor.SettingsTreeView.SettingsPages.OfType<GitSettingsPage>().Single();
            page.LoadSettings();

            GitSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            accessor.GitPath.Text.Should().Be(gitPath);
            accessor.EnvironmentStatus.Text.Should().Contain(OperatingSystem.IsWindows() ? "%HOME%" : "$HOME");
            accessor.LinuxToolsDir.IsVisible.Should().Be(OperatingSystem.IsWindows());
            accessor.LinuxToolsLabel.IsVisible.Should().Be(OperatingSystem.IsWindows());
            accessor.DownloadGit.IsVisible.Should().Be(OperatingSystem.IsWindows());

            accessor.GitPath.Text = "git";
            if (OperatingSystem.IsWindows())
            {
                accessor.LinuxToolsDir.Text = temporaryDirectory;
            }

            page.SaveSettings();
            AppSettings.GitCommandValue.Should().Be("git");
            if (OperatingSystem.IsWindows())
            {
                Path.TrimEndingDirectorySeparator(AppSettings.LinuxToolsDir).Should().Be(temporaryDirectory);
            }
        }
        finally
        {
            AppSettings.GitCommandValue = originalGitCommand;
            AppSettings.LinuxToolsDir = originalLinuxToolsDirectory;
            Environment.SetEnvironmentVariable("PATH", originalPath);
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [AvaloniaTest]
    public void FormFixHome_should_apply_a_readable_custom_home()
    {
        string originalCustomHome = AppSettings.CustomHomeDir;
        bool originalUserProfileHome = AppSettings.UserProfileHomeDir;
        string? originalHome = Environment.GetEnvironmentVariable("HOME");
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), $"gitext-home-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);
        File.WriteAllText(Path.Join(temporaryDirectory, ".gitconfig"), "[user]");
        try
        {
            FormFixHome form = new();
            FormFixHome.TestAccessor accessor = form.GetTestAccessor();
            accessor.LoadSettings();
            accessor.OtherHome.IsChecked = true;
            accessor.OtherHomeDirectory.Text = temporaryDirectory;

            accessor.ApplySettings().Should().BeTrue();

            AppSettings.CustomHomeDir.Should().Be(temporaryDirectory);
            AppSettings.UserProfileHomeDir.Should().BeFalse();
            EnvironmentConfiguration.GetHomeDir().Should().Be(temporaryDirectory);
            FormFixHome.TestAccessor.HasGlobalGitConfig(temporaryDirectory).Should().BeTrue();
        }
        finally
        {
            AppSettings.CustomHomeDir = originalCustomHome;
            AppSettings.UserProfileHomeDir = originalUserProfileHome;
            Environment.SetEnvironmentVariable("HOME", originalHome);
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [AvaloniaTest]
    public void Git_and_home_settings_should_preserve_original_translation_keys()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        GitSettingsPage page = new();
        FormFixHome home = new();

        page.AddTranslationItems(translation);
        home.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(GitSettingsPage), "lblGitCommand", "Text", "Command used to run git (git.cmd or git.exe)");
        translation.Received(1).AddTranslationItem(
            nameof(GitSettingsPage), "ChangeHomeButton", "Text", "Change HOME");
        translation.Received(1).AddTranslationItem(
            nameof(FormFixHome), "groupBox8", "Text", "Environment");
        translation.Received(1).AddTranslationItem(
            nameof(FormFixHome), "otherHome", "Text", "&Other");
    }

    [AvaloniaTest]
    public void Settings_tree_should_preserve_root_replacement_navigation_and_search()
    {
        SettingsTreeViewUserControl tree = new();
        TestPage rootPage = new("Root settings", "root keyword");
        TestPage childPage = new("Child page", "searchable option");
        SettingsPageReference rootReference = new SettingsPageReferenceByType(typeof(RootGroup));

        tree.AddSettingsPage(new RootGroup(), parentPageReference: null, icon: null);
        tree.AddSettingsPage(rootPage, rootReference, icon: null, asRoot: true);
        tree.AddSettingsPage(childPage, rootReference, icon: null);

        List<SettingsPageSelectedEventArgs> selections = [];
        tree.SettingsPageSelected += (_, e) => selections.Add(e);

        Window window = new() { Content = tree, Width = 320, Height = 300 };
        window.Show();
        try
        {
            tree.GotoPage(childPage.PageReference);
            selections.Should().ContainSingle();
            selections[0].SettingsPage.Should().BeSameAs(childPage);
            selections[0].IsTriggeredByGoto.Should().BeTrue();

            TextBox search = tree.FindControl<TextBox>("textBoxFind")
                ?? throw new InvalidOperationException("Settings search box was not created.");
            search.Text = "searchable";
            Dispatcher.UIThread.RunJobs();

            TreeViewItem childNode = (tree.FindControl<TreeView>("treeView1")
                    ?? throw new InvalidOperationException("Settings tree was not created."))
                .Items.OfType<TreeViewItem>().Single()
                .Items.OfType<TreeViewItem>().Single();
            childNode.Classes.Should().Contain("settings-search-match");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void SettingsPageBase_should_run_shared_bindings_and_track_loading_state()
    {
        TestSettingsPage page = new();
        TestBinding binding = new();
        page.AddControlBinding(binding);
        page.AttachHost(new TestHost());

        page.LoadSettings();
        binding.LoadCount.Should().Be(1);
        page.WasLoadingDuringLoad.Should().BeTrue();
        page.SettingsLoaded.Should().BeTrue();

        page.SaveSettings();
        binding.SaveCount.Should().Be(1);
    }

    private sealed class RootGroup : GroupSettingsPage
    {
        public RootGroup()
            : base("Root")
        {
        }
    }

    private sealed class TestPage(string title, params string[] keywords) : ISettingsPage
    {
        public Control GuiControl { get; } = new Border();

        public bool IsInstantSavePage => false;

        public SettingsPageReference PageReference { get; } = new TestPageReference(Guid.NewGuid());

        public string GetTitle() => title;

        public IEnumerable<string> GetSearchKeywords() => keywords;

        public void OnPageShown()
        {
        }

        public void LoadSettings()
        {
        }

        public void SaveSettings()
        {
        }
    }

    private sealed class TestPageReference(Guid id) : SettingsPageReference
    {
        public override bool Equals(object? obj) => obj is TestPageReference other && other.Id == Id;

        public override int GetHashCode() => Id.GetHashCode();

        private Guid Id { get; } = id;
    }

    private sealed class TestSettingsPage : SettingsPageBase
    {
        private readonly TestSettingsSource _settings = new();

        public TestSettingsPage()
            : base(EmptyServiceProvider.Instance)
        {
            Text = "Test";
        }

        public bool WasLoadingDuringLoad { get; private set; }

        public bool SettingsLoaded => IsSettingsLoaded;

        public void AttachHost(ISettingsPageHost host) => Init(host);

        protected override SettingsSource GetCurrentSettings()
        {
            WasLoadingDuringLoad = IsLoadingSettings;
            return _settings;
        }
    }

    private sealed class TestBinding : ISettingControlBinding
    {
        public int LoadCount { get; private set; }

        public int SaveCount { get; private set; }

        public string Caption() => string.Empty;

        public WinFormsShims.Control GetControl() => new();

        public GitExtensions.Extensibility.Settings.ISetting GetSetting()
            => throw new NotSupportedException();

        public void LoadSetting(SettingsSource settings) => LoadCount++;

        public void SaveSetting(SettingsSource settings) => SaveCount++;
    }

    private sealed class TestSettingsSource : SettingsSource
    {
        public override string? GetValue(string name) => null;

        public override void SetValue(string name, string? value)
        {
        }
    }

    private sealed class TestHost : ISettingsPageHost
    {
        public CheckSettingsLogic CheckSettingsLogic => throw new NotSupportedException();

        public void GotoPage(SettingsPageReference settingsPageReference)
        {
        }

        public void LoadAll()
        {
        }

        public void SaveAll()
        {
        }
    }
}
