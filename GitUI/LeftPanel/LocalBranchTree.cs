using GitCommands.Git;
using GitUI.CommandDialogs;
using GitUI.Script;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitUI.LeftPanel
{
    internal sealed class LocalBranchTree : BaseRefTree
    {
        private readonly IAheadBehindDataProvider? _aheadBehindDataProvider;
        private readonly IRevisionGridInfo _revisionGridInfo;

        public LocalBranchTree(TreeNode treeNode, IGitUICommandsSource uiCommands, IAheadBehindDataProvider? aheadBehindDataProvider, ICheckRefs refsSource, IRevisionGridInfo revisionGridInfo)
            : base(treeNode, uiCommands, refsSource, RefsFilter.Heads)
        {
            _aheadBehindDataProvider = aheadBehindDataProvider;
            _revisionGridInfo = revisionGridInfo;
        }

        protected override Nodes FillTree(IReadOnlyList<IGitRef> branches, CancellationToken token)
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
            string currentBranch = _revisionGridInfo.GetCurrentBranch();
            Dictionary<string, BaseRevisionNode> pathToNode = new();
            foreach (IGitRef branch in PrioritizedBranches(branches))
            {
                token.ThrowIfCancellationRequested();

                Validates.NotNull(branch.ObjectId);

                LocalBranchNode localBranchNode = new(this, branch.ObjectId, branch.Name, branch.Name == currentBranch, visible: true);

                if (aheadBehindData is not null && aheadBehindData.TryGetValue(localBranchNode.FullPath, out AheadBehindData aheadBehind))
                {
                    localBranchNode.UpdateAheadBehind(aheadBehind.ToDisplay(), aheadBehind.RemoteRef);
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

            var currentBranch = Nodes.DepthEnumerator<LocalBranchNode>().FirstOrDefault(b => b.IsCurrent);
            TreeViewNode.TreeView.SelectedNode = currentBranch?.TreeViewNode;
        }
    }
}
