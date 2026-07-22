using Avalonia.Controls;
using GitCommands;
using GitCommands.Remotes;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;
using GitUIPluginInterfaces.RepositoryHosts;

namespace GitUI.LeftPanel;

internal sealed class RemoteRepoNode : BaseRevisionNode
{
    private readonly Remote? _remote;
    private readonly IConfigFileRemoteSettingsManager? _remotesManager;

    public RemoteRepoNode(
        RemoteBranchTree tree,
        NodeBase parent,
        string fullPath,
        Remote? remote,
        bool enabled,
        IConfigFileRemoteSettingsManager? remotesManager)
        : base(tree, parent, fullPath, gitRef: null, GetIcon(remote))
    {
        _remote = remote;
        Enabled = enabled;
        _remotesManager = remotesManager;
        if (remote is Remote value)
        {
            ToolTip.SetTip(TreeViewNode, value.FetchUrl);
        }
    }

    public bool Enabled { get; }

    public bool CanToggle => _remotesManager is not null;

    public bool IsRemoteUrlUsingHttp => _remote?.FetchUrl.IsUrlUsingHttp() == true;

    private RemoteBranchTree RemoteTree => (RemoteBranchTree)Tree;

    public void PopupManageRemotesForm()
        => RemoteTree.PopupManageRemotesForm(FullPath);

    public bool Fetch()
    {
        UICommands.StartPullDialogAndPullImmediately(
            out bool pullCompleted,
            Owner,
            remote: FullPath,
            pullAction: GitPullAction.Fetch);
        return pullCompleted;
    }

    public bool Prune()
    {
        UICommands.StartPullDialogAndPullImmediately(
            out bool pullCompleted,
            Owner,
            remote: FullPath,
            pullAction: GitPullAction.FetchPruneAll);
        return pullCompleted;
    }

    public void OpenRemoteUrlInBrowser()
    {
        if (IsRemoteUrlUsingHttp)
        {
            OsShellUtil.OpenUrlInDefaultBrowser(_remote!.Value.FetchUrl);
        }
    }

    public void Enable(bool fetch)
    {
        _remotesManager?.ToggleRemoteState(FullPath, disabled: false);
        if (fetch)
        {
            Fetch();
        }
        else
        {
            UICommands.RepoChangedNotifier.Notify();
        }
    }

    public void Disable()
    {
        _remotesManager?.ToggleRemoteState(FullPath, disabled: true);
        UICommands.RepoChangedNotifier.Notify();
    }

    internal override void OnDoubleClick()
        => PopupManageRemotesForm();

    private static Avalonia.Media.IImage GetIcon(Remote? remote)
    {
        string url = remote?.FetchUrl ?? string.Empty;
        if (url.Contains("github.com", StringComparison.OrdinalIgnoreCase))
        {
            return Images.GitHub;
        }

        if (url.Contains("bitbucket.", StringComparison.OrdinalIgnoreCase))
        {
            return Images.BitBucket;
        }

        if (url.Contains("visualstudio.com", StringComparison.OrdinalIgnoreCase)
            || url.Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase))
        {
            return Images.VisualStudioTeamServices;
        }

        return Images.Remote;
    }
}
