using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            public Node ParentNode { get; internal set; }

            /// <summary>Gets the <see cref="GitUICommands"/> reference.</summary>
            public GitUICommands UiCommands { get; private set; }

            public Node(GitUICommands uiCommands, TreeNode treeNode = null)
            {
                if (treeNode != null)
                {
                    TreeNode = treeNode;
                }
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
            new public TParent ParentNode { get; private set; }

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

        /* readme
         * Ok, so this may look a bit confusing, but here's the gist:
         * On FormBrowse init or repo change, NewRepo is called:
         *      basically sets the new git module and creates a new ListWatcher
         * ListWatcher holds the previous value of the nodes
         * When FormBrowse is refreshed/reloaded, ReloadAsync is invoked
         * ReloadAsync calls ListWatcher.CheckUpdateAsync which will async get current values for the nodes
         *      if the current values don't equal previous values, the UI is updated via ReloadNodes
         * When the UI is updated, the root node's children are cleared and new nodes are created
         */
        /// <summary>Base class for a root node in a <see cref="RepoObjectsTree"/>.</summary>
        abstract class RootNode : Node
        {
            protected ValueWatcher _Watcher;

            protected RootNode(GitUICommands uiCommands, TreeNode treeNode = null)
                : base(uiCommands, treeNode) { }

            /// <summary>Readies the tree set for a new repo.</summary>
            public virtual void RepoChanged()
            {
                ReloadAsync();
            }

            /// <summary>Async reloads the set of nodes.</summary>
            public virtual Task ReloadAsync()
            {
                return _Watcher.CheckUpdateAsync();
            }
        }

        /// <summary>Root node in a <see cref="RepoObjectsTree"/> of type <see cref="TChild"/>.</summary>
        class RootNode<TChild> : RootNode, ICollection<TChild>
            where TChild : Node
        {
            public virtual IList<TChild> Children { get; protected set; }

            protected readonly Func<ICollection<TChild>> _getValues;
            protected readonly Action<ICollection<TChild>, RootNode<TChild>> _onReload;
            protected ListWatcher<TChild> _WatcherT;
            readonly Func<TreeNodeCollection, TChild, TreeNode> _addChild;

            public RootNode(TreeNode rootNode, GitUICommands uiCommands,
                Func<ICollection<TChild>> getValues,
                Action<ICollection<TChild>, RootNode<TChild>> onReload,
                Func<TreeNodeCollection, TChild, TreeNode> addChild
                )
                : base(uiCommands, rootNode)
            {
                _getValues = getValues;
                _onReload = onReload;
                _addChild = addChild;
            }

            /// <summary>Readies the tree set for a new repo.</summary>
            public override void RepoChanged()
            {
                _Watcher = _WatcherT = new ListWatcher<TChild>(
                    _getValues,
                    (olds, news) =>
                    {
                        //Children.Clear()
                    }, // clear children in BG thread
                    ReloadNodes);
                base.RepoChanged();
            }

            /// <summary>Reloads the set of nodes based on the specified <paramref name="items"/>.</summary>
            protected virtual void ReloadNodes(ICollection<TChild> items)
            {
                Children.Clear();
                TreeNode.TreeView.Update(() =>
                {
                    TreeNode.Nodes.Clear();

                    foreach (TChild item in items)
                    {
                        TreeNode child = AddChild(TreeNode.Nodes, item);
                        item.ParentNode = this;
                        item.ApplyStyle();
                    }

                    _onReload(items, this);
                });
            }

            /// <summary>Adds a child <see cref="TreeNode"/> based on the specified <paramref name="item"/>.</summary>
            protected virtual TreeNode AddChild(TreeNodeCollection nodes, TChild item)
            {
                return _addChild(nodes, item);
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
    }

    internal class DropAction { }

    internal class ContextAction { }
}
