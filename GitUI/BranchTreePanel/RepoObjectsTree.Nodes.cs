using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.UserControls;
using JetBrains.Annotations;

namespace GitUI.BranchTreePanel
{
    partial class RepoObjectsTree
    {
        private sealed class Nodes : IEnumerable<Node>
        {
            private readonly List<Node> _nodesList = new List<Node>();

            public Tree Tree { get; }

            public Nodes(Tree tree)
            {
                Tree = tree;
            }

            public void AddNode(Node node)
            {
                _nodesList.Add(node);
            }

            public void Clear()
            {
                _nodesList.Clear();
            }

            public IEnumerator<Node> GetEnumerator() => _nodesList.GetEnumerator();

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

            /// <summary>
            /// Returns all nodes of a given TNode type using depth-first, pre-order method
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
                var prevNodes = new HashSet<Node>();
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
        }

        private abstract class Tree
        {
            protected readonly Nodes Nodes;
            private readonly IGitUICommandsSource _uiCommandsSource;

            private readonly CancellationTokenSequence _reloadCancellationTokenSequence = new CancellationTokenSequence();

            protected Tree(TreeNode treeNode, IGitUICommandsSource uiCommands)
            {
                Nodes = new Nodes(this);
                _uiCommandsSource = uiCommands;
                TreeViewNode = treeNode;

                uiCommands.UICommandsChanged += (a, e) =>
                {
                    // When GitModule has changed, clear selected node
                    if (TreeViewNode?.TreeView != null)
                    {
                        TreeViewNode.TreeView.SelectedNode = null;
                    }

                    // Also clear treeview nodes so that any previous state is flushed
                    // (e.g. expanded/collapsed state, etc.)
                    if (TreeViewNode != null)
                    {
                        TreeViewNode.Nodes.Clear();
                    }

                    e.OldCommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
                    uiCommands.UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
                };

                uiCommands.UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
            }

            private void UICommands_PostRepositoryChanged(object sender, GitUIPluginInterfaces.GitUIEventArgs e)
            {
                // Run on UI thread
                TreeViewNode.TreeView.InvokeAsync(RefreshTree).FileAndForget();
            }

            public abstract void RefreshTree();

            public TreeNode TreeViewNode { get; }
            public GitUICommands UICommands => _uiCommandsSource.UICommands;

            /// <summary>
            /// A flag to indicate that node SelectionChanged event is not user-originated and
            /// must not trigger the event handling sequence.
            /// </summary>
            public bool IgnoreSelectionChangedEvent { get; set; }
            protected GitModule Module => UICommands.Module;

            // Invoke from child class to reload nodes for the current Tree. Clears Nodes, invokes
            // input async function that should populate Nodes, then fills the tree view with its contents,
            // making sure to disable/enable the control.
            protected void ReloadNodes(Func<CancellationToken, Task> loadNodesTask)
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    var repoObjectTree = TreeViewNode.TreeView.Parent;

