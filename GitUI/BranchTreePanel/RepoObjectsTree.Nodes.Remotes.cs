using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Remotes;
using GitUI.BranchTreePanel.Interfaces;
using GitUI.HelperDialogs;
using GitUI.Properties;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree
    {
        private sealed class RemoteBranchTree : Tree
        {
            private readonly TranslationString _inactiveRemoteNodeLabel = new TranslationString("Inactive");

            public RemoteBranchTree(TreeNode treeNode, IGitUICommandsSource uiCommands)
                : base(treeNode, uiCommands)
            {
            }

            protected override void PostRepositoryChanged()
            {
                StartAsyncRefreshTree();
            }

            protected override async Task RefreshTreeAsync(CancellationToken token)
            {
                await ReloadNodesAsync(LoadNodesAsync, token);
            }

            private async Task<Nodes> LoadNodesAsync(CancellationToken token)
            {
                await TaskScheduler.Default;
                token.ThrowIfCancellationRequested();
                var nodes = new Nodes(this);
                var pathToNodes = new Dictionary<string, BaseBranchNode>();

                var branches = Module.GetRefs(tags: true, branches: true, noLocks: true)
                    .Where(branch => branch.IsRemote && !branch.IsTag)
                    .OrderBy(branch => branch.Name)
                    .Select(branch => branch.Name);

                token.ThrowIfCancellationRequested();

                var enabledRemoteRepoNodes = new List<RemoteRepoNode>();
                var remoteByName = Module.GetRemotes().ToDictionary(r => r.Name);

                var gitRemoteManager = new GitRemoteManager(() => Module);

                // Create nodes for enabled remotes with branches
                foreach (var branchPath in branches)
                {
                    token.ThrowIfCancellationRequested();
                    var remoteName = branchPath.SubstringUntil('/');
                    if (remoteByName.TryGetValue(remoteName, out var remote))
                    {
                        var remoteBranchNode = new RemoteBranchNode(this, branchPath);
                        var parent = remoteBranchNode.CreateRootNode(
                            pathToNodes,
                            (tree, parentPath) => CreateRemoteBranchPathNode(tree, parentPath, remote));

                        if (parent != null)
                        {
                            enabledRemoteRepoNodes.Add((RemoteRepoNode)parent);
                        }
                    }
                }

                // Create nodes for enabled remotes without branches
                var enabledRemotesNoBranches = gitRemoteManager.GetEnabledRemoteNamesWithoutBranches();
                foreach (var remoteName in enabledRemotesNoBranches)
                {
                    if (remoteByName.TryGetValue(remoteName, out var remote))
                    {
                        var node = new RemoteRepoNode(this, remoteName, gitRemoteManager, remote, true);
                        enabledRemoteRepoNodes.Add(node);
                    }
                }

                // Add enabled remote nodes in order
                enabledRemoteRepoNodes
                    .OrderBy(node => node.FullPath)
                    .ForEach(node => nodes.AddNode(node));

                // Add disabled remotes, if any
                var disabledRemotes = gitRemoteManager.GetDisabledRemotes();
                if (disabledRemotes.Count > 0)
                {
                    var disabledRemoteRepoNodes = new List<RemoteRepoNode>();
                    foreach (var remote in disabledRemotes.OrderBy(remote => remote.Name))
                    {
                        var node = new RemoteRepoNode(this, remote.Name, gitRemoteManager, remote, false);
                        disabledRemoteRepoNodes.Add(node);
                    }

                    var disabledFolderNode = new RemoteRepoFolderNode(this, _inactiveRemoteNodeLabel.Text);
                    disabledRemoteRepoNodes
                        .OrderBy(node => node.FullPath)
                        .ForEach(node => disabledFolderNode.Nodes.AddNode(node));

                    nodes.AddNode(disabledFolderNode);
                }

                return nodes;

                BaseBranchNode CreateRemoteBranchPathNode(Tree tree, string parentPath, Remote remote)
                {
                    if (parentPath == remote.Name)
                    {
                        return new RemoteRepoNode(tree, parentPath, gitRemoteManager, remote, true);
                    }

                    return new BasePathNode(tree, parentPath);
                }
            }

            protected override void PostFillTreeViewNode(CancellationToken token, bool firstTime)
            {
                if (firstTime)
                {
                    TreeViewNode.Expand();
                }
            }

            internal void PopupManageRemotesForm(string remoteName)
            {
                UICommands.StartRemotesDialog(TreeViewNode.TreeView, remoteName);
            }
        }

        private sealed class RemoteBranchNode : BaseBranchNode, IGitRefActions, ICanDelete, ICanRename
        {
            public RemoteBranchNode(Tree tree, string fullPath) : base(tree, fullPath)
            {
            }

            internal override void OnSelected()
            {
                if (Tree.IgnoreSelectionChangedEvent)
                {
                    return;
                }

                base.OnSelected();
                SelectRevision();
            }

            public bool Fetch()
            {
                var remoteBranchInfo = GetRemoteBranchInfo();
                UICommands.StartPullDialogAndPullImmediately(
                    out bool pullCompleted,
                    TreeViewNode.TreeView,
                    remoteBranch: remoteBranchInfo.BranchName,
                    remote: remoteBranchInfo.Remote,
                    pullAction: AppSettings.PullAction.Fetch);
                return pullCompleted;
            }

            private readonly struct RemoteBranchInfo
            {
                public string Remote { get; }
                public string BranchName { get; }

                public RemoteBranchInfo(string remote, string branchName)
                {
                    Remote = remote;
                    BranchName = branchName;
                }
            }

            private RemoteBranchInfo GetRemoteBranchInfo()
            {
                var remote = FullPath.Split('/').First();
                var branch = FullPath.Substring(remote.Length + 1);
                return new RemoteBranchInfo(remote, branch);
            }

            public bool CreateBranch()
            {
                return UICommands.StartCreateBranchDialog(TreeViewNode.TreeView, FullPath);
            }

            public bool Delete()
            {
                var remoteBranchInfo = GetRemoteBranchInfo();
                return UICommands.StartDeleteRemoteBranchDialog(TreeViewNode.TreeView, remoteBranchInfo.Remote + '/' + remoteBranchInfo.BranchName);
            }

            public bool Rename()
            {
                return UICommands.StartRenameDialog(TreeViewNode.TreeView, FullPath);
            }

            public bool Checkout()
            {
                return UICommands.StartCheckoutRemoteBranch(TreeViewNode.TreeView, FullPath);
            }

            public bool Merge()
            {
                return UICommands.StartMergeBranchDialog(TreeViewNode.TreeView, FullPath);
            }

            internal override void OnDoubleClick()
            {
                Checkout();
            }

            public bool FetchAndMerge()
            {
                return Fetch() && Merge();
            }

            public bool FetchAndCheckout()
            {
                return Fetch() && Checkout();
            }

            public bool FetchAndCreateBranch()
            {
                return Fetch() && CreateBranch();
            }

            public bool FetchAndRebase()
            {
                return Fetch() && Rebase();
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();
                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = nameof(Images.BranchRemote);
            }
        }

        private sealed class RemoteRepoNode : BaseBranchNode
        {
            private readonly Remote _remote;
            private readonly IGitRemoteManager _gitRemoteManager;

            public RemoteRepoNode(Tree tree, string fullPath, IGitRemoteManager gitRemoteManager, Remote remote, bool isEnabled)
                : base(tree, fullPath)
            {
                _remote = remote;
                Enabled = isEnabled;
                _gitRemoteManager = gitRemoteManager;
            }

            public bool Enabled { get; private set; }

            public bool Fetch()
            {
                Trace.Assert(Enabled);
                return DoFetch();
            }

            public void Enable(bool fetch)
            {
                Trace.Assert(!Enabled);
                _gitRemoteManager.ToggleRemoteState(Name, disabled: false);
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
                _gitRemoteManager.ToggleRemoteState(Name, disabled: true);
                UICommands.RepoChangedNotifier.Notify();
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();

                TreeViewNode.ToolTipText = _remote.FetchUrl != _remote.PushUrl
                    ? $"Push: {_remote.PushUrl}\nFetch: {_remote.FetchUrl}"
                    : _remote.FetchUrl;

                string imageKey;
                if (_remote.PushUrl.Contains("github.com") || _remote.FetchUrl.Contains("github.com"))
                {
                    imageKey = nameof(Images.GitHub);
                }
                else if (_remote.PushUrl.Contains("bitbucket.") || _remote.FetchUrl.Contains("bitbucket."))
                {
                    imageKey = nameof(Images.BitBucket);
                }
                else if (_remote.PushUrl.Contains("visualstudio.com") || _remote.FetchUrl.Contains("visualstudio.com"))
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
        }

        private sealed class RemoteRepoFolderNode : BaseBranchNode
        {
            public RemoteRepoFolderNode(Tree tree, string name) : base(tree, name)
            {
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();
                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = nameof(Images.FolderClosed);
            }

            protected override string DisplayText()
            {
                return Name;
            }
        }
    }
}
