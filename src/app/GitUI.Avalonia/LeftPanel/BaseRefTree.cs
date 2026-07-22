using System.Text.RegularExpressions;
using GitCommands;
using GitExtensions.Extensibility.Git;

namespace GitUI.LeftPanel;

internal abstract class BaseRefTree : BaseRevisionTree
{
    protected BaseRefTree(RepoObjectsTree owner, RepoTreeKind kind, string caption, Avalonia.Media.IImage icon)
        : base(owner, kind, caption, icon)
    {
    }

    protected void FillNested(
        IEnumerable<IGitRef> refs,
        Func<NodeBase, string, IGitRef?, int, BaseRevisionNode> createNode)
    {
        PathEntry root = new(string.Empty);
        int order = 0;
        foreach (IGitRef gitRef in OrderRefs(refs))
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

    private static IEnumerable<IGitRef> OrderRefs(IEnumerable<IGitRef> refs)
    {
        string expression = AppSettings.PrioritizedBranchNames;
        Regex? priority = null;
        if (!string.IsNullOrWhiteSpace(expression))
        {
            try
            {
                priority = new Regex($"^({expression})$", RegexOptions.ExplicitCapture);
            }
            catch (ArgumentException)
            {
                priority = null;
            }
        }

        return refs
            .OrderBy(gitRef => priority?.IsMatch(gitRef.LocalName) == true ? 0 : 1)
            .ThenBy(gitRef => gitRef.Name, StringComparer.OrdinalIgnoreCase);
    }

    private static void AddNode(NodeBase parent, NodeBase child)
    {
        parent.TreeViewNode.Items.Add(child.TreeViewNode);
    }

    private sealed class PathEntry(string path)
    {
        public Dictionary<string, PathEntry> Children { get; } = new(StringComparer.Ordinal);

        public IGitRef? GitRef { get; set; }

        public int Order { get; set; } = int.MaxValue;

        public string Path { get; } = path;
    }
}
