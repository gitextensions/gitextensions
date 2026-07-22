namespace GitUI.LeftPanel;

internal sealed class BranchPathNode : BasePathNode
{
    public BranchPathNode(LocalBranchTree tree, NodeBase parent, string fullPath)
        : base(tree, parent, fullPath)
    {
    }

    public void DeleteAll()
    {
        IEnumerable<string> branches = DescendantsAndSelf()
            .OfType<LocalBranchNode>()
            .Select(branch => branch.FullPath);
        UICommands.StartDeleteBranchDialog(Owner, branches);
    }

    public void CreateBranch()
        => UICommands.StartCreateBranchDialog(Owner, objectId: default, $"{FullPath}/");
}
