using Avalonia.Controls;

namespace GitUI.LeftPanel;

partial class RepoObjectsTree
{
    /// <summary>
    /// We assume tree to position indices are 0-based and sequential. In case this
    /// is no longer true, because for e.g. user has reverted to an earlier version,
    /// this function will fix the indices, attempting to maintain the existing order.
    /// </summary>
    private void FixInvalidTreeToPositionIndices()
    {
        RepoTreeKind[] kinds = Enum.GetValues<RepoTreeKind>();
        RepoTreeKind[] orderedKinds =
        [
            .. kinds
                .OrderBy(GetTreePosition)
                .ThenBy(kind => kind),
        ];
        for (int i = 0; i < orderedKinds.Length; i++)
        {
            SetTreePosition(orderedKinds[i], i);
        }
    }

    private Dictionary<Tree, int> GetTreeToPositionIndex()
    {
        return _trees.ToDictionary(tree => tree, tree => tree.PositionIndex);
    }

    private void SaveTreeToPositionIndex(Dictionary<Tree, int> treeToPositionIndex)
    {
        foreach ((Tree tree, int position) in treeToPositionIndex)
        {
            tree.PositionIndex = position;
        }
    }

    private void ReorderTreeNode(TreeViewItem node, bool up)
    {
        if (node.Tag is Tree tree)
        {
            ReorderTree(tree, up);
        }
    }

    public void ClearTrees()
    {
        foreach (Tree tree in _trees)
        {
            tree.TreeViewNode.Items.Clear();
            tree.Nodes.Clear();
        }
    }

    private void ShowEnabledTrees()
    {
        ApplyRoots();
    }

    private void tsbShowBranches_Click(object? sender, EventArgs e)
        => ToggleTree(RepoTreeKind.Branches, tsbShowBranches.IsChecked == true);

    private void tsbShowRemotes_Click(object? sender, EventArgs e)
        => ToggleTree(RepoTreeKind.Remotes, tsbShowRemotes.IsChecked == true);

    private void tsbShowWorktrees_Click(object? sender, EventArgs e)
        => ToggleTree(RepoTreeKind.Worktrees, tsbShowWorktrees.IsChecked == true);

    private void tsbShowTags_Click(object? sender, EventArgs e)
        => ToggleTree(RepoTreeKind.Tags, tsbShowTags.IsChecked == true);

    private void tsbShowSubmodules_Click(object? sender, EventArgs e)
        => ToggleTree(RepoTreeKind.Submodules, tsbShowSubmodules.IsChecked == true);

    private void tsbShowStashes_Click(object? sender, EventArgs e)
        => ToggleTree(RepoTreeKind.Stashes, tsbShowStashes.IsChecked == true);
}
