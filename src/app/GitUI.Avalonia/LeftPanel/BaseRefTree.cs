using System.Text.RegularExpressions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid;
using Microsoft.VisualStudio.Threading;

namespace GitUI.LeftPanel;

internal abstract class BaseRefTree : BaseRevisionTree
{
    // Retains the list of currently loaded refs (branches/tags).
    // This is needed to apply filtering without reloading the data.
    protected IReadOnlyList<IGitRef>? _loadedRefs;

    protected readonly RefsFilter _refsFilter;

    protected BaseRefTree(
        RepoObjectsTree owner,
        RepoTreeKind kind,
        string caption,
        Avalonia.Media.IImage icon,
        ICheckRefs? refsSource,
        RefsFilter filter)
        : base(owner, kind, caption, icon, refsSource)
    {
        _refsFilter = filter;
    }

    protected override void OnAttached()
    {
        _loadedRefs = null;
        base.OnAttached();
    }

    private async Task<Nodes> LoadNodesAsync(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs, CancellationToken token)
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

    protected void FillNested(
        IEnumerable<IGitRef> refs,
        Func<NodeBase, string, IGitRef?, int, BaseRevisionNode> createNode)
    {
        PathEntry root = new(string.Empty);
        int order = 0;
        foreach (IGitRef gitRef in refs)
        {
            PathEntry entry = root;
            string[] parts = gitRef.Name.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string path = string.Empty;
            foreach (string part in parts)
            {
                path = string.IsNullOrEmpty(path) ? part : $"{path}/{part}";
                if (!entry.Children.TryGetValue(part, out PathEntry? child))
                {
                    child = new PathEntry(path);
                    entry.Children.Add(part, child);
                }

                child.Order = Math.Min(child.Order, order);
                entry = child;
            }

            entry.GitRef ??= gitRef;
            order++;
        }

        AddChildren(this, root, level: 0);
        return;

        void AddChildren(NodeBase parent, PathEntry parentEntry, int level)
        {
            foreach (PathEntry entry in parentEntry.Children.Values
                         .OrderBy(item => item.Order)
                         .ThenBy(item => item.Path, StringComparer.OrdinalIgnoreCase))
            {
                BaseRevisionNode node = createNode(parent, entry.Path, entry.GitRef, level);
                AddNode(parent, node);
                AddChildren(node, entry, level + 1);
            }
        }
    }

    protected virtual Nodes FillTree(IReadOnlyList<IGitRef> branches, CancellationToken token)
        => Nodes;

    /// <summary>
    /// Requests (from FormBrowse) to refresh the data tree and to apply filtering, if necessary.
    /// </summary>
    /// <param name="getRefs">Function to get refs.</param>
    internal void Refresh(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
    {
        if (!IsAttached)
        {
            return;
        }

        // Since the commits of some branches or tags could have been filtered or not been loaded,
        // we need to iterate over the list and rebind the tree.
        RefreshInternal(getRefs);
    }

    /// <summary>
    /// Requests to refresh the data tree and to apply filtering, if necessary.
    /// </summary>
    protected internal void RefreshInternal(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
    {
        // Break the local cache to ensure the data is requeried to reflect the required sort order.
        _loadedRefs = null;

        HashSet<string> expandedNodes =
        [
            .. DescendantsAndSelf()
                .Where(node => node.TreeViewNode.IsExpanded)
                .Select(RepoObjectsTree.GetNodeIdentity),
        ];
        TreeSelectionState selectedNodes = Owner.CaptureSelectionState(this);

        Owner.UpdateNodes(() =>
        {
            Nodes.Clear();
            TreeViewNode.Items.Clear();
            _loadedRefs = getRefs(_refsFilter);
            FillTree(_loadedRefs, CancellationToken.None);
            foreach (NodeBase node in DescendantsAndSelf().Skip(1))
            {
                node.ApplyStyle();
            }

            Nodes.FillTreeViewNode(TreeViewNode);

            foreach (NodeBase node in DescendantsAndSelf())
            {
                node.TreeViewNode.IsExpanded = expandedNodes.Contains(RepoObjectsTree.GetNodeIdentity(node));
            }

            Owner.RestoreSelectionState(this, selectedNodes);
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
        => OrderByPriority(branches, node => node.LocalName, AppSettings.PrioritizedBranchNames);

    protected IEnumerable<RemoteRepoNode> PrioritizedRemotes(IReadOnlyList<RemoteRepoNode> remotes)
        => OrderByPriority(remotes.OrderBy(node => node.FullPath).ToList(), node => node.FullPath, AppSettings.PrioritizedRemoteNames);

    /// <summary>
    ///  Order the references by the priorities in the setting regex
    /// </summary>
    /// <typeparam name="T">The type to prioritize, e.g. IGitRef.</typeparam>
    /// <param name="references">The branches or remotes to prioritize.</param>
    /// <param name="keySelector">Function in T to get the sorter.</param>
    /// <param name="setting">String with regexes with priorities separated by semicolon.</param>
    /// <returns>The resorted references.</returns>
    private static IEnumerable<T> OrderByPriority<T>(IReadOnlyList<T> references, Func<T, string> keySelector, string setting)
    {
        // Sort prio branches first (if set) with the compile cache (no need to instantiate)
        string[] regexes = [.. setting.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(regex => $"^({regex})$")];
        if (regexes.Length == 0)
        {
            return references;
        }

        const int additionalRegexCacheEntries = 10;

        // A lot of regexes will push out entries from the static regex cache, increasing the time a lot (several hundred ms).
        // (Switching the regex to the outer loop will decrease the effect but the code structure is simpler this way).
        // The static .NET regex cache must be able to include at least all regexes in the loop,
        // ideally also all common use in a "refresh" loop, so increase the size.
        // A few usages in branches vs remote in this method, CommitInfo adds usage for
        // split length of PrioritizedBranchNames (for remotes) and PrioritizedRemoteNames,
        // a few usages in submodule status processing etc.
        // This check should probably be done in a central location only at startup.
        if ((regexes.Length * 2) + additionalRegexCacheEntries > Regex.CacheSize)
        {
            Regex.CacheSize = (regexes.Length * 2) + additionalRegexCacheEntries;
        }

        Dictionary<string, int> orderByNodeKey = [];
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

    private static void AddNode(NodeBase parent, NodeBase child)
    {
        parent.AddChild(child);
    }

    private sealed class PathEntry(string path)
    {
        public Dictionary<string, PathEntry> Children { get; } = new(StringComparer.Ordinal);

        public IGitRef? GitRef { get; set; }

        public int Order { get; set; } = int.MaxValue;

        public string Path { get; } = path;
    }
}
