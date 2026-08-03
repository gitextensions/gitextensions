namespace GitUI;

partial class FileStatusList
{
    private static void ExpandAll(DiffTreeNode? node)
    {
        if (node is null)
        {
            return;
        }

        node.IsExpanded = true;
        foreach (DiffTreeNode child in node.Children)
        {
            ExpandAll(child);
        }
    }

    private static void ExpandAll(FileTreeNode? node)
    {
        if (node is null)
        {
            return;
        }

        node.IsExpanded = true;
        foreach (FileTreeNode child in node.Children)
        {
            ExpandAll(child);
        }
    }

    /// <summary>
    ///  Replaces possible placeholders with the actual children.
    /// </summary>
    /// <remarks>
    ///  Avalonia materializes TreeView containers lazily, so the data children remain attached
    ///  and only their visual containers are delayed until expansion.
    /// </remarks>
    private static void RestoreChildrenOfFolderNodes(IEnumerable<DiffTreeNode> nodes, Action? afterAction = null, bool delayExpansion = false)
    {
        foreach (DiffTreeNode node in nodes)
        {
            if (!delayExpansion)
            {
                ExpandAll(node);
            }
        }

        afterAction?.Invoke();
    }
}
