using GitUI.Properties;

namespace GitUI.LeftPanel
{
    internal class BasePathNode : BaseRevisionNode
    {
        public BasePathNode(Tree tree, string fullPath) : base(tree, fullPath, visible: true)
        {
        }

        public override void ApplyStyle()
        {
            base.ApplyStyle();

            TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey =
                FullPath == TranslatedStrings.Inactive ? nameof(Images.EyeClosed) : nameof(Images.BranchFolder);
        }
    }
}
