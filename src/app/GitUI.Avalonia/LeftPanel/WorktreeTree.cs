using System.Buffers;
using GitExtensions.Extensibility.Git;
using GitUI.CommandsDialogs.WorktreeDialog;
using GitUI.Properties;

using ResourceManager;

namespace GitUI.LeftPanel;

internal sealed class WorktreeTree : Tree
{
    public WorktreeTree(RepoObjectsTree owner)
        : base(owner, RepoTreeKind.Worktrees, TranslatedStrings.Worktrees, Images.WorkTree)
    {
        Complete(TranslatedStrings.Worktrees, Images.WorkTree, count: 0, expanded: false);
    }

    public void Load(IReadOnlyList<GitWorktree> worktrees, string currentWorkingDirectory)
    {
        HashSet<string> selected = OwnerControl.CaptureSelectedNodeIdentities(this);
        bool firstLoad = TreeViewNode.Items.Count == 0;
        bool wasExpanded = TreeViewNode.IsExpanded;
        TreeViewNode.Items.Clear();

        string currentWorkingDir = currentWorkingDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string mainWorktreePath = worktrees.Count > 0
            ? worktrees[0].Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            : currentWorkingDir;
        string parentDir = Path.GetDirectoryName(mainWorktreePath) ?? mainWorktreePath;
        List<(GitWorktree Worktree, bool IsCurrent, string RelativePath)> worktreeInfos = [];
        StringComparison pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        foreach (GitWorktree worktree in worktrees)
        {
            bool isCurrent = string.Equals(
                worktree.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                currentWorkingDir,
                pathComparison);
            string relativePath;
            try
            {
                relativePath = Path.GetRelativePath(parentDir, worktree.Path);
            }
            catch (ArgumentException)
            {
                relativePath = worktree.Path;
            }

            worktreeInfos.Add((worktree, isCurrent, relativePath));
        }

        string commonPrefix = GetCommonPrefix(worktreeInfos.Skip(1).Select(worktree => worktree.RelativePath));
        for (int i = 0; i < worktreeInfos.Count; i++)
        {
            (GitWorktree worktree, bool isCurrent, string relativePath) = worktreeInfos[i];
            string displayPath = i > 0
                && commonPrefix.Length > 0
                && relativePath.StartsWith(commonPrefix, StringComparison.OrdinalIgnoreCase)
                    ? relativePath[commonPrefix.Length..]
                    : relativePath;
            AddChild(new WorktreeNode(this, worktree, isCurrent, displayPath));
        }

        Complete(
            TranslatedStrings.Worktrees,
            Images.WorkTree,
            worktrees.Count,
            expanded: firstLoad ? worktrees.Count > 1 : wasExpanded);
        OwnerControl.RestoreSelectedNodes(this, selected);
    }

    /// <summary>
    /// Finds the longest common prefix among the given relative paths, snapping to a word boundary.
    /// </summary>
    /// <remarks>
    /// Word boundaries are directory separators and the characters <c>_</c>, <c>-</c>, <c>.</c>, and space.
    /// </remarks>
    internal static string GetCommonPrefix(IEnumerable<string> paths)
    {
        ReadOnlySpan<char> prefix = default;
        bool hasDirectorySeparator = false;
        int count = 0;

        foreach (string path in paths)
        {
            ReadOnlySpan<char> span = path.AsSpan();
            count++;

            if (count == 1)
            {
                prefix = span;
                hasDirectorySeparator = span.ContainsAny(DirectorySeparatorChars);
                continue;
            }

            hasDirectorySeparator = hasDirectorySeparator || span.ContainsAny(DirectorySeparatorChars);
            int limit = Math.Min(prefix.Length, span.Length);
            int matchLength = 0;
            for (int i = 0; i < limit; i++)
            {
                if (char.ToUpperInvariant(prefix[i]) != char.ToUpperInvariant(span[i]))
                {
                    break;
                }

                matchLength = i + 1;
            }

            prefix = prefix[..matchLength];
        }

        if (count < 2 || prefix.Length == 0)
        {
            return string.Empty;
        }

        int directorySeparatorIndex = prefix.LastIndexOfAny(DirectorySeparatorChars);
        if (directorySeparatorIndex >= 0)
        {
            return prefix[..(directorySeparatorIndex + 1)].ToString();
        }

        if (hasDirectorySeparator)
        {
            return string.Empty;
        }

        int boundaryIndex = prefix.LastIndexOfAny(WordBoundaryChars);
        return boundaryIndex < 0 ? string.Empty : prefix[..(boundaryIndex + 1)].ToString();
    }

    public void CreateWorktree(IWin32Window owner)
    {
        string mainWorktreePath = TreeViewNode.Items
            .Cast<Avalonia.Controls.TreeViewItem>()
            .Select(item => item.Tag)
            .OfType<WorktreeNode>()
            .FirstOrDefault()?.Worktree.Path ?? UICommands.Module.WorkingDir;
        UICommands.WorktreeCreate(owner, mainWorktreePath);
    }

    public void PruneWorktrees(IWin32Window owner)
    {
        if (UICommands.StartCommandLineProcessDialog(owner, command: null, "worktree prune"))
        {
            UICommands.RepoChangedNotifier.Notify();
        }
    }

    public void ManageWorktrees(IWin32Window owner)
    {
        using FormManageWorktree form = new(UICommands);
        form.ShowDialog(owner);
        if (form.ShouldRefreshRevisionGrid)
        {
            UICommands.RepoChangedNotifier.Notify();
        }
    }

    private static readonly SearchValues<char> DirectorySeparatorChars = SearchValues.Create(
        [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);

    private static readonly SearchValues<char> WordBoundaryChars = SearchValues.Create(
        ['_', '-', '.', ' ']);
}
