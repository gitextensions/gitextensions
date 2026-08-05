using GitExtensions.Extensibility.Git;
using GitUI.LeftPanel.Interfaces;
using GitUI.Properties;

namespace GitUI.LeftPanel;

internal sealed class TagNode : BaseRevisionNode, IGitRefActions, ICanDelete
{
    public TagNode(TagTree tree, NodeBase parent, IGitRef gitRef)
        : base(tree, parent, gitRef.Name, gitRef, Images.TagHorizontal)
    {
    }

    public bool CreateBranch()
        => UICommands.StartCreateBranchDialog(Owner, ObjectId);

    public bool Delete()
        => UICommands.StartDeleteTagDialog(Owner, FullPath);

    public bool Merge()
        => UICommands.StartMergeBranchDialog(Owner, FullPath);

    public bool Checkout()
        => UICommands.StartCheckoutRevisionDialog(Owner, FullPath);

    internal override void OnDoubleClick()
        => CreateBranch();

    internal override void OnDelete()
        => Delete();
}
