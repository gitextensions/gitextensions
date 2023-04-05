using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.LeftPanel
{
    internal abstract class BaseBranchLeafNode : BaseRevisionNode
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

        protected string? AheadBehind { get; set; }

        protected string? RelatedBranch { get; set; }

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

        public override void ApplyStyle()
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

        public void UpdateAheadBehind(string aheadBehindData, string relatedBranch)
        {
            AheadBehind = aheadBehindData;
            RelatedBranch = relatedBranch;
        }

        protected override string DisplayText()
        {
            return string.IsNullOrEmpty(AheadBehind) ? Name : $"{Name} ({AheadBehind})";
        }

        protected override void SelectRevision()
        {
            TreeViewNode.TreeView?.BeginInvoke(() =>
            {
                string branch = RelatedBranch is null || !Control.ModifierKeys.HasFlag(Keys.Alt)
                    ? FullPath
                    : RelatedBranch;
                UICommands.BrowseGoToRef(branch, showNoRevisionMsg: true, toggleSelection: Control.ModifierKeys.HasFlag(Keys.Control));
                TreeViewNode.TreeView?.Focus();
            });
        }
    }
}
