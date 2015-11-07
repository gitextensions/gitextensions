using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces.Notifications;
using ResourceManager;

namespace GitUI.UserControls
{
    // "branches"
    public partial class RepoObjectsTree
    {
        #region private classes

        /// <summary>base class for a branch node</summary>
        private abstract class BaseBranchNode : Node
        {
            protected readonly char PathSeparator = '/';

            /// <summary>Short name of the branch/branch path. <example>"issue1344"</example></summary>
            public string Name { get; protected set; }

            public string ParentPath { get; private set; }

            /// <summary>Full path of the branch. <example>"issues/issue1344"</example></summary>
            public string FullPath
            {
                get { return ParentPath.Combine(PathSeparator, Name); }
            }

            public override int GetHashCode()
            {
                return FullPath.GetHashCode();
            }

            /// <summary>Two <see cref="BaseBranchNode"/> instances are equal
            ///  if their <see cref="FullPath"/> values are equal.</summary>
            protected bool Equals(BaseBranchNode other)
            {
                if (other == null)
                {
                    return false;
                }

                return (other == this) || string.Equals(FullPath, other.FullPath);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as BaseBranchNode);
            }

            protected BaseBranchNode(Tree aTree, string aFullPath)
                : base(aTree, null)
            {
                aFullPath = aFullPath.Trim();
                if (aFullPath.IsNullOrEmpty())
                    throw new ArgumentNullException("aFullPath");

                string[] dirs = aFullPath.Split(PathSeparator);
                Name = dirs[dirs.Length - 1];
                ParentPath = dirs.Take(dirs.Length - 1).Join(PathSeparator);
            }

            internal BaseBranchNode CreateRootNode(IDictionary<string, BaseBranchNode> nodes,
                Func<Tree, string, BaseBranchNode> createPathNode)

            {
                if (ParentPath.IsNullOrEmpty())
                    return this;

                BaseBranchNode parent;
                BaseBranchNode result;

                if (nodes.TryGetValue(ParentPath, out parent))
                {
                    result = null;
                }
                else
                {
                    parent = createPathNode(Tree, ParentPath);
                    nodes.Add(ParentPath, parent);
                    result = parent.CreateRootNode(nodes, createPathNode);
                }

                parent.Nodes.AddNode(this);

                return result;
            }

            public override string DisplayText()
            {
                return Name;
            }

            protected void SelectRevision()
            {
                TreeViewNode.TreeView.BeginInvoke(new Action(() =>
                {
                    UICommands.BrowseGoToRef(FullPath, showNoRevisionMsg: true);
                    if (TreeViewNode.TreeView != null)
                    {
                        TreeViewNode.TreeView.Focus();
                    }
                }));
            }
        }

        /// <summary>Local branch node.</summary>
        private sealed class BranchNode : BaseBranchNode
        {
            public BranchNode(Tree aTree, string aFullPath)
                : base(aTree, aFullPath.TrimStart(GitModule.ActiveBranchIndicator))
            {
                IsDraggable = false;
                AllowDrop = false;
                IsActive = aFullPath.StartsWith(GitModule.ActiveBranchIndicator.ToString());
            }

            /// <summary>true if this <see cref="BranchNode"/> is the active branch.</summary>
            public bool IsActive { get; private set; }

            /// <summary>Indicates whether this <see cref="BranchNode"/> is setup for remote tracking.</summary>
            public bool IsTrackingSetup()
            {
                return !string.IsNullOrWhiteSpace(Module.LocalConfigFile.GetValue(GitRef.RemoteSettingName(FullPath)));
            }

            /// <summary>Styles the <see cref="Node.TreeViewNode"/>.</summary>
            protected override void ApplyStyle()
            {
                base.ApplyStyle();
                if (IsActive)
                {
                    TreeViewNode.NodeFont = new Font(TreeViewNode.NodeFont, FontStyle.Bold);
                }
            }

