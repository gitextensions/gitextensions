using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUI.Notifications;
using GitUIPluginInterfaces.Notifications;

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
                internal set
                {
                    _TreeNode = value;
                    _TreeNode.Tag = this;
                    ApplyStyle();
                }
            }

            /// <summary>Gets the parent node.</summary>
            public Node ParentNode { get; internal set; }

            /// <summary>Gets the <see cref="GitUICommands"/> reference.</summary>
            public GitUICommands UiCommands { get; private set; }
            /// <summary>Gets the <see cref="GitModule"/> reference.</summary>
            public GitModule Git { get; private set; }

            protected Node(GitUICommands uiCommands, TreeNode treeNode = null)
            {
                if (treeNode != null)
                {
                    TreeNode = treeNode;
                }
                UiCommands = uiCommands;
                Git = uiCommands.Module;
                Notifier = NotificationManager.Get(Git);
                IsDraggable = false;
                ContextActions = new ContextAction[0];
                AllowDrop = false;

                dragDropActions = new Lazy<IEnumerable<DragDropAction>>(CreateDragDropActions);
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

            #region Drag/Drop
            /// <summary>true if the <see cref="Node"/> is draggable.</summary>
            public virtual bool IsDraggable { get; protected set; }
            /// <summary>true if the <see cref="Node"/> will accept a dropped object.</summary>
            public virtual bool AllowDrop { get; protected set; }
            /// <summary>true if the <see cref="Node"/> will accept the specified <paramref name="dragged"/> object.</summary>
            public bool Accepts(Node dragged)
            {
                return dragDropActions.Value.Any(dda => dda.Drag(dragged));
            }
            /// <summary>Drops the object onto this <see cref="Node"/>.</summary>
            public void Drop(object droppedObject)
            {
                if (dragDropActions.Value.Any(dda => dda.Drop(droppedObject))) { }
            }

            /// <summary>Gets the valid <see cref="DragDropAction"/>s.</summary>
            protected virtual IEnumerable<DragDropAction> CreateDragDropActions() { return new DragDropAction[0]; }
            Lazy<IEnumerable<DragDropAction>> dragDropActions;

            /// <summary>Valid drag-drop action.</summary>
            protected class DragDropAction
            {
                Func<object, bool> _canDrop;
                Action<object> _onDrop;

                protected DragDropAction(Func<object, bool> canDrop, Action<object> onDrop)
                {
                    _canDrop = canDrop;
                    _onDrop = onDrop;
                }

                /// <summary>Execute a drag, which indicates whether a drop is allowed.</summary>
                public bool Drag(object draggedObject)
                {
                    return _canDrop(draggedObject);
                }

                /// <summary>Execute the drop, with a validity check.</summary>
                public bool Drop(object droppedObject)
                {
                    if (_canDrop(droppedObject))
                    {
                        _onDrop(droppedObject);
                        return true;
                    }
                    return false;
                }
            }

            /// <summary><see cref="DragDropAction"/> with safe type-casting.</summary>
            /// <typeparam name="TDragged">Type of the dragged/dropped object.</typeparam>
            protected class DragDropAction<TDragged> : DragDropAction
                where TDragged : class
            {
                public DragDropAction(Func<TDragged, bool> canDrop, Action<TDragged> onDrop)
                    : base(obj =>
                    {
                        TDragged t = obj as TDragged;
                        return (t != null) && canDrop(t);
                    }, obj => onDrop(obj as TDragged)) { }
            }
            #endregion Drag/Drop

            protected INotifier Notifier { get; private set; }

            /// <summary>Wraps <see cref="INotifier.Notify"/>.</summary>
            protected void Notify(Notification notification)
            {
                Notifier.Notify(notification);
            }

            /// <summary>Depending on a git command's result, publishes a notification.</summary>
            /// <param name="result">Result of the git command.</param>
            /// <param name="successNotification">Notification to publish if successful.</param>
            /// <param name="failNotification">Notification to publish if failed.</param>
            protected void NotifyIf(
                        GitCommandResult result,
                        Func<Notification> successNotification,
                        Func<Notification> failNotification)
            {
                Notifier.NotifyIf(result, successNotification, failNotification);
            }

            /// <summary>Gets the <see cref="Node"/> from a <see cref="TreeNode"/>'s tag.</summary>
            public static Node GetNode(TreeNode treeNode)
            {
                return (Node)treeNode.Tag;
            }

            /// <summary>Casts the <see cref="System.Windows.Forms.TreeNode.Tag"/> to a <see cref="Node"/>.</summary>
            public static Node GetNodeSafe(TreeNode treeNode)
            {
                return GetNodeSafe<Node>(treeNode);
            }

            /// <summary>Casts the <see cref="System.Windows.Forms.TreeNode.Tag"/> to a <see cref="Node"/>.</summary>
            public static TNode GetNodeSafe<TNode>(TreeNode treeNode)
                where TNode : Node
            {
                return treeNode.Tag as TNode;
            }

            /// <summary>Executes an action if <see cref="TreeNode"/> holds a <see cref="Node"/>.</summary>
            public static bool OnNode(TreeNode treeNode, Action<Node> action)
            {
                return OnNode<Node>(treeNode, action);
            }

            /// <summary>Executes an action if <see cref="TreeNode"/> holds a <see cref="Node"/>.</summary>
            public static bool OnNode<TNode>(TreeNode treeNode, Action<TNode> action)
                where TNode : Node
            {
                TNode node = GetNodeSafe<TNode>(treeNode);
                if (node != null)
                {
                    action(node);
                    return true;
                }
                return false;
            }

        }

        /// <summary>Node with a value</summary>
        abstract class Node<TValue> : Node
        {
            public TValue Value { get; private set; }

            protected Node(TValue value, GitUICommands uiCommands, TreeNode treeNode = null)
                : base(uiCommands, treeNode)
            {
                Value = value;
            }
        }

        /// <summary>Node with a value and a strongly-typed parent.</summary>
        class Node<TValue, TParent> : Node<TValue>
        {
            public TParent Parent { get; private set; }
            public Node(TValue value, TParent parent, GitUICommands uiCommands, TreeNode treeNode = null)
                : base(value, uiCommands, treeNode)
            {
                Parent = parent;
            }

            public override string ToString() { return Value.ToString(); }
        }

        /// <summary>Node with a value and children.</summary>
        abstract class ParentNode<TValue, TChild> : Node<TValue>
        {
            /// <summary>Gets the node's children.</summary>
            public ICollection<TChild> Children { get; private set; }

            protected ParentNode(GitUICommands uiCommands, TValue value, IEnumerable<TChild> children = null, TreeNode treeNode = null)
                : base(value, uiCommands, treeNode)
            {
                Children = new Collection<TChild>(
                    children != null
                    ? children.ToList()
                    : new List<TChild>());
            }
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

        /// <summary>Root node in a <see cref="RepoObjectsTree"/>, with children of type <see cref="TChild"/>.</summary>
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
                this.Children = new List<TChild>();

                _Watcher = _WatcherT = new ListWatcher<TChild>(
                  _getValues,
                  (olds, news) =>
                  {
                      Children.Clear(); // clear children in BG thread
                      OnReloading(olds, news);
                  },
                  ReloadNodes);
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
