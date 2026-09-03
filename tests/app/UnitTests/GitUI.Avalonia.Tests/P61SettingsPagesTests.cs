using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.Shells;
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
    public void Browse_repository_settings_should_use_the_registered_shell_provider_contract()
    {
        string originalTerminal = AppSettings.ConEmuTerminal.Value;
        try
        {
            IShellDescriptor bash = CreateShellDescriptor("bash", hasExecutable: true);
            IShellDescriptor pwsh = CreateShellDescriptor("pwsh", hasExecutable: false);
            IShellProvider shellProvider = Substitute.For<IShellProvider>();
            shellProvider.GetShells().Returns([bash, pwsh]);
            shellProvider.GetShell(Arg.Any<string?>()).Returns(bash);
            IServiceProvider serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IShellProvider)).Returns(shellProvider);

            AppSettings.ConEmuTerminal.Value = "pwsh";
            FormBrowseRepoSettingsPage page = new(serviceProvider);
            page.LoadSettings();
            FormBrowseRepoSettingsPage.TestAccessor accessor = page.GetTestAccessor();

            accessor.Terminal.Items.Cast<IShellDescriptor>().Should().Equal(bash, pwsh);
            accessor.Terminal.SelectedItem.Should().BeSameAs(pwsh);
            accessor.Terminal.SelectedItem = bash;
            page.SaveSettings();
            AppSettings.ConEmuTerminal.Value.Should().Be("bash");
        }
        finally
        {
            AppSettings.ConEmuTerminal.Value = originalTerminal;
        }
    }

    [Test]
    public void Portable_shell_provider_should_preserve_the_original_descriptor_order_and_fallback()
    {
        ShellProvider provider = new();
        IReadOnlyList<IShellDescriptor> shells = provider.GetShells();

        shells.Select(shell => shell.Name).Should().Equal("bash", "cmd", "pwsh", "powershell");
        foreach (IShellDescriptor shell in shells)
        {
            shell.Icon.Should().NotBeNull();
        }

        provider.GetShell("missing").Should().BeSameAs(shells[0]);
        provider.GetShell(null).Should().BeSameAs(shells[0]);
        provider.GetShellCommandLine("cmd").Should().NotBeNullOrWhiteSpace();
        if (OperatingSystem.IsWindows())
        {
            shells[0].ExecutableName.Should().BeOneOf("git-bash.exe", "bash.exe", "sh.exe");
        }
        else
        {
            shells[0].ExecutableName.Should().BeOneOf("bash", "sh");
        }

        shells[2].ExecutableName.Should().Be(OperatingSystem.IsWindows() ? "pwsh.exe" : "pwsh");
    }

    [Test]
    public void Portable_shell_descriptors_should_emit_platform_native_change_directory_commands()
    {
        string path = new DirectoryInfo(Path.GetTempPath()).FullName;
        string bashCommand = new BashShell().GetChangeDirCommand(path);

        bashCommand.Should().StartWith("cd ");
        if (OperatingSystem.IsWindows())
        {
            bashCommand.Should().NotContain(@":\");
            new CmdShell().GetChangeDirCommand(path).Should().StartWith("cd /D ");
        }
        else
        {
            bashCommand.Should().Contain(path);
        }

        new PwshShell().GetChangeDirCommand(path).Should().StartWith("cd ");
        new PowerShellShell().GetChangeDirCommand(path).Should().StartWith("cd ");
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
    public void Translation_chooser_should_list_English_and_commit_only_an_activated_selection()
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
            Grid english = items[0].Content.Should().BeOfType<Grid>().Which;
            english.Width.Should().Be(190);
            english.Height.Should().Be(98);
            english.Children.OfType<Border>().Single().Child.Should().BeOfType<Image>()
                .Which.Source.Should().NotBeNull();

            accessor.Translations.SelectedItem = items[0];

            AppSettings.Translation.Should().BeEmpty("selecting a WinForms ListView item does not activate it");
            accessor.ActivateSelectedTranslation();

            AppSettings.Translation.Should().Be("English");
        }
        finally
        {
            AppSettings.Translation = originalTranslation;
        }
    }

    [AvaloniaTest]
    public void Translation_chooser_should_wrap_large_icon_items_in_the_original_three_column_shape()
    {
        using FormChooseTranslation form = new();
        FormChooseTranslation.TestAccessor accessor = form.GetTestAccessor();
        accessor.LoadTranslations();
        form.Show();
        Dispatcher.UIThread.RunJobs();
        List<ListBoxItem> items = accessor.Translations.Items.OfType<ListBoxItem>().Take(4).ToList();
        items.Should().HaveCount(4);

        items[0].Bounds.Size.Should().Be(new Avalonia.Size(190, 119));
        items[1].Bounds.Y.Should().Be(items[0].Bounds.Y);
        items[2].Bounds.Y.Should().Be(items[0].Bounds.Y);
        items[3].Bounds.Y.Should().BeGreaterThan(items[0].Bounds.Y);
    }

    [AvaloniaTest]
    public void Settings_pages_should_preserve_native_96_dpi_designer_geometry()
    {
        AssertNativeLayout(
            new BlameViewerSettingsPage(),
            341,
            272,
            ("groupBoxBlameSettings", new Avalonia.Rect(11, 11, 319, 97)),
            ("groupBoxDisplayResult", new Avalonia.Rect(11, 114, 319, 197)));
        AssertNativeLayout(
            new CommitDialogSettingsPage(),
            1014,
            950,
            ("groupBoxBehaviour", new Avalonia.Rect(0, 0, 1014, 294)),
            ("tableLayoutPanelBehaviour", new Avalonia.Rect(3, 19, 1008, 272)),
            ("grpAdditionalButtons", new Avalonia.Rect(6, 191, 1002, 97)));
        AssertNativeLayout(
            new FormBrowseRepoSettingsPage(),
            738,
            438,
            ("tlpnlMain", new Avalonia.Rect(8, 8, 722, 422)),
            ("groupBox1", new Avalonia.Rect(11, 11, 716, 159)),
            ("gbTabs", new Avalonia.Rect(11, 176, 716, 136)));
        AssertNativeLayout(
            new ShellExtensionSettingsPage(),
            1502,
            331,
            ("tlpnlMain", new Avalonia.Rect(8, 8, 1486, 315)),
            ("gbExplorerIntegration", new Avalonia.Rect(11, 11, 1480, 63)),
            ("gbCascadingMenu", new Avalonia.Rect(11, 80, 1480, 502)));
        AssertNativeLayout(
            new FormChooseTranslation(),
            816,
            578,
            ("label1", new Avalonia.Rect(12, 9, 126, 15)),
            ("label2", new Avalonia.Rect(12, 33, 338, 15)),
            ("lvTranslations", new Avalonia.Rect(12, 51, 776, 476)));
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
        TestSettingControlBinding binding = new(setting);
        page.AddSettingControl(binding);

        ScrollViewer scrollViewer = page.Content.Should().BeOfType<ScrollViewer>().Subject;
        scrollViewer.HorizontalScrollBarVisibility.Should().Be(ScrollBarVisibility.Auto);
        scrollViewer.VerticalScrollBarVisibility.Should().Be(ScrollBarVisibility.Auto);
        Grid grid = scrollViewer.Content.Should().BeOfType<Grid>().Subject;
        grid.ColumnDefinitions.Should().HaveCount(3);
        grid.ColumnSpacing.Should().Be(0);
        grid.RowSpacing.Should().Be(0);
        grid.Children.OfType<TextBlock>().Should().ContainSingle().Which.Text.Should().Be("Enabled");
        CheckBox checkBox = grid.Children.OfType<CheckBox>().Single();
        checkBox.Margin.Should().Be(new Avalonia.Thickness(3));

        page.LoadSettings();
        checkBox.IsThreeState.Should().BeTrue();
        checkBox.IsChecked.Should().BeNull("the global source has no explicit value for the test setting");
        binding.LoadCount.Should().Be(1, "AutoLayout must use the supplied binding instance");
        ((ISettingsLayout)page).Invoking(layout => layout.GetControl()).Should().Throw<NotImplementedException>();
    }

    private static void AssertNativeLayout(
        Control view,
        double width,
        double height,
        params (string Name, Avalonia.Rect Bounds)[] expectedControls)
    {
        Window window = view as Window ?? new Window { Content = view };
        window.Width = width;
        window.Height = height;
        window.SizeToContent = SizeToContent.Manual;
        try
        {
            window.Show();
            window.Width = width;
            window.Height = height;
            Dispatcher.UIThread.RunJobs();

            foreach ((string name, Avalonia.Rect expectedBounds) in expectedControls)
            {
                Control control = view.FindControl<Control>(name)
                    ?? throw new InvalidOperationException($"The native-layout control '{name}' was not created.");
                Avalonia.Point origin = Avalonia.VisualExtensions.TranslatePoint(control, default, view)
                    ?? throw new InvalidOperationException($"The native-layout control '{name}' is detached from its page.");
                new Avalonia.Rect(origin, control.Bounds.Size).Should().Be(
                    expectedBounds,
                    $"{name} must retain the WinForms 96-DPI Designer bounds");
            }
        }
        finally
        {
            window.Close();
            if (!ReferenceEquals(window, view))
            {
                (view as IDisposable)?.Dispose();
            }
        }
    }

    private sealed class TestAutoLayoutPage : AutoLayoutSettingsPage
    {
        internal TestAutoLayoutPage()
            : base(EmptyServiceProvider.Instance)
        {
        }
    }

    private static IShellDescriptor CreateShellDescriptor(string name, bool hasExecutable)
        => new TestShellDescriptor(name, hasExecutable);

    private sealed class TestShellDescriptor(string name, bool hasExecutable) : IShellDescriptor
    {
        public string? ExecutableCommandLine => null;
        public string ExecutableName => name;
        public string? ExecutablePath => hasExecutable ? name : null;
        public bool HasExecutable => hasExecutable;
        public Avalonia.Media.IImage Icon => null!;
        public string Name => name;
        public string GetChangeDirCommand(string path) => string.Empty;
        public override string ToString() => Name;
    }

    private sealed class TestSettingControlBinding(BoolSetting setting) : ISettingControlBinding
    {
        private readonly GitExtensions.Shims.WinForms.CheckBox _control = new();

        internal int LoadCount { get; private set; }

        public GitExtensions.Shims.WinForms.Control GetControl() => _control;

        public void LoadSetting(SettingsSource settings)
        {
            LoadCount++;
            _control.CheckState = GitExtensions.Shims.WinForms.CheckState.Indeterminate;
        }

        public void SaveSetting(SettingsSource settings)
        {
        }

        public string Caption() => setting.Caption;

        public ISetting GetSetting() => setting;
    }
}
