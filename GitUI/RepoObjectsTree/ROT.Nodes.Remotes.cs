using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUI.CommandsDialogs;
using GitUI.HelperDialogs;

namespace GitUI.UserControls
{
    // "remotes"
    public partial class RepoObjectsTree
    {
        // commits in current branch but NOT remote branch
        // git rev-list {remote}/{branch}..{branch}

        // commits in remote branch but NOT in current branch
        // git rev-list {branch}..{remote}/{branch}

        // commits in either, but NOT in both
        // git rev-list {ref1}...{ref2}

        // $ git remote
        // $ git remote show {remote}

        // $ git for-each-ref --sort=-upstream --format='%(upstream:short) <- %(refname:short)' refs/heads
        // master <- origin/master
        // pu <- origin/pu

        private class RemoteBranchTree : Tree
        {
            public RemoteBranchTree(TreeNode aTreeNode, IGitUICommandsSource uiCommands) : base(aTreeNode, uiCommands)
            {
                uiCommands.GitUICommandsChanged += uiCommands_GitUICommandsChanged;
            }

            private void uiCommands_GitUICommandsChanged(object sender, GitUICommandsChangedEventArgs e)
            {
                TreeViewNode.TreeView.SelectedNode = null;
            }

            protected override void LoadNodes(CancellationToken token)
            {
                var nodes = new Dictionary<string, BaseBranchNode>();

                var branches = Module.GetRefs(true, true)
                    .Where(branch => branch.IsRemote && !branch.IsTag)
                    .OrderBy(r => r.Name)
                    .Select(branch => branch.Name);

                var remotes = Module.GetRemotes(allowEmpty: true);
                var branchFullPaths = new List<string>();
                foreach (var branchPath in branches)
                {
                    var remote = branchPath.Split('/').First();
                    if (!remotes.Contains(remote))
                    {
                        continue;
                    }
                    var remoteBranchNode = new RemoteBranchNode(this, branchPath);
                    var parent = remoteBranchNode.CreateRootNode(nodes,
                        (tree, parentPath) => CreateRemoteBranchPathNode(tree, parentPath, remote));
                    if (parent != null)
                        Nodes.AddNode(parent);
                    branchFullPaths.Add(remoteBranchNode.FullPath);
                }
                FireBranchAddedEvent(branchFullPaths);
            }

            private static BaseBranchNode CreateRemoteBranchPathNode(Tree tree,
                string parentPath, string remoteName)
            {
                if (parentPath == remoteName)
                {
                    return new RemoteRepoNode(tree, parentPath);
                }
                return new BasePathNode(tree, parentPath);
            }

            protected override void FillTreeViewNode()
            {
                base.FillTreeViewNode();
                TreeViewNode.Expand();
            }

            public void RenameOrAddRemote(string orgName, string newName)
            {
                var treeNode = FindRemoteRepoTreeNodeByName(orgName);
                if (treeNode == null)
                {
                    AddRemote(newName);
                    return;
                }

                treeNode.Text = newName;
                var remoteRepoNode = FindRemoteRepoNodeByName(orgName);
                remoteRepoNode.ChangeName(newName);
            }

            private RemoteRepoNode FindRemoteRepoNodeByName(string remoteName)
            {
                foreach (var node in Nodes)
                {
                    if (node.DisplayText() != remoteName)
                    {
                        continue;
                    }
                    var remoteRepoNode = node as RemoteRepoNode;
                    if (remoteRepoNode != null)
                    {
                        return remoteRepoNode;
                    }
                }
                return null;
            }

            private TreeNode FindRemoteRepoTreeNodeByName(string remoteName)
            {
                return TreeViewNode.Nodes.Cast<TreeNode>().FirstOrDefault(treeNode => treeNode.Text == remoteName);
            }

            public void DeleteRemote(string remoteName)
            {
                var treeNode = FindRemoteRepoTreeNodeByName(remoteName);
                if (treeNode == null)
                {
                    return;
                }
                TreeViewNode.Nodes.Remove(treeNode);
                var repoNode = FindRemoteRepoNodeByName(remoteName);
                Nodes.Remove(repoNode);
            }

