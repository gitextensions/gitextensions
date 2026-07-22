using GitExtensions.Extensibility.Git;

namespace GitUI.LeftPanel;

internal sealed class LocalBranchNode : BaseBranchLeafNode
{
    public LocalBranchNode(LocalBranchTree tree, NodeBase parent, IGitRef gitRef, bool isCurrent)
        : base(tree, parent, gitRef.Name, gitRef, remote: false, isCurrent)
    {
        IsCurrent = isCurrent;
    }

    public bool IsCurrent { get; }

    public bool Checkout()
        => MessageBoxes.ConfirmBranchCheckout(Owner, FullPath)
           && UICommands.StartCheckoutBranch(Owner, FullPath, remote: false);

    public bool CreateBranch()
        => UICommands.StartCreateBranchDialog(Owner, FullPath);

    public bool Merge()
        => UICommands.StartMergeBranchDialog(Owner, FullPath);

    public bool Delete()
        => UICommands.StartDeleteBranchDialog(Owner, FullPath);

    public bool Rename()
        => UICommands.StartRenameDialog(Owner, FullPath);

    internal override void OnDoubleClick()
    {
        if (!IsCurrent)
        {
            Checkout();
        }
    }
}
