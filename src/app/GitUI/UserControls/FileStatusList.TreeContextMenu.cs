using GitUI.Properties;
using GitUI.UserControls;

namespace GitUI;

partial class FileStatusList
{
    private ToolStripMenuItem _collapseAll = new("C&ollapse all", Images.CollapseAll);
    private ToolStripMenuItem _collapseRootFolders = new("Collap&se root folders", Images.TreeCollapseAll);
    private ToolStripMenuItem _expandAll = new("E&xpand all", Images.ExpandAll);
    private ToolStripMenuItem _selectAll = new("S&elect all", Images.FileTree);
    private ToolStripSeparator _treeContextMenuSeparator = new() { Name = nameof(_treeContextMenuSeparator) };

    private void CreateTreeContextMenuItems()
    {
        _selectAll.Click += SelectAll_Click;
        _collapseAll.Click += CollapseAll_Click;
        _expandAll.Click += ExpandAll_Click;
        _collapseRootFolders.Click += CollapseRootFolders_Click;
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
        items.Add(_collapseRootFolders);
    }

    private void UpdateStatusOfTreeContextMenuItems()
    {
        bool hasSubnodes = FileStatusListView.SelectedNodes.Any(node => node.Nodes.Count > 0);

        // in the commit window the selection is usually a FILE (no children), which used
        // to hide the tree commands entirely; also offer them when any root has children
        bool rootsHaveSubnodes = FileStatusListView.Nodes.Cast<TreeNode>().Any(node => node.Nodes.Count > 0);
        hasSubnodes = hasSubnodes || rootsHaveSubnodes;

        _collapseAll.Visible = hasSubnodes;
        _expandAll.Visible = hasSubnodes;
        _selectAll.Visible = hasSubnodes;
        _treeContextMenuSeparator.Visible = hasSubnodes;

        _collapseRootFolders.Visible = _isFileTreeMode && FileStatusListView.Nodes.Cast<TreeNode>().Any(node => node.IsExpanded);
    }

    private void CollapseAll_Click(object? sender, EventArgs e)
    {
        // collapse the selection; when it holds no subnodes (typical in the commit
        // window where a file is selected), collapse the whole tree instead
        if (FileStatusListView.SelectedNodes.Any(node => node.Nodes.Count > 0))
        {
            foreach (TreeNode node in FileStatusListView.SelectedNodes)
            {
                node.Collapse(ignoreChildren: false);
            }
        }
        else
        {
            foreach (TreeNode node in FileStatusListView.Nodes)
            {
                node.Collapse(ignoreChildren: false);
            }
        }
    }

    private void CollapseRootFolders_Click(object? sender, EventArgs e)
    {
        if (FileStatusListView.FocusedNode?.Parent is TreeNode parent)
        {
            while (parent.Parent is not null)
            {
                parent = parent.Parent;
            }

            FileStatusListView.SelectedNode = parent;
        }

        foreach (TreeNode node in FileStatusListView.Nodes)
        {
            node.Collapse(ignoreChildren: true);
        }
    }

    private void ExpandAll_Click(object? sender, EventArgs e)
    {
        IEnumerable<TreeNode> targets = FileStatusListView.SelectedNodes.Any(node => node.Nodes.Count > 0)
            ? FileStatusListView.SelectedNodes
            : FileStatusListView.Nodes.Cast<TreeNode>();
        foreach (TreeNode node in targets)
        {
            ExpandAll(node);
        }
    }

    private void SelectAll_Click(object? sender, EventArgs e)
    {
        HashSet<TreeNode> selectedItems = [];
        IEnumerable<TreeNode> targets = FileStatusListView.SelectedNodes.Any(node => node.Nodes.Count > 0)
            ? FileStatusListView.SelectedNodes
            : FileStatusListView.Nodes.Cast<TreeNode>();
        foreach (TreeNode node in targets)
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
