using System.Diagnostics;
using System.Drawing;
using GitUI.BranchTreePanel.Interfaces;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree
    {
        [DebuggerDisplay("(Local) FullPath = {FullPath}, Hash = {ObjectId}, Visible: {Visible}")]
        private sealed class LocalBranchNode : BaseBranchLeafNode, IGitRefActions, ICanRename, ICanDelete
        {
            public LocalBranchNode(Tree tree, in ObjectId? objectId, string fullPath, bool isCurrent, bool visible)
                : base(tree, objectId, fullPath, visible, nameof(Images.BranchLocal), nameof(Images.BranchLocalMerged))
            {
                IsActive = isCurrent;
            }

            public bool IsActive { get; }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();

                SetNodeFont(IsActive ? FontStyle.Bold : FontStyle.Regular);
            }

            public override bool Equals(object obj)
                => base.Equals(obj)
                    && obj is LocalBranchNode localBranchNode
                    && IsActive == localBranchNode.IsActive;

            public override int GetHashCode()
                => base.GetHashCode() ^ IsActive.GetHashCode();

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
}
