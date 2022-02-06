using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.UserControls;
using GitUIPluginInterfaces;

namespace GitUI.BranchTreePanel
{
    internal abstract class Tree : NodeBase, IDisposable
    {
        private readonly IGitUICommandsSource _uiCommandsSource;
        private readonly CancellationTokenSequence _reloadCancellationTokenSequence = new();
        private bool _firstReloadNodesSinceModuleChanged = true;

        // A flag to indicate whether the data is being filtered (e.g. Show Current Branch Only).
        private protected AsyncLocal<bool> IsFiltering = new();

        protected Tree(TreeNode treeNode, IGitUICommandsSource uiCommands)
        {
            Nodes = new Nodes(this);
            _uiCommandsSource = uiCommands;
            TreeViewNode = treeNode;
            treeNode.Tag = this;

            uiCommands.UICommandsChanged += (a, e) =>
            {
                // When GitModule has changed, clear selected node
                if (TreeViewNode?.TreeView is not null)
                {
                    TreeViewNode.TreeView.SelectedNode = null;
                }

                // Certain operations need to happen the first time after we change modules. For example,
                // we don't want to use the expanded/collapsed state of existing nodes in the tree, but at
                // the same time, we don't want to remove them from the tree as this is visible to the user,
                // as well as less efficient.
                _firstReloadNodesSinceModuleChanged = true;
            };
        }

        public void Dispose()
        {
            Detached();
            _reloadCancellationTokenSequence.Dispose();
        }

        public GitUICommands UICommands => _uiCommandsSource.UICommands;

        /// <summary>
        /// A flag to indicate that node SelectionChanged event is not user-originated and
        /// must not trigger the event handling sequence.
        /// </summary>
        public bool IgnoreSelectionChangedEvent { get; set; }
        protected GitModule Module => UICommands.Module;
        protected bool IsAttached { get; private set; }
        protected virtual bool SupportsFiltering { get; } = false;

        public void Attached()
        {
            IsAttached = true;
            OnAttached();
        }

        protected virtual void OnAttached()
        {
            IsFiltering.Value = false;
        }

        public void Detached()
        {
            _reloadCancellationTokenSequence.CancelCurrent();
            IsAttached = false;
            OnDetached();
        }

        protected virtual void OnDetached()
        {
        }

        /// <summary>
        /// Requests to refresh the data tree and to apply filtering, if necessary.
        /// </summary>
        protected internal virtual void Refresh(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
        {
            // NOTE: descendants may need to break their local caches to ensure the latest data is loaded.

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ReloadNodesAsync(LoadNodesAsync, getRefs);
            });
        }

