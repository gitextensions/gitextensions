using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using GitUI.UserControls.RevisionGrid;

namespace GitUI.LeftPanel;

internal sealed class LocalBranchTree : BaseRefTree
{
    private readonly IAheadBehindDataProvider? _aheadBehindDataProvider;
    private readonly IRevisionGridInfo? _revisionGridInfo;
    private readonly string _currentBranch;

    public LocalBranchTree(
        RepoObjectsTree owner,
        IReadOnlyList<IGitRef> branches,
        string currentBranch,
        IAheadBehindDataProvider? aheadBehindDataProvider = null,
        ICheckRefs? refsSource = null,
        IRevisionGridInfo? revisionGridInfo = null)
        : base(owner, RepoTreeKind.Branches, TranslatedStrings.Branches, Images.BranchLocalRoot, refsSource, RefsFilter.Heads)
    {
        _aheadBehindDataProvider = aheadBehindDataProvider;
        _revisionGridInfo = revisionGridInfo;
        _currentBranch = currentBranch;
        FillTree(branches, CancellationToken.None);
        Complete(TranslatedStrings.Branches, Images.BranchLocalRoot, expanded: true);
    }

    protected override Nodes FillTree(IReadOnlyList<IGitRef> branches, CancellationToken token)
    {
        IReadOnlyDictionary<string, AheadBehindData>? aheadBehindData = _aheadBehindDataProvider?.GetData();
        string currentBranch = _revisionGridInfo?.GetCurrentBranch() ?? _currentBranch;
        FillNested(
            PrioritizedBranches(branches),
            (parent, path, gitRef, _) =>
            {
                token.ThrowIfCancellationRequested();
                if (gitRef is null)
                {
                    return new BranchPathNode(this, parent, path);
                }

                if (gitRef.ObjectId.IsZero)
                {
                    throw new InvalidOperationException($"Branch '{gitRef.Name}' has no ObjectId.");
                }

                LocalBranchNode node = new(this, parent, gitRef, path == currentBranch);
                if (aheadBehindData?.TryGetValue(node.FullPath, out AheadBehindData aheadBehind) is true)
                {
                    node.UpdateAheadBehind(aheadBehind.ToDisplay(), aheadBehind.RemoteRef);
                }

                return node;
            });
        return Nodes;
    }

    protected override void PostFillTreeViewNode(bool firstTime)
    {
        if (firstTime)
        {
            TreeViewNode.IsExpanded = true;
        }
    }
}