            public override bool Equals(object obj)
            {
                if (base.Equals(obj))
                {
                    BranchNode branchNode = obj as BranchNode;
                    return branchNode != null && IsActive == branchNode.IsActive;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            /// <summary>Checkout the branch.</summary>
            internal override void OnDoubleClick()
            {
                base.OnDoubleClick();
                Checkout();
            }

            internal override void OnSelected()
            {
                base.OnSelected();

                SelectRevision();
            }

            protected override IEnumerable<DragDropAction> CreateDragDropActions()
            {
                /*
                var stashDD = new DragDropAction<StashNode>(
                    (draggedStash) => IsActive,
                    (draggedStash) =>
                    {
                        // normal -> Pop
                        // Alt -> Apply
                        UICommands.StartStashDialog();
                    });
                */
                var branchDD = new DragDropAction<BranchNode>(draggedBranch =>
                {
                    string activeBranch = UICommands.Module.GetSelectedBranch();
                    if (Equals(FullPath, activeBranch))
                    {
// target is active -> merge dropped
                        return true;
                    }
                    if (Equals(draggedBranch.FullPath, activeBranch))
                    {
// dragged is active -> merge dragged
                        return true;
                    }
                    return false;
                }, draggedBranch =>
                {
                    string activeBranch = UICommands.Module.GetSelectedBranch();
                    if (Equals(FullPath, activeBranch))
                    {
// target is active -> merge dropped
                        UICommands.StartMergeBranchDialog(draggedBranch.FullPath);
                    }
                    if (Equals(draggedBranch.FullPath, activeBranch))
                    {
// dropped is active -> merge target
                        UICommands.StartMergeBranchDialog(FullPath);
                    }
                });


                return new DragDropAction[] {/*stashDD,*/ branchDD};
            }

            public void Checkout()
            {
                UICommands.StartCheckoutBranch(FullPath, false);
            }

            public void Delete()
            {
                UICommands.StartDeleteBranchDialog(ParentWindow(), new string[] {FullPath});
            }

            public void DeleteForce()
            {
                var branchHead = GitRef.CreateBranchRef(UICommands.Module, null, FullPath);
                var cmd = new GitDeleteBranchCmd(new GitRef[] {branchHead}, true);
                UICommands.StartCommandLineProcessDialog(cmd, null);
            }
        }

        private class BasePathNode : BaseBranchNode
        {
            public BasePathNode(Tree aTree, string aFullPath) : base(aTree, aFullPath)
            {
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();
                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = "folder.png";
            }
        }

        /// <summary>Part of a path leading to local branch(es)</summary>
        private class BranchPathNode : BasePathNode
        {
            /// <summary>Creates a new <see cref="BranchPathNode"/>.</summary>
            public BranchPathNode(Tree aTree, string aFullPath)
                : base(aTree, aFullPath)
            {
            }

            public override string ToString()
            {
                return string.Format("{0}{1}", Name, PathSeparator);
            }

            public void DeleteAll()
            {
                var branches = Nodes.DepthEnumerator<BranchNode>().Select(branch => branch.FullPath);
                UICommands.StartDeleteBranchDialog(ParentWindow(), branches);
            }

            public void DeleteAllForce()
            {
                var branches = Nodes.DepthEnumerator<BranchNode>();
                var branchHeads =
                    branches.Select(branch => GitRef.CreateBranchRef(UICommands.Module, null, branch.FullPath));
                var cmd = new GitDeleteBranchCmd(branchHeads, true);
                UICommands.StartCommandLineProcessDialog(cmd, null);
            }
        }

        //    /// <summary>Name of the remote for this remote branch. <example>"origin"</example></summary>
        //    public string Remote { get; private set; }
        //    /// <summary>Full name of the branch, excluding the remote name. <example>"issues/issue1344"</example></summary>
        //    public string FullBranchName { get; private set; }

        //    public RemoteBranchNode(GitUICommands uiCommands,
        //        string remote, string branch, int level, string activeBranchPath = null, BranchPathNode parent = null)
        //        : base(uiCommands, branch, level, activeBranchPath, parent, isLocal: false)
        //    {
        //        Remote = remote;
        //        FullBranchName = FullPath.Substring(FullPath.IndexOf(BranchesNode.Separator));
        //    }

        //!if (targetBranch.IsRemote)
        //{// local branch -> remote branch = push
        //    uiCommands.StartPushDialog(
        //        new GitPushAction(
        //            ((RemoteBranchNode)targetBranch).Remote,
        //            draggedBranch.FullPath,
        //            targetBranch.FullPath));
        //}
        //}


        private class BranchTree : Tree
        {
            public const char PathSeparator = '/';
            private string SelectedBranch;

            public BranchTree(TreeNode aTreeNode, IGitUICommandsSource uiCommands)
                : base(aTreeNode, uiCommands)
            {
                uiCommands.GitUICommandsChanged += uiCommands_GitUICommandsChanged;
            }

            private void uiCommands_GitUICommandsChanged(object sender, GitUICommandsChangedEventArgs e)
            {
                //select active branch after repo change
                TreeViewNode.TreeView.SelectedNode = null;
                SelectedBranch = null;
            }

            protected override void LoadNodes(System.Threading.CancellationToken token)
            {
                FillBranchTree(Module.GetBranchNames());
            }

            /// <summary>Gets the hierarchical branch tree from the specified list of <paramref name="branches"/>.</summary>
            public void FillBranchTree(IEnumerable<string> branches)
            {
                #region ex

                // (input)
                // a-branch
                // develop/crazy-branch
                // develop/features/feat-next
                // develop/features/feat-next2
                // develop/issues/iss444
                // develop/wild-branch
                // issues/iss111
                // master
                //
                // ->
                // (output)
                // 0 a-branch
                // 0 develop/
                // 1   features/
                // 2      feat-next
                // 2      feat-next2
                // 1   issues/
                // 2      iss444
                // 1   wild-branch
                // 1   wilds/
                // 2      card
                // 0 issues/
                // 1     iss111
                // 0 master

                #endregion ex

                Dictionary<string, BaseBranchNode> nodes = new Dictionary<string, BaseBranchNode>();
                foreach (string branch in branches)
                {
                    BranchNode branchNode = new BranchNode(this, branch);
                    BaseBranchNode parent = branchNode.CreateRootNode(nodes,
                        (tree, parentPath) => new BranchPathNode(tree, parentPath));
                    if (parent != null)
                        Nodes.AddNode(parent);
                }
            }

            protected override void FillTreeViewNode()
            {
                var selectedNode = Node.GetNodeSafe<BranchNode>(TreeViewNode.TreeView.SelectedNode);
                if (selectedNode != null)
                {
                    SelectedBranch = selectedNode.FullPath;
                }

                base.FillTreeViewNode();

                TreeViewNode.Text = string.Format("{0} ({1})", Strings.branches, Nodes.Count);

                if (SelectedBranch != null)
                {
                    selectedNode =
                        Nodes.DepthEnumerator<BranchNode>()
                            .Where(b => b.FullPath.Equals(SelectedBranch))
                            .FirstOrDefault();
                    if (selectedNode != null)
                    {
                        selectedNode.Select();
                        return;
                    }
                }

                var activeBranch = Nodes.DepthEnumerator<BranchNode>().Where(b => b.IsActive).FirstOrDefault();
                if (activeBranch == null)
                {
                    TreeViewNode.TreeView.SelectedNode = null;
                }
                else
                {
                    activeBranch.Select();
                    SelectedBranch = activeBranch.FullPath;
                }
            }
        }
    #endregion private classes

    }
}
