using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        internal sealed class Nodes : IEnumerable<Node>
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

            public IEnumerator<Node> GetEnumerator()
                => _nodesList.GetEnumerator();

            public void InsertNode(int index, Node node)
                => _nodesList.Insert(index, node);

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                => GetEnumerator();

            /// <summary>
            /// Returns all nodes of a given TNode type using depth-first, pre-order method.
            /// </summary>
            public IEnumerable<TNode> DepthEnumerator<TNode>() where TNode : NodeBase
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

        /// <summary>A common base class for both <see cref="Node"/> and <see cref="Tree"/>.</summary>
        internal abstract class NodeBase
        {
            /// <summary>The child nodes.</summary>
            protected internal Nodes Nodes { get; protected set; }

            /// <summary>The corresponding tree node.</summary>
            protected internal virtual TreeNode TreeViewNode { get; set; }

            /// <summary>
            /// Marks this node to be included in multi-selection. See <see cref="Select(bool, bool)"/>.
            /// This is remembered here instead of relying on the status of <see cref="TreeViewNode"/>
            /// because <see cref="Nodes.FillTreeViewNode(TreeNode)"/> recycles <see cref="TreeNode"/>s
            /// and may change the association between <see cref="Node"/> and <see cref="TreeNode"/>.
            /// </summary>
            protected internal bool IsSelected { get; set; }

            protected internal void Select(bool select, bool includingDescendants = false)
            {
                IsSelected = select;
                ApplyStyle(); // toggle multi-selected node style

                // recursively process descendants if required
                if (includingDescendants && this.HasChildren())
                {
                    foreach (var child in Nodes)
                    {
                        child.Select(select, includingDescendants);
                    }
                }
            }

            #region style / appearance
            protected virtual void ApplyStyle()
            {
                SetFont(GetFontStyle());
                TreeViewNode.ToolTipText = string.Empty;
            }

            protected virtual FontStyle GetFontStyle()
                => IsSelected ? FontStyle.Underline : FontStyle.Regular;

            private void SetFont(FontStyle style)
            {
                if (style == FontStyle.Regular)
                {
                    // For regular, set to null to use the NativeTreeView font
                    if (TreeViewNode.NodeFont is not null)
                    {
                        ResetFont();
                    }
                }
                else
                {
                    // If current font doesn't have the input style, get rid of it
                    if (TreeViewNode.NodeFont is not null && TreeViewNode.NodeFont.Style != style)
                    {
                        ResetFont();
                    }

                    // If non-null, our font is already valid, otherwise create a new one
                    TreeViewNode.NodeFont ??= new Font(AppSettings.Font, style);
                }
            }

            private void ResetFont()
            {
                TreeViewNode.NodeFont.Dispose();
                TreeViewNode.NodeFont = null;
            }
            #endregion
        }

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
                var multiSelected = this.GetMultiSelection().Select(node => node.GetHashCode()).ToArray();

                Nodes.Clear();
                Nodes.AddNodes(newNodes);

                // re-apply multi-selection
                if (multiSelected.Length > 0)
                {
                    this.GetNodesAndSelf().Where(node => multiSelected.Contains(node.GetHashCode()))
                        .ForEach(node => node.IsSelected = true);
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
                    _treeViewNode.ContextMenuStrip = GetContextMenuStrip();
                    ApplyText();
                    ApplyStyle();
                }
            }

            private static readonly Dictionary<Type, ContextMenuStrip> DefaultContextMenus = new();

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

    internal static class NodeExtensions
    {
        internal static IEnumerable<RepoObjectsTree.NodeBase> GetNodesAndSelf(this RepoObjectsTree.Tree tree)
            => tree.DepthEnumerator<RepoObjectsTree.NodeBase>().Prepend(tree);

        internal static IEnumerable<RepoObjectsTree.NodeBase> GetMultiSelection(this RepoObjectsTree.Tree tree)
            => tree.GetNodesAndSelf().Where(node => node.IsSelected);

        internal static bool HasChildren(this RepoObjectsTree.NodeBase node)
            => node.Nodes.Count > 0;

        internal static IEnumerable<RepoObjectsTree.NodeBase> HavingChildren(this IEnumerable<RepoObjectsTree.NodeBase> nodes)
            => nodes.Where(node => node.HasChildren());

        internal static IEnumerable<RepoObjectsTree.NodeBase> Expandable(this IEnumerable<RepoObjectsTree.NodeBase> nodes)
            => nodes.Where(node => !node.TreeViewNode.IsExpanded);

        internal static IEnumerable<RepoObjectsTree.NodeBase> Collapsible(this IEnumerable<RepoObjectsTree.NodeBase> nodes)
            => nodes.Where(node => node.TreeViewNode.IsExpanded);
    }
}
