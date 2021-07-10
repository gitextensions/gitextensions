using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands.Git;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;

namespace GitUI.BranchTreePanel
{
    public partial class RepoObjectsTree
    {
        private sealed class BranchTree : Tree
        {
            private readonly IAheadBehindDataProvider? _aheadBehindDataProvider;
            private readonly ICheckRefs _refsSource;

            // Retains the list of currently loaded branches.
            // This is needed to apply filtering without reloading the data.
            // Whether or not force the reload of data is controlled by <see cref="_isFiltering"/> flag.
            private IReadOnlyList<IGitRef>? _loadedBranches;

            public BranchTree(TreeNode treeNode, IGitUICommandsSource uiCommands, IAheadBehindDataProvider? aheadBehindDataProvider, ICheckRefs refsSource)
                : base(treeNode, uiCommands)
            {
                _aheadBehindDataProvider = aheadBehindDataProvider;
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
                    _loadedBranches = Module.GetRefs(RefsFilter.Heads);
                    token.ThrowIfCancellationRequested();
                }

                return FillBranchTree(_loadedBranches, token);
            }

            /// <inheritdoc/>
            protected internal override void Refresh()
            {
                // Break the local cache to ensure the data is requeried to reflect the required sort order.
                _loadedBranches = null;

                base.Refresh();
            }

            private Nodes FillBranchTree(IReadOnlyList<IGitRef> branches, CancellationToken token)
            {
                #region example

                // (input)
                // a-branch
                // develop/crazy-branch
                // develop/features/feat-next
                // develop/features/feat-next2
                // develop/issues/iss444
                // develop/wild-branch
                // issues/iss111
                // master
                //
                // ->
                // (output)
                // 0 a-branch
                // 0 develop/
                // 1   features/
                // 2      feat-next
                // 2      feat-next2
                // 1   issues/
                // 2      iss444
                // 1   wild-branch
                // 1   wilds/
                // 2      card
                // 0 issues/
                // 1     iss111
                // 0 master

                #endregion

                Nodes nodes = new(this);
                var aheadBehindData = _aheadBehindDataProvider?.GetData();

                var currentBranch = Module.GetSelectedBranch();
                Dictionary<string, BaseBranchNode> pathToNode = new();
                foreach (IGitRef branch in branches)
                {
                    token.ThrowIfCancellationRequested();

                    Validates.NotNull(branch.ObjectId);

                    bool isVisible = !IsFiltering.Value || _refsSource.Contains(branch.ObjectId);
                    LocalBranchNode localBranchNode = new(this, branch.ObjectId, branch.Name, branch.Name == currentBranch, isVisible);

                    if (aheadBehindData is not null && aheadBehindData.ContainsKey(localBranchNode.FullPath))
                    {
                        localBranchNode.UpdateAheadBehind(aheadBehindData[localBranchNode.FullPath].ToDisplay());
                    }

                    var parent = localBranchNode.CreateRootNode(pathToNode, (tree, parentPath) => new BranchPathNode(tree, parentPath));
                    if (parent is not null)
                    {
                        nodes.AddNode(parent);
                    }
                }

                return nodes;
            }

            protected override void PostFillTreeViewNode(bool firstTime)
            {
                if (firstTime)
                {
                    TreeViewNode.Expand();
                }

                // Skip hidden node
                if (TreeViewNode.TreeView is null)
                {
                    return;
                }

                if (TreeViewNode.TreeView.SelectedNode is not null)
                {
                    // If there's a selected treenode, don't stomp over it
                    return;
                }

                var activeBranch = Nodes.DepthEnumerator<LocalBranchNode>().FirstOrDefault(b => b.IsActive);
                TreeViewNode.TreeView.SelectedNode = activeBranch?.TreeViewNode;
            }
        }
    }
}
