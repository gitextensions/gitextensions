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
    /// <param name="nodes">The nodes which are prepared for expansion.</param>
    /// <param name="afterAction">An optional action which is performed before exiting Begin/EndUpdate.</param>
    /// <param name="delayExpansion">If <c>true</c>, the subchildren are replaced with placeholders.</param>
    private static void RestoreChildrenOfFolderNodes(IEnumerable<DiffTreeNode> nodes, Action? afterAction = null, bool delayExpansion = false)
    {
        // Avalonia keeps data children attached and lazily creates their TreeView containers instead of using placeholder nodes.
        foreach (DiffTreeNode node in nodes)
        {
            if (delayExpansion)
            {
                foreach (DiffTreeNode child in node.Children)
                {
                    child.IsExpanded = false;
                }
            }
            else
            {
                ExpandAll(node);
            }
        }

        afterAction?.Invoke();
    }

    private static void RestoreChildrenOfFolderNodes(IEnumerable<FileTreeNode> nodes, Action? afterAction = null, bool delayExpansion = false)
    {
        // Avalonia keeps data children attached and lazily creates their TreeView containers instead of using placeholder nodes.
        foreach (FileTreeNode node in nodes)
        {
            if (delayExpansion)
            {
                foreach (FileTreeNode child in node.Children)
                {
                    child.IsExpanded = false;
                }
            }
            else
            {
                ExpandAll(node);
            }
        }

        afterAction?.Invoke();
    }
}
