using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitUI.BranchTreePanel
{
    internal abstract class BaseRefTree : BaseRevisionTree
    {
        // Retains the list of currently loaded refs (branches/tags).
        // This is needed to apply filtering without reloading the data.
        protected IReadOnlyList<IGitRef>? _loadedRefs;

        protected readonly RefsFilter _refsFilter;

        protected BaseRefTree(TreeNode treeNode, IGitUICommandsSource uiCommands, ICheckRefs refsSource, RefsFilter filter)
            : base(treeNode, uiCommands, refsSource)
        {
            _refsFilter = filter;
        }

        protected override void OnAttached()
        {
            _loadedRefs = null;
            base.OnAttached();
        }

        private async Task<Nodes> LoadNodesAsync(CancellationToken token, Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
        {
            await TaskScheduler.Default;
            token.ThrowIfCancellationRequested();

            if (_loadedRefs is null)
            {
                _loadedRefs = getRefs(_refsFilter);
                token.ThrowIfCancellationRequested();
            }

            return FillTree(_loadedRefs, token);
        }

        protected abstract Nodes FillTree(IReadOnlyList<IGitRef> branches, CancellationToken token);

        /// <summary>
        /// Requests (from FormBrowse) to refresh the data tree and to apply filtering, if necessary.
        /// </summary>
        /// <param name="getRefs">Function to get refs.</param>
        /// <param name="forceRefresh">Refresh may be required as references may have been changed.</param>
        internal void Refresh(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs, bool forceRefresh)
        {
            if (!IsAttached)
            {
                return;
            }

            // Since the commits of some branches or tags could have been filtered or not been loaded,
            // we need to iterate over the list and rebind the tree.
            Refresh(getRefs);
        }

        /// <summary>
        /// Requests to refresh the data tree and to apply filtering, if necessary.
        /// </summary>
        protected internal void Refresh(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
        {
            // Break the local cache to ensure the data is requeried to reflect the required sort order.
            _loadedRefs = null;

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ReloadNodesAsync(LoadNodesAsync, getRefs);
            });
        }

        internal override void UpdateVisibility()
        {
            if (!IsAttached)
            {
                return;
            }

            base.UpdateVisibility();
        }
    }
}
