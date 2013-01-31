using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.UserControls
{
    partial class RepoObjectsTree
    {
        /// <summary>base class for a node</summary>
        abstract class Node
        {
            public TreeNode TreeNode { get; private set; }

            /// <summary>Gets the <see cref="GitUICommands"/> reference.</summary>
            public GitUICommands UiCommands { get; private set; }

            public Node(TreeNode treeNode, GitUICommands uiCommands)
            {
                TreeNode = treeNode;
                UiCommands = uiCommands;
                ContextActions = new ContextAction[0];
                ValidDrops = new DropAction[0];
            }

            internal virtual void OnClick() { }
            internal virtual void OnDoubleClick() { }
            public virtual IEnumerable<ContextAction> ContextActions { get; protected set; }

            public bool IsDraggable { get; protected set; }
            public bool HasDrops { get { return ValidDrops.Any(); } }
            public virtual void Drop(object droppedObject) { }
            public virtual void DragOver(object draggedObject) { }
            public virtual IEnumerable<DropAction> ValidDrops { get; protected set; }
        }

        /// <summary>base class for a node, with a parent</summary>
        abstract class Node<TParent> : Node
        {
            public TParent ParentNode { get; private set; }

            protected Node(TParent parent, TreeNode treeNode, GitUICommands uiCommands)
                : base(treeNode, uiCommands)
            {
                ParentNode = parent;
            }
        }

        interface IValueNode<TValue>
        {
            TValue Value { get; }
        }

        /// <summary>base class for a node, with a parent</summary>
        abstract class Node<T, TParent> : Node<TParent>, IValueNode<T>
        {
            public T Value { get; private set; }

            protected Node(T value, TParent parent, TreeNode treeNode, GitUICommands uiCommands)
                : base(parent, treeNode, uiCommands)
            {
                Value = value;
            }
        }

        class RootNode<TChild> : Node
        {
            public virtual IList<TChild> Children { get; protected set; }

            public RootNode(TreeNode treeNode, GitUICommands uiCommands, IEnumerable<TChild> children = null)
                : base(treeNode, uiCommands)
            {
                Children = new List<TChild>(children ?? new TChild[] { });
            }
        }

        /// <summary>base class for a root node</summary>
        class RootNode<T, TChild> : RootNode<TChild>, IValueNode<T>
            where T : class
            where TChild : Node
        {
            public T Value { get; private set; }

            public RootNode(T value, TreeNode treeNode, GitUICommands uiCommands, IEnumerable<TChild> children = null)
                : base(treeNode, uiCommands, children)
            {
                Value = value;
            }

        }
    }

    internal class DropAction { }

    internal class ContextAction { }
}
