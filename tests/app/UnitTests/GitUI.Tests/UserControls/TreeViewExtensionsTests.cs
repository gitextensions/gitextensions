using GitUI.UserControls;

namespace GitUITests.UserControls;
public class TreeViewExtensionsTests
{
    private TreeView _treeView = null!;
    private TreeNode _root = null!;
    private TreeNode _a = null!;
    private TreeNode _b = null!;
    private TreeNode _c = null!;
    private TreeNode _b1 = null!;
    private TreeNode _b2 = null!;
    private TreeNode _b3 = null!;
    private TreeNode _b2_1 = null!;

    [SetUp]
    public void Setup()
    {
        _treeView = new TreeView();

        _root = _treeView.Nodes.Add("Root");

        _a = _root.Nodes.Add("A");
        _b = _root.Nodes.Add("B");
        _c = _root.Nodes.Add("C");

        _b1 = _b.Nodes.Add("B_1");
        _b2 = _b.Nodes.Add("B_2");
        _b3 = _b.Nodes.Add("B_3");

        _b2_1 = _b2.Nodes.Add("B_2_1");
    }

    [TearDown]
    public void TearDown()
    {
        _treeView.Dispose();
    }

    [Test]
    public void GetExpandedNodesState_should_return_no_expanded_nodes()
    {
        HashSet<string> expandedNodes = _root.GetExpandedNodesState();
        expandedNodes.Count.Should().Be(0);
    }

    [Test]
    public void GetExpandedNodesState_should_return_one_expanded_node()
    {
        _b.Expand();
        HashSet<string> expandedNodes = _root.GetExpandedNodesState();
        expandedNodes.Count.Should().Be(1);
        expandedNodes.Contains(@"Root\B").Should().Be(true);
    }

    [Test]
    public void GetExpandedNodesState_should_return_all_expanded_nodes()
    {
        _root.ExpandAll();
        HashSet<string> expandedNodes = _root.GetExpandedNodesState();
        expandedNodes.Count.Should().Be(8);
    }

    [Test]
    public void RestoreExpandedNodesState_should_restore_no_nodes()
    {
        HashSet<string> expandedNodes = [];
        _root.RestoreExpandedNodesState(expandedNodes);

        HashSet<string> expandedNodesPost = _root.GetExpandedNodesState();
        expandedNodesPost.Count.Should().Be(0);
    }

    [Test]
    public void RestoreExpandedNodesState_should_restore_one_node()
    {
        HashSet<string> expandedNodes = [@"Root\B"];
        _root.RestoreExpandedNodesState(expandedNodes);

        HashSet<string> expandedNodesPost = _root.GetExpandedNodesState();
        expandedNodesPost.Count.Should().Be(1);
        expandedNodes.Contains(@"Root\B").Should().Be(true);
    }

    [Test]
    public void RestoreExpandedNodesState_should_restore_all_nodes()
    {
        HashSet<string> expandedNodes =
        [
            $"{_root.GetFullNamePath()}",
            $"{_a.GetFullNamePath()}",
            $"{_b.GetFullNamePath()}",
            $"{_b1.GetFullNamePath()}",
            $"{_b2.GetFullNamePath()}",
            $"{_b2_1.GetFullNamePath()}",
            $"{_b3.GetFullNamePath()}",
            $"{_c.GetFullNamePath()}",
        ];

        _root.RestoreExpandedNodesState(expandedNodes);

        HashSet<string> expandedNodesPost = _root.GetExpandedNodesState();
        expandedNodesPost.Count.Should().Be(8);
        expandedNodesPost.Should().BeEquivalentTo(expandedNodes);
    }

    [Test]
    public void GetFullNamePath_should_return_node_names_if_set()
    {
        // Only override the names for 2 of the 4 nodes
        _b.Name = "B_Name";
        _b2_1.Name = "B_2_1_Name";
        _b2_1.GetFullNamePath().Should().Be(@"Root\B_Name\B_2\B_2_1_Name");
    }
}
