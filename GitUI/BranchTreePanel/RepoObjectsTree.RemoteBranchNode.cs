using System.Diagnostics;
using System.Linq;
using GitCommands;
using GitExtUtils;
using GitUI.BranchTreePanel.Interfaces;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree
    {
        [DebuggerDisplay("(Remote) FullPath = {FullPath}, Hash = {ObjectId}, Visible: {Visible}")]
        private sealed class RemoteBranchNode : BaseBranchLeafNode, IGitRefActions, ICanDelete, ICanRename
        {
            public RemoteBranchNode(Tree tree, in ObjectId objectId, string fullPath, bool visible)
                : base(tree, objectId, fullPath, visible, nameof(Images.BranchRemote), nameof(Images.BranchRemoteMerged))
            {
            }

            internal override void OnSelected()
            {
                if (Tree.IgnoreSelectionChangedEvent)
                {
                    return;
                }

                base.OnSelected();
                SelectRevision();
            }

            public bool Fetch()
            {
                var remoteBranchInfo = GetRemoteBranchInfo();
                UICommands.StartPullDialogAndPullImmediately(
                    out bool pullCompleted,
                    TreeViewNode.TreeView,
                    remoteBranch: remoteBranchInfo.BranchName,
                    remote: remoteBranchInfo.Remote,
                    pullAction: AppSettings.PullAction.Fetch);
                return pullCompleted;
            }

            private RemoteBranchInfo GetRemoteBranchInfo()
            {
                var remote = FullPath.LazySplit('/').First();
                var branch = FullPath.Substring(remote.Length + 1);
                return new RemoteBranchInfo(remote, branch);
            }

            public bool CreateBranch()
            {
                return UICommands.StartCreateBranchDialog(TreeViewNode.TreeView, FullPath);
            }

            public bool Delete()
            {
                var remoteBranchInfo = GetRemoteBranchInfo();
                return UICommands.StartDeleteRemoteBranchDialog(TreeViewNode.TreeView, remoteBranchInfo.Remote + '/' + remoteBranchInfo.BranchName);
            }

            public bool Rename()
            {
                return UICommands.StartRenameDialog(TreeViewNode.TreeView, FullPath);
            }

            public bool Checkout()
            {
                return UICommands.StartCheckoutRemoteBranch(TreeViewNode.TreeView, FullPath);
            }

            public bool Merge()
            {
                return UICommands.StartMergeBranchDialog(TreeViewNode.TreeView, FullPath);
            }

            internal override void OnDoubleClick()
            {
                Checkout();
            }

            public bool FetchAndMerge()
            {
                return Fetch() && Merge();
            }

            public bool FetchAndCheckout()
            {
                return Fetch() && Checkout();
            }

            public bool FetchAndCreateBranch()
            {
                return Fetch() && CreateBranch();
            }

            public bool FetchAndRebase()
            {
                return Fetch() && Rebase();
            }

            private readonly struct RemoteBranchInfo
            {
                public string Remote { get; }
                public string BranchName { get; }

                public RemoteBranchInfo(string remote, string branchName)
                {
                    Remote = remote;
                    BranchName = branchName;
                }
            }
        }
    }
}
