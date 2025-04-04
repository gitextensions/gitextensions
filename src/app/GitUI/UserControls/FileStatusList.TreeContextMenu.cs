#nullable enable

using GitUI.Properties;
using GitUI.UserControls;

namespace GitUI;

partial class FileStatusList
{
    private ToolStripMenuItem _collapseAll = new("C&ollapse all", Images.CollapseAll);
    private ToolStripMenuItem _expandAll = new("E&xpand all", Images.ExpandAll);
    private ToolStripMenuItem _selectAll = new("S&elect all", Images.FileTree);
    private ToolStripSeparator _treeContextMenuSeparator = new() { Name = nameof(_treeContextMenuSeparator) };

    private void CreateTreeContextMenuItems()
    {
        _selectAll.Click += SelectAll_Click;
        _collapseAll.Click += CollapseAll_Click;
        _expandAll.Click += ExpandAll_Click;
    }

    private void InsertTreeContextMenuItems(ToolStripItemCollection items, int index)
    {
        if (items.Find(_treeContextMenuSeparator.Name!, searchAllChildren: false).Length > 0)
        {
            return;
        }

        items.Insert(index++, _selectAll);
        items.Insert(index++, _collapseAll);
        items.Insert(index++, _expandAll);
        items.Insert(index++, _treeContextMenuSeparator);
    }

    private void UpdateStatusOfTreeContextMenuItems()
    {
        bool hasSubnodes = FileStatusListView.SelectedNodes.Any(node => node.Nodes.Count > 0);

        _collapseAll.Visible = hasSubnodes;
        _expandAll.Visible = hasSubnodes;
        _selectAll.Visible = hasSubnodes;
        _treeContextMenuSeparator.Visible = hasSubnodes;
    }

    private void CollapseAll_Click(object? sender, EventArgs e)
    {
        foreach (TreeNode node in FileStatusListView.SelectedNodes)
        {
            node.Collapse(ignoreChildren: false);
        }
    }

    private void ExpandAll_Click(object? sender, EventArgs e)
    {
        foreach (TreeNode node in FileStatusListView.SelectedNodes)
        {
            ExpandAll(node);
        }
    }

    private void SelectAll_Click(object? sender, EventArgs e)
    {
        HashSet<TreeNode> selectedItems = [];
        foreach (TreeNode node in FileStatusListView.SelectedNodes)
        {
            ExpandAll(node);
            foreach (TreeNode leaf in node.Items().Where(node => node.Tag is FileStatusItem))
            {
                selectedItems.Add(leaf);
            }
        }

        FileStatusListView.SetSelectedNodes(selectedItems, focusedNode: FileStatusListView.FocusedNode);
    }
}
