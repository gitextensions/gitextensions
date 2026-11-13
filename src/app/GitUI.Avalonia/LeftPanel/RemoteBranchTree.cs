using GitCommands;
using GitCommands.Git;
using GitCommands.Remotes;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;
using GitUI.UserControls.RevisionGrid;

namespace GitUI.LeftPanel;

internal sealed class RemoteBranchTree : BaseRefTree
{
    private readonly IAheadBehindDataProvider? _aheadBehindDataProvider;
    private Dictionary<string, Remote> _remoteByName = [];
    private readonly IReadOnlyList<Remote> _disabledRemotes;
    private readonly IReadOnlyList<Remote>? _enabledRemotes;
    private readonly IConfigFileRemoteSettingsManager? _remotesManager;

    public RemoteBranchTree(
        RepoObjectsTree owner,
        IReadOnlyList<IGitRef> branches,
        IReadOnlyList<Remote>? enabledRemotes = null,
        IReadOnlyList<Remote>? disabledRemotes = null,
        IConfigFileRemoteSettingsManager? remotesManager = null,
        IAheadBehindDataProvider? aheadBehindDataProvider = null,
        ICheckRefs? refsSource = null)
        : base(owner, RepoTreeKind.Remotes, TranslatedStrings.Remotes, Images.BranchRemoteRoot, refsSource, RefsFilter.Remotes)
    {
        _aheadBehindDataProvider = aheadBehindDataProvider;
        _disabledRemotes = disabledRemotes ?? [];
        _enabledRemotes = enabledRemotes;
        _remoteByName = (_enabledRemotes ?? []).ToDictionary(remote => remote.Name, StringComparer.Ordinal);
        _remotesManager = remotesManager;
        Nodes.AddNodes(FillTree(branches, CancellationToken.None));
        Complete(TranslatedStrings.Remotes, Images.BranchRemoteRoot, expanded: true);
    }

    protected override async Task PreloadAsync(CancellationToken token)
    {
        IReadOnlyList<Remote> remotes = await Module.GetRemotesAsync();
        token.ThrowIfCancellationRequested();
        _remoteByName = remotes.ToDictionary(r => r.Name);
    }

    protected override Nodes FillTree(IReadOnlyList<IGitRef> branches, CancellationToken token)
    {
        // More than one local can point to a single remote branch, pick one of them.
        Nodes nodes = new(this);
        Dictionary<string, BaseRevisionNode> pathToNodes = [];
        IDictionary<string, AheadBehindData>? aheadBehindData = _aheadBehindDataProvider?.GetData()?.DistinctBy(r => r.Value.RemoteRef).ToDictionary(r => r.Value.RemoteRef, r => r.Value);

        List<RemoteRepoNode> enabledRemoteRepoNodes = [];
        Dictionary<string, Remote> remoteByName = _remoteByName;

        // Cross-platform constraint: FormBrowse reads the remote configuration with the refs snapshot,
        // and the standalone previewer omits it to show every remote branch.
        IConfigFileRemoteSettingsManager? remotesManager = _remotesManager;

        // Create nodes for enabled remotes with branches
        foreach (IGitRef branch in PrioritizedBranches(branches))
        {
            token.ThrowIfCancellationRequested();

            if (branch.ObjectId.IsZero)
            {
                throw new InvalidOperationException($"Branch '{branch.Name}' has no ObjectId.");
            }

            string remoteName = branch.Name.SubstringUntil('/');
            Remote? remote = remoteByName.TryGetValue(remoteName, out Remote configured) ? configured : null;
            if (remote is not null || _enabledRemotes is null)
            {
                RemoteBranchNode remoteBranchNode = new(this, branch.ObjectId, branch.Name, visible: true) { GitRef = branch };
                if (aheadBehindData?.TryGetValue(branch.CompleteName, out AheadBehindData aheadBehind) is true)
                {
                    remoteBranchNode.UpdateAheadBehind(aheadBehind.ToDisplay(reverse: true), $"{GitRefName.RefsHeadsPrefix}{aheadBehind.Branch}");
                }

                BaseRevisionNode? parent = remoteBranchNode.CreateRootNode(
                    pathToNodes,
                    (tree, parentPath) => CreateRemoteBranchPathNode(tree, parentPath, remoteName, remote));

                if (parent is not null)
                {
                    enabledRemoteRepoNodes.Add((RemoteRepoNode)parent);
                }
            }
        }

        // Create nodes for enabled remotes without branches
        IReadOnlyList<string> enabledRemotesNoBranches = GetEnabledRemoteNamesWithoutBranches(branches, remoteByName);
        foreach (string remoteName in enabledRemotesNoBranches)
        {
            if (remoteByName.TryGetValue(remoteName, out Remote remote))
            {
                RemoteRepoNode node = new(this, remoteName, remotesManager, remote, true);
                enabledRemoteRepoNodes.Add(node);
            }
        }

        // Add enabled remote nodes in order
        foreach (RemoteRepoNode node in PrioritizedRemotes(enabledRemoteRepoNodes))
        {
            nodes.AddNode(node);
        }

        // Add disabled remotes, if any
        IReadOnlyList<Remote> disabledRemotes = _disabledRemotes;
        if (disabledRemotes.Count > 0)
        {
            List<RemoteRepoNode> disabledRemoteRepoNodes = [];
            foreach (Remote remote in disabledRemotes)
            {
                RemoteRepoNode node = new(this, remote.Name, remotesManager, remote, false);
                disabledRemoteRepoNodes.Add(node);
            }

            RemoteRepoFolderNode disabledFolderNode = new(this, TranslatedStrings.Inactive);
            foreach (RemoteRepoNode node in PrioritizedRemotes(disabledRemoteRepoNodes))
            {
                disabledFolderNode.Nodes.AddNode(node);
            }

            nodes.AddNode(disabledFolderNode);
        }

        return nodes;

        BaseRevisionNode CreateRemoteBranchPathNode(Tree tree, string parentPath, string remoteName, Remote? remote)
        {
            if (parentPath == remoteName)
            {
                return new RemoteRepoNode(tree, parentPath, remotesManager, remote, true);
            }

            return new BasePathNode(tree, parentPath);
        }

        IReadOnlyList<string> GetEnabledRemoteNamesWithoutBranches(IReadOnlyList<IGitRef> branches, Dictionary<string, Remote> remoteByName)
        {
            HashSet<string> remotesWithBranches = [.. branches.Select(branch => branch.Name.SubstringUntil('/'))];

            HashSet<string> allRemotes = [.. remoteByName.Select(kv => kv.Value.Name)];

            return allRemotes.Except(remotesWithBranches).ToList();
        }
    }

    protected override void PostFillTreeViewNode(bool firstTime)
    {
        if (firstTime)
        {
            TreeViewNode.IsExpanded = true;
        }
    }

    internal void PopupManageRemotesForm(string? remoteName)
        => UICommands.StartRemotesDialog(Owner, remoteName);

    internal bool FetchAll()
    {
        UICommands.StartPullDialogAndPullImmediately(
            out bool pullCompleted,
            Owner,
            pullAction: GitPullAction.FetchAll);
        return pullCompleted;
    }

    internal bool FetchPruneAll()
    {
        UICommands.StartPullDialogAndPullImmediately(
            out bool pullCompleted,
            Owner,
            pullAction: GitPullAction.FetchPruneAll);
        return pullCompleted;
    }
}
