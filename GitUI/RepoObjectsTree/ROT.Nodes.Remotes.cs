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

        static readonly string remoteKey = Guid.NewGuid().ToString();
        static readonly string remotePushMirrorKey = Guid.NewGuid().ToString();
        static readonly string remotesKey = Guid.NewGuid().ToString();
        static readonly string remoteBranchStaleKey = Guid.NewGuid().ToString();
        static readonly string remoteBranchNewKey = Guid.NewGuid().ToString();
        static readonly string remoteBranchUnTrackedKey = Guid.NewGuid().ToString();

        /*
        /// <summary>Reloads the remotes.</summary>
        static void OnReloadRemotes(ICollection<RemoteNode> remotes, Tree<RemoteNode> remotesNode)
        {
            remotesNode.TreeNode.Text = string.Format("{0} ({1})", Strings.remotes, remotes.Count);
        }

        /// <summary>Adds the specified <paramref name="remoteNode"/> to the remotes tree.</summary>
        TreeNode OnAddRemote(TreeNodeCollection nodes, RemoteNode remoteNode)
        {
            RemoteInfo remote = remoteNode.Value;
            bool hasSameURLs = remote.PushUrls.Count() == 1 && Equals(remote.PushUrls.First(), remote.FetchUrl);

            string remoteImgKey = remote.IsMirror ? remotePushMirrorKey : remoteKey;

            string remoteTip;
            if (remote.IsMirror)
            {// setup as push mirror
                remoteTip = string.Format(Strings.RemoteMirrorsTipFormat.Text,
                    hasSameURLs
                        ? remote.FetchUrl
                        : remote.PushUrls.First());
            }
            else
            {
                remoteTip = hasSameURLs
                    ? remote.FetchUrl
                    : Strings.RemoteDifferingUrlsTip.Text;
            }

            TreeNode treeNode = new TreeNode(remote.Name)
            {
                //Name = string.Format("remotes{0}", remote.Name),TODO: branch name may have invalid chars for Control.Name
                ToolTipText = remoteTip,
                ContextMenuStrip = menuRemote,
                ImageKey = remoteImgKey,
                SelectedImageKey = remoteImgKey,
            };
            nodes.Add(treeNode);
            treeNode.Nodes.AddRange(remoteNode.Children.Select(child =>
            {
                RemoteInfo.RemoteTrackingBranch remoteTrackingBranch = child.Value;
                string imgKey = branchKey;
                string toolTip = remoteTrackingBranch.Name;//Module.CompareCommits();
                ContextMenuStrip menu = menuRemoteBranchTracked;
                if (remoteTrackingBranch.IsHead)
                {
                    imgKey = headBranchKey;
                }
                else if (remoteTrackingBranch.Status == RemoteInfo.RemoteTrackingBranch.State.Stale)
                {
                    imgKey = remoteBranchStaleKey;
                    toolTip = string.Format(Strings.RemoteBranchStaleTipFormat.Text, remoteTrackingBranch.Name);
                    menu = menuRemoteBranchStale;
                }
                else if (remoteTrackingBranch.Status == RemoteInfo.RemoteTrackingBranch.State.New)
                {
                    imgKey = remoteBranchNewKey;
                    toolTip = string.Format(Strings.RemoteBranchNewTipFormat.Text, remoteTrackingBranch.Name);
                    menu = menuRemoteBranchNew;
                }
                else
                {// explicitly UN-tracked -> grey, sort to bottom of list
                    // need to parse from settings
                    imgKey = remoteBranchUnTrackedKey;
                    menu = menuRemoteBranchUnTracked;
                    throw new NotImplementedException();
                }
                TreeNode childTreeNode = new TreeNode(remoteTrackingBranch.Name)
                {
                    ContextMenuStrip = menu,
                    ImageKey = imgKey,
                    SelectedImageKey = imgKey,
                    ToolTipText = toolTip,
                };
                child.TreeNode = childTreeNode;
                return childTreeNode;
            }).ToArray());

            return treeNode;
        }

        /// <summary><see cref="Node"/> for a remote repo.</summary>
        sealed class RemoteNode : ParentNode<RemoteInfo, RemoteBranchNode>
        {
            public RemoteNode(RemoteInfo remote, GitUICommands uiCommands)
                : base(uiCommands, remote, remote.RemoteTrackingBranches.Select(b => new RemoteBranchNode(uiCommands, b))) { }
        }
        */

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
                    .Select(branch => branch.Name);

                var remotes = Module.GetRemotes(allowEmpty: false);
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
                }
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
                IsDraggable = false;
            }

            protected override IEnumerable<DragDropAction> CreateDragDropActions()
            {
                throw new NotImplementedException();
                // (local) Branch onto this RemoteBranch -> push
                //var dropLocalBranch = new DragDropAction<BranchNode>(
                //    branch => Value.PushConfig != null && Equals(Value.PushConfig.LocalBranch, branch.FullPath),
                //    branch =>
                //    {
                //        GitPush push = Value.CreatePush(branch.FullPath);

                //        if (Module.CompareCommits(branch.FullPath, Value.FullPath).State == BranchCompareStatus.AheadPublishable)
                //        {
                //            // local is ahead and publishable (remote has NOT diverged)
                //            Module.Push(push);
                //            throw new NotImplementedException("tell user about fail or success.");
                //            // if fail because remote diverged since Git.CompareCommits conditional (unlikely) -> tell user to fetch/merge or pull
                //        }
                //        else
                //        {
                //            throw new NotImplementedException("tell user to fetch/merge or pull");
                //        }
                //    });

                //return new[] { dropLocalBranch, };
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

            /// <summary>Un-track the remote branch and remove the local copy.</summary>
            public void UnTrack()
            {
                throw new NotImplementedException();
                //string error = Module.RemoteCmd(GitRemote.UnTrack(Value.Remote, Value));
                //GC.KeepAlive(error);
                //bool isSuccess = true;
                //if (isSuccess)
                //{
                //    TreeNode.Parent.Nodes.Remove(TreeNode);
                //    Value.Remote.UnTrack(Value);
                //}
                //throw new NotImplementedException("this one actually works, but need to change UI state and more testing");
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
                this.TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = "RemoteMirror.png";
            }

            public void ChangeName(string newName)
            {
                Name = newName;
            }
        }
    }
}
