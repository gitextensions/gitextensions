using GitUI.Properties;

namespace GitUI.LeftPanel;

internal class BasePathNode : BaseRevisionNode
{
    public BasePathNode(Tree tree, NodeBase parent, string fullPath)
        : base(tree, parent, fullPath, gitRef: null, Images.BranchFolder)
    {
    }

    public override void ApplyStyle()
    {
        base.ApplyStyle();
    }

    protected override Avalonia.Media.IImage GetVisibleIcon()
        => FullPath == TranslatedStrings.Inactive ? Images.EyeClosed : Images.BranchFolder;
}
