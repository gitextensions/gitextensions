using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Remotes;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree
    {
        private sealed class RemoteBranchTree : Tree
        {
            private readonly ICheckRefs _refsSource;

            // Retains the list of currently loaded branches.
            // This is needed to apply filtering without reloading the data.
            // Whether or not force the reload of data is controlled by <see cref="_isFiltering"/> flag.
            private IReadOnlyList<IGitRef>? _loadedBranches;

            public RemoteBranchTree(TreeNode treeNode, IGitUICommandsSource uiCommands, ICheckRefs refsSource)
                : base(treeNode, uiCommands)
            {
                _refsSource = refsSource;
            }

            protected override bool SupportsFiltering => true;

            protected override Task OnAttachedAsync()
            {
                IsFiltering.Value = false;
                return ReloadNodesAsync(LoadNodesAsync);
            }

            protected override Task PostRepositoryChangedAsync()
            {
                IsFiltering.Value = false;
                return ReloadNodesAsync(LoadNodesAsync);
            }

            protected override async Task<Nodes> LoadNodesAsync(CancellationToken token)
            {
                await TaskScheduler.Default;
                token.ThrowIfCancellationRequested();

                if (!IsFiltering.Value || _loadedBranches is null)
                {
                    _loadedBranches = Module.GetRefs(RefsFilter.Remotes).ToList();
                    token.ThrowIfCancellationRequested();
                }

                return await FillBranchTreeAsync(_loadedBranches, token);
            }

            /// <inheritdoc/>
            protected internal override void Refresh()
            {
                // Break the local cache to ensure the data is requeried to reflect the required sort order.
                _loadedBranches = null;

                base.Refresh();
            }

            private async Task<Nodes> FillBranchTreeAsync(IReadOnlyList<IGitRef> branches, CancellationToken token)
            {
                Nodes nodes = new(this);
                Dictionary<string, BaseBranchNode> pathToNodes = new();

                List<RemoteRepoNode> enabledRemoteRepoNodes = new();
                var remoteByName = (await Module.GetRemotesAsync().ConfigureAwaitRunInline()).ToDictionary(r => r.Name);

                ConfigFileRemoteSettingsManager remotesManager = new(() => Module);

                // Create nodes for enabled remotes with branches
                foreach (IGitRef branch in branches)
                {
                    token.ThrowIfCancellationRequested();

                    Validates.NotNull(branch.ObjectId);

                    bool isVisible = !IsFiltering.Value || _refsSource.Contains(branch.ObjectId);
                    var remoteName = branch.Name.SubstringUntil('/');
                    if (remoteByName.TryGetValue(remoteName, out Remote remote))
                    {
                        RemoteBranchNode remoteBranchNode = new(this, branch.ObjectId, branch.Name, isVisible);

                        var parent = remoteBranchNode.CreateRootNode(
                            pathToNodes,
                            (tree, parentPath) => CreateRemoteBranchPathNode(tree, parentPath, remote));

                        if (parent is not null)
                        {
                            enabledRemoteRepoNodes.Add((RemoteRepoNode)parent);
                        }
                    }
                }

                // Create nodes for enabled remotes without branches
                var enabledRemotesNoBranches = remotesManager.GetEnabledRemoteNamesWithoutBranches();
                foreach (var remoteName in enabledRemotesNoBranches)
                {
                    if (remoteByName.TryGetValue(remoteName, out var remote))
                    {
                        RemoteRepoNode node = new(this, remoteName, remotesManager, remote, true);
                        enabledRemoteRepoNodes.Add(node);
                    }
                }

                // Add enabled remote nodes in order
                enabledRemoteRepoNodes
                    .OrderBy(node => node.FullPath)
                    .ForEach(node => nodes.AddNode(node));

                // Add disabled remotes, if any
                var disabledRemotes = remotesManager.GetDisabledRemotes();
                if (disabledRemotes.Count > 0)
                {
                    List<RemoteRepoNode> disabledRemoteRepoNodes = new();
                    foreach (var remote in disabledRemotes.OrderBy(remote => remote.Name))
                    {
                        RemoteRepoNode node = new(this, remote.Name, remotesManager, remote, false);
                        disabledRemoteRepoNodes.Add(node);
                    }

                    RemoteRepoFolderNode disabledFolderNode = new(this, TranslatedStrings.Inactive);
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
                        return new RemoteRepoNode(tree, parentPath, remotesManager, remote, true);
                    }

                    return new BasePathNode(tree, parentPath);
                }
            }

            protected override void PostFillTreeViewNode(bool firstTime)
            {
                if (firstTime)
                {
                    TreeViewNode.Expand();
                }
            }

            internal void PopupManageRemotesForm(string? remoteName)
            {
                UICommands.StartRemotesDialog(TreeViewNode.TreeView, remoteName);
            }

            internal bool FetchAll()
            {
                UICommands.StartPullDialogAndPullImmediately(
                    out bool pullCompleted,
                    TreeViewNode.TreeView,
                    pullAction: AppSettings.PullAction.FetchAll);
                return pullCompleted;
            }

            internal bool FetchPruneAll()
            {
                UICommands.StartPullDialogAndPullImmediately(
                    out bool pullCompleted,
                    TreeViewNode.TreeView,
                    pullAction: AppSettings.PullAction.FetchPruneAll);
                return pullCompleted;
            }
        }
    }
}
