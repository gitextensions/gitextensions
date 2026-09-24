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
        IDictionary<string, AheadBehindData>? aheadBehindData = _aheadBehindDataProvider?.GetData()?
            .DistinctBy(pair => pair.Value.RemoteRef)
            .ToDictionary(pair => pair.Value.RemoteRef, pair => pair.Value);
        Dictionary<string, Remote> enabledByName = enabledRemotes?.ToDictionary(remote => remote.Name, StringComparer.Ordinal) ?? [];
        FillNested(
            PrioritizedBranches(branches),
            (parent, path, gitRef, level) =>
            {
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
                    return new RemoteRepoNode(this, parent, path, remote, enabled: true, remotesManager);
                }

                return new BasePathNode(this, parent, path);
            });

        HashSet<string> representedRemotes =
        [
            .. DescendantsAndSelf()
                .OfType<RemoteRepoNode>()
                .Select(node => node.FullPath),
        ];
        Remote[] emptyRemotes =
        [
            .. (enabledRemotes ?? [])
                .Where(remote => !representedRemotes.Contains(remote.Name))
                .OrderBy(remote => remote.Name, StringComparer.OrdinalIgnoreCase),
        ];
        foreach (Remote remote in emptyRemotes)
        {
            AddChild(new RemoteRepoNode(this, this, remote.Name, remote, enabled: true, remotesManager));
        }

        RemoteRepoNode[] enabledNodes = [.. Nodes.OfType<RemoteRepoNode>()];
        if (enabledNodes.Length > 0)
        {
            Nodes.Clear();
            Nodes.AddNodes(PrioritizedRemotes(enabledNodes));
        }

        if (disabledRemotes?.Count > 0)
        {
            RemoteRepoFolderNode inactive = new(this, this, TranslatedStrings.Inactive);
            List<RemoteRepoNode> disabledNodes = [];
            foreach (Remote remote in disabledRemotes)
            {
                disabledNodes.Add(new RemoteRepoNode(this, inactive, remote.Name, remote, enabled: false, remotesManager));
            }

            foreach (RemoteRepoNode node in PrioritizedRemotes(disabledNodes))
            {
                inactive.AddChild(node);
            }

            AddChild(inactive);
        }

        Complete(TranslatedStrings.Remotes, Images.BranchRemoteRoot, expanded: true);
    }

    public void PopupManageRemotesForm(string? remoteName)
        => UICommands.StartRemotesDialog(Owner, remoteName);

    public bool FetchAll()
    {
        UICommands.StartPullDialogAndPullImmediately(
            out bool pullCompleted,
            Owner,
            pullAction: GitPullAction.FetchAll);
        return pullCompleted;
    }

    public bool FetchPruneAll()
    {
        UICommands.StartPullDialogAndPullImmediately(
            out bool pullCompleted,
            Owner,
            pullAction: GitPullAction.FetchPruneAll);
        return pullCompleted;
    }
}
