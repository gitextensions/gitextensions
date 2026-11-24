using System.Collections.Concurrent;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.NBugReports;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using IHotkeySettingsLoader = ResourceManager.IHotkeySettingsLoader;
using WinFormsShims = GitExtensions.Shims.WinForms;

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
    private bool _showReflogReferences;

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task ReloadRevisions_should_read_log_while_metadata_is_pending_and_wait_before_inserting_rows(bool delayRefs)
    {
        using AppSettingsTestScope settings = new();
        AppSettings.ShowStashes = true;
        AppSettings.RevisionGraphShowArtificialCommits = false;
        GitModule repository = CreateRepository();
        ObjectId head = repository.GetCurrentCheckout();
        File.WriteAllText(Path.Combine(_workingDirectory, "untracked.txt"), "stash content");
        repository.GitExecutable.RunCommand(new GitArgumentBuilder("stash") { "push", "--quiet", "--include-untracked" }).Should().BeTrue();

        TaskCompletionSource metadataStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource logStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource loaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ManualResetEventSlim releaseMetadata = new();
        using CancellationTokenSource deadline = new();
        ConcurrentQueue<string> operations = new();
        IGitExecutor originalExecutor = _serviceContainer.GetRequiredService<IGitExecutorProvider>().GetExecutor(_workingDirectory);
        IGitCommandRunner runner = Substitute.For<IGitCommandRunner>();
        runner.RunDetached(Arg.Any<CancellationToken>(), Arg.Any<ArgumentString>(), Arg.Any<bool>(), Arg.Any<bool>(),
                Arg.Any<bool>(), Arg.Any<Encoding?>(), Arg.Any<bool>())
            .Returns(call =>
            {
                ArgumentString arguments = call.ArgAt<ArgumentString>(1);
                string command = arguments.ToString();
                operations.Enqueue($"{Environment.CurrentManagedThreadId}: {command}");
                bool isLog = command.StartsWith("log ", StringComparison.Ordinal) || command.Contains(" log ", StringComparison.Ordinal);
                if (!delayRefs && isLog
                    && command.Contains("--dirstat=files", StringComparison.Ordinal))
                {
                    metadataStarted.TrySetResult();
                    releaseMetadata.Wait(deadline.Token);
                }
                else if (isLog
                    && !command.Contains("--dirstat=files", StringComparison.Ordinal))
                {
                    logStarted.TrySetResult();
                }

                return originalExecutor.GitCommandRunner.RunDetached(call.ArgAt<CancellationToken>(0), arguments,
                    call.ArgAt<bool>(2), call.ArgAt<bool>(3), call.ArgAt<bool>(4), call.ArgAt<Encoding?>(5), call.ArgAt<bool>(6));
            });
        IGitExecutor executor = Substitute.For<IGitExecutor>();
        executor.WorkingDir.Returns(originalExecutor.WorkingDir);
        executor.GetGitDirectory().Returns(originalExecutor.GetGitDirectory());
        executor.GitExecutable.Returns(originalExecutor.GitExecutable);
        executor.GitCommandRunner.Returns(runner);
        executor.GitWindowsExecutable.Returns(originalExecutor.GitWindowsExecutable);
        executor.GitWindowsCommandRunner.Returns(originalExecutor.GitWindowsCommandRunner);
        executor.WslDistro.Returns(originalExecutor.WslDistro);
        IGitExecutorProvider provider = Substitute.For<IGitExecutorProvider>();
        provider.GetExecutor(Arg.Any<string>()).Returns(executor);
        GitModule module = new(provider, _workingDirectory);
        RevisionGridControl control = CreateControl(module);
        control.RevisionsLoaded += (_, _) => loaded.TrySetResult();
        try
        {
            deadline.CancelAfter(TimeSpan.FromSeconds(15));
            operations.Enqueue($"UI: {Environment.CurrentManagedThreadId}; stashes={AppSettings.ShowStashes}; reflog={AppSettings.ShowReflogReferences.Value}");
            control.ReloadRevisions(module, getRefs: filter =>
            {
                operations.Enqueue($"refs: {Environment.CurrentManagedThreadId}");
                if (delayRefs)
                {
                    metadataStarted.TrySetResult();
                    releaseMetadata.Wait(deadline.Token);
                }

                return repository.GetRefs(filter);
            });
            await metadataStarted.Task.WaitAsync(deadline.Token);
            await logStarted.Task.WaitAsync(deadline.Token);
            control._gridView.ItemCount.Should().Be(0, "the original gates insertion on refs and stash preparation");
            control.GetTestAccessor().IsDataLoadComplete.Should().BeFalse();
            releaseMetadata.Set();
            await loaded.Task.WaitAsync(deadline.Token);
            control.GetTestAccessor().IsDataLoadComplete.Should().BeTrue();
            control.SelectedRevision.Should().NotBeNull();
            control._gridView.Items.OfType<GitRevision>().Should().Contain(revision => revision.ObjectId == head);
            control._gridView.Items.OfType<GitRevision>().Single(revision => revision.ObjectId == head)
                .Refs.Should().Contain(gitRef => gitRef.Name == "selection");
            control._gridView.Items.OfType<GitRevision>().Should().Contain(revision => revision.ReflogSelector == "refs/stash@{0}");
        }
        finally
        {
            releaseMetadata.Set();
            control.CancelBackgroundTasks();
            await TestContext.Out.WriteLineAsync(string.Join(Environment.NewLine, operations));
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task Ref_hover_should_remain_usable_after_grid_reattachment(bool reattach)
    {
        using AppSettingsTestScope settings = new();
        AppSettings.RevisionGraphShowArtificialCommits = false;
        GitModule module = CreateRepository();
        AddCommit(module, "second");
        RevisionGridControl control = CreateControl(module);
        Window window = new() { Content = control, Width = 1000, Height = 300 };
        window.Show();
        try
        {
            await ReloadAsync(control, module);
            if (reattach)
            {
                window.Content = null;
                window.Content = control;
                await ReloadAsync(control, module);
            }

            Settle(window);
            RevisionGridRefRenderer.RefLabelControl label = control._gridView.GetVisualDescendants()
                .OfType<RevisionGridRefRenderer.RefLabelControl>().Single(candidate => candidate.GitRef?.Name == "selection");
            Point point = label.TranslatePoint(new Point(label.Bounds.Width / 2, label.Bounds.Height / 2), window)!.Value;
            (window.InputHitTest(point) as Visual)?.GetSelfAndVisualAncestors().Should().Contain(label);
            window.MouseMove(point);
            Settle(window);
            RevisionGraphColumnProvider provider = control.ColumnProviders
                .OfType<RevisionGraphColumnProvider>().Single();
            HoverHighlightCalculator calculator = (HoverHighlightCalculator)typeof(RevisionGraphColumnProvider)
                .GetField("_hoverHighlight", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(provider)!;
            ObjectId head = module.GetCurrentCheckout();
            await WaitUntilAsync(() => calculator.HighlightedIds?.Contains(head) == true);
            control._gridView.GetVisualDescendants().OfType<RevisionGridRefRenderer.RefLabelControl>()
                .Single(candidate => candidate.GitRef?.Name == "selection").IsHighlighted.Should().BeTrue();
        }
        finally
        {
            control.CancelBackgroundTasks();
            window.Close();
            ((IDisposable)control).Dispose();
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_close_should_dispose_its_revision_grid_resources()
    {
        using AppSettingsTestScope settings = new();
        GitModule module = CreateRepository();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module), new BrowseArguments());
        form.Show();
        try
        {
            await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision?.ObjectId == module.GetCurrentCheckout());
            RevisionGraphColumnProvider provider = form.RevisionGrid.ColumnProviders.OfType<RevisionGraphColumnProvider>().Single();
            GitRevision revision = form.RevisionGrid.SelectedRevision!;
            IGitRef branch = revision.Refs.Single(gitRef => gitRef.Name == "selection");
            form.Close();

            Func<Task> hoverAfterClose = () => provider.SetHoverHighlightAsync(branch, 0);
            await hoverAfterClose.Should().ThrowAsync<OperationCanceledException>();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public async Task ReloadRevisions_should_use_the_original_translated_autostash_caption()
    {
        using AppSettingsTestScope settings = new();
        AppSettings.ShowStashes = true;
        AppSettings.RevisionGraphShowArtificialCommits = false;
        GitModule module = CreateRepository();
        AddCommit(module, "second");
        File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "pending change");
        ProcessStartInfo startInfo = new(AppSettings.GitCommand)
        {
            WorkingDirectory = module.WorkingDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (string argument in new[] { "rebase", "--autostash", "--exec", "exit 1", "HEAD~1" })
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process rebase = new() { StartInfo = startInfo };
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(30));
        rebase.Start().Should().BeTrue();
        try
        {
            Task<string> output = rebase.StandardOutput.ReadToEndAsync(timeout.Token);
            Task<string> error = rebase.StandardError.ReadToEndAsync(timeout.Token);
            await rebase.WaitForExitAsync(timeout.Token);
            await Task.WhenAll(output, error);
            rebase.ExitCode.Should().Be(1);
            File.Exists(Path.Join(module.WorkingDirGitDir, "rebase-merge", "autostash")).Should().BeTrue();
            RevisionGridControl control = CreateControl(module);
            try
            {
                await ReloadAsync(control, module);

                control._gridView.Items.OfType<GitRevision>().Should().ContainSingle(revision => revision.IsAutostash)
                    .Which.Subject.Should().Be(ResourceManager.TranslatedStrings.Autostash);
            }
            finally
            {
                ((IDisposable)control).Dispose();
            }
        }
        finally
        {
            if (!rebase.HasExited)
            {
                rebase.Kill(entireProcessTree: true);
                await rebase.WaitForExitAsync();
            }
        }
    }

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        _showArtificialCommits = AppSettings.RevisionGraphShowArtificialCommits;
        _showStashes = AppSettings.ShowStashes;
        _showReflogReferences = AppSettings.ShowReflogReferences.Value;
        AppSettings.ShowReflogReferences.Value = false;
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
        AppSettings.ShowReflogReferences.Value = _showReflogReferences;
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public async Task ReloadRevisions_should_report_reader_errors_through_the_application_error_path()
    {
        const string missingRevision = "refs/heads/parity-revision-that-does-not-exist";
        GitModule module = CreateRepository();
        var previousPresenter = UIReporter.TestAccessor.TaskDialogPresenter;
        List<TaskDialogPage> reports = [];
        UIReporter.TestAccessor.TaskDialogPresenter = (_, page) =>
        {
            Dispatcher.UIThread.CheckAccess().Should().BeTrue();
            reports.Add(page);
            TaskDialogButton ignore = page.Buttons.Single(button => button.Text == GitUI.TranslatedStrings.ButtonIgnore);
            ignore.PerformClick();
            return ignore;
        };
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module), new BrowseArguments());
        try
        {
            form.Show();
            await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision?.ObjectId == module.GetCurrentCheckout());

            form.RevisionGrid.ReloadRevisions(module, getRefs: _ => throw new IOException($"Cannot read {missingRevision}"));
            await WaitUntilAsync(() => reports.Count == 1);

            reports.Should().ContainSingle();
            (reports[0].Heading + reports[0].Text).Should().Contain(missingRevision);
            form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!.Text.Should().Contain("Failed to load revisions");
            form.IsVisible.Should().BeTrue();

            form.RevisionGrid.ReloadRevisions(module);
            await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision?.ObjectId == module.GetCurrentCheckout());
            reports.Should().ContainSingle();
        }
        finally
        {
            form.Close();
            UIReporter.TestAccessor.TaskDialogPresenter = previousPresenter;
        }
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

        using FormBrowse form = new(new GitUICommands(_serviceContainer, module), new BrowseArguments());
        form.Show();
        try
        {
            await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision?.ObjectId == head
                && form.RevisionInfo.Revision?.ObjectId == head);

            form.RevisionGrid.GetSelectedRevisions().Should().ContainSingle()
                .Which.ObjectId.Should().Be(head);
            form.RevisionGrid.SelectedId.Should().Be(default(ObjectId));
            form.RevisionGrid.SelectedRevision?.Refs.Should().Contain(gitRef => gitRef.IsSelected);
            form.RevisionGrid._gridView.IsKeyboardFocusWithin.Should().BeTrue();
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
            control.GetNativeTestAccessor().PendingSelectedObjectIds.Should().BeEmpty();
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
            (control._gridView.SelectedItem as GitRevision)?.ObjectId.Should().Be(first);
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
            (form.RevisionGrid._gridView.SelectedItem as GitRevision)?.ObjectId.Should().Be(expectedCurrent);
            form.RevisionGrid._gridView.SelectedItems!.OfType<GitRevision>()
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
        control.GetNativeTestAccessor().SetRevisions(revisions);
        GitRevision first = reverseSelection ? revisions[2] : revisions[0];
        GitRevision last = reverseSelection ? revisions[0] : revisions[2];
        control.SetSelectedRevision(first.ObjectId).Should().BeTrue();
        control.SetSelectedRevision(last.ObjectId, toggleSelection: true).Should().BeTrue();

        control.GetSelectedRevisions().Should().Equal(last, first);
        control.GetSelectedRevisions(SortDirection.Ascending).Should().Equal(revisions[0], revisions[2]);
        control.GetSelectedRevisions(SortDirection.Descending).Should().Equal(revisions[2], revisions[0]);
        control.GetSelectedRevisionOrDefault().Should().BeSameAs(last);
        control.LatestSelectedRevision.Should().BeSameAs(last);
        control._gridView.SelectedItem.Should().BeSameAs(first);
    }

    [AvaloniaTest]
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void Right_click_should_preserve_only_the_selected_latest_row_and_retain_selection_in_empty_space(int clickedIndex)
    {
        GitRevision[] revisions = [Revision(1), Revision(2), Revision(3)];
        RevisionGridControl control = CreatePausedControl(revisions[0].ObjectId, out _);
        control.GetNativeTestAccessor().SetRevisions(revisions);
        control.DisableContextMenu();
        Window window = new() { Width = 900, Height = 240, Content = control };
        window.Show();
        try
        {
            control.SetSelectedRevision(revisions[0].ObjectId).Should().BeTrue();
            control.SetSelectedRevision(revisions[2].ObjectId, toggleSelection: true).Should().BeTrue();
            Settle(window);
            Point point = clickedIndex < 0
                ? control._gridView.TranslatePoint(new Point(100, 180), window)!.Value
                : RowPoint(control, window, clickedIndex);
            window.MouseMove(point);
            window.MouseDown(point, MouseButton.Right);
            window.MouseUp(point, MouseButton.Right);
            Settle(window);

            if (clickedIndex == 2)
            {
                control.GetSelectedRevisions().Should().Equal(revisions[2], revisions[0]);
                control._gridView.SelectedItem.Should().BeSameAs(revisions[0]);
            }
            else if (clickedIndex >= 0)
            {
                control.GetSelectedRevisions().Should().Equal(revisions[clickedIndex]);
                control._gridView.SelectedItem.Should().BeSameAs(revisions[clickedIndex]);
            }
            else
            {
                control.GetSelectedRevisions().Should().Equal(revisions[2], revisions[0]);
                control.GetSelectedRevisionOrDefault().Should().BeSameAs(revisions[2]);
                control._gridView.SelectedItem.Should().BeSameAs(revisions[0]);
            }

            control.LatestSelectedRevision.Should().BeSameAs(clickedIndex >= 0 ? revisions[clickedIndex] : null);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Right_click_should_preserve_the_keyboard_caret_when_it_selects_a_different_row()
    {
        GitRevision[] revisions = [Revision(1), Revision(2), Revision(3)];
        RevisionGridControl control = CreatePausedControl(revisions[0].ObjectId, out _);
        control.GetNativeTestAccessor().SetRevisions(revisions);
        control.DisableContextMenu();
        Window window = new() { Width = 900, Height = 240, Content = control };
        window.Show();
        try
        {
            DeselectLatestRow(control, window, revisions);
            IInputElement? caret = window.FocusManager?.GetFocusedElement();
            caret.Should().BeSameAs(control._gridView.ContainerFromIndex(2));
            Point point = RowPoint(control, window, 0);
            window.MouseDown(point, MouseButton.Right);
            window.MouseUp(point, MouseButton.Right);
            Settle(window);

            control.GetSelectedRevisions().Should().Equal(revisions[0]);
            window.FocusManager!.GetFocusedElement().Should().BeSameAs(caret);
            window.KeyPress(Key.Up, RawInputModifiers.None, PhysicalKey.ArrowUp, keySymbol: null);
            Settle(window);

            control.GetSelectedRevisions().Should().Equal(revisions[1]);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Empty_mouse_down_should_cancel_pending_load_selection_without_selecting_a_revision()
    {
        GitRevision first = Revision(1);
        GitRevision requested = Revision(2);
        RevisionGridControl control = CreatePausedControl(requested.ObjectId, out IGitModule module);
        control.DisableContextMenu();
        Window window = new() { Width = 900, Height = 240, Content = control };
        window.Show();
        try
        {
            StartControlledLoad(control, module);
            control.GetNativeTestAccessor().ReadRevisions([first]);
            control.GetNativeTestAccessor().PendingSelectedObjectIds.Should().Equal(requested.ObjectId);
            Settle(window);
            Point point = control._gridView.TranslatePoint(new Point(100, 180), window)!.Value;
            window.MouseDown(point, MouseButton.Right);
            window.MouseUp(point, MouseButton.Right);
            Settle(window);

            control.GetNativeTestAccessor().PendingSelectedObjectIds.Should().BeEmpty();
            control.GetSelectedRevisions().Should().BeEmpty();
            control.LatestSelectedRevision.Should().BeNull();
            control.GetNativeTestAccessor().ReadRevisions([requested]);
            control.GetSelectedRevisions().Should().BeEmpty();
        }
        finally
        {
            control.CancelBackgroundTasks();
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Keyboard_copy_should_use_restoration_order_while_default_and_copy_menu_keep_latest_selection_first()
    {
        WinFormsShims.IClipboard? previousClipboard = null;
        try
        {
            previousClipboard = WinFormsShims.ShimHost.Clipboard;
        }
        catch (InvalidOperationException)
        {
        }

        WinFormsShims.IClipboard clipboard = Substitute.For<WinFormsShims.IClipboard>();
        WinFormsShims.ShimHost.Clipboard = clipboard;
        try
        {
            GitRevision first = Revision(1);
            GitRevision last = Revision(2);
            RevisionGridControl control = CreatePausedControl(first.ObjectId, out _);
            control.GetNativeTestAccessor().SetRevisions([first, last]);
            control.SetSelectedRevision(first.ObjectId).Should().BeTrue();
            control.SetSelectedRevision(last.ObjectId, toggleSelection: true).Should().BeTrue();
            control._gridView.RaiseEvent(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Key = Key.C,
                KeyModifiers = KeyModifiers.Control,
            });

            clipboard.Received(1).SetText(string.Join(Environment.NewLine, first.ObjectId, last.ObjectId));
            control.GetSelectedRevisions().Should().Equal(last, first);
            control.GetSelectedRevisionOrDefault().Should().BeSameAs(last);
            CopyContextMenuItem menu = control.FindControl<CopyContextMenuItem>("copyToClipboardToolStripMenuItem")!;
            menu.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
            MenuItem hashes = menu.Items.OfType<MenuItem>()
                .Single(item => item.Header?.ToString()?.Contains("Commit hash", StringComparison.Ordinal) == true);
            hashes.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            clipboard.Received(1).SetText(last.Guid + "\n" + first.Guid);
        }
        finally
        {
            WinFormsShims.ShimHost.Clipboard = previousClipboard ?? Substitute.For<WinFormsShims.IClipboard>();
        }
    }

    [AvaloniaTest]
    [TestCase("createTagToolStripMenuItem")]
    [TestCase("createNewBranchToolStripMenuItem")]
    [TestCase("checkoutRevisionToolStripMenuItem")]
    [TestCase("fixupCommitToolStripMenuItem")]
    [TestCase("squashCommitToolStripMenuItem")]
    [TestCase("amendCommitToolStripMenuItem")]
    public void Single_revision_commands_should_target_the_clicked_row_after_Ctrl_deselects_it(string menuName)
    {
        GitRevision[] revisions = [Revision(1), Revision(2), Revision(3)];
        RevisionGridControl control = CreatePausedControl(revisions[0].ObjectId, out _);
        control.GetNativeTestAccessor().SetRevisions(revisions);
        control.DisableContextMenu();
        Window window = new() { Width = 900, Height = 240, Content = control };
        window.Show();
        try
        {
            DeselectLatestRow(control, window, revisions);
            MenuItem item = control.FindControl<MenuItem>(menuName)!;
            item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            switch (menuName)
            {
                case "createTagToolStripMenuItem":
                    control.UICommands.Received(1).StartCreateTagDialog(null, revisions[2]);
                    break;
                case "createNewBranchToolStripMenuItem":
                    control.UICommands.Received(1).StartCreateBranchDialog(null, revisions[2].ObjectId);
                    break;
                case "checkoutRevisionToolStripMenuItem":
                    control.UICommands.Received(1).StartCheckoutRevisionDialog(null, revisions[2].Guid);
                    break;
                case "fixupCommitToolStripMenuItem":
                    control.UICommands.Received(1).StartFixupCommitDialog(null, revisions[2]);
                    break;
                case "squashCommitToolStripMenuItem":
                    control.UICommands.Received(1).StartSquashCommitDialog(null, revisions[2]);
                    break;
                case "amendCommitToolStripMenuItem":
                    control.UICommands.Received(1).StartAmendCommitDialog(null, revisions[2]);
                    break;
            }

            control.GetSelectedRevisionOrDefault().Should().BeSameAs(revisions[0]);
            control.LatestSelectedRevision.Should().BeSameAs(revisions[2]);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Parent_and_child_navigation_should_start_from_the_clicked_row_not_the_remaining_selection(bool child)
    {
        GitRevision[] revisions = [Revision(1), Revision(2), Revision(3), Revision(4)];
        revisions[0].ParentIds = [revisions[1].ObjectId];
        revisions[1].ParentIds = [revisions[2].ObjectId];
        revisions[2].ParentIds = [revisions[3].ObjectId];
        RevisionGridControl control = CreatePausedControl(revisions[0].ObjectId, out _);
        control.GetNativeTestAccessor().SetRevisions(revisions);
        control.DisableContextMenu();
        Window window = new() { Width = 900, Height = 240, Content = control };
        window.Show();
        try
        {
            DeselectLatestRow(control, window, revisions);
            control.ExecuteCommand(child ? RevisionGridControl.Command.GoToChild : RevisionGridControl.Command.GoToFirstParent)
                .Should().BeTrue();

            control.GetSelectedRevisions().Should().Equal(child ? revisions[1] : revisions[3]);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Select_as_base_should_keep_the_default_selection_contract_after_Ctrl_deselects_the_clicked_row()
    {
        GitRevision[] revisions = [Revision(1), Revision(2), Revision(3)];
        RevisionGridControl control = CreatePausedControl(revisions[0].ObjectId, out _);
        control.GetNativeTestAccessor().SetRevisions(revisions);
        control.DisableContextMenu();
        Window window = new() { Width = 900, Height = 240, Content = control };
        window.Show();
        try
        {
            DeselectLatestRow(control, window, revisions);
            control.ExecuteCommand(RevisionGridControl.Command.SelectAsBaseToCompare).Should().BeTrue();

            typeof(RevisionGridControl).GetField("_baseCommitToCompare",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .GetValue(control).Should().BeSameAs(revisions[0]);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase("all")]
    [TestCase("filtered")]
    [TestCase("current")]
    public void Branch_mode_commands_should_not_refresh_an_already_checked_mode_or_clear_its_selection(string mode)
    {
        GitRevision[] revisions = [Revision(1), Revision(2), Revision(3)];
        RevisionGridControl control = CreatePausedControl(revisions[0].ObjectId, out _);
        control.SuspendRefreshRevisions();
        try
        {
            ApplyMode();
        }
        finally
        {
            control.ResumeRefreshRevisions();
        }

        control.GetNativeTestAccessor().SetRevisions(revisions);
        control.SetSelectedRevision(revisions[0].ObjectId).Should().BeTrue();
        control.SetSelectedRevision(revisions[2].ObjectId, toggleSelection: true).Should().BeTrue();
        int filtersChanged = 0;
        int loadsStarted = 0;
        control.FilterChanged += (_, _) => filtersChanged++;
        control.RevisionsLoading += (_, _) => loadsStarted++;

        ApplyMode();

        filtersChanged.Should().Be(0);
        loadsStarted.Should().Be(0);
        control.GetSelectedRevisions().Should().Equal(revisions[2], revisions[0]);
        control._gridView.SelectedItem.Should().BeSameAs(revisions[0]);
        return;

        void ApplyMode()
        {
            switch (mode)
            {
                case "all": control.ShowAllBranches(); break;
                case "filtered": control.ShowFilteredBranches(); break;
                case "current": control.ShowCurrentBranchOnly(); break;
            }
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Navigate_history_should_report_a_filtered_target_and_preserve_the_remaining_history(bool forward)
    {
        WinFormsShims.IMessageBoxHost? previousHost = null;
        try
        {
            previousHost = WinFormsShims.ShimHost.MessageBoxHost;
        }
        catch (InvalidOperationException)
        {
        }

        WinFormsShims.IMessageBoxHost host = Substitute.For<WinFormsShims.IMessageBoxHost>();
        WinFormsShims.ShimHost.MessageBoxHost = host;
        try
        {
            GitRevision[] revisions =
            [
                new(ObjectId.Parse(new string('a', 40))),
                new(ObjectId.Parse(new string('b', 40))),
                new(ObjectId.Parse(new string('c', 40))),
            ];
            RevisionGridControl control = CreatePausedControl(revisions[0].ObjectId, out _);
            control.GetNativeTestAccessor().SetRevisions(revisions);
            foreach (GitRevision revision in revisions)
            {
                control.SetSelectedRevision(revision.ObjectId).Should().BeTrue();
            }

            if (forward)
            {
                control.NavigateBackward();
                control.NavigateBackward();
            }

            control.GetNativeTestAccessor().SetRevisions([revisions[0], revisions[2]]);
            if (forward)
            {
                control.NavigateForward();
            }
            else
            {
                control.NavigateBackward();
            }

            control.GetSelectedRevisions().Should().Equal(forward ? revisions[0] : revisions[2]);
            host.Received(1).Show(
                Arg.Any<WinFormsShims.IWin32Window?>(),
                Arg.Is<string?>(text => text != null && text.Contains(revisions[1].ObjectId.ToShortString())),
                Arg.Is<string?>(caption => !string.IsNullOrWhiteSpace(caption)),
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error,
                WinFormsShims.MessageBoxDefaultButton.Button1);

            if (forward)
            {
                control.NavigateForward();
            }
            else
            {
                control.NavigateBackward();
            }

            control.GetSelectedRevisions().Should().Equal(forward ? revisions[2] : revisions[0]);
            host.ReceivedCalls().Should().ContainSingle();
        }
        finally
        {
            WinFormsShims.ShimHost.MessageBoxHost = previousHost ?? Substitute.For<WinFormsShims.IMessageBoxHost>();
        }
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
    public async Task Filtered_HEAD_should_place_artificial_rows_before_its_displayed_ancestor_not_an_unrelated_first_row()
    {
        GitModule module = CreateRepository();
        ObjectId initial = module.GetCurrentCheckout();
        AddCommit(module, "visible-ancestor");
        ObjectId ancestor = module.GetCurrentCheckout();
        AddCommit(module, "excluded-HEAD");
        ObjectId head = module.GetCurrentCheckout();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("checkout") { "--quiet", "-b", "unrelated", initial }).Should().BeTrue();
        Thread.Sleep(1_100);
        AddCommit(module, "visible-unrelated");
        ObjectId unrelated = module.GetCurrentCheckout();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("checkout") { "--quiet", "selection" }).Should().BeTrue();
        File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "dirty");
        RevisionGridControl control = CreateControl(module);
        try
        {
            await ReloadAsync(control, module, revisionFilter: "--all --grep=visible --date-order");

            control.CurrentCheckout.Should().Be(head);
            ObjectId[] rows = control._gridView.Items.OfType<GitRevision>()
                .Select(revision => revision.ObjectId).ToArray();
            rows.Should().Equal(unrelated, ObjectId.WorkTreeId, ObjectId.IndexId, ancestor);
            control.GetNativeTestAccessor().HasGraphParent(ObjectId.WorkTreeId, ObjectId.IndexId).Should().BeTrue();
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
            control.GetNativeTestAccessor().PendingSelectedObjectIds.Should().BeEmpty();
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
                control.revisionPage.Content.Should().BeOfType<EmptyRepoControl>();
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
            control.GetNativeTestAccessor().PendingSelectedObjectIds.Should().Equal(first.ObjectId, selected.ObjectId);

            control.GetNativeTestAccessor().ReadRevisions([first]);
            control.SelectedRevision.Should().BeNull();
            control.GetNativeTestAccessor().PendingSelectedObjectIds.Should().Equal(first.ObjectId, selected.ObjectId);

            control.GetNativeTestAccessor().ReadRevisions([selected]);
            control.GetSelectedRevisions().Select(revision => revision.ObjectId)
                .Should().Equal(selected.ObjectId, first.ObjectId);
            control.GetSelectedRevisionOrDefault().Should().BeSameAs(selected);
            control.LatestSelectedRevision.Should().BeSameAs(selected);
            control._gridView.SelectedItem.Should().BeSameAs(first);
            control.GetNativeTestAccessor().PendingSelectedObjectIds.Should().BeEmpty();
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

            control.GetNativeTestAccessor().PendingSelectedObjectIds.Should().Equal(first.ObjectId, selected.ObjectId);

            control.SelectedId = first.ObjectId;
            StartControlledLoad(control, module);

            control.GetNativeTestAccessor().PendingSelectedObjectIds.Should().Equal(first.ObjectId);
            control.GetNativeTestAccessor().ReadRevisions([first, selected]);
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
            control.GetNativeTestAccessor().ReadRevisions([first]);

            control.GetNativeTestAccessor().CompleteLoading();

            control.GetSelectedRevisions().Should().ContainSingle().Which.Should().BeSameAs(first);
            control.GetNativeTestAccessor().PendingSelectedObjectIds.Should().BeEmpty();
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
            control.GetNativeTestAccessor().ReadRevisions([first]);
            if (programmatic)
            {
                control.SetSelectedRevision(first.ObjectId).Should().BeTrue();
            }
            else
            {
                control._gridView.SelectedItem = first;
            }

            otherControl.Focus(NavigationMethod.Pointer).Should().BeTrue();

            control.GetNativeTestAccessor().ReadRevisions([selected]);
            control.GetNativeTestAccessor().CompleteLoading();

            control.SelectedRevision.Should().BeSameAs(first);
            otherControl.IsKeyboardFocusWithin.Should().BeTrue();
            control.GetNativeTestAccessor().PendingSelectedObjectIds.Should().BeEmpty();
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
            control._gridView.IsKeyboardFocusWithin.Should().BeTrue();
            otherControl.Focus(NavigationMethod.Pointer).Should().BeTrue();

            control.GetNativeTestAccessor().ReadRevisions([selected]);
            control.GetNativeTestAccessor().CompleteLoading();

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
        control.GetNativeTestAccessor().SetRevisions(revisions);
        control.SetSelectedRevision(revisions[0].ObjectId).Should().BeTrue();
        if (multipleSelection)
        {
            control.SetSelectedRevision(revisions[1].ObjectId, toggleSelection: true).Should().BeTrue();
        }

        await Task.Delay(100);
        Dispatcher.UIThread.RunJobs();
        int notifications = 0;
        control.SelectionChanged += (_, _) => notifications++;

        control.GetNativeTestAccessor().SetRevisions(revisions.Reverse());
        await Task.Delay(100);
        Dispatcher.UIThread.RunJobs();

        GitRevision expectedDetails = multipleSelection ? revisions[1] : revisions[0];
        control.SelectedRevision.Should().BeSameAs(expectedDetails);
        control.GetSelectedRevisionOrDefault().Should().BeSameAs(expectedDetails);
        control.LatestSelectedRevision.Should().BeSameAs(expectedDetails);
        control._gridView.SelectedItem.Should().BeSameAs(revisions[0]);
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
        control.GetNativeTestAccessor().SetRevisions([first, second]);
        control.SetSelectedRevision(first.ObjectId).Should().BeTrue();
        ObjectId targetId = changesSelection ? second.ObjectId : first.ObjectId;
        IGitRef relatedRef = new GitRef(null!, targetId, "refs/remotes/origin/related");
        module.RevParse(targetId.ToString()).Returns(targetId);
        control.SelectedId.Should().Be(default(ObjectId));

        control.TryGoToRelatedRef(relatedRef).Should().Be(changesSelection);

        control.GetSelectedRevisionOrDefault()?.ObjectId.Should().Be(targetId);
    }

    [AvaloniaTest]
    public async Task Cancelled_stream_callbacks_should_not_replace_selection_or_focus_from_the_active_load()
    {
        using AppSettingsTestScope settings = new();
        AppSettings.RevisionGraphShowArtificialCommits = false;
        GitModule module = CreateRepository();
        RevisionGridControl control = CreateControl(module);
        TaskCompletionSource staleMetadataStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource staleMetadataFinished = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using ManualResetEventSlim releaseStaleMetadata = new();
        using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(15));
        try
        {
            control.ReloadRevisions(module, getRefs: filter =>
            {
                staleMetadataStarted.TrySetResult();
                try
                {
                    releaseStaleMetadata.Wait(deadline.Token);
                    return module.GetRefs(filter);
                }
                finally
                {
                    staleMetadataFinished.TrySetResult();
                }
            });
            await staleMetadataStarted.Task.WaitAsync(deadline.Token);
            await ReloadAsync(control, module);
            GitRevision selected = control.SelectedRevision!;
            selected.ObjectId.Should().Be(module.GetCurrentCheckout());
            releaseStaleMetadata.Set();
            await staleMetadataFinished.Task.WaitAsync(deadline.Token);
            control.CancelBackgroundTasks();
            Dispatcher.UIThread.RunJobs();

            control._gridView.Items.OfType<GitRevision>().Should().ContainSingle()
                .Which.Should().BeSameAs(selected);
            control.SelectedRevision.Should().BeSameAs(selected);
        }
        finally
        {
            releaseStaleMetadata.Set();
            control.CancelBackgroundTasks();
        }
    }

    [AvaloniaTest]
    public async Task Ref_menu_should_use_the_original_tag_icon_and_repository_wide_unambiguous_name()
    {
        string? previousShowTags = AppSettings.SettingsContainer.GetValue("showtags");
        try
        {
            AppSettings.ShowTags = true;
            GitModule module = CreateRepository();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "same" }).Should().BeTrue();
            AddCommit(module, "tagged");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("tag") { "same" }).Should().BeTrue();
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            commands.RepoChangedNotifier.Returns(Substitute.For<ILockableNotifier>());
            commands.GetService(Arg.Any<Type>()).Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
            IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
            source.UICommands.Returns(commands);
            RevisionGridControl control = new() { UICommandsSource = source };
            Window window = new() { Width = 1000, Height = 300, Content = control };
            try
            {
                window.Show();
                await ReloadAsync(control, module);
                control.SetSelectedRevision(module.GetCurrentCheckout()).Should().BeTrue();
                Settle(window);
                ListBoxItem row = control._gridView.GetVisualDescendants().OfType<ListBoxItem>()
                    .Single(item => item.DataContext is GitRevision revision && revision.ObjectId == module.GetCurrentCheckout());
                Point position = row.TranslatePoint(new Point(row.Bounds.Width - 40, row.Bounds.Height / 2), window)!.Value;
                window.MouseDown(position, MouseButton.Right, RawInputModifiers.None);
                window.MouseUp(position, MouseButton.Right, RawInputModifiers.None);
                Dispatcher.UIThread.RunJobs();
                ContextMenu menu = control.FindControl<ContextMenu>("mainContextMenu")!;
                menu.IsOpen.Should().BeTrue();
                MenuItem merge = control.FindControl<MenuItem>("mergeBranchToolStripMenuItem")!.Items
                    .OfType<MenuItem>().Single(item => item.Header as string == "same");
                using AwesomeAssertions.Execution.AssertionScope assertions = new();
                ((Image)merge.Icon!).Source.Should().BeSameAs(GitUI.Properties.Images.BranchLocal);
                merge.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                commands.ReceivedCalls().Where(call => call.GetMethodInfo().Name == nameof(IGitUICommands.StartMergeBranchDialog))
                    .Select(call => call.GetArguments()[1]).Should().Equal("refs/tags/same");
                menu.Close();
            }
            finally
            {
                window.Close();
                control.CancelBackgroundTasks();
            }
        }
        finally
        {
            AppSettings.SettingsContainer.SetValue("showtags", previousShowTags);
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

        // GitModule snapshots its Git-directory path, so construct the UI input after git init.
        return new GitModule(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
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
        module.ConfigureCancelledRevisionRead();
        commands.Module.Returns(module);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());
        source.UICommands.Returns(commands);
        return new RevisionGridControl { UICommandsSource = source };
    }

    private static void DeselectLatestRow(RevisionGridControl control, Window window, IReadOnlyList<GitRevision> revisions)
    {
        control.SetSelectedRevision(revisions[0].ObjectId).Should().BeTrue();
        control.SetSelectedRevision(revisions[2].ObjectId, toggleSelection: true).Should().BeTrue();
        Settle(window);
        Point point = RowPoint(control, window, 2);
        window.MouseMove(point, RawInputModifiers.Control);
        window.MouseDown(point, MouseButton.Left, RawInputModifiers.Control);
        window.MouseUp(point, MouseButton.Left, RawInputModifiers.Control);
        Settle(window);

        control.GetSelectedRevisions().Should().Equal(revisions[0]);
        control.LatestSelectedRevision.Should().BeSameAs(revisions[2]);
    }

    private static Point RowPoint(RevisionGridControl control, Window window, int rowIndex)
    {
        ListBoxItem row = (ListBoxItem)control._gridView.ContainerFromIndex(rowIndex)!;
        Point point = row.TranslatePoint(new Point(100, row.Bounds.Height / 2), window)!.Value;
        (window.InputHitTest(point) as Visual)?.GetSelfAndVisualAncestors().OfType<ListBoxItem>()
            .FirstOrDefault().Should().BeSameAs(row);
        return point;
    }

    private static void Settle(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        using Avalonia.Media.Imaging.WriteableBitmap? frame = window.CaptureRenderedFrame();
        frame.Should().NotBeNull();
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
        // Let production prepare its metadata while the fixture cancels external log I/O.
        // Streamed-row assertions then use the same pending selection as the real refs stage.
        control.ReloadRevisions(module, getRefs: _ => []);
        control.GetNativeTestAccessor().JoinBackgroundOperations();
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
