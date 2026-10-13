using System.ComponentModel.Design;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using IHotkeySettingsLoader = ResourceManager.IHotkeySettingsLoader;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class RevisionGridSelectionTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;
    private bool _showArtificialCommits;
    private bool _showStashes;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        _showArtificialCommits = AppSettings.RevisionGraphShowArtificialCommits;
        _showStashes = AppSettings.ShowStashes;
        AppSettings.RevisionGraphShowArtificialCommits = true;
        AppSettings.ShowStashes = false;

        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        _serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
        _serviceContainer.AddService<IAppTitleGenerator>(new AppTitleGenerator(repositoryDescriptionProvider));
        _serviceContainer.AddService<ILinkFactory>(new LinkFactory());
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.RevisionSelection.Tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.RevisionGraphShowArtificialCommits = _showArtificialCommits;
        AppSettings.ShowStashes = _showStashes;
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task FormBrowse_should_initially_select_HEAD_and_load_its_details_regardless_of_worktree_status(
        bool dirty,
        bool showArtificial)
    {
        AppSettings.RevisionGraphShowArtificialCommits = showArtificial;
        GitModule module = CreateRepository();
        ObjectId head = module.GetCurrentCheckout();
        if (dirty)
        {
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "dirty");
        }

        using FormBrowse form = new(new GitUICommands(_serviceContainer, module));
        form.Show();
        try
        {
            await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision?.ObjectId == head
                && form.RevisionInfo.Revision?.ObjectId == head);

            form.RevisionGrid.GetSelectedRevisions().Should().ContainSingle()
                .Which.ObjectId.Should().Be(head);
            form.RevisionGrid.SelectedId.Should().Be(default(ObjectId));
            form.RevisionGrid.SelectedRevision?.Refs.Should().Contain(gitRef => gitRef.IsSelected);
            form.RevisionGrid.GetTestAccessor().Revisions.IsKeyboardFocusWithin.Should().BeTrue();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task ReloadRevisions_should_preserve_an_explicit_requested_revision_over_HEAD(bool artificial)
    {
        GitModule module = CreateRepository();
        ObjectId requestedId = artificial ? ObjectId.WorkTreeId : module.GetCurrentCheckout();
        AddCommit(module, "second");
        RevisionGridControl control = CreateControl(module);
        Window window = new() { Content = control, Width = 800, Height = 300 };
        window.Show();
        try
        {
            control.SelectedId = requestedId;
            control.SelectedId.Should().Be(requestedId);
            await ReloadAsync(control, module);

            control.SelectedRevision?.ObjectId.Should().Be(requestedId);
            control.SelectedId.Should().Be(default(ObjectId));
            control.GetTestAccessor().PendingSelectedObjectIds.Should().BeEmpty();
        }
        finally
        {
            control.CancelBackgroundTasks();
            window.Close();
        }
    }

    [AvaloniaTest]
    public async Task ReloadRevisions_should_restore_all_selected_rows_when_HEAD_has_not_changed()
    {
        GitModule module = CreateRepository();
        ObjectId first = module.GetCurrentCheckout();
        AddCommit(module, "second");
        RevisionGridControl control = CreateControl(module);
        try
        {
            await ReloadAsync(control, module);
            control.SetSelectedRevision(first).Should().BeTrue();
            control.SetSelectedRevision(ObjectId.WorkTreeId, toggleSelection: true).Should().BeTrue();

            await ReloadAsync(control, module);

            control.GetSelectedRevisions().Select(revision => revision.ObjectId)
                .Should().Equal(ObjectId.WorkTreeId, first);
            control.SelectedRevision?.ObjectId.Should().Be(ObjectId.WorkTreeId);
            control.LatestSelectedRevision?.ObjectId.Should().Be(ObjectId.WorkTreeId);
            (control.GetTestAccessor().Revisions.SelectedItem as GitRevision)?.ObjectId.Should().Be(first);
        }
        finally
        {
            control.CancelBackgroundTasks();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task FormBrowse_should_keep_the_last_diff_endpoint_in_details_and_the_first_endpoint_current_on_reload(bool reverseEndpoints)
    {
        GitModule module = CreateRepository();
        ObjectId oldest = module.GetCurrentCheckout();
        AddCommit(module, "second");
        AddCommit(module, "third");
        ObjectId newest = module.GetCurrentCheckout();
        ObjectId firstId = reverseEndpoints ? newest : oldest;
        ObjectId selectedId = reverseEndpoints ? oldest : newest;
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module),
            new BrowseArguments { FirstId = firstId, SelectedId = selectedId });
        form.Show();
        try
        {
            await WaitUntilAsync(() => form.RevisionGrid.GetSelectedRevisions().Count == 2
                && form.RevisionInfo.Revision?.ObjectId == selectedId);
            AssertEndpoints(firstId, selectedId);

            await ReloadAsync(form.RevisionGrid, module);
            await WaitUntilAsync(() => form.RevisionInfo.Revision?.ObjectId == selectedId);
            AssertEndpoints(firstId, selectedId);

            form.RevisionGrid.SetSelectedRevision(selectedId).Should().BeTrue();
            form.RevisionGrid.SetSelectedRevision(firstId, toggleSelection: true).Should().BeTrue();
            await WaitUntilAsync(() => form.RevisionInfo.Revision?.ObjectId == firstId);
            AssertEndpoints(selectedId, firstId);

            await ReloadAsync(form.RevisionGrid, module);
            await WaitUntilAsync(() => form.RevisionInfo.Revision?.ObjectId == firstId);
            AssertEndpoints(selectedId, firstId);
        }
        finally
        {
            form.Close();
        }

        void AssertEndpoints(ObjectId expectedCurrent, ObjectId expectedDetails)
        {
            form.RevisionGrid.GetSelectedRevisions().Select(revision => revision.ObjectId)
                .Should().Equal(expectedDetails, expectedCurrent);
            form.RevisionGrid.GetSelectedRevisionOrDefault()?.ObjectId.Should().Be(expectedDetails);
            form.RevisionGrid.SelectedRevision?.ObjectId.Should().Be(expectedDetails);
            form.GetLatestSelectedRevision()?.ObjectId.Should().Be(expectedDetails);
            form.RevisionInfo.Revision?.ObjectId.Should().Be(expectedDetails);
            (form.RevisionGrid.GetTestAccessor().Revisions.SelectedItem as GitRevision)?.ObjectId.Should().Be(expectedCurrent);
            form.RevisionGrid.GetTestAccessor().Revisions.SelectedItems!.OfType<GitRevision>()
                .Select(revision => revision.ObjectId).Should().Equal(expectedCurrent, expectedDetails);
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void GetSelectedRevisions_should_distinguish_selection_order_from_explicit_graph_row_sorting(bool reverseSelection)
    {
        GitRevision[] revisions = [Revision(1), Revision(2), Revision(3)];
        RevisionGridControl control = CreatePausedControl(revisions[0].ObjectId, out _);
        control.GetTestAccessor().SetRevisions(revisions);
        GitRevision first = reverseSelection ? revisions[2] : revisions[0];
        GitRevision last = reverseSelection ? revisions[0] : revisions[2];
        control.SetSelectedRevision(first.ObjectId).Should().BeTrue();
        control.SetSelectedRevision(last.ObjectId, toggleSelection: true).Should().BeTrue();

        control.GetSelectedRevisions().Should().Equal(last, first);
        control.GetSelectedRevisions(SortDirection.Ascending).Should().Equal(revisions[0], revisions[2]);
        control.GetSelectedRevisions(SortDirection.Descending).Should().Equal(revisions[2], revisions[0]);
        control.GetSelectedRevisionOrDefault().Should().BeSameAs(last);
        control.LatestSelectedRevision.Should().BeSameAs(last);
        control.GetTestAccessor().Revisions.SelectedItem.Should().BeSameAs(first);
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task ReloadRevisions_should_select_changed_HEAD_unless_an_explicit_request_supersedes_it(bool explicitRequest)
    {
        GitModule module = CreateRepository();
        ObjectId first = module.GetCurrentCheckout();
        RevisionGridControl control = CreateControl(module);
        try
        {
            await ReloadAsync(control, module);
            control.SetSelectedRevision(ObjectId.WorkTreeId).Should().BeTrue();
            AddCommit(module, "second");
            ObjectId second = module.GetCurrentCheckout();

            await ReloadAsync(control, module, selectedObjectId: explicitRequest ? first : default);

            control.GetSelectedRevisions().Should().ContainSingle()
                .Which.ObjectId.Should().Be(explicitRequest ? first : second);
        }
        finally
        {
            control.CancelBackgroundTasks();
        }
    }

    [AvaloniaTest]
    public async Task ReloadRevisions_should_select_an_available_ancestor_when_the_requested_revision_is_filtered_out()
    {
        GitModule module = CreateRepository();
        ObjectId first = module.GetCurrentCheckout();
        AddCommit(module, "second");
        ObjectId second = module.GetCurrentCheckout();
        AddCommit(module, "third");
        RevisionGridControl control = CreateControl(module);
        try
        {
            await ReloadAsync(control, module, revisionFilter: first.ToString(), selectedObjectId: second);

            control.SelectedRevision?.ObjectId.Should().Be(first);
            control.SelectedRevision?.IsArtificial.Should().BeFalse();
        }
        finally
        {
            control.CancelBackgroundTasks();
        }
    }

    [AvaloniaTest]
    public async Task ReloadRevisions_should_fall_back_to_the_first_real_row_when_no_requested_ancestor_is_available()
    {
        GitModule module = CreateRepository();
        ObjectId first = module.GetCurrentCheckout();
        RevisionGridControl control = CreateControl(module);
        try
        {
            await ReloadAsync(control, module, selectedObjectId: ObjectId.Parse(new string('a', 40)));

            control.SelectedRevision?.ObjectId.Should().Be(first);
            control.GetTestAccessor().PendingSelectedObjectIds.Should().BeEmpty();
        }
        finally
        {
            control.CancelBackgroundTasks();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task ReloadRevisions_should_handle_an_unborn_HEAD_without_an_invented_commit_selection(bool showArtificial)
    {
        AppSettings.RevisionGraphShowArtificialCommits = showArtificial;
        GitModule module = CreateRepository(createCommit: false);
        RevisionGridControl control = CreateControl(module);
        try
        {
            await ReloadAsync(control, module);

            control.CurrentCheckout.IsZero.Should().BeTrue();
            if (showArtificial)
            {
                control.SelectedRevision?.ObjectId.Should().Be(ObjectId.WorkTreeId);
            }
            else
            {
                control.SelectedRevision.Should().BeNull();
                control.GetTestAccessor().CurrentPage.Should().BeOfType<EmptyRepoControl>();
            }
        }
        finally
        {
            control.CancelBackgroundTasks();
        }
    }

    [AvaloniaTest]
    public void AppendRevisions_should_wait_for_both_explicit_diff_endpoints_in_separate_batches()
    {
        GitRevision first = Revision(1);
        GitRevision selected = Revision(2);
        RevisionGridControl control = CreatePausedControl(selected.ObjectId, out IGitModule module);
        try
        {
            control.FirstId = first.ObjectId;
            control.SelectedId = selected.ObjectId;
            StartControlledLoad(control, module);
            control.GetTestAccessor().PendingSelectedObjectIds.Should().Equal(first.ObjectId, selected.ObjectId);

            control.GetTestAccessor().ReadRevisions([first]);
            control.SelectedRevision.Should().BeNull();
            control.GetTestAccessor().PendingSelectedObjectIds.Should().Equal(first.ObjectId, selected.ObjectId);

            control.GetTestAccessor().ReadRevisions([selected]);
            control.GetSelectedRevisions().Select(revision => revision.ObjectId)
                .Should().Equal(selected.ObjectId, first.ObjectId);
            control.GetSelectedRevisionOrDefault().Should().BeSameAs(selected);
            control.LatestSelectedRevision.Should().BeSameAs(selected);
            control.GetTestAccessor().Revisions.SelectedItem.Should().BeSameAs(first);
            control.GetTestAccessor().PendingSelectedObjectIds.Should().BeEmpty();
        }
        finally
        {
            control.CancelBackgroundTasks();
        }
    }

    [AvaloniaTest]
    public void ReloadRevisions_should_retain_unloaded_diff_endpoints_until_an_explicit_request_supersedes_them()
    {
        GitRevision first = Revision(1);
        GitRevision selected = Revision(2);
        RevisionGridControl control = CreatePausedControl(selected.ObjectId, out IGitModule module);
        try
        {
            control.FirstId = first.ObjectId;
            control.SelectedId = selected.ObjectId;
            StartControlledLoad(control, module);
            StartControlledLoad(control, module);

            control.GetTestAccessor().PendingSelectedObjectIds.Should().Equal(first.ObjectId, selected.ObjectId);

            control.SelectedId = first.ObjectId;
            StartControlledLoad(control, module);

            control.GetTestAccessor().PendingSelectedObjectIds.Should().Equal(first.ObjectId);
            control.GetTestAccessor().ReadRevisions([first, selected]);
            control.GetSelectedRevisions().Should().ContainSingle().Which.Should().BeSameAs(first);
        }
        finally
        {
            control.CancelBackgroundTasks();
        }
    }

    [AvaloniaTest]
    public void OnLoadingCompleted_should_settle_with_the_available_requested_endpoint()
    {
        GitRevision first = Revision(1);
        GitRevision selected = Revision(2);
        RevisionGridControl control = CreatePausedControl(selected.ObjectId, out IGitModule module);
        try
        {
            control.FirstId = first.ObjectId;
            control.SelectedId = selected.ObjectId;
            StartControlledLoad(control, module);
            control.GetTestAccessor().ReadRevisions([first]);

            control.GetTestAccessor().CompleteLoading();

            control.GetSelectedRevisions().Should().ContainSingle().Which.Should().BeSameAs(first);
            control.GetTestAccessor().PendingSelectedObjectIds.Should().BeEmpty();
        }
        finally
        {
            control.CancelBackgroundTasks();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void AppendRevisions_should_not_overwrite_user_selection_or_take_focus_after_it_leaves_the_grid(bool programmatic)
    {
        GitRevision first = Revision(1);
        GitRevision selected = Revision(2);
        RevisionGridControl control = CreatePausedControl(selected.ObjectId, out IGitModule module);
        TextBox otherControl = new();
        StackPanel host = new() { Children = { control, otherControl } };
        Window window = new() { Content = host, Width = 800, Height = 300 };
        window.Show();
        try
        {
            StartControlledLoad(control, module);
            control.GetTestAccessor().ReadRevisions([first]);
            if (programmatic)
            {
                control.SetSelectedRevision(first.ObjectId).Should().BeTrue();
            }
            else
            {
                control.GetTestAccessor().Revisions.SelectedItem = first;
            }

            otherControl.Focus(NavigationMethod.Pointer).Should().BeTrue();

            control.GetTestAccessor().ReadRevisions([selected]);
            control.GetTestAccessor().CompleteLoading();

            control.SelectedRevision.Should().BeSameAs(first);
            otherControl.IsKeyboardFocusWithin.Should().BeTrue();
            control.GetTestAccessor().PendingSelectedObjectIds.Should().BeEmpty();
        }
        finally
        {
            control.CancelBackgroundTasks();
            window.Close();
        }
    }

    [AvaloniaTest]
    public void AppendRevisions_should_preserve_focus_moved_away_while_waiting_for_the_first_batch()
    {
        GitRevision selected = Revision(1);
        RevisionGridControl control = CreatePausedControl(selected.ObjectId, out IGitModule module);
        TextBox otherControl = new();
        Grid host = new() { RowDefinitions = new RowDefinitions("*,Auto"), Children = { control, otherControl } };
        Grid.SetRow(otherControl, 1);
        Window window = new() { Content = host, Width = 800, Height = 300 };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            StartControlledLoad(control, module);
            control.GetTestAccessor().Revisions.IsKeyboardFocusWithin.Should().BeTrue();
            otherControl.Focus(NavigationMethod.Pointer).Should().BeTrue();

            control.GetTestAccessor().ReadRevisions([selected]);
            control.GetTestAccessor().CompleteLoading();

            control.SelectedRevision.Should().BeSameAs(selected);
            otherControl.IsKeyboardFocusWithin.Should().BeTrue();
        }
        finally
        {
            control.CancelBackgroundTasks();
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task ApplyFinalRevisionOrder_should_not_publish_a_transient_empty_selection_for_retained_details(bool multipleSelection)
    {
        RevisionGridControl control = CreatePausedControl(Revision(1).ObjectId, out _);
        GitRevision[] revisions = [Revision(1), Revision(2)];
        control.GetTestAccessor().SetRevisions(revisions);
        control.SetSelectedRevision(revisions[0].ObjectId).Should().BeTrue();
        if (multipleSelection)
        {
            control.SetSelectedRevision(revisions[1].ObjectId, toggleSelection: true).Should().BeTrue();
        }

        await Task.Delay(100);
        Dispatcher.UIThread.RunJobs();
        int notifications = 0;
        control.SelectionChanged += (_, _) => notifications++;

        control.GetTestAccessor().SetRevisions(revisions.Reverse());
        await Task.Delay(100);
        Dispatcher.UIThread.RunJobs();

        GitRevision expectedDetails = multipleSelection ? revisions[1] : revisions[0];
        control.SelectedRevision.Should().BeSameAs(expectedDetails);
        control.GetSelectedRevisionOrDefault().Should().BeSameAs(expectedDetails);
        control.LatestSelectedRevision.Should().BeSameAs(expectedDetails);
        control.GetTestAccessor().Revisions.SelectedItem.Should().BeSameAs(revisions[0]);
        control.GetSelectedRevisions().Should().Equal(multipleSelection ? [revisions[1], revisions[0]] : [revisions[0]]);
        notifications.Should().Be(0);
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void TryGoToRelatedRef_should_report_actual_selection_changes_after_the_initial_request_is_consumed(bool changesSelection)
    {
        GitRevision first = Revision(1);
        GitRevision second = Revision(2);
        RevisionGridControl control = CreatePausedControl(first.ObjectId, out IGitModule module);
        control.GetTestAccessor().SetRevisions([first, second]);
        control.SetSelectedRevision(first.ObjectId).Should().BeTrue();
        ObjectId targetId = changesSelection ? second.ObjectId : first.ObjectId;
        IGitRef relatedRef = new GitRef(null!, targetId, "refs/remotes/origin/related");
        module.RevParse(targetId.ToString()).Returns(targetId);
        control.SelectedId.Should().Be(default(ObjectId));

        control.TryGoToRelatedRef(relatedRef).Should().Be(changesSelection);

        control.GetSelectedRevisionOrDefault()?.ObjectId.Should().Be(targetId);
    }

    [AvaloniaTest]
    public void Cancelled_stream_callbacks_should_not_replace_selection_or_focus_from_the_active_load()
    {
        GitRevision first = Revision(1);
        GitRevision selected = Revision(2);
        RevisionGridControl control = CreatePausedControl(selected.ObjectId, out IGitModule module);
        using CancellationTokenSource staleLoad = new();
        try
        {
            StartControlledLoad(control, module);
            control.GetTestAccessor().ReadRevisions([selected]);
            staleLoad.Cancel();

            IObserver<IReadOnlyList<GitRevision>> staleObserver = control.GetTestAccessor().CreateRevisionObserver(staleLoad.Token);
            staleObserver.OnNext([first]);
            staleObserver.OnCompleted();
            Dispatcher.UIThread.RunJobs();

            control.GetTestAccessor().Revisions.Items.OfType<GitRevision>().Should().ContainSingle()
                .Which.Should().BeSameAs(selected);
            control.SelectedRevision.Should().BeSameAs(selected);
        }
        finally
        {
            control.CancelBackgroundTasks();
        }
    }

    private GitModule CreateRepository(bool createCommit = true)
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "selection" }).Should().BeTrue();
        module.SetSetting("user.name", "Selection Test");
        module.SetSetting("user.email", "selection@example.com");
        if (createCommit)
        {
            AddCommit(module, "initial");
        }

        return module;
    }

    private RevisionGridControl CreateControl(GitModule module)
    {
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        source.UICommands.Returns(new GitUICommands(_serviceContainer, module));
        return new RevisionGridControl { UICommandsSource = source };
    }

    private void AddCommit(GitModule module, string subject)
    {
        File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), subject);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", subject }).Should().BeTrue();
    }

    private static RevisionGridControl CreatePausedControl(ObjectId head, out IGitModule module)
    {
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        module = Substitute.For<IGitModule>();
        module.GetCurrentCheckout().Returns(head);
        commands.Module.Returns(module);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());
        source.UICommands.Returns(commands);
        return new RevisionGridControl { UICommandsSource = source };
    }

    private static async Task ReloadAsync(
        RevisionGridControl control,
        IGitModule module,
        string revisionFilter = "--all",
        ObjectId selectedObjectId = default)
    {
        bool completed = false;
        EventHandler<RevisionLoadEventArgs> onLoaded = (_, _) => completed = true;
        control.RevisionsLoaded += onLoaded;
        try
        {
            control.ReloadRevisions(module, revisionFilter, selectedObjectId);
            await WaitUntilAsync(() => completed);
        }
        finally
        {
            control.RevisionsLoaded -= onLoaded;
        }
    }

    private static void StartControlledLoad(RevisionGridControl control, IGitModule module)
    {
        // Deliver deterministic stream callbacks without blocking a pool-backed headless UI context.
        // TaskManager handles the interrupted provider; real Git reads are covered separately above.
        control.ReloadRevisions(module, getRefs: _ => throw new OperationCanceledException());
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        condition().Should().BeTrue("the owned revision load should complete before the timeout");
    }

    private static GitRevision Revision(int index)
        => new(ObjectId.Parse(index.ToString("x40"))) { Subject = $"Revision {index}", Author = "Selection Test" };
}
