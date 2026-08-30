using System.ComponentModel.Design;
using System.Diagnostics;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Editor;
using GitUI.ScriptsEngine;
using GitUI.SpellChecker;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class FormCommitTests
{
    private const string FeatCommitTypeForTest = "feat";

    private string _originalApplicationExecutablePath = null!;
    private string _originalWorkingDirectory = null!;
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        AppSettings.TestAccessor settingsAccessor = AppSettings.GetTestAccessor();
        _originalApplicationExecutablePath = settingsAccessor.ApplicationExecutablePath;
        settingsAccessor.ApplicationExecutablePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "GitExtensions.Avalonia.exe");

        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        _serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        WinFormsShims.ShimHost.MessageBoxHost = new StubMessageBoxHost { Result = WinFormsShims.DialogResult.Yes };

        _originalWorkingDirectory = Directory.GetCurrentDirectory();
        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.Tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.GetTestAccessor().ApplicationExecutablePath = _originalApplicationExecutablePath;
        _serviceContainer.Dispose();
        Directory.SetCurrentDirectory(_originalWorkingDirectory);
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void FormCommit_should_construct_and_use_existing_translation_keys()
    {
        FormCommit form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormCommit), "$this", "Text", "Commit");
        translation.Received(1).AddTranslationItem(nameof(FormCommit), "toolStageItem", "Text", "&Stage");
        translation.Received(1).AddTranslationItem(nameof(FormCommit), "toolUnstageItem", "Text", "&Unstage");
        translation.Received(1).AddTranslationItem(nameof(FormCommit), "commitStagedCountLabel", "Text", "Staged");
        translation.Received(1).AddTranslationItem(nameof(FormCommit), "Commit", "Text", "&Commit");
        translation.Received(1).AddTranslationItem(nameof(FormCommit), "_commitAndPush", "Text", "Commit && &push");
        translation.Received(1).AddTranslationItem(nameof(FormCommit), "_enterCommitMessage", "Text", "Please enter commit message");
        translation.Received(1).AddTranslationItem(nameof(FormCommit), "_enterCommitMessageCaption", "Text", "Commit message");
        translation.Received(1).AddTranslationItem(
            nameof(FormCommit),
            "_mergeConflicts",
            "Text",
            "There are unresolved merge conflicts, solve merge conflicts before committing.");
        translation.Received(1).AddTranslationItem(nameof(FormCommit), "_mergeConflictsCaption", "Text", "Merge conflicts");
        translation.Received(1).AddTranslationItem(nameof(FormCommit), "_formTitle", "Text", "Commit to {0} ({1})");
        translation.Received(1).AddTranslationItem(nameof(FormCommit), "_stageAll", "Text", "Stage all");
        translation.Received(1).AddTranslationItem(nameof(FormCommit), "_unstageAll", "Text", "Unstage all");
        translation.Received(1).AddTranslationItem(nameof(FormCommit), "_wordWrapCommitMessageBody", "Text", "&Word wrap (except subject line)");

        Button stageAll = form.FindControl<Button>("toolStageAllItem")
            ?? throw new InvalidOperationException("Stage-all button was not created.");
        Button unstageAll = form.FindControl<Button>("toolUnstageAllItem")
            ?? throw new InvalidOperationException("Unstage-all button was not created.");
        ToolTip.GetTip(stageAll).Should().Be("Stage all");
        ToolTip.GetTip(unstageAll).Should().Be("Unstage all");
        Button commitAndPush = form.FindControl<Button>("CommitAndPush")
            ?? throw new InvalidOperationException("Commit-and-push button was not created.");
        commitAndPush.Content.Should().Be("Commit & _push");
        commitAndPush.IsEnabled.Should().BeTrue(
            "the original keeps commit actions enabled and validates their inputs only after invocation");

        form.FindControl<DropDownButton>("commitMessageToolStripMenuItem").Should().NotBeNull();
        form.FindControl<DropDownButton>("commitTemplatesToolStripMenuItem").Should().NotBeNull();
        form.FindControl<DropDownButton>("tsmiOptions").Should().NotBeNull();
        form.FindControl<CheckBox>("Amend").Should().NotBeNull();
        form.FindControl<CheckBox>("ResetAuthor").Should().NotBeNull();
        form.FindControl<CheckBox>("StageInSuperproject").Should().NotBeNull();
        form.FindControl<Button>("ResetSoft").Should().NotBeNull();
        form.FindControl<Button>("SolveMergeconflicts").Should().NotBeNull();

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(
            emittedKeys.Length,
            "each field must be routed through exactly one translation path");
    }

    [AvaloniaTest]
    public async Task FormCommit_should_add_body_only_word_wrap_to_the_message_context_menu()
    {
        int originalLineLimit = AppSettings.CommitValidationMaxCntCharsPerLine;
        AppSettings.CommitValidationMaxCntCharsPerLine = 12;
        FormCommit form = new(new GitUICommands(_serviceContainer, CreateRepositoryWithTwoUnstagedChanges()));
        try
        {
            EditNetSpell message = form.GetTestAccessor().Message;
            message.Text = "subject line remains unchanged\nbody words need wrapping";

            EditNetSpell.TestAccessor messageAccessor = message.GetTestAccessor();
            messageAccessor.OpenContextMenu();
            MenuItem wordWrap = messageAccessor.ContextMenu.Items
                .OfType<MenuItem>()
                .Single(item => Equals(item.Header, "_Word wrap (except subject line)"));

            wordWrap.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));

            message.Text.Split('\n')[0].Should().Be("subject line remains unchanged");
            message.Text.Replace("\r\n", "\n", StringComparison.Ordinal)
                .Should().Be("subject line remains unchanged\nbody words\nneed\nwrapping");
        }
        finally
        {
            form.Close();
            await form.GetTestAccessor().ClosePersistenceTask;
            AppSettings.CommitValidationMaxCntCharsPerLine = originalLineLimit;
        }
    }

    [AvaloniaTest]
    public async Task FormCommit_should_match_the_96_dpi_designer_shell_and_control_order()
    {
        FormCommit form = new(new GitUICommands(_serviceContainer, CreateRepositoryWithStagedAndUnstagedChanges()));
        try
        {
            form.Show();
            FileStatusList unstaged = form.FindControl<FileStatusList>("Unstaged")!;
            FileStatusList staged = form.FindControl<FileStatusList>("Staged")!;
            await WaitForCountsAsync(unstaged, 1, staged, 1);

            form.ClientSize.Should().Be(new Size(918, 644));
            Grid splitMain = form.FindControl<Grid>("splitMain")!;
            Grid splitLeft = form.FindControl<Grid>("splitLeft")!;
            Grid splitRight = form.FindControl<Grid>("splitRight")!;
            Grid tableLayoutPanel1 = form.FindControl<Grid>("tableLayoutPanel1")!;
            Grid toolbarCommit = form.FindControl<Grid>("toolbarCommit")!;
            StackPanel flowCommitButtons = form.FindControl<StackPanel>("flowCommitButtons")!;

            splitMain.ColumnDefinitions.Select(column => column.Width).Should().Equal(
                new GridLength(397),
                new GridLength(6),
                new GridLength(1, GridUnitType.Star));
            splitLeft.RowDefinitions.Select(row => row.Height).Should().Equal(
                new GridLength(268),
                new GridLength(6),
                new GridLength(1, GridUnitType.Star));
            splitRight.RowDefinitions.Select(row => row.Height).Should().Equal(
                new GridLength(412),
                new GridLength(6),
                new GridLength(192));
            tableLayoutPanel1.RowDefinitions.Select(row => row.Height).Should().Equal(
                new GridLength(28),
                new GridLength(1, GridUnitType.Star));
            splitLeft.Bounds.Size.Should().Be(new Size(391, 610));
            splitRight.Bounds.Size.Should().Be(new Size(509, 610));
            flowCommitButtons.Bounds.Width.Should().Be(171);
            toolbarCommit.Bounds.Width.Should().Be(324);

            Grid.GetColumn(flowCommitButtons).Should().Be(0);
            Grid.GetRowSpan(flowCommitButtons).Should().Be(2);
            form.FindControl<FileViewer>("SelectedDiff")!.TranslatePoint(default, splitRight)!.Value.Y
                .Should().BeLessThan(form.FindControl<GitUI.SpellChecker.EditNetSpell>("Message")!.TranslatePoint(default, splitRight)!.Value.Y);

            DropDownButton messageMenu = form.FindControl<DropDownButton>("commitMessageToolStripMenuItem")!;
            DropDownButton optionsMenu = form.FindControl<DropDownButton>("tsmiOptions")!;
            DropDownButton templatesMenu = form.FindControl<DropDownButton>("commitTemplatesToolStripMenuItem")!;
            Grid.GetColumn(messageMenu).Should().Be(0);
            Grid.GetColumn(optionsMenu).Should().Be(2);
            templatesMenu.GetVisualAncestors().Should().Contain(toolbarCommit);
            optionsMenu.Bounds.Width.Should().BeGreaterThan(0, "Options must remain visible before toolbar overflow items");
            form.FindControl<StackPanel>("toolbarCommitInlineItems")!.IsVisible.Should().BeFalse(
                "the source ToolStrip overflows templates at the 324-DIP Designer width");
            form.FindControl<DropDownButton>("toolbarCommitOverflow")!.IsVisible.Should().BeTrue();

            flowCommitButtons.GetVisualChildren().OfType<Control>().Select(control => control.Name).Should().ContainInOrder(
                "Commit",
                "CommitAndPush",
                "StageInSuperproject",
                "Amend",
                "AmendPanel",
                "StashStaged",
                "btnResetAllChanges",
                "btnResetUnstagedChanges");
            form.FindControl<TextBlock>("commitStagedCount")!.Text.Should().Be("1/2");
            form.FindControl<Button>("Commit")!.IsEnabled.Should().BeTrue(
                "the original validates an empty commit message after the enabled Commit button is invoked");
        }
        finally
        {
            form.Close();
            await form.GetTestAccessor().ClosePersistenceTask;
        }
    }

    [AvaloniaTest]
    public async Task FormCommit_should_apply_the_original_conventional_commit_prefix_rules()
    {
        FormCommit form = new(new GitUICommands(_serviceContainer, CreateRepositoryWithTwoUnstagedChanges()));
        FormCommit.TestAccessor accessor = form.GetTestAccessor();
        (string Current, int Position, bool Scope, string Expected, int ExpectedPosition)[] cases =
        [
            ("message", 0, false, "fix: message", 5),
            ("feat: message", 7, false, "fix: message", 6),
            ("feat(scope): message", 10, false, "fix(scope): message", 9),
            ("feat: message", 7, true, "fix(): message", 4),
            ("feat(scope): message", 10, true, "fix(scope): message", 12),
        ];

        try
        {
            form.Show();
            await WaitForCountsAsync(
                form.FindControl<FileStatusList>("Unstaged")!,
                2,
                form.FindControl<FileStatusList>("Staged")!,
                0);
            foreach ((string current, int position, bool scope, string expected, int expectedPosition) in cases)
            {
                accessor.SetMessageState(current, position);
                accessor.IncludeFeatureParentheses = scope;
                (string message, int selectionStart) = accessor.PrefixOrReplaceKeyword("fix");
                message.Should().Be(expected);
                selectionStart.Should().Be(expectedPosition);
            }
        }
        finally
        {
            form.Close();
            await accessor.ClosePersistenceTask;
        }
    }

    [AvaloniaTest]
    public void FormCommit_should_register_the_original_commit_message_auto_complete_providers()
    {
        FormCommit form = new(new GitUICommands(_serviceContainer, CreateRepositoryWithTwoUnstagedChanges()));

        form.GetTestAccessor().Message.GetTestAccessor().AutoCompleteProviderCount.Should().Be(2);

        form.Close();
    }

    [AvaloniaTest]
    public Task FormCommit_should_initialize_a_fixup_message_like_the_original()
        => AssertCommitKindAsync(CommitKind.Fixup, "fixup! Target commit", editable: false);

    [AvaloniaTest]
    public Task FormCommit_should_initialize_a_squash_message_like_the_original()
        => AssertCommitKindAsync(CommitKind.Squash, "squash! Target commit", editable: false);

    [AvaloniaTest]
    public Task FormCommit_should_initialize_an_amend_message_like_the_original()
        => AssertCommitKindAsync(
            CommitKind.Amend,
            $"amend! Target commit{Environment.NewLine}{Environment.NewLine}Target body",
            editable: true);

    private async Task AssertCommitKindAsync(CommitKind kind, string expected, bool editable)
    {
        string gitDirectory = Path.Combine(_workingDirectory, ".git");
        Directory.CreateDirectory(gitDirectory);
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDirectory);
        module.WorkingDirGitDir.Returns(gitDirectory);
        module.CommitEncoding.Returns(Encoding.UTF8);
        module.FilesEncoding.Returns(Encoding.UTF8);
        module.GetSelectedBranch().Returns("main");
        module.GetAllChangedFilesWithSubmodulesStatus(Arg.Any<CancellationToken>()).Returns([]);
        module.GetRefs(Arg.Any<RefsFilter>()).Returns([]);
        module.GetRemoteNames().Returns([]);
        GitUICommands commands = new(_serviceContainer, module);
        GitRevision revision = new(ObjectId.Parse("0123456789012345678901234567890123456789"))
        {
            Subject = "Target commit",
            Body = "Target body",
        };

        FormCommit form = new(commands, kind, revision);
        try
        {
            form.Show();
            GitUI.SpellChecker.EditNetSpell message = form.GetTestAccessor().Message;
            Button modifyMessage = form.FindControl<Button>("modifyCommitMessageButton")
                ?? throw new InvalidOperationException("Modify-message button was not created.");

            Stopwatch stopwatch = Stopwatch.StartNew();
            while (string.IsNullOrEmpty(message.Text) && stopwatch.Elapsed < TimeSpan.FromSeconds(15))
            {
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(10);
            }

            message.Text.Should().NotBeNullOrEmpty($"the {kind} initial message should be assigned");
            message.Text!.Replace("\r\n", "\n", StringComparison.Ordinal).Should().Be(
                expected.Replace("\r\n", "\n", StringComparison.Ordinal),
                $"the {kind} mode should preserve the original autosquash message format");
            message.IsEnabled.Should().Be(editable);
            modifyMessage.IsVisible.Should().Be(!editable);
        }
        finally
        {
            form.Close();
            await form.GetTestAccessor().ClosePersistenceTask;
        }
    }

    [AvaloniaTest]
    public void FormCommit_should_build_the_complete_commit_argument_contract()
    {
        bool useFormCommitMessage = AppSettings.UseFormCommitMessage;
        AppSettings.UseFormCommitMessage = true;
        GitModule module = CreateRepositoryWithTwoUnstagedChanges();
        FormCommit form = new(new GitUICommands(_serviceContainer, module));
        try
        {
            form.FindControl<CheckBox>("signOffToolStripMenuItem")!.IsChecked = true;
            form.FindControl<CheckBox>("noVerifyToolStripMenuItem")!.IsChecked = true;
            form.FindControl<CheckBox>("ResetAuthor")!.IsChecked = true;
            form.FindControl<TextBox>("toolAuthor")!.Text = "Custom Author <author@example.com>";
            form.FindControl<ComboBox>("gpgSignCommitToolStripComboBox")!.SelectedIndex = 3;
            form.FindControl<TextBox>("toolStripGpgKeyTextBox")!.Text = "ABC123";

            string arguments = form.GetTestAccessor().CreateCommitArguments(amend: true, allowEmpty: true).ToString();

            arguments.Should().Contain("--amend");
            arguments.Should().Contain("--signoff");
            arguments.Should().Contain("--no-verify");
            arguments.Should().Contain("--author=\"Custom Author <author@example.com>\"");
            arguments.Should().Contain("--gpg-sign=ABC123");
            arguments.Should().Contain("--allow-empty");
            arguments.Should().Contain("--reset-author");
            arguments.Should().Contain("COMMITMESSAGE");
        }
        finally
        {
            AppSettings.UseFormCommitMessage = useFormCommitMessage;
            form.Close();
        }
    }

    [AvaloniaTest]
    public async Task FormCommit_should_load_message_history_and_saved_templates()
    {
        string lastCommitMessage = AppSettings.LastCommitMessage;
        string commitTemplates = AppSettings.CommitTemplates;
        bool showOnlyMyMessages = AppSettings.CommitDialogShowOnlyMyMessages;
        string gitDirectory = Path.Combine(_workingDirectory, ".git");
        Directory.CreateDirectory(gitDirectory);
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDirectory);
        module.WorkingDirGitDir.Returns(gitDirectory);
        module.CommitEncoding.Returns(Encoding.UTF8);
        module.FilesEncoding.Returns(Encoding.UTF8);
        module.GetSelectedBranch().Returns("main");
        module.GetAllChangedFilesWithSubmodulesStatus(Arg.Any<CancellationToken>()).Returns([]);
        module.GetRefs(Arg.Any<RefsFilter>()).Returns([]);
        module.GetRemoteNames().Returns([]);
        FormCommit form = new(new GitUICommands(_serviceContainer, module));
        FormCommit.TestAccessor accessor = form.GetTestAccessor();
        try
        {
            form.Show();
            AppSettings.LastCommitMessage = "Last message from another repository";
            AppSettings.CommitDialogShowOnlyMyMessages = false;
            CommitTemplateItem.SaveToSettings([new CommitTemplateItem("Saved template", "Template body", icon: null, isRegex: false)]);

            accessor.PopulateCommitMessageHistory();
            MenuItem lastMessage = accessor.CommitMessageFlyout.Items
                .OfType<MenuItem>()
                .First(item => Equals(item.Header, "Last message from another repository"));
            lastMessage.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            form.GetTestAccessor().Message.Text.Should().Be("Last message from another repository");

            accessor.PopulateCommitTemplates();
            MenuItem savedTemplate = accessor.CommitTemplatesFlyout.Items
                .OfType<MenuItem>()
                .First(item => Equals(item.Header, "Saved template"));
            savedTemplate.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            form.GetTestAccessor().Message.Text.Should().Be("Template body");

            MenuItem conventional = accessor.CommitTemplatesFlyout.Items.OfType<MenuItem>().Last(item => item.Items.OfType<MenuItem>().Any());
            conventional.Items.OfType<MenuItem>().Select(item => item.Header).Should().Contain(FeatCommitTypeForTest);

            // The commit-message settings entry follows the conventional-commits submenu.
            accessor.CommitTemplatesFlyout.Items.OfType<MenuItem>().Last().Header
                .Should().Be("_Edit commit message templates and settings...");
        }
        finally
        {
            AppSettings.LastCommitMessage = lastCommitMessage;
            AppSettings.CommitTemplates = commitTemplates;
            AppSettings.CommitDialogShowOnlyMyMessages = showOnlyMyMessages;
            form.Close();
            await accessor.ClosePersistenceTask;
        }
    }

    [AvaloniaTest]
    public void FormCommit_should_apply_the_shared_commit_message_validation_settings()
    {
        int maxFirstLine = AppSettings.CommitValidationMaxCntCharsFirstLine;
        int maxPerLine = AppSettings.CommitValidationMaxCntCharsPerLine;
        bool secondLineEmpty = AppSettings.CommitValidationSecondLineMustBeEmpty;
        string validationRegex = AppSettings.CommitValidationRegEx;
        StubMessageBoxHost stub = new() { Result = WinFormsShims.DialogResult.No };
        WinFormsShims.ShimHost.MessageBoxHost = stub;
        FormCommit form = new(new GitUICommands(_serviceContainer, CreateRepositoryWithTwoUnstagedChanges()));
        try
        {
            AppSettings.CommitValidationMaxCntCharsFirstLine = 5;
            AppSettings.CommitValidationMaxCntCharsPerLine = 0;
            AppSettings.CommitValidationSecondLineMustBeEmpty = false;
            AppSettings.CommitValidationRegEx = string.Empty;

            form.GetTestAccessor().IsCommitMessageValid("Too long").Should().BeFalse();
            stub.Messages.Should().ContainSingle(message => message.Contains("too many characters", StringComparison.Ordinal));
        }
        finally
        {
            AppSettings.CommitValidationMaxCntCharsFirstLine = maxFirstLine;
            AppSettings.CommitValidationMaxCntCharsPerLine = maxPerLine;
            AppSettings.CommitValidationSecondLineMustBeEmpty = secondLineEmpty;
            AppSettings.CommitValidationRegEx = validationRegex;
            WinFormsShims.ShimHost.MessageBoxHost = new StubMessageBoxHost { Result = WinFormsShims.DialogResult.Yes };
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormCommit_should_apply_commit_font_auto_wrap_and_second_line_indentation()
    {
        WinFormsShims.Font commitFont = AppSettings.CommitFont;
        int maxFirstLine = AppSettings.CommitValidationMaxCntCharsFirstLine;
        int maxPerLine = AppSettings.CommitValidationMaxCntCharsPerLine;
        bool secondLineEmpty = AppSettings.CommitValidationSecondLineMustBeEmpty;
        bool autoWrap = AppSettings.CommitValidationAutoWrap;
        bool indentAfterFirstLine = AppSettings.CommitValidationIndentAfterFirstLine;
        using WinFormsShims.Font expectedFont = new("Consolas", 13F);
        try
        {
            AppSettings.CommitFont = expectedFont;
            AppSettings.CommitValidationMaxCntCharsFirstLine = 0;
            AppSettings.CommitValidationMaxCntCharsPerLine = 20;
            AppSettings.CommitValidationSecondLineMustBeEmpty = true;
            AppSettings.CommitValidationAutoWrap = true;
            AppSettings.CommitValidationIndentAfterFirstLine = true;
            FormCommit form = new(new GitUICommands(_serviceContainer, CreateRepositoryWithTwoUnstagedChanges()));
            try
            {
                GitUI.SpellChecker.EditNetSpell message = form.GetTestAccessor().Message;
                message.TextBoxFont.Name.Should().Be(expectedFont.Name);
                message.TextBoxFont.Size.Should().Be(expectedFont.Size);

                message.Text = "Subject\nbody words that must wrap onto another line";

                string[] lines = message.Text.ReplaceLineEndings("\n").Split('\n');
                lines[1].Should().BeEmpty();
                lines[2].Should().StartWith(" - ");
                lines.Skip(2).Should().OnlyContain(line => line.Length <= 20);
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.CommitFont = commitFont;
            AppSettings.CommitValidationMaxCntCharsFirstLine = maxFirstLine;
            AppSettings.CommitValidationMaxCntCharsPerLine = maxPerLine;
            AppSettings.CommitValidationSecondLineMustBeEmpty = secondLineEmpty;
            AppSettings.CommitValidationAutoWrap = autoWrap;
            AppSettings.CommitValidationIndentAfterFirstLine = indentAfterFirstLine;
        }
    }

    [AvaloniaTest]
    public async Task FormCommit_should_restore_selection_filter_and_visibility_settings()
    {
        bool selectionFilterVisible = AppSettings.CommitDialogSelectionFilter;
        bool showResetAll = AppSettings.ShowResetAllChanges;
        bool showResetWorkTree = AppSettings.ShowResetWorkTreeChanges;
        bool showCommitAndPush = AppSettings.ShowCommitAndPush;
        try
        {
            AppSettings.CommitDialogSelectionFilter = true;
            AppSettings.ShowResetAllChanges = false;
            AppSettings.ShowResetWorkTreeChanges = false;
            AppSettings.ShowCommitAndPush = false;
            FormCommit form = new(new GitUICommands(_serviceContainer, CreateRepositoryWithTwoUnstagedChanges()));
            try
            {
                form.Show();
                FormCommit.TestAccessor accessor = form.GetTestAccessor();
                FileStatusList unstaged = form.FindControl<FileStatusList>("Unstaged")!;
                await WaitUntilAsync(() => unstaged.GitItemStatuses.Count == 2);

                accessor.SelectionFilterVisible.Should().BeTrue();
                form.FindControl<Button>("btnResetAllChanges")!.IsVisible.Should().BeFalse();
                form.FindControl<Button>("btnResetUnstagedChanges")!.IsVisible.Should().BeFalse();
                form.FindControl<Button>("CommitAndPush")!.IsVisible.Should().BeFalse();

                accessor.SelectionFilter.Text = "tracked";
                accessor.ApplySelectionFilter();
                unstaged.SelectedGitItems.Should().ContainSingle(item => item.Name == "tracked.txt");

                accessor.SelectionFilter.Text = "[";
                accessor.ApplySelectionFilter();
                accessor.SelectionFilter.Classes.Should().Contain("file-filter-invalid");

                accessor.ExecuteCommand(FormCommit.Command.ToggleSelectionFilter).Should().BeTrue();
                accessor.SelectionFilterVisible.Should().BeFalse();
            }
            finally
            {
                form.Close();
                await form.GetTestAccessor().ClosePersistenceTask;
            }
        }
        finally
        {
            AppSettings.CommitDialogSelectionFilter = selectionFilterVisible;
            AppSettings.ShowResetAllChanges = showResetAll;
            AppSettings.ShowResetWorkTreeChanges = showResetWorkTree;
            AppSettings.ShowCommitAndPush = showCommitAndPush;
        }
    }

    [AvaloniaTest]
    public async Task FormCommit_should_stage_and_commit_changes_end_to_end()
    {
        bool closeProcessDialog = AppSettings.CloseProcessDialog;
        string lastCommitMessage = AppSettings.LastCommitMessage;
        AppSettings.CloseProcessDialog = true;
        TestScriptEventRecorder scriptEvents = TestScriptEventRecorder.Install(_serviceContainer);
        GitModule module = CreateRepositoryWithTwoUnstagedChanges();
        ObjectId initialCommit = module.GetCurrentCheckout();
        GitUICommands commands = new(_serviceContainer, module);
        FormCommit form = new(commands);
        try
        {
            form.Show();
            FileStatusList unstaged = form.FindControl<FileStatusList>("Unstaged")
                ?? throw new InvalidOperationException("Unstaged file list was not created.");
            FileStatusList staged = form.FindControl<FileStatusList>("Staged")
                ?? throw new InvalidOperationException("Staged file list was not created.");
            Button stageAll = form.FindControl<Button>("toolStageAllItem")
                ?? throw new InvalidOperationException("Stage-all button was not created.");
            GitUI.SpellChecker.EditNetSpell message = form.GetTestAccessor().Message;
            Button commit = form.FindControl<Button>("Commit")
                ?? throw new InvalidOperationException("Commit button was not created.");
            Button commitAndPush = form.FindControl<Button>("CommitAndPush")
                ?? throw new InvalidOperationException("Commit-and-push button was not created.");

            await WaitForCountsAsync(unstaged, 2, staged, 0);
            stageAll.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            await WaitForCountsAsync(unstaged, 0, staged, 2);

            message.Text = "Commit from Avalonia";
            await WaitUntilAsync(() => commit.IsEnabled);
            commitAndPush.IsEnabled.Should().BeTrue("FormPush is now available for the follow-up action");
            form.CaptureRenderedFrame().Should().NotBeNull("the ready-to-commit dialog should render headlessly");
            commit.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));

            await WaitUntilAsync(() => !form.IsVisible && module.GetCurrentCheckout() != initialCommit);

            GitRevision revision = module.GetRevision(module.GetCurrentCheckout(), shortFormat: true, loadRefs: false);
            revision.Subject.Should().Be("Commit from Avalonia");
            module.GetAllChangedFilesWithSubmodulesStatus().Should().BeEmpty();
            File.Exists(Path.Combine(module.WorkingDirGitDir, "COMMITMESSAGE")).Should().BeFalse();
            scriptEvents.Events.Should().Equal(ScriptEvent.BeforeCommit, ScriptEvent.AfterCommit);
        }
        finally
        {
            AppSettings.CloseProcessDialog = closeProcessDialog;
            AppSettings.LastCommitMessage = lastCommitMessage;
            if (form.IsVisible)
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public async Task FormCommit_should_apply_author_signoff_and_no_verify_end_to_end()
    {
        bool closeProcessDialog = AppSettings.CloseProcessDialog;
        bool closeCommitDialog = AppSettings.CloseCommitDialogAfterCommit;
        bool useFormCommitMessage = AppSettings.UseFormCommitMessage;
        string lastCommitMessage = AppSettings.LastCommitMessage;
        AppSettings.CloseProcessDialog = true;
        AppSettings.CloseCommitDialogAfterCommit = true;
        AppSettings.UseFormCommitMessage = true;
        GitModule module = CreateRepositoryWithTwoUnstagedChanges();
        ObjectId initialCommit = module.GetCurrentCheckout();
        string hookPath = Path.Combine(_workingDirectory, ".git", "hooks", "pre-commit");
        File.WriteAllText(hookPath, "#!/bin/sh\nexit 1\n", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(
                hookPath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        FormCommit form = new(new GitUICommands(_serviceContainer, module));
        try
        {
            form.Show();
            FileStatusList unstaged = form.FindControl<FileStatusList>("Unstaged")!;
            FileStatusList staged = form.FindControl<FileStatusList>("Staged")!;
            await WaitForCountsAsync(unstaged, 2, staged, 0);

            form.FindControl<Button>("toolStageAllItem")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            await WaitForCountsAsync(unstaged, 0, staged, 2);

            form.GetTestAccessor().Message.Text = "Commit with options";
            form.FindControl<TextBox>("toolAuthor")!.Text = "Custom Author <author@example.com>";
            form.FindControl<CheckBox>("signOffToolStripMenuItem")!.IsChecked = true;
            form.FindControl<CheckBox>("noVerifyToolStripMenuItem")!.IsChecked = true;
            form.FindControl<ComboBox>("gpgSignCommitToolStripComboBox")!.SelectedIndex = 1;
            Button commit = form.FindControl<Button>("Commit")!;
            await WaitUntilAsync(() => commit.IsEnabled);
            commit.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));

            await WaitUntilAsync(() => !form.IsVisible && module.GetCurrentCheckout() != initialCommit);
            GitRevision revision = module.GetRevision(module.GetCurrentCheckout(), shortFormat: false, loadRefs: false);
            revision.Author.Should().Be("Custom Author");
            revision.AuthorEmail.Should().Be("author@example.com");
            revision.Body.Should().Contain("Signed-off-by: Avalonia Test <avalonia@example.com>");
        }
        finally
        {
            AppSettings.CloseProcessDialog = closeProcessDialog;
            AppSettings.CloseCommitDialogAfterCommit = closeCommitDialog;
            AppSettings.UseFormCommitMessage = useFormCommitMessage;
            AppSettings.LastCommitMessage = lastCommitMessage;
            if (form.IsVisible)
            {
                form.Close();
            }
        }
    }

    [Test]
    public void StartCommitDialog_should_honor_pre_commit_cancellation()
    {
        GitModule module = CreateRepositoryWithTwoUnstagedChanges();
        GitUICommands commands = new(_serviceContainer, module);
        commands.PreCommit += (_, e) => e.Cancel = true;

        bool result = commands.StartCommitDialog(owner: null);

        result.Should().BeFalse();
    }

    [AvaloniaTest]
    public async Task FormCommit_should_stage_and_unstage_selected_and_all_files()
    {
        GitModule module = CreateRepositoryWithTwoUnstagedChanges();
        GitUICommands commands = new(_serviceContainer, module);
        FormCommit form = new(commands);
        try
        {
            form.Show();
            FileStatusList unstaged = form.FindControl<FileStatusList>("Unstaged")
                ?? throw new InvalidOperationException("Unstaged file list was not created.");
            FileStatusList staged = form.FindControl<FileStatusList>("Staged")
                ?? throw new InvalidOperationException("Staged file list was not created.");
            Button stageSelected = form.FindControl<Button>("toolStageItem")
                ?? throw new InvalidOperationException("Stage button was not created.");
            Button stageAll = form.FindControl<Button>("toolStageAllItem")
                ?? throw new InvalidOperationException("Stage-all button was not created.");
            Button unstageSelected = form.FindControl<Button>("toolUnstageItem")
                ?? throw new InvalidOperationException("Unstage button was not created.");
            Button unstageAll = form.FindControl<Button>("toolUnstageAllItem")
                ?? throw new InvalidOperationException("Unstage-all button was not created.");

            await WaitForCountsAsync(unstaged, 2, staged, 0);

            await DoubleClickSelectedItemAsync(unstaged);
            await WaitForCountsAsync(unstaged, 1, staged, 1);

            await DoubleClickSelectedItemAsync(staged);
            await WaitForCountsAsync(unstaged, 2, staged, 0);

            unstaged.SelectedGitItems = [unstaged.GitItemStatuses[0]];
            await WaitUntilAsync(() => stageSelected.IsEnabled);
            stageSelected.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            await WaitForCountsAsync(unstaged, 1, staged, 1);

            staged.SelectedGitItems = [staged.GitItemStatuses[0]];
            await WaitUntilAsync(() => unstageSelected.IsEnabled);
            unstageSelected.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            await WaitForCountsAsync(unstaged, 2, staged, 0);

            stageAll.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            await WaitForCountsAsync(unstaged, 0, staged, 2);

            unstageAll.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            await WaitForCountsAsync(unstaged, 2, staged, 0);

            module.GetAllChangedFilesWithSubmodulesStatus().Should().OnlyContain(item => item.Staged == StagedStatus.WorkTree);
        }
        finally
        {
            form.Close();
            await form.GetTestAccessor().ClosePersistenceTask;
        }
    }

    [AvaloniaTest]
    public async Task FormCommit_should_show_staged_and_unstaged_changes_with_a_diff_preview()
    {
        GitModule module = CreateRepositoryWithStagedAndUnstagedChanges();
        GitUICommands commands = new(_serviceContainer, module);
        FormCommit form = new(commands);
        try
        {
            form.Show();
            FileStatusList unstaged = form.FindControl<FileStatusList>("Unstaged")
                ?? throw new InvalidOperationException("Unstaged file list was not created.");
            FileStatusList staged = form.FindControl<FileStatusList>("Staged")
                ?? throw new InvalidOperationException("Staged file list was not created.");
            FileViewer selectedDiff = form.FindControl<FileViewer>("SelectedDiff")
                ?? throw new InvalidOperationException("Diff viewer was not created.");
            TextEditor diffEditor = selectedDiff.TextEditor;

            await WaitUntilAsync(() =>
                unstaged.GitItemStatuses.Count == 1
                && staged.GitItemStatuses.Count == 1
                && diffEditor.Document?.Text.Contains("unstaged line", StringComparison.Ordinal) == true,
                () => $"unstaged/staged={unstaged.GitItemStatuses.Count}/{staged.GitItemStatuses.Count}; diff={diffEditor.Document?.Text}");

            form.CaptureRenderedFrame().Should().NotBeNull("the dirty-repository staging view should render headlessly");
            unstaged.SelectedItem.Should().NotBeNull();
            staged.SelectedItem.Should().BeNull("the unstaged list is selected first, matching the upstream dialog");

            staged.SelectedGitItems = [staged.GitItemStatuses[0]];
            await WaitUntilAsync(() =>
                diffEditor.Document?.Text.Contains("staged line", StringComparison.Ordinal) == true
                && diffEditor.Document.Text.Contains("unstaged line", StringComparison.Ordinal) == false);
            staged.SelectedItem.Should().NotBeNull();
            unstaged.SelectedItem.Should().BeNull();
            selectedDiff.GetTestAccessor().OpenWithDifftool.Should().NotBeNull(
                "FormCommit should retain its external-difftool consumer action in FileViewer");
        }
        finally
        {
            form.Close();
            await form.GetTestAccessor().ClosePersistenceTask;
        }
    }

    private GitModule CreateRepositoryWithStagedAndUnstagedChanges()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");

        string fileName = Path.Combine(_workingDirectory, "tracked.txt");
        File.WriteAllText(fileName, "initial line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });

        File.AppendAllText(fileName, "staged line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        File.AppendAllText(fileName, "unstaged line\n");
        return module;
    }

    private GitModule CreateRepositoryWithTwoUnstagedChanges()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");

        string trackedFileName = Path.Combine(_workingDirectory, "tracked.txt");
        File.WriteAllText(trackedFileName, "initial line\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });

        File.AppendAllText(trackedFileName, "modified line\n");
        File.WriteAllText(Path.Combine(_workingDirectory, "untracked.txt"), "new line\n");
        return module;
    }

    private static async Task DoubleClickSelectedItemAsync(FileStatusList fileStatusList)
    {
        fileStatusList.SelectedGitItems = [fileStatusList.GitItemStatuses[0]];
        await WaitUntilAsync(() => fileStatusList.SelectedGitItems.Count > 0);
        fileStatusList.GetTestAccessor().DoubleClick();
    }

    private static Task WaitForCountsAsync(FileStatusList unstaged, int unstagedCount, FileStatusList staged, int stagedCount)
    {
        return WaitUntilAsync(
            () => unstaged.GitItemStatuses.Count == unstagedCount && staged.GitItemStatuses.Count == stagedCount,
            () => $"expected unstaged/staged counts {unstagedCount}/{stagedCount}, actual {unstaged.GitItemStatuses.Count}/{staged.GitItemStatuses.Count}");
    }

    private static async Task WaitUntilAsync(Func<bool> condition, Func<string>? timeoutReason = null)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        condition().Should().BeTrue(timeoutReason?.Invoke() ?? "the changed files and diff should load before the timeout");
    }

    private sealed class StubMessageBoxHost : WinFormsShims.IMessageBoxHost
    {
        public List<string> Messages { get; } = [];

        public WinFormsShims.DialogResult Result { get; set; }

        public WinFormsShims.DialogResult Show(
            WinFormsShims.IWin32Window? owner,
            string? text,
            string? caption,
            WinFormsShims.MessageBoxButtons buttons,
            WinFormsShims.MessageBoxIcon icon,
            WinFormsShims.MessageBoxDefaultButton defaultButton)
        {
            Messages.Add(text ?? string.Empty);
            return buttons is WinFormsShims.MessageBoxButtons.YesNo or WinFormsShims.MessageBoxButtons.YesNoCancel
                ? Result
                : WinFormsShims.DialogResult.OK;
        }
    }
}
