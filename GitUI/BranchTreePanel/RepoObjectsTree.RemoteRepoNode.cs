using System.Diagnostics;
using GitCommands;
using GitCommands.Remotes;
using GitUI.Properties;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree
    {
        [DebuggerDisplay("Remote = {_remote.Name}, FullPath = {FullPath}")]
        private sealed class RemoteRepoNode : BaseBranchNode
        {
            private readonly Remote _remote;
            private readonly IConfigFileRemoteSettingsManager _remotesManager;

            public RemoteRepoNode(Tree tree, string fullPath, IConfigFileRemoteSettingsManager remotesManager, Remote remote, bool isEnabled)
                : base(tree, fullPath, visible: true)
            {
                _remote = remote;
                Enabled = isEnabled;
                _remotesManager = remotesManager;
            }

            public bool Enabled { get; }

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

                OsShellUtil.OpenUrlInDefaultBrowser(_remote.FetchUrl);
            }

            public bool IsRemoteUrlUsingHttp => _remote.FetchUrl.IsUrlUsingHttp();

            public void Enable(bool fetch)
            {
                Trace.Assert(!Enabled);
                _remotesManager.ToggleRemoteState(Name, disabled: false);
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
                _remotesManager.ToggleRemoteState(Name, disabled: true);
                UICommands.RepoChangedNotifier.Notify();
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();

                TreeViewNode.ToolTipText = _remote.PushUrls.Count != 1 && _remote.FetchUrl != _remote.PushUrls[0]
                    ? $"Fetch: {_remote.FetchUrl}\nPush: {string.Join("\n", _remote.PushUrls.ToArray())}"
                    : _remote.FetchUrl;

                string imageKey;
                if (_remote.FetchUrl.Contains("github.com"))
                {
                    imageKey = nameof(Images.GitHub);
                }
                else if (_remote.FetchUrl.Contains("bitbucket."))
                {
                    imageKey = nameof(Images.BitBucket);
                }
                else if (_remote.FetchUrl.Contains("visualstudio.com") || _remote.FetchUrl.Contains("dev.azure.com"))
                {
                    imageKey = nameof(Images.VisualStudioTeamServices);
                }
                else
                {
                    imageKey = nameof(Images.Remote);
                }

                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = imageKey;
            }

            internal override void OnDoubleClick()
            {
                PopupManageRemotesForm();
            }

            internal void PopupManageRemotesForm()
            {
                ((RemoteBranchTree)Tree).PopupManageRemotesForm(FullPath);
            }

            private bool DoFetch()
            {
                UICommands.StartPullDialogAndPullImmediately(
                    out bool pullCompleted,
                    TreeViewNode.TreeView,
                    remote: FullPath,
                    pullAction: AppSettings.PullAction.Fetch);
                return pullCompleted;
            }

            private bool DoPrune()
            {
                UICommands.StartPullDialogAndPullImmediately(
                    out bool pullCompleted,
                    TreeViewNode.TreeView,
                    remote: FullPath,
                    pullAction: AppSettings.PullAction.FetchPruneAll);
                return pullCompleted;
            }
        }
    }
}
