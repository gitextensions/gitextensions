using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class P61SettingsPagesTests
{
    [AvaloniaTest]
    public void FormSettings_should_register_missing_pages_in_the_original_hierarchy_and_order()
    {
        GitUI.ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();

        List<ISettingsPage> pages = accessor.SettingsTreeView.SettingsPages.ToList();
        DetailedSettingsPage detailed = pages.OfType<DetailedSettingsPage>().Single();
        FormBrowseRepoSettingsPage browse = pages.OfType<FormBrowseRepoSettingsPage>().Single();
        CommitDialogSettingsPage commit = pages.OfType<CommitDialogSettingsPage>().Single();
        DiffViewerSettingsPage diff = pages.OfType<DiffViewerSettingsPage>().Single();
        BlameViewerSettingsPage blame = pages.OfType<BlameViewerSettingsPage>().Single();

        TreeView tree = accessor.SettingsTreeView.FindControl<TreeView>("treeView1")
            ?? throw new InvalidOperationException("The settings tree was not created.");
        TreeViewItem detailedNode = tree.Items
            .OfType<TreeViewItem>()
            .SelectMany(node => node.Items.OfType<TreeViewItem>())
            .Single(node => node.Tag is DetailedSettingsPage);
        detailedNode.Tag.Should().BeSameAs(detailed);
        detailedNode.Items.OfType<TreeViewItem>().Select(node => node.Tag).Should().ContainInOrder(
            browse,
            commit,
            diff,
            blame);

        pages.OfType<ShellExtensionSettingsPage>().Count().Should().Be(OperatingSystem.IsWindows() ? 1 : 0);

        form.GotoPage(FormBrowseRepoSettingsPage.GetPageReference());
        SettingsPageHeader header = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        header.GetTestAccessor().Page.Should().BeSameAs(browse);
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void Blame_viewer_settings_should_roundtrip_all_values()
    {
        bool[] original =
        [
            AppSettings.IgnoreWhitespaceOnBlame,
            AppSettings.DetectCopyInFileOnBlame,
            AppSettings.DetectCopyInAllOnBlame,
            AppSettings.BlameDisplayAuthorFirst,
            AppSettings.BlameShowAuthor,
            AppSettings.BlameShowAuthorDate,
            AppSettings.BlameShowAuthorTime,
            AppSettings.BlameShowLineNumbers,
            AppSettings.BlameShowOriginalFilePath,
            AppSettings.BlameShowAuthorAvatar,
        ];
        try
        {
            AppSettings.IgnoreWhitespaceOnBlame = true;
            AppSettings.DetectCopyInFileOnBlame = false;
            AppSettings.DetectCopyInAllOnBlame = true;
            AppSettings.BlameDisplayAuthorFirst = false;
            AppSettings.BlameShowAuthor = true;
            AppSettings.BlameShowAuthorDate = false;
            AppSettings.BlameShowAuthorTime = true;
            AppSettings.BlameShowLineNumbers = false;
            AppSettings.BlameShowOriginalFilePath = true;
            AppSettings.BlameShowAuthorAvatar = false;

            BlameViewerSettingsPage page = new();
            page.LoadSettings();
            BlameViewerSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            accessor.IgnoreWhitespace.IsChecked.Should().BeTrue();
            accessor.DetectCopyInFile.Checked.Should().BeFalse();
            accessor.DetectCopyInAll.Checked.Should().BeTrue();
            accessor.DisplayAuthorFirst.IsChecked.Should().BeFalse();
            accessor.ShowAuthor.IsChecked.Should().BeTrue();
            accessor.ShowAuthorDate.IsChecked.Should().BeFalse();
            accessor.ShowAuthorTime.IsChecked.Should().BeTrue();
            accessor.ShowLineNumbers.IsChecked.Should().BeFalse();
            accessor.ShowOriginalFilePath.IsChecked.Should().BeTrue();
            accessor.ShowAuthorAvatar.IsChecked.Should().BeFalse();

            accessor.IgnoreWhitespace.IsChecked = false;
            accessor.DetectCopyInFile.Checked = true;
            accessor.DetectCopyInAll.Checked = false;
            accessor.DisplayAuthorFirst.IsChecked = true;
            accessor.ShowAuthor.IsChecked = false;
            accessor.ShowAuthorDate.IsChecked = true;
            accessor.ShowAuthorTime.IsChecked = false;
            accessor.ShowLineNumbers.IsChecked = true;
            accessor.ShowOriginalFilePath.IsChecked = false;
            accessor.ShowAuthorAvatar.IsChecked = true;
            page.SaveSettings();

            AppSettings.IgnoreWhitespaceOnBlame.Should().BeFalse();
            AppSettings.DetectCopyInFileOnBlame.Should().BeTrue();
            AppSettings.DetectCopyInAllOnBlame.Should().BeFalse();
            AppSettings.BlameDisplayAuthorFirst.Should().BeTrue();
            AppSettings.BlameShowAuthor.Should().BeFalse();
            AppSettings.BlameShowAuthorDate.Should().BeTrue();
            AppSettings.BlameShowAuthorTime.Should().BeFalse();
            AppSettings.BlameShowLineNumbers.Should().BeTrue();
            AppSettings.BlameShowOriginalFilePath.Should().BeFalse();
            AppSettings.BlameShowAuthorAvatar.Should().BeTrue();
        }
        finally
        {
            AppSettings.IgnoreWhitespaceOnBlame = original[0];
            AppSettings.DetectCopyInFileOnBlame = original[1];
            AppSettings.DetectCopyInAllOnBlame = original[2];
            AppSettings.BlameDisplayAuthorFirst = original[3];
            AppSettings.BlameShowAuthor = original[4];
            AppSettings.BlameShowAuthorDate = original[5];
            AppSettings.BlameShowAuthorTime = original[6];
            AppSettings.BlameShowLineNumbers = original[7];
            AppSettings.BlameShowOriginalFilePath = original[8];
            AppSettings.BlameShowAuthorAvatar = original[9];
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void Commit_dialog_settings_should_roundtrip_all_values()
    {
        bool[] original =
        [
            AppSettings.ShowErrorsWhenStagingFiles,
            AppSettings.EnsureCommitMessageSecondLineEmpty,
            AppSettings.UseFormCommitMessage,
            AppSettings.ShowCommitAndPush,
            AppSettings.ShowResetWorkTreeChanges,
            AppSettings.ShowResetAllChanges,
            AppSettings.ProvideAutocompletion,
            AppSettings.RememberAmendCommitState,
        ];
        int originalPreviousMessages = AppSettings.CommitDialogNumberOfPreviousMessages;
        try
        {
            AppSettings.ShowErrorsWhenStagingFiles = true;
            AppSettings.EnsureCommitMessageSecondLineEmpty = false;
            AppSettings.UseFormCommitMessage = true;
            AppSettings.CommitDialogNumberOfPreviousMessages = 17;
            AppSettings.ShowCommitAndPush = false;
            AppSettings.ShowResetWorkTreeChanges = true;
            AppSettings.ShowResetAllChanges = false;
            AppSettings.ProvideAutocompletion = true;
            AppSettings.RememberAmendCommitState = false;

            CommitDialogSettingsPage page = new();
            page.LoadSettings();
            CommitDialogSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            accessor.ShowErrorsWhenStagingFiles.IsChecked.Should().BeTrue();
            accessor.EnsureSecondLineEmpty.IsChecked.Should().BeFalse();
            accessor.WriteMessageInCommitWindow.IsChecked.Should().BeTrue();
            accessor.PreviousMessages.Value.Should().Be(17);
            accessor.ShowCommitAndPush.IsChecked.Should().BeFalse();
            accessor.ShowResetWorkTreeChanges.IsChecked.Should().BeTrue();
            accessor.ShowResetAllChanges.IsChecked.Should().BeFalse();
            accessor.Autocomplete.IsChecked.Should().BeTrue();
            accessor.RememberAmendState.IsChecked.Should().BeFalse();

            accessor.ShowErrorsWhenStagingFiles.IsChecked = false;
            accessor.EnsureSecondLineEmpty.IsChecked = true;
            accessor.WriteMessageInCommitWindow.IsChecked = false;
            accessor.PreviousMessages.Value = 23;
            accessor.ShowCommitAndPush.IsChecked = true;
            accessor.ShowResetWorkTreeChanges.IsChecked = false;
            accessor.ShowResetAllChanges.IsChecked = true;
            accessor.Autocomplete.IsChecked = false;
            accessor.RememberAmendState.IsChecked = true;
            page.SaveSettings();

            AppSettings.ShowErrorsWhenStagingFiles.Should().BeFalse();
            AppSettings.EnsureCommitMessageSecondLineEmpty.Should().BeTrue();
            AppSettings.UseFormCommitMessage.Should().BeFalse();
            AppSettings.CommitDialogNumberOfPreviousMessages.Should().Be(23);
            AppSettings.ShowCommitAndPush.Should().BeTrue();
            AppSettings.ShowResetWorkTreeChanges.Should().BeFalse();
            AppSettings.ShowResetAllChanges.Should().BeTrue();
            AppSettings.ProvideAutocompletion.Should().BeFalse();
            AppSettings.RememberAmendCommitState.Should().BeTrue();
        }
        finally
        {
            AppSettings.ShowErrorsWhenStagingFiles = original[0];
            AppSettings.EnsureCommitMessageSecondLineEmpty = original[1];
            AppSettings.UseFormCommitMessage = original[2];
            AppSettings.ShowCommitAndPush = original[3];
            AppSettings.ShowResetWorkTreeChanges = original[4];
            AppSettings.ShowResetAllChanges = original[5];
            AppSettings.ProvideAutocompletion = original[6];
            AppSettings.RememberAmendCommitState = original[7];
            AppSettings.CommitDialogNumberOfPreviousMessages = originalPreviousMessages;
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void Browse_repository_settings_should_roundtrip_all_values_and_panel_visibility()
    {
        bool[] original =
        [
            AppSettings.ShowConEmuTab.Value,
            AppSettings.UseBrowseForFileHistory.Value,
            AppSettings.UseDiffViewerForBlame.Value,
            AppSettings.ShowGpgInformation.Value,
            AppSettings.ShowFindInCommitFilesGitGrep.Value,
            AppSettings.ShowRevisionGridTooltips.Value,
            AppSettings.ShowOutputHistoryAsTab.Value,
            AppSettings.OutputHistoryPanelVisible.Value,
        ];
        int originalDepth = AppSettings.OutputHistoryDepth.Value;
        string originalTerminal = AppSettings.ConEmuTerminal.Value;
        try
        {
            AppSettings.ShowConEmuTab.Value = true;
            AppSettings.UseBrowseForFileHistory.Value = false;
            AppSettings.UseDiffViewerForBlame.Value = true;
            AppSettings.ShowGpgInformation.Value = false;
            AppSettings.ShowFindInCommitFilesGitGrep.Value = true;
            AppSettings.ShowRevisionGridTooltips.Value = false;
            AppSettings.ShowOutputHistoryAsTab.Value = true;
            AppSettings.OutputHistoryDepth.Value = 77;
            AppSettings.ConEmuTerminal.Value = "bash";

            FormBrowseRepoSettingsPage page = new();
            page.LoadSettings();
            FormBrowseRepoSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            accessor.ShowConsoleTab.Checked.Should().BeTrue();
            accessor.UseBrowseForFileHistory.Checked.Should().BeFalse();
            accessor.UseDiffViewerForBlame.Checked.Should().BeTrue();
            accessor.ShowGpgInformation.Checked.Should().BeFalse();
            accessor.ShowGitGrep.Checked.Should().BeTrue();
            accessor.ShowRevisionGridTooltip.Checked.Should().BeFalse();
            accessor.ShowOutputHistoryAsTab.Checked.Should().BeTrue();
            accessor.OutputHistoryDepth.Value.Should().Be(77);

            accessor.ShowConsoleTab.Checked = false;
            accessor.UseBrowseForFileHistory.Checked = true;
            accessor.UseDiffViewerForBlame.Checked = false;
            accessor.ShowGpgInformation.Checked = true;
            accessor.ShowGitGrep.Checked = false;
            accessor.ShowRevisionGridTooltip.Checked = true;
            accessor.ShowOutputHistoryAsTab.Checked = false;
            accessor.OutputHistoryDepth.Value = 42;
            page.SaveSettings();

            AppSettings.ShowConEmuTab.Value.Should().BeFalse();
            AppSettings.UseBrowseForFileHistory.Value.Should().BeTrue();
            AppSettings.UseDiffViewerForBlame.Value.Should().BeFalse();
            AppSettings.ShowGpgInformation.Value.Should().BeTrue();
            AppSettings.ShowFindInCommitFilesGitGrep.Value.Should().BeFalse();
            AppSettings.ShowRevisionGridTooltips.Value.Should().BeTrue();
            AppSettings.ShowOutputHistoryAsTab.Value.Should().BeFalse();
            AppSettings.OutputHistoryDepth.Value.Should().Be(42);
            AppSettings.OutputHistoryPanelVisible.Value.Should().BeTrue();
            AppSettings.ConEmuTerminal.Value.Should().Be("bash");
        }
        finally
        {
            AppSettings.ShowConEmuTab.Value = original[0];
            AppSettings.UseBrowseForFileHistory.Value = original[1];
            AppSettings.UseDiffViewerForBlame.Value = original[2];
            AppSettings.ShowGpgInformation.Value = original[3];
            AppSettings.ShowFindInCommitFilesGitGrep.Value = original[4];
            AppSettings.ShowRevisionGridTooltips.Value = original[5];
            AppSettings.ShowOutputHistoryAsTab.Value = original[6];
            AppSettings.OutputHistoryPanelVisible.Value = original[7];
            AppSettings.OutputHistoryDepth.Value = originalDepth;
            AppSettings.ConEmuTerminal.Value = originalTerminal;
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void Shell_extension_settings_should_roundtrip_three_states_and_preview()
    {
        string originalItems = AppSettings.CascadeShellMenuItems;
        bool originalAlwaysShow = AppSettings.AlwaysShowAllCommands;
        try
        {
            AppSettings.CascadeShellMenuItems = "012012012012012012";
            AppSettings.AlwaysShowAllCommands = true;

            ShellExtensionSettingsPage page = new();
            page.LoadSettings();
            ShellExtensionSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            accessor.MenuEntries.Should().HaveCount(18);
            accessor.MenuEntries[0].IsChecked.Should().BeTrue();
            accessor.MenuEntries[1].IsChecked.Should().BeNull();
            accessor.MenuEntries[2].IsChecked.Should().BeFalse();
            accessor.AlwaysShowAllCommands.IsChecked.Should().BeTrue();
            accessor.Preview.Text.Should().Contain("GitExt Add files...");
            accessor.Preview.Text.Should().Contain("Git Extensions >");

            foreach (CheckBox checkBox in accessor.MenuEntries)
            {
                checkBox.IsChecked = false;
            }

            accessor.MenuEntries[0].IsChecked = null;
            accessor.MenuEntries[1].IsChecked = true;
            accessor.AlwaysShowAllCommands.IsChecked = false;
            page.SaveSettings();

            AppSettings.CascadeShellMenuItems.Should().Be("102222222222222222");
            AppSettings.AlwaysShowAllCommands.Should().BeFalse();
        }
        finally
        {
            AppSettings.CascadeShellMenuItems = originalItems;
            AppSettings.AlwaysShowAllCommands = originalAlwaysShow;
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void Translation_chooser_should_list_English_and_commit_a_one_click_selection()
    {
        string originalTranslation = AppSettings.Translation;
        try
        {
            AppSettings.Translation = string.Empty;
            using FormChooseTranslation form = new();
            FormChooseTranslation.TestAccessor accessor = form.GetTestAccessor();
            accessor.LoadTranslations();
            List<ListBoxItem> items = accessor.Translations.Items.OfType<ListBoxItem>().ToList();
            items.Should().NotBeEmpty();
            items[0].Tag.Should().Be("English");
            StackPanel english = items[0].Content.Should().BeOfType<StackPanel>().Which;
            english.Children.OfType<Image>().Single().Source.Should().NotBeNull();

            accessor.Translations.SelectedItem = items[0];

            AppSettings.Translation.Should().Be("English");
        }
        finally
        {
            AppSettings.Translation = originalTranslation;
        }
    }

    [AvaloniaTest]
    public void Missing_pages_should_preserve_original_translation_keys()
    {
        ITranslation translation = Substitute.For<ITranslation>();

        BlameViewerSettingsPage blame = new();
        blame.AddTranslationItems(translation);
        CommitDialogSettingsPage commit = new();
        commit.AddTranslationItems(translation);
        FormBrowseRepoSettingsPage browse = new();
        browse.AddTranslationItems(translation);
        ShellExtensionSettingsPage shell = new();
        shell.AddTranslationItems(translation);
        using FormChooseTranslation chooser = new();
        chooser.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(BlameViewerSettingsPage), "groupBoxDisplayResult", "Text", "Display result settings");
        translation.Received(1).AddTranslationItem(
            nameof(CommitDialogSettingsPage), "groupBoxBehaviour", "Text", "Behaviour");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowseRepoSettingsPage), "gbTabs", "Text", "Tabs (restart required)");
        translation.Received(1).AddTranslationItem(
            nameof(ShellExtensionSettingsPage), "RegisterButton", "Text", "&Enable shell extension");
        translation.Received(1).AddTranslationItem(
            nameof(FormChooseTranslation), "label1", "Text", "Choose your language");
    }

    [AvaloniaTest]
    public void Auto_layout_should_materialize_and_load_the_shared_setting_as_native_Avalonia_controls()
    {
        BoolSetting setting = new("P61AutoLayout", "Enabled", defaultValue: false);
        TestAutoLayoutPage page = new();
        page.AddSettingControl(new TestSettingControlBinding(setting));

        Grid grid = page.Content.Should().BeOfType<Grid>().Subject;
        grid.ColumnDefinitions.Should().HaveCount(3);
        grid.Children.OfType<TextBlock>().Should().ContainSingle().Which.Text.Should().Be("Enabled");
        CheckBox checkBox = grid.Children.OfType<CheckBox>().Single();

        page.LoadSettings();
        checkBox.IsThreeState.Should().BeTrue();
        checkBox.IsChecked.Should().BeNull("the global source has no explicit value for the test setting");
    }

    private sealed class TestAutoLayoutPage : AutoLayoutSettingsPage
    {
        internal TestAutoLayoutPage()
            : base(EmptyServiceProvider.Instance)
        {
        }
    }

    private sealed class TestSettingControlBinding(BoolSetting setting) : ISettingControlBinding
    {
        public GitExtensions.Shims.WinForms.Control GetControl() => new();

        public void LoadSetting(SettingsSource settings)
        {
        }

        public void SaveSetting(SettingsSource settings)
        {
        }

        public string Caption() => setting.Caption;

        public ISetting GetSetting() => setting;
    }
}
