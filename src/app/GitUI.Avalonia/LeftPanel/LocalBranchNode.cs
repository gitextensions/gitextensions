using GitExtensions.Extensibility.Git;
using GitUI.LeftPanel.Interfaces;

namespace GitUI.LeftPanel;

internal sealed class LocalBranchNode : BaseBranchLeafNode, IGitRefActions, ICanRename, ICanDelete
{
    public LocalBranchNode(LocalBranchTree tree, NodeBase parent, IGitRef gitRef, bool isCurrent)
        : base(tree, parent, gitRef.Name, gitRef, remote: false, isCurrent)
    {
        IsCurrent = isCurrent;
    }

    /// <summary>Indicates whether this is the currently checked-out branch.</summary>
    public bool IsCurrent { get; }

    internal override void OnDoubleClick()
    {
        if (!IsCurrent)
        {
            Checkout();
        }
    }

    internal override void OnRename()
        => Rename();

    internal override void OnDelete()
        => Delete();

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
}
