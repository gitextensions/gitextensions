using System.Collections.Generic;
using System.Windows.Forms;

namespace GitUI.UserControls
{
    public static class TreeViewExtensions
    {
        public static HashSet<string> GetExpandedNodesState(this TreeNode node)
        {
            var result = new HashSet<string>();
            node.DoGetExpandedNodesState(result);
            return result;
        }

        private static void DoGetExpandedNodesState(this TreeNode node, HashSet<string> expandedNodes)
        {
            if (node.IsExpanded)
            {
                expandedNodes.Add(node.FullPath);
            }

            foreach (TreeNode childNode in node.Nodes)
            {
                DoGetExpandedNodesState(childNode, expandedNodes);
            }
        }

        public static void RestoreExpandedNodesState(this TreeNode node, HashSet<string> expandedNodes)
        {
            foreach (var path in expandedNodes)
            {
                var foundNode = GetNodeFromPath(node, path);
                foundNode?.Expand();
            }
        }

        private static TreeNode GetNodeFromPath(TreeNode node, string path)
        {
            if (node.FullPath == path)
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
