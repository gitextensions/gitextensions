using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace GitUI
{
    /// <summary>Provides helpful members to <see cref="TreeView"/> and related classes.</summary>
    static class TreeViewExtensions
    {
        /// <summary>Indicates whether a <see cref="TreeNode"/> has any child nodes.</summary>
        public static bool IsParent(this TreeNode item)
        {
            return item.Nodes.Count != 0;
        }

        /// <summary>Indicates whether a <see cref="TreeNode"/> has NO children.</summary>
        public static bool HasNoChildren(this TreeNode node)
        {
            return node.IsParent() == false;
        }

        /// <summary>Indicates whether a <see cref="TreeNode"/> descends from an <paramref name="ancestor"/>.</summary>
        public static bool IsDescendantOf(this TreeNode child, TreeNode ancestor)
        {
            if (child.Parent == null)
            {// node at root
                return false;
            }
            if (child.Level <= ancestor.Level)
            {// on same level OR node is older
                return false;
            }
            if (ancestor.Nodes.Count == 0)
            {// ancestor has no children
                return false;
            }
            return
                (child.Parent == ancestor)// one last quick check
                ||
                child.Parent.IsDescendantOf(ancestor);// else recurse (if Parent is an ancestor, then child is ancestor)
        }

        /// <summary>Indicates whether an <see cref="TreeNode"/> is an ancestor of the specified <paramref name="child"/>.</summary>
        public static bool IsAncestorOf(this TreeNode ancestor, TreeNode child)
        {
            return child.IsDescendantOf(ancestor);
        }

        /// <summary>Suspends drawing, updates, then redraws a <see cref="TreeView"/>.</summary>
        public static void Update(this TreeView tree, Action update)
        {
            tree.BeginUpdate();
            update();
            tree.EndUpdate();
        }

        /// <summary>Adds <see cref="TreeView"/> checked nodes to a supplied list.</summary>
        public static void listNodes(this TreeView tree, List<TreeNode> nodes, TreeNode node)
        {

            if (node.Nodes.Count == 0 && node.Checked)
            {
                nodes.Add(node);
            }
            else
            {
                foreach (TreeNode child in node.Nodes)
                {
                    tree.listNodes(nodes, child);
                }
            }

        }
        /// <summary>Switches all <see cref="TreeNode"/> children recursively to a supplied state</summary>
        public static void CheckAllChildNodes(this TreeNode treeNode, bool nodeChecked)
        {
            treeNode.Checked = nodeChecked;
            foreach (TreeNode node in treeNode.Nodes)
            {
                node.CheckAllChildNodes(nodeChecked);
            }
        }

        /// <summary>Checks all <see cref="TreeNode"/> ancestors if the have all children checked, unchecks them otherwise</summary>
        public static void CheckParentNodes(this TreeNode node)
        {
            TreeNode parent = node.Parent;
            while (parent != null)
            {
                parent.Checked = parent.Nodes.OfType<TreeNode>().All(tn => tn.Checked);
                parent = parent.Parent;
            }
        }


    }
}