using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls
{
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

    public partial class RepoObjectsTree
    {
        /// <summary>Base class for a <see cref="RepoObjectsTree"/> set of nodes.</summary>
        abstract class RepoObjectsTreeSet
        {
            /// <summary>root <see cref="TreeNode"/></summary>
            protected readonly TreeNode _treeNode;
            readonly Action<TreeNode> _applyStyle;
            protected ValueWatcher _Watcher;

            protected RepoObjectsTreeSet(TreeNode treeNode, Action<TreeNode> applyStyle)
            {
                _treeNode = treeNode;
                _applyStyle = applyStyle ?? (node => { });
            }

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

            /// <summary>Applies a style to the specified <see cref="TreeNode"/>.</summary>
            public virtual void ApplyTreeNodeStyle(TreeNode treeNode)
            {
                RepoObjectsTree.ApplyTreeNodeStyle(treeNode);
                _applyStyle(treeNode);
            }
        }

        /// <summary><see cref="RepoObjectsTree"/> set of nodes, with an underlying type T.</summary>
        class RepoObjectsTreeSet<T> : RepoObjectsTreeSet
        {
            protected readonly Func<ICollection<T>> _getValues;
            protected readonly Action<ICollection<T>> _onReload;
            protected ListWatcher<T> _WatcherT;
            readonly Func<TreeNodeCollection, T, TreeNode> _addChild;

            public RepoObjectsTreeSet(
                GitModule git,
                TreeNode treeNode,
                Func<ICollection<T>> getValues,
                Action<ICollection<T>> onReload,
                Func<TreeNodeCollection, T, TreeNode> addChild, Action<TreeNode> applyStyle
                )
                : base(treeNode, applyStyle)
            {
                _getValues = getValues;
                _onReload = onReload;
                _addChild = addChild;
            }

            /// <summary>Readies the tree set for a new repo.</summary>
            public override void RepoChanged()
            {
                _Watcher = _WatcherT = new ListWatcher<T>(() => _getValues(), ReloadNodes);
                base.RepoChanged();
            }

            /// <summary>Reloads the set of nodes based on the specified <paramref name="items"/>.</summary>
            protected virtual void ReloadNodes(ICollection<T> items)
            {
                _treeNode.TreeView.Update(() =>
                {
                    _treeNode.Nodes.Clear();

                    foreach (T item in items)
                    {
                        TreeNode child = AddChild(_treeNode.Nodes, item);
                        if (child != null)
                        {
                            ApplyStyle(child);
                        }
                    }

                    _onReload(items);
                });
            }

            /// <summary>Adds a child <see cref="TreeNode"/> based on the specified <paramref name="item"/>.</summary>
            protected virtual TreeNode AddChild(TreeNodeCollection nodes, T item)
            {
                return _addChild(nodes, item);
            }

            /// <summary>Applies a style to the specified <see cref="TreeNode"/>.</summary>
            protected virtual void ApplyStyle(TreeNode treeNode)
            {
                base.ApplyTreeNodeStyle(treeNode);
            }
        }
    }
}
