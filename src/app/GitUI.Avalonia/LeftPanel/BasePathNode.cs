using GitUI.Compat;
using GitUI.Properties;

namespace GitUI.LeftPanel;

internal class BasePathNode : BaseRevisionNode
{
    public BasePathNode(Tree tree, string fullPath) : base(tree, fullPath, visible: true)
    {
    }

    public override void ApplyStyle()
    {
        base.ApplyStyle();

        TreeViewNode.SetImage(
            FullPath == TranslatedStrings.Inactive ? Images.EyeClosed : Images.BranchFolder);
    }
}
