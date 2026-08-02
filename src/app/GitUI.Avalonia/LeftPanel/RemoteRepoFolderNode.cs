namespace GitUI.LeftPanel;

internal sealed class RemoteRepoFolderNode : BaseRevisionNode
{
    public RemoteRepoFolderNode(RemoteBranchTree tree, NodeBase parent, string fullPath)
        : base(tree, parent, fullPath, gitRef: null, GitUI.Properties.Images.EyeClosed)
    {
    }
}