            public void AddRemote(string remoteName)
            {
                Nodes.AddNode(new RemoteRepoNode(this, remoteName));
                Nodes.FillTreeViewNode(this.TreeViewNode);
            }
        }

        /// <summary>for a branch on a remote repo.</summary>
        sealed class RemoteBranchNode : BaseBranchNode
        {
            public RemoteBranchNode(Tree aTree, string aFullPath) : base(aTree, aFullPath)
            {
            }

            internal override void OnSelected()
            {
                base.OnSelected();
                SelectRevision();
            }

            /// <summary>Download updates from the remote branch.</summary>
            public void Fetch()
            {
                var remoteBranchInfo = GetRemoteBranchInfo();
                var cmd = Module.FetchCmd(remoteBranchInfo.Remote, remoteBranchInfo.BranchName,
                    null, null);
                var ret = FormRemoteProcess.ShowDialog(TreeViewNode.TreeView, Module, cmd);
                if (ret)
                {
                    UICommands.RepoChangedNotifier.Notify();
                }
            }

            private struct RemoteBranchInfo
            {
                public string Remote { get; set; }

                public string BranchName { get; set; }
            }

            private RemoteBranchInfo GetRemoteBranchInfo()
            {
                var remote = FullPath.Split('/').First();
                var branch = FullPath.Substring(remote.Length + 1);
                return new RemoteBranchInfo {Remote = remote, BranchName = branch};
            }

            public void Pull()
            {
                bool pullCompleted = false;
                var remoteBranchInfo = GetRemoteBranchInfo();
                UICommands.StartPullDialog(this.TreeViewNode.TreeView, pullOnShow: false,
                    remoteBranch: remoteBranchInfo.BranchName, remote: remoteBranchInfo.Remote,
                    pullCompleted: out pullCompleted, fetchAll: false);
            }

            /// <summary>Create a local branch from the remote branch.</summary>
            public void CreateBranch()
            {
                UICommands.StartCreateBranchDialog(this.TreeViewNode.TreeView, new GitRevision(Module, FullPath));
            }

            /// <summary>Delete the branch on the remote repository.</summary>
            public void Delete()
            {
                var remoteBranchInfo = GetRemoteBranchInfo();
                var cmd = new GitDeleteRemoteBranchCmd(remoteBranchInfo.Remote, remoteBranchInfo.BranchName);
                if (MessageBoxes.ConfirmDeleteRemoteBranch(TreeViewNode.TreeView, remoteBranchInfo.BranchName, remoteBranchInfo.Remote))
                {
                    UICommands.StartCommandLineProcessDialog(cmd, null);
                }
            }

            public void Checkout()
            {
                using (var form = new FormCheckoutBranch(UICommands, FullPath, remote: true))
                {
                    form.ShowDialog(TreeViewNode.TreeView);
                }
            }

            internal override void OnDoubleClick()
            {
                Checkout();
            }

            public void Merge()
            {
                using (var form = new FormMergeBranch(UICommands, FullPath))
                {
                    form.ShowDialog(TreeViewNode.TreeView);
                }
            }

            public void Rebase()
            {
                using (var form = new FormRebase(UICommands, FullPath))
                {
                    form.ShowDialog(TreeViewNode.TreeView);
                }
            }

            public void Reset()
            {
                using (var form = new FormResetCurrentBranch(UICommands, new GitRevision(Module, FullPath)))
                {
                    form.ShowDialog(TreeViewNode.TreeView);
                }
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();
                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = "RemoteBranch.png";
            }
        }

        sealed class RemoteRepoNode : BaseBranchNode
        {
            public RemoteRepoNode(Tree aTree, string aFullPath) : base(aTree, aFullPath)
            {
            }

            public void Fetch()
            {
                var cmd = Module.FetchCmd(FullPath, null, null, null);
                var ret = FormRemoteProcess.ShowDialog(TreeViewNode.TreeView, Module, cmd);
                if (ret)
                {
                    UICommands.RepoChangedNotifier.Notify();
                }
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();
                this.TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = "RemoteRepo.png";
            }

            public void ChangeName(string newName)
            {
                Name = newName;
            }
        }
    }
}
