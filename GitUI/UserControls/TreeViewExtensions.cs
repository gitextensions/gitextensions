using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace GitUI.UserControls
{
    public static class TreeViewExtensions
    {
        /// <summary>
        /// Similar to TreeNode.FullPath, this function returns a full path made up of TreeNode.Name rather
        /// than TreeNode.Text, if the former is non-empty. This is useful as it allows the node's Text to
        /// be changed, while the node's Name can remain a key for operations such as getting and restoring
        /// the expanded node state.
        /// </summary>
        public static string GetFullNamePath(this TreeNode node)
        {
            var sep = node.TreeView != null ? node.TreeView.PathSeparator : "\\";

            string result = GetNameOrText(node);
            var currNode = node;
            while (currNode.Parent != null)
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

        /// <summary>
        /// Returns a set of expanded node paths to be used with RestoreExpandedNodeState starting from the input node.
        /// This function makes use of GetFullNamePath, rather than TreeNode.FullPath, so you can vary the node's Text,
        /// as long as the node's Name remains stable.
        /// </summary>
        public static HashSet<string> GetExpandedNodesState(this TreeNode node)
        {
            var result = new HashSet<string>();
            node.DoGetExpandedNodesState(result);
            return result;
        }

        /// <summary>
        /// Restores the expanded state of nodes under the input node using the set returned by GetExpandedNodesState
        /// </summary>
        public static void RestoreExpandedNodesState(this TreeNode node, HashSet<string> expandedNodes)
        {
            foreach (var path in expandedNodes)
            {
                var foundNode = GetNodeFromPath(node, path);
                foundNode?.Expand();
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

        private static TreeNode GetNodeFromPath(TreeNode node, string path)
        {
            if (GetFullNamePath(node) == path)
            {
                return node;
            }

            foreach (TreeNode childNode in node.Nodes)
            {
                var foundNode = GetNodeFromPath(childNode, path);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }

            return null;
        }
    }
}
