using Avalonia.Controls;
using GitUI.LeftPanel.Interfaces;

namespace GitUI.LeftPanel;

partial class RepoObjectsTree
{
    private static void EnableMenuItems(bool enabled, params MenuItem[] items)
    {
        foreach (MenuItem item in items)
        {
            item.IsVisible = enabled;
            item.IsEnabled = enabled;
        }
    }

    /* add Expand All / Collapse All menu entry
     * depending on whether node is expanded or collapsed and has child nodes at all */
    private void EnableExpandCollapseContextMenu(NodeBase[] selectedNodes)
    {
        NodeBase[] multiSelectedParents = [.. selectedNodes.HavingChildren()];
        SetAction(RepoAction.Expand, multiSelectedParents.Length > 0, multiSelectedParents.Expandable().Any());
        SetAction(RepoAction.Collapse, multiSelectedParents.Length > 0, multiSelectedParents.Collapsible().Any());
    }

    private void EnableMoveTreeUpDownContexMenu(bool hasSingleSelection, NodeBase? selectedNode)
    {
        Tree[] visibleTrees = [.. _trees.Where(tree => tree.IsEnabled).OrderBy(tree => tree.PositionIndex)];
        int index = selectedNode is Tree tree ? Array.IndexOf(visibleTrees, tree) : -1;
        SetAction(RepoAction.MoveUp, hasSingleSelection && index >= 0, index > 0);
        SetAction(RepoAction.MoveDown, hasSingleSelection && index >= 0, index < visibleTrees.Length - 1);
    }

    private static void RegisterClick(MenuItem item, Action onClick)
    {
        item.Click += (o, e) => onClick();
    }

    private void RegisterClick<T>(MenuItem item, Action<T> onClick) where T : class, INode
    {
        item.Click += (o, e) => Node.OnNode(treeMain.SelectedItem as TreeViewItem, onClick);
    }
}
