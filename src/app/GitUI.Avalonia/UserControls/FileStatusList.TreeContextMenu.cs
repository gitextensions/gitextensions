using Avalonia.Controls;

namespace GitUI;

partial class FileStatusList
{
    private void CreateTreeContextMenuItems()
    {
        _selectAll.Click += SelectAll_Click;
        _collapseAll.Click += CollapseAll_Click;
        _expandAll.Click += ExpandAll_Click;
        _collapseRootFolders.Click += CollapseRootFolders_Click;
    }

    private void InsertTreeContextMenuItems(System.Collections.IList items, int index)
    {
        if (items.Contains(_treeContextMenuSeparator))
        {
            return;
        }

        items.Insert(index++, _selectAll);
        items.Insert(index++, _collapseAll);
        items.Insert(index++, _expandAll);
        items.Insert(index, _treeContextMenuSeparator);
        items.Add(_collapseRootFolders);
    }

    private void UpdateStatusOfTreeContextMenuItems()
    {
        bool hasSubnodes = GetSelectedTreeNodes().Any(HasChildren);

        _collapseAll.IsVisible = hasSubnodes;
        _expandAll.IsVisible = hasSubnodes;
        _selectAll.IsVisible = hasSubnodes;
        _treeContextMenuSeparator.IsVisible = hasSubnodes;

        _collapseRootFolders.IsVisible = _isFileTreeMode && tvFiles.Items.Cast<FileTreeNode>().Any(node => node.IsExpanded);
    }

    private void CollapseAll_Click(object? sender, EventArgs e)
    {
        foreach (object node in GetSelectedTreeNodes())
        {
            SetExpansion(node, expanded: false);
        }
    }

    private void CollapseRootFolders_Click(object? sender, EventArgs e)
    {
        if (tvFiles.SelectedItem is FileTreeNode selected)
        {
            while (selected.Parent is not null)
            {
                selected = selected.Parent;
            }

            tvFiles.SelectedItem = selected;
        }

        foreach (FileTreeNode node in tvFiles.Items.Cast<FileTreeNode>())
        {
            node.IsExpanded = false;
        }
    }

    private void ExpandAll_Click(object? sender, EventArgs e)
    {
        foreach (object node in GetSelectedTreeNodes())
        {
            SetExpansion(node, expanded: true);
        }
    }

    private void SelectAll_Click(object? sender, EventArgs e)
    {
        TreeView tree = _isFileTreeMode ? tvFiles : tvDiffFiles;
        object[] selectedNodes = [.. GetSelectedTreeNodes()];
        HashSet<object> selectedItems = [];
        foreach (object node in selectedNodes)
        {
            SetExpansion(node, expanded: true);
            foreach (object leaf in DescendantsAndSelf(node).Where(IsFileNode))
            {
                selectedItems.Add(leaf);
            }
        }

        tree.SelectedItems?.Clear();
        foreach (object item in selectedItems)
        {
            tree.SelectedItems?.Add(item);
        }
    }

    private IEnumerable<object> GetSelectedTreeNodes()
        => _isFileTreeMode
            ? tvFiles.SelectedItems?.Cast<object>() ?? []
            : _showDiffGroups
                ? tvDiffFiles.SelectedItems?.Cast<object>() ?? []
                : [];

    private static bool HasChildren(object node)
        => node switch
        {
            FileTreeNode fileNode => fileNode.Children.Count > 0,
            DiffTreeNode diffNode => diffNode.Children.Count > 0,
            _ => false
        };

    private static bool IsFileNode(object node)
        => node switch
        {
            FileTreeNode fileNode => fileNode.Item is not null,
            DiffTreeNode diffNode => diffNode.Item is not null,
            _ => false
        };

    private static IEnumerable<object> DescendantsAndSelf(object node)
    {
        yield return node;
        IEnumerable<object> children = node switch
        {
            FileTreeNode fileNode => fileNode.Children,
            DiffTreeNode diffNode => diffNode.Children,
            _ => []
        };
        foreach (object child in children)
        {
            foreach (object descendant in DescendantsAndSelf(child))
            {
                yield return descendant;
            }
        }
    }

    private static void SetExpansion(object node, bool expanded)
    {
        switch (node)
        {
            case FileTreeNode fileNode:
                if (fileNode.Children.Count > 0)
                {
                    fileNode.IsExpanded = expanded;
                }

                foreach (FileTreeNode child in fileNode.Children)
                {
                    SetExpansion(child, expanded);
                }

                break;

            case DiffTreeNode diffNode:
                if (diffNode.Children.Count > 0)
                {
                    diffNode.IsExpanded = expanded;
                }

                foreach (DiffTreeNode child in diffNode.Children)
                {
                    SetExpansion(child, expanded);
                }

                break;
        }
    }
}
