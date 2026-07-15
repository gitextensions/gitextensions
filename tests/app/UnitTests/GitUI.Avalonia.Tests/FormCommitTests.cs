using System.ComponentModel.Design;
using System.Diagnostics;
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

namespace GitExtensionsTests;

[TestFixture]
public sealed class FormCommitTests
{
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

        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.Tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        Directory.Delete(_workingDirectory, recursive: true);
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
        (commitAndPush.Content as TextBlock)?.Text.Should().Be("Commit & _push");
        commitAndPush.IsEnabled.Should().BeFalse("there are no staged changes in the construction-only form");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(
            emittedKeys.Length,
            "each field must be routed through exactly one translation path");
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
}
