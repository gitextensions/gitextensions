using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using RemoteBranch = GitCommands.Git.RemoteInfo.RemoteBranch;

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

        static readonly string remoteKey = "remote";
        static readonly string remotesKey = "remotes";

        /// <summary>Reloads the remotes.</summary>
        static void OnReloadRemotes(ICollection<RemoteNode> remotes, RootNode<RemoteNode> remotesNode)
        {
            remotesNode.TreeNode.Text = string.Format("{0} ({1})", Strings.remotes, remotes.Count);
        }

        /// <summary>Adds the specified <paramref name="remoteNode"/> to the remotes tree.</summary>
        TreeNode OnAddRemote(TreeNodeCollection nodes, RemoteNode remoteNode)
        {
            RemoteInfo remote = remoteNode.Value;
            TreeNode treeNode = new TreeNode(remote.Name)
            {
                //Name = string.Format("remotes{0}", remote.Name),TODO: branch name may have invalid chars for Control.Name
                Tag = remoteNode,
                //ToolTipText = "",
                ContextMenuStrip = menuRemote,
                ImageKey = remoteKey,
                SelectedImageKey = remoteKey,
            };
            nodes.Add(treeNode);

            return treeNode;
        }

        /// <summary>Remote repo node.</summary>
        sealed class RemoteNode : ParentNode<RemoteInfo, RemoteBranchNode>
        {
            public RemoteNode(RemoteInfo remote, GitUICommands uiCommands)
                : base(uiCommands, remote, remote.Branches.Select(b => new RemoteBranchNode(uiCommands, b))) { }
        }

        /// <summary>Remote-tracking branch node.</summary>
        sealed class RemoteBranchNode : Node<RemoteBranch>
        {
            public RemoteBranchNode(GitUICommands uiCommands,
                 RemoteBranch branch)
                : base(branch, uiCommands)
            {
                IsDraggable = true;
            }

            protected override IEnumerable<DragDropAction> CreateDragDropActions()
            {
                // (local) Branch onto this RemoteBranch -> push
                return new List<DragDropAction>
                {
                    new DragDropAction<BranchNode>(
                        branch => Value.PushConfig != null && Equals(Value.PushConfig.LocalBranch, branch.FullPath),
                        branch =>
                        {
                            GitPush push = Value.CreatePush(branch.FullPath);
                                
                            if (Git.CompareCommits(branch.FullPath,Value.FullPath).State == BranchCompareStatus.AheadPublishable)
                            {// local is ahead and publishable (remote has NOT diverged)
                                Git.Push(push);
                                throw new NotImplementedException("Need to tell user about fail or success.");
                            }
                            else
                            {
                                //UiCommands.StartPushDialog(push);
                            }
                            //UiCommands.StartPushDialog()
                        })
                };
            }
        }

    }
}
