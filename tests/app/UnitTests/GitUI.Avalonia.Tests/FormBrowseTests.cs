using System.Collections.Concurrent;
using System.ComponentModel.Design;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.Git.Gpg;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.Blame;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.LeftPanel;
using GitUI.ScriptsEngine;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FormBrowseTests
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
        _serviceContainer.AddService<IAppTitleGenerator>(new AppTitleGenerator(repositoryDescriptionProvider));
        _serviceContainer.AddService<ILinkFactory>(new LinkFactory());
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

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
    public void QuickFetch_should_stop_when_the_before_fetch_script_cancels()
    {
        TestScriptEventRecorder scriptEvents = TestScriptEventRecorder.Install(_serviceContainer);
        scriptEvents.CancelledEvents.Add(ScriptEvent.BeforeFetch);
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        FormBrowse form = new(new GitUICommands(_serviceContainer, module));

        form.ExecuteCommand(FormBrowse.Command.QuickFetch).Should().BeTrue();

        scriptEvents.Events.Should().Equal(ScriptEvent.BeforeFetch);
    }

    [AvaloniaTest]
    public async Task FormBrowse_should_reload_after_repository_changed_notification()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "content");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });

        GitUICommands commands = new(_serviceContainer, module);
        FormBrowse form = new(commands);
        try
        {
            form.Show();
            RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")
                ?? throw new InvalidOperationException("Revision grid was not created.");
            TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")
                ?? throw new InvalidOperationException("Revision loading status was not created.");

            await WaitUntilAsync(() => loadingStatus.Text == "1 revisions");

            RevisionGridRefRenderer.RefLabelControl currentBranch =
                revisionGrid.GetVisualDescendants()
                    .OfType<RevisionGridRefRenderer.RefLabelControl>()
                    .Single();
            currentBranch.Icon.Should().Be(RefLabelIcon.Head);
            currentBranch.FontWeight.Should().Be(Avalonia.Media.FontWeight.Bold);
            TextBlock currentCommitSubject = revisionGrid.GetVisualDescendants()
                .OfType<TextBlock>()
                .Single(textBlock => textBlock.Classes.Contains("revision-subject"));
            currentCommitSubject.FontWeight.Should().Be(Avalonia.Media.FontWeight.Bold);

            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "updated");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });
            ObjectId remoteCommit = module.GetCurrentCheckout();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("reset") { "--quiet", "--hard", "HEAD~" });
            module.GitExecutable.RunCommand(new GitArgumentBuilder("update-ref") { "refs/remotes/origin/main", remoteCommit });

            bool reloadStarted = false;
            loadingStatus.PropertyChanged += (_, e) =>
                reloadStarted |= e.Property == TextBlock.TextProperty && loadingStatus.Text == "Loading…";
            commands.RepoChangedNotifier.Notify();

            await WaitUntilAsync(() => reloadStarted && loadingStatus.Text == "2 revisions");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task FormBrowse_should_load_stashes_into_the_left_panel_and_select_their_revision()
    {
        bool originalShowStashes = AppSettings.ShowStashes;
        bool originalShowStashTree = AppSettings.RepoObjectsTreeShowStashes;
        FormBrowse? form = null;
        try
        {
            AppSettings.ShowStashes = true;
            AppSettings.RepoObjectsTreeShowStashes = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "stashed");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("stash") { "push", "-m", "older left panel".Quote() });
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "stashed again");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("stash") { "push", "-m", "latest left panel".Quote() });
            IReadOnlyCollection<GitRevision> stashes = new RevisionReader(module).GetStashes(CancellationToken.None);
            GitRevision olderStash = stashes.Last();
            form = new FormBrowse(new GitUICommands(_serviceContainer, module));
            form.Show();
            RepoObjectsTree repoObjectsTree = form.FindControl<RepoObjectsTree>("repoObjectsTree")!;
            RepoObjectsTree.TestAccessor accessor = repoObjectsTree.GetTestAccessor();
            RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")!;

            await WaitUntilAsync(() => accessor.Tree.Items.Count == 6);
            TreeViewItem stashRoot = accessor.Tree.Items.Cast<TreeViewItem>().Last();
            await WaitUntilAsync(() => stashRoot.Items.Count == 2);
            TreeViewItem stashItem = stashRoot.Items.Cast<TreeViewItem>().Last();

            accessor.Tree.SelectedItem = stashItem;

            await WaitUntilAsync(() => revisionGrid.SelectedRevision?.ObjectId == olderStash.ObjectId);
            repoObjectsTree.SelectedRevisionObjectId.Should().Be(olderStash.ObjectId);
            revisionGrid.SelectedRevision!.ReflogSelector.Should().Be("refs/stash@{1}");
        }
        finally
        {
            form?.Close();
            AppSettings.ShowStashes = originalShowStashes;
            AppSettings.RepoObjectsTreeShowStashes = originalShowStashTree;
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task FormBrowse_should_expose_loaded_worktrees_in_the_left_panel_and_toolbar()
    {
        bool originalShowWorktrees = AppSettings.RepoObjectsTreeShowWorktrees;
        string linkedPath = $"{_workingDirectory}-linked";
        FormBrowse? form = null;
        try
        {
            AppSettings.RepoObjectsTreeShowWorktrees = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "feature" });
            module.GitExecutable.RunCommand(new GitArgumentBuilder("worktree") { "add", "--quiet", linkedPath.Quote(), "feature" });
            form = new FormBrowse(new GitUICommands(_serviceContainer, module));
            form.Show();
            RepoObjectsTree repoObjectsTree = form.FindControl<RepoObjectsTree>("repoObjectsTree")!;
            RepoObjectsTree.TestAccessor accessor = repoObjectsTree.GetTestAccessor();
            IconSplitButton worktreeButton = form.FindControl<IconSplitButton>("toolStripWorktrees")!;

            await WaitUntilAsync(() => worktreeButton.IsVisible);

            TreeViewItem root = accessor.Tree.Items.Cast<TreeViewItem>()
                .Single(item => HeaderText(item).StartsWith("Worktrees", StringComparison.Ordinal));
            HeaderText(root).Should().Be("Worktrees (2)");
            root.Items.Cast<TreeViewItem>().Should().HaveCount(2);

            MenuFlyout flyout = (MenuFlyout)worktreeButton.Flyout!;
            flyout.ShowAt(worktreeButton);
            Dispatcher.UIThread.RunJobs();
            MenuItem[] worktreeItems = flyout.Items.OfType<MenuItem>().Take(2).ToArray();
            worktreeItems.Should().HaveCount(2);
            worktreeItems[0].IsChecked.Should().BeTrue();
            worktreeItems[0].IsEnabled.Should().BeFalse();
            worktreeItems[1].Header!.ToString().Should().Contain("feature");
            worktreeItems[1].IsEnabled.Should().BeTrue();
            flyout.Items.OfType<MenuItem>().Skip(2).Select(item => item.Header!.ToString()).Should().Equal(
                GitUI.TranslatedStrings.CreateWorktree,
                GitUI.TranslatedStrings.PruneWorktrees,
                GitUI.TranslatedStrings.ManageWorktrees);
            flyout.Hide();
        }
        finally
        {
            form?.Close();
            AppSettings.RepoObjectsTreeShowWorktrees = originalShowWorktrees;
            TestDirectory.Delete(linkedPath);
        }
    }

    [AvaloniaTest]
    public void FormBrowse_worktree_surfaces_should_reuse_the_existing_translation_keys()
    {
        FormBrowse form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "manageWorktreeToolStripMenuItem", "Text", "Manage &worktrees...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "toolStripWorktrees", "ToolTipText", "Worktrees");
    }

    [AvaloniaTest]
    public async Task FormBrowse_quick_revision_filter_should_reload_the_revision_grid()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "second");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });

        FormBrowse form = new(new GitUICommands(_serviceContainer, module));
        try
        {
            form.Show();
            RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")!;
            TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
            await WaitUntilAsync(() => loadingStatus.Text == "2 revisions");

            FilterToolBar filters = form.FindControl<FilterToolBar>("ToolStripFilters")!;
            ComboBox revisionFilter = filters.FindControl<ComboBox>("tstxtRevisionFilter")!;
            revisionFilter.Text = "initial";
            revisionFilter.RaiseEvent(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyUpEvent,
                Key = Key.Enter,
            });

            await WaitUntilAsync(() => loadingStatus.Text == "1 revisions");
            revisionGrid.SelectedRevision!.Subject.Should().Be("initial");
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_commit_and_diff_tabs_should_follow_the_selected_revision()
    {
        CommitInfoPosition originalPosition = AppSettings.CommitInfoPosition;
        bool originalShowSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.ShowSplitViewLayout = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "\nsecond");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });

            FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            try
            {
                form.Show();
                TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
                await WaitUntilAsync(() =>
                    loadingStatus.Text == "2 revisions"
                    && form.RevisionInfo.Revision?.Subject == "second"
                    && form.fileStatusList.GitItemStatuses.Count == 1
                    && form.fileViewer.TextEditor.Text.Contains("+second", StringComparison.Ordinal));

                form.CommitInfoTabControl.SelectedItem.Should().BeSameAs(form.CommitInfoTabPage);
                form.fileStatusList.SelectedItem!.Name.Should().Be("tracked.txt");

                form.CommitInfoTabControl.SelectedItem = form.DiffTabPage;
                Dispatcher.UIThread.RunJobs();
                form.fileStatusList.Bounds.Height.Should().BeGreaterThan(0);
                form.fileViewer.TextEditor.Text.Should().Contain("+second");
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.CommitInfoPosition = originalPosition;
            AppSettings.ShowSplitViewLayout = originalShowSplitView;
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_file_tree_should_load_lazily_and_follow_the_path_filter()
    {
        CommitInfoPosition originalPosition = AppSettings.CommitInfoPosition;
        bool originalShowSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.ShowSplitViewLayout = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            Directory.CreateDirectory(Path.Combine(_workingDirectory, "src"));
            Directory.CreateDirectory(Path.Combine(_workingDirectory, "docs"));
            File.WriteAllText(Path.Combine(_workingDirectory, "src", "followed.txt"), "followed file");
            File.WriteAllText(Path.Combine(_workingDirectory, "docs", "other.txt"), "other file");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "src/followed.txt", "docs/other.txt" });
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "add tree".Quote() });

            FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            try
            {
                form.Show();
                TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
                await WaitUntilAsync(() => loadingStatus.Text == "2 revisions");

                form.fileTree.DisplayedRevision.Should().BeNull("the hidden tab must not enumerate the repository tree");
                form.fileTree.FileStatusList.GitItemStatuses.Should().BeEmpty();

                form.RevisionGrid.SetAndApplyPathFilter("\"src/followed.txt\"");
                await WaitUntilAsync(() =>
                    loadingStatus.Text == "1 revisions"
                    && form.RevisionGrid.SelectedRevision?.Subject == "add tree");

                form.CommitInfoTabControl.SelectedItem = form.TreeTabPage;
                Dispatcher.UIThread.RunJobs();
                await WaitUntilAsync(() =>
                    form.fileTree.DisplayedRevision?.Subject == "add tree"
                    && form.fileTree.FileStatusList.GitItemStatuses.Count == 3
                    && form.fileTree.FileStatusList.SelectedItem?.Name == "src/followed.txt"
                    && form.fileTree.FileViewer.TextEditor.Text.Contains("followed file", StringComparison.Ordinal));

                TreeView tree = form.fileTree.FileStatusList.FindControl<TreeView>("tvFiles")!;
                ListBox list = form.fileTree.FileStatusList.FindControl<ListBox>("lstFiles")!;
                tree.IsVisible.Should().BeTrue();
                list.IsVisible.Should().BeFalse();
                form.fileTree.FileStatusList.SelectedRelativePath.Should().Be(RelativePath.From("src/followed.txt"));
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.CommitInfoPosition = originalPosition;
            AppSettings.ShowSplitViewLayout = originalShowSplitView;
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_diff_should_toggle_blame_in_the_existing_viewer()
    {
        bool originalUseDiffViewerForBlame = AppSettings.UseDiffViewerForBlame.Value;
        try
        {
            AppSettings.UseDiffViewerForBlame.Value = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "\nsecond");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });

            FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            try
            {
                form.Show();
                TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
                await WaitUntilAsync(() =>
                    loadingStatus.Text == "2 revisions"
                    && form.fileStatusList.SelectedItem?.Name == "tracked.txt"
                    && form.fileViewer.TextEditor.Text.Contains("+second", StringComparison.Ordinal));

                form.CommitInfoTabControl.SelectedItem = form.DiffTabPage;
                Dispatcher.UIThread.RunJobs();
                MenuItem blameMenu = form.fileStatusList.FindControl<MenuItem>("tsmiBlame")!;
                BlameControl blame = form.revisionDiff.FindControl<BlameControl>("BlameControl")!;

                blameMenu.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                await WaitUntilAsync(() =>
                    blame.IsVisible
                    && blame.BlameFile.TextEditor.Text.Contains("second", StringComparison.Ordinal));
                blameMenu.IsChecked.Should().BeTrue();
                form.fileViewer.IsVisible.Should().BeFalse();

                blameMenu.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                await WaitUntilAsync(() =>
                    form.fileViewer.IsVisible
                    && form.fileViewer.TextEditor.Text.Contains("+second", StringComparison.Ordinal));
                blame.IsVisible.Should().BeFalse();
                blameMenu.IsChecked.Should().BeFalse();
            }
            finally
            {
                Stopwatch closeStopwatch = Stopwatch.StartNew();
                form.Close();
                closeStopwatch.Stop();
                closeStopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5),
                    "switching away from Blame must not leave an owner task waiting on the unpumped Avalonia dispatcher");
            }
        }
        finally
        {
            AppSettings.UseDiffViewerForBlame.Value = originalUseDiffViewerForBlame;
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_diff_should_open_file_tree_in_blame_mode_when_configured()
    {
        bool originalUseDiffViewerForBlame = AppSettings.UseDiffViewerForBlame.Value;
        try
        {
            AppSettings.UseDiffViewerForBlame.Value = false;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "\nsecond");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });

            FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            try
            {
                form.Show();
                TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
                await WaitUntilAsync(() =>
                    loadingStatus.Text == "2 revisions"
                    && form.fileStatusList.SelectedItem?.Name == "tracked.txt");

                form.CommitInfoTabControl.SelectedItem = form.DiffTabPage;
                Dispatcher.UIThread.RunJobs();
                form.fileStatusList.FindControl<MenuItem>("tsmiBlame")!
                    .RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

                BlameControl blame = form.fileTree.FindControl<BlameControl>("BlameControl")!;
                await WaitUntilAsync(() =>
                    ReferenceEquals(form.CommitInfoTabControl.SelectedItem, form.TreeTabPage)
                    && form.fileTree.FileStatusList.SelectedItem?.Name == "tracked.txt"
                    && blame.IsVisible
                    && blame.BlameFile.TextEditor.Text.Contains("second", StringComparison.Ordinal));
                form.fileTree.FileStatusList.FindControl<MenuItem>("tsmiBlame")!.IsChecked.Should().BeTrue();
                form.fileTree.FileViewer.IsVisible.Should().BeFalse();
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.UseDiffViewerForBlame.Value = originalUseDiffViewerForBlame;
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_gpg_tab_should_load_lazily_and_ignore_stale_results()
    {
        bool originalShowGpgInformation = AppSettings.ShowGpgInformation.Value;
        CommitInfoPosition originalPosition = AppSettings.CommitInfoPosition;
        bool originalShowSplitView = AppSettings.ShowSplitViewLayout;
        try
        {
            AppSettings.ShowGpgInformation.Value = true;
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            AppSettings.ShowSplitViewLayout = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.WriteAllText(Path.Combine(_workingDirectory, "second.txt"), "second commit");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "second.txt" });
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "second commit".Quote() });

            ConcurrentDictionary<ObjectId, TaskCompletionSource<GpgInfo?>> completions = [];
            IGpgInfoProvider provider = Substitute.For<IGpgInfoProvider>();
            provider.LoadGpgInfoAsync(Arg.Any<GitRevision?>()).Returns(callInfo =>
            {
                GitRevision revision = callInfo.Arg<GitRevision>();
                TaskCompletionSource<GpgInfo?> completion = new();
                completions[revision.ObjectId] = completion;
                return completion.Task;
            });

            FormBrowse form = new(new GitUICommands(_serviceContainer, module), provider);
            try
            {
                form.Show();
                TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
                await WaitUntilAsync(() => loadingStatus.Text == "2 revisions");

                GitRevision headRevision = form.RevisionGrid.SelectedRevision!;
                ObjectId parentId = headRevision.FirstParentId;
                form.GpgInfoTabPage.IsVisible.Should().BeTrue();
                _ = provider.DidNotReceive().LoadGpgInfoAsync(Arg.Any<GitRevision?>());

                form.CommitInfoTabControl.SelectedItem = form.GpgInfoTabPage;
                Dispatcher.UIThread.RunJobs();
                await WaitUntilAsync(() => completions.ContainsKey(headRevision.ObjectId));
                form.revisionGpgInfo1.IsKeyboardFocusWithin.Should().BeTrue();

                form.RevisionGrid.SetSelectedRevision(parentId).Should().BeTrue();
                await WaitUntilAsync(() => completions.ContainsKey(parentId));
                completions[parentId].SetResult(new GpgInfo(
                    CommitStatus.MissingPublicKey,
                    "current revision signature",
                    TagStatus.TagNotSigned,
                    TagVerificationMessage: null));

                TextBox commitInfo = form.revisionGpgInfo1.FindControl<TextBox>("txtCommitGpgInfo")!;
                TextBox tagInfo = form.revisionGpgInfo1.FindControl<TextBox>("txtTagGpgInfo")!;
                Image commitPicture = form.revisionGpgInfo1.FindControl<Image>("commitSignPicture")!;
                Image tagPicture = form.revisionGpgInfo1.FindControl<Image>("tagSignPicture")!;
                await WaitUntilAsync(() => commitInfo.Text == "current revision signature");
                tagInfo.Text.Should().Be("Tag is not signed");
                tagInfo.IsVisible.Should().BeTrue();
                commitPicture.Source.Should().BeSameAs(GitUI.Properties.Images.CommitSignatureWarning);
                tagPicture.IsVisible.Should().BeFalse();

                completions[headRevision.ObjectId].SetResult(new GpgInfo(
                    CommitStatus.GoodSignature,
                    "stale revision signature",
                    TagStatus.OneGood,
                    "stale tag signature"));
                Dispatcher.UIThread.RunJobs();
                commitInfo.Text.Should().Be("current revision signature");

                form.RefreshGpgInfo(new GitRevision(ObjectId.WorkTreeId));
                form.GpgInfoTabPage.IsVisible.Should().BeFalse();
                form.CommitInfoTabControl.SelectedItem.Should().BeSameAs(form.TreeTabPage);

                AppSettings.ShowGpgInformation.Value = false;
                form.RefreshGpgInfo(headRevision);
                form.GpgInfoTabPage.IsVisible.Should().BeFalse();
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.ShowGpgInformation.Value = originalShowGpgInformation;
            AppSettings.CommitInfoPosition = originalPosition;
            AppSettings.ShowSplitViewLayout = originalShowSplitView;
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_should_cancel_an_unfinished_gpg_load_when_closed()
    {
        bool originalShowGpgInformation = AppSettings.ShowGpgInformation.Value;
        try
        {
            AppSettings.ShowGpgInformation.Value = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            TaskCompletionSource<GpgInfo?> unfinishedLoad = new(TaskCreationOptions.RunContinuationsAsynchronously);
            IGpgInfoProvider provider = Substitute.For<IGpgInfoProvider>();
            provider.LoadGpgInfoAsync(Arg.Any<GitRevision?>()).Returns(unfinishedLoad.Task);

            FormBrowse form = new(new GitUICommands(_serviceContainer, module), provider);
            try
            {
                form.Show();
                TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
                await WaitUntilAsync(() => loadingStatus.Text == "1 revisions");

                form.CommitInfoTabControl.SelectedItem = form.GpgInfoTabPage;
                Dispatcher.UIThread.RunJobs();
                await WaitUntilAsync(() => provider.ReceivedCalls().Any());
                form.RefreshGpgInfo(new GitRevision(ObjectId.WorkTreeId));
                Dispatcher.UIThread.RunJobs();

                Stopwatch stopwatch = Stopwatch.StartNew();
                form.Close();
                stopwatch.Stop();

                stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5),
                    "closing the browser must cancel its pending GPG wait instead of blocking for the task-manager timeout");
            }
            finally
            {
                if (form.IsVisible)
                {
                    form.Close();
                }
            }
        }
        finally
        {
            AppSettings.ShowGpgInformation.Value = originalShowGpgInformation;
        }
    }

    [AvaloniaTest]
    public async Task Revision_grid_notes_provider_should_load_and_render_git_notes()
    {
        bool originalShowNotesColumn = AppSettings.ShowGitNotesColumn.Value;
        bool originalShowGitNotes = AppSettings.ShowGitNotes;
        bool originalShowToolTips = AppSettings.ShowRevisionGridTooltips.Value;
        try
        {
            AppSettings.ShowGitNotesColumn.Value = true;
            AppSettings.ShowGitNotes = false;
            AppSettings.ShowRevisionGridTooltips.Value = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("notes")
            {
                "add",
                "-m",
                "First note\nSecond note".Quote(),
            }).Should().BeTrue();

            GitUICommands commands = new(_serviceContainer, module);
            FormBrowse form = new(commands);
            try
            {
                form.Show();
                RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")
                    ?? throw new InvalidOperationException("Revision grid was not created.");
                TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")
                    ?? throw new InvalidOperationException("Revision loading status was not created.");

                await WaitUntilAsync(() => loadingStatus.Text == "1 revisions");

                TextBlock notesCell = revisionGrid.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Single(textBlock => textBlock.Classes.Contains("revision-notes-cell"));
                notesCell.Text.Should().Be("First note");
                ToolTip.GetTip(notesCell).Should().Be("First note\nSecond note");
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.ShowGitNotesColumn.Value = originalShowNotesColumn;
            AppSettings.ShowGitNotes = originalShowGitNotes;
            AppSettings.ShowRevisionGridTooltips.Value = originalShowToolTips;
        }
    }

    [AvaloniaTest]
    public async Task Revision_grid_should_highlight_the_selected_author_and_expose_lane_tooltips()
    {
        bool originalShowAuthor = AppSettings.ShowAuthorNameColumn;
        bool originalShowToolTips = AppSettings.ShowRevisionGridTooltips.Value;
        try
        {
            AppSettings.ShowAuthorNameColumn = true;
            AppSettings.ShowRevisionGridTooltips.Value = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            ObjectId initialCommit = module.GetCurrentCheckout();
            module.SetSetting("user.name", "Second Author");
            module.SetSetting("user.email", "second@example.com");
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "second");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });
            ObjectId secondCommit = module.GetCurrentCheckout();

            FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            try
            {
                form.Show();
                RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")
                    ?? throw new InvalidOperationException("Revision grid was not created.");
                TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")
                    ?? throw new InvalidOperationException("Revision loading status was not created.");
                await WaitUntilAsync(() => loadingStatus.Text == "2 revisions" && revisionGrid.SelectedRevision is not null);

                TextBlock[] authorCells =
                [
                    .. revisionGrid.GetVisualDescendants()
                        .OfType<TextBlock>()
                        .Where(textBlock => textBlock.Classes.Contains("revision-author-cell")),
                ];
                authorCells.Single(cell => cell.Text == "Second Author").FontWeight
                    .Should().Be(Avalonia.Media.FontWeight.Bold);
                authorCells.Single(cell => cell.Text == "Avalonia Test").FontWeight
                    .Should().Be(Avalonia.Media.FontWeight.Normal);

                RevisionGraphColumnProvider graphProvider =
                    (RevisionGraphColumnProvider)revisionGrid.ColumnProviders[0];
                graphProvider.GetLaneToolTip(revisionGrid.SelectedRevision!, x: 1)
                    .Should().Contain(revisionGrid.SelectedRevision!.Guid);

                revisionGrid.SetSelectedRevision(initialCommit).Should().BeTrue();
                Dispatcher.UIThread.RunJobs();
                authorCells.Single(cell => cell.Text == "Avalonia Test").FontWeight
                    .Should().Be(Avalonia.Media.FontWeight.Bold);
                authorCells.Single(cell => cell.Text == "Second Author").FontWeight
                    .Should().Be(Avalonia.Media.FontWeight.Normal);

                IGitRef relatedRef = Substitute.For<IGitRef>();
                relatedRef.Guid.Returns(initialCommit.ToString());
                relatedRef.ObjectId.Returns(initialCommit);
                revisionGrid.SetSelectedRevision(secondCommit).Should().BeTrue();
                revisionGrid.GoToRelatedRef(relatedRef).Should().BeTrue();
                revisionGrid.SelectedRevision!.ObjectId.Should().Be(initialCommit);
            }
            finally
            {
                form.Close();
            }
        }
        finally
        {
            AppSettings.ShowAuthorNameColumn = originalShowAuthor;
            AppSettings.ShowRevisionGridTooltips.Value = originalShowToolTips;
        }
    }

    [AvaloniaTest]
    public async Task RevisionGrid_context_menu_should_route_the_selected_revision()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "feature" });
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(Arg.Any<Type>()).Returns(call => _serviceContainer.GetService(call.Arg<Type>()));

        FormBrowse form = new(commands);
        try
        {
            form.Show();
            RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")
                ?? throw new InvalidOperationException("Revision grid was not created.");
            TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")
                ?? throw new InvalidOperationException("Revision loading status was not created.");
            await WaitUntilAsync(() => loadingStatus.Text == "1 revisions" && revisionGrid.SelectedRevision is not null);

            ContextMenu contextMenu = revisionGrid.FindControl<ContextMenu>("revisionContextMenu")
                ?? throw new InvalidOperationException("Revision context menu was not created.");
            MenuItem checkoutBranch = revisionGrid.FindControl<MenuItem>("checkoutBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Checkout-branch menu item was not created.");
            MenuItem pushBranch = revisionGrid.FindControl<MenuItem>("tsmiPushBranch")
                ?? throw new InvalidOperationException("Push-branch menu item was not created.");
            MenuItem mergeBranch = revisionGrid.FindControl<MenuItem>("mergeBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Merge-branch menu item was not created.");
            MenuItem createBranch = revisionGrid.FindControl<MenuItem>("createNewBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Create-branch menu item was not created.");
            MenuItem renameBranch = revisionGrid.FindControl<MenuItem>("renameBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Rename-branch menu item was not created.");
            MenuItem deleteBranch = revisionGrid.FindControl<MenuItem>("deleteBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Delete-branch menu item was not created.");
            MenuItem createTag = revisionGrid.FindControl<MenuItem>("createTagToolStripMenuItem")
                ?? throw new InvalidOperationException("Create-tag menu item was not created.");
            CopyContextMenuItem copy = revisionGrid.FindControl<CopyContextMenuItem>("copyToClipboardToolStripMenuItem")
                ?? throw new InvalidOperationException("Copy menu item was not created.");
            MenuItem rebase = revisionGrid.FindControl<MenuItem>("rebaseToolStripMenuItem")
                ?? throw new InvalidOperationException("Rebase menu item was not created.");
            MenuItem view = revisionGrid.FindControl<MenuItem>("viewToolStripMenuItem")
                ?? throw new InvalidOperationException("View menu item was not created.");
            ListBox revisions = revisionGrid.FindControl<ListBox>("lstRevisions")
                ?? throw new InvalidOperationException("Revision list was not created.");

            (string Name, ThemeVariant Variant)[] themes =
            [
                ("Light", ThemeVariant.Light),
                ("Dark", ThemeVariant.Dark),
            ];
            foreach ((string themeName, ThemeVariant themeVariant) in themes)
            {
                form.RequestedThemeVariant = themeVariant;
                contextMenu.Open(revisions);
                Dispatcher.UIThread.RunJobs();

                TopLevel contextMenuRoot = TopLevel.GetTopLevel(contextMenu)
                    ?? throw new InvalidOperationException("Revision context menu did not open in a top level.");
                WriteableBitmap? contextMenuFrame = contextMenuRoot.CaptureRenderedFrame();
                contextMenuFrame.Should().NotBeNull($"the opened context menu should render in {themeName}");
                view.IsSubMenuOpen = true;
                Dispatcher.UIThread.RunJobs();
                WriteableBitmap? viewMenuFrame = contextMenuRoot.CaptureRenderedFrame();
                viewMenuFrame.Should().NotBeNull($"the opened revision View menu should render in {themeName}");
                if (Environment.GetEnvironmentVariable("GITEXT_CAPTURE_REVISION_CONTEXT_MENU") == "1")
                {
                    string captureDirectory = Path.Combine(Path.GetTempPath(), "gitextensions-avalonia-revision-context");
                    Directory.CreateDirectory(captureDirectory);
                    using FileStream stream = File.Create(Path.Combine(captureDirectory, $"{themeName}.png"));
                    contextMenuFrame!.Save(stream, PngBitmapEncoderOptions.Default);
                    using FileStream viewStream = File.Create(Path.Combine(captureDirectory, $"{themeName}.View.png"));
                    viewMenuFrame!.Save(viewStream, PngBitmapEncoderOptions.Default);
                }

                view.IsSubMenuOpen = false;
                contextMenu.Close();
            }

            ObjectId selectedObjectId = revisionGrid.SelectedRevision!.ObjectId;
            checkoutBranch.IsEnabled.Should().BeTrue();
            pushBranch.IsEnabled.Should().BeTrue();
            mergeBranch.IsEnabled.Should().BeTrue();
            createBranch.IsEnabled.Should().BeTrue();
            renameBranch.IsEnabled.Should().BeTrue();
            deleteBranch.IsEnabled.Should().BeTrue();
            createTag.IsEnabled.Should().BeTrue();
            copy.Items.Should().NotBeEmpty();

            MenuItem checkoutFeature = checkoutBranch.Items.Cast<MenuItem>()
                .Single(item => item.Header?.ToString() == "feature");
            MenuItem pushFeature = pushBranch.Items.Cast<MenuItem>()
                .Single(item => item.Header?.ToString() == "feature");
            MenuItem mergeFeature = mergeBranch.Items.Cast<MenuItem>()
                .Single(item => item.Header?.ToString() == "feature");
            MenuItem renameFeature = renameBranch.Items.Cast<MenuItem>()
                .Single(item => item.Header?.ToString() == "feature");
            MenuItem deleteFeature = deleteBranch.Items.Cast<MenuItem>()
                .Single(item => item.Header?.ToString() == "feature");

            checkoutFeature.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            pushFeature.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            mergeFeature.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            renameFeature.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            deleteFeature.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            createBranch.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            createTag.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            commands.Received(1).StartCheckoutBranch(form, "feature");
            bool pushCompleted;
            commands.Received(1).StartPushDialog(form, false, false, out pushCompleted, "feature");
            commands.Received(1).StartMergeBranchDialog(form, "feature");
            commands.Received(1).StartRenameDialog(form, "feature");
            commands.Received(1).StartDeleteBranchDialog(form, "feature");
            commands.Received(1).StartCreateBranchDialog(form, selectedObjectId);
            commands.Received(1).StartCreateTagDialog(
                form,
                Arg.Is<GitRevision>(revision => revision.ObjectId == selectedObjectId));

            bool originalDontConfirmRebase = AppSettings.DontConfirmRebase;
            try
            {
                AppSettings.DontConfirmRebase = true;
                rebase.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                commands.Received(1).StartRebase(form, "feature");
            }
            finally
            {
                AppSettings.DontConfirmRebase = originalDontConfirmRebase;
            }
        }
        finally
        {
            form.Close();
        }
    }

    private GitModule CreateRepositoryWithInitialCommit()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "tracked.txt"), "content");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" });
        return module;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        condition().Should().BeTrue("the repository reload should complete before the timeout");
    }

    private static string HeaderText(TreeViewItem item)
        => ((TextBlock)((StackPanel)item.Header!).Children[1]).Text!;
}
