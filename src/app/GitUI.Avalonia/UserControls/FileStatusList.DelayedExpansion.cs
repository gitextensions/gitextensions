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
}
