namespace GitUI.UserControls;

public static class TreeViewExtensions
{
    public static void EnsureVerticallyVisible(this TreeNode? node)
    {
        if (node?.TreeView is not TreeView treeView)
        {
            return;
        }

        node.EnsureVisible();

        // EnsureVisible leads to horizontal scrolling in some cases. We make sure to force horizontal scroll back to 0.
        treeView.ScrollLeftMost();
    }

    /// <summary>
    /// Returns a set of expanded node paths to be used with RestoreExpandedNodeState starting from the input node.
    /// This function makes use of GetFullNamePath, rather than TreeNode.FullPath, so you can vary the node's Text,
    /// as long as the node's Name remains stable.
    /// </summary>
    public static HashSet<string> GetExpandedNodesState(this TreeNode node)
    {
        HashSet<string> result = [];
        node.DoGetExpandedNodesState(result);
        return result;
    }

    /// <summary>
    /// Similar to TreeNode.FullPath, this function returns a full path made up of TreeNode.Name rather
    /// than TreeNode.Text, if the former is non-empty. This is useful as it allows the node's Text to
    /// be changed, while the node's Name can remain a key for operations such as getting and restoring
    /// the expanded node state.
    /// </summary>
    public static string GetFullNamePath(this TreeNode node)
    {
        string sep = node.TreeView is not null ? node.TreeView.PathSeparator : "\\";

        string result = GetNameOrText(node);
        TreeNode currNode = node;
        while (currNode.Parent is not null)
        {
            currNode = currNode.Parent;
            result = GetNameOrText(currNode) + sep + result;
        }

        return result;

        string GetNameOrText(TreeNode n)
        {
            return n.Name.Length > 0 ? n.Name : n.Text;
        }
    }

    public static TreeNode? GetNodeFromPath(this TreeNode node, string? path)
    {
        if (GetFullNamePath(node) == path)
        {
            return node;
        }

        foreach (TreeNode childNode in node.Nodes)
        {
            TreeNode foundNode = GetNodeFromPath(childNode, path);
            if (foundNode is not null)
            {
                return foundNode;
            }
        }

        return null;
    }

    /// <summary>
    /// Restores the expanded state of nodes under the input node using the set returned by GetExpandedNodesState.
    /// </summary>
    public static void RestoreExpandedNodesState(this TreeNode node, HashSet<string> expandedNodes)
    {
        foreach (string path in expandedNodes)
        {
            TreeNode foundNode = GetNodeFromPath(node, path);
            foundNode?.Expand();
        }
    }

    public static void ScrollLeftMost(this TreeView? treeView)
    {
        if (treeView is not null)
        {
            NativeMethods.SendMessageW(treeView.Handle, NativeMethods.WM_HSCROLL, (IntPtr)NativeMethods.SBH.LEFT, IntPtr.Zero);
        }
    }

    private static void DoGetExpandedNodesState(this TreeNode node, HashSet<string> expandedNodes)
    {
        if (node.IsExpanded)
        {
            expandedNodes.Add(GetFullNamePath(node));
        }

        foreach (TreeNode childNode in node.Nodes)
        {
            DoGetExpandedNodesState(childNode, expandedNodes);
        }
    }
}