                    try
                    {
                        var token = _reloadCancellationTokenSequence.Next();

                        repoObjectTree.Enabled = false;

                        TreeViewNode.TreeView.BeginUpdate();
                        IgnoreSelectionChangedEvent = true;
                        Nodes.Clear();

                        await loadNodesTask(token);

                        token.ThrowIfCancellationRequested();
                        await TreeViewNode.TreeView.SwitchToMainThreadAsync();

                        FillTreeViewNode();
                    }
                    finally
                    {
                        IgnoreSelectionChangedEvent = false;
                        TreeViewNode.TreeView.EndUpdate();
                        repoObjectTree.Enabled = true;
                        ExpandPathToSelectedNode();
                    }
                }).FileAndForget();
            }

            private void FillTreeViewNode()
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                bool firstTime = TreeViewNode.Nodes.Count == 0;

                var expandedNodesState = TreeViewNode.GetExpandedNodesState();
                Nodes.FillTreeViewNode(TreeViewNode);
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
                if (TreeViewNode.TreeView.SelectedNode != null)
                {
                    SetSelectedNode(TreeViewNode.TreeView.SelectedNode);
                    EnsureNodeVisible(TreeViewNode.TreeView.SelectedNode);
                }

                if (TreeViewNode.TreeView.Nodes.Count > 0)
                {
                    // No selected node, just make sure the first node is visible
                    EnsureNodeVisible(TreeViewNode.TreeView.Nodes[0]);
                }

                return;

                void SetSelectedNode(TreeNode node)
                {
                    TreeViewNode.TreeView.SelectedNode = node;
                }

                void EnsureNodeVisible(TreeNode node)
                {
                    node.EnsureVisible();

                    // EnsureVisible leads to horizontal scrolling in some cases. We make sure to force horizontal
                    // scroll back to 0. Note that we use SendMessage rather than SetScrollPos as the former works
                    // outside of Begin/EndUpdate.
                    NativeMethods.SendMessageInt((IntPtr)TreeViewNode.TreeView.Handle, NativeMethods.WM_HSCROLL, (IntPtr)NativeMethods.SB_LEFT, IntPtr.Zero);
                }
            }
        }

        private abstract class Node
        {
            public readonly Nodes Nodes;
            protected Tree Tree => Nodes.Tree;
            protected GitUICommands UICommands => Tree.UICommands;

            protected GitModule Module => UICommands.Module;

            protected Node(Tree tree)
            {
                Nodes = new Nodes(tree);
            }

            private TreeNode _treeViewNode;
            public TreeNode TreeViewNode
            {
                get => _treeViewNode;
                set
                {
                    _treeViewNode = value;
                    _treeViewNode.Tag = this;
                    _treeViewNode.Name = DisplayText();
                    _treeViewNode.Text = DisplayText();
                    _treeViewNode.ContextMenuStrip = GetContextMenuStrip();
                    ApplyStyle();
                }
            }

            private static readonly Dictionary<Type, ContextMenuStrip> DefaultContextMenus
                = new Dictionary<Type, ContextMenuStrip>();

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

            [CanBeNull]
            protected virtual ContextMenuStrip GetContextMenuStrip()
            {
                DefaultContextMenus.TryGetValue(GetType(), out var result);
                return result;
            }

            [CanBeNull]
            protected IWin32Window ParentWindow()
            {
                return TreeViewNode.TreeView.FindForm();
            }

            public virtual string DisplayText()
            {
                return ToString();
            }

            protected void SetNodeFont(FontStyle style)
            {
                if (style == FontStyle.Regular)
                {
                    // For regular, set to null to use the NativeTreeView font
                    if (TreeViewNode.NodeFont != null)
                    {
                        TreeViewNode.NodeFont.Dispose();
                        TreeViewNode.NodeFont = null;
                    }
                }
                else
                {
                    // If current font doesn't have the input style, get rid of it
                    if (TreeViewNode.NodeFont != null && !TreeViewNode.NodeFont.Style.HasFlag(style))
                    {
                        TreeViewNode.NodeFont.Dispose();
                        TreeViewNode.NodeFont = null;
                    }

                    // If non-null, our font is already valid, otherwise create a new one
                    if (TreeViewNode.NodeFont == null)
                    {
                        TreeViewNode.NodeFont = new Font(AppSettings.Font, style);
                    }
                }
            }

            protected virtual void ApplyStyle()
            {
                SetNodeFont(FontStyle.Regular);
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

            [CanBeNull]
            private static T GetNodeSafe<T>([CanBeNull] TreeNode treeNode) where T : Node
            {
                return treeNode?.Tag as T;
            }

            public static void OnNode<T>(TreeNode treeNode, Action<T> action) where T : Node
            {
                var node = GetNodeSafe<T>(treeNode);

                if (node != null)
                {
                    action(node);
                }
            }
        }
    }
}
