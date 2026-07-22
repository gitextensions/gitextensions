using GitUI.Properties;

namespace GitUI.LeftPanel;

internal class BasePathNode : BaseRevisionNode
{
    public BasePathNode(Tree tree, NodeBase parent, string fullPath)
        : base(tree, parent, fullPath, gitRef: null, Images.BranchFolder)
    {
    }
}
