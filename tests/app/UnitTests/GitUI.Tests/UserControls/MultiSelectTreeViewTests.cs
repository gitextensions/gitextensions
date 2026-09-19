using GitUI.UserControls;

namespace GitUITests.UserControls;

[Apartment(ApartmentState.STA)]
public class MultiSelectTreeViewTests
{
    private MultiSelectTreeView _treeView;
    private TreeNode _node;

    [SetUp]
    public void Setup()
    {
        _treeView = new MultiSelectTreeView();
        _node = _treeView.Nodes.Add("a file");
        _treeView.SetSelectedNodes([_node], _node);
    }

    [TearDown]
    public void TearDown()
    {
        _treeView.Dispose();
    }

    [Test]
    public void OnMouseDown_must_clear_the_selection_when_no_item_was_clicked()
    {
        RaiseMouseDown(MouseButtons.Left);

        _treeView.SelectedNodes.Should().BeEmpty();
    }

    [Test]
    public void OnMouseDown_must_notify_that_the_selection_was_cleared()
    {
        int changes = 0;
        _treeView.SelectedNodesChanged += (s, e) => changes++;

        RaiseMouseDown(MouseButtons.Left);

        changes.Should().Be(1, "the consumers have to learn that nothing is selected any more");
    }

    [Test]
    public void OnMouseDown_must_keep_the_selection_for_the_right_mouse_button()
    {
        // The context menu acts on the selection, so a right click must not clear it
        RaiseMouseDown(MouseButtons.Right);

        _treeView.SelectedNodes.Should().BeEquivalentTo([_node]);
    }

    private void RaiseMouseDown(MouseButtons button)
        => _treeView.GetTestAccessor().OnMouseDown(new MouseEventArgs(button, clicks: 1, x: 5, y: 5000, delta: 0));
}
