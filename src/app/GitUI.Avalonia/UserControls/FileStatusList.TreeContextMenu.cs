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

    private void InsertTreeContextMenuItems(IList<object> items, int index)
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
        bool isTree = _isFileTreeMode || _showDiffGroups;
        _collapseAll.IsVisible = isTree;
        _expandAll.IsVisible = isTree;
        _selectAll.IsVisible = isTree;
        _treeContextMenuSeparator.IsVisible = isTree;
        _collapseRootFolders.IsVisible = _isFileTreeMode;
    }

    private void CollapseAll_Click(object? sender, EventArgs e)
        => SetTreeExpansion(expanded: false, rootOnly: false);

    private void CollapseRootFolders_Click(object? sender, EventArgs e)
        => SetTreeExpansion(expanded: false, rootOnly: true);

    private void ExpandAll_Click(object? sender, EventArgs e)
        => SetTreeExpansion(expanded: true, rootOnly: false);

    private void SelectAll_Click(object? sender, EventArgs e)
        => SelectAll();
}
