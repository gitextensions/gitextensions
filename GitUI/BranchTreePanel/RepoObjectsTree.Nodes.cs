using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
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

            protected Tree(TreeNode treeNode, IGitUICommandsSource uiCommands)
            {
                Nodes = new Nodes(this);
                _uiCommandsSource = uiCommands;
                TreeViewNode = treeNode;
            }

            public TreeNode TreeViewNode { get; }
            public GitUICommands UICommands => _uiCommandsSource.UICommands;

            /// <summary>
            /// A flag to indicate that node SelectionChanged event is not user-originated and
            /// must not trigger the event handling sequence.
            /// </summary>
            public bool IgnoreSelectionChangedEvent { get; set; }
            protected GitModule Module => UICommands.Module;

            public async Task ReloadAsync(CancellationToken token)
            {
                await TreeViewNode.TreeView.SwitchToMainThreadAsync(token);
                ClearNodes();

                await LoadNodesAsync(token).ConfigureAwait(false);

                await TreeViewNode.TreeView.SwitchToMainThreadAsync(token);
                TreeViewNode.TreeView.BeginUpdate();
                try
                {
                    FillTreeViewNode();
                    if (TreeViewNode.TreeView.SelectedNode != null)
                    {
                        TreeViewNode.TreeView.SelectedNode.EnsureVisible();
                    }
                    else if (TreeViewNode.TreeView.Nodes.Count > 0)
                    {
                        TreeViewNode.TreeView.Nodes[0].EnsureVisible();
                    }
                }
                finally
                {
                    TreeViewNode.TreeView.EndUpdate();
                }
            }

            protected abstract Task LoadNodesAsync(CancellationToken token);

            protected virtual void ClearNodes()
            {
                Nodes.Clear();
            }

            protected virtual void FillTreeViewNode()
            {
                Nodes.FillTreeViewNode(TreeViewNode);
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
                protected get => _treeViewNode;
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

            protected virtual void ApplyStyle()
            {
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
