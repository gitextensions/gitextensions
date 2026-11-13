using System.Diagnostics;

namespace GitUI.LeftPanel;

[DebuggerDisplay("(Branch path) FullPath = {FullPath}")]
internal sealed class BranchPathNode : BasePathNode
{
    public BranchPathNode(LocalBranchTree tree, NodeBase parent, string fullPath)
        : base(tree, parent, fullPath)
    {
    }

    public override string ToString()
    {
        return $"{Name}{PathSeparator}";
    }

    public void DeleteAll()
    {
        IEnumerable<string> branches = DescendantsAndSelf()
            .OfType<LocalBranchNode>()
            .Select(branch => branch.FullPath);
        UICommands.StartDeleteBranchDialog(Owner, branches);
    }

    public void CreateBranch()
    {
        string newBranchNamePrefix = FullPath + PathSeparator;
        UICommands.StartCreateBranchDialog(ParentWindow(), objectId: default, newBranchNamePrefix);
    }
}