        /// <summary>
        /// Requests to refresh the data tree and to apply filtering, if necessary.
        /// </summary>
        /// <param name="isFiltering">
        ///  <see langword="true"/>, if the data is being filtered; otherwise <see langword="false"/>.
        /// </param>
        /// <param name="forceRefresh">Refresh may be required as references may have been changed.</param>
        internal void Refresh(bool isFiltering, bool forceRefresh, Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
        {
            if (!IsAttached)
            {
                return;
            }

            // If we're not currently filtering and no need to filter now -> exit.
            // Else we need to iterate over the list and rebind the tree - whilst there
            // could be a situation whether a user just refreshed the grid, there could
            // also be a situation where the user applied a different filter, or checked
            // out a different ref (e.g. a branch or commit), and we have a different
            // set of branches to show/hide.
            if (!forceRefresh && (!SupportsFiltering || (!isFiltering && !IsFiltering.Value)))
            {
                return;
            }

            IsFiltering.Value = isFiltering && SupportsFiltering;
            Refresh(getRefs);
        }

        protected abstract Task<Nodes> LoadNodesAsync(CancellationToken token, Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs);

        public IEnumerable<TNode> DepthEnumerator<TNode>() where TNode : NodeBase
            => Nodes.DepthEnumerator<TNode>();

        internal IEnumerable<NodeBase> GetNodesAndSelf()
            => DepthEnumerator<NodeBase>().Prepend(this);

        internal IEnumerable<NodeBase> GetSelectedNodes()
            => GetNodesAndSelf().Where(node => node.IsSelected);

        // Invoke from child class to reload nodes for the current Tree. Clears Nodes, invokes
        // input async function that should populate Nodes, then fills the tree view with its contents,
        // making sure to disable/enable the control.
        protected async Task ReloadNodesAsync(Func<CancellationToken, Func<RefsFilter, IReadOnlyList<IGitRef>>, Task<Nodes>> loadNodesTask,
            Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
        {
            var token = _reloadCancellationTokenSequence.Next();

            var treeView = TreeViewNode.TreeView;

            if (treeView is null || !IsAttached)
            {
                return;
            }

            // Module is invalid in Dashboard
            Nodes newNodes = Module.IsValidGitWorkingDir() ? await loadNodesTask(token, getRefs) : new(tree: null);

            await treeView.SwitchToMainThreadAsync(token);

            // remember multi-selected nodes
            HashSet<int> multiSelected = GetSelectedNodes().Select(node => node.GetHashCode()).ToHashSet();

            Nodes.Clear();
            Nodes.AddNodes(newNodes);

            // re-apply multi-selection
            if (multiSelected.Count > 0)
            {
                foreach (NodeBase node in GetNodesAndSelf().Where(node => multiSelected.Contains(node.GetHashCode())))
                {
                    node.IsSelected = true;
                }
            }

            // Check again after switch to main thread
            treeView = TreeViewNode.TreeView;

            if (treeView is null || !IsAttached)
            {
                return;
            }

            try
            {
                string? originalSelectedNodeFullNamePath = treeView.SelectedNode?.GetFullNamePath();

                treeView.BeginUpdate();
                IgnoreSelectionChangedEvent = true;
                FillTreeViewNode(originalSelectedNodeFullNamePath, _firstReloadNodesSinceModuleChanged);
            }
            finally
            {
                IgnoreSelectionChangedEvent = false;
                treeView.EndUpdate();
                ExpandPathToSelectedNode();
                _firstReloadNodesSinceModuleChanged = false;
            }
        }

        private void FillTreeViewNode(string? originalSelectedNodeFullNamePath, bool firstTime)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var expandedNodesState = firstTime ? new HashSet<string>() : TreeViewNode.GetExpandedNodesState();
            Nodes.FillTreeViewNode(TreeViewNode);

            var selectedNode = TreeViewNode.TreeView.SelectedNode;

            if (originalSelectedNodeFullNamePath != selectedNode?.GetFullNamePath())
            {
                var node = TreeViewNode.GetNodeFromPath(originalSelectedNodeFullNamePath);

                if (node is not null)
                {
                    TreeViewNode.TreeView.SelectedNode = !(node.Tag is BaseBranchNode branchNode) || branchNode.Visible
                        ? node
                        : null;
                }
            }

            PostFillTreeViewNode(firstTime);

            TreeViewNode.RestoreExpandedNodesState(expandedNodesState);
        }

        // Called after the TreeView has been populated from Nodes. A good place to update properties
        // of the TreeViewNode, such as it's name (TreeViewNode.Text), Expand/Collapse state, and
        // to set selected node (TreeViewNode.TreeView.SelectedNode).
        protected virtual void PostFillTreeViewNode(bool firstTime)
        {
        }

        private void ExpandPathToSelectedNode()
        {
            if (TreeViewNode.TreeView.SelectedNode is not null)
            {
                EnsureNodeVisible(TreeViewNode.TreeView.Handle, TreeViewNode.TreeView.SelectedNode);
            }
            else if (TreeViewNode.TreeView.Nodes.Count > 0)
            {
                // No selected node, just make sure the first node is visible
                EnsureNodeVisible(TreeViewNode.TreeView.Handle, TreeViewNode.TreeView.Nodes[0]);
            }

            return;

            static void EnsureNodeVisible(IntPtr hwnd, TreeNode node)
            {
                node.EnsureVisible();

                // EnsureVisible leads to horizontal scrolling in some cases. We make sure to force horizontal
                // scroll back to 0. Note that we use SendMessage rather than SetScrollPos as the former works
                // outside of Begin/EndUpdate.
                NativeMethods.SendMessageW(hwnd, NativeMethods.WM_HSCROLL, (IntPtr)NativeMethods.SBH.LEFT, IntPtr.Zero);
            }
        }
    }
}
