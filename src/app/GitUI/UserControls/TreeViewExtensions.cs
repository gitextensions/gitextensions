#nullable enable

using GitExtensions.Extensibility;

namespace GitUI.UserControls;

public static class TreeViewExtensions
{
    public static void EnsureVerticallyVisible(this TreeNode? node)
    {
        if (node is null)
        {
            return;
        }

        if (node.TreeView is not TreeView treeView)
        {
            DebugHelpers.Fail(@$"{nameof(EnsureVerticallyVisible)}: Node ""{node.Text}"" does not belong to a TreeView.");
            return;
        }

        node.EnsureVisible();

        // EnsureVisible leads to horizontal scrolling in some cases. We make sure to force horizontal scroll back to 0.
        treeView.ScrollLeftMost();
    }

    public static void ExpandTopDownTo(this TreeView? treeView, TreeNode node)
    {
        if (treeView is null)
        {
            return;
        }

        List<TreeNode> parents = [];
        AddParents(parents, node, treeView.Nodes);
        foreach (TreeNode parent in parents)
        {
            parent.Expand();
        }

        return;

        static bool AddParents(List<TreeNode> parentsOfNode, TreeNode node, TreeNodeCollection nodes)
        {
            IEnumerable<TreeNode> actualNodes = GetActualNodes(nodes);
            foreach (TreeNode parent in actualNodes)
            {
                if (parent == node)
                {
                    return true;
                }

                if (parent.Nodes.Count == 0)
                {
                    continue;
                }

                bool found = AddParents(parentsOfNode, node, parent.Nodes);
                if (found)
                {
                    parentsOfNode.Insert(0, parent);
                    return true;
                }
            }

            return false;
        }
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
            if (GetNodeFromPath(childNode, path) is TreeNode foundNode)
            {
                return foundNode;
            }
        }

        return null;
    }

    public static IEnumerable<TreeNode> Items(this TreeView? treeView)
        => treeView is null ? [] : Recurse(treeView.Nodes);

    public static IEnumerable<TreeNode> Items(this TreeNode? node)
    {
        if (node is null)
        {
            yield break;
        }

        yield return node;

        foreach (TreeNode subNode in Recurse(node.Nodes))
        {
            yield return subNode;
        }
    }

    public static IEnumerable<T> ItemTags<T>(this TreeView? treeView) where T : class
        => treeView.Items().ItemTags<T>();

    public static IEnumerable<T> ItemTags<T>(this TreeNode? node) where T : class
        => node.Items().ItemTags<T>();

    /// <summary>
    /// Restores the expanded state of nodes under the input node using the set returned by GetExpandedNodesState.
    /// </summary>
    public static void RestoreExpandedNodesState(this TreeNode node, HashSet<string> expandedNodes)
    {
        foreach (string path in expandedNodes)
        {
            TreeNode? foundNode = GetNodeFromPath(node, path);
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

    public static IEnumerable<T> SelectedItemTags<T>(this MultiSelectTreeView? treeView) where T : class
        => treeView is null ? [] : treeView.SelectedNodes.ItemTags<T>();

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

    private static IEnumerable<TreeNode> GetActualNodes(TreeNodeCollection nodes)
        => nodes.Count == 1 && nodes[0].Tag is TreeNode[] actualNodes
            ? actualNodes
            : nodes.Cast<TreeNode>();

    /// <summary>
    ///  Returns the Tag of the nodes which can be casted to T - without iterating subnodes.
    /// </summary>
    private static IEnumerable<T> ItemTags<T>(this IEnumerable<TreeNode> nodes) where T : class
        => nodes.Select(node => node.Tag as T).Where(value => value is not null).Cast<T>();

    private static IEnumerable<TreeNode> Recurse(TreeNodeCollection nodes)
    {
        foreach (TreeNode node in GetActualNodes(nodes))
        {
            foreach (TreeNode treeNode in node.Items())
            {
                yield return treeNode;
            }
        }
    }
}
