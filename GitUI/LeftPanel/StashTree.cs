using GitCommands;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitUI.LeftPanel
{
    internal sealed class StashTree : BaseRevisionTree
    {
        public StashTree(TreeNode treeNode, IGitUICommandsSource uiCommands, ICheckRefs refsSource)
            : base(treeNode, uiCommands, refsSource)
        {
        }

        internal void Refresh(Lazy<IReadOnlyCollection<GitRevision>> getStashRevs)
        {
            if (!IsAttached)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ReloadNodesAsync((token, _) =>
                {
                    Task<Nodes>? loadNodesTask = null;
                    _updateSemaphore.WaitAsync();
                    try
                    {
                        loadNodesTask = LoadNodesAsync(token, getStashRevs);
                    }
                    finally
                    {
                        _updateSemaphore.Release();
                    }

                    return loadNodesTask;
                }, null).ConfigureAwait(false);
            });
        }

        private async Task<Nodes> LoadNodesAsync(CancellationToken token, Lazy<IReadOnlyCollection<GitRevision>> getStashRevs)
        {
            await TaskScheduler.Default;
            token.ThrowIfCancellationRequested();

            return FillStashTree(getStashRevs.Value, token);
        }

        private Nodes FillStashTree(IReadOnlyCollection<GitRevision> stashes, CancellationToken token)
        {
            Nodes nodes = new(this);
            Dictionary<string, BaseRevisionNode> pathToNodes = new();

            foreach (GitRevision stash in stashes)
            {
                token.ThrowIfCancellationRequested();

                // Visibility is set after the grid is loaded
                StashNode node = new(this, stash.ObjectId, stash.ReflogSelector, stash.Subject, visible: false);
                Node? parent = node.CreateRootNode(pathToNodes, (tree, parentPath) => new BasePathNode(tree, parentPath));

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
                TreeViewNode.Collapse();
            }
        }

        public void StashAll(IWin32Window owner)
        {
            UICommands.StashSave(owner, AppSettings.IncludeUntrackedFilesInManualStash);
        }

        public void StashStaged(IWin32Window owner)
        {
            UICommands.StashStaged(owner);
        }

        public void OpenStash(IWin32Window owner)
        {
            UICommands.StartStashDialog(owner, manageStashes: true);
        }
    }
}
