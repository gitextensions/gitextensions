using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;

namespace GitUI.UserControls
{
    partial class RepoObjectsTree
    {
        class Nodes
        {
            public readonly Tree Tree;
            public readonly Node OwnerNode;
            private readonly IList<Node> NodesList = new List<Node>();

            public Nodes(Tree aTree, Node aOwnerNode)
            {
                Tree = aTree;
                OwnerNode = aOwnerNode;
            }

            public virtual void AddNode(Node aNode)
            {
                NodesList.Add(aNode);
                aNode.ParentNode = OwnerNode;
            }

            public void Clear()
            {
                NodesList.Clear();
            }

            public void Remove(Node aNode)
            {
                NodesList.Remove(aNode);
            }

            public IEnumerator<Node> GetEnumerator()
            {
                var e = NodesList.GetEnumerator();
                return e;
            }

            /// <summary>
            /// Returns all nodes of a given TNode type using depth-first, pre-order method
            /// </summary>
            /// <typeparam name="TNode"></typeparam>
            /// <returns></returns>
            public IEnumerable<TNode> DepthEnumerator<TNode>() where TNode : Node
            {
                foreach (var node in this)
                {
                    if(node is TNode)
                        yield return (TNode)node;

                    foreach (var subnode in node.Nodes.DepthEnumerator<TNode>())
                        yield return subnode;
                }
            }

            internal void FillTreeViewNode(TreeNode aTreeViewNode)
            {
                HashSet<Node> prevNodes = new HashSet<Node>();
                for (int i = 0; i < aTreeViewNode.Nodes.Count; i++)
                {
                    var tvNode = aTreeViewNode.Nodes[i];
                    prevNodes.Add(Node.GetNode(tvNode));
                }

                int oldNodeIdx = 0;
                foreach (Node node in this)
                {
                    TreeNode tvNode;

                    if (oldNodeIdx < aTreeViewNode.Nodes.Count)
                    {
                        tvNode = aTreeViewNode.Nodes[oldNodeIdx];
                        Node oldNode = Node.GetNode(tvNode);
                        if (!oldNode.Equals(node) && !prevNodes.Contains(node))
                        {
                            tvNode = aTreeViewNode.Nodes.Insert(oldNodeIdx, string.Empty);
                        }
                    }
                    else
                    {
                        tvNode = aTreeViewNode.Nodes.Add(string.Empty);
                    }

                    node.TreeViewNode = tvNode;
                    //recurse to subnodes
                    node.Nodes.FillTreeViewNode(tvNode);
                    oldNodeIdx++;
                }

                while (oldNodeIdx < aTreeViewNode.Nodes.Count)
                {
                    aTreeViewNode.Nodes.RemoveAt(oldNodeIdx);
                }

            }

            public int Count { get { return NodesList.Count; } }
        }

        abstract class Tree
        {
            public readonly Nodes Nodes;
            private readonly IGitUICommandsSource UICommandsSource;
            public GitUICommands UICommands { get { return UICommandsSource.UICommands; } }
            public GitModule Module { get { return UICommands.Module; } }
            public TreeNode TreeViewNode { get; private set; }
            public Action<List<string>> OnBranchesAdded;

            public Tree(TreeNode aTreeNode, IGitUICommandsSource uiCommands)
            {
                Nodes = new Nodes(this, null);
                UICommandsSource = uiCommands;
                TreeViewNode = aTreeNode;
            }

            public Task ReloadTask(CancellationToken token)
            {
                ClearNodes();
                Task task = new Task(() => LoadNodes(token), token);
                Action<Task> continuationAction = (t) =>
                    {
                        TreeViewNode.TreeView.BeginUpdate();
                        try
                        {
                            FillTreeViewNode();
                        }
                        finally
                        {
                            if (TreeViewNode.TreeView.SelectedNode != null)
                            {
                                TreeViewNode.TreeView.SelectedNode.EnsureVisible();
                            }
                            else if(TreeViewNode.TreeView.Nodes.Count > 0)
                            {
                                TreeViewNode.TreeView.Nodes[0].EnsureVisible();
                            }
                            TreeViewNode.TreeView.EndUpdate();
                        }
                    };

                task.ContinueWith(continuationAction, token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.FromCurrentSynchronizationContext());
                return task;
            }

            protected abstract void LoadNodes(CancellationToken token);

