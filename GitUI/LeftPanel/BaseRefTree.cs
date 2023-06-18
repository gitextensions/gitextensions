using System.Text.RegularExpressions;
using GitCommands;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitUI.LeftPanel
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

        protected IEnumerable<IGitRef> PrioritizedBranches(IReadOnlyList<IGitRef> branches)
        {
            return OrderByPriority(branches, node => node.LocalName, AppSettings.PrioritizedBranchNames);
        }

        protected IEnumerable<RemoteRepoNode> PrioritizedRemotes(IReadOnlyList<RemoteRepoNode> remotes)
        {
            return OrderByPriority(remotes.OrderBy(node => node.FullPath).ToList(), node => node.FullPath, AppSettings.PrioritizedRemoteNames);
        }

        /// <summary>
        /// Order the references by the priorities in the setting regex
        /// </summary>
        /// <typeparam name="T">The type to prioritize, e.g. IGitRef.</typeparam>
        /// <param name="references">The branches or remotes to prioritize.</param>
        /// <param name="keySelector">Function in T to get the sorter.</param>
        /// <param name="setting">String with regexes with priorities separated by semicolon.</param>
        /// <returns>The resorted references.</returns>
        private static IEnumerable<T> OrderByPriority<T>(IReadOnlyList<T> references, Func<T, string> keySelector, string setting)
        {
            // Sort prio branches first (if set) with the compile cache (no need to instantiate)
            string[] regexes = setting.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(regex => $"^({regex})$")
                .ToArray();

            if (!regexes.Any())
            {
                return references;
            }

            // A lot of regexes will push out entries from the static regex cache, increasing the time a lot (several hundred ms).
            // (Switching the regex to the outer loop will decrease the effect but the code structure is simpler this way).
            // The static .NET regex cache must be able to include at least all regexes in the loop,
            // ideally also all common use in a "refresh" loop, so increase the size.
            // A few usages in branches vs remote in this method, CommitInfo adds usage for
            // split length of PrioritizedBranchNames (for remotes) and PrioritizedRemoteNames,
            // a few usages in submodule status processing etc.
            // This check should probably be done in a central location only at startup.
            const int additionalRegexCacheEntries = 10;
            if ((regexes.Length * 2) + additionalRegexCacheEntries > Regex.CacheSize)
            {
                Regex.CacheSize = (regexes.Length * 2) + additionalRegexCacheEntries;
            }

            Dictionary<string, int> orderByNodeKey = new();
            foreach (T node in references)
            {
                int currentOrder = 0;
                foreach (string regex in regexes)
                {
                    string key = keySelector(node);
                    if (Regex.IsMatch(key, regex, RegexOptions.ExplicitCapture))
                    {
                        orderByNodeKey[key] = currentOrder;
                        break;
                    }

                    currentOrder++;
                }
            }

            // Order by the sort match, with no match last as int.Max
            return references.OrderBy(node => orderByNodeKey.GetValueOrDefault(keySelector(node), int.MaxValue));
        }
    }
}
