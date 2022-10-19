using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitUI.BranchTreePanel
{
    internal abstract class BaseRefTree : BaseRevisionTree
    {
        // A flag to indicate whether the data is being filtered (e.g. Show Current Branch Only).
        private bool _isFiltering = false;

        // Retains the list of currently loaded refs (branches/tags).
        // This is needed to apply filtering without reloading the data.
        // Whether or not force the reload of data is controlled by <see cref="_isFiltering"/> flag.
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
        /// <param name="isFiltering">
        ///  <see langword="true"/>, if the data is being filtered; otherwise <see langword="false"/>.
        /// </param>
        internal void Refresh(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs, bool forceRefresh, bool isFiltering)
        {
            if (!IsAttached)
            {
                return;
            }

            // If we're not currently filtering and no need to filter now -> exit.
            // Else we need to iterate over the list and rebind the tree - whilst there
            // could be a situation whether a user just refreshed the grid, there could
            // also be a situation where the user applied a different filter, or checked
            // out a different ref (e.g. a branch or commit), and we have a different
            // set of branches to show/hide.
            if (_loadedRefs is not null && !forceRefresh && (!isFiltering && !_isFiltering))
            {
                return;
            }

            _isFiltering = isFiltering;
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
            if (!IsAttached || !_isFiltering)
            {
                return;
            }

            base.UpdateVisibility();
        }
    }
}
