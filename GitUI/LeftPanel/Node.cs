using GitUI.LeftPanel.Interfaces;
using Microsoft;

namespace GitUI.LeftPanel
{
    internal abstract class Node : NodeBase, INode
    {
        protected Tree Tree
        {
            get
            {
                Validates.NotNull(Nodes.Tree);
                return Nodes.Tree;
            }
        }

        protected GitUICommands UICommands => Tree.UICommands;

        protected Node(Tree? tree)
        {
            Nodes = new Nodes(tree);
        }

        private TreeNode? _treeViewNode;

        /// <summary>
        /// The tree node representing this node.
        /// Note that it may not always be set and which <see cref="Node"/> it represents may change
        /// because <see cref="Nodes.FillTreeViewNode(TreeNode)"/> recycles <see cref="TreeNode"/>s.
        /// </summary>
        protected internal override TreeNode TreeViewNode
        {
            get
            {
                Validates.NotNull(_treeViewNode);
                return _treeViewNode;
            }
            set
            {
                _treeViewNode = value;
                _treeViewNode.Tag = this;
                ApplyText();
                ApplyStyle();
            }
        }

        protected IWin32Window? ParentWindow()
        {
            return TreeViewNode.TreeView?.FindForm();
        }

        protected virtual string DisplayText()
        {
            return ToString();
        }

        // Override to provide a unique node name (key), otherwise DisplayText is used
        protected virtual string NodeName()
        {
            return DisplayText();
        }

        protected void ApplyText()
        {
            Validates.NotNull(_treeViewNode);

            _treeViewNode.Name = NodeName();
            _treeViewNode.Text = DisplayText();
        }

        internal virtual void OnSelected()
        {
        }

        internal virtual void OnClick()
        {
        }

        internal virtual void OnDoubleClick()
        {
        }

        internal virtual void OnRename()
        {
        }

        internal virtual void OnDelete()
        {
        }

        public static Node GetNode(TreeNode treeNode)
        {
            return (Node)treeNode.Tag;
        }

        internal static T? GetNodeSafe<T>(TreeNode? treeNode) where T : class, INode
        {
            return treeNode?.Tag as T;
        }

        public static void OnNode<T>(TreeNode treeNode, Action<T> action) where T : class, INode
        {
            var node = GetNodeSafe<T>(treeNode);

            if (node is not null)
            {
                action(node);
            }
        }
    }
}
