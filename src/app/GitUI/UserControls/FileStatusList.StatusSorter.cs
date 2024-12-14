using GitExtensions.Extensibility.Git;
using GitUI.UserControls;

namespace GitUI;

partial class FileStatusList
{
    internal sealed class StatusSorter : IStatusSorter
    {
        public TreeNode CreateTreeSortedByPath(IEnumerable<GitItemStatus> statuses, bool flat, Func<GitItemStatus, TreeNode> createNode)
        {
            TreeNode root = new() { Tag = RelativePath.From("") };

            TreeNode? previousLeaf = null;
            foreach (GitItemStatus status in statuses.OrderBy(s => s, new PathFirstComparer()))
            {
                TreeNode parent = flat || previousLeaf is null
                    ? root
                    : GetOrCreateParent(previousLeaf, status.Path) ?? root;
                TreeNode leaf = createNode(status);
                parent.Nodes.Add(leaf);
                previousLeaf = leaf;
            }

            root.Items().ForEach(RemoveParentPath);

            return root;

            void RemoveParentPath(TreeNode node)
            {
                if (node.Parent?.Tag is RelativePath parentPath)
                {
                    if (parentPath.Length > 0 && node.Text.StartsWith(parentPath.Value))
                    {
                        node.Text = node.Text[(parentPath.Length + 1)..];
                    }

                    if (!flat && node.Tag is FileStatusItem item && item.Item.Path != parentPath && !item.Item.IsRangeDiff)
                    {
                        node.StateImageIndex = 0;
                    }
                }
            }
        }

        private static RelativePath GetCommonPath(RelativePath relativePathA, RelativePath relativePathB)
        {
            string a = $"{relativePathA}/";
            string b = $"{relativePathB}/";
            for (int commonEnd = 0; ; ++commonEnd)
            {
                if (commonEnd >= a.Length || commonEnd >= b.Length || a[commonEnd] != b[commonEnd])
                {
                    // Revert possible partial match
                    while (commonEnd > 0 && a[--commonEnd] != '/')
                    {
                    }

                    return RelativePath.From(a[..commonEnd]);
                }
            }
        }

        private static TreeNode? GetOrCreateParent(TreeNode previousLeaf, RelativePath currentPath)
        {
            RelativePath previousPath = ((FileStatusItem)previousLeaf.Tag).Item.Path;
            if (previousPath == currentPath && (RelativePath)previousLeaf.Parent.Tag == currentPath)
            {
                return previousLeaf.Parent;
            }

            RelativePath commonPath = GetCommonPath(previousPath, currentPath);
            if (commonPath.Length == 0)
            {
                return null;
            }

            TreeNode splitCandidate = previousLeaf;
            RelativePath splitCandidatePath = previousPath;
            while (splitCandidate.Parent?.Tag is RelativePath path && path.Value.StartsWith(commonPath.Value))
            {
                splitCandidate = splitCandidate.Parent;
                splitCandidatePath = path;
            }

            if (splitCandidate != previousLeaf && (splitCandidatePath == currentPath || splitCandidatePath == commonPath))
            {
                return splitCandidate;
            }

            return Split(splitCandidate, commonPath);
        }

        private static TreeNode Split(TreeNode subNode, RelativePath commonPath)
        {
            TreeNode parentNode = subNode.Parent ?? throw new ArgumentNullException($"{nameof(subNode)}.{nameof(subNode.Parent)}");
            int index = parentNode.Nodes.IndexOf(subNode);
            parentNode.Nodes.Remove(subNode);
            TreeNode commonFolderNode = new(commonPath.Value) { Tag = commonPath };
            commonFolderNode.Nodes.Add(subNode);
            parentNode.Nodes.Insert(index, commonFolderNode);
            return commonFolderNode;
        }

        private sealed class PathFirstComparer : IComparer<GitItemStatus>
        {
            public int Compare(GitItemStatus l, GitItemStatus r)
                => (l, r) switch
                {
                    (null, null) => 0,
                    (_, null) => -1,
                    (null, _) => 1,
                    _ => CompareNonNull(l, r)
                };

            private static int CompareNonNull(GitItemStatus l, GitItemStatus r)
            {
                int pathComparison = (l.Path.Value, r.Path.Value) switch
                {
                    ("", "") => 0,
                    (_, "") => -1,
                    ("", _) => 1,
                    _ => StringComparer.InvariantCultureIgnoreCase.Compare(l.Path.Value, r.Path.Value)
                };

                return pathComparison switch
                {
                    -1 => r.Path.Value.StartsWith(l.Path.Value, StringComparison.InvariantCultureIgnoreCase) ? 1 : -1,
                    1 => l.Path.Value.StartsWith(r.Path.Value, StringComparison.InvariantCultureIgnoreCase) ? -1 : 1,
                    _ => StringComparer.InvariantCultureIgnoreCase.Compare(l.Name, r.Name)
                };
            }
        }

        internal static class TestAccessor
        {
            public static string GetCommonPath(string a, string b) => StatusSorter.GetCommonPath(RelativePath.From(a), RelativePath.From(b)).Value;
        }
    }
}
