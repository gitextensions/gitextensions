using GitUI.UserControls;

namespace GitUITests.UserControls;

[Apartment(ApartmentState.STA)]
public class MultiSelectTreeViewTests
{
    [TestCase(1, new[] { 4 }, new[] { 1, 2, 3, 4 }, false)]
    [TestCase(4, new[] { 1 }, new[] { 1, 2, 3, 4 }, false)]
    [TestCase(1, new[] { 4, 2 }, new[] { 1, 2 }, false)]
    [TestCase(1, new[] { 4, 1 }, new[] { 1 }, false)]
    [TestCase(2, new[] { 5 }, new[] { 1, 2, 3 }, true)]
    public void Dragging_the_mouse_should_select_the_range_unless_dragging_a_selected_node(int mouseDownIndex, int[] mouseMoveIndices, int[] expectedSelection, bool startsDragAndDrop)
    {
        using Form form = new() { ClientSize = new Size(300, 300) };
        using ImageList imageList = new() { ImageSize = new Size(16, 16) };
        using TestTreeView treeView = new() { Dock = DockStyle.Fill, FullRowSelect = true, ImageList = imageList, ShowRootLines = false };
        form.Controls.Add(treeView);
        for (int i = 0; i < 6; ++i)
        {
            treeView.Nodes.Add($"file{i}.txt");
        }

        _ = treeView.Handle;
        if (startsDragAndDrop)
        {
            treeView.SetSelectedNodes([treeView.Nodes[1], treeView.Nodes[2], treeView.Nodes[3]], treeView.Nodes[1]);
        }

        int mouseDownEvents = 0;
        treeView.MouseDown += (_, _) => ++mouseDownEvents;

        treeView.MouseDownOn(mouseDownIndex);
        foreach (int index in mouseMoveIndices)
        {
            treeView.MouseMoveTo(index);
        }

        treeView.SelectedNodes.Select(node => node.Index).Should().BeEquivalentTo(expectedSelection);
        if (!startsDragAndDrop)
        {
            treeView.FocusedNode!.Index.Should().Be(mouseMoveIndices[^1]);
        }

        // FileStatusList starts drag-and-drop from the MouseDown event
        mouseDownEvents.Should().Be(startsDragAndDrop ? 1 : 0);
    }

    private sealed class TestTreeView : MultiSelectTreeView
    {
        public void MouseDownOn(int index)
            => OnMouseDown(new MouseEventArgs(MouseButtons.Left, clicks: 1, x: ClientSize.Width / 2, y: GetY(index), delta: 0));

        public void MouseMoveTo(int index)
            => OnMouseMove(new MouseEventArgs(MouseButtons.Left, clicks: 0, x: ClientSize.Width / 2, y: GetY(index), delta: 0));

        private int GetY(int index)
        {
            Rectangle bounds = Nodes[index].Bounds;
            return bounds.Top + (bounds.Height / 2);
        }
    }
}
