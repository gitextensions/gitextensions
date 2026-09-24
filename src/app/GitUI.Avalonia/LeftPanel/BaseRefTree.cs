using System.Text.RegularExpressions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid;

namespace GitUI.LeftPanel;

internal abstract class BaseRefTree : BaseRevisionTree
{
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

    protected IEnumerable<IGitRef> PrioritizedBranches(IReadOnlyList<IGitRef> branches)
        => OrderByPriority(branches, node => node.LocalName, AppSettings.PrioritizedBranchNames);

    protected IEnumerable<RemoteRepoNode> PrioritizedRemotes(IReadOnlyList<RemoteRepoNode> remotes)
        => OrderByPriority(remotes.OrderBy(node => node.FullPath).ToList(), node => node.FullPath, AppSettings.PrioritizedRemoteNames);

    /// <summary>
    /// Order the references by the priorities in the setting regex
    /// </summary>
    private static IEnumerable<T> OrderByPriority<T>(IReadOnlyList<T> references, Func<T, string> keySelector, string setting)
    {
        string[] regexes = [.. setting.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(regex => $"^({regex})$")];
        if (regexes.Length == 0)
        {
            return references;
        }

        const int additionalRegexCacheEntries = 10;
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
