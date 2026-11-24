using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Media;
using GitCommands;
using GitCommands.Remotes;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.Properties;
using GitUIPluginInterfaces.RepositoryHosts;

namespace GitUI.LeftPanel;

[DebuggerDisplay("Remote = {_remote.Name}, FullPath = {FullPath}")]
internal sealed class RemoteRepoNode : BaseRevisionNode
{
    private readonly Remote? _remote;
    private readonly IConfigFileRemoteSettingsManager? _remotesManager;

    public RemoteRepoNode(Tree tree, string fullPath, IConfigFileRemoteSettingsManager? remotesManager, Remote? remote, bool isEnabled)
        : base(tree, fullPath, visible: true)
    {
        _remote = remote;
        Enabled = isEnabled;
        _remotesManager = remotesManager;
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

        // Framework constraint: the standalone previewer shows remote nodes without their configuration.
        if (_remote is not Remote remote)
        {
            TreeViewNode.SetImage(Images.Remote);
            return;
        }

        ToolTip.SetTip(
            TreeViewNode,
            remote.PushUrls.Count != 1 && remote.FetchUrl != remote.PushUrls[0]
                ? $"Fetch: {remote.FetchUrl}\nPush: {string.Join("\n", remote.PushUrls.ToArray())}"
                : remote.FetchUrl);

        IImage imageKey;
        if (remote.FetchUrl.Contains("github.com"))
        {
            imageKey = Images.GitHub;
        }
        else if (remote.FetchUrl.Contains("bitbucket."))
        {
            imageKey = Images.BitBucket;
        }
        else if (remote.FetchUrl.Contains("visualstudio.com") || remote.FetchUrl.Contains("dev.azure.com"))
        {
            imageKey = Images.VisualStudioTeamServices;
        }
        else
        {
            imageKey = Images.Remote;
        }

        TreeViewNode.SetImage(imageKey);
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
}
