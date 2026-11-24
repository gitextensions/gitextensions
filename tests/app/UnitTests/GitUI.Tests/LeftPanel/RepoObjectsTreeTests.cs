using GitCommands.Git;
using GitCommands.Submodules;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.LeftPanel;
using GitUI.UserControls.RevisionGrid;
using NSubstitute;

namespace GitUITests.LeftPanel;

[Apartment(ApartmentState.STA)]
public sealed class RepoObjectsTreeTests
{
    private RepoObjectsTree _tree = null!;
    private RepoObjectsTree.TestAccessor _testAccessor = default!;

    [SetUp]
    public void SetUp()
    {
        IGitModule module = Substitute.For<IGitModule>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(typeof(ISubmoduleStatusProvider)).Returns(Substitute.For<ISubmoduleStatusProvider>());
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        source.UICommands.Returns(commands);

        _tree = new RepoObjectsTree();
        _tree.UICommandsSource = source;
        _tree.Initialize(
            Substitute.For<IAheadBehindDataProvider>(),
            _ => { },
            Substitute.For<ICheckRefs>(),
            Substitute.For<IRevisionGridInfo>());
        _testAccessor = _tree.GetTestAccessor();

        // A tree view without a window handle defers setting its selected node,
        // which the context menu relies on.
        _ = _testAccessor.TreeView.Handle;
    }

    [TearDown]
    public void TearDown()
    {
        _tree.Dispose();
    }

    [Test]
    public void AddTree_should_add_the_bisect_tree_as_the_first_root_node()
    {
        _testAccessor.AddTree(_testAccessor.BisectTree);

        _testAccessor.TreeView.Nodes[0].Tag.Should().BeSameAs(_testAccessor.BisectTree);
        _testAccessor.TreeView.Nodes[0].Text.Should().Be(TranslatedStrings.Bisect);
    }

    [Test]
    public void AddTree_should_add_the_bisect_tree_first_and_the_other_trees_in_position_index_order()
    {
        _testAccessor.AddTree(_testAccessor.BisectTree);

        _testAccessor.TreeView.Nodes.Cast<TreeNode>().Select(node => node.Text).Should().Equal(
            TranslatedStrings.Bisect,
            TranslatedStrings.Branches,
            TranslatedStrings.Remotes,
            TranslatedStrings.Worktrees,
            TranslatedStrings.Tags,
            TranslatedStrings.Submodules,
            TranslatedStrings.Stashes);
    }

    [Test]
    public void ReorderTreeNode_should_not_move_the_bisect_tree_up_or_down()
    {
        _testAccessor.AddTree(_testAccessor.BisectTree);
        TreeNode bisectNode = _testAccessor.TreeView.Nodes[0];
        string[] orderBefore = GetNodeTexts();

        _testAccessor.ReorderTreeNode(bisectNode, up: true);
        _testAccessor.ReorderTreeNode(bisectNode, up: false);

        GetNodeTexts().Should().Equal(orderBefore);
    }

    [Test]
    public void ReorderTreeNode_should_not_move_a_tree_above_the_bisect_tree()
    {
        _testAccessor.AddTree(_testAccessor.BisectTree);
        TreeNode nodeBelowBisect = _testAccessor.TreeView.Nodes[1];
        string[] orderBefore = GetNodeTexts();

        _testAccessor.ReorderTreeNode(nodeBelowBisect, up: true);

        GetNodeTexts().Should().Equal(orderBefore);
    }

    [Test]
    public void context_menu_should_hide_move_up_and_down_for_the_bisect_tree()
    {
        _testAccessor.AddTree(_testAccessor.BisectTree);
        _testAccessor.SelectTree(_testAccessor.BisectTree);
        _testAccessor.OpenContextMenu();

        // Assert Enabled rather than Visible: a ToolStripItem's Visible always reads false
        // while the ContextMenuStrip is not shown, which is the case in unit tests.
        _testAccessor.ContextMenu.Items["mnubtnMoveUp"]!.Enabled.Should().BeFalse();
        _testAccessor.ContextMenu.Items["mnubtnMoveDown"]!.Enabled.Should().BeFalse();
    }

    [Test]
    public void context_menu_should_disable_move_up_for_the_tree_below_the_bisect_tree()
    {
        _testAccessor.AddTree(_testAccessor.BisectTree);
        Tree treeBelowBisect = (Tree)_testAccessor.TreeView.Nodes[1].Tag!;
        _testAccessor.SelectTree(treeBelowBisect);
        _testAccessor.OpenContextMenu();

        // Assert Enabled rather than Visible: a ToolStripItem's Visible always reads false
        // while the ContextMenuStrip is not shown, which is the case in unit tests.
        _testAccessor.ContextMenu.Items["mnubtnMoveUp"]!.Enabled.Should().BeFalse();
        _testAccessor.ContextMenu.Items["mnubtnMoveDown"]!.Enabled.Should().BeTrue();
    }

    private string[] GetNodeTexts()
        => _testAccessor.TreeView.Nodes.Cast<TreeNode>().Select(node => node.Text).ToArray();
}
