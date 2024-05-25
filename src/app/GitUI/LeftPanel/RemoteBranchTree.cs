using GitCommands;
using GitCommands.Git;
using GitCommands.Remotes;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid;
using Microsoft;

namespace GitUI.LeftPanel
{
    internal sealed class RemoteBranchTree : BaseRefTree
    {
        private readonly IAheadBehindDataProvider? _aheadBehindDataProvider;

        public RemoteBranchTree(TreeNode treeNode, IGitUICommandsSource uiCommands, IAheadBehindDataProvider? aheadBehindDataProvider, ICheckRefs refsSource)
            : base(treeNode, uiCommands, refsSource, RefsFilter.Remotes)
        {
            _aheadBehindDataProvider = aheadBehindDataProvider;
        }

        protected override Nodes FillTree(IReadOnlyList<IGitRef> branches, CancellationToken token)
        {
            // More than one local can point to a single remote branch, pick one of them.
            Nodes nodes = new(this);
            Dictionary<string, BaseRevisionNode> pathToNodes = [];
            IDictionary<string, AheadBehindData>? aheadBehindData = _aheadBehindDataProvider?.GetData()?.DistinctBy(r => r.Value.RemoteRef).ToDictionary(r => r.Value.RemoteRef, r => r.Value);

            List<RemoteRepoNode> enabledRemoteRepoNodes = [];
            Dictionary<string, Remote> remoteByName = ThreadHelper.JoinableTaskFactory.Run(Module.GetRemotesAsync).ToDictionary(r => r.Name);

            ConfigFileRemoteSettingsManager remotesManager = new(() => Module);

            // Create nodes for enabled remotes with branches
            foreach (IGitRef branch in PrioritizedBranches(branches))
            {
                token.ThrowIfCancellationRequested();

                Validates.NotNull(branch.ObjectId);

                string remoteName = branch.Name.SubstringUntil('/');
                if (remoteByName.TryGetValue(remoteName, out Remote remote))
                {
                    RemoteBranchNode remoteBranchNode = new(this, branch.ObjectId, branch.Name, visible: true);
                    if (aheadBehindData?.TryGetValue(branch.CompleteName, out AheadBehindData aheadBehind) is true)
                    {
                        remoteBranchNode.UpdateAheadBehind(aheadBehind.ToDisplay(), $"{GitRefName.RefsHeadsPrefix}{aheadBehind.Branch}");
                    }

                    BaseRevisionNode parent = remoteBranchNode.CreateRootNode(
                        pathToNodes,
                        (tree, parentPath) => CreateRemoteBranchPathNode(tree, parentPath, remote));

                    if (parent is not null)
                    {
                        enabledRemoteRepoNodes.Add((RemoteRepoNode)parent);
                    }
                }
            }

            // Create nodes for enabled remotes without branches
            IReadOnlyList<string> enabledRemotesNoBranches = GetEnabledRemoteNamesWithoutBranches(branches, remoteByName);
            foreach (string remoteName in enabledRemotesNoBranches)
            {
                if (remoteByName.TryGetValue(remoteName, out Remote remote))
                {
                    RemoteRepoNode node = new(this, remoteName, remotesManager, remote, true);
                    enabledRemoteRepoNodes.Add(node);
                }
            }

            // Add enabled remote nodes in order
            foreach (RemoteRepoNode node in PrioritizedRemotes(enabledRemoteRepoNodes))
            {
                nodes.AddNode(node);
            }

            // Add disabled remotes, if any
            IReadOnlyList<Remote> disabledRemotes = remotesManager.GetDisabledRemotes();
            if (disabledRemotes.Count > 0)
            {
                List<RemoteRepoNode> disabledRemoteRepoNodes = [];
                foreach (Remote remote in disabledRemotes)
                {
                    RemoteRepoNode node = new(this, remote.Name, remotesManager, remote, false);
                    disabledRemoteRepoNodes.Add(node);
                }

                RemoteRepoFolderNode disabledFolderNode = new(this, TranslatedStrings.Inactive);
                foreach (RemoteRepoNode node in PrioritizedRemotes(disabledRemoteRepoNodes))
                {
                    disabledFolderNode.Nodes.AddNode(node);
                }

                nodes.AddNode(disabledFolderNode);
            }

            return nodes;

            BaseRevisionNode CreateRemoteBranchPathNode(Tree tree, string parentPath, Remote remote)
            {
                if (parentPath == remote.Name)
                {
                    return new RemoteRepoNode(tree, parentPath, remotesManager, remote, true);
                }

                return new BasePathNode(tree, parentPath);
            }

            IReadOnlyList<string> GetEnabledRemoteNamesWithoutBranches(IReadOnlyList<IGitRef> branches, Dictionary<string, Remote> remoteByName)
            {
                HashSet<string> remotesWithBranches = branches
                    .Select(branch => branch.Name.SubstringUntil('/'))
                    .ToHashSet();

                HashSet<string> allRemotes = remoteByName.Select(kv => kv.Value.Name).ToHashSet();

                return allRemotes.Except(remotesWithBranches).ToList();
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
                pullAction: GitPullAction.FetchAll);
            return pullCompleted;
        }

        internal bool FetchPruneAll()
        {
            UICommands.StartPullDialogAndPullImmediately(
                out bool pullCompleted,
                TreeViewNode.TreeView,
                pullAction: GitPullAction.FetchPruneAll);
            return pullCompleted;
        }
    }
}
