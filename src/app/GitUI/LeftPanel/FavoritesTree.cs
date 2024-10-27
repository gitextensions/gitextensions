using GitCommands;
using GitCommands.Git;
using GitCommands.Remotes;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.CommandDialogs;
using GitUI.Properties;
using GitUI.UserControls.RevisionGrid;
using Microsoft;

namespace GitUI.LeftPanel
{
    internal sealed class FavoritesTree : BaseRefTree
    {
        private readonly IAheadBehindDataProvider? _aheadBehindDataProvider;
        private readonly IRevisionGridInfo _revisionGridInfo;
        private readonly CachedFavorites _cachedFavorites = new();
        private readonly IGitUICommandsSource _refUiCommand;

        public FavoritesTree(TreeNode treeNode, IGitUICommandsSource uiCommands, IAheadBehindDataProvider? aheadBehindDataProvider, ICheckRefs refsSource, IRevisionGridInfo revisionGridInfo)
            : base(treeNode, uiCommands, refsSource, RefsFilter.Remotes | RefsFilter.Heads)
        {
            _aheadBehindDataProvider = aheadBehindDataProvider;
            _revisionGridInfo = revisionGridInfo;
            _refUiCommand = uiCommands;

            if (refsSource is GitModuleControl { Module.WorkingDirGitDir: not null } gitModuleControl)
            {
                _cachedFavorites.Location = gitModuleControl.Module.WorkingDirGitDir;
            }
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
            FavoriteNode remoteNode = new(this, TranslatedStrings.Remotes, nameof(Images.BranchRemoteRoot)) { Visible = true };
            FavoriteNode localNode = new(this, TranslatedStrings.Local, nameof(Images.BranchLocalRoot)) { Visible = true };

            Dictionary<string, BaseRevisionNode> pathToNodesRemote = [];
            Dictionary<string, BaseRevisionNode> pathToNodeLocal = [];
            IDictionary<string, AheadBehindData> aheadBehindDataLocal = _aheadBehindDataProvider?.GetData();

            foreach (IGitRef branch in branches)
            {
                token.ThrowIfCancellationRequested();

                if (branch.IsRemote)
                {
                    CreateRemoteBranchTree(branch, pathToNodesRemote, remoteNode.Nodes, aheadBehindDataLocal);
                }
                else if (branch.IsHead)
                {
                    CreateLocalBranchTree(branch, pathToNodeLocal, localNode.Nodes, aheadBehindDataLocal);
                }
            }

            _cachedFavorites.Favorites.ToList().Except(branches.Select(p => p.ObjectId?.ToString())).ForEach(p => _cachedFavorites.Remove(p));

            Nodes nodes = new(this);
            nodes.AddNode(localNode);
            nodes.AddNode(remoteNode);

            return nodes;
        }

        protected override void PostFillTreeViewNode(bool firstTime)
        {
            if (firstTime)
            {
                TreeViewNode.Collapse();
            }
        }

        private void CreateLocalBranchTree(IGitRef branch, Dictionary<string, BaseRevisionNode> pathToNodes, Nodes nodes, IDictionary<string, AheadBehindData>? aheadBehindData)
        {
            Validates.NotNull(branch.ObjectId);

            if (!_cachedFavorites.Contains(branch.ObjectId.ToString()))
            {
                return;
            }

            string currentBranch = _revisionGridInfo.GetCurrentBranch();

            LocalBranchNode branchNode = new(this, branch.ObjectId, branch.Name, branch.Name == currentBranch, true);

            if (aheadBehindData?.TryGetValue(branchNode.FullPath, out AheadBehindData aheadBehind) is true)
            {
                branchNode.UpdateAheadBehind(aheadBehind.ToDisplay(), aheadBehind.RemoteRef);
            }

            BaseRevisionNode parent = branchNode.CreateRootNode(pathToNodes, CreatePathNode);

            if (parent is not null)
            {
                nodes.AddNode(parent);
            }

            BaseRevisionNode CreatePathNode(Tree tree, string parentPath)
            {
                BranchPathNode branchPathNode = new(tree, parentPath);

                return branchPathNode;
            }
        }

        private void CreateRemoteBranchTree(IGitRef branch, Dictionary<string, BaseRevisionNode> pathToNodes, Nodes nodes, IDictionary<string, AheadBehindData>? aheadBehindData)
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
                RemoteBranchNode branchNode = new(this, branch.ObjectId, branch.Name, true);

                if (aheadBehindData?.TryGetValue(branch.CompleteName, out AheadBehindData aheadBehind) is true)
                {
                    branchNode.UpdateAheadBehind(aheadBehind.ToDisplay(), $"{GitRefName.RefsHeadsPrefix}{aheadBehind.Branch}");
                }

                BaseRevisionNode parent = branchNode.CreateRootNode(pathToNodes, (tree, parentPath) => CreateRemoteBranchPathNode(tree, parentPath, remote));

                if (parent is not null)
                {
                    nodes.AddNode(parent);
                }
            }

            BaseRevisionNode CreateRemoteBranchPathNode(Tree tree, string parentPath, Remote remote)
            {
                if (parentPath == remote.Name)
                {
                    return new RemoteRepoNode(tree, parentPath, remotesManager, remote, true);
                }

                return new BasePathNode(tree, parentPath);
            }
        }
    }
}
