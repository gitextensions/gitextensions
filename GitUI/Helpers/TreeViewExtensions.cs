using System;
using System.Windows.Forms;

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
    }
}