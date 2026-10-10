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
    private readonly IReadOnlyList<Remote> _disabledRemotes;
    private readonly IReadOnlyList<Remote> _enabledRemotes;
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
        _enabledRemotes = enabledRemotes ?? [];
        _remotesManager = remotesManager;
        FillTree(branches, CancellationToken.None);
        Complete(TranslatedStrings.Remotes, Images.BranchRemoteRoot, expanded: true);
    }

    protected override Nodes FillTree(IReadOnlyList<IGitRef> branches, CancellationToken token)
    {
        // More than one local can point to a single remote branch, pick one of them.
        IDictionary<string, AheadBehindData>? aheadBehindData = _aheadBehindDataProvider?.GetData()?
            .DistinctBy(pair => pair.Value.RemoteRef)
            .ToDictionary(pair => pair.Value.RemoteRef, pair => pair.Value);
        Dictionary<string, Remote> enabledByName = _enabledRemotes.ToDictionary(remote => remote.Name, StringComparer.Ordinal);

        // Create nodes for enabled remotes with branches
        FillNested(
            PrioritizedBranches(branches),
            (parent, path, gitRef, level) =>
            {
                token.ThrowIfCancellationRequested();
                if (gitRef is not null)
                {
                    if (gitRef.ObjectId.IsZero)
                    {
                        throw new InvalidOperationException($"Branch '{gitRef.Name}' has no ObjectId.");
                    }

                    RemoteBranchNode node = new(this, parent, gitRef);
                    if (aheadBehindData?.TryGetValue(gitRef.CompleteName, out AheadBehindData aheadBehind) is true)
                    {
                        node.UpdateAheadBehind(aheadBehind.ToDisplay(reverse: true), $"{GitRefName.RefsHeadsPrefix}{aheadBehind.Branch}");
                    }

                    return node;
                }

                if (level == 0)
                {
                    Remote? remote = enabledByName.TryGetValue(path, out Remote value) ? value : null;
                    return new RemoteRepoNode(this, parent, path, remote, enabled: true, _remotesManager);
                }

                return new BasePathNode(this, parent, path);
            });

        // Create nodes for enabled remotes without branches
        HashSet<string> representedRemotes =
        [
            .. DescendantsAndSelf()
                .OfType<RemoteRepoNode>()
                .Select(node => node.FullPath),
        ];
        foreach (Remote remote in _enabledRemotes
                     .Where(remote => !representedRemotes.Contains(remote.Name))
                     .OrderBy(remote => remote.Name, StringComparer.OrdinalIgnoreCase))
        {
            AddChild(new RemoteRepoNode(this, this, remote.Name, remote, enabled: true, _remotesManager));
        }

        // Add enabled remote nodes in order
        RemoteRepoNode[] enabledNodes = [.. Nodes.OfType<RemoteRepoNode>()];
        if (enabledNodes.Length > 0)
        {
            Nodes.Clear();
            Nodes.AddNodes(PrioritizedRemotes(enabledNodes));
        }

        // Add disabled remotes, if any
        if (_disabledRemotes.Count > 0)
        {
            List<RemoteRepoNode> disabledRemoteRepoNodes = [];
            RemoteRepoFolderNode disabledFolderNode = new(this, this, TranslatedStrings.Inactive);
            foreach (Remote remote in _disabledRemotes)
            {
                disabledRemoteRepoNodes.Add(new RemoteRepoNode(this, disabledFolderNode, remote.Name, remote, enabled: false, _remotesManager));
            }

            foreach (RemoteRepoNode node in PrioritizedRemotes(disabledRemoteRepoNodes))
            {
                disabledFolderNode.AddChild(node);
            }

            AddChild(disabledFolderNode);
        }

        return Nodes;
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
