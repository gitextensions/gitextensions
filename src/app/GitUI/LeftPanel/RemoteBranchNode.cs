using System.Diagnostics;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.LeftPanel.Interfaces;
using GitUI.Properties;

namespace GitUI.LeftPanel
{
    [DebuggerDisplay("(Remote) FullPath = {FullPath}, Hash = {ObjectId}, Visible: {Visible}")]
    internal sealed class RemoteBranchNode : BaseBranchLeafNode, IGitRefActions, ICanDelete
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
            RemoteBranchInfo remoteBranchInfo = GetRemoteBranchInfo();
            UICommands.StartPullDialogAndPullImmediately(
                out bool pullCompleted,
                TreeViewNode.TreeView,
                remoteBranch: remoteBranchInfo.BranchName,
                remote: remoteBranchInfo.Remote,
                pullAction: GitPullAction.Fetch);
            return pullCompleted;
        }

        private RemoteBranchInfo GetRemoteBranchInfo()
        {
            string remote = FullPath.LazySplit('/').First();
            string branch = FullPath[(remote.Length + 1)..];
            return new RemoteBranchInfo(remote, branch);
        }

        internal override void OnDelete()
        {
            Delete();
        }

        public bool CreateBranch()
        {
            return UICommands.StartCreateBranchDialog(TreeViewNode.TreeView, FullPath);
        }

        public bool Delete()
        {
            RemoteBranchInfo remoteBranchInfo = GetRemoteBranchInfo();
            return UICommands.StartDeleteRemoteBranchDialog(TreeViewNode.TreeView, remoteBranchInfo.Remote + '/' + remoteBranchInfo.BranchName);
        }

        public bool Checkout()
        {
            return MessageBoxes.ConfirmBranchCheckout(ParentWindow(), FullPath) && UICommands.StartCheckoutRemoteBranch(TreeViewNode.TreeView, FullPath);
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
