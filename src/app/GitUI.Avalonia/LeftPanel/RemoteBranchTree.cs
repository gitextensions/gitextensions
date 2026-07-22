using GitCommands;
using GitCommands.Remotes;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;

namespace GitUI.LeftPanel;

internal sealed class RemoteBranchTree : BaseRefTree
{
    public RemoteBranchTree(
        RepoObjectsTree owner,
        IReadOnlyList<IGitRef> branches,
        IReadOnlyList<Remote>? enabledRemotes = null,
        IReadOnlyList<Remote>? disabledRemotes = null,
        IConfigFileRemoteSettingsManager? remotesManager = null)
        : base(owner, RepoTreeKind.Remotes, TranslatedStrings.Remotes, Images.BranchRemoteRoot)
    {
        Dictionary<string, Remote> enabledByName = enabledRemotes?.ToDictionary(remote => remote.Name, StringComparer.Ordinal) ?? [];
        FillNested(
            branches,
            (parent, path, gitRef, level) =>
            {
                if (gitRef is not null)
                {
                    return new RemoteBranchNode(this, parent, gitRef);
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
            TreeViewNode.Items.Add(new RemoteRepoNode(this, this, remote.Name, remote, enabled: true, remotesManager).TreeViewNode);
        }

        if (disabledRemotes?.Count > 0)
        {
            RemoteRepoFolderNode inactive = new(this, this, TranslatedStrings.Inactive);
            foreach (Remote remote in disabledRemotes.OrderBy(remote => remote.Name, StringComparer.OrdinalIgnoreCase))
            {
                inactive.TreeViewNode.Items.Add(new RemoteRepoNode(this, inactive, remote.Name, remote, enabled: false, remotesManager).TreeViewNode);
            }

            TreeViewNode.Items.Add(inactive.TreeViewNode);
        }

        int itemCount = branches.Count + emptyRemotes.Length + (disabledRemotes?.Count ?? 0);
        Complete(TranslatedStrings.Remotes, Images.BranchRemoteRoot, itemCount, expanded: true);
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
