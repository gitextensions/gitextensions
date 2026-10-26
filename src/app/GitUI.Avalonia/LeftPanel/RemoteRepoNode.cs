using System.Diagnostics;
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
        ApplyStyle();
    }

    public bool Enabled { get; }

    public bool CanToggle => _remotesManager is not null;

    public bool Fetch()
    {
        Trace.Assert(Enabled);
        return DoFetch();
    }

    public bool Prune()
    {
        Trace.Assert(Enabled);
        return DoPrune();
    }

    public void OpenRemoteUrlInBrowser()
    {
        if (!IsRemoteUrlUsingHttp)
        {
            return;
        }

        OsShellUtil.OpenUrlInDefaultBrowser(_remote!.Value.FetchUrl);
    }

    public bool IsRemoteUrlUsingHttp => _remote?.FetchUrl.IsUrlUsingHttp() == true;

    public void Enable(bool fetch)
    {
        Trace.Assert(!Enabled);
        _remotesManager?.ToggleRemoteState(Name, disabled: false);
        if (fetch)
        {
            // DoFetch invokes UICommands.RepoChangedNotifier.Notify
            DoFetch();
        }
        else
        {
            UICommands.RepoChangedNotifier.Notify();
        }
    }

    public void Disable()
    {
        Trace.Assert(Enabled);
        _remotesManager?.ToggleRemoteState(Name, disabled: true);
        UICommands.RepoChangedNotifier.Notify();
    }

    public override void ApplyStyle()
    {
        base.ApplyStyle();

        if (_remote is Remote remote)
        {
            ToolTip.SetTip(
                TreeViewNode,
                remote.PushUrls.Count != 1 && remote.FetchUrl != remote.PushUrls[0]
                    ? $"Fetch: {remote.FetchUrl}\nPush: {string.Join("\n", remote.PushUrls.ToArray())}"
                    : remote.FetchUrl);
        }
    }

    internal override void OnDoubleClick()
        => PopupManageRemotesForm();

    internal void PopupManageRemotesForm()
        => ((RemoteBranchTree)Tree).PopupManageRemotesForm(FullPath);

    private bool DoFetch()
    {
        UICommands.StartPullDialogAndPullImmediately(
            out bool pullCompleted,
            Owner,
            remote: FullPath,
            pullAction: GitPullAction.Fetch);
        return pullCompleted;
    }

    private bool DoPrune()
    {
        UICommands.StartPullDialogAndPullImmediately(
            out bool pullCompleted,
            Owner,
            remote: FullPath,
            pullAction: GitPullAction.FetchPruneAll);
        return pullCompleted;
    }

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
