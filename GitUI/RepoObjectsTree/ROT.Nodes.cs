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
            /// <summary>Gets the <see cref="TreeNode"/> which holds this <see cref="Node"/>.
            /// <remarks>Setting this value will automatically call <see cref="ApplyStyle"/>.</remarks></summary>
            public TreeNode TreeNode
            {
                get { return _TreeNode; }
                internal set { _TreeNode = value; ApplyStyle(); }
            }

            /// <summary>Gets the parent node.</summary>
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
                IsDraggable = false;
                ContextActions = new ContextAction[0];
                AllowDrop = false;
            }

            /// <summary>Styles the <see cref="TreeNode"/>.</summary>
            internal virtual void ApplyStyle()
            {
                TreeNode.NodeFont = Settings.Font;
            }

            /// <summary>Occurs when the <see cref="Node"/> is selected.</summary>
            internal virtual void OnSelected() { }
            /// <summary>Occurs when the <see cref="Node"/> is clicked.</summary>
            internal virtual void OnClick() { }
            /// <summary>Occurs when the <see cref="Node"/> is double-clicked.</summary>
            internal virtual void OnDoubleClick() { }
            public virtual IEnumerable<ContextAction> ContextActions { get; protected set; }

            /// <summary>true if the <see cref="Node"/> is draggable.</summary>
            public virtual bool IsDraggable { get; protected set; }
            /// <summary>true if the <see cref="Node"/> will accept a dropped object.</summary>
            public virtual bool AllowDrop { get; protected set; }
            /// <summary>true if the <see cref="Node"/> will accept the specified <paramref name="dragged"/> object.</summary>
            public virtual bool Accepts(Node dragged) { return false; }
            /// <summary>Drops the object onto this <see cref="Node"/>.</summary>
            public virtual void Drop(object droppedObject) { }
          
            /// <summary>Gets the <see cref="Node"/> from a <see cref="TreeNode"/>'s tag.</summary>
            public static Node GetNode(TreeNode treeNode)
            {
                return (Node)treeNode.Tag;
            }

            /// <summary>Casts the <see cref="System.Windows.Forms.TreeNode.Tag"/> to a <see cref="Node"/>.</summary>
            public static Node GetNodeSafe(TreeNode treeNode)
            {
                return treeNode.Tag as Node;
            }

            /// <summary>Executes an action if <see cref="TreeNode"/> holds a <see cref="Node"/>.</summary>
            public static bool OnNode(TreeNode treeNode, Action<Node> action)
            {
                Node node = GetNodeSafe(treeNode);
                if (node != null)
                {
                    action(node);
                    return true;
                }
                return false;
            }

        }

        /// <summary>base class for a node with a (strongly-typed) parent</summary>
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

            public override string ToString() { return Value.ToString(); }
        }

        /* readme
         * Ok, so this may look a bit confusing, but here's the gist:
         * On FormBrowse init or repo change, RepoChanged is called:
         *      basically creates a new ListWatcher
         * ListWatcher holds the previous value of the nodes
         * When FormBrowse is refreshed/reloaded, ReloadAsync is invoked
         * ReloadAsync calls ListWatcher.CheckUpdateAsync which will async get current values for the nodes
         *      if the current values don't equal previous values, the UI is updated via ReloadNodes
         * When the UI is updated, the root node's children are cleared and new nodes are created
         */
        /// <summary>Base class for a root node in a <see cref="RepoObjectsTree"/>.</summary>
        abstract class RootNode : Node
        {
            /// <summary><see cref="ValueWatcher"/> instance</summary>
            protected ValueWatcher _Watcher;

            protected RootNode(GitUICommands uiCommands, TreeNode treeNode = null)
                : base(uiCommands, treeNode) { }

            /// <summary>Readies the tree set for a new repo. <remarks>Calls <see cref="ReloadAsync"/>.</remarks></summary>
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
            /// <summary>Gets the root's child <see cref="Node"/>s.</summary>
            public virtual IList<TChild> Children { get; protected set; }

            protected readonly Func<ICollection<TChild>> _getValues;
            readonly Action<ICollection<TChild>, ICollection<TChild>> _onReloading;
            protected readonly Action<ICollection<TChild>, RootNode<TChild>> _onReload;
            protected ListWatcher<TChild> _WatcherT;
            readonly Func<TreeNodeCollection, TChild, TreeNode> _addChild;

            public RootNode(TreeNode rootNode, GitUICommands uiCommands,
                Func<ICollection<TChild>> getValues,
                Action<ICollection<TChild>, ICollection<TChild>> onReloading,
                Action<ICollection<TChild>, RootNode<TChild>> onReload,
                Func<TreeNodeCollection, TChild, TreeNode> addChild
                )
                : base(uiCommands, rootNode)
            {
                if (getValues == null)
                {
                    throw new ArgumentNullException("getValues", "Must provide a function to retrieve values.");
                }
                _getValues = getValues;
                _onReloading = onReloading ?? ((olds, news) => { });
                _onReload = onReload ?? ((items, root) => { });
                _addChild = addChild ?? ((nodes, child) => null);
                Children = new List<TChild>();
            }

            /// <summary>Readies the tree set for a new repo. <remarks>Calls <see cref="RootNode.ReloadAsync"/>.</remarks></summary>
            public override void RepoChanged()
            {
                _Watcher = _WatcherT = new ListWatcher<TChild>(
                    _getValues,
                    (olds, news) =>
                    {
                        Children.Clear(); // clear children in BG thread
                        OnReloading(olds, news);
                    },
                    ReloadNodes);
                base.RepoChanged();
            }

            /// <summary>Reloads the set of nodes based on the specified <paramref name="items"/>.</summary>
            protected virtual void ReloadNodes(ICollection<TChild> items)
            {
                TreeNode.TreeView.Update(() =>
                 {
                     TreeNode.Nodes.Clear();

                     foreach (TChild item in items)
                     {
                         TreeNode treeNode = AddChild(TreeNode.Nodes, item);
                         item.ParentNode = this;
                         if (treeNode != null)
                         {
                             item.TreeNode = treeNode;
                         }
                     }

                     _onReload(items, this);
                 });
            }

            /// <summary>Adds a child <see cref="TreeNode"/> based on the specified <paramref name="item"/>.</summary>
            protected virtual TreeNode AddChild(TreeNodeCollection nodes, TChild item)
            {
                return _addChild(nodes, item);
            }

            /// <summary>Occurs on the background thread immediately before <see cref="ReloadNodes"/> is called.</summary>
            protected virtual void OnReloading(ICollection<TChild> olds, ICollection<TChild> news)
            {
                Children.Clear();
                _onReloading(olds, news);
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

    internal class ContextAction { }
}
