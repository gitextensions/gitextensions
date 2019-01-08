using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUI.CommandsDialogs;
using GitUI.HelperDialogs;
using GitUI.Properties;
using Microsoft.VisualStudio.Threading;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree
    {
        private sealed class RemoteBranchTree : Tree
        {
            public RemoteBranchTree(TreeNode treeNode, IGitUICommandsSource uiCommands)
                : base(treeNode, uiCommands)
            {
                // TODO unsubscribe this event as needed
                uiCommands.UICommandsChanged += delegate { TreeViewNode.TreeView.SelectedNode = null; };
            }

            protected override async Task LoadNodesAsync(CancellationToken token)
            {
                await TaskScheduler.Default;
                token.ThrowIfCancellationRequested();
                var nodes = new Dictionary<string, BaseBranchNode>();

                var branches = Module.GetRefs(tags: true, branches: true, noLocks: true)
                    .Where(branch => branch.IsRemote && !branch.IsTag)
                    .OrderBy(branch => branch.Name)
                    .Select(branch => branch.Name);

                token.ThrowIfCancellationRequested();
                var remoteByName = Module.GetRemotes().ToDictionary(r => r.Name);
                foreach (var branchPath in branches)
                {
                    token.ThrowIfCancellationRequested();
                    var remoteName = branchPath.SubstringUntil('/');
                    if (remoteByName.TryGetValue(remoteName, out var remote))
                    {
                        var remoteBranchNode = new RemoteBranchNode(this, branchPath);
                        var parent = remoteBranchNode.CreateRootNode(
                            nodes,
                            (tree, parentPath) => CreateRemoteBranchPathNode(tree, parentPath, remote));
                        if (parent != null)
                        {
                            Nodes.AddNode(parent);
                        }
                    }
                }

                return;

                BaseBranchNode CreateRemoteBranchPathNode(Tree tree, string parentPath, Remote remote)
                {
                    if (parentPath == remote.Name)
                    {
                        return new RemoteRepoNode(tree, parentPath, remote);
                    }

                    return new BasePathNode(tree, parentPath);
                }
            }

            protected override void FillTreeViewNode()
            {
                base.FillTreeViewNode();
                TreeViewNode.Expand();
            }
        }

        private sealed class RemoteBranchNode : BaseBranchNode
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
                var objectId = Module.RevParse(FullPath);

                if (objectId == null)
                {
                    MessageBox.Show($"Branch \"{FullPath}\" could not be resolved.");
                    return false;
                }
                else
                {
                    return UICommands.StartCreateBranchDialog(TreeViewNode.TreeView, objectId);
                }
            }

            public bool Delete()
            {
                var remoteBranchInfo = GetRemoteBranchInfo();
                return UICommands.StartDeleteRemoteBranchDialog(TreeViewNode.TreeView, remoteBranchInfo.Remote + '/' + remoteBranchInfo.BranchName);
            }

            public bool Checkout()
            {
                return UICommands.StartCheckoutRemoteBranch(TreeViewNode.TreeView, FullPath);
            }

            internal override void OnDoubleClick()
            {
                Checkout();
            }

            public bool Merge()
            {
                return UICommands.StartMergeBranchDialog(TreeViewNode.TreeView, FullPath);
            }

            public bool Rebase()
            {
                return UICommands.StartRebaseDialog(TreeViewNode.TreeView, FullPath);
            }

            public void Reset()
            {
                // TODO: Move this into new function UICommands.StartResetCurrentBranchDialog

                var objectId = Module.RevParse(FullPath);

                if (objectId == null)
                {
                    MessageBox.Show($"Branch \"{FullPath}\" could not be resolved.");
                }
                else
                {
                    using (var form = new FormResetCurrentBranch(UICommands, Module.GetRevision(objectId)))
                    {
                        form.ShowDialog(TreeViewNode.TreeView);
                    }
                }
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

            public RemoteRepoNode(Tree tree, string fullPath, Remote remote) : base(tree, fullPath)
            {
                _remote = remote;
            }

            public bool Fetch()
            {
                UICommands.StartPullDialogAndPullImmediately(
                    out bool pullCompleted,
                    TreeViewNode.TreeView,
                    remote: FullPath,
                    pullAction: AppSettings.PullAction.Fetch);
                return pullCompleted;
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
        }
    }
}
