using System.Collections.Concurrent;
using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using CommonTestUtils;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Editor;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class RevisionDiffThreadingTests
{
    private const string FirstFile = "folder/first.txt";
    private const string SecondFile = "folder/second.txt";
    private const string FirstContents = "changed first\n";
    private const string SecondContents = "changed second\n";
    private const string SamplePatch = "diff --git a/folder/first.txt b/folder/first.txt\n--- a/folder/first.txt\n+++ b/folder/first.txt\n@@ -1 +1 @@\n-before first\n+changed first\n";
    private readonly ConcurrentQueue<Exception> _exceptions = new();
    private readonly List<RevisionDiffControl> _controls = [];
    private readonly List<Window> _windows = [];
    private GitModuleTestHelper _repository = null!;
    private ServiceContainer _serviceContainer = null!;
    private JoinableTaskContext? _previousContext;
    private JoinableTaskContext _context = null!;
    private bool _showAuthorAvatar;
    private GitRevision _parent = null!;
    private GitRevision _head = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        Dispatcher.UIThread.VerifyAccess();
        _previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        _context = new JoinableTaskContext(Thread.CurrentThread, SynchronizationContext.Current);
        ThreadHelper.JoinableTaskContext = _context;
        _exceptions.Clear();
        _controls.Clear();
        _windows.Clear();
        _showAuthorAvatar = AppSettings.BlameShowAuthorAvatar;
        AppSettings.BlameShowAuthorAvatar = false;
        WinFormsShims.Application.ThreadException += Application_ThreadException;

        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        System.IO.Abstractions.FileSystem fileSystem = new();
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(new GitDirectoryResolver(fileSystem));
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _repository = new GitModuleTestHelper(nameof(RevisionDiffThreadingTests));
        _repository.CreateRepoFile("folder", "first.txt", "before first\n");
        _repository.CreateRepoFile("folder", "second.txt", "before second\n");
        _repository.Module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", FirstFile, SecondFile }).Should().BeTrue();
        _repository.Module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "before" }).Should().BeTrue();
        _parent = _repository.Module.GetRevision(_repository.Module.GetCurrentCheckout());
        _repository.CreateRepoFile("folder", "first.txt", FirstContents);
        _repository.CreateRepoFile("folder", "second.txt", SecondContents);
        _repository.Module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "after" }).Should().BeTrue();
        _head = _repository.Module.GetRevision(_repository.Module.GetCurrentCheckout());
    }

    [TearDown]
    public async Task TearDown()
    {
        try
        {
            foreach (Window window in _windows)
            {
                window.Close();
            }

            foreach (RevisionDiffControl control in _controls)
            {
                control.CancelLoadCustomDifftools();
                control.CancelBackgroundTasks();
            }

            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(10));
            await ThreadHelper.JoinPendingOperationsAsync(cancellation.Token);
            cancellation.IsCancellationRequested.Should().BeFalse("all owned operations must finish before restoring the UI context");
            _exceptions.Should().BeEmpty("FileAndForget must not hide a dispatcher access failure from the regression test");
        }
        finally
        {
            WinFormsShims.Application.ThreadException -= Application_ThreadException;
            AppSettings.BlameShowAuthorAvatar = _showAuthorAvatar;
            _repository.Dispose();
            TestDirectory.Delete(_repository.TemporaryPath);
            _serviceContainer.Dispose();
            ThreadHelper.JoinableTaskContext = _previousContext!;
            _context.Dispose();
        }
    }

    [AvaloniaTest]
    public async Task SelectFileOrFolder_should_render_selected_file_after_background_dispatch([Values] bool fileTreeMode)
    {
        RevisionDiffControl control = CreateControl(fileTreeMode);
        SeedFiles(control, fileTreeMode);
        RevisionDiffControl.TestAccessor accessor = control.GetTestAccessor();
        accessor.DiffFiles.tsmiBlame.IsChecked.Should().BeFalse();

        control.SelectFileOrFolder(() => { }, RelativePath.From(FirstFile));
        await JoinAsync(control);

        _exceptions.Should().BeEmpty();
        accessor.DiffFiles.SelectedItem.Should().NotBeNull();
        accessor.DiffFiles.SelectedItem!.Item.Name.Should().Be(FirstFile);
        accessor.DiffText.GetText().Should().Contain("changed first");
        accessor.DiffText.GetTestAccessor().ViewMode.Should().Be(fileTreeMode ? ViewMode.Text : ViewMode.Diff);
        accessor.DiffText.IsVisible.Should().BeTrue();
        accessor.BlameControl.IsVisible.Should().BeFalse();

        control.SelectFileOrFolder(() => { }, RelativePath.From(SecondFile));
        await JoinAsync(control);

        _exceptions.Should().BeEmpty();
        accessor.DiffFiles.SelectedItem!.Item.Name.Should().Be(SecondFile);
        accessor.DiffText.GetText().Should().Contain("changed second").And.NotContain("changed first");
    }

    [AvaloniaTest]
    public async Task SelectFileOrFolder_should_render_selected_folder_after_background_dispatch()
    {
        RevisionDiffControl control = CreateControl(fileTreeMode: true);
        SeedFiles(control, fileTreeMode: true);
        RevisionDiffControl.TestAccessor accessor = control.GetTestAccessor();

        control.SelectFileOrFolder(() => { }, RelativePath.From("folder"));
        await JoinAsync(control);

        _exceptions.Should().BeEmpty();
        accessor.DiffFiles.SelectedFolder.Should().Be(RelativePath.From("folder"));
        accessor.DiffText.GetText().Should().Contain("(2) folder/").And.Contain("first.txt").And.Contain("second.txt");
        accessor.DiffText.IsVisible.Should().BeTrue();
        accessor.BlameControl.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    public async Task SelectFileOrFolder_should_load_real_blame_after_background_dispatch([Values] bool fileTreeMode)
    {
        RevisionDiffControl control = CreateControl(fileTreeMode);
        SeedFiles(control, fileTreeMode);
        RevisionDiffControl.TestAccessor accessor = control.GetTestAccessor();

        control.SelectFileOrFolder(() => { }, RelativePath.From(FirstFile), requestBlame: true);
        await JoinAsync(control);

        _exceptions.Should().BeEmpty();
        accessor.DiffFiles.tsmiBlame.IsChecked.Should().BeTrue();
        accessor.DiffFiles.SelectedItem!.Item.Name.Should().Be(FirstFile);
        accessor.BlameControl.IsVisible.Should().BeTrue();
        accessor.DiffText.IsVisible.Should().BeFalse();
        accessor.BlameControl.GetTestAccessor().BlameFile.GetText().Should().Contain("changed first");
        accessor.BlameControl.GetTestAccessor().Blame.Should().NotBeNull();
        accessor.BlameControl.GetTestAccessor().Blame!.Lines.Should().ContainSingle();
        accessor.BlameControl.GetTestAccessor().Blame!.Lines[0].Commit.ObjectId.Should().Be(_head.ObjectId);
    }

    [AvaloniaTest]
    public async Task ShowSelectedFile_should_read_the_real_menu_and_render_on_the_owner_from_a_worker(
        [Values] bool fileTreeMode,
        [Values] bool blame)
    {
        RevisionDiffControl control = CreateControl(fileTreeMode);
        SeedFiles(control, fileTreeMode);
        RevisionDiffControl.TestAccessor accessor = control.GetTestAccessor();
        accessor.DiffFiles.SelectFileOrFolder(RelativePath.From(FirstFile), notify: false).Should().BeTrue();
        accessor.DiffFiles.tsmiBlame.IsChecked = blame;
        bool enteredOnWorker = false;

        await Task.Run(() =>
        {
            enteredOnWorker = !Dispatcher.UIThread.CheckAccess();
            accessor.ShowSelectedFile();
        });
        await JoinAsync(control);

        enteredOnWorker.Should().BeTrue();
        _exceptions.Should().BeEmpty();
        accessor.DiffFiles.SelectedItem!.Item.Name.Should().Be(FirstFile);
        accessor.BlameControl.IsVisible.Should().Be(blame);
        accessor.DiffText.IsVisible.Should().Be(!blame);
        FileViewer displayedViewer = blame ? accessor.BlameControl.GetTestAccessor().BlameFile : accessor.DiffText;
        displayedViewer.GetText().Should().Contain("changed first");
    }

    [AvaloniaTest]
    public async Task ShowSelectedFile_should_clear_the_view_when_no_file_is_selected()
    {
        RevisionDiffControl control = CreateControl(fileTreeMode: false);
        SeedFiles(control, fileTreeMode: false);
        RevisionDiffControl.TestAccessor accessor = control.GetTestAccessor();
        control.SelectFileOrFolder(() => { }, RelativePath.From(FirstFile));
        await JoinAsync(control);
        accessor.DiffText.GetText().Should().Contain("changed first");
        accessor.DiffFiles.ClearSelected();

        accessor.ShowSelectedFile();
        await JoinAsync(control);

        _exceptions.Should().BeEmpty();
        accessor.DiffFiles.SelectedItems.Should().BeEmpty();
        accessor.DiffText.GetText().Should().BeEmpty();
        accessor.DiffText.IsVisible.Should().BeTrue();
        accessor.BlameControl.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    public async Task DisplayDiffTab_should_select_and_render_real_calculated_changes_on_the_owner_thread()
    {
        RevisionDiffControl control = CreateControl(fileTreeMode: false);
        RevisionDiffControl.TestAccessor accessor = control.GetTestAccessor();

        await Task.Run(() => control.DisplayDiffTab([_head]));
        await JoinAsync(control);

        _exceptions.Should().BeEmpty();
        control.DisplayedRevision.Should().Be(_head);
        accessor.DiffFiles.AllItems.Should().HaveCount(2);
        accessor.DiffFiles.SelectedItems.Should().ContainSingle();
        accessor.DiffText.GetText().Should().Contain("+changed");
        accessor.DiffText.GetTestAccessor().ViewMode.Should().Be(ViewMode.Diff);
    }

    [AvaloniaTest]
    public async Task RefreshArtificial_should_restore_the_real_worktree_selection_on_the_owner_thread()
    {
        GitRevision workingTree = new(ObjectId.WorkTreeId) { ParentIds = [_head.ObjectId] };
        RevisionDiffControl control = CreateControl(fileTreeMode: false, selectedRevisions: [workingTree]);
        RevisionDiffControl.TestAccessor accessor = control.GetTestAccessor();
        _repository.CreateRepoFile("folder", "first.txt", "worktree first\n");
        control.DisplayDiffTab([workingTree]);
        await JoinAsync(control);
        accessor.DiffFiles.SelectedItem.Should().NotBeNull();
        accessor.DiffFiles.SelectedItem!.Item.Name.Should().Be(FirstFile);
        accessor.DiffText.GetText().Should().Contain("+worktree first");
        _repository.CreateRepoFile("folder", "first.txt", "refreshed worktree\n");

        TaskCompletionSource selectionChanged = new(TaskCreationOptions.RunContinuationsAsynchronously);
        EventHandler selectionChangedHandler = (_, _) =>
        {
            if (accessor.DiffFiles.SelectedItem?.Item.Name == FirstFile)
            {
                selectionChanged.TrySetResult();
            }
        };
        accessor.DiffFiles.SelectedIndexChanged += selectionChangedHandler;
        try
        {
            control.RefreshArtificial();
            // FileStatusList's real throttled notification is outside the owner's task collection.
            await selectionChanged.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await JoinAsync(control);
        }
        finally
        {
            accessor.DiffFiles.SelectedIndexChanged -= selectionChangedHandler;
        }

        _exceptions.Should().BeEmpty();
        accessor.DiffFiles.SelectedItem.Should().NotBeNull();
        accessor.DiffFiles.SelectedItem!.Item.Name.Should().Be(FirstFile);
        accessor.DiffText.GetText().Should().Contain("+refreshed worktree").And.NotContain("+worktree first");
    }

    [AvaloniaTest]
    public async Task Clear_should_remove_the_actual_tree_selection_before_replacing_items([Values] bool fileTreeMode)
    {
        RevisionDiffControl control = CreateControl(fileTreeMode);
        SeedFiles(control, fileTreeMode);
        FileStatusList files = control.GetTestAccessor().DiffFiles;
        files.SelectFileOrFolder(RelativePath.From(FirstFile), notify: false).Should().BeTrue();
        FileStatusList.TestAccessor accessor = files.GetTestAccessor();
        TreeView renderer = fileTreeMode ? accessor.Tree : accessor.DiffTree;
        renderer.SelectedItem.Should().NotBeNull();

        files.Clear();

        renderer.SelectedItem.Should().BeNull("an old node must not survive hidden behind the cleared renderer flags");
        (renderer.SelectedItems?.Cast<object>()).Should().BeEmpty();
        files.AllItems.Should().BeEmpty();
        files.SelectedItem.Should().BeNull();
        files.SelectedItems.Should().BeEmpty();
        files.SelectedFolder.Should().BeNull();

        SeedFiles(control, fileTreeMode);
        files.SelectedItems.Should().BeEmpty();
        TaskCompletionSource selectionChanged = new(TaskCreationOptions.RunContinuationsAsynchronously);
        EventHandler selectionChangedHandler = (_, _) =>
        {
            if (files.SelectedItem?.Item.Name == FirstFile)
            {
                selectionChanged.TrySetResult();
            }
        };
        files.SelectedIndexChanged += selectionChangedHandler;
        try
        {
            files.SelectFileOrFolder(RelativePath.From(FirstFile), notify: true).Should().BeTrue();
            await selectionChanged.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await JoinAsync(control);
        }
        finally
        {
            files.SelectedIndexChanged -= selectionChangedHandler;
        }

        _exceptions.Should().BeEmpty();
        files.SelectedItem.Should().NotBeNull();
        files.SelectedItem!.Item.Name.Should().Be(FirstFile);
        control.GetTestAccessor().DiffText.GetText().Should().Contain("changed first");
    }

    [AvaloniaTest]
    public async Task CancelBackgroundTasks_should_join_queued_selection_without_repopulating_the_view()
    {
        RevisionDiffControl control = CreateControl(fileTreeMode: false);
        SeedFiles(control, fileTreeMode: false);
        RevisionDiffControl.TestAccessor accessor = control.GetTestAccessor();
        accessor.DiffFiles.SelectFileOrFolder(RelativePath.From(FirstFile), notify: false).Should().BeTrue();
        accessor.ShowSelectedFile();

        control.CancelBackgroundTasks();
        await JoinAsync(control);

        _exceptions.Should().BeEmpty();
        accessor.DiffFiles.AllItems.Should().BeEmpty();
        accessor.DiffFiles.SelectedItems.Should().BeEmpty();
        accessor.DiffText.GetText().Should().BeEmpty();
    }

    [AvaloniaTest]
    public void ViewPatchAsync_should_finish_during_a_synchronous_owner_join()
    {
        RevisionDiffControl control = CreateControl(fileTreeMode: false);
        RevisionDiffControl.TestAccessor accessor = control.GetTestAccessor();
        FileStatusItem item = CreateFileItem(FirstFile);
        bool startedOnWorker = false;
        bool completed = false;
        // The headless UI itself can be a pool thread, where await TaskScheduler.Default
        // completes synchronously. Enter from a distinct worker before queuing the real owner task.
        _context.Factory.Run(async () => await Task.Run(() => accessor.TaskManager.FileAndForget(async () =>
        {
            await TaskScheduler.Default;
            startedOnWorker = !Dispatcher.UIThread.CheckAccess();
            await accessor.DiffText.ViewPatchAsync(item, SamplePatch, line: null, openWithDifftool: null);
            completed = true;
        })));

        using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(10));
        _context.Factory.Run(() => accessor.TaskManager.JoinPendingOperationsAsync(cancellation.Token));

        cancellation.IsCancellationRequested.Should().BeFalse("the owner's join must pump the editor's owner-factory UI continuation");
        _exceptions.Should().BeEmpty();
        startedOnWorker.Should().BeTrue();
        completed.Should().BeTrue();
        accessor.DiffText.GetText().Should().Contain("+changed first");
    }

    private void Application_ThreadException(object? sender, ThreadExceptionEventArgs e)
        => _exceptions.Enqueue(e.Exception);

    private RevisionDiffControl CreateControl(bool fileTreeMode, IReadOnlyList<GitRevision>? selectedRevisions = null)
    {
        GitUICommands commands = new(_serviceContainer, _repository.Module);
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        source.UICommands.Returns(commands);
        RevisionDiffControl control = new() { UICommandsSource = source };
        _controls.Add(control);
        RevisionDiffControl? fileTree = null;
        if (!fileTreeMode)
        {
            fileTree = new RevisionDiffControl { UICommandsSource = source };
            _controls.Add(fileTree);
        }

        IRevisionGridInfo grid = Substitute.For<IRevisionGridInfo>();
        grid.CurrentCheckout.Returns(_head.ObjectId);
        grid.GetActualRevision(_head.ObjectId).Returns(_head);
        grid.GetActualRevision(Arg.Any<GitRevision>()).Returns(call => call.Arg<GitRevision>());
        grid.GetRevision(_head.ObjectId).Returns(_head);
        grid.GetRevision(_parent.ObjectId).Returns(_parent);
        grid.DescribeRevision(Arg.Any<GitRevision>(), Arg.Any<int>()).Returns(call => call.Arg<GitRevision>().ObjectId.ToShortString());
        grid.GetSelectedRevisions().Returns(selectedRevisions ?? [_head]);
        control.Bind(grid, Substitute.For<IRevisionGridUpdate>(), fileTree, pathFilter: null, refreshGitStatus: null);
        control.GetTestAccessor().DiffFiles.SelectFirstItemOnSetItems = false;
        control.GetTestAccessor().DiffFiles.UICommandsSource = source;
        control.GetTestAccessor().DiffText.UICommandsSource = source;
        control.GetTestAccessor().BlameControl.UICommandsSource = source;
        Window owner = new() { Width = 800, Height = 600, Content = control };
        _windows.Add(owner);
        owner.Show();
        return control;
    }

    private FileStatusItem CreateFileItem(string path)
        => new(_parent, _head, new GitItemStatus(path)
        {
            IsTracked = true,
            IsChanged = true,
            TreeId = _repository.Module.GetFileBlobHash(path, _head.ObjectId),
        });

    private async Task JoinAsync(RevisionDiffControl control)
    {
        using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(10));
        await control.GetTestAccessor().TaskManager.JoinPendingOperationsAsync(cancellation.Token);
        cancellation.IsCancellationRequested.Should().BeFalse("the real control's background operation must complete");
        Dispatcher.UIThread.VerifyAccess();
    }

    private void SeedFiles(RevisionDiffControl control, bool fileTreeMode)
        => control.GetTestAccessor().DiffFiles.SetDiffs(
            [new FileStatusWithDescription(_parent, _head, "changed files", [CreateFileItem(FirstFile).Item, CreateFileItem(SecondFile).Item])],
            fileTreeMode);
}
