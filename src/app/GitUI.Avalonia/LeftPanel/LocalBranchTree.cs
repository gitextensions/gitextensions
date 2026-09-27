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
        #region example

        // (input)
        // a-branch
        // develop/crazy-branch
        // develop/features/feat-next
        // develop/features/feat-next2
        // develop/issues/iss444
        // develop/wild-branch
        // issues/iss111
        // master
        //
        // ->
        // (output)
        // 0 a-branch
        // 0 develop/
        // 1   features/
        // 2      feat-next
        // 2      feat-next2
        // 1   issues/
        // 2      iss444
        // 1   wild-branch
        // 1   wilds/
        // 2      card
        // 0 issues/
        // 1     iss111
        // 0 master

        #endregion

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

        // Skip hidden node
        // Selection is restored after every root has been attached by RepoObjectsTree.
        // If there's a selected treenode, don't stomp over it
    }
}
