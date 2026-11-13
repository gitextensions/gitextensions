using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUI;
using GitUI.LeftPanel;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

/// <summary>
///  Exercises the source tree's logical-selection actions and ref-only commands through actual menu items.
/// </summary>
[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class RepoObjectsTreeSourceActionTests
{
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
    public void TearDown()
    {
        try
        {
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            ThreadHelper.JoinableTaskContext = _previousContext!;
            _context.Dispose();
        }
    }

    [AvaloniaTest]
    [TestCase(true, false)]
    [TestCase(true, true)]
    [TestCase(false, false)]
    [TestCase(false, true)]
    public void Expand_and_Collapse_should_act_on_all_logically_selected_parents_not_the_caret(bool expand, bool independentLeafCaret)
    {
        WithIsolatedSettings(() =>
        {
            RepoObjectsTree control = new();
            try
            {
                control.SetRefs(
                    [CreateRef("refs/heads/alpha/one"), CreateRef("refs/heads/alpha/nested/two"),
                     CreateRef("refs/heads/beta/one"), CreateRef("refs/heads/beta/nested/two"), CreateRef("refs/heads/untouched/one")], []);
                RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();
                TreeViewItem branches = accessor.TreeView.Items.Cast<TreeViewItem>().Single(item => item.Tag is LocalBranchTree);
                TreeViewItem alpha = Child(branches, "alpha");
                TreeViewItem beta = Child(branches, "beta");
                TreeViewItem untouched = Child(branches, "untouched");
                TreeViewItem alphaNested = Child(alpha, "nested");
                TreeViewItem betaNested = Child(beta, "nested");
                accessor.SelectNode<BranchPathNode>([HeaderText(branches), "alpha"]);
                accessor.SelectNode<BranchPathNode>([HeaderText(branches), "beta"], multiple: true);
                TreeViewItem caret = independentLeafCaret ? Child(untouched, "one") : beta;
                accessor.TreeView.SelectedItem = caret;
                foreach (TreeViewItem item in new[] { alpha, beta, alphaNested, betaNested, untouched })
                {
                    item.IsExpanded = !expand;
                }

                TreeViewItem[] selection = [.. accessor.LogicalSelection];
                selection.Should().BeEquivalentTo([alpha, beta]);
                accessor.OpenContextMenu().Should().BeTrue();
                MenuItem action = accessor.GetActionMenuItem(expand ? "Expand" : "Collapse");
                action.IsVisible.Should().BeTrue();
                action.IsEnabled.Should().BeTrue();
                Click(action);

                foreach (TreeViewItem item in new[] { alpha, beta, alphaNested, betaNested })
                {
                    item.IsExpanded.Should().Be(expand);
                }

                untouched.IsExpanded.Should().Be(!expand, "an unselected parent must not participate in the action");
                accessor.LogicalSelection.Should().BeEquivalentTo(selection);
                accessor.TreeView.SelectedItem.Should().BeSameAs(caret);
            }
            finally
            {
                DisposeRoots(control);
            }
        });
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Branch_folder_ref_only_actions_should_remain_available_in_bare_repositories(bool bare)
    {
        WithIsolatedSettings(() =>
        {
            IGitUICommands commands = CreateCommands(bare);
            RepoObjectsTree control = CreateControl(commands);
            try
            {
                control.SetRefs([CreateRef("refs/heads/features/one"), CreateRef("refs/heads/features/two"), CreateRef("refs/heads/other")], []);
                RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();
                TreeViewItem branches = accessor.TreeView.Items.Cast<TreeViewItem>().Single(item => item.Tag is LocalBranchTree);
                accessor.SelectNode<BranchPathNode>([HeaderText(branches), "features"]);
                accessor.OpenContextMenu().Should().BeTrue();
                MenuItem create = accessor.GetActionMenuItem("CreateInFolder");
                MenuItem delete = accessor.GetActionMenuItem("DeleteFolderBranches");
                create.IsVisible.Should().BeTrue();
                create.IsEnabled.Should().BeTrue();
                delete.IsVisible.Should().BeTrue();
                delete.IsEnabled.Should().BeTrue();
                commands.ClearReceivedCalls();
                Click(create);
                Click(delete);

                commands.Received(1).StartCreateBranchDialog(control, objectId: default, newBranchNamePrefix: "features/");
                commands.Received(1).StartDeleteBranchDialog(control,
                    Arg.Is<IEnumerable<string>>(names => names.OrderBy(name => name).SequenceEqual(new[] { "features/one", "features/two" })));
            }
            finally
            {
                DisposeRoots(control);
            }
        });
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void FetchCreate_should_allow_ref_only_bare_operations_and_require_a_completed_fetch(bool bare, bool fetchCompleted)
    {
        WithIsolatedSettings(() =>
        {
            IGitUICommands commands = CreateCommands(bare);
            RepoObjectsTree control = CreateControl(commands);
            try
            {
                commands.StartPullDialogAndPullImmediately(out Arg.Any<bool>(), control,
                    remoteBranch: "topic", remote: "origin", pullAction: GitPullAction.Fetch)
                    .Returns(call =>
                    {
                        call[0] = fetchCompleted;
                        return fetchCompleted;
                    });
                control.SetRefs([CreateRef("refs/remotes/origin/topic", "origin")], []);
                RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();
                TreeViewItem remotes = accessor.TreeView.Items.Cast<TreeViewItem>().Single(item => item.Tag is RemoteBranchTree);
                accessor.SelectNode<RemoteBranchNode>([HeaderText(remotes), "origin", "topic"]);
                accessor.OpenContextMenu().Should().BeTrue();
                MenuItem create = accessor.GetActionMenuItem("FetchCreate");
                create.IsVisible.Should().BeTrue();
                create.IsEnabled.Should().BeTrue();
                accessor.GetActionMenuItem("FetchBranch").IsEnabled.Should().BeTrue();
                foreach (string action in new[] { "FetchMerge", "FetchCheckout", "FetchRebase" })
                {
                    accessor.GetActionMenuItem(action).IsVisible.Should().BeTrue();
                    accessor.GetActionMenuItem(action).IsEnabled.Should().Be(!bare);
                }

                commands.ClearReceivedCalls();
                Click(create);

                commands.Received(1).StartPullDialogAndPullImmediately(out Arg.Any<bool>(), control,
                    remoteBranch: "topic", remote: "origin", pullAction: GitPullAction.Fetch);
                commands.Received(fetchCompleted ? 1 : 0).StartCreateBranchDialog(control, "origin/topic");
                commands.DidNotReceiveWithAnyArgs().StartMergeBranchDialog(default, default!);
                commands.DidNotReceiveWithAnyArgs().StartCheckoutRemoteBranch(default, default!);
                commands.DidNotReceiveWithAnyArgs().StartRebaseDialog(default, default!);
            }
            finally
            {
                DisposeRoots(control);
            }
        });
    }

    private static TreeViewItem Child(TreeViewItem parent, string text)
        => parent.Items.Cast<TreeViewItem>().Single(item => HeaderText(item) == text);

    private static void Click(MenuItem item)
        => item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

    private static IGitUICommands CreateCommands(bool bare)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.IsBareRepository().Returns(bare);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        return commands;
    }

    private static RepoObjectsTree CreateControl(IGitUICommands commands)
    {
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        source.UICommands.Returns(commands);
        return new RepoObjectsTree { UICommandsSource = source };
    }

    private static GitRef CreateRef(string completeName, string remote = "")
        => new(Substitute.For<IGitModule>(), ObjectId.Random(), completeName, remote);

    private static void DisposeRoots(RepoObjectsTree control)
    {
        FieldInfo field = typeof(RepoObjectsTree).GetField("_rootNodes", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new AssertionException("The actual tree owners must exist.");
        foreach (Tree root in field.GetValue(control) as IEnumerable<Tree> ?? [])
        {
            root.Dispose();
        }

        Dispatcher.UIThread.RunJobs();
    }

    private static string HeaderText(TreeViewItem item)
        => ((StackPanel)item.Header!).Children.OfType<TextBlock>().Single().Text ?? string.Empty;

    private static void WithIsolatedSettings(Action action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "GitExtensions.TreeSourceActions", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            using GitExtSettingsCache cache = new(Path.Combine(directory, "capture.settings"), autoSave: false);
            DistributedSettings settings = new(lowerPriority: null, cache, SettingLevel.Unknown);
            AppSettings.UsingContainer(settings, () =>
            {
                AppSettings.RepoObjectsTreeShowBranches = true;
                AppSettings.RepoObjectsTreeShowRemotes = true;
                action();
            });
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
