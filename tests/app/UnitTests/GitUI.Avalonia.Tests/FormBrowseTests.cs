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
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.Blame;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.CommandsDialogs.Menus;
using GitUI.Compat;
using GitUI.LeftPanel;
using GitUI.Properties;
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
    private bool _revisionGraphShowArtificialCommits;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        _revisionGraphShowArtificialCommits = AppSettings.RevisionGraphShowArtificialCommits;
        AppSettings.RevisionGraphShowArtificialCommits = false;

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
        AppSettings.RevisionGraphShowArtificialCommits = _revisionGraphShowArtificialCommits;
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void FormBrowse_should_show_dashboard_only_when_the_repository_is_invalid()
    {
        GitModule invalidModule = new(
            _serviceContainer.GetRequiredService<IGitExecutorProvider>(),
            _workingDirectory);
        using (FormBrowse dashboardForm = new(new GitUICommands(_serviceContainer, invalidModule)))
        {
            dashboardForm.Show();
            Dispatcher.UIThread.RunJobs();

            dashboardForm.FindControl<Dashboard>("dashboard")!.IsVisible.Should().BeTrue();
            dashboardForm.FindControl<Grid>("mainContentGrid")!.IsVisible.Should().BeFalse();
            dashboardForm.FindControl<WrapPanel>("toolPanel")!.IsVisible.Should().BeFalse();
        }

        invalidModule.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        using FormBrowse repositoryForm = new(new GitUICommands(_serviceContainer, invalidModule));
        repositoryForm.Show();
        Dispatcher.UIThread.RunJobs();

        repositoryForm.FindControl<Dashboard>("dashboard")!.IsVisible.Should().BeFalse();
        repositoryForm.FindControl<Grid>("mainContentGrid")!.IsVisible.Should().BeTrue();
        repositoryForm.FindControl<WrapPanel>("toolPanel")!.IsVisible.Should().BeTrue();
    }

    [AvaloniaTest]
    public async Task FormBrowse_should_show_the_empty_repository_surface_when_no_revision_exists()
    {
        GitModule module = new(
            _serviceContainer.GetRequiredService<IGitExecutorProvider>(),
            _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module));

        form.Show();

        TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
        await WaitUntilAsync(() => loadingStatus.Text == "0 revisions");
        form.RevisionGrid.GetTestAccessor().CurrentPage.Should().BeOfType<EmptyRepoControl>();
    }

    [AvaloniaTest]
    public async Task FormBrowse_should_focus_the_revision_list_after_loading_and_when_commanded()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module));

        form.Show();
        TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
        await WaitUntilAsync(() => loadingStatus.Text == "1 revisions");
        ListBox revisions = form.RevisionGrid.GetTestAccessor().Revisions;
        object? focused = TopLevel.GetTopLevel(revisions)?.FocusManager?.GetFocusedElement();
        revisions.IsKeyboardFocusWithin.Should().BeTrue($"the focused element was {focused}");

        ComboBox revisionFilter = form.ToolStripFilters.GetTestAccessor().RevisionFilter;
        form.ToolStripFilters.SetFocus();
        revisionFilter.IsKeyboardFocusWithin.Should().BeTrue();

        form.ExecuteCommand(FormBrowse.Command.FocusRevisionGrid).Should().BeTrue();
        revisions.IsKeyboardFocusWithin.Should().BeTrue();
        revisionFilter.IsKeyboardFocusWithin.Should().BeFalse();
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
    public void AddNotes_should_ignore_a_missing_revision_selection()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" });
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module));

        form.ExecuteCommand(FormBrowse.Command.AddNotes).Should().BeTrue();
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task FormBrowse_branch_selector_should_open_and_checkout_the_selected_branch()
    {
        bool originalAlwaysShowCheckout = AppSettings.AlwaysShowCheckoutBranchDlg;
        bool originalCheckForUncommittedChanges = AppSettings.CheckForUncommittedChangesInCheckoutBranch;
        bool originalCloseProcessDialog = AppSettings.CloseProcessDialog;
        GitModule module = CreateRepositoryWithInitialCommit();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { "feature" });
        try
        {
            AppSettings.AlwaysShowCheckoutBranchDlg = false;
            AppSettings.CheckForUncommittedChangesInCheckoutBranch = true;
            AppSettings.CloseProcessDialog = true;
            using FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            form.Show();
            TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
            await WaitUntilAsync(() => loadingStatus.Text == "1 revisions");

            IconSplitButton branchSelector = form.FindControl<IconSplitButton>("branchSelect")!;
            Button[] templateButtons = branchSelector.GetVisualDescendants()
                .OfType<Button>()
                .Where(button => button.Name is "PART_PrimaryButton" or "PART_SecondaryButton")
                .ToArray();
            Button primaryButton = templateButtons.Single(button => button.Name == "PART_PrimaryButton");
            Button secondaryButton = templateButtons.Single(button => button.Name == "PART_SecondaryButton");
            MenuFlyout flyout = (MenuFlyout)branchSelector.Flyout!;

            Click(form, primaryButton, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            flyout.IsOpen.Should().BeTrue();
            MenuItem[] branchItems = flyout.Items.OfType<MenuItem>().Skip(1).ToArray();
            branchItems.Select(item => item.Header as string).Should().Contain("feature");
            branchItems.Should().OnlyContain(item => item.Icon is Image);

            flyout.Hide();
            Dispatcher.UIThread.RunJobs();
            Click(form, secondaryButton, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            flyout.IsOpen.Should().BeTrue("the arrow retains Avalonia's native split-button behavior");
            MenuItem feature = flyout.Items
                .OfType<MenuItem>()
                .Single(item => item.Header as string == "feature");
            TopLevel popup = TopLevel.GetTopLevel(feature)
                ?? throw new InvalidOperationException("The branch flyout did not render.");
            Click(popup, feature, MouseButton.Left);

            await WaitUntilAsync(() => module.GetSelectedBranch() == "feature");
            await WaitUntilAsync(() => branchSelector.Content as string == "feature");
        }
        finally
        {
            AppSettings.AlwaysShowCheckoutBranchDlg = originalAlwaysShowCheckout;
            AppSettings.CheckForUncommittedChangesInCheckoutBranch = originalCheckForUncommittedChanges;
            AppSettings.CloseProcessDialog = originalCloseProcessDialog;
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task FormBrowse_should_show_artificial_revisions_and_live_toolbar_status()
    {
        bool originalShowArtificial = AppSettings.RevisionGraphShowArtificialCommits;
        bool originalShowArtificialStatus = AppSettings.ShowGitStatusForArtificialCommits;
        bool originalShowToolbarStatus = AppSettings.ShowGitStatusInBrowseToolbar;
        bool originalShowAheadBehind = AppSettings.ShowAheadBehindData;
        string remoteDirectory = Path.Combine(
            Path.GetTempPath(),
            $"GitExtensions.Avalonia.Remote-{Guid.NewGuid():N}");
        Directory.CreateDirectory(remoteDirectory);

        try
        {
            AppSettings.RevisionGraphShowArtificialCommits = true;
            AppSettings.ShowGitStatusForArtificialCommits = true;
            AppSettings.ShowGitStatusInBrowseToolbar = true;
            AppSettings.ShowAheadBehindData = true;

            GitModule module = CreateRepositoryWithInitialCommit();
            GitModule remote = new(
                _serviceContainer.GetRequiredService<IGitExecutorProvider>(),
                remoteDirectory);
            remote.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "--bare" });
            module.GitExecutable.RunCommand(
                new GitArgumentBuilder("remote") { "add", "origin", remoteDirectory });
            module.GitExecutable.RunCommand(
                new GitArgumentBuilder("push") { "--quiet", "--set-upstream", "origin", "HEAD" });

            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "second commit");
            module.GitExecutable.RunCommand(
                new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "unstaged");
            File.WriteAllText(Path.Combine(_workingDirectory, "staged.txt"), "staged");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "staged.txt" });

            using FormBrowse form = new(new GitUICommands(_serviceContainer, module));
            form.Show();

            TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
            IconButton commitButton = form.FindControl<IconButton>("toolStripButtonCommit")!;
            ToolStripPushButton pushButton = form.FindControl<ToolStripPushButton>("toolStripButtonPush")!;

            await WaitUntilAsync(() =>
                loadingStatus.Text == "4 revisions"
                && commitButton.Content?.ToString() == "Commit (2)"
                && pushButton.GetTestAccessor().GetButtonText() == "1↑");

            form.RevisionGrid.ShowUncommittedChangesIfPossible.Should().BeTrue();
            form.RevisionGrid.GetChangeCount(ObjectId.WorkTreeId)!.Changed.Should().ContainSingle();
            form.RevisionGrid.GetChangeCount(ObjectId.IndexId)!.New.Should().ContainSingle();
            commitButton.Icon.Should().BeSameAs(Images.RepoStateMixed);
            form.RevisionGrid.GetVisualDescendants()
                .OfType<RevisionGridRefRenderer.RefLabelControl>()
                .Select(label => label.Label)
                .Should()
                .Contain([ResourceManager.TranslatedStrings.Workspace, ResourceManager.TranslatedStrings.Index]);
            RevisionGridControl.TestAccessor revisionGrid = form.RevisionGrid.GetTestAccessor();
            revisionGrid.HasGraphParent(ObjectId.WorkTreeId, ObjectId.IndexId).Should().BeTrue();
            revisionGrid.HasGraphParent(ObjectId.IndexId, module.GetCurrentCheckout()).Should().BeTrue();
        }
        finally
        {
            AppSettings.RevisionGraphShowArtificialCommits = originalShowArtificial;
            AppSettings.ShowGitStatusForArtificialCommits = originalShowArtificialStatus;
            AppSettings.ShowGitStatusInBrowseToolbar = originalShowToolbarStatus;
            AppSettings.ShowAheadBehindData = originalShowAheadBehind;
            TestDirectory.Delete(remoteDirectory);
        }
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
            revisionGrid.GetTestAccessor().Revisions.Items
                .Cast<GitRevision>()
                .Select(revision => revision.ObjectId)
                .Should().OnlyHaveUniqueItems();
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
    [Category("P8.6h.3b.2b.2b.2b.4")]
    public async Task Revision_grid_should_publish_the_final_graph_order_with_stash_and_artificial_rows()
    {
        bool originalShowStashes = AppSettings.ShowStashes;
        bool originalShowArtificial = AppSettings.RevisionGraphShowArtificialCommits;
        FormBrowse? form = null;
        try
        {
            AppSettings.ShowStashes = true;
            AppSettings.RevisionGraphShowArtificialCommits = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "second");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });
            ObjectId head = module.GetCurrentCheckout();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "stashed");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("stash") { "push", "-m", "row order".Quote() });
            ObjectId stash = module.RevParse("refs/stash");
            ObjectId stashIndex = module.RevParse("refs/stash^2");
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "working");

            form = new FormBrowse(new GitUICommands(_serviceContainer, module));
            form.Show();
            RevisionGridControl revisionGrid = form.RevisionGrid;
            TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
            await WaitUntilAsync(() => loadingStatus.Text == "6 revisions");

            revisionGrid.GetTestAccessor().Revisions.Items
                .Cast<GitRevision>()
                .Select(revision => revision.ObjectId)
                .Should().Equal(stash, ObjectId.WorkTreeId, ObjectId.IndexId, stashIndex, head, module.RevParse("HEAD~1"));

            revisionGrid.SetSelectedRevision(head).Should().BeTrue();
            ContextMenu contextMenu = revisionGrid.FindControl<ContextMenu>("mainContextMenu")
                ?? throw new InvalidOperationException("Revision context menu was not created.");
            MenuItem deleteBranch = revisionGrid.FindControl<MenuItem>("deleteBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Delete-branch menu item was not created.");
            contextMenu.Open(revisionGrid.GetTestAccessor().Revisions);
            Dispatcher.UIThread.RunJobs();
            deleteBranch.IsVisible.Should().BeTrue();
            deleteBranch.IsEnabled.Should().BeFalse();
            deleteBranch.Bounds.Height.Should().BeGreaterThan(0);
            contextMenu.Close();
        }
        finally
        {
            form?.Close();
            AppSettings.ShowStashes = originalShowStashes;
            AppSettings.RevisionGraphShowArtificialCommits = originalShowArtificial;
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

        MenuItem maintenance = form.FindControl<MenuItem>("gitMaintenanceToolStripMenuItem")!;
        maintenance.Items.OfType<MenuItem>().Should().ContainSingle(item => item.Name == "recoverLostObjectsToolStripMenuItem");

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "manageWorktreeToolStripMenuItem", "Text", "Manage &worktrees...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "toolStripMenuItemReflog", "Text", "Show reflo&g...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "toolStripWorktrees", "ToolTipText", "Worktrees");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "archiveToolStripMenuItem", "Text", "Archi&ve revision...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "gitMaintenanceToolStripMenuItem", "Text", "&Git maintenance");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "compressGitDatabaseToolStripMenuItem", "Text", "&Compress git database");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "recoverLostObjectsToolStripMenuItem", "Text", "&Recover lost objects...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "deleteIndexLockToolStripMenuItem", "Text", "&Delete index.lock");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "editLocalGitConfigToolStripMenuItem", "Text", "&Edit .git/config");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "repoSettingsToolStripMenuItem", "Text", "Rep&ository settings...");
    }

    [AvaloniaTest]
    public void FormBrowse_repository_menu_should_match_the_current_supported_WinForms_inventory()
    {
        using FormBrowse form = new();

        string[] actualItems = form.repositoryToolStripMenuItem.Items
            .Select(item => item switch
            {
                Separator => "|",
                MenuItem menuItem => menuItem.Name ?? throw new InvalidOperationException("A repository menu item has no name."),
                _ => throw new InvalidOperationException($"Unexpected repository menu entry: {item?.GetType().Name}"),
            })
            .ToArray();
        actualItems.Should().Equal(
            "refreshToolStripMenuItem",
            "fileExplorerToolStripMenuItem",
            "|",
            "manageRemoteRepositoriesToolStripMenuItem1",
            "|",
            "manageSubmodulesToolStripMenuItem",
            "updateAllSubmodulesToolStripMenuItem",
            "synchronizeAllSubmodulesToolStripMenuItem",
            "|",
            "manageWorktreeToolStripMenuItem",
            "|",
            "gitMaintenanceToolStripMenuItem",
            "repoSettingsToolStripMenuItem");

        KeyGesture fileExplorerShortcut = new(Key.O, KeyModifiers.Control | KeyModifiers.Shift);
        form.fileExplorerToolStripMenuItem.HotKey.Should().BeEquivalentTo(fileExplorerShortcut);
        form.fileExplorerToolStripMenuItem.InputGesture.Should().BeEquivalentTo(fileExplorerShortcut);

        MenuItem maintenance = form.gitMaintenanceToolStripMenuItem;
        maintenance.Items.OfType<MenuItem>().Select(item => item.Name).Should().Equal(
            "compressGitDatabaseToolStripMenuItem",
            "recoverLostObjectsToolStripMenuItem",
            "deleteIndexLockToolStripMenuItem",
            "editLocalGitConfigToolStripMenuItem");

        foreach (string unavailableName in new[]
        {
            "editgitignoreToolStripMenuItem1",
            "editgitinfoexcludeToolStripMenuItem",
            "editGitAttributesToolStripMenuItem",
            "editmailmapToolStripMenuItem",
            "menuitemSparse",
            "closeToolStripMenuItem",
        })
        {
            form.FindControl<Control>(unavailableName).Should().BeNull(
                $"{unavailableName} must remain absent until its native dialog or Dashboard action exists");
        }
    }

    [AvaloniaTest]
    public void FormBrowse_repository_menu_should_route_ported_dialog_commands_and_preserve_bare_repository_state()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "--bare" });
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(Arg.Any<Type>())
            .Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
        using FormBrowse form = new(commands);

        form.manageRemoteRepositoriesToolStripMenuItem1.IsEnabled.Should().BeTrue();
        form.manageSubmodulesToolStripMenuItem.IsEnabled.Should().BeFalse();
        form.updateAllSubmodulesToolStripMenuItem.IsEnabled.Should().BeFalse();
        form.synchronizeAllSubmodulesToolStripMenuItem.IsEnabled.Should().BeFalse();
        form.gitMaintenanceToolStripMenuItem.IsEnabled.Should().BeTrue();
        form.repoSettingsToolStripMenuItem.IsEnabled.Should().BeTrue();

        form.manageRemoteRepositoriesToolStripMenuItem1.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.manageSubmodulesToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.updateAllSubmodulesToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.synchronizeAllSubmodulesToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.recoverLostObjectsToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.repoSettingsToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartRemotesDialog(form);
        commands.Received(1).StartSubmodulesDialog(form);
        commands.Received(1).StartUpdateSubmodulesDialog(form);
        commands.Received(1).StartSyncSubmodulesDialog(form);
        commands.Received(1).StartVerifyDatabaseDialog(form);
        commands.Received(1).StartRepoSettingsDialog(form);
    }

    [AvaloniaTest]
    public void FormBrowse_start_tools_and_help_menus_should_match_the_current_supported_WinForms_inventory()
    {
        using FormBrowse form = new();

        GetItemNames(form.fileToolStripMenuItem).Should().Equal(
            "initNewRepositoryToolStripMenuItem",
            "openToolStripMenuItem",
            "tsmiFavouriteRepositories",
            "tsmiRecentRepositories",
            "|",
            "cloneToolStripMenuItem",
            "|",
            "exitToolStripMenuItem");
        GetItemNames(form.toolsToolStripMenuItem).Should().Equal(
            "gitBashToolStripMenuItem",
            "gitGUIToolStripMenuItem",
            "kGitToolStripMenuItem",
            "|",
            "gitcommandLogToolStripMenuItem",
            "|",
            "settingsToolStripMenuItem");
        GetItemNames(form.helpToolStripMenuItem).Should().Equal(
            "userManualToolStripMenuItem",
            "changelogToolStripMenuItem",
            "|",
            "translateToolStripMenuItem",
            "|",
            "donateToolStripMenuItem",
            "tsmiTelemetryEnabled",
            "reportAnIssueToolStripMenuItem",
            "checkForUpdatesToolStripMenuItem",
            "aboutToolStripMenuItem");

        foreach (string unavailableName in new[]
        {
            "PuTTYToolStripMenuItem",
        })
        {
            form.FindControl<Control>(unavailableName).Should().BeNull(
                $"{unavailableName} must remain absent until its shared owner or native dialog exists");
        }

        return;

        static string[] GetItemNames(MenuItem parent)
            => parent.Items
                .Select(item => item switch
                {
                    Separator => "|",
                    MenuItem menuItem => menuItem.Name
                        ?? throw new InvalidOperationException($"A {parent.Name} child has no name."),
                    _ => throw new InvalidOperationException(
                        $"Unexpected {parent.Name} entry: {item?.GetType().Name}"),
                })
                .ToArray();
    }

    [AvaloniaTest]
    public void FormBrowse_start_tools_and_help_menus_should_preserve_translation_identities()
    {
        using FormBrowse form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "fileToolStripMenuItem", "Text", "&Start");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "initNewRepositoryToolStripMenuItem", "Text", "&Create new repository...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "openToolStripMenuItem", "Text", "&Open...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "cloneToolStripMenuItem", "Text", "C&lone repository...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "gitBashToolStripMenuItem", "Text", "Git &bash");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "gitGUIToolStripMenuItem", "Text", "Git &GUI");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "kGitToolStripMenuItem", "Text", "Git&K");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "gitcommandLogToolStripMenuItem", "Text", "Git &command log");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "settingsToolStripMenuItem", "Text", "&Settings...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "userManualToolStripMenuItem", "Text", "User &manual");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "translateToolStripMenuItem", "Text", "&Translate");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "tsmiTelemetryEnabled", "Text", "&Yes, I allow telemetry");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "reportAnIssueToolStripMenuItem", "Text", "&Report an issue");
    }

    [AvaloniaTest]
    public void FormBrowse_start_tools_and_help_menu_roots_should_not_receive_the_host_title_translation()
    {
        using FormBrowse form = new();
        ITranslation translation = Substitute.For<ITranslation>();
        translation.TranslateItem(nameof(FormBrowse), "$this", "Text", Arg.Any<Func<string>>())
            .Returns("Git Extensions");
        translation.TranslateItem(nameof(FormBrowse), "fileToolStripMenuItem", "Text", Arg.Any<Func<string>>())
            .Returns("&Start translated");
        translation.TranslateItem(nameof(FormBrowse), "toolsToolStripMenuItem", "Text", Arg.Any<Func<string>>())
            .Returns("&Tools translated");
        translation.TranslateItem(nameof(FormBrowse), "helpToolStripMenuItem", "Text", Arg.Any<Func<string>>())
            .Returns("&Help translated");

        form.TranslateItems(translation);

        form.fileToolStripMenuItem.Header.Should().Be("_Start translated");
        form.toolsToolStripMenuItem.Header.Should().Be("_Tools translated");
        form.helpToolStripMenuItem.Header.Should().Be("_Help translated");
    }

    [AvaloniaTest]
    public void FormBrowse_start_tools_and_help_menus_should_route_commands_and_refresh_state()
    {
        bool isBare = false;
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDirectory);
        module.IsValidGitWorkingDir().Returns(false);
        module.IsBareRepository().Returns(_ => isBare);
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(Arg.Any<Type>())
            .Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
        using FormBrowse form = new(commands);
        StartToolStripMenuItem.TestAccessor start = form.fileToolStripMenuItem.GetTestAccessor();
        ToolsToolStripMenuItem.TestAccessor tools = form.toolsToolStripMenuItem.GetTestAccessor();
        HelpToolStripMenuItem.TestAccessor help = form.helpToolStripMenuItem.GetTestAccessor();

        start.InitNewRepositoryMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        start.CloneMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        tools.GitGuiMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        tools.GitKMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        tools.SettingsMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        help.TelemetryMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartInitializeDialog(
            null,
            null,
            Arg.Any<EventHandler<GitModuleEventArgs>>());
        commands.Received(1).StartCloneDialog(
            null,
            string.Empty,
            false,
            Arg.Any<EventHandler<GitModuleEventArgs>>());
        module.Received(1).RunGui();
        module.Received(1).RunGitK();
        commands.Received(1).StartSettingsDialog(null, null);
        commands.Received(1).StartGeneralSettingsDialog(null);

        form.toolsToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
        tools.GitGuiMenuItem.IsEnabled.Should().BeTrue();
        isBare = true;
        form.toolsToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
        tools.GitGuiMenuItem.IsEnabled.Should().BeFalse();

        bool? originalTelemetry = AppSettings.TelemetryEnabled;
        try
        {
            AppSettings.TelemetryEnabled = true;
            form.helpToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
            help.TelemetryMenuItem.IsChecked.Should().BeTrue();
        }
        finally
        {
            AppSettings.TelemetryEnabled = originalTelemetry;
        }
    }

    [AvaloniaTest]
    public void FormBrowse_navigate_and_view_menus_should_match_the_supported_revision_grid_inventory()
    {
        using FormBrowse form = new();
        MenuItem navigate = GetMainMenuItem(form, "navigateToolStripMenuItem");
        MenuItem view = GetMainMenuItem(form, "viewToolStripMenuItem");

        form.mainMenuStrip.Items.OfType<MenuItem>().Select(item => item.Name).Should().Equal(
            "fileToolStripMenuItem",
            "repositoryToolStripMenuItem",
            "navigateToolStripMenuItem",
            "viewToolStripMenuItem",
            "commandsToolStripMenuItem",
            "_repositoryHostsToolStripMenuItem",
            "pluginsToolStripMenuItem",
            "toolsToolStripMenuItem",
            "helpToolStripMenuItem");
        GetTaggedItemNames(navigate).Should().Equal(
            "ToggleBetweenArtificialAndHeadCommits",
            "GotoCurrentRevision",
            "GotoCommit",
            "|",
            "GotoChildCommit",
            "GotoParentCommit",
            "GotoFirstParentCommit",
            "GotoLastParentCommit",
            "GotoMergeBaseCommit",
            "|",
            "NavigateBackward",
            "NavigateForward",
            "|",
            "QuickSearch",
            "PrevQuickSearch",
            "NextQuickSearch");
        GetTaggedItemNames(view).Should().Equal(
            "BranchesToolStripMenuItem",
            "ShowAllBranches",
            "ShowCurrentBranchOnly",
            "ShowFilteredBranches",
            "ShowReflogReferences",
            "|",
            "filterToolStripMenuItem",
            "|",
            "drawNonrelativesGrayToolStripMenuItem",
            "HighlightSelectedBranch",
            "|",
            "CommitsToolStripMenuItem",
            "ShowArtificialCommits",
            "ShowStashes",
            "showGitNotesToolStripMenuItem",
            "ShowSessionCheckpoints",
            "|",
            "Grid_labelsToolStripMenuItem",
            "ShowRemoteBranches",
            "showTagsToolStripMenuItem",
            "ShowSuperprojectTags",
            "ShowSuperprojectRemoteBranches",
            "ShowSuperprojectBranches",
            "|",
            "Grid_infoToolStripMenuItem",
            "showBuildStatusIconToolStripMenuItem",
            "showBuildStatusTextToolStripMenuItem",
            "showCommitMessageBodyToolStripMenuItem",
            "showAuthorDateToolStripMenuItem",
            "showRelativeDateToolStripMenuItem",
            "|",
            "ColumnsToolStripMenuItem",
            "showRevisionGraphColumnToolStripMenuItem",
            "showGitNotesColumnToolStripMenuItem",
            "showAuthorAvatarColumnToolStripMenuItem",
            "showAuthorNameColumnToolStripMenuItem",
            "showDateColumnToolStripMenuItem",
            "showIdColumnToolStripMenuItem",
            "|",
            "SortingToolStripMenuItem",
            "AuthorDateSort",
            "TopoOrder",
            "|",
            "Settings_persistenceToolStripMenuItem",
            "SaveAsDefault");

        string[] unsupportedCommands =
        [
            "toolbarsMenuItem",
        ];
        GetTaggedItemNames(navigate)
            .Concat(GetTaggedItemNames(view))
            .Should().NotContain(unsupportedCommands);

        foreach (string captionTag in new[]
        {
            "BranchesToolStripMenuItem",
            "CommitsToolStripMenuItem",
            "Grid_labelsToolStripMenuItem",
            "Grid_infoToolStripMenuItem",
            "ColumnsToolStripMenuItem",
            "SortingToolStripMenuItem",
            "Settings_persistenceToolStripMenuItem",
        })
        {
            MenuItem caption = GetTaggedMenuItem(view, captionTag);
            caption.IsEnabled.Should().BeFalse();
            caption.IsHitTestVisible.Should().BeFalse();
            caption.Classes.Should().Contain("gitextensions-menu-caption");
        }
    }

    [AvaloniaTest]
    public void FormBrowse_navigate_and_view_menus_should_preserve_translation_ownership()
    {
        using FormBrowse form = new();
        ITranslation translation = Substitute.For<ITranslation>();
        translation.TranslateItem(
                nameof(FormBrowse),
                "navigateToolStripMenuItem",
                "Text",
                Arg.Any<Func<string?>>())
            .Returns("&Navigate translated");
        translation.TranslateItem(
                "RevisionGrid",
                "ShowRemoteBranches",
                "Text",
                Arg.Any<Func<string?>>())
            .Returns("Translated remote &branches");

        form.RevisionGrid.AddTranslationItems(translation);
        form.AddTranslationItems(translation);
        form.RevisionGrid.TranslateItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse), "navigateToolStripMenuItem", "Text", "&Navigate");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse), "viewToolStripMenuItem", "Text", "&View");
        translation.Received(1).AddTranslationItem(
            "RevisionGrid", "BranchesToolStripMenuItem", "Text", "Branches");
        translation.Received(1).AddTranslationItem(
            "RevisionGrid", "ShowRemoteBranches", "Text", "Show remote &branches");
        translation.DidNotReceive().AddTranslationItem(
            nameof(FormBrowse), "ShowRemoteBranches", "Text", Arg.Any<string>());

        MenuItem navigate = GetMainMenuItem(form, "navigateToolStripMenuItem");
        MenuItem view = GetMainMenuItem(form, "viewToolStripMenuItem");
        navigate.Header.Should().Be("_Navigate translated");
        GetTaggedMenuItem(view, "ShowRemoteBranches").Header.Should().Be("Translated remote _branches");
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2b.2b.2b.5")]
    public void FormBrowse_view_menu_should_share_revision_grid_command_state_and_routing()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(_workingDirectory);
        module.IsValidGitWorkingDir().Returns(true);
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(Arg.Any<Type>())
            .Returns(call => _serviceContainer.GetService(call.Arg<Type>()));

        bool originalShowRemoteBranches = AppSettings.ShowRemoteBranches;
        try
        {
            AppSettings.ShowRemoteBranches = false;
            using FormBrowse form = new(commands);
            MenuItem view = GetMainMenuItem(form, "viewToolStripMenuItem");
            MenuItem mainRemote = GetTaggedMenuItem(view, "ShowRemoteBranches");
            MenuItem contextRemote = GetTaggedMenuItem(form.RevisionGrid.ViewMenuItem, "ShowRemoteBranches");

            view.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
            mainRemote.IsChecked.Should().BeFalse();
            contextRemote.IsChecked.Should().BeFalse();
            mainRemote.InputGesture.Should().Be(new KeyGesture(Key.R, KeyModifiers.Control | KeyModifiers.Shift));

            mainRemote.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            AppSettings.ShowRemoteBranches.Should().BeTrue();
            mainRemote.IsChecked.Should().BeTrue();
            contextRemote.IsChecked.Should().BeTrue();

            AppSettings.ShowRemoteBranches = false;
            view.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
            mainRemote.IsChecked.Should().BeFalse();
            contextRemote.IsChecked.Should().BeFalse();
        }
        finally
        {
            AppSettings.ShowRemoteBranches = originalShowRemoteBranches;
        }
    }

    [AvaloniaTest]
    public async Task FormBrowse_navigate_menu_should_route_through_the_revision_grid_selection()
    {
        bool originalShowArtificialCommits = AppSettings.RevisionGraphShowArtificialCommits;
        try
        {
            AppSettings.RevisionGraphShowArtificialCommits = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "dirty");
            ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            commands.RepoChangedNotifier.Returns(notifier);
            commands.GetService(Arg.Any<Type>())
                .Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
            using FormBrowse form = new(commands);
            form.Show();
            await WaitUntilAsync(() => form.RevisionGrid.SelectedRevision?.IsArtificial == true);

            MenuItem navigate = GetMainMenuItem(form, "navigateToolStripMenuItem");
            MenuItem goToCurrent = GetTaggedMenuItem(navigate, "GotoCurrentRevision");
            MenuItem toggleArtificial = GetTaggedMenuItem(navigate, "ToggleBetweenArtificialAndHeadCommits");
            navigate.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
            goToCurrent.IsEnabled.Should().BeTrue();

            goToCurrent.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            form.RevisionGrid.SelectedRevision!.ObjectId.Should().Be(module.GetCurrentCheckout());

            navigate.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));
            toggleArtificial.IsEnabled.Should().BeTrue();
            toggleArtificial.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            form.RevisionGrid.SelectedRevision!.IsArtificial.Should().BeTrue();
        }
        finally
        {
            AppSettings.RevisionGraphShowArtificialCommits = originalShowArtificialCommits;
        }
    }

    [AvaloniaTest]
    public void FormBrowse_commands_menu_should_match_the_current_supported_WinForms_inventory()
    {
        using FormBrowse form = new();

        string[] actualItems = form.commandsToolStripMenuItem.Items
            .Select(item => item switch
            {
                Separator => "|",
                MenuItem menuItem => menuItem.Name ?? throw new InvalidOperationException("A Commands menu item has no name."),
                _ => throw new InvalidOperationException($"Unexpected Commands menu entry: {item?.GetType().Name}"),
            })
            .ToArray();
        actualItems.Should().Equal(
            "commitToolStripMenuItem",
            "undoLastCommitToolStripMenuItem",
            "pullToolStripMenuItem",
            "pushToolStripMenuItem",
            "|",
            "stashToolStripMenuItem",
            "resetToolStripMenuItem",
            "|",
            "branchToolStripMenuItem",
            "deleteBranchToolStripMenuItem",
            "checkoutBranchToolStripMenuItem",
            "mergeBranchToolStripMenuItem",
            "rebaseToolStripMenuItem",
            "runMergetoolToolStripMenuItem",
            "|",
            "tagToolStripMenuItem",
            "deleteTagToolStripMenuItem",
            "|",
            "cherryPickToolStripMenuItem",
            "archiveToolStripMenuItem",
            "toolStripMenuItemReflog",
            "|",
            "applyPatchToolStripMenuItem",
            "patchToolStripMenuItem");

        MenuFlyout pullFlyout = (MenuFlyout)form.toolStripButtonPull.Flyout!;
        pullFlyout.Items.Should().Contain(form.fetchAllToolStripMenuItem);

        foreach (string unavailableName in new[]
        {
            "cleanupToolStripMenuItem",
            "checkoutToolStripMenuItem",
            "bisectToolStripMenuItem",
            "formatPatchToolStripMenuItem",
        })
        {
            form.FindControl<Control>(unavailableName).Should().BeNull(
                $"{unavailableName} must remain absent until its native dialog exists");
        }
    }

    [AvaloniaTest]
    public void FormBrowse_pull_toolbar_shortcuts_should_preserve_the_original_action_order()
    {
        using FormBrowse form = new();

        string[] shortcutNames = form.ToolStripMain.Children
            .OfType<IconButton>()
            .Where(button => button.Name?.StartsWith(FormBrowse.FetchPullToolbarShortcutsPrefix, StringComparison.Ordinal) is true)
            .Select(button => button.Name!)
            .ToArray();

        shortcutNames.Should().Equal(
            "pull_shortcut_fetchToolStripMenuItem",
            "pull_shortcut_fetchAllToolStripMenuItem",
            "pull_shortcut_fetchPruneAllToolStripMenuItem",
            "pull_shortcut_mergeToolStripMenuItem",
            "pull_shortcut_rebaseToolStripMenuItem1",
            "pull_shortcut_pullToolStripMenuItem1");
        form.defaultPullDialogToolStripMenuItem.Tag.Should().Be(GitPullAction.None);
        form.defaultPullMergeToolStripMenuItem.Tag.Should().Be(GitPullAction.Merge);
        form.defaultPullRebaseToolStripMenuItem.Tag.Should().Be(GitPullAction.Rebase);
        form.defaultPullFetchToolStripMenuItem.Tag.Should().Be(GitPullAction.Fetch);
        form.defaultPullFetchAllToolStripMenuItem.Tag.Should().Be(GitPullAction.FetchAll);
        form.defaultPullFetchPruneAllToolStripMenuItem.Tag.Should().Be(GitPullAction.FetchPruneAll);
    }

    [AvaloniaTest]
    public void FormBrowse_commands_menu_should_preserve_translation_identities()
    {
        using FormBrowse form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "undoLastCommitToolStripMenuItem", "Text", "&Undo last commit...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "pushToolStripMenuItem", "Text", "&Push...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "resetToolStripMenuItem", "Text", "&Reset changes...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "runMergetoolToolStripMenuItem", "Text", "&Solve merge conflicts...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "cherryPickToolStripMenuItem", "Text", "Cherr&y pick...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "applyPatchToolStripMenuItem", "Text", "&Apply patch...");
        translation.Received(1).AddTranslationItem(nameof(FormBrowse), "fetchAllToolStripMenuItem", "Text", "Fetch &all");
    }

    [AvaloniaTest]
    public async Task FormBrowse_commands_menu_should_route_functional_commands_and_update_selection_state()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(Arg.Any<Type>())
            .Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
        using FormBrowse form = new(commands);

        form.Show();
        TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
        await WaitUntilAsync(() => loadingStatus.Text == "1 revisions" && form.RevisionGrid.SelectedRevision is not null);
        form.commandsToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));

        new[]
        {
            form.commitToolStripMenuItem,
            form.undoLastCommitToolStripMenuItem,
            form.pushToolStripMenuItem,
            form.resetToolStripMenuItem,
            form.branchToolStripMenuItem,
            form.deleteBranchToolStripMenuItem,
            form.checkoutBranchToolStripMenuItem,
            form.mergeBranchToolStripMenuItem,
            form.rebaseToolStripMenuItem,
            form.runMergetoolToolStripMenuItem,
            form.tagToolStripMenuItem,
            form.deleteTagToolStripMenuItem,
            form.cherryPickToolStripMenuItem,
            form.archiveToolStripMenuItem,
            form.toolStripMenuItemReflog,
            form.applyPatchToolStripMenuItem,
        }.Should().OnlyContain(item => item.IsEnabled);

        form.pushToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.resetToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.runMergetoolToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.cherryPickToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        form.applyPatchToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        commands.Received(1).StartPushDialog(form, pushOnShow: false);
        commands.Received(1).StartResetChangesDialog(
            form,
            Arg.Any<IReadOnlyCollection<GitItemStatus>>(),
            onlyWorkTree: false);
        commands.Received(1).StartResolveConflictsDialog(form, offerCommit: true);
        commands.Received(1).StartCherryPickDialog(
            form,
            Arg.Is<IEnumerable<GitRevision>>(revisions => revisions.Single().ObjectId == module.GetCurrentCheckout()));
        commands.Received(1).StartApplyPatchDialog(form, patchFile: null);
    }

    [AvaloniaTest]
    public void FormBrowse_commands_menu_should_follow_invalid_and_bare_repository_state()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(Arg.Any<Type>())
            .Returns(call => _serviceContainer.GetService(call.Arg<Type>()));

        using (FormBrowse invalidForm = new(commands))
        {
            invalidForm.repositoryToolStripMenuItem.IsVisible.Should().BeFalse();
            invalidForm.commandsToolStripMenuItem.IsVisible.Should().BeFalse();
        }

        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "--bare" });
        using FormBrowse bareForm = new(commands);
        bareForm.repositoryToolStripMenuItem.IsVisible.Should().BeTrue();
        bareForm.commandsToolStripMenuItem.IsVisible.Should().BeTrue();
        bareForm.commandsToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.SubmenuOpenedEvent));

        new[]
        {
            bareForm.commitToolStripMenuItem,
            bareForm.undoLastCommitToolStripMenuItem,
            bareForm.stashToolStripMenuItem,
            bareForm.resetToolStripMenuItem,
            bareForm.branchToolStripMenuItem,
            bareForm.deleteBranchToolStripMenuItem,
            bareForm.checkoutBranchToolStripMenuItem,
            bareForm.mergeBranchToolStripMenuItem,
            bareForm.rebaseToolStripMenuItem,
            bareForm.runMergetoolToolStripMenuItem,
            bareForm.cherryPickToolStripMenuItem,
            bareForm.toolStripMenuItemReflog,
            bareForm.applyPatchToolStripMenuItem,
        }.Should().OnlyContain(item => !item.IsEnabled);

        bareForm.pullToolStripMenuItem.IsEnabled.Should().BeTrue();
        bareForm.pushToolStripMenuItem.IsEnabled.Should().BeTrue();
        bareForm.patchToolStripMenuItem.IsEnabled.Should().BeTrue();
    }

    [AvaloniaTest]
    [NonParallelizable]
    public async Task FormBrowse_undo_last_commit_should_preserve_changes_in_the_index()
    {
        bool originalDontConfirm = AppSettings.DontConfirmUndoLastCommit.Value;
        FormBrowse? form = null;
        try
        {
            AppSettings.DontConfirmUndoLastCommit.Value = true;
            GitModule module = CreateRepositoryWithInitialCommit();
            ObjectId initialCommit = module.GetCurrentCheckout();
            File.AppendAllText(Path.Combine(_workingDirectory, "tracked.txt"), "second");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" });
            form = new FormBrowse(new GitUICommands(_serviceContainer, module));
            form.Show();
            TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
            await WaitUntilAsync(() => loadingStatus.Text == "2 revisions");
            bool reloadStarted = false;
            loadingStatus.PropertyChanged += (_, e) =>
                reloadStarted |= e.Property == TextBlock.TextProperty && loadingStatus.Text == "Loading…";

            form.undoLastCommitToolStripMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            await WaitUntilAsync(() => reloadStarted && loadingStatus.Text == "1 revisions");
            module.GetCurrentCheckout().Should().Be(initialCommit);
            module.GitExecutable.GetOutput(new GitArgumentBuilder("diff") { "--cached", "--name-only" })
                .Should().Contain("tracked.txt");
        }
        finally
        {
            form?.Close();
            AppSettings.DontConfirmUndoLastCommit.Value = originalDontConfirm;
        }
    }

    [AvaloniaTest]
    public void FormBrowse_repository_host_menu_should_preserve_translation_identities()
    {
        using FormBrowse form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "_repositoryHostsToolStripMenuItem",
            "Text",
            "(Repository hosts)");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "_forkCloneRepositoryToolStripMenuItem",
            "Text",
            "&Fork/Clone repository...");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "_viewPullRequestsToolStripMenuItem",
            "Text",
            "View &pull requests...");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "_createPullRequestsToolStripMenuItem",
            "Text",
            "&Create pull requests...");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "_addUpstreamRemoteToolStripMenuItem",
            "Text",
            "&Add upstream remote");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "_noReposHostPluginLoaded",
            "Text",
            "No repository host plugin loaded.");
        translation.Received(1).AddTranslationItem(
            nameof(FormBrowse),
            "_noReposHostFound",
            "Text",
            "Could not find any relevant repository hosts for the currently open repository.");
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void FormBrowse_repository_host_menu_should_route_all_functional_contributions()
    {
        IRepositoryHostPlugin[] originalHosts = [.. PluginRegistry.GitHosters];
        GitModule module = CreateRepositoryWithInitialCommit();
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(Arg.Any<Type>())
            .Returns(call => _serviceContainer.GetService(call.Arg<Type>()));
        IRepositoryHostPlugin host = Substitute.For<IRepositoryHostPlugin>();
        host.Name.Returns("TestHost");
        host.GitModuleIsRelevantToMe().Returns(true);
        PluginRegistry.GitHosters.Clear();
        PluginRegistry.GitHosters.Add(host);
        FormBrowse? form = null;

        try
        {
            form = new FormBrowse(commands);
            form.Show();
            form.UpdateRepositoryHostsMenuForTest(validWorkingDir: true);
            MenuItem hostMenu = form.FindControl<MenuItem>("_repositoryHostsToolStripMenuItem")!;
            MenuItem forkClone = form.FindControl<MenuItem>("_forkCloneRepositoryToolStripMenuItem")!;
            MenuItem viewPullRequests = form.FindControl<MenuItem>("_viewPullRequestsToolStripMenuItem")!;
            MenuItem createPullRequest = form.FindControl<MenuItem>("_createPullRequestsToolStripMenuItem")!;
            MenuItem addUpstream = form.FindControl<MenuItem>("_addUpstreamRemoteToolStripMenuItem")!;

            hostMenu.IsVisible.Should().BeTrue();
            hostMenu.Header.Should().Be("TestHost");
            forkClone.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            viewPullRequests.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            createPullRequest.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            addUpstream.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            commands.Received(1).StartCloneForkFromHoster(
                form,
                host,
                Arg.Any<EventHandler<GitModuleEventArgs>>());
            commands.Received(1).StartPullRequestsDialog(form, host);
            commands.Received(1).StartCreatePullRequest(form, host);
            commands.Received(1).AddUpstreamRemote(form, host);
        }
        finally
        {
            form?.Close();
            PluginRegistry.GitHosters.Clear();
            PluginRegistry.GitHosters.AddRange(originalHosts);
        }
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
    public async Task FormBrowse_should_apply_command_line_browse_arguments_before_loading()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        ObjectId selectedId = module.GetCurrentCheckout();
        File.WriteAllText(Path.Combine(_workingDirectory, "other.txt"), "other");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "other.txt" });
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial other".Quote() });
        BrowseArguments args = new()
        {
            RevFilter = "initial",
            PathFilter = "tracked.txt",
            SelectedId = selectedId,
            IsFileHistoryMode = true,
        };

        FormBrowse form = new(new GitUICommands(_serviceContainer, module), args);
        try
        {
            form.Show();
            TextBlock loadingStatus = form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!;
            await WaitUntilAsync(() => loadingStatus.Text == "1 revisions" && form.RevisionGrid.SelectedRevision is not null);

            form.RevisionGrid.SelectedId.Should().Be(selectedId);
            form.FindControl<FilterToolBar>("ToolStripFilters")!
                .FindControl<ComboBox>("tstxtRevisionFilter")!.Text.Should().Be("initial");
            form.FindControl<Control>("leftPanel")!.IsVisible.Should().BeFalse();
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
                form.fileStatusList.SelectedItem!.Item.Name.Should().Be("tracked.txt");

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
                    && form.fileTree.FileStatusList.SelectedItem?.Item.Name == "src/followed.txt"
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
                    && form.fileStatusList.SelectedItem?.Item.Name == "tracked.txt"
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
                    && form.fileStatusList.SelectedItem?.Item.Name == "tracked.txt");

                form.CommitInfoTabControl.SelectedItem = form.DiffTabPage;
                Dispatcher.UIThread.RunJobs();
                form.fileStatusList.FindControl<MenuItem>("tsmiBlame")!
                    .RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

                BlameControl blame = form.fileTree.FindControl<BlameControl>("BlameControl")!;
                await WaitUntilAsync(() =>
                    ReferenceEquals(form.CommitInfoTabControl.SelectedItem, form.TreeTabPage)
                    && form.fileTree.FileStatusList.SelectedItem?.Item.Name == "tracked.txt"
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
    [Category("P8.6h.3b.2b.2b.2b.5")]
    public async Task RevisionGrid_rebase_menu_should_match_current_branch_state()
    {
        GitModule module = CreateRepositoryWithInitialCommit();
        using FormBrowse form = new(new GitUICommands(_serviceContainer, module));
        form.Show();
        RevisionGridControl revisionGrid = form.FindControl<RevisionGridControl>("RevisionGrid")
            ?? throw new InvalidOperationException("Revision grid was not created.");
        TextBlock loadingStatus = revisionGrid.FindControl<TextBlock>("lblLoadingStatus")
            ?? throw new InvalidOperationException("Revision loading status was not created.");
        await WaitUntilAsync(() => loadingStatus.Text == "1 revisions" && revisionGrid.SelectedRevision is not null);

        ContextMenu contextMenu = revisionGrid.FindControl<ContextMenu>("mainContextMenu")
            ?? throw new InvalidOperationException("Revision context menu was not created.");
        ListBox revisions = revisionGrid.FindControl<ListBox>("_gridView")
            ?? throw new InvalidOperationException("Revision list was not created.");
        MenuItem rebaseOn = revisionGrid.FindControl<MenuItem>("rebaseOnToolStripMenuItem")
            ?? throw new InvalidOperationException("Rebase-on menu item was not created.");
        MenuItem rebase = revisionGrid.FindControl<MenuItem>("rebaseToolStripMenuItem")
            ?? throw new InvalidOperationException("Rebase menu item was not created.");
        MenuItem rebaseInteractively = revisionGrid.FindControl<MenuItem>("rebaseInteractivelyToolStripMenuItem")
            ?? throw new InvalidOperationException("Interactive-rebase menu item was not created.");
        MenuItem rebaseWithAdvancedOptions = revisionGrid.FindControl<MenuItem>("rebaseWithAdvOptionsToolStripMenuItem")
            ?? throw new InvalidOperationException("Advanced-rebase menu item was not created.");

        contextMenu.Open(revisions);
        Dispatcher.UIThread.RunJobs();
        rebaseOn.IsVisible.Should().BeTrue("WinForms keeps the Rebase on parent available for a regular revision");
        rebaseOn.IsEnabled.Should().BeTrue();

        rebaseOn.IsSubMenuOpen = true;
        Dispatcher.UIThread.RunJobs();
        rebase.IsEnabled.Should().BeFalse("the selected HEAD has no other branch to rebase onto");
        rebaseInteractively.IsEnabled.Should().BeFalse();
        rebaseWithAdvancedOptions.IsEnabled.Should().BeFalse();
        rebaseOn.IsSubMenuOpen = false;
        contextMenu.Close();
    }

    [AvaloniaTest]
    [Category("P8.6h.3b.2b.2b.2b.4")]
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

            ContextMenu contextMenu = revisionGrid.FindControl<ContextMenu>("mainContextMenu")
                ?? throw new InvalidOperationException("Revision context menu was not created.");
            MenuItem checkoutBranch = revisionGrid.FindControl<MenuItem>("checkoutBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Checkout-branch menu item was not created.");
            MenuItem pushBranch = revisionGrid.FindControl<MenuItem>("tsmiPushBranch")
                ?? throw new InvalidOperationException("Push-branch menu item was not created.");
            MenuItem mergeBranch = revisionGrid.FindControl<MenuItem>("mergeBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Merge-branch menu item was not created.");
            MenuItem resetCurrentBranch = revisionGrid.FindControl<MenuItem>("resetCurrentBranchToHereToolStripMenuItem")
                ?? throw new InvalidOperationException("Reset-current-branch menu item was not created.");
            MenuItem createBranch = revisionGrid.FindControl<MenuItem>("createNewBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Create-branch menu item was not created.");
            MenuItem renameBranch = revisionGrid.FindControl<MenuItem>("renameBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Rename-branch menu item was not created.");
            MenuItem deleteBranch = revisionGrid.FindControl<MenuItem>("deleteBranchToolStripMenuItem")
                ?? throw new InvalidOperationException("Delete-branch menu item was not created.");
            MenuItem createTag = revisionGrid.FindControl<MenuItem>("createTagToolStripMenuItem")
                ?? throw new InvalidOperationException("Create-tag menu item was not created.");
            MenuItem archiveRevision = revisionGrid.FindControl<MenuItem>("archiveRevisionToolStripMenuItem")
                ?? throw new InvalidOperationException("Archive-revision menu item was not created.");
            MenuItem cherryPick = revisionGrid.FindControl<MenuItem>("cherryPickCommitToolStripMenuItem")
                ?? throw new InvalidOperationException("Cherry-pick menu item was not created.");
            MenuItem revertCommit = revisionGrid.FindControl<MenuItem>("revertCommitToolStripMenuItem")
                ?? throw new InvalidOperationException("Revert-commit menu item was not created.");
            MenuItem archive = form.FindControl<MenuItem>("archiveToolStripMenuItem")
                ?? throw new InvalidOperationException("Archive menu item was not created.");
            CopyContextMenuItem copy = revisionGrid.FindControl<CopyContextMenuItem>("copyToClipboardToolStripMenuItem")
                ?? throw new InvalidOperationException("Copy menu item was not created.");
            MenuItem rebase = revisionGrid.FindControl<MenuItem>("rebaseToolStripMenuItem")
                ?? throw new InvalidOperationException("Rebase menu item was not created.");
            MenuItem editCommit = revisionGrid.FindControl<MenuItem>("editCommitToolStripMenuItem")
                ?? throw new InvalidOperationException("Edit-commit menu item was not created.");
            MenuItem rewordCommit = revisionGrid.FindControl<MenuItem>("rewordCommitToolStripMenuItem")
                ?? throw new InvalidOperationException("Reword-commit menu item was not created.");
            MenuItem view = revisionGrid.FindControl<MenuItem>("viewToolStripMenuItem")
                ?? throw new InvalidOperationException("View menu item was not created.");
            ListBox revisions = revisionGrid.FindControl<ListBox>("_gridView")
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
                copy.Bounds.Height.Should().BeGreaterThan(0);
                deleteBranch.IsVisible.Should().BeTrue();
                deleteBranch.IsEnabled.Should().BeTrue();
                deleteBranch.Bounds.Height.Should().BeGreaterThan(0);
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
            resetCurrentBranch.IsVisible.Should().BeTrue();
            createBranch.IsEnabled.Should().BeTrue();
            renameBranch.IsEnabled.Should().BeTrue();
            deleteBranch.IsEnabled.Should().BeTrue();
            createTag.IsEnabled.Should().BeTrue();
            archiveRevision.IsVisible.Should().BeTrue();
            cherryPick.IsVisible.Should().BeTrue();
            revertCommit.IsVisible.Should().BeTrue();
            archive.IsEnabled.Should().BeTrue();
            editCommit.IsEnabled.Should().BeTrue();
            rewordCommit.IsEnabled.Should().BeTrue();
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
            archiveRevision.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            cherryPick.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            revertCommit.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            archive.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

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
            commands.Received(2).StartArchiveDialog(
                form,
                Arg.Is<GitRevision>(revision => revision.ObjectId == selectedObjectId),
                null,
                null);
            commands.Received(1).StartCherryPickDialog(
                form,
                Arg.Is<IEnumerable<GitRevision>>(revisions => revisions.Single().ObjectId == selectedObjectId));
            commands.Received(1).StartRevertCommitDialog(
                form,
                Arg.Is<GitRevision>(revision => revision.ObjectId == selectedObjectId));

            bool originalDontConfirmRebase = AppSettings.DontConfirmRebase.Value;
            try
            {
                AppSettings.DontConfirmRebase.Value = true;
                rebase.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                commands.Received(1).StartRebase(form, "feature");
            }
            finally
            {
                AppSettings.DontConfirmRebase.Value = originalDontConfirmRebase;
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

    private static void Click(TopLevel topLevel, Control control, MouseButton button)
    {
        Avalonia.Point clickPoint = Avalonia.VisualExtensions.TranslatePoint(
            control,
            new Avalonia.Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            topLevel) ?? throw new InvalidOperationException("The control position was not available.");
        topLevel.MouseDown(clickPoint, button, RawInputModifiers.None);
        topLevel.MouseUp(clickPoint, button, RawInputModifiers.None);
    }

    private static string HeaderText(TreeViewItem item)
        => ((TextBlock)((StackPanel)item.Header!).Children[1]).Text!;

    private static MenuItem GetMainMenuItem(FormBrowse form, string name)
        => form.mainMenuStrip.Items
            .OfType<MenuItem>()
            .Single(item => item.Name == name);

    private static MenuItem GetTaggedMenuItem(MenuItem parent, string tag)
        => parent.Items
            .OfType<MenuItem>()
            .Single(item => item.Tag as string == tag || item.Name == tag);

    private static string[] GetTaggedItemNames(MenuItem parent)
        => parent.Items
            .Select(item => item switch
            {
                Separator => "|",
                MenuItem menuItem => menuItem.Tag as string ?? menuItem.Name
                    ?? throw new InvalidOperationException("A shared menu command has no tag."),
                _ => throw new InvalidOperationException($"Unexpected shared menu entry: {item?.GetType().Name}"),
            })
            .ToArray();
}
