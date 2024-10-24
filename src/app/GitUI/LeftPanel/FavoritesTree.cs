using GitCommands;
using GitCommands.Git;
using GitCommands.Remotes;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.CommandDialogs;
using GitUI.UserControls.RevisionGrid;
using Microsoft;

namespace GitUI.LeftPanel
{
    internal sealed class FavoritesTree : BaseRefTree
    {
        private readonly IAheadBehindDataProvider? _aheadBehindDataProvider;
        private readonly IRevisionGridInfo _revisionGridInfo;
        private readonly CachedFavorites _cachedFavorites = new();

        public FavoritesTree(TreeNode treeNode, IGitUICommandsSource uiCommands, IAheadBehindDataProvider? aheadBehindDataProvider, ICheckRefs refsSource, IRevisionGridInfo revisionGridInfo)
            : base(treeNode, uiCommands, refsSource, RefsFilter.Remotes | RefsFilter.Heads)
        {
            _aheadBehindDataProvider = aheadBehindDataProvider;
            _revisionGridInfo = revisionGridInfo;

            if (refsSource is GitModuleControl { Module.WorkingDirGitDir: not null } gitModuleControl)
            {
                _cachedFavorites.Location = gitModuleControl.Module.WorkingDirGitDir;
            }

            _cachedFavorites.Load();
        }

        public void Add(NodeBase node)
        {
            if (node is BaseRevisionNode baseRevisionNode && baseRevisionNode.ObjectId != null)
            {
                _cachedFavorites.Add(baseRevisionNode.ObjectId.ToString());
            }

            Refresh(new FilteredGitRefsProvider(UICommands.Module).GetRefs, true);
        }

        public void Remove(NodeBase node)
        {
            if (node is BaseRevisionNode baseRevisionNode && baseRevisionNode.ObjectId != null)
            {
                _cachedFavorites.Remove(baseRevisionNode.ObjectId.ToString());
            }

            Refresh(new FilteredGitRefsProvider(UICommands.Module).GetRefs, true);
        }

        protected override Nodes FillTree(IReadOnlyList<IGitRef> branches, CancellationToken token)
        {
            Nodes nodes = new(this);
            Dictionary<string, BaseRevisionNode> pathToNodesRemote = [];
            Dictionary<string, BaseRevisionNode> pathToNodeLocal = [];
            IDictionary<string, AheadBehindData> aheadBehindDataLocal = _aheadBehindDataProvider?.GetData();
            IDictionary<string, AheadBehindData>? aheadBehindDataRemote = aheadBehindDataLocal?.DistinctBy(r => r.Value.RemoteRef).ToDictionary(r => r.Value.RemoteRef, r => r.Value);

            foreach (IGitRef branch in branches)
            {
                token.ThrowIfCancellationRequested();

                if (branch.IsRemote)
                {
                    CreateRemoteBranchTree(branch, pathToNodesRemote, nodes, aheadBehindDataRemote);
                }
                else if (branch.IsHead)
                {
                    CreateLocalBranchTree(branch, pathToNodeLocal, nodes, aheadBehindDataLocal);
                }
            }

            _cachedFavorites.Favorites.ToList().Except(branches.Select(p => p.ObjectId?.ToString())).ForEach(p => _cachedFavorites.Favorites.Remove(p));
            return nodes;
        }

        protected override void PostFillTreeViewNode(bool firstTime)
        {
            if (firstTime)
            {
                TreeViewNode.Collapse();
            }
        }

        private void CreateLocalBranchTree(IGitRef branch, Dictionary<string, BaseRevisionNode> pathToNodeLocal, Nodes nodes, IDictionary<string, AheadBehindData>? aheadBehindDataLocal)
        {
            Validates.NotNull(branch.ObjectId);

            if (!_cachedFavorites.Contains(branch.ObjectId.ToString()))
            {
                return;
            }

            string currentBranch = _revisionGridInfo.GetCurrentBranch();

            LocalBranchNode localBranchNode = new(this, branch.ObjectId, branch.Name, branch.Name == currentBranch, true) { Visible = false };

            if (aheadBehindDataLocal?.TryGetValue(localBranchNode.FullPath, out AheadBehindData aheadBehind) is true)
            {
                localBranchNode.UpdateAheadBehind(aheadBehind.ToDisplay(), aheadBehind.RemoteRef);
            }

            BaseRevisionNode parent = localBranchNode.CreateRootNode(pathToNodeLocal, (tree, parentPath) => new BranchPathNode(tree, parentPath));

            if (parent is not null)
            {
                nodes.AddNode(parent);
            }
        }

        private void CreateRemoteBranchTree(IGitRef branch, Dictionary<string, BaseRevisionNode> pathToNodes, Nodes nodes, IDictionary<string, AheadBehindData>? aheadBehindDataRemote)
        {
            Validates.NotNull(branch.ObjectId);

            if (!_cachedFavorites.Contains(branch.ObjectId.ToString()))
            {
                return;
            }

            ConfigFileRemoteSettingsManager remotesManager = new(() => Module);
            Dictionary<string, Remote> remoteByName = ThreadHelper.JoinableTaskFactory.Run(Module.GetRemotesAsync).ToDictionary(r => r.Name);

            string remoteName = branch.Name.SubstringUntil('/');

            if (remoteByName.TryGetValue(remoteName, out Remote remote))
            {
                RemoteBranchNode remoteBranchNode = new(this, branch.ObjectId, branch.Name, true);

                if (aheadBehindDataRemote?.TryGetValue(branch.CompleteName, out AheadBehindData aheadBehind) is true)
                {
                    remoteBranchNode.UpdateAheadBehind(aheadBehind.ToDisplay(), $"{GitRefName.RefsHeadsPrefix}{aheadBehind.Branch}");
                }

                BaseRevisionNode parent = remoteBranchNode.CreateRootNode(
                    pathToNodes,
                    (tree, parentPath) => CreateRemoteBranchPathNode(tree, parentPath, remote));

                if (parent is not null)
                {
                    nodes.AddNode(parent);
                }
            }

            BaseRevisionNode CreateRemoteBranchPathNode(Tree tree, string parentPath, Remote remote)
            {
                if (parentPath == remote.Name)
                {
                    return new RemoteRepoNode(tree, parentPath, remotesManager, remote, true) { Visible = false };
                }

                return new BasePathNode(tree, parentPath);
            }
        }
    }
}
