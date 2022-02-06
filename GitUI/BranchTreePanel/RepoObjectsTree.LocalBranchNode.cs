using System.Diagnostics;
using System.Drawing;
using GitUI.BranchTreePanel.Interfaces;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.BranchTreePanel
{
    [DebuggerDisplay("(Local) FullPath = {FullPath}, Hash = {ObjectId}, Visible: {Visible}")]
    internal sealed class LocalBranchNode : BaseBranchLeafNode, IGitRefActions, ICanRename, ICanDelete
    {
        public LocalBranchNode(Tree tree, in ObjectId? objectId, string fullPath, bool isCurrent, bool visible)
            : base(tree, objectId, fullPath, visible, nameof(Images.BranchLocal), nameof(Images.BranchLocalMerged))
        {
            IsCurrent = isCurrent;
        }

        /// <summary>Indicates whether this is the currently checked-out branch.</summary>
        public bool IsCurrent { get; }

        protected override FontStyle GetFontStyle()
            => base.GetFontStyle() | (IsCurrent ? FontStyle.Bold : FontStyle.Regular);

        public override bool Equals(object obj)
            => base.Equals(obj) && obj is LocalBranchNode;

        public override int GetHashCode()
            => base.GetHashCode();

        internal override void OnDoubleClick()
        {
            Checkout();
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

        internal override void OnRename()
        {
            Rename();
        }

        internal override void OnDelete()
        {
            Delete();
        }

        public bool Checkout()
        {
            return UICommands.StartCheckoutBranch(ParentWindow(), branch: FullPath, remote: false);
        }

        public bool CreateBranch()
        {
            return UICommands.StartCreateBranchDialog(ParentWindow(), branch: FullPath);
        }

        public bool Merge()
        {
            return UICommands.StartMergeBranchDialog(ParentWindow(), branch: FullPath);
        }

        public bool Delete()
        {
            return UICommands.StartDeleteBranchDialog(ParentWindow(), branch: FullPath);
        }

        public bool Rename()
        {
            return UICommands.StartRenameDialog(ParentWindow(), branch: FullPath);
        }
    }
}
