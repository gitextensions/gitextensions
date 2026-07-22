using GitExtensions.Extensibility.Git;
using GitUI.Properties;

namespace GitUI.LeftPanel;

internal sealed class LocalBranchTree : BaseRefTree
{
    public LocalBranchTree(RepoObjectsTree owner, IReadOnlyList<IGitRef> branches, string currentBranch)
        : base(owner, RepoTreeKind.Branches, TranslatedStrings.Branches, Images.BranchLocalRoot)
    {
        FillNested(
            branches,
            (parent, path, gitRef, _) => gitRef is null
                ? new BranchPathNode(this, parent, path)
                : new LocalBranchNode(this, parent, gitRef, path == currentBranch));
        Complete(TranslatedStrings.Branches, Images.BranchLocalRoot, branches.Count, expanded: true);
    }
}
