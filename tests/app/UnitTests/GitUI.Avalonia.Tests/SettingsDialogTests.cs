using System.Text;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.ExternalLinks;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.CommandsDialogs.SettingsDialog.RevisionLinks;
using GitUI.Compat;
using GitUI.ConsoleEmulation;
using GitUI.Hotkey;
using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using ResourceManager.Hotkey;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class SettingsDialogTests
{
    [SetUp]
    public void SetUp()
    {
        GitUI.ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void FormBrowse_should_expose_the_live_settings_toolbar_action()
    {
        FormBrowse form = new();

        Button settings = form.FindControl<Button>("EditSettings")
            ?? throw new InvalidOperationException("Settings toolbar button was not created.");
        settings.Content.Should().Be("Settings");
        settings.IsVisible.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormSettings_should_register_the_complete_checklist_and_its_ssh_repair_page()
    {
        using FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();
        ChecklistSettingsPage checklist = accessor.SettingsTreeView.SettingsPages
            .OfType<ChecklistSettingsPage>()
            .Single();
        SshSettingsPage ssh = accessor.SettingsTreeView.SettingsPages
            .OfType<SshSettingsPage>()
            .Single();

        checklist.SshSettingsPage.Should().BeSameAs(ssh);
        string[] originalStatusNames =
        [
            "GitFound",
            "UserNameSet",
            "MergeTool",
            "DiffTool",
            "ShellExtensionsRegistered",
            "GitBinFound",
            "GitExtensionsInstall",
            "SshConfig",
            "translationConfig",
            "GcmDetected",
        ];
        originalStatusNames.All(name => checklist.FindControl<Button>(name) is not null)
            .Should().BeTrue();
        originalStatusNames
            .Select(name => name == "GcmDetected" ? "GcmDetectedFix" : $"{name}_Fix")
            .All(name => checklist.FindControl<Button>(name) is not null)
            .Should().BeTrue();

        ITranslation translation = Substitute.For<ITranslation>();
        ssh.AddTranslationItems(translation);
        translation.Received(1).AddTranslationItem(
            nameof(SshSettingsPage),
            "label18",
            "Text",
            Arg.Is<string>(text => text.StartsWith("OpenSSH is a commandline tool.", StringComparison.Ordinal)));
        translation.Received(1).AddTranslationItem(
            nameof(SshSettingsPage),
            "PlinkBrowse",
            "Text",
            "Browse");
    }

    [AvaloniaTest]
    public void Checklist_repairs_should_route_unconfigured_identity_and_diff_tools_to_global_settings()
    {
        TestHost host = new();
        ChecklistSettingsPage checklist = SettingsPageBase.Create<ChecklistSettingsPage>(
            host,
            EmptyServiceProvider.Instance);

        checklist.FindControl<Button>("UserNameSet_Fix")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        checklist.FindControl<Button>("MergeTool_Fix")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        checklist.FindControl<Button>("DiffTool_Fix")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        host.GotoPages.Should().HaveCount(3);
        host.GotoPages.Should().OnlyContain(reference => reference.Equals(GitConfigSettingsPage.GetPageReference()));
    }

    [AvaloniaTest]
    public void FormSettings_should_host_the_checklist_root_and_preserve_dialog_chrome()
    {
        FormSettings form = new();
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();

            FormSettings.TestAccessor accessor = form.GetTestAccessor();
            SettingsPageHeader header = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
            header.GetTestAccessor().Page.Should().BeOfType<ChecklistSettingsPage>();
            accessor.OkButton.IsDefault.Should().BeTrue();
            accessor.CancelButton.Content.Should().Be("Cancel");
            accessor.InstantSaveNotice.IsVisible.Should().BeTrue();
            form.Title.Should().Be("Settings - Checklist");
            form.CaptureRenderedFrame().Should().NotBeNull();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormSettings_should_apply_discard_and_cancel_through_the_original_page_lifecycle()
    {
        FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        TestPage page = new("Lifecycle");
        accessor.SettingsTreeView.AddSettingsPage(page, parentPageReference: null, icon: null);
        form.GotoPage(page.PageReference);

        accessor.DiscardButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        accessor.ApplyButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        page.LoadCount.Should().Be(1);
        page.SaveCount.Should().Be(1);

        form.Show();
        accessor.CancelButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        page.SaveCount.Should().Be(1, "Cancel must discard deferred page edits without saving them");
    }

    [AvaloniaTest]
    public void FormSettings_should_report_expected_page_validation_failures_and_propagate_unexpected_ones()
    {
        WinFormsShims.IMessageBoxHost? originalMessageBoxHost = TryGetMessageBoxHost();
        StubMessageBoxHost messageBoxes = new();
        WinFormsShims.ShimHost.MessageBoxHost = messageBoxes;
        try
        {
            FormSettings form = new();
            FormSettings.TestAccessor accessor = form.GetTestAccessor();
            TestPage page = new("Validation")
            {
                SaveException = new SaveSettingsException(new InvalidOperationException("Invalid page value")),
            };
            accessor.SettingsTreeView.AddSettingsPage(page, parentPageReference: null, icon: null);

            accessor.SaveSettings().Should().BeFalse();
            messageBoxes.Messages.Should().ContainSingle()
                .Which.Should().Be(("Failed to save all settings", "Invalid page value"));

            page.SaveException = new InvalidOperationException("Unexpected page failure");
            Action save = () => accessor.SaveSettings();
            save.Should().Throw<InvalidOperationException>().WithMessage("Unexpected page failure");
        }
        finally
        {
            WinFormsShims.ShimHost.MessageBoxHost = originalMessageBoxHost ?? new StubMessageBoxHost();
        }

        static WinFormsShims.IMessageBoxHost? TryGetMessageBoxHost()
        {
            try
            {
                return WinFormsShims.ShimHost.MessageBoxHost;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }

    [AvaloniaTest]
    public void FormSettings_modal_close_after_save_should_return_ok_without_reentering_close()
    {
        FormSettings form = new();
        form.GetTestAccessor().MarkSaved();
        Dispatcher.UIThread.Post(form.Close);

        WinFormsShims.DialogResult result = form.ShowDialog(owner: null);

        result.Should().Be(WinFormsShims.DialogResult.OK);
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
            ThemeId selectedTheme = ThemeId.DefaultDark;
            AppSettings.MulticolorBranches = true;
            AppSettings.RevisionGraphDrawAlternateBackColor = false;
            AppSettings.RevisionGraphDrawNonRelativesGray = true;
            AppSettings.RevisionGraphDrawNonRelativesTextGray = false;
            AppSettings.HighlightAuthoredRevisions = true;
            AppSettings.FillRefLabels = false;
            AppSettings.ThemeId = selectedTheme;
            AppSettings.ThemeVariations = [ThemeVariations.Colorblind];
            AppSettings.UseSystemVisualStyle = false;
            AvaloniaThemeSettings.ApplyAppSettings();

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
            page.SelectedThemeId.Should().Be(selectedTheme);
            accessor.Colorblind.IsChecked.Should().BeTrue();
            accessor.Colorblind.IsVisible.Should().BeTrue();
            accessor.UseSystemVisualStyle.IsVisible.Should().BeTrue();
            accessor.OpenThemeFolder.IsVisible.Should().BeTrue();
            accessor.OpenThemeFolders.Items.Should().HaveCount(2);
            accessor.OpenThemeFolders.Items.OfType<MenuItem>().Select(item => item.Header)
                .Should().Equal("Application folder", "User folder");
            accessor.RestartNeeded.IsVisible.Should().BeFalse();

            page.SaveSettings();
            AppSettings.ThemeId.Should().Be(selectedTheme);

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
            AvaloniaThemeSettings.ApplyAppSettings();
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
        translation.Received(1).AddTranslationItem(
            nameof(ColorsSettingsPage), "sbOpenThemeFolder", "Text", "Open theme folder");
        translation.Received(1).AddTranslationItem(
            nameof(ColorsSettingsPage), "chkColorblind", "Text", "Colorblind");
        translation.Received(1).AddTranslationItem(
            nameof(ColorsSettingsPage), "tsmiApplicationFolder", "Text", "Application folder");
        translation.Received(1).AddTranslationItem(
            nameof(ColorsSettingsPage), "tsmiUserFolder", "Text", "User folder");
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
            FormFixHome.TestAccessor.HasGlobalGitConfigForTesting(temporaryDirectory).Should().BeTrue();
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
        configPage.GetTitle().Should().Be("Config");

        form.GotoPage(new SettingsPageReferenceByType(typeof(GitConfigAdvancedSettingsPage)));
        SettingsPageHeader advancedHeader = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        advancedHeader.GetTestAccessor().Page.Should().BeSameAs(advancedPage);
        advancedPage.GetTitle().Should().Be("Advanced");
    }

    [AvaloniaTest]
    public void FormSettings_should_offer_the_supported_built_in_editor_command_first()
    {
        string[] editors = EditorHelper.GetEditors();
        editors[0].Should().Be(EditorHelper.GetBuiltInEditorCommand());
        editors[0].Should().Be(EditorHelper.GetDefaultEditor());
        editors[0].Should().EndWith($"GitExtensions.Avalonia{(OperatingSystem.IsWindows() ? ".exe" : string.Empty)}\" fileeditor");

        FormSettings form = new();
        Action initializePages = () => form.GetTestAccessor().InitializePages();
        initializePages.Should().NotThrow();
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
    public void Appearance_settings_should_map_general_and_avatar_values()
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
        bool originalShowAvatarColumn = AppSettings.ShowAuthorAvatarColumn;
        bool originalShowAvatarInCommitInfo = AppSettings.ShowAuthorAvatarInCommitInfo;
        int originalAvatarCacheDays = AppSettings.AvatarImageCacheDays;
        try
        {
            AppSettings.RelativeDate = true;
            AppSettings.ShowRepoCurrentBranch = false;
            AppSettings.ShowCurrentBranchInVisualStudio = true;
            AppSettings.EnableAutoScale = false;
            AppSettings.TruncatePathMethod = TruncatePathMethod.TrimStart;
            AppSettings.Translation = "English";
            AppSettings.Dictionary = "none";
            AppSettings.ShowAuthorAvatarColumn = true;
            AppSettings.ShowAuthorAvatarInCommitInfo = false;
            AppSettings.AvatarImageCacheDays = 23;

            AppearanceSettingsPage page = new();
            page.LoadSettings();
            AppearanceSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            accessor.ShowRelativeDate.IsChecked.Should().BeTrue();
            accessor.ShowRepositoryBranch.IsChecked.Should().BeFalse();
            accessor.ShowVisualStudioBranch.IsChecked.Should().BeTrue();
            accessor.ShowVisualStudioBranch.IsVisible.Should().Be(OperatingSystem.IsWindows());
            accessor.EnableAutoScale.IsChecked.Should().BeFalse();
            accessor.TruncatePathMethod.SelectedIndex.Should().Be(2);
            accessor.AuthorImages.IsVisible.Should().BeTrue();
            accessor.ShowAvatarInCommitGraph.IsChecked.Should().BeTrue();
            accessor.ShowAvatarInCommitInfo.IsChecked.Should().BeFalse();
            accessor.CacheDays.Value.Should().Be(23);

            accessor.ShowRelativeDate.IsChecked = false;
            accessor.ShowRepositoryBranch.IsChecked = true;
            accessor.EnableAutoScale.IsChecked = true;
            accessor.TruncatePathMethod.SelectedIndex = 3;
            accessor.ShowAvatarInCommitGraph.IsChecked = false;
            accessor.ShowAvatarInCommitInfo.IsChecked = true;
            accessor.CacheDays.Value = 41;
            accessor.AvatarProvider.SelectedIndex = Array.IndexOf(Enum.GetValues<AvatarProvider>(), AvatarProvider.None);
            accessor.AvatarFallback.SelectedIndex = Array.IndexOf(Enum.GetValues<AvatarFallbackType>(), AvatarFallbackType.Retro);
            page.SaveSettings();

            AppSettings.RelativeDate.Should().BeFalse();
            AppSettings.ShowRepoCurrentBranch.Should().BeTrue();
            AppSettings.EnableAutoScale.Should().BeTrue();
            AppSettings.TruncatePathMethod.Should().Be(TruncatePathMethod.FileNameOnly);
            AppSettings.ShowAuthorAvatarColumn.Should().BeFalse();
            AppSettings.ShowAuthorAvatarInCommitInfo.Should().BeTrue();
            AppSettings.AvatarImageCacheDays.Should().Be(41);
            AppSettings.AvatarProvider.Should().Be(AvatarProvider.None);
            AppSettings.AvatarFallbackType.Should().Be(AvatarFallbackType.Retro);
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
            AppSettings.ShowAuthorAvatarColumn = originalShowAvatarColumn;
            AppSettings.ShowAuthorAvatarInCommitInfo = originalShowAvatarInCommitInfo;
            AppSettings.AvatarImageCacheDays = originalAvatarCacheDays;
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
        const string remoteTooltip = "Regex to prioritize remote names in the left panel and commit info.\nThe remotes matching the pattern will be shown before the others.\nSeparate the priorities with ';'.";
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
            nameof(SortingSettingsPage),
            "_prioRemoteNamesTooltip",
            "Text",
            Arg.Is<string>(text => text.ReplaceLineEndings("\n") == remoteTooltip));

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
    public void FormSettings_should_register_console_style_after_fonts_beneath_appearance_and_navigate_to_it()
    {
        FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();
        ConsoleStyleSettingsPage consoleStyle = accessor.SettingsTreeView.SettingsPages
            .OfType<ConsoleStyleSettingsPage>()
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
                accessor.SettingsTreeView.SettingsPages.OfType<AppearanceFontsSettingsPage>().Single(),
                consoleStyle);

        form.GotoPage(ConsoleStyleSettingsPage.GetPageReference());

        SettingsPageHeader header = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        header.GetTestAccessor().Page.Should().BeSameAs(consoleStyle);
        consoleStyle.GetTitle().Should().Be("Console style");
    }

    [AvaloniaTest]
    public void Console_style_settings_should_map_emulator_theme_and_font_with_original_fallbacks()
    {
        string originalEmulator = AppSettings.ConsoleEmulatorName.Value;
        string originalStyle = AppSettings.ConEmuStyle.Value;
        WinFormsShims.Font? originalFont = AppSettings.ConEmuConsoleFont;
        try
        {
            IConsoleEmulator plainText = CreateConsoleEmulator("PlainText", "Plain text", []);
            IConsoleEmulator themed = CreateConsoleEmulator("Themed", "Themed console", ["Dark", "Light"]);
            IConsoleEmulatorsRegistry registry = Substitute.For<IConsoleEmulatorsRegistry>();
            registry.AvailableConsoleEmulators.Returns([plainText, themed]);
            IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IConsoleEmulatorsRegistry)).Returns(registry);

            AppSettings.ConsoleEmulatorName.Value = "themed";
            AppSettings.ConEmuStyle.Value = "dark";
            AppSettings.ConEmuConsoleFont = new WinFormsShims.Font(
                "Courier New",
                11,
                WinFormsShims.FontStyle.Bold | WinFormsShims.FontStyle.Italic);

            ConsoleStyleSettingsPage page = new(serviceProvider);
            page.LoadSettings();
            ConsoleStyleSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            accessor.ConsoleEmulator.SelectedItem.Should().BeSameAs(themed);
            accessor.ConsoleStyle.Items.Cast<string>().Should().Equal("Default", "Dark", "Light");
            accessor.ConsoleStyle.SelectedItem.Should().Be("Dark");
            accessor.ConsoleStyle.IsEnabled.Should().BeTrue();
            accessor.ConsoleFont.Content.Should().Be("Courier New, 11");
            accessor.ConsoleFont.FontWeight.Should().Be(FontWeight.Bold);
            accessor.ConsoleFont.FontStyle.Should().Be(FontStyle.Italic);
            accessor.ConsoleFontReset.IsVisible.Should().BeTrue();

            accessor.ConsoleEmulator.SelectedItem = plainText;
            accessor.ConsoleStyle.Items.Cast<string>().Should().Equal("Default");
            accessor.ConsoleStyle.SelectedIndex.Should().Be(0);
            accessor.ConsoleStyle.IsEnabled.Should().BeFalse();
            WinFormsShims.Font savedFont = new("Cascadia Mono", 13.5F, WinFormsShims.FontStyle.Bold);
            accessor.SetConsoleFont(savedFont);
            page.SaveSettings();

            AppSettings.ConsoleEmulatorName.Value.Should().Be("PlainText");
            AppSettings.ConEmuStyle.Value.Should().Be("Default");
            AppSettings.ConEmuConsoleFont.Should().NotBeNull();
            AppSettings.ConEmuConsoleFont!.Name.Should().Be(savedFont.Name);
            AppSettings.ConEmuConsoleFont.Size.Should().Be(savedFont.Size);
            AppSettings.ConEmuConsoleFont.Style.Should().Be(savedFont.Style);

            accessor.SetConsoleFont(null);
            accessor.ConsoleFont.Content.Should().Be("Console Default");
            accessor.ConsoleFontReset.IsVisible.Should().BeFalse();
            page.SaveSettings();
            AppSettings.ConEmuConsoleFont.Should().BeNull();
        }
        finally
        {
            AppSettings.ConsoleEmulatorName.Value = originalEmulator;
            AppSettings.ConEmuStyle.Value = originalStyle;
            AppSettings.ConEmuConsoleFont = originalFont;
        }
    }

    [AvaloniaTest]
    public void Console_style_settings_should_preserve_original_translation_keys()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        ConsoleStyleSettingsPage page = new();

        page.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(ConsoleStyleSettingsPage), "groupBoxConsoleSettings", "Text", "Console settings (restart required)");
        translation.Received(1).AddTranslationItem(
            nameof(ConsoleStyleSettingsPage), "lblConsoleEmulator", "Text", "Console emulator");
        translation.Received(1).AddTranslationItem(
            nameof(ConsoleStyleSettingsPage), "consoleFontChangeButton", "Text", "font name");
        translation.Received(1).AddTranslationItem(
            nameof(ConsoleStyleSettingsPage), "_defaultThemeDisplayName", "Text", "Default");
        translation.Received(1).AddTranslationItem(
            nameof(ConsoleStyleSettingsPage), "_consoleDefaultFontText", "Text", "Console Default");
        translation.Received(1).AddTranslationItem(
            nameof(ConsoleStyleSettingsPage), "consoleFontResetButton", "consoleFontToolTip", "Reset to default");
        translation.DidNotReceive().AddTranslationItem(
            nameof(ConsoleStyleSettingsPage), "consoleFontResetButton", "toolTip", Arg.Any<string>());
        Avalonia.Controls.ToolTip.GetTip(page.GetTestAccessor().ConsoleFontReset)
            .Should().Be("Reset to default");
    }

    [AvaloniaTest]
    public void FormSettings_should_register_revision_links_before_build_server_integration()
    {
        FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();
        List<ISettingsPage> pages = accessor.SettingsTreeView.SettingsPages.ToList();
        RevisionLinksSettingsPage revisionLinks = pages
            .OfType<RevisionLinksSettingsPage>()
            .Single();
        BuildServerIntegrationSettingsPage buildServer = pages
            .OfType<BuildServerIntegrationSettingsPage>()
            .Single();

        pages.IndexOf(revisionLinks).Should().BeLessThan(pages.IndexOf(buildServer));

        form.GotoPage(RevisionLinksSettingsPage.GetPageReference());

        SettingsPageHeader header = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        header.GetTestAccessor().Page.Should().BeSameAs(revisionLinks);
        revisionLinks.GetTitle().Should().Be("Revision links");
    }

    [AvaloniaTest]
    public void Revision_links_settings_should_roundtrip_all_fields_and_editable_formats()
    {
        DistributedSettings settings = CreateRevisionLinkSettings();
        ExternalLinkDefinition original = new()
        {
            Name = "Original",
            Enabled = true,
            SearchPattern = "issue-(\\d+)",
            NestedSearchPattern = "\\d+",
            RemoteSearchPattern = "github",
            UseRemotesPattern = "origin",
            UseOnlyFirstRemote = true,
            SearchInParts = { ExternalLinkDefinition.RevisionPart.Message },
            RemoteSearchInParts = { ExternalLinkDefinition.RemotePart.URL },
            LinkFormats =
            {
                new ExternalLinkFormat
                {
                    Caption = "Issue {0}",
                    Format = "https://example.test/issues/{0}",
                },
            },
        };
        new ExternalLinksStorage().Save(settings, [original]);
        RevisionLinksSettingsPage page = new();
        RevisionLinksSettingsPage.TestAccessor accessor = page.GetTestAccessor();
        accessor.LoadFromSettings(settings);

        Window window = new()
        {
            Content = page,
            Width = 900,
            Height = 620,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            TextBox[] formatEditors = accessor.Links
                .GetVisualDescendants()
                .OfType<TextBox>()
                .ToArray();
            formatEditors.Should().HaveCount(2);
            formatEditors[0].Text = "Ticket {0}";
            formatEditors[1].Text = "https://tracker.test/tickets/{0}";
            Dispatcher.UIThread.RunJobs();
            ExternalLinkDefinition selectedDefinition = accessor.SelectedDefinition
                ?? throw new InvalidOperationException("The revision-link definition was not selected.");
            selectedDefinition.LinkFormats.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new ExternalLinkFormat
                {
                    Caption = "Ticket {0}",
                    Format = "https://tracker.test/tickets/{0}",
                });

            accessor.Name.Text = "Updated";
            accessor.SearchPattern.Text = "ticket-(\\d+)";
            accessor.NestedPattern.Text = "(\\d+)";
            accessor.RemotePattern.Text = "tracker";
            accessor.UseRemotes.Text = "upstream|fork";
            accessor.Enabled.IsChecked = false;
            accessor.Message.IsChecked = false;
            accessor.LocalBranch.IsChecked = true;
            accessor.RemoteBranch.IsChecked = true;
            accessor.Url.IsChecked = false;
            accessor.PushUrl.IsChecked = true;
            accessor.OnlyFirstRemote.IsChecked = false;
            accessor.SaveToSettings();

            ExternalLinkDefinition saved = new ExternalLinksStorage()
                .Load(settings)
                .Should()
                .ContainSingle()
                .Subject;
            saved.Name.Should().Be("Updated");
            saved.Enabled.Should().BeFalse();
            saved.SearchPattern.Should().Be("ticket-(\\d+)");
            saved.NestedSearchPattern.Should().Be("(\\d+)");
            saved.RemoteSearchPattern.Should().Be("tracker");
            saved.UseRemotesPattern.Should().Be("upstream|fork");
            saved.UseOnlyFirstRemote.Should().BeFalse();
            saved.SearchInParts.Should().BeEquivalentTo(
                [
                    ExternalLinkDefinition.RevisionPart.LocalBranches,
                    ExternalLinkDefinition.RevisionPart.RemoteBranches,
                ]);
            saved.RemoteSearchInParts.Should().BeEquivalentTo(
                [ExternalLinkDefinition.RemotePart.PushURL]);
            saved.LinkFormats.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new ExternalLinkFormat
                {
                    Caption = "Ticket {0}",
                    Format = "https://tracker.test/tickets/{0}",
                });
            window.CaptureRenderedFrame().Should().NotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Revision_links_settings_should_add_and_remove_categories_and_formats()
    {
        RevisionLinksSettingsPage page = new();
        RevisionLinksSettingsPage.TestAccessor accessor = page.GetTestAccessor();
        accessor.LoadFromSettings(CreateRevisionLinkSettings());

        accessor.Categories.ItemCount.Should().Be(0);

        accessor.AddCategory();

        ExternalLinkDefinition definition = accessor.SelectedDefinition
            ?? throw new InvalidOperationException("The new revision-link definition was not selected.");
        definition.Name.Should().Be("<new>");
        definition.Enabled.Should().BeTrue();
        definition.UseRemotesPattern.Should().Be("upstream|origin");
        definition.UseOnlyFirstRemote.Should().BeTrue();
        definition.SearchInParts.Should().BeEquivalentTo(
            [ExternalLinkDefinition.RevisionPart.Message]);
        definition.RemoteSearchInParts.Should().BeEquivalentTo(
            [ExternalLinkDefinition.RemotePart.URL]);

        accessor.AddLink();
        accessor.SelectedFormat.Should().NotBeNull();
        definition.LinkFormats.Should().ContainSingle();

        accessor.RemoveLink();
        definition.LinkFormats.Should().BeEmpty();

        accessor.RemoveCategory();
        accessor.Categories.ItemCount.Should().Be(0);
        accessor.SelectedDefinition.Should().BeNull();
    }

    [AvaloniaTest]
    public async Task Revision_links_templates_should_prefer_upstream_and_run_as_owned_work()
    {
        DistributedSettings settings = CreateRevisionLinkSettings();
        RevisionLinksSettingsPage page = new();
        RevisionLinksSettingsPage.TestAccessor accessor = page.GetTestAccessor();
        accessor.LoadFromSettings(settings);
        IGitModule module = Substitute.For<IGitModule>();
        IReadOnlyList<Remote> remotes =
        [
            new Remote("origin", "https://github.com/origin/project.git", "https://github.com/origin/project.git"),
            new Remote("fork", "https://github.com/fork/project.git", "https://github.com/fork/project.git"),
            new Remote("upstream", "https://github.com/upstream/project.git", "https://github.com/upstream/project.git"),
        ];
        module.GetRemotesAsync().Returns(Task.FromResult(remotes));

        accessor.ExtractTemplate(new GitHubExternalLinkDefinitionExtractor(), module);
        await accessor.JoinTemplateOperationsAsync().WaitAsync(TimeSpan.FromSeconds(10));
        accessor.SaveToSettings();

        IReadOnlyList<ExternalLinkDefinition> definitions = new ExternalLinksManager(settings)
            .GetEffectiveSettings();
        definitions.Should().HaveCount(3);
        definitions.SelectMany(definition => definition.LinkFormats)
            .Select(format => format.Format ?? string.Empty)
            .Should().OnlyContain(format =>
                format.Contains("github.com/upstream/project", StringComparison.Ordinal));
        accessor.Categories.ItemCount.Should().Be(3);
    }

    [AvaloniaTest]
    public void Revision_links_settings_should_preserve_original_translation_identities()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        RevisionLinksSettingsPage page = new();

        page.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(RevisionLinksSettingsPage), "$this", "Text", "Revision links");
        translation.Received(1).AddTranslationItem(
            nameof(RevisionLinksSettingsPage), "CaptionCol", "HeaderText", "Caption");
        translation.Received(1).AddTranslationItem(
            nameof(RevisionLinksSettingsPage), "URICol", "HeaderText", "URI");
        translation.Received(1).AddTranslationItem(
            nameof(RevisionLinksSettingsPage), "_addTemplate", "Text", "Add {0} templates");
        translation.DidNotReceive().AddTranslationItem(
            nameof(RevisionLinksSettingsPage), "CaptionCol", "Text", Arg.Any<string>());
        translation.DidNotReceive().AddTranslationItem(
            nameof(RevisionLinksSettingsPage), "URICol", "Text", Arg.Any<string>());
    }

    [AvaloniaTest]
    public void FormSettings_should_register_and_navigate_to_the_scripts_page()
    {
        FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();
        ScriptsSettingsPage scripts = accessor.SettingsTreeView.SettingsPages
            .OfType<ScriptsSettingsPage>()
            .Single();

        TreeView tree = accessor.SettingsTreeView.FindControl<TreeView>("treeView1")
            ?? throw new InvalidOperationException("The settings tree was not created.");
        tree.Items
            .OfType<TreeViewItem>()
            .SelectMany(node => node.Items.OfType<TreeViewItem>())
            .Select(node => node.Tag)
            .Should().Contain(scripts);

        form.GotoPage(ScriptsSettingsPage.GetPageReference());

        SettingsPageHeader header = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        header.GetTestAccessor().Page.Should().BeSameAs(scripts);
        scripts.GetTitle().Should().Be("Scripts");
    }

    [AvaloniaTest]
    public void Scripts_settings_should_roundtrip_every_serialized_field()
    {
        ScriptInfo original = new()
        {
            Enabled = false,
            HotkeyCommandIdentifier = 9017,
            Name = "Original",
            Command = "git",
            Arguments = "status",
            AskConfirmation = false,
            IsPowerShell = false,
            OnEvent = ScriptEvent.BeforeCommit,
            AddToRevisionGridContextMenu = false,
            RunInBackground = false,
            Icon = "Pull",
            IconFilePath = "old-icon.png",
        };
        System.ComponentModel.BindingList<ScriptInfo> scripts = [original];
        IScriptsManager manager = Substitute.For<IScriptsManager>();
        manager.GetScripts().Returns(scripts);
        manager.SerializeIntoXml().Returns("<serialized-scripts />");
        IServiceProvider services = Substitute.For<IServiceProvider>();
        services.GetService(typeof(IScriptsManager)).Returns(manager);
        string? originalOwnScripts = AppSettings.OwnScripts;

        try
        {
            ScriptsSettingsPage page = new(services);
            page.LoadSettings();
            ScriptsSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            accessor.OnEvent.Items.Cast<ScriptEvent>().Should().Equal(Enum.GetValues<ScriptEvent>());

            accessor.Name.Text = "Updated";
            accessor.Enabled.IsChecked = true;
            accessor.Command.Text = "pwsh";
            accessor.Arguments.Text = "-File test.ps1";
            accessor.OnEvent.SelectedItem = ScriptEvent.AfterPush;
            accessor.SelectIcon("Push");
            accessor.IconFilePath.Text = "new-icon.png";
            accessor.AskConfirmation.IsChecked = true;
            accessor.RunInBackground.IsChecked = true;
            accessor.IsPowerShell.IsChecked = true;
            accessor.AddToRevisionGridContextMenu.IsChecked = true;

            page.SaveSettings();

            ScriptInfo saved = scripts.Should().ContainSingle().Subject;
            saved.Enabled.Should().BeTrue();
            saved.HotkeyCommandIdentifier.Should().Be(9017);
            saved.Name.Should().Be("Updated");
            saved.Command.Should().Be("pwsh");
            saved.Arguments.Should().Be("-File test.ps1");
            saved.AskConfirmation.Should().BeTrue();
            saved.IsPowerShell.Should().BeTrue();
            saved.OnEvent.Should().Be(ScriptEvent.AfterPush);
            saved.AddToRevisionGridContextMenu.Should().BeTrue();
            saved.RunInBackground.Should().BeTrue();
            saved.Icon.Should().Be("Push");
            saved.IconFilePath.Should().Be("new-icon.png");
            AppSettings.OwnScripts.Should().Be("<serialized-scripts />");
        }
        finally
        {
            AppSettings.OwnScripts = originalOwnScripts;
        }
    }

    [AvaloniaTest]
    public void Scripts_settings_should_preserve_order_and_allocate_stable_user_hotkey_ids()
    {
        System.ComponentModel.BindingList<ScriptInfo> scripts =
        [
            new ScriptInfo { Name = "First", HotkeyCommandIdentifier = 9001 },
            new ScriptInfo { Name = "Second", HotkeyCommandIdentifier = 9010 },
        ];
        IScriptsManager manager = Substitute.For<IScriptsManager>();
        manager.GetScripts().Returns(scripts);
        manager.SerializeIntoXml().Returns("<ordered-scripts />");
        IServiceProvider services = Substitute.For<IServiceProvider>();
        services.GetService(typeof(IScriptsManager)).Returns(manager);
        string? originalOwnScripts = AppSettings.OwnScripts;

        try
        {
            ScriptsSettingsPage page = new(services);
            page.LoadSettings();
            ScriptsSettingsPage.TestAccessor accessor = page.GetTestAccessor();

            accessor.Select(0);
            accessor.MoveDown.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            accessor.Add.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            IReadOnlyList<ScriptInfo> scriptSnapshot = accessor.GetScripts();
            scriptSnapshot.Select(script => script.Name).Should().Equal("Second", "First", "<New Script>");
            scriptSnapshot[^1].HotkeyCommandIdentifier.Should().Be(9011);
            accessor.Delete.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            page.SaveSettings();

            scripts.Select(script => script.Name).Should().Equal("Second", "First");
            scripts.Select(script => script.HotkeyCommandIdentifier).Should().Equal(9010, 9001);
        }
        finally
        {
            AppSettings.OwnScripts = originalOwnScripts;
        }
    }

    [AvaloniaTest]
    public void Scripts_settings_should_preserve_original_translation_keys_and_help_content()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        ScriptsSettingsPage page = new();

        page.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(ScriptsSettingsPage), "_scriptSettingsPageHelpDisplayArgumentsHelp", "Text", "Arguments help");
        translation.Received(1).AddTranslationItem(
            nameof(ScriptsSettingsPage), "_scriptSettingsPageHelpDisplayContent", "Text", Arg.Is<string>(text =>
                text.Contains("{SelectedRelativePaths}", StringComparison.Ordinal)
                && text.Contains("{ColumnNumber}", StringComparison.Ordinal)));
        page.GetTestAccessor().ArgumentsHelp.Content.Should().Be("?");
    }

    [AvaloniaTest]
    public void FormSettings_should_register_hotkeys_after_scripts()
    {
        FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();
        List<ISettingsPage> pages = accessor.SettingsTreeView.SettingsPages.ToList();
        ScriptsSettingsPage scripts = pages.OfType<ScriptsSettingsPage>().Single();
        HotkeysSettingsPage hotkeys = pages.OfType<HotkeysSettingsPage>().Single();

        pages.IndexOf(scripts).Should().BeLessThan(pages.IndexOf(hotkeys));

        form.GotoPage(HotkeysSettingsPage.GetPageReference());

        SettingsPageHeader header = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        header.GetTestAccessor().Page.Should().BeSameAs(hotkeys);
        hotkeys.GetTitle().Should().Be("Hotkeys");
    }

    [AvaloniaTest]
    public void Hotkeys_settings_should_capture_apply_clear_reset_and_save()
    {
        string? serializedHotkeys = AppSettings.SerializedHotkeys;
        AppSettings.SerializedHotkeys = string.Empty;
        try
        {
            IHotkeySettingsManager manager = new HotkeySettingsManager();
            IServiceProvider services = Substitute.For<IServiceProvider>();
            services.GetService(typeof(IHotkeySettingsManager)).Returns(manager);
            HotkeysSettingsPage page = new(services);
            HotkeysSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            Window window = new()
            {
                Content = page,
                Width = 791,
                Height = 525,
            };
            window.Show();
            try
            {
                page.LoadSettings();
                int browseIndex = accessor.Hotkeys.Values!
                    .Select((setting, index) => (setting, index))
                    .Single(item => item.setting.Name == FormBrowse.HotkeySettingsName)
                    .index;
                accessor.Hotkeys.Settings.SelectedIndex = browseIndex;
                int refreshIndex = accessor.Hotkeys.Mappings.Items
                    .Cast<HotkeyCommand>()
                    .Select((command, index) => (command, index))
                    .Single(item => item.command.CommandCode == (int)FormBrowse.Command.Refresh)
                    .index;
                accessor.Hotkeys.Mappings.SelectedIndex = refreshIndex;
                accessor.Hotkeys.Hotkey.Focus();

                window.KeyPress(
                    Key.F6,
                    RawInputModifiers.Control,
                    PhysicalKey.F6,
                    keySymbol: null);
                accessor.Hotkeys.Hotkey.KeyData.Should()
                    .Be(WinFormsShims.Keys.Control | WinFormsShims.Keys.F6);
                accessor.Hotkeys.Apply.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                accessor.Hotkeys.Values![browseIndex].Commands![refreshIndex].KeyData.Should()
                    .Be(WinFormsShims.Keys.Control | WinFormsShims.Keys.F6);
                Dispatcher.UIThread.RunJobs();
                accessor.Hotkeys.Mappings.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Should()
                    .Contain(textBlock => textBlock.Text == "Ctrl+F6");

                accessor.Hotkeys.Mappings.SelectedIndex = refreshIndex;
                accessor.Hotkeys.Clear.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                accessor.Hotkeys.Values[browseIndex].Commands![refreshIndex].KeyData.Should()
                    .Be(WinFormsShims.Keys.None);

                accessor.Hotkeys.Reset.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                HotkeyCommand resetRefresh = accessor.Hotkeys.Values!
                    .Single(setting => setting.Name == FormBrowse.HotkeySettingsName)
                    .Commands!
                    .Single(command => command.CommandCode == (int)FormBrowse.Command.Refresh);
                resetRefresh.KeyData.Should().Be(WinFormsShims.Keys.F5);

                page.SaveSettings();
                manager.LoadHotkeys(FormBrowse.HotkeySettingsName)
                    .Should()
                    .ContainSingle(command =>
                        command.CommandCode == (int)FormBrowse.Command.Refresh
                        && command.KeyData == WinFormsShims.Keys.F5);
                window.CaptureRenderedFrame().Should().NotBeNull();
            }
            finally
            {
                window.Close();
            }
        }
        finally
        {
            AppSettings.SerializedHotkeys = serializedHotkeys!;
        }
    }

    [AvaloniaTest]
    public void Hotkeys_settings_should_preserve_original_translation_keys()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        HotkeysSettingsPage page = new();
        ControlHotkeys control = page.GetLogicalDescendants()
            .OfType<ControlHotkeys>()
            .Single();

        page.AddTranslationItems(translation);
        control.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(HotkeysSettingsPage), "$this", "Text", "Hotkeys");
        translation.Received(1).AddTranslationItem(
            nameof(ControlHotkeys), "lHotkeyableItems", "Text", "Hotkeyable Items");
        translation.Received(1).AddTranslationItem(
            nameof(ControlHotkeys), "columnCommand", "Text", "Command");
        translation.Received(1).AddTranslationItem(
            nameof(ControlHotkeys), "columnKey", "Text", "Key");
        translation.Received(1).AddTranslationItem(
            nameof(ControlHotkeys), "txtHotkey", "Text", "None");
        translation.Received(1).AddTranslationItem(
            nameof(ControlHotkeys), "bResetToDefaults", "Text", "Reset all Hotkeys to defaults");
    }

    [AvaloniaTest]
    public void FormSettings_should_register_advanced_after_hotkeys_with_confirmations_beneath_it()
    {
        FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();
        List<ISettingsPage> pages = accessor.SettingsTreeView.SettingsPages.ToList();
        HotkeysSettingsPage hotkeys = pages.OfType<HotkeysSettingsPage>().Single();
        AdvancedSettingsPage advanced = pages.OfType<AdvancedSettingsPage>().Single();
        ConfirmationsSettingsPage confirmations = pages.OfType<ConfirmationsSettingsPage>().Single();

        pages.IndexOf(hotkeys).Should().BeLessThan(pages.IndexOf(advanced));

        TreeView tree = accessor.SettingsTreeView.FindControl<TreeView>("treeView1")
            ?? throw new InvalidOperationException("The settings tree was not created.");
        TreeViewItem advancedNode = tree.Items
            .OfType<TreeViewItem>()
            .SelectMany(node => node.Items.OfType<TreeViewItem>())
            .Single(node => node.Tag is AdvancedSettingsPage);
        advancedNode.Items
            .OfType<TreeViewItem>()
            .Select(node => node.Tag)
            .Should()
            .ContainSingle(item => ReferenceEquals(item, confirmations));

        form.GotoPage(ConfirmationsSettingsPage.GetPageReference());

        SettingsPageHeader header = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        header.GetTestAccessor().Page.Should().BeSameAs(confirmations);
        confirmations.GetTitle().Should().Be("Confirmations");
    }

    [AvaloniaTest]
    public void Advanced_settings_should_roundtrip_live_values_and_preserve_hidden_consumer_values()
    {
        bool originalAlwaysShowCheckout = AppSettings.AlwaysShowCheckoutBranchDlg;
        bool originalUseLocalChanges = AppSettings.UseDefaultCheckoutBranchAction;
        bool originalDontShowHelpImages = AppSettings.DontShowHelpImages;
        bool originalAlwaysShowAdvanced = AppSettings.AlwaysShowAdvOpt;
        bool originalCheckForUpdates = AppSettings.CheckForUpdates;
        bool originalCheckForReleaseCandidates = AppSettings.CheckForReleaseCandidates;
        bool originalConsoleEmulator = AppSettings.UseConsoleEmulatorForCommands.Value;
        bool originalAutoNormalise = AppSettings.AutoNormaliseBranchName;
        string originalAutoNormaliseSymbol = AppSettings.AutoNormaliseSymbol;
        bool originalForcedPush = AppSettings.CommitAndPushForcedWhenAmend;
        try
        {
            AppSettings.AlwaysShowCheckoutBranchDlg = true;
            AppSettings.UseDefaultCheckoutBranchAction = false;
            AppSettings.DontShowHelpImages = true;
            AppSettings.AlwaysShowAdvOpt = false;
            AppSettings.CheckForUpdates = true;
            AppSettings.CheckForReleaseCandidates = false;
            AppSettings.UseConsoleEmulatorForCommands.Value = true;
            AppSettings.AutoNormaliseBranchName = false;
            AppSettings.AutoNormaliseSymbol = "-";
            AppSettings.CommitAndPushForcedWhenAmend = false;

            AdvancedSettingsPage page = new();
            page.LoadSettings();
            AdvancedSettingsPage.TestAccessor accessor = page.GetTestAccessor();

            accessor.AlwaysShowCheckoutDialog.IsChecked.Should().BeTrue();
            accessor.UseLocalChangesAction.IsChecked.Should().BeFalse();
            accessor.DontShowHelpImages.IsChecked.Should().BeTrue();
            accessor.AlwaysShowAdvancedOptions.IsChecked.Should().BeFalse();
            accessor.AutoNormaliseBranchName.IsChecked.Should().BeFalse();
            accessor.AutoNormaliseSymbol.IsEnabled.Should().BeFalse();
            accessor.AutoNormaliseSymbol.SelectedItem!.ToString().Should().Be("-");
            accessor.UpdatesGroup.IsVisible.Should().BeTrue();
            accessor.ConsoleEmulatorRow.IsVisible.Should().BeFalse();
            accessor.ConsoleEmulator.IsVisible.Should().BeFalse();
            accessor.CheckForUpdates.IsVisible.Should().BeTrue();
            accessor.CheckForReleaseCandidates.IsVisible.Should().BeTrue();

            accessor.AlwaysShowCheckoutDialog.IsChecked = false;
            accessor.UseLocalChangesAction.IsChecked = true;
            accessor.DontShowHelpImages.IsChecked = false;
            accessor.AlwaysShowAdvancedOptions.IsChecked = true;
            accessor.AutoNormaliseBranchName.IsChecked = true;
            accessor.AutoNormaliseSymbol.IsEnabled.Should().BeTrue();
            accessor.AutoNormaliseSymbol.SelectedItem = accessor.AutoNormaliseSymbol.Items
                .Cast<object>()
                .Single(item => item.ToString() == "(none)");
            accessor.CommitAndPushForcedWhenAmend.IsChecked = true;
            page.SaveSettings();

            AppSettings.AlwaysShowCheckoutBranchDlg.Should().BeFalse();
            AppSettings.UseDefaultCheckoutBranchAction.Should().BeTrue();
            AppSettings.DontShowHelpImages.Should().BeFalse();
            AppSettings.AlwaysShowAdvOpt.Should().BeTrue();
            AppSettings.AutoNormaliseBranchName.Should().BeTrue();
            AppSettings.AutoNormaliseSymbol.Should().BeEmpty();
            AppSettings.CommitAndPushForcedWhenAmend.Should().BeTrue();
            AppSettings.CheckForUpdates.Should().BeTrue();
            AppSettings.CheckForReleaseCandidates.Should().BeFalse();
            AppSettings.UseConsoleEmulatorForCommands.Value.Should().BeTrue();
        }
        finally
        {
            AppSettings.AlwaysShowCheckoutBranchDlg = originalAlwaysShowCheckout;
            AppSettings.UseDefaultCheckoutBranchAction = originalUseLocalChanges;
            AppSettings.DontShowHelpImages = originalDontShowHelpImages;
            AppSettings.AlwaysShowAdvOpt = originalAlwaysShowAdvanced;
            AppSettings.CheckForUpdates = originalCheckForUpdates;
            AppSettings.CheckForReleaseCandidates = originalCheckForReleaseCandidates;
            AppSettings.UseConsoleEmulatorForCommands.Value = originalConsoleEmulator;
            AppSettings.AutoNormaliseBranchName = originalAutoNormalise;
            AppSettings.AutoNormaliseSymbol = originalAutoNormaliseSymbol;
            AppSettings.CommitAndPushForcedWhenAmend = originalForcedPush;
        }
    }

    [AvaloniaTest]
    public void Confirmations_settings_should_preserve_direct_inverted_and_nullable_mappings()
    {
        bool originalAmend = AppSettings.DontConfirmAmend.Value;
        bool originalUndo = AppSettings.DontConfirmUndoLastCommit.Value;
        bool originalCommitIfNoBranch = AppSettings.DontConfirmCommitIfNoBranch;
        bool originalRebase = AppSettings.DontConfirmRebase.Value;
        bool originalFetchAndPrune = AppSettings.DontConfirmFetchAndPruneAll.Value;
        bool originalPushNewBranch = AppSettings.DontConfirmPushNewBranch.Value;
        bool originalAddTrackingRef = AppSettings.DontConfirmAddTrackingRef;
        bool originalDeleteUnmerged = AppSettings.DontConfirmDeleteUnmergedBranch.Value;
        bool originalBranchCheckout = AppSettings.ConfirmBranchCheckout.Value;
        bool? originalAutoPopAfterPull = AppSettings.AutoPopStashAfterPull;
        bool? originalAutoPopAfterCheckout = AppSettings.AutoPopStashAfterCheckoutBranch;
        bool originalStashDrop = AppSettings.DontConfirmStashDrop;
        bool originalResolveConflicts = AppSettings.DontConfirmResolveConflicts.Value;
        bool originalCommitAfterConflicts = AppSettings.DontConfirmCommitAfterConflictsResolved.Value;
        bool originalSecondAbort = AppSettings.DontConfirmSecondAbortConfirmation.Value;
        bool? originalUpdateSubmodules = AppSettings.DontConfirmUpdateSubmodulesOnCheckout;
        bool originalSwitchWorktree = AppSettings.DontConfirmSwitchWorktree.Value;
        try
        {
            AppSettings.DontConfirmAmend.Value = false;
            AppSettings.DontConfirmUndoLastCommit.Value = true;
            AppSettings.DontConfirmCommitIfNoBranch = false;
            AppSettings.DontConfirmRebase.Value = true;
            AppSettings.DontConfirmFetchAndPruneAll.Value = false;
            AppSettings.DontConfirmPushNewBranch.Value = true;
            AppSettings.DontConfirmAddTrackingRef = false;
            AppSettings.DontConfirmDeleteUnmergedBranch.Value = true;
            AppSettings.ConfirmBranchCheckout.Value = true;
            AppSettings.AutoPopStashAfterPull = null;
            AppSettings.AutoPopStashAfterCheckoutBranch = true;
            AppSettings.DontConfirmStashDrop = false;
            AppSettings.DontConfirmResolveConflicts.Value = true;
            AppSettings.DontConfirmCommitAfterConflictsResolved.Value = false;
            AppSettings.DontConfirmSecondAbortConfirmation.Value = true;
            AppSettings.DontConfirmUpdateSubmodulesOnCheckout = null;
            AppSettings.DontConfirmSwitchWorktree.Value = false;

            ConfirmationsSettingsPage page = new();
            page.LoadSettings();
            ConfirmationsSettingsPage.TestAccessor accessor = page.GetTestAccessor();

            accessor.Amend.IsChecked.Should().BeTrue();
            accessor.UndoLastCommit.IsChecked.Should().BeFalse();
            accessor.CommitIfNoBranch.IsChecked.Should().BeTrue();
            accessor.RebaseOnTop.IsChecked.Should().BeFalse();
            accessor.FetchAndPruneAll.IsChecked.Should().BeTrue();
            accessor.PushNewBranch.IsChecked.Should().BeFalse();
            accessor.AddTrackingRef.IsChecked.Should().BeTrue();
            accessor.DeleteUnmergedBranch.IsChecked.Should().BeFalse();
            accessor.BranchCheckout.IsChecked.Should().BeTrue();
            accessor.AutoPopStashAfterPull.IsChecked.Should().BeNull();
            accessor.AutoPopStashAfterCheckout.IsChecked.Should().BeFalse();
            accessor.ConfirmStashDrop.IsChecked.Should().BeTrue();
            accessor.ResolveConflicts.IsChecked.Should().BeFalse();
            accessor.CommitAfterConflictsResolved.IsChecked.Should().BeTrue();
            accessor.SecondAbort.IsChecked.Should().BeFalse();
            accessor.UpdateModules.IsChecked.Should().BeNull();
            accessor.SwitchWorktree.IsChecked.Should().BeTrue();

            accessor.Amend.IsChecked = false;
            accessor.UndoLastCommit.IsChecked = null;
            accessor.CommitIfNoBranch.IsChecked = false;
            accessor.RebaseOnTop.IsChecked = true;
            accessor.FetchAndPruneAll.IsChecked = false;
            accessor.PushNewBranch.IsChecked = true;
            accessor.AddTrackingRef.IsChecked = false;
            accessor.DeleteUnmergedBranch.IsChecked = true;
            accessor.BranchCheckout.IsChecked = false;
            accessor.AutoPopStashAfterPull.IsChecked = true;
            accessor.AutoPopStashAfterCheckout.IsChecked = null;
            accessor.ConfirmStashDrop.IsChecked = false;
            accessor.ResolveConflicts.IsChecked = true;
            accessor.CommitAfterConflictsResolved.IsChecked = false;
            accessor.SecondAbort.IsChecked = true;
            accessor.UpdateModules.IsChecked = false;
            accessor.SwitchWorktree.IsChecked = false;
            page.SaveSettings();

            AppSettings.DontConfirmAmend.Value.Should().BeTrue();
            AppSettings.DontConfirmUndoLastCommit.Value.Should().BeTrue();
            AppSettings.DontConfirmCommitIfNoBranch.Should().BeTrue();
            AppSettings.DontConfirmRebase.Value.Should().BeFalse();
            AppSettings.DontConfirmFetchAndPruneAll.Value.Should().BeTrue();
            AppSettings.DontConfirmPushNewBranch.Value.Should().BeFalse();
            AppSettings.DontConfirmAddTrackingRef.Should().BeTrue();
            AppSettings.DontConfirmDeleteUnmergedBranch.Value.Should().BeFalse();
            AppSettings.ConfirmBranchCheckout.Value.Should().BeFalse();
            AppSettings.AutoPopStashAfterPull.Should().BeFalse();
            AppSettings.AutoPopStashAfterCheckoutBranch.Should().BeNull();
            AppSettings.DontConfirmStashDrop.Should().BeTrue();
            AppSettings.DontConfirmResolveConflicts.Value.Should().BeFalse();
            AppSettings.DontConfirmCommitAfterConflictsResolved.Value.Should().BeTrue();
            AppSettings.DontConfirmSecondAbortConfirmation.Value.Should().BeFalse();
            AppSettings.DontConfirmUpdateSubmodulesOnCheckout.Should().BeTrue();
            AppSettings.DontConfirmSwitchWorktree.Value.Should().BeTrue();
        }
        finally
        {
            AppSettings.DontConfirmAmend.Value = originalAmend;
            AppSettings.DontConfirmUndoLastCommit.Value = originalUndo;
            AppSettings.DontConfirmCommitIfNoBranch = originalCommitIfNoBranch;
            AppSettings.DontConfirmRebase.Value = originalRebase;
            AppSettings.DontConfirmFetchAndPruneAll.Value = originalFetchAndPrune;
            AppSettings.DontConfirmPushNewBranch.Value = originalPushNewBranch;
            AppSettings.DontConfirmAddTrackingRef = originalAddTrackingRef;
            AppSettings.DontConfirmDeleteUnmergedBranch.Value = originalDeleteUnmerged;
            AppSettings.ConfirmBranchCheckout.Value = originalBranchCheckout;
            AppSettings.AutoPopStashAfterPull = originalAutoPopAfterPull;
            AppSettings.AutoPopStashAfterCheckoutBranch = originalAutoPopAfterCheckout;
            AppSettings.DontConfirmStashDrop = originalStashDrop;
            AppSettings.DontConfirmResolveConflicts.Value = originalResolveConflicts;
            AppSettings.DontConfirmCommitAfterConflictsResolved.Value = originalCommitAfterConflicts;
            AppSettings.DontConfirmSecondAbortConfirmation.Value = originalSecondAbort;
            AppSettings.DontConfirmUpdateSubmodulesOnCheckout = originalUpdateSubmodules;
            AppSettings.DontConfirmSwitchWorktree.Value = originalSwitchWorktree;
        }
    }

    [AvaloniaTest]
    public void Advanced_and_confirmations_settings_should_preserve_original_translation_keys()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        AdvancedSettingsPage advanced = new();
        ConfirmationsSettingsPage confirmations = new();

        advanced.AddTranslationItems(translation);
        confirmations.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(AdvancedSettingsPage), "$this", "Text", "Advanced");
        translation.Received(1).AddTranslationItem(
            nameof(AdvancedSettingsPage), "CheckoutGB", "Text", "Checkout");
        translation.Received(1).AddTranslationItem(
            nameof(AdvancedSettingsPage), "chkConsoleEmulator", "ToolTipText", Arg.Is<string>(text =>
                text.Contains("command progress dialogs", StringComparison.Ordinal)));
        translation.Received(1).AddTranslationItem(
            nameof(AdvancedSettingsPage), "chkAutoNormaliseBranchName", "ToolTipText", Arg.Is<string>(text =>
                text.Contains("git branch naming rules", StringComparison.Ordinal)));
        translation.Received(1).AddTranslationItem(
            nameof(ConfirmationsSettingsPage), "$this", "Text", "Confirmations");
        translation.Received(1).AddTranslationItem(
            nameof(ConfirmationsSettingsPage), "gbConfirmations", "Text", "Confirm actions");
        translation.Received(1).AddTranslationItem(
            nameof(ConfirmationsSettingsPage), "lblGroupConflictResolution", "Text", "Rebase / conflict resolution:");
        translation.Received(1).AddTranslationItem(
            nameof(ConfirmationsSettingsPage), "chkAutoPopStashAfterPull", "Text",
            "Apply stashed changes after successful pull (else stash will be popped automatically)");
    }

    [AvaloniaTest]
    public void Simple_help_display_should_apply_its_title_and_content_when_opened()
    {
        SimpleHelpDisplayDialog dialog = new()
        {
            DialogTitle = "Arguments help",
            ContentText = "{WorkingDir}",
        };
        dialog.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            dialog.Title.Should().Be("Arguments help");
            dialog.ContentTextBox.Text.Should().Be("{WorkingDir}");
            dialog.ContentTextBox.CaretIndex.Should().Be(0);
            dialog.CaptureRenderedFrame().Should().NotBeNull();
        }
        finally
        {
            dialog.Close();
        }
    }

    private static IConsoleEmulator CreateConsoleEmulator(
        string name,
        string displayName,
        IReadOnlyCollection<string> themes)
    {
        IConsoleEmulator emulator = Substitute.For<IConsoleEmulator>();
        emulator.Name.Returns(name);
        emulator.DisplayName.Returns(displayName);
        emulator.AvailableThemes.Returns(themes);
        return emulator;
    }

    private static DistributedSettings CreateRevisionLinkSettings()
        => new(
            lowerPriority: null,
            new GitExtSettingsCache(
                Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.settings"),
                autoSave: false),
            SettingLevel.Global);

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

            tree.GotoPage(new TestPageReference(Guid.NewGuid()));
            selections.Should().ContainSingle("an unknown page reference must not fall back to the first root");

            tree.GotoPage(rootPage.PageReference);
            selections.Should().HaveCount(2);
            selections[1].SettingsPage.Should().BeSameAs(rootPage);

            tree.GotoPage(childPage.PageReference);

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

        public int LoadCount { get; private set; }

        public int SaveCount { get; private set; }

        public Exception? SaveException { get; set; }

        public string GetTitle() => title;

        public IEnumerable<string> GetSearchKeywords() => keywords;

        public void OnPageShown()
        {
        }

        public void LoadSettings()
        {
            LoadCount++;
        }

        public void SaveSettings()
        {
            SaveCount++;
            if (SaveException is not null)
            {
                throw SaveException;
            }
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
        public List<SettingsPageReference> GotoPages { get; } = [];

        public CheckSettingsLogic CheckSettingsLogic => throw new NotSupportedException();

        public void GotoPage(SettingsPageReference settingsPageReference)
        {
            GotoPages.Add(settingsPageReference);
        }

        public void LoadAll()
        {
        }

        public void SaveAll()
        {
        }
    }

    private sealed class StubMessageBoxHost : WinFormsShims.IMessageBoxHost
    {
        public List<(string Caption, string Text)> Messages { get; } = [];

        public WinFormsShims.DialogResult Show(
            WinFormsShims.IWin32Window? owner,
            string? text,
            string? caption,
            WinFormsShims.MessageBoxButtons buttons,
            WinFormsShims.MessageBoxIcon icon,
            WinFormsShims.MessageBoxDefaultButton defaultButton)
        {
            Messages.Add((caption ?? string.Empty, text ?? string.Empty));
            return WinFormsShims.DialogResult.OK;
        }
    }
}
