using GitExtensions.Extensibility.Git;
using GitUI.LeftPanel.Interfaces;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.LeftPanel;

internal sealed class LocalBranchNode : BaseBranchLeafNode, IGitRefActions, ICanRename, ICanDelete
{
    public LocalBranchNode(LocalBranchTree tree, NodeBase parent, IGitRef gitRef, bool isCurrent)
        : base(tree, parent, gitRef.Name, gitRef, remote: false)
    {
        IsCurrent = isCurrent;
    }

    /// <summary>Indicates whether this is the currently checked-out branch.</summary>
    public bool IsCurrent { get; }

    protected override WinFormsShims.FontStyle GetFontStyle()
        => base.GetFontStyle() | (IsCurrent ? WinFormsShims.FontStyle.Bold : WinFormsShims.FontStyle.Regular);

    public override bool Equals(object? obj)
        => base.Equals(obj) && obj is LocalBranchNode;

    public override int GetHashCode()
        => base.GetHashCode();

    internal override void OnDoubleClick()
        => Checkout();

    internal override void OnSelected()
    {
        if (Tree.IgnoreSelectionChangedEvent)
        {
            return;
        }

        base.OnSelected();
        SelectRevision();
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
