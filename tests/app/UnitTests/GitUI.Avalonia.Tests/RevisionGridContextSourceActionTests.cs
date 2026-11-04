using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

/// <summary>
///  Exercises source revision-context gates and their actual routed menu consumers.
/// </summary>
[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class RevisionGridContextSourceActionTests
{
    private static readonly FieldInfo CachedTools = typeof(CustomDiffMergeToolCache).GetField("_tools", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingFieldException(typeof(CustomDiffMergeToolCache).FullName, "_tools");
    private JoinableTaskContext? _previousContext;
    private JoinableTaskContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        _previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        _context = new JoinableTaskContext(Thread.CurrentThread, SynchronizationContext.Current);
        ThreadHelper.JoinableTaskContext = _context;
    }

    [TearDown]
    public async Task TearDown()
    {
        try
        {
            Dispatcher.UIThread.RunJobs();
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(10));
            await ThreadHelper.JoinPendingOperationsAsync(cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            ThreadHelper.JoinableTaskContext = _previousContext!;
            _context.Dispose();
        }
    }

    [AvaloniaTest]
    [TestCase("commit", false)]
    [TestCase("stash", false)]
    [TestCase("autostash", false)]
    [TestCase("commit", true)]
    [TestCase("stash", true)]
    [TestCase("autostash", true)]
    public void Stash_context_should_preserve_source_apply_pop_and_drop_gates(string kind, bool bare)
    {
        using GridFixture fixture = new(bare, kind);
        fixture.OpenContextMenu();

        bool stash = kind == "stash";
        bool canApply = !bare && (kind is "stash" or "autostash");
        fixture.AssertMenu("applyStashToolStripMenuItem", canApply);
        fixture.AssertMenu("popStashToolStripMenuItem", !bare && stash);
        fixture.AssertMenu("dropStashToolStripMenuItem", !bare && stash);
        fixture.Grid.FindControl<Control>("sepStash")!.IsVisible.Should().Be(canApply);
    }

    [AvaloniaTest]
    [TestCase("stash")]
    [TestCase("autostash")]
    public void ApplyStash_should_route_both_source_stash_kinds_to_the_selected_commit(string kind)
    {
        using GridFixture fixture = new(bare: false, kind: kind);
        fixture.OpenContextMenu();
        fixture.AssertMenu("applyStashToolStripMenuItem", true);
        fixture.Item("applyStashToolStripMenuItem").RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        fixture.Commands.Received(1).StashApply(Arg.Any<WinFormsShims.IWin32Window>(), fixture.Revisions[0].Guid);
        fixture.Commands.DidNotReceive().StashPop(Arg.Any<WinFormsShims.IWin32Window>(), Arg.Any<string>());
    }

    [AvaloniaTest]
    [TestCase("current", false)]
    [TestCase("local", false)]
    [TestCase("remote", false)]
    [TestCase("tag", false)]
    [TestCase("mixed", false)]
    [TestCase("current", true)]
    [TestCase("local", true)]
    [TestCase("remote", true)]
    [TestCase("tag", true)]
    [TestCase("mixed", true)]
    public void Ref_context_should_retain_bare_repository_safe_actions_and_source_worktree_gates(string refs, bool bare)
    {
        using GridFixture fixture = new(bare, refs: refs);
        fixture.OpenContextMenu();

        bool current = refs is "current" or "mixed";
        bool local = refs is "local" or "mixed";
        bool remote = refs is "remote" or "mixed";
        bool tag = refs is "tag" or "mixed";
        fixture.AssertMenu("renameBranchToolStripMenuItem", current || local);
        fixture.AssertMenu("deleteTagToolStripMenuItem", tag);
        fixture.AssertMenu("tsmiSelectInLeftPanel", true);
        fixture.AssertMenu("createTagToolStripMenuItem", true);
        fixture.AssertMenu("checkoutBranchToolStripMenuItem", !bare && (local || remote));
        fixture.AssertMenu("tsmiPushBranch", !bare && (current || local));
        fixture.AssertMenu("mergeBranchToolStripMenuItem", !bare && (local || remote || tag));
        fixture.AssertMenu("rebaseOnToolStripMenuItem", !bare);
        fixture.AssertMenu("resetCurrentBranchToHereToolStripMenuItem", !bare);
        fixture.AssertMenu("checkoutRevisionToolStripMenuItem", !bare);
        fixture.AssertMenu("createNewBranchToolStripMenuItem", !bare);
        MenuItem deleteBranch = fixture.Item("deleteBranchToolStripMenuItem");
        deleteBranch.IsVisible.Should().Be(current || (!bare && (local || remote)));
        deleteBranch.IsEnabled.Should().Be(!bare && (local || remote));

        string[] branches = refs switch
        {
            "current" => ["current"],
            "local" => ["other"],
            "remote" => [],
            "tag" => [],
            _ => ["current", "other"],
        };
        fixture.Item("renameBranchToolStripMenuItem").Items.OfType<MenuItem>()
            .Select(item => item.Header?.ToString()).Should().Equal(branches);
        string[] tags = tag ? ["release"] : [];
        fixture.Item("deleteTagToolStripMenuItem").Items.OfType<MenuItem>()
            .Select(item => item.Header?.ToString()).Should().Equal(tags);
    }

    [AvaloniaTest]
    [TestCase("rename", false)]
    [TestCase("deleteTag", false)]
    [TestCase("selectInLeftPanel", false)]
    [TestCase("rename", true)]
    [TestCase("deleteTag", true)]
    [TestCase("selectInLeftPanel", true)]
    public void Ref_children_should_execute_the_chosen_safe_action_once_without_bubbling_another_ref(string action, bool bare)
    {
        using GridFixture fixture = new(bare, refs: "mixed");
        fixture.OpenContextMenu();
        string parentName = action switch
        {
            "rename" => "renameBranchToolStripMenuItem",
            "deleteTag" => "deleteTagToolStripMenuItem",
            _ => "tsmiSelectInLeftPanel",
        };
        fixture.AssertMenu(parentName, true);
        string target = action == "deleteTag" ? "release" : "other";
        MenuItem item = fixture.Item(parentName).Items.OfType<MenuItem>()
            .Single(child => child.Header?.ToString() == target);
        item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        if (action == "rename")
        {
            fixture.Commands.Received(1).StartRenameDialog(Arg.Any<WinFormsShims.IWin32Window>(), target);
        }
        else if (action == "deleteTag")
        {
            fixture.Commands.Received(1).StartDeleteTagDialog(Arg.Any<WinFormsShims.IWin32Window>(), target);
        }
        else
        {
            fixture.SelectedRefs.Should().Equal([target],
                "the child callback must not bubble into the parent's first-ref Tag callback");
        }
    }

    [AvaloniaTest]
    [TestCase(1, false)]
    [TestCase(2, false)]
    [TestCase(3, false)]
    [TestCase(1, true)]
    [TestCase(2, true)]
    [TestCase(3, true)]
    public async Task Difftool_menu_should_forward_the_selected_tool_and_source_first_and_latest_revisions(int count, bool custom)
    {
        using GridFixture fixture = new(bare: false, count: count);
        object? previousTools = CachedTools.GetValue(CustomDiffMergeToolCache.DiffToolCache);
        try
        {
            await CustomDiffMergeToolCache.DiffToolCache.ClearAsync();
            fixture.Module.GetEffectiveSetting(Arg.Any<string>(), Arg.Any<string>()).Returns("meld");
            fixture.Module.GetCustomDiffMergeTools(isDiff: true, Arg.Any<CancellationToken>()).Returns(
                "'git difftool --tool=<tool>' may be set to one of the following:\n\t\tkdiff3\n\t\tmeld\n");
            await CustomDiffMergeToolCache.DiffToolCache.GetToolsAsync(fixture.Module, delay: 0, CancellationToken.None);
            AppSettings.ShowAvailableDiffTools = true;
            fixture.Grid.LoadCustomDifftools();
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(10));
            await ThreadHelper.JoinPendingOperationsAsync(cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            fixture.OpenContextMenu();

            MenuItem difftool = fixture.Item("openCommitsWithDiffToolMenuItem");
            difftool.IsEnabled.Should().BeTrue();
            difftool.Items.OfType<MenuItem>().Select(item => item.Tag).Where(tag => tag is not null)
                .Should().Equal("meld", "kdiff3");
            MenuItem clicked = custom
                ? difftool.Items.OfType<MenuItem>().Single(item => item.Tag as string == "kdiff3")
                : difftool;
            clicked.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            string first = count == 1 ? fixture.Revisions[0].FirstParentId.ToString() : fixture.Revisions[0].Guid;
            fixture.Module.Received(1).OpenWithDifftoolDirDiff(first, fixture.Revisions[^1].Guid,
                customTool: custom ? "kdiff3" : null);
            fixture.Module.ReceivedCalls().Count(call => call.GetMethodInfo().Name == nameof(IGitModule.OpenWithDifftoolDirDiff))
                .Should().Be(1, "a custom child must not also invoke the parent's default tool");
        }
        finally
        {
            fixture.Grid.CancelLoadCustomDifftools();
            await CustomDiffMergeToolCache.DiffToolCache.ClearAsync();
            CachedTools.SetValue(CustomDiffMergeToolCache.DiffToolCache, previousTools);
        }
    }

    [AvaloniaTest]
    public void DiffSelectedCommitsWithDifftool_should_keep_the_source_null_base_for_a_root_commit()
    {
        using GridFixture fixture = new(bare: false);
        fixture.Revisions[0].ParentIds = [];
        fixture.Grid.DiffSelectedCommitsWithDifftool("meld");

        fixture.Module.Received(1).OpenWithDifftoolDirDiff(null, fixture.Revisions[0].Guid, customTool: "meld");
    }

    [AvaloniaTest]
    public void Edit_by_fixup_should_keep_the_original_menu_order_and_ignore_a_root_commit()
    {
        using GridFixture fixture = new(bare: false, kind: "commit");
        fixture.Revisions[0].ParentIds = [];
        fixture.OpenContextMenu();
        fixture.Item("manipulateCommitToolStripMenuItem").Items.OfType<MenuItem>()
            .Take(4).Select(item => item.Name).Should().Equal(
                "editCommitToolStripMenuItem", "editCommitWithFixupToolStripMenuItem",
                "rewordCommitToolStripMenuItem", "fixupCommitToolStripMenuItem");
        int openedWindows = 0;
        using IDisposable subscription = Window.WindowOpenedEvent.AddClassHandler<Window>((_, _) => openedWindows++);

        fixture.Item("editCommitWithFixupToolStripMenuItem").RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        openedWindows.Should().Be(0);
    }

    private sealed class GridFixture : IDisposable
    {
        private readonly Dictionary<string, string?> _settings;

        public GridFixture(bool bare, string kind = "commit", string refs = "none", int count = 1)
        {
            string[] keys =
            [
                "difftools.showavailable",
                "showrevisiongridauthoravatarcolumn",
                "showRemoteBranches",
                "showtags",
                "AlwaysShowAdvOpt",
            ];
            _settings = keys.ToDictionary(key => key, key => AppSettings.SettingsContainer.GetValue(key), StringComparer.Ordinal);
            AppSettings.ShowAvailableDiffTools = false;
            AppSettings.ShowAuthorAvatarColumn = false;
            AppSettings.ShowRemoteBranches = true;
            AppSettings.ShowTags = true;
            AppSettings.AlwaysShowAdvOpt = false;
            Module = Substitute.For<IGitModule>();
            Module.GitVersion.Returns(Substitute.For<IGitVersion>());
            Module.IsBareRepository().Returns(bare);
            Module.GetSelectedBranch().Returns("current");
            Commands = Substitute.For<IGitUICommands>();
            Commands.Module.Returns(Module);
            Commands.RepoChangedNotifier.Returns(Substitute.For<ILockableNotifier>());
            Commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());
            IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
            source.UICommands.Returns(Commands);
            Grid = new RevisionGridControl(Substitute.For<ICommitDataManager>()) { UICommandsSource = source };
            Grid.SuspendRefreshRevisions();
            Grid.SelectInLeftPanel += SelectedRefs.Add;
            Revisions = [.. Enumerable.Range(1, count).Select(index => new GitRevision(ObjectId.Parse(index.ToString("x40")))
            {
                Subject = "Commit " + index,
                Body = "Commit " + index,
                ParentIds = [ObjectId.Parse((index + 100).ToString("x40"))],
                Author = "Context Test",
            })];
            if (kind == "stash")
            {
                Revisions[0].ReflogSelector = "refs/stash@{3}";
            }
            else if (kind == "autostash")
            {
                Revisions[0].IsAutostash = true;
            }

            Revisions[0].Refs = refs switch
            {
                "current" => [Ref("refs/heads/current")],
                "local" => [Ref("refs/heads/other")],
                "remote" => [Ref("refs/remotes/origin/other")],
                "tag" => [Ref("refs/tags/release")],
                "mixed" => [Ref("refs/heads/current"), Ref("refs/heads/other"), Ref("refs/remotes/origin/other"), Ref("refs/tags/release")],
                _ => [],
            };
            Grid.GetTestAccessor().SetRevisions(Revisions);
            Window = new Window { Width = 1000, Height = 220, Content = Grid };
            Window.Show();
            Window.Activate();
            for (int index = 0; index < Revisions.Length; index++)
            {
                Grid.SetSelectedRevision(Revisions[index].ObjectId, toggleSelection: index > 0).Should().BeTrue();
            }

            Settle();
            Menu = Grid.FindControl<ContextMenu>("mainContextMenu")
                ?? throw new AssertionException("The source context menu must exist.");
        }

        public IGitModule Module { get; }

        public IGitUICommands Commands { get; }

        public RevisionGridControl Grid { get; }

        public GitRevision[] Revisions { get; }

        public Window Window { get; }

        public ContextMenu Menu { get; }

        public List<string> SelectedRefs { get; } = [];

        public MenuItem Item(string name)
            => Grid.FindControl<MenuItem>(name) ?? throw new AssertionException("The source menu item must exist: " + name);

        public void AssertMenu(string name, bool enabled)
        {
            Item(name).IsVisible.Should().Be(enabled, name + " must follow the source context gate");
            Item(name).IsEnabled.Should().Be(enabled, name + " must follow the source context gate");
        }

        public void OpenContextMenu()
        {
            Settle();
            Control row = Grid.GetTestAccessor().Revisions.ContainerFromIndex(Revisions.Length - 1) as Control
                ?? throw new AssertionException("The real selected row must be realized.");
            Point point = row.TranslatePoint(new Point(row.Bounds.Width - 5, row.Bounds.Height / 2), Window)
                ?? throw new AssertionException("The real selected row must remain attached.");
            (Window.InputHitTest(point) as Visual)?.GetSelfAndVisualAncestors().Should().Contain(row);
            Window.MouseDown(point, MouseButton.Right);
            Window.MouseUp(point, MouseButton.Right);
            Settle();
            Menu.IsOpen.Should().BeTrue("the actual right-button route must open the row's context menu");
        }

        public void Settle()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            using WriteableBitmap frame = Window.CaptureRenderedFrame()
                ?? throw new AssertionException("The actual context-menu owner must render.");
        }

        private GitRef Ref(string name) => new(Module, Revisions[0].ObjectId, name);

        public void Dispose()
        {
            Menu.Close();
            Grid.CancelBackgroundTasks();
            Grid.GetTestAccessor().SetRevisions([]);
            Window.Close();
            Dispatcher.UIThread.RunJobs();
            foreach (KeyValuePair<string, string?> setting in _settings)
            {
                AppSettings.SettingsContainer.SetValue(setting.Key, setting.Value);
            }
        }
    }
}
