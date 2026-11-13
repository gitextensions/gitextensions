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
        Nodes.AddNodes(FillTree(branches, CancellationToken.None));
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

        Nodes nodes = new(this);
        IReadOnlyDictionary<string, AheadBehindData>? aheadBehindData = _aheadBehindDataProvider?.GetData();
        string currentBranch = _revisionGridInfo?.GetCurrentBranch() ?? _currentBranch;
        Dictionary<string, BaseRevisionNode> pathToNode = [];
        foreach (IGitRef branch in PrioritizedBranches(branches))
        {
            token.ThrowIfCancellationRequested();

            if (branch.ObjectId.IsZero)
            {
                throw new InvalidOperationException($"Branch '{branch.Name}' has no ObjectId.");
            }

            LocalBranchNode localBranchNode = new(this, branch.ObjectId, branch.Name, branch.Name == currentBranch, visible: true) { GitRef = branch };

            if (aheadBehindData?.TryGetValue(localBranchNode.FullPath, out AheadBehindData aheadBehind) is true)
            {
                localBranchNode.UpdateAheadBehind(aheadBehind.ToDisplay(), aheadBehind.RemoteRef);
            }

            BaseRevisionNode? parent = localBranchNode.CreateRootNode(pathToNode, (tree, parentPath) => new BranchPathNode(tree, parentPath));
            if (parent is not null)
            {
                nodes.AddNode(parent);
            }
        }

        return nodes;
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
