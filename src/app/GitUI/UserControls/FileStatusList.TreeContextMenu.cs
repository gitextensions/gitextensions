#nullable enable

using GitUI.Properties;
using GitUI.UserControls;

namespace GitUI;

partial class FileStatusList
{
    private ToolStripMenuItem _collapse = new("C&ollapse all", Images.CollapseAll);
    private ToolStripMenuItem _expand = new("E&xpand all", Images.ExpandAll);
    private ToolStripMenuItem _selectAll = new("S&elect all", Images.FileTree);
    private ToolStripSeparator _treeContextMenuSeparator = new() { Name = nameof(_treeContextMenuSeparator) };

    private void CreateTreeContextMenuItems()
    {
        _selectAll.Click += SelectAll_Click;
        _collapse.Click += Collapse_Click;
        _expand.Click += Expand_Click;
    }

    private void InsertTreeContextMenuItems(ToolStripItemCollection items, int index)
    {
        if (items.Find(_treeContextMenuSeparator.Name!, searchAllChildren: false).Length > 0)
        {
            return;
        }

        items.Insert(index++, _selectAll);
        items.Insert(index++, _collapse);
        items.Insert(index++, _expand);
        items.Insert(index++, _treeContextMenuSeparator);
    }

    private void UpdateStatusOfTreeContextMenuItems()
    {
        bool hasSubnodes = FileStatusListView.FocusedNode?.Nodes.Count is > 0;

        _collapse.Visible = hasSubnodes;
        _expand.Visible = hasSubnodes;
        _selectAll.Visible = hasSubnodes;
        _treeContextMenuSeparator.Visible = hasSubnodes;
    }

    private void Collapse_Click(object? sender, EventArgs e)
    {
        FileStatusListView.FocusedNode?.Collapse(ignoreChildren: false);
    }

    private void Expand_Click(object? sender, EventArgs e)
    {
        if (FileStatusListView.FocusedNode is not TreeNode node)
        {
            return;
        }

        node.ExpandAll();
        FileStatusListView.FocusedNode = node;
    }

    private void SelectAll_Click(object? sender, EventArgs e)
    {
        if (FileStatusListView.FocusedNode is not TreeNode node)
        {
            return;
        }

        node.ExpandAll();
        FileStatusListView.SetSelectedNodes(node.Items().Where(node => node.Tag is FileStatusItem).ToHashSet(), focusedNode: node);
    }
}
