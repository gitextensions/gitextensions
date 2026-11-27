using GitUI;
using GitUI.LeftPanel;
using NSubstitute;

namespace GitUITests.LeftPanel;

[Apartment(ApartmentState.STA)]
public sealed class NodesTests
{
    private static readonly Color TreeViewForeColor = Color.Crimson;

    private TreeView _treeView = null!;
    private TestTree _tree = null!;

    [SetUp]
    public void Setup()
    {
        _treeView = new TreeView { ForeColor = TreeViewForeColor };

        // The real left panel only builds nodes for a tree view with a window handle, which is what makes
        // adding nodes and changing their properties expensive.
        _ = _treeView.Handle;

        _tree = new TestTree(_treeView.Nodes.Add("Root"), Substitute.For<IGitUICommandsSource>());
    }

    [TearDown]
    public void TearDown()
    {
        _tree.Dispose();
        _treeView.Dispose();
    }

    [Test]
    public void FillTreeViewNode_should_build_the_tree_view_nodes_in_order()
    {
        TestNode folder = AddNode(_tree.Nodes, "folder");
        AddNode(folder.Nodes, "folder/first");
        TestNode second = AddNode(folder.Nodes, "folder/second");
        AddNode(second.Nodes, "folder/second/leaf");
        AddNode(_tree.Nodes, "other");

        Fill();

        TreeNode root = _tree.TreeViewNode;
        root.Nodes.Cast<TreeNode>().Select(node => node.Text).Should().Equal("folder", "other");
        root.Nodes[0].Nodes.Cast<TreeNode>().Select(node => node.Text).Should().Equal("first", "second");
        root.Nodes[0].Nodes[1].Nodes.Cast<TreeNode>().Select(node => node.Text).Should().Equal("leaf");
        root.Nodes[0].Nodes[1].Nodes[0].FullPath.Should().Be(@"Root\folder\second\leaf");
        root.Nodes[0].Tag.Should().BeSameAs(folder);
        root.Nodes[0].Name.Should().Be("folder");
    }

    [Test]
    public void FillTreeViewNode_should_style_new_nodes_before_they_are_attached_to_the_tree_view()
    {
        TestNode folder = AddNode(_tree.Nodes, "folder");
        TestNode leaf = AddNode(folder.Nodes, "folder/leaf");

        Fill();

        // Styling a node that already belongs to a tree view with a handle is a message to the native control per property.
        folder.WasAttachedWhenStyled.Should().BeFalse();
        leaf.WasAttachedWhenStyled.Should().BeFalse();
        leaf.TreeViewNode.TreeView.Should().BeSameAs(_treeView, "because the finished subtree is attached afterwards");
    }

    [Test]
    public void FillTreeViewNode_should_restyle_recycled_nodes_in_place()
    {
        TestNode first = AddNode(_tree.Nodes, "first");
        Fill();
        TreeNode firstTreeNode = first.TreeViewNode;

        _tree.Nodes.Clear();
        TestNode recreated = AddNode(_tree.Nodes, "first");
        Fill();

        recreated.TreeViewNode.Should().BeSameAs(firstTreeNode, "because equal nodes recycle their TreeNode");
        recreated.WasAttachedWhenStyled.Should().BeTrue();
        _tree.TreeViewNode.Nodes.Count.Should().Be(1);
        firstTreeNode.Tag.Should().BeSameAs(recreated);
    }

    [Test]
    public void FillTreeViewNode_should_insert_new_nodes_between_recycled_ones_and_remove_stale_ones()
    {
        TestNode a = AddNode(_tree.Nodes, "a");
        TestNode c = AddNode(_tree.Nodes, "c");
        Fill();
        TreeNode treeNodeA = a.TreeViewNode;
        TreeNode treeNodeC = c.TreeViewNode;

        _tree.Nodes.Clear();
        AddNode(_tree.Nodes, "a");
        TestNode b = AddNode(_tree.Nodes, "b");
        AddNode(_tree.Nodes, "c");
        Fill();

        TreeNode root = _tree.TreeViewNode;
        root.Nodes.Cast<TreeNode>().Select(node => node.Text).Should().Equal("a", "b", "c");
        root.Nodes[0].Should().BeSameAs(treeNodeA);
        root.Nodes[2].Should().BeSameAs(treeNodeC);
        b.WasAttachedWhenStyled.Should().BeFalse("because b is new, it is built before it is inserted");

        _tree.Nodes.Clear();
        AddNode(_tree.Nodes, "a");
        Fill();

        root.Nodes.Cast<TreeNode>().Select(node => node.Text).Should().Equal("a");
    }

    [Test]
    public void FillTreeViewNode_should_colour_visible_nodes_like_the_tree_view_and_invisible_nodes_differently()
    {
        TestRevisionNode visible = new(_tree, "visible", visible: true);
        TestRevisionNode invisible = new(_tree, "invisible", visible: false);
        TestRevisionNode nested = new(_tree, "folder/nested", visible: true);
        TestRevisionNode folder = new(_tree, "folder", visible: true);
        folder.Nodes.AddNode(nested);
        _tree.Nodes.AddNode(visible);
        _tree.Nodes.AddNode(invisible);
        _tree.Nodes.AddNode(folder);

        Fill();

        // The colour comes from the tree view although the nodes are styled before they are attached to it.
        visible.TreeViewNode.ForeColor.Should().Be(TreeViewForeColor);
        nested.TreeViewNode.ForeColor.Should().Be(TreeViewForeColor);
        folder.TreeViewNode.ForeColor.Should().Be(TreeViewForeColor);
        invisible.TreeViewNode.ForeColor.Should().NotBe(TreeViewForeColor);
        invisible.TreeViewNode.ImageKey.Should().Be("EyeClosed");
    }

    private static TestNode AddNode(Nodes nodes, string fullPath)
    {
        TestNode node = new(fullPath);
        nodes.AddNode(node);

        return node;
    }

    private void Fill()
    {
        _tree.Nodes.FillTreeViewNode(_tree.TreeViewNode);
    }

    private sealed class TestTree : Tree
    {
        public TestTree(TreeNode treeNode, IGitUICommandsSource uiCommands)
            : base(treeNode, uiCommands)
        {
        }
    }

    /// <summary>A node equal to other nodes with the same path, remembering whether it was attached when it was styled.</summary>
    private sealed class TestNode : Node
    {
        private readonly string _fullPath;

        public TestNode(string fullPath)
            : base(tree: null)
        {
            _fullPath = fullPath;
        }

        public bool? WasAttachedWhenStyled { get; private set; }

        public override void ApplyStyle()
        {
            base.ApplyStyle();
            WasAttachedWhenStyled = TreeViewNode.TreeView is not null;
        }

        public override bool Equals(object? obj) => obj is TestNode other && other._fullPath == _fullPath;

        public override int GetHashCode() => _fullPath.GetHashCode();

        protected override string DisplayText() => _fullPath[(_fullPath.LastIndexOf('/') + 1)..];
    }

    private sealed class TestRevisionNode : BaseRevisionNode
    {
        public TestRevisionNode(Tree tree, string fullPath, bool visible)
            : base(tree, fullPath, visible)
        {
        }
    }
}
