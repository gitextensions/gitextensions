using System.ComponentModel.Design;
using System.Diagnostics;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
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
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FormCommitTests
{
    private const string FeatCommitTypeForTest = "feat";

    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

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

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.Tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
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

        Button stageAll = form.FindControl<Button>("toolStageAllItem")
            ?? throw new InvalidOperationException("Stage-all button was not created.");
        Button unstageAll = form.FindControl<Button>("toolUnstageAllItem")
            ?? throw new InvalidOperationException("Unstage-all button was not created.");
        ToolTip.GetTip(stageAll).Should().Be("Stage all");
        ToolTip.GetTip(unstageAll).Should().Be("Unstage all");
        Button commitAndPush = form.FindControl<Button>("CommitAndPush")
            ?? throw new InvalidOperationException("Commit-and-push button was not created.");
        commitAndPush.Content.Should().Be("Commit & _push");
        commitAndPush.IsEnabled.Should().BeFalse("there are no staged changes in the construction-only form");

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
            TextBox message = form.FindControl<TextBox>("Message")
                ?? throw new InvalidOperationException("Commit message editor was not created.");
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
    public void FormCommit_should_load_message_history_and_saved_templates()
    {
        string lastCommitMessage = AppSettings.LastCommitMessage;
        string commitTemplates = AppSettings.CommitTemplates;
        bool showOnlyMyMessages = AppSettings.CommitDialogShowOnlyMyMessages;
        GitModule module = CreateRepositoryWithTwoUnstagedChanges();
        FormCommit form = new(new GitUICommands(_serviceContainer, module));
        try
        {
            AppSettings.LastCommitMessage = "Last message from another repository";
            AppSettings.CommitDialogShowOnlyMyMessages = false;
            CommitTemplateItem.SaveToSettings([new CommitTemplateItem("Saved template", "Template body", icon: null, isRegex: false)]);

            FormCommit.TestAccessor accessor = form.GetTestAccessor();
            accessor.PopulateCommitMessageHistory();
            MenuItem lastMessage = accessor.CommitMessageFlyout.Items
                .OfType<MenuItem>()
                .First(item => Equals(item.Header, "Last message from another repository"));
            lastMessage.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
            form.FindControl<TextBox>("Message")!.Text.Should().Be("Last message from another repository");

            accessor.PopulateCommitTemplates();
            MenuItem savedTemplate = accessor.CommitTemplatesFlyout.Items
                .OfType<MenuItem>()
                .First(item => Equals(item.Header, "Saved template"));
            savedTemplate.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
            form.FindControl<TextBox>("Message")!.Text.Should().Be("Template body");

            MenuItem conventional = accessor.CommitTemplatesFlyout.Items.OfType<MenuItem>().Last();
            conventional.Items.OfType<MenuItem>().Select(item => item.Header).Should().Contain(FeatCommitTypeForTest);
        }
        finally
        {
            AppSettings.LastCommitMessage = lastCommitMessage;
            AppSettings.CommitTemplates = commitTemplates;
            AppSettings.CommitDialogShowOnlyMyMessages = showOnlyMyMessages;
            form.Close();
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
    public async Task FormCommit_should_stage_and_commit_changes_end_to_end()
    {
        bool closeProcessDialog = AppSettings.CloseProcessDialog;
        string lastCommitMessage = AppSettings.LastCommitMessage;
        AppSettings.CloseProcessDialog = true;
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
            TextBox message = form.FindControl<TextBox>("Message")
                ?? throw new InvalidOperationException("Commit message editor was not created.");
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

            form.FindControl<TextBox>("Message")!.Text = "Commit with options";
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

            DoubleClickSelectedItem(form, unstaged);
            await WaitForCountsAsync(unstaged, 1, staged, 1);

            DoubleClickSelectedItem(form, staged);
            await WaitForCountsAsync(unstaged, 2, staged, 0);

            stageSelected.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            await WaitForCountsAsync(unstaged, 1, staged, 1);

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
            TextEditor diffEditor = selectedDiff.FindControl<TextEditor>("TextEditor")
                ?? throw new InvalidOperationException("Diff editor was not created.");

            await WaitUntilAsync(() =>
                unstaged.GitItemStatuses.Count == 1
                && staged.GitItemStatuses.Count == 1
                && diffEditor.Document?.Text.Contains("unstaged line", StringComparison.Ordinal) == true);

            form.CaptureRenderedFrame().Should().NotBeNull("the dirty-repository staging view should render headlessly");
            unstaged.SelectedItem.Should().NotBeNull();
            staged.SelectedItem.Should().BeNull("the unstaged list is selected first, matching the upstream dialog");

            ListBox stagedFiles = staged.FindControl<ListBox>("lstFiles")
                ?? throw new InvalidOperationException("Staged file list box was not created.");
            stagedFiles.SelectedIndex = 0;
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

    private static void DoubleClickSelectedItem(FormCommit form, FileStatusList fileStatusList)
    {
        ListBox listBox = fileStatusList.FindControl<ListBox>("lstFiles")
            ?? throw new InvalidOperationException("File list box was not created.");
        ListBoxItem item = listBox.ContainerFromIndex(listBox.SelectedIndex) as ListBoxItem
            ?? throw new InvalidOperationException("Selected file row was not realized.");
        Point point = item.TranslatePoint(new Point(12, item.Bounds.Height / 2), form)
            ?? throw new InvalidOperationException("Selected file row position was not available.");

        form.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        form.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        form.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        form.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
    }

    private static Task WaitForCountsAsync(FileStatusList unstaged, int unstagedCount, FileStatusList staged, int stagedCount)
    {
        return WaitUntilAsync(() =>
            unstaged.GitItemStatuses.Count == unstagedCount
            && staged.GitItemStatuses.Count == stagedCount);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        condition().Should().BeTrue("the changed files and diff should load before the timeout");
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
