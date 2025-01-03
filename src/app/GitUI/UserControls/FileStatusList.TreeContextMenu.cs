using GitUI.Properties;
using GitUI.UserControls;

namespace GitUI;

partial class FileStatusList
{
    private ToolStripMenuItem _collapse = new("&Collapse", Images.CollapseAll);
    private ToolStripMenuItem _expand = new("&Expand", Images.ExpandAll);
    private ToolStripMenuItem _selectAll = new("&Select all", Images.FileTree);
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
        bool hasSubnodes = FileStatusListView.LastSelectedNode?.Nodes.Count is > 0;

        _collapse.Visible = hasSubnodes;
        _expand.Visible = hasSubnodes;
        _selectAll.Visible = hasSubnodes;
        _treeContextMenuSeparator.Visible = hasSubnodes;

        _collapse.Enabled = hasSubnodes && FileStatusListView.LastSelectedNode.IsExpanded;
        _expand.Enabled = hasSubnodes;
        _selectAll.Enabled = hasSubnodes;
    }

    private void Collapse_Click(object sender, EventArgs e)
    {
        FileStatusListView.LastSelectedNode?.Collapse(ignoreChildren: false);
    }

    private void Expand_Click(object sender, EventArgs e)
    {
        TreeNode? node = FileStatusListView.LastSelectedNode;
        if (node is not null)
        {
            node.ExpandAll();
            FileStatusListView.FocusedNode = node;
        }
    }

    private void SelectAll_Click(object sender, EventArgs e)
    {
        TreeNode? node = FileStatusListView.LastSelectedNode;
        if (node is null)
        {
            return;
        }

        node.ExpandAll();
        FileStatusListView.SelectedNodes = node.Items().Where(node => node.Tag is FileStatusItem).ToHashSet();
        FileStatusListView.FocusedNode = node;
    }
}
