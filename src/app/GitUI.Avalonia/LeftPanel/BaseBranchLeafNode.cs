using GitExtensions.Extensibility.Git;
using GitUI.Properties;

namespace GitUI.LeftPanel;

internal abstract class BaseBranchLeafNode : BaseRevisionNode
{
    protected BaseBranchLeafNode(Tree tree, NodeBase parent, string fullPath, IGitRef gitRef, bool remote, bool isCurrent = false)
        : base(tree, parent, fullPath, gitRef, remote ? Images.BranchRemote : Images.BranchLocal, isCurrent)
    {
    }
}
