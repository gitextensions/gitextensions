using System.Diagnostics;
using GitExtensions.Extensibility.Git;
using GitUI.LeftPanel.Interfaces;
using GitUI.Properties;

namespace GitUI.LeftPanel
{
    [DebuggerDisplay("(Tag) FullPath = {FullPath}, Hash = {ObjectId}, Visible: {Visible}")]
    internal sealed class TagNode : BaseRevisionNode, IGitRefActions, ICanDelete
    {
        public TagNode(Tree tree, in ObjectId? objectId, string fullPath, bool visible)
            : base(tree, fullPath, visible)
        {
            ObjectId = objectId;
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

        internal override void OnDoubleClick()
        {
            CreateBranch();
        }

        internal override void OnDelete()
        {
            Delete();
        }

        public bool CreateBranch()
        {
            return UICommands.StartCreateBranchDialog(TreeViewNode.TreeView, ObjectId);
        }

        public bool Delete()
        {
            return UICommands.StartDeleteTagDialog(TreeViewNode.TreeView, FullPath);
        }

        public bool Merge()
        {
            return UICommands.StartMergeBranchDialog(TreeViewNode.TreeView, FullPath);
        }

        public override void ApplyStyle()
        {
            base.ApplyStyle();

            TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey =
                Visible
                    ? nameof(Images.TagHorizontal)
                    : nameof(Images.EyeClosed);
        }

        public bool Checkout()
        {
            return UICommands.StartCheckoutRevisionDialog(TreeViewNode.TreeView, FullPath);
        }
    }
}
