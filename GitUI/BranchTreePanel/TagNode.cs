using System.Diagnostics;
using System.Windows.Forms;
using GitUI.BranchTreePanel.Interfaces;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.BranchTreePanel
{
    [DebuggerDisplay("(Tag) FullPath = {FullPath}, Hash = {ObjectId}, Visible: {Visible}")]
    internal sealed class TagNode : BaseBranchNode, IGitRefActions, ICanDelete
    {
        public TagNode(Tree tree, in ObjectId? objectId, string fullPath, bool visible)
            : base(tree, fullPath, visible)
        {
            ObjectId = objectId;
        }

        public ObjectId? ObjectId { get; }

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

        protected override void ApplyStyle()
        {
            base.ApplyStyle();

            TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey =
                Visible
                    ? nameof(Images.TagHorizontal)
                    : nameof(Images.EyeClosed);
        }

        public bool Checkout()
        {
            using FormCheckoutRevision form = new(UICommands);
            form.SetRevision(FullPath);
            return form.ShowDialog(TreeViewNode.TreeView) != DialogResult.Cancel;
        }
    }
}
