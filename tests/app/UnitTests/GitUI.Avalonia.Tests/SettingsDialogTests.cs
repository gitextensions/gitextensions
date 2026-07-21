using System.Text;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.Compat;
using GitUIPluginInterfaces;
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
    public void FormSettings_should_register_and_navigate_to_both_git_config_pages()
    {
        FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();

        GitConfigSettingsPage configPage = accessor.SettingsTreeView.SettingsPages
            .OfType<GitConfigSettingsPage>()
            .Single();
        GitConfigAdvancedSettingsPage advancedPage = accessor.SettingsTreeView.SettingsPages
            .OfType<GitConfigAdvancedSettingsPage>()
            .Single();

        form.GotoPage(GitConfigSettingsPage.GetPageReference());
        SettingsPageHeader configHeader = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        configHeader.GetTestAccessor().Page.Should().BeSameAs(configPage);
        configPage.GetTitle().Should().Be("Git config");

        form.GotoPage(new SettingsPageReferenceByType(typeof(GitConfigAdvancedSettingsPage)));
        SettingsPageHeader advancedHeader = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        advancedHeader.GetTestAccessor().Page.Should().BeSameAs(advancedPage);
        advancedPage.GetTitle().Should().Be("Advanced");
    }

    [AvaloniaTest]
    public void Git_config_pages_should_expose_the_original_editable_fields_and_tri_state_options()
    {
        GitConfigSettingsPage configPage = new();
        GitConfigSettingsPage.TestAccessor config = configPage.GetTestAccessor();
        GitConfigAdvancedSettingsPage advancedPage = new();

        config.Editor.IsEditable.Should().BeTrue();
        config.CredentialHelper.IsEditable.Should().BeTrue();
        config.MergeTool.IsEditable.Should().BeTrue();
        config.DiffTool.IsEditable.Should().BeTrue();
        config.AutoCrlfOptions.Should().HaveCount(4);
        config.MergeTool.Items.Should().NotBeEmpty();
        config.DiffTool.Items.Should().NotBeEmpty();

        IReadOnlyList<CheckBox> advancedSettings = advancedPage.GetTestAccessor().Settings;
        advancedSettings.Should().HaveCount(8);
        advancedSettings.Should().OnlyContain(checkBox => checkBox.IsThreeState);
    }

    [AvaloniaTest]
    public void Available_encodings_dialog_should_partition_encodings_without_duplicates()
    {
        FormAvailableEncodings form = new();
        FormAvailableEncodings.TestAccessor accessor = form.GetTestAccessor();

        accessor.Included.Select(encoding => encoding.WebName)
            .Should().OnlyHaveUniqueItems();
        accessor.Available.Select(encoding => encoding.WebName)
            .Should().OnlyHaveUniqueItems();
        accessor.Included.Select(encoding => encoding.WebName)
            .Intersect(accessor.Available.Select(encoding => encoding.WebName))
            .Should().BeEmpty();

        Encoding? available = accessor.Available.FirstOrDefault();
        if (available is not null)
        {
            form.FindControl<ListBox>("ListAvailableEncodings")!.SelectedItem = available;
            accessor.Add.IsEnabled.Should().BeTrue();
        }
    }

    [Test]
    public void Git_config_controller_should_prefer_an_existing_supplied_location()
    {
        GitConfigSettingsPageController controller = new();
        string temporaryDirectory = Path.GetTempPath();
        string temporaryFile = Path.GetTempFileName();
        try
        {
            controller.GetInitialDirectory(temporaryFile, string.Empty)
                .Should().Be(temporaryDirectory);
        }
        finally
        {
            File.Delete(temporaryFile);
        }
    }

    [AvaloniaTest]
    public void Git_config_surfaces_should_preserve_original_translation_keys()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        GitConfigSettingsPage config = new();
        GitConfigAdvancedSettingsPage advanced = new();
        FormAvailableEncodings encodings = new();

        config.AddTranslationItems(translation);
        advanced.AddTranslationItems(translation);
        encodings.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(GitConfigSettingsPage), "label3", "Text", "User name");
        translation.Received(1).AddTranslationItem(
            nameof(GitConfigSettingsPage), "globalAutoCrlfNotSet", "Text", "Not set");
        translation.Received(1).AddTranslationItem(
            nameof(GitConfigAdvancedSettingsPage), "checkBoxPullRebase", "Text", "Rebase local branch when pulling (instead of merge)");
        translation.Received(1).AddTranslationItem(
            nameof(FormAvailableEncodings), "lAvaolableEncodings", "Text", "Available:");
    }

    [AvaloniaTest]
    public void FormSettings_should_show_the_git_introduction_when_the_git_root_is_selected()
    {
        FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();
        GitRootIntroductionPage introduction = accessor.SettingsTreeView.SettingsPages
            .OfType<GitRootIntroductionPage>()
            .Single();

        form.GotoPage(GitSettingsGroup.GetPageReference());

        accessor.CurrentPage.Should().BeSameAs(introduction);
        introduction.GetTitle().Should().Be("Git Settings");
    }

    [AvaloniaTest]
    public void Git_root_introduction_should_preserve_original_translation_keys()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        GitRootIntroductionPage page = new();

        page.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(GitRootIntroductionPage), "label1", "Text", "Select one of the subnodes to view or edit the Git settings");
    }

    [AvaloniaTest]
    public void FormSettings_should_register_appearance_and_keep_colors_beneath_its_page_reference()
    {
        FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();
        AppearanceSettingsPage appearance = accessor.SettingsTreeView.SettingsPages
            .OfType<AppearanceSettingsPage>()
            .Single();

        form.GotoPage(AppearanceSettingsPage.GetPageReference());

        SettingsPageHeader header = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        header.GetTestAccessor().Page.Should().BeSameAs(appearance);
        accessor.SettingsTreeView.SettingsPages.OfType<ColorsSettingsPage>().Should().ContainSingle();
        appearance.GetTitle().Should().Be("Appearance");
    }

    [AvaloniaTest]
    public void Appearance_settings_should_map_supported_values_without_touching_deferred_avatar_settings()
    {
        bool originalRelativeDate = AppSettings.RelativeDate;
        bool originalRepositoryBranch = AppSettings.ShowRepoCurrentBranch;
        bool originalVisualStudioBranch = AppSettings.ShowCurrentBranchInVisualStudio;
        bool originalAutoScale = AppSettings.EnableAutoScale;
        TruncatePathMethod originalTruncation = AppSettings.TruncatePathMethod;
        string originalTranslation = AppSettings.Translation;
        string originalDictionary = AppSettings.Dictionary;
        AvatarProvider originalAvatarProvider = AppSettings.AvatarProvider;
        AvatarFallbackType originalFallback = AppSettings.AvatarFallbackType;
        string originalAvatarTemplate = AppSettings.CustomAvatarTemplate;
        try
        {
            AppSettings.RelativeDate = true;
            AppSettings.ShowRepoCurrentBranch = false;
            AppSettings.ShowCurrentBranchInVisualStudio = true;
            AppSettings.EnableAutoScale = false;
            AppSettings.TruncatePathMethod = TruncatePathMethod.TrimStart;
            AppSettings.Translation = "English";
            AppSettings.Dictionary = "none";

            AppearanceSettingsPage page = new();
            page.LoadSettings();
            AppearanceSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            accessor.ShowRelativeDate.IsChecked.Should().BeTrue();
            accessor.ShowRepositoryBranch.IsChecked.Should().BeFalse();
            accessor.ShowVisualStudioBranch.IsChecked.Should().BeTrue();
            accessor.ShowVisualStudioBranch.IsVisible.Should().Be(OperatingSystem.IsWindows());
            accessor.EnableAutoScale.IsChecked.Should().BeFalse();
            accessor.TruncatePathMethod.SelectedIndex.Should().Be(2);
            accessor.AuthorImages.IsVisible.Should().BeFalse();

            accessor.ShowRelativeDate.IsChecked = false;
            accessor.ShowRepositoryBranch.IsChecked = true;
            accessor.EnableAutoScale.IsChecked = true;
            accessor.TruncatePathMethod.SelectedIndex = 3;
            page.SaveSettings();

            AppSettings.RelativeDate.Should().BeFalse();
            AppSettings.ShowRepoCurrentBranch.Should().BeTrue();
            AppSettings.EnableAutoScale.Should().BeTrue();
            AppSettings.TruncatePathMethod.Should().Be(TruncatePathMethod.FileNameOnly);
            AppSettings.AvatarProvider.Should().Be(originalAvatarProvider);
            AppSettings.AvatarFallbackType.Should().Be(originalFallback);
            AppSettings.CustomAvatarTemplate.Should().Be(originalAvatarTemplate);
        }
        finally
        {
            AppSettings.RelativeDate = originalRelativeDate;
            AppSettings.ShowRepoCurrentBranch = originalRepositoryBranch;
            AppSettings.ShowCurrentBranchInVisualStudio = originalVisualStudioBranch;
            AppSettings.EnableAutoScale = originalAutoScale;
            AppSettings.TruncatePathMethod = originalTruncation;
            AppSettings.Translation = originalTranslation;
            AppSettings.Dictionary = originalDictionary;
            AppSettings.AvatarProvider = originalAvatarProvider;
            AppSettings.AvatarFallbackType = originalFallback;
            AppSettings.CustomAvatarTemplate = originalAvatarTemplate;
        }
    }

    [AvaloniaTest]
    public void Appearance_settings_should_preserve_original_translation_keys()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        AppearanceSettingsPage page = new();

        page.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(AppearanceSettingsPage), "gbGeneral", "Text", "&General");
        translation.Received(1).AddTranslationItem(
            nameof(AppearanceSettingsPage), "truncatePathMethod", "Item0", "None");
        translation.Received(1).AddTranslationItem(
            nameof(AppearanceSettingsPage), "lblLanguage", "Text", "Language (restart required)");
        translation.Received(1).AddTranslationItem(
            nameof(AppearanceSettingsPage), "ShowAuthorAvatarInCommitGraph", "Text", "Show author's avatar column in the commit graph");
    }

    [AvaloniaTest]
    public void FormSettings_should_register_sorting_beneath_appearance_and_navigate_to_it()
    {
        FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();
        SortingSettingsPage sorting = accessor.SettingsTreeView.SettingsPages
            .OfType<SortingSettingsPage>()
            .Single();

        TreeView tree = accessor.SettingsTreeView.FindControl<TreeView>("treeView1")
            ?? throw new InvalidOperationException("The settings tree was not created.");
        TreeViewItem appearanceNode = tree.Items
            .OfType<TreeViewItem>()
            .SelectMany(node => node.Items.OfType<TreeViewItem>())
            .Single(node => node.Tag is AppearanceSettingsPage);
        appearanceNode.Items
            .OfType<TreeViewItem>()
            .Select(node => node.Tag)
            .Should().ContainInOrder(sorting, accessor.SettingsTreeView.SettingsPages.OfType<ColorsSettingsPage>().Single());

        form.GotoPage(SortingSettingsPage.GetPageReference());

        SettingsPageHeader header = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        header.GetTestAccessor().Page.Should().BeSameAs(sorting);
        sorting.GetTitle().Should().Be("Sorting");
    }

    [AvaloniaTest]
    public void Sorting_settings_should_preserve_enum_order_and_roundtrip_all_values()
    {
        RevisionSortOrder originalRevisionSort = AppSettings.RevisionSortOrder.Value;
        GitRefsSortBy originalRefsSortBy = AppSettings.RefsSortBy;
        GitRefsSortOrder originalRefsSortOrder = AppSettings.RefsSortOrder;
        string originalPrioritizedBranches = AppSettings.PrioritizedBranchNames;
        string originalPrioritizedRemotes = AppSettings.PrioritizedRemoteNames;
        try
        {
            AppSettings.RevisionSortOrder.Value = RevisionSortOrder.AuthorDate;
            AppSettings.RefsSortBy = GitRefsSortBy.refname;
            AppSettings.RefsSortOrder = GitRefsSortOrder.Ascending;
            AppSettings.PrioritizedBranchNames = "main;release/.*";
            AppSettings.PrioritizedRemoteNames = "upstream;origin";

            SortingSettingsPage page = new();
            page.LoadSettings();
            SortingSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            accessor.RevisionsSortBy.SelectedIndex.Should().Be((int)RevisionSortOrder.AuthorDate);
            accessor.BranchesSortBy.SelectedIndex.Should().Be((int)GitRefsSortBy.refname);
            accessor.BranchesOrder.SelectedIndex.Should().Be((int)GitRefsSortOrder.Ascending);
            accessor.PrioritizedBranchNames.Text.Should().Be("main;release/.*");
            accessor.PrioritizedRemoteNames.Text.Should().Be("upstream;origin");
            accessor.RevisionSortItems.Should().Equal("GitDefault", "AuthorDate", "Topology");
            accessor.BranchSortItems.Should().Equal(
                "Git default",
                "Author date",
                "Committer date",
                "Creator date",
                "Tagger date",
                "Alpha-numeric",
                "Version",
                "Object size",
                "Originating remote");
            accessor.BranchOrderItems.Should().Equal("A ↓ Z", "Z ↑ A");

            accessor.RevisionsSortBy.SelectedIndex = (int)RevisionSortOrder.Topology;
            accessor.BranchesSortBy.SelectedIndex = (int)GitRefsSortBy.upstream;
            accessor.BranchesOrder.SelectedIndex = (int)GitRefsSortOrder.Descending;
            accessor.PrioritizedBranchNames.Text = "develop;feature/.*";
            accessor.PrioritizedRemoteNames.Text = "origin;company";
            page.SaveSettings();

            AppSettings.RevisionSortOrder.Value.Should().Be(RevisionSortOrder.Topology);
            AppSettings.RefsSortBy.Should().Be(GitRefsSortBy.upstream);
            AppSettings.RefsSortOrder.Should().Be(GitRefsSortOrder.Descending);
            AppSettings.PrioritizedBranchNames.Should().Be("develop;feature/.*");
            AppSettings.PrioritizedRemoteNames.Should().Be("origin;company");
        }
        finally
        {
            AppSettings.RevisionSortOrder.Value = originalRevisionSort;
            AppSettings.RevisionSortOrder.Save();
            AppSettings.RefsSortBy = originalRefsSortBy;
            AppSettings.RefsSortOrder = originalRefsSortOrder;
            AppSettings.PrioritizedBranchNames = originalPrioritizedBranches;
            AppSettings.PrioritizedRemoteNames = originalPrioritizedRemotes;
        }
    }

    [AvaloniaTest]
    public void Sorting_settings_should_preserve_original_translation_keys_and_help_text()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        SortingSettingsPage page = new();

        page.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(SortingSettingsPage), "gbGeneral", "Text", "Sorting");
        translation.Received(1).AddTranslationItem(
            nameof(SortingSettingsPage), "lblBranchesSortBy", "Text", "Sort branches by");
        translation.Received(1).AddTranslationItem(
            nameof(SortingSettingsPage), "_revisionSortWarningTooltip", "Text", "Sorting revisions may delay rendering of the revision graph.");
        translation.Received(1).AddTranslationItem(
            nameof(SortingSettingsPage), "_prioRemoteNamesTooltip", "Text", "Regex to prioritize remote names in the left panel and commit info.\nThe remotes matching the pattern will be shown before the others.\nSeparate the priorities with ';'.");

        SortingSettingsPage.TestAccessor accessor = page.GetTestAccessor();
        ToolTip.GetTip(accessor.RevisionSortOrderHelp)
            .Should().BeOfType<TextBlock>().Which.Text.Should().Be("Sorting revisions may delay rendering of the revision graph.");
        ToolTip.GetTip(accessor.PrioBranchNamesHelp)
            .Should().BeOfType<TextBlock>().Which.Text.Should().Contain("prioritize branch names");
        ToolTip.GetTip(accessor.PrioRemoteNamesHelp)
            .Should().BeOfType<TextBlock>().Which.Text.Should().Contain("prioritize remote names");
    }

    [AvaloniaTest]
    public void FormSettings_should_register_fonts_after_colors_beneath_appearance_and_navigate_to_it()
    {
        FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();
        AppearanceFontsSettingsPage fonts = accessor.SettingsTreeView.SettingsPages
            .OfType<AppearanceFontsSettingsPage>()
            .Single();

        TreeView tree = accessor.SettingsTreeView.FindControl<TreeView>("treeView1")
            ?? throw new InvalidOperationException("The settings tree was not created.");
        TreeViewItem appearanceNode = tree.Items
            .OfType<TreeViewItem>()
            .SelectMany(node => node.Items.OfType<TreeViewItem>())
            .Single(node => node.Tag is AppearanceSettingsPage);
        appearanceNode.Items
            .OfType<TreeViewItem>()
            .Select(node => node.Tag)
            .Should().ContainInOrder(
                accessor.SettingsTreeView.SettingsPages.OfType<SortingSettingsPage>().Single(),
                accessor.SettingsTreeView.SettingsPages.OfType<ColorsSettingsPage>().Single(),
                fonts);

        form.GotoPage(AppearanceFontsSettingsPage.GetPageReference());

        SettingsPageHeader header = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        header.GetTestAccessor().Page.Should().BeSameAs(fonts);
        fonts.GetTitle().Should().Be("Fonts");
    }

    [AvaloniaTest]
    public void Appearance_font_settings_should_roundtrip_fonts_and_end_of_line_marker_style()
    {
        WinFormsShims.Font originalDiffFont = AppSettings.FixedWidthFont;
        WinFormsShims.Font originalApplicationFont = AppSettings.Font;
        WinFormsShims.Font originalCommitFont = AppSettings.CommitFont;
        WinFormsShims.Font originalMonospaceFont = AppSettings.MonospaceFont;
        bool originalShowEolMarkerAsGlyph = AppSettings.ShowEolMarkerAsGlyph;
        try
        {
            AppSettings.FixedWidthFont = new WinFormsShims.Font("Consolas", 10, WinFormsShims.FontStyle.Bold);
            AppSettings.Font = new WinFormsShims.Font("Segoe UI", 9);
            AppSettings.CommitFont = new WinFormsShims.Font("Arial", 11, WinFormsShims.FontStyle.Italic);
            AppSettings.MonospaceFont = new WinFormsShims.Font("Courier New", 12);
            AppSettings.ShowEolMarkerAsGlyph = true;

            AppearanceFontsSettingsPage page = new();
            page.LoadSettings();
            AppearanceFontsSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            accessor.DiffFont.Content.Should().Be("Consolas, 10");
            accessor.DiffFont.FontWeight.Should().Be(FontWeight.Bold);
            accessor.ApplicationFont.Content.Should().Be("Segoe UI, 9");
            accessor.CommitFont.Content.Should().Be("Arial, 11");
            accessor.CommitFont.FontStyle.Should().Be(FontStyle.Italic);
            accessor.MonospaceFont.Content.Should().Be("Courier New, 12");
            accessor.ShowEolMarkerAsGlyph.IsChecked.Should().BeTrue();
            accessor.DiffDialogFixedPitchOnly.Should().BeTrue();

            WinFormsShims.Font savedDiff = new("Cascadia Mono", 13, WinFormsShims.FontStyle.Italic);
            WinFormsShims.Font savedApplication = new("Tahoma", 10, WinFormsShims.FontStyle.Bold);
            WinFormsShims.Font savedCommit = new("Verdana", 14);
            WinFormsShims.Font savedMonospace = new(
                "Lucida Console",
                15,
                WinFormsShims.FontStyle.Bold | WinFormsShims.FontStyle.Italic);
            accessor.SetFonts(savedDiff, savedApplication, savedCommit, savedMonospace);
            accessor.ShowEolMarkerAsGlyph.IsChecked = false;
            page.SaveSettings();

            AppSettings.FixedWidthFont.Name.Should().Be(savedDiff.Name);
            AppSettings.FixedWidthFont.Size.Should().Be(savedDiff.Size);
            AppSettings.FixedWidthFont.Style.Should().Be(savedDiff.Style);
            AppSettings.Font.Name.Should().Be(savedApplication.Name);
            AppSettings.Font.Size.Should().Be(savedApplication.Size);
            AppSettings.Font.Style.Should().Be(savedApplication.Style);
            AppSettings.CommitFont.Name.Should().Be(savedCommit.Name);
            AppSettings.CommitFont.Size.Should().Be(savedCommit.Size);
            AppSettings.CommitFont.Style.Should().Be(savedCommit.Style);
            AppSettings.MonospaceFont.Name.Should().Be(savedMonospace.Name);
            AppSettings.MonospaceFont.Size.Should().Be(savedMonospace.Size);
            AppSettings.MonospaceFont.Style.Should().Be(savedMonospace.Style);
            AppSettings.ShowEolMarkerAsGlyph.Should().BeFalse();
        }
        finally
        {
            AppSettings.FixedWidthFont = originalDiffFont;
            AppSettings.Font = originalApplicationFont;
            AppSettings.CommitFont = originalCommitFont;
            AppSettings.MonospaceFont = originalMonospaceFont;
            AppSettings.ShowEolMarkerAsGlyph = originalShowEolMarkerAsGlyph;
        }
    }

    [AvaloniaTest]
    public void Appearance_font_settings_should_preserve_original_translation_keys()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        AppearanceFontsSettingsPage page = new();

        page.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(AppearanceFontsSettingsPage), "gbFonts", "Text", "Fonts (restart required)");
        translation.Received(1).AddTranslationItem(
            nameof(AppearanceFontsSettingsPage), "label56", "Text", "Code font");
        translation.Received(1).AddTranslationItem(
            nameof(AppearanceFontsSettingsPage), "diffFontChangeButton", "Text", "font name");
        translation.Received(1).AddTranslationItem(
            nameof(AppearanceFontsSettingsPage), "ShowEolMarkerAsGlyph", "Text", "Show end-of-line markers as glyph instead of \"\\r\\n\" etc.");
        translation.Received(1).AddTranslationItem(
            nameof(AppearanceFontsSettingsPage), "label36", "Text", "Monospace font");
    }

    [AvaloniaTest]
    public void Font_dialog_should_build_the_selected_font_and_refresh_its_preview()
    {
        WinFormsShims.Font original = new("Arial", 9);
        FontDialogWindow dialog = new(original, fixedPitchOnly: false);
        FontDialogWindow.TestAccessor accessor = dialog.GetTestAccessor();

        accessor.FontFamily.Text = "Courier New";
        accessor.FontSize.Value = 13.5m;
        accessor.Bold.IsChecked = true;
        accessor.Italic.IsChecked = true;
        accessor.Accept();

        dialog.SelectedFont.Name.Should().Be("Courier New");
        dialog.SelectedFont.Size.Should().Be(13.5F);
        dialog.SelectedFont.Style.Should().Be(
            WinFormsShims.FontStyle.Bold | WinFormsShims.FontStyle.Italic);
        accessor.Preview.FontFamily.Name.Should().Be("Courier New");
        accessor.Preview.FontWeight.Should().Be(FontWeight.Bold);
        accessor.Preview.FontStyle.Should().Be(FontStyle.Italic);
        dialog.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
    }

    [AvaloniaTest]
    public void Font_dialog_fixed_pitch_list_should_only_include_fixed_pitch_system_fonts()
    {
        IReadOnlyList<string> allFonts = FontDialog.GetSystemFontNames(fixedPitchOnly: false);
        IReadOnlyList<string> fixedPitchFonts = FontDialog.GetSystemFontNames(fixedPitchOnly: true);

        fixedPitchFonts.Should().OnlyContain(name => allFonts.Contains(name));
        fixedPitchFonts
            .Select(name => new FontFamily(name))
            .Should().OnlyContain(fontFamily => FontDialog.IsFixedPitch(fontFamily));
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
