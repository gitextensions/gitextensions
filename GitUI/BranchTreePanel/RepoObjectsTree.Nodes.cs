using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.BranchTreePanel.Interfaces;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitUI.BranchTreePanel
{
    partial class RepoObjectsTree
    {
        private sealed class Nodes : IEnumerable<Node>
        {
            private readonly List<Node> _nodesList = new();

            public Tree? Tree { get; }

            public Nodes(Tree? tree)
            {
                Tree = tree;
            }

            /// <summary>
            /// Adds a new node to the collection.
            /// </summary>
            /// <param name="node">The node to add.</param>
            public void AddNode(Node node)
            {
                _nodesList.Add(node);
            }

            public void AddNodes(IEnumerable<Node> nodes)
            {
                _nodesList.AddRange(nodes);
            }

            public void Clear()
            {
                _nodesList.Clear();
            }

            public IEnumerator<Node> GetEnumerator() => _nodesList.GetEnumerator();

            public void InsertNode(int index, Node node) => _nodesList.Insert(index, node);

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

            /// <summary>
            /// Returns all nodes of a given TNode type using depth-first, pre-order method.
            /// </summary>
            public IEnumerable<TNode> DepthEnumerator<TNode>() where TNode : Node
            {
                foreach (var node in this)
                {
                    if (node is TNode node1)
                    {
                        yield return node1;
                    }

                    foreach (var subNode in node.Nodes.DepthEnumerator<TNode>())
                    {
                        yield return subNode;
                    }
                }
            }

            /// <summary>
            /// This function is responsible for building the TreeNode structure that matches this Nodes's
            /// structure, recursively. To avoid needlessly recreating TreeNodes, it recycles existing ones
            /// if they are considered equal, so it's important to implement Equals on Node classes.
            /// </summary>
            internal void FillTreeViewNode(TreeNode treeViewNode)
            {
                HashSet<Node> prevNodes = new();
                for (var i = 0; i < treeViewNode.Nodes.Count; i++)
                {
                    prevNodes.Add(Node.GetNode(treeViewNode.Nodes[i]));
                }

                var oldNodeIdx = 0;
                foreach (var node in this)
                {
                    TreeNode treeNode;

                    if (oldNodeIdx < treeViewNode.Nodes.Count)
                    {
                        treeNode = treeViewNode.Nodes[oldNodeIdx];
                        var oldNode = Node.GetNode(treeNode);
                        if (!oldNode.Equals(node) && !prevNodes.Contains(node))
                        {
                            treeNode = treeViewNode.Nodes.Insert(oldNodeIdx, string.Empty);
                        }
                    }
                    else
                    {
                        treeNode = treeViewNode.Nodes.Add(string.Empty);
                    }

                    node.TreeViewNode = treeNode;
                    node.Nodes.FillTreeViewNode(treeNode);
                    oldNodeIdx++;
                }

                while (oldNodeIdx < treeViewNode.Nodes.Count)
                {
                    treeViewNode.Nodes.RemoveAt(oldNodeIdx);
                }
            }

            public int Count => _nodesList.Count;

            public Node? LastNode => _nodesList.Count > 0 ? _nodesList[_nodesList.Count - 1] : null;
        }

        private abstract class Tree : IDisposable
        {
            protected readonly Nodes Nodes;
            private readonly IGitUICommandsSource _uiCommandsSource;
            private readonly CancellationTokenSequence _reloadCancellationTokenSequence = new();
            private bool _firstReloadNodesSinceModuleChanged = true;

            // A flag to indicate whether the data is currently being filtered or not.
            // This helps to reduce unnecessary treeview rebinds.
            private bool _isCurrentlyFiltering;

            // A flag to indicate whether the data is being filtered (e.g. Show Current Branch Only).
            private protected static AsyncLocal<bool> IsFiltering = new();

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

                    // Rebind callbacks
                    if (e.OldCommands is not null)
                    {
                        e.OldCommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
                    }

                    uiCommands.UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
                };

                uiCommands.UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
            }

            public void Dispose()
            {
                Detached();
                _reloadCancellationTokenSequence.Dispose();
            }

            private void UICommands_PostRepositoryChanged(object sender, GitUIEventArgs e)
            {
                if (!IsAttached)
                {
                    return;
                }

                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    await PostRepositoryChangedAsync();
                }).FileAndForget();
            }

            protected virtual Task PostRepositoryChangedAsync()
            {
                return Task.CompletedTask;
            }

            public TreeNode TreeViewNode { get; }
            public GitUICommands UICommands => _uiCommandsSource.UICommands;

            /// <summary>
            /// A flag to indicate that node SelectionChanged event is not user-originated and
            /// must not trigger the event handling sequence.
            /// </summary>
            public bool IgnoreSelectionChangedEvent { get; set; }
            protected GitModule Module => UICommands.Module;
            protected bool IsAttached { get; private set; }
            protected virtual bool SupportsFiltering { get; } = false;

            public Task AttachedAsync()
            {
                IsAttached = true;
                return OnAttachedAsync();
            }

            protected virtual Task OnAttachedAsync()
            {
                return Task.CompletedTask;
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
            protected internal virtual void Refresh()
            {
                // NOTE: descendants may need to break their local caches to ensure the latest data is loaded.

                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    IsFiltering.Value = _isCurrentlyFiltering;
                    await ReloadNodesAsync(LoadNodesAsync);
                });
            }

            /// <summary>
            /// Requests to refresh the data tree and to apply filtering, if necessary.
            /// </summary>
            /// <param name="isFiltering">
            ///  <see langword="true"/>, if the data is being filtered; otherwise <see langword="false"/>.
            /// </param>
            internal void ToggleFilterMode(bool isFiltering)
            {
                // If we're not currently filtering and no need to filter now -> exit.
                // Else we need to iterate over the list and rebind the tree - whilst there
                // could be a situation whether a user just refreshed the grid, there could
                // also be a situation where the user applied a different filter, or checked
                // out a different ref (e.g. a branch or commit), and we have a different
                // set of branches to show/hide.

                if (!SupportsFiltering || (!isFiltering && !_isCurrentlyFiltering))
                {
                    return;
                }

                _isCurrentlyFiltering = isFiltering;

                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    IsFiltering.Value = true;
                    await ReloadNodesAsync(LoadNodesAsync);
                });
            }

            protected abstract Task<Nodes> LoadNodesAsync(CancellationToken token);

            public IEnumerable<TNode> DepthEnumerator<TNode>() where TNode : Node
                => Nodes.DepthEnumerator<TNode>();

            // Invoke from child class to reload nodes for the current Tree. Clears Nodes, invokes
            // input async function that should populate Nodes, then fills the tree view with its contents,
            // making sure to disable/enable the control.
            protected async Task ReloadNodesAsync(Func<CancellationToken, Task<Nodes>> loadNodesTask)
            {
                var token = _reloadCancellationTokenSequence.Next();

                var treeView = TreeViewNode.TreeView;
                if (treeView is null || !IsAttached)
                {
                    return;
                }

                // Module is invalid in Dashboard
                Nodes newNodes = Module.IsValidGitWorkingDir() ? await loadNodesTask(token) : new(tree: null);

                await treeView.SwitchToMainThreadAsync(token);

                Nodes.Clear();
                Nodes.AddNodes(newNodes);

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

        private abstract class Node : INode
        {
            public readonly Nodes Nodes;

            protected Tree Tree
            {
                get
                {
                    Validates.NotNull(Nodes.Tree);
                    return Nodes.Tree;
                }
            }

            protected GitUICommands UICommands => Tree.UICommands;

            protected GitModule Module => UICommands.Module;

            protected Node(Tree? tree)
            {
                Nodes = new Nodes(tree);
            }

            private TreeNode? _treeViewNode;

            public TreeNode TreeViewNode
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
                    _treeViewNode.ContextMenuStrip = GetContextMenuStrip();
                    ApplyText();
                    ApplyStyle();
                }
            }

            private static readonly Dictionary<Type, ContextMenuStrip> DefaultContextMenus
                = new();

            public static void RegisterContextMenu(Type type, ContextMenuStrip menu)
            {
                if (DefaultContextMenus.ContainsKey(type))
                {
                    // the translation unit test may create the RepoObjectTree multiple times,
                    // which results in a duplicate key exception.
                    return;
                }

                DefaultContextMenus.Add(type, menu);
            }

            protected virtual ContextMenuStrip? GetContextMenuStrip()
            {
                DefaultContextMenus.TryGetValue(GetType(), out var result);
                return result;
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

            protected void SetNodeFont(FontStyle style)
            {
                if (style == FontStyle.Regular)
                {
                    // For regular, set to null to use the NativeTreeView font
                    if (TreeViewNode.NodeFont is not null)
                    {
                        TreeViewNode.NodeFont.Dispose();
                        TreeViewNode.NodeFont = null;
                    }
                }
                else
                {
                    // If current font doesn't have the input style, get rid of it
                    if (TreeViewNode.NodeFont is not null && !TreeViewNode.NodeFont.Style.HasFlag(style))
                    {
                        TreeViewNode.NodeFont.Dispose();
                        TreeViewNode.NodeFont = null;
                    }

                    // If non-null, our font is already valid, otherwise create a new one
                    TreeViewNode.NodeFont ??= new Font(AppSettings.Font, style);
                }
            }

            protected void ApplyText()
            {
                Validates.NotNull(_treeViewNode);

                _treeViewNode.Name = NodeName();
                _treeViewNode.Text = DisplayText();
            }

            protected virtual void ApplyStyle()
            {
                SetNodeFont(FontStyle.Regular);
                TreeViewNode.ToolTipText = string.Empty;
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
}
