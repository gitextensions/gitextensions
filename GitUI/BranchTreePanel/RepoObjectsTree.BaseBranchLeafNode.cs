using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree
    {
        private class BaseBranchLeafNode : BaseBranchNode
        {
            private readonly string _imageKeyMerged;
            private readonly string _imageKeyUnmerged;

            private bool _isMerged = false;

            public BaseBranchLeafNode(Tree tree, in ObjectId? objectId, string fullPath, bool visible, string imageKeyUnmerged, string imageKeyMerged)
                : base(tree, fullPath, visible)
            {
                ObjectId = objectId;
                _imageKeyUnmerged = imageKeyUnmerged;
                _imageKeyMerged = imageKeyMerged;
            }

            public bool IsMerged
            {
                get => _isMerged;
                set
                {
                    if (_isMerged == value)
                    {
                        return;
                    }

                    _isMerged = value;

                    ApplyStyle();
                }
            }

            public ObjectId? ObjectId { get; }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();

                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey =
                    Visible
                        ? IsMerged ? _imageKeyMerged : _imageKeyUnmerged
                        : nameof(Images.EyeClosed);
                if (!Visible)
                {
                    TreeViewNode.ToolTipText = string.Format(TranslatedStrings.InvisibleCommit, FullPath);
                }
                else if (_isMerged)
                {
                    TreeViewNode.ToolTipText = string.Format(TranslatedStrings.ContainedInCurrentCommit, Name);
                }
            }
        }
    }
}
