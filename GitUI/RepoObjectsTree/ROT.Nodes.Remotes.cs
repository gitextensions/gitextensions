using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;

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


        /// <summary>Reloads the remotes.</summary>
        static void OnReloadRemotes(ICollection<RemoteNode> remotes, RootNode<RemoteNode> remotesNode)
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
                else if (false)
                {// explicitly UN-tracked -> grey, sort to bottom of list
                    // need to parse from settings
                    throw new NotImplementedException();
                    imgKey = remoteBranchUnTrackedKey;
                    menu = menuRemoteBranchUnTracked;
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

        /// <summary><see cref="Node"/> for a branch on a remote repo.</summary>
        sealed class RemoteBranchNode : Node<RemoteInfo.RemoteTrackingBranch>
        {
            public RemoteBranchNode(GitUICommands uiCommands,
                 RemoteInfo.RemoteTrackingBranch trackingBranch)
                : base(trackingBranch, uiCommands)
            {
                IsDraggable = true;
            }

            protected override IEnumerable<DragDropAction> CreateDragDropActions()
            {
                // (local) Branch onto this RemoteBranch -> push
                var dropLocalBranch = new DragDropAction<BranchNode>(
                    branch => Value.PushConfig != null && Equals(Value.PushConfig.LocalBranch, branch.FullPath),
                    branch =>
                    {
                        GitPush push = Value.CreatePush(branch.FullPath);

                        if (Git.CompareCommits(branch.FullPath, Value.FullPath).State == BranchCompareStatus.AheadPublishable)
                        {
                            // local is ahead and publishable (remote has NOT diverged)
                            Git.Push(push);
                            throw new NotImplementedException("tell user about fail or success.");
                            // if fail because remote diverged since Git.CompareCommits conditional (unlikely) -> tell user to fetch/merge or pull
                        }
                        else
                        {
                            throw new NotImplementedException("tell user to fetch/merge or pull");
                        }
                    });

                return new[] { dropLocalBranch, };
            }

            internal override void OnSelected()
            {
                base.OnSelected();
                UiCommands.BrowseRepo.GoToRef(Value.FullPath);
            }

            /// <summary>Download updates from the remote branch.</summary>
            public void Fetch()
            {
                //Value.Fetch();
                throw new NotImplementedException("create a GitFetch command (similar to GitPush class)");
            }

            public void Pull()
            {
                throw new NotImplementedException();
            }

            /// <summary>Create a local branch from the remote branch.</summary>
            public void CreateBranch()
            {
                if (Value.Status == RemoteInfo.RemoteTrackingBranch.State.New)
                {
                    Fetch();
                }

                throw new NotImplementedException("be able to specify source branch");
                //UiCommands.StartCreateBranchDialog(Value.FullPath);
            }

            /// <summary>Un-track the remote branch and remove the local copy.</summary>
            public void UnTrack()
            {
                string error = Git.RemoteCmd(GitRemote.UnTrack(Value.Remote, Value));
                GC.KeepAlive(error);
                bool isSuccess = true;
                if (isSuccess)
                {
                    TreeNode.Parent.Nodes.Remove(TreeNode);
                    Value.Remote.UnTrack(Value);
                }
                throw new NotImplementedException("this one actually works, but need to change UI state and more testing");
            }

            /// <summary>Delete the branch on the remote repository.</summary>
            public void Delete()
            {
                // needs BIG WARNING

                //GitPush pushDelete = Value.RemoteBranch.Delete();

                throw new NotImplementedException("show warning; implement GitPush RemoteBranch.Delete()");
            }


        }

    }
}