            protected virtual void ClearNodes()
            {
                Nodes.Clear();
            }

            protected virtual void FillTreeViewNode()
            {
                Nodes.FillTreeViewNode(TreeViewNode);
            }

            protected void FireBranchAddedEvent(List<string> branchFullPaths)
            {
                if (OnBranchesAdded != null)
                {
                    OnBranchesAdded(branchFullPaths);
                }
            }
        }

        abstract class Node
        {
            /// <summary>Gets the parent node.</summary>
            public Node ParentNode { get; internal set; }
            public readonly Nodes Nodes;
            public Tree Tree { get { return Nodes.Tree; } }
            /// <summary>Gets the <see cref="GitUICommands"/> reference.</summary>
            public GitUICommands UICommands { get { return Tree.UICommands; } }
            /// <summary>Gets the <see cref="GitModule"/> reference.</summary>
            public GitModule Module { get { return UICommands.Module; } }

            protected Node(Tree aTree, Node aParentNode)
            {
                Nodes = new Nodes(aTree, this);
                //Notifier = NotificationManager.Get(UICommands);
                ParentNode = aParentNode;
            }

            TreeNode _TreeViewNode;
            /// <summary>Gets the <see cref="TreeViewNode"/> which holds this <see cref="Node"/>.
            /// <remarks>Setting this value will automatically call <see cref="ApplyStyle"/>.</remarks></summary>
            public TreeNode TreeViewNode
            {
                get { return _TreeViewNode; }
                internal set
                {
                    _TreeViewNode = value;
                    _TreeViewNode.Tag = this;
                    _TreeViewNode.Text = DisplayText();
                    _TreeViewNode.ContextMenuStrip = GetContextMenuStrip();
                    ApplyStyle();
                }
            }

            private static Dictionary<Type, ContextMenuStrip> DefaultContextMenus = new Dictionary<Type, ContextMenuStrip>();

            public static void RegisterContextMenu(Type aType, ContextMenuStrip aMenu)
            {
                if (DefaultContextMenus.ContainsKey(aType))
                {
                    // the translation unit test may create the RepoObjectTree multiple times,
                    // which results in a duplicate key exception.
                    return;
                }
                DefaultContextMenus.Add(aType, aMenu);
            }

            protected virtual ContextMenuStrip GetContextMenuStrip()
            {
                ContextMenuStrip result = null;
                DefaultContextMenus.TryGetValue(GetType(), out result);
                return result;
            }

            public IWin32Window ParentWindow()
            {
                return TreeViewNode.TreeView.FindForm();
            }

            public virtual string DisplayText()
            {
                return ToString();
            }

            /// <summary>Styles the <see cref="TreeViewNode"/>.</summary>
            protected virtual void ApplyStyle()
            {
                TreeViewNode.NodeFont = AppSettings.Font;
            }

            public void Select()
            {
                if (TreeViewNode.TreeView.SelectedNode == TreeViewNode)
                {
                    OnSelected();
                }
                else
                {
                    TreeViewNode.TreeView.SelectedNode = TreeViewNode;
                }
            }

            /// <summary>Occurs when the <see cref="Node"/> is selected.</summary>
            internal virtual void OnSelected() { }
            /// <summary>Occurs when the <see cref="Node"/> is clicked.</summary>
            internal virtual void OnClick() { }
            /// <summary>Occurs when the <see cref="Node"/> is double-clicked.</summary>
            internal virtual void OnDoubleClick() { }

            /// <summary>Gets the <see cref="Node"/> from a <see cref="TreeViewNode"/>'s tag.</summary>
            public static Node GetNode(TreeNode treeNode)
            {
                return (Node)treeNode.Tag;
            }

            /// <summary>Casts the <see cref="System.Windows.Forms.TreeNode.Tag"/> to a <see cref="Node"/>.</summary>
            public static T GetNodeSafe<T>(TreeNode treeNode) where T : Node
            {
                if (treeNode == null)
                    return null;

                return treeNode.Tag as T;
            }

            /// <summary>Executes an action if <see cref="TreeViewNode"/> holds a <see cref="Node"/>.</summary>
            public static bool OnNode<T>(TreeNode treeNode, Action<T> action) where T : Node
            {
                T node = GetNodeSafe<T>(treeNode);
                if (node != null)
                {
                    action(node);
                    return true;
                }
                return false;
            }
        }
    }
}
