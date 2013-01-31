using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls
{
    partial class RepoObjectsTree
    {
        /// <summary>base class for a node</summary>
        abstract class Node
        {
            TreeNode _TreeNode;
            /// <summary>Gets the <see cref="TreeNode"/> which holds this <see cref="Node"/>.</summary>
            public TreeNode TreeNode
            {
                get { return _TreeNode; }
                internal set { _TreeNode = value; ApplyStyle(); }
            }

            /// <summary>Gets the <see cref="GitUICommands"/> reference.</summary>
            public GitUICommands UiCommands { get; private set; }

            public Node(GitUICommands uiCommands, TreeNode treeNode = null)
            {
                TreeNode = treeNode;
                UiCommands = uiCommands;
                ContextActions = new ContextAction[0];
                ValidDrops = new DropAction[0];
            }

            /// <summary>Styles the <see cref="TreeNode"/>.</summary>
            internal virtual void ApplyStyle()
            {
                TreeNode.NodeFont = Settings.Font;
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

            protected Node(TParent parent, GitUICommands uiCommands, TreeNode treeNode = null)
                : base(uiCommands, treeNode)
            {
                ParentNode = parent;
            }
        }

        interface IValueNode<out TValue>
        {
            TValue Value { get; }
        }

        /// <summary>Basic node with a value.</summary>
        class Node<T, TParent> : Node<TParent>, IValueNode<T>
        {
            public T Value { get; private set; }

            public Node(T value, TParent parent, GitUICommands uiCommands, TreeNode treeNode = null)
                : base(parent, uiCommands, treeNode)
            {
                Value = value;
            }
        }

        /// <summary>Root node in a <see cref="RepoObjectsTreeSet"/> of type <see cref="TChild"/>.</summary>
        class RootNode<TChild> : Node, ICollection<TChild>
        {
            public virtual IList<TChild> Children { get; protected set; }

            public RootNode(GitUICommands uiCommands, TreeNode treeNode = null, IEnumerable<TChild> children = null)
                : base(uiCommands, treeNode)
            {
                Children = new List<TChild>(children ?? new TChild[] { });
            }

            #region ICollectionT (wraps Children)
            public IEnumerator<TChild> GetEnumerator() { return Children.GetEnumerator(); }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
            public void Add(TChild item) { Children.Add(item); }
            public void Clear() { Children.Clear(); }
            public bool Contains(TChild item) { return Children.Contains(item); }
            public void CopyTo(TChild[] array, int arrayIndex) { Children.CopyTo(array, arrayIndex); }
            public bool Remove(TChild item) { return Children.Remove(item); }
            public int Count { get { return Children.Count; } }
            public bool IsReadOnly { get { return Children.IsReadOnly; } }
            #endregion ICollectionT (wraps Children)
        }

        /// <summary>base class for a root node</summary>
        class RootNode<T, TChild> : RootNode<TChild>, IValueNode<T>
            where T : class
            where TChild : Node
        {
            public T Value { get; private set; }

            public RootNode(T value, GitUICommands uiCommands, TreeNode treeNode = null, IEnumerable<TChild> children = null)
                : base(uiCommands, treeNode, children)
            {
                Value = value;
            }

        }
    }

    internal class DropAction { }

    internal class ContextAction { }
}
