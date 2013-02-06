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

        // $ git for-each-ref --format='%(upstream:short) <- %(refname:short)' refs/heads
        // master <- origin/master
        // pu <- origin/pu

        // $ git ls-remote {remote}
        // {sha}    HEAD
        // ...
        // {sha}    refs/heads/{branch}
        // ...
        // {sha}    refs/tags/{tag}

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
                Name = string.Format("remotes{0}", remote.Name),
                Tag = remoteNode,
                //ToolTipText = "",
                ContextMenuStrip = menuRemote,
                ImageKey = remoteKey,
                SelectedImageKey = remoteKey,
            };
            nodes.Add(treeNode);

            return treeNode;
        }

        /// <summary>Stash node.</summary>
        sealed class RemoteNode : Node<RemoteInfo, RootNode<RemoteNode>>
        {
            public RemoteNode(RemoteInfo remote, GitUICommands uiCommands)
                : base(remote, null, uiCommands)
            {
                IsDraggable = true;
            }
        }

    }
}
