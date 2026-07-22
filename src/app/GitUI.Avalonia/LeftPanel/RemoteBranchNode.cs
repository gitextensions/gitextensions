using GitCommands;
using GitExtensions.Extensibility.Git;

namespace GitUI.LeftPanel;

internal sealed class RemoteBranchNode : BaseBranchLeafNode
{
    public RemoteBranchNode(RemoteBranchTree tree, NodeBase parent, IGitRef gitRef)
        : base(tree, parent, gitRef.Name, gitRef, remote: true)
    {
    }

    public bool Fetch()
    {
        (string remote, string branch) = GetRemoteBranchInfo();
        UICommands.StartPullDialogAndPullImmediately(
            out bool pullCompleted,
            Owner,
            remoteBranch: branch,
            remote,
            pullAction: GitPullAction.Fetch);
        return pullCompleted;
    }

    public bool CreateBranch()
        => UICommands.StartCreateBranchDialog(Owner, FullPath);

    public bool Checkout()
        => MessageBoxes.ConfirmBranchCheckout(Owner, FullPath)
           && UICommands.StartCheckoutRemoteBranch(Owner, FullPath);

    public bool Merge()
        => UICommands.StartMergeBranchDialog(Owner, FullPath);

    public bool FetchAndMerge()
        => Fetch() && Merge();

    public bool FetchAndCheckout()
        => Fetch() && Checkout();

    public bool FetchAndCreateBranch()
        => Fetch() && CreateBranch();

    public bool FetchAndRebase()
        => Fetch() && Rebase();

    internal override void OnDoubleClick()
        => Checkout();

    private (string Remote, string Branch) GetRemoteBranchInfo()
    {
        int separator = FullPath.IndexOf('/');
        return separator < 0
            ? (FullPath, string.Empty)
            : (FullPath[..separator], FullPath[(separator + 1)..]);
    }
}
