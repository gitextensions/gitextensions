using System.Buffers;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.CommandsDialogs.WorktreeDialog;
using Microsoft.VisualStudio.Threading;

namespace GitUI.LeftPanel;

internal sealed class WorktreeTree(TreeNode treeNode, IGitUICommandsSource uiCommands) : Tree(treeNode, uiCommands)
{
    internal void Refresh()
    {
        if (!IsAttached)
        {
            return;
        }

        ReloadNodesDetached((_, cancellationToken) => LoadNodesAsync(cancellationToken), getRefs: null!);
    }

    private async Task<Nodes> LoadNodesAsync(CancellationToken token)
    {
        await TaskScheduler.Default;
        token.ThrowIfCancellationRequested();

        return FillWorktreeTree(token);
    }

    private Nodes FillWorktreeTree(CancellationToken token)
    {
        Nodes nodes = new(this);
        string currentWorkingDir = Module.WorkingDir.TrimEnd(Path.DirectorySeparatorChar);

        IReadOnlyList<GitWorktree> worktrees = Module.GetWorktrees();

        // Use the main worktree's parent as the reference for relative paths.
        // Git always lists the main worktree first. Using the main worktree's parent
        // ensures linked worktrees in a sibling directory produce paths like
        // "repo.worktrees/feature-a" rather than flat names with no directory component.
        string mainWorktreePath = worktrees.Count > 0
            ? worktrees[0].Path.TrimEnd(Path.DirectorySeparatorChar)
            : currentWorkingDir;
        string parentDir = Path.GetDirectoryName(mainWorktreePath) ?? mainWorktreePath;

        List<(GitWorktree Worktree, bool IsCurrent, string RelativePath)> worktreeInfos = [];

        foreach (GitWorktree worktree in worktrees)
        {
            token.ThrowIfCancellationRequested();

            bool isCurrent = string.Equals(
                worktree.Path.TrimEnd(Path.DirectorySeparatorChar),
                currentWorkingDir,
                StringComparison.OrdinalIgnoreCase);

            string relativePath;
            try
            {
                relativePath = Path.GetRelativePath(parentDir, worktree.Path);
            }
            catch
            {
                relativePath = worktree.Path;
            }

            worktreeInfos.Add((worktree, isCurrent, relativePath));
        }

        // Strip the common directory prefix from non-main worktrees to reduce visual noise.
        // The first worktree is always the main worktree and is excluded from prefix calculation.
        string commonPrefix = GetCommonPrefix(
            worktreeInfos.Skip(1).Select(w => w.RelativePath));

        for (int i = 0; i < worktreeInfos.Count; i++)
        {
            token.ThrowIfCancellationRequested();

            (GitWorktree worktree, bool isCurrent, string relativePath) = worktreeInfos[i];

            string displayPath = i > 0
                && commonPrefix.Length > 0
                && relativePath.StartsWith(commonPrefix, StringComparison.OrdinalIgnoreCase)
                ? relativePath[commonPrefix.Length..]
                : relativePath;

            WorktreeNode node = new(this, worktree, isCurrent, displayPath);
            nodes.AddNode(node);
        }

        return nodes;
    }

    /// <summary>
    ///  Finds the longest common prefix among the given relative paths, snapping to a word boundary.
    /// </summary>
    /// <remarks>
    ///  Word boundaries are directory separators and the characters <c>_</c>, <c>-</c>, <c>.</c>, and space.
    ///  This ensures names like "apricot" and "apple" are not truncated to "pricot" and "pple".
    /// </remarks>
    /// <returns>
    ///  The common prefix including its trailing delimiter, or an empty string if there is no common prefix.
    /// </returns>
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

            // Find the longest character-for-character match.
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
            return "";
        }

        // Snap to the last directory separator in the common prefix.
        int dirSepIndex = prefix.LastIndexOfAny(DirectorySeparatorChars);
        if (dirSepIndex >= 0)
        {
            return prefix[..(dirSepIndex + 1)].ToString();
        }

        // Fall back to word-boundary characters only when none of the paths contain
        // directory separators. This handles the case where all worktrees are flat
        // siblings in the same directory (e.g. "repo_dev", "repo_test").
        if (hasDirectorySeparator)
        {
            return "";
        }

        int boundaryIndex = prefix.LastIndexOfAny(WordBoundaryChars);
        if (boundaryIndex < 0)
        {
            return "";
        }

        return prefix[..(boundaryIndex + 1)].ToString();
    }

    private static readonly SearchValues<char> DirectorySeparatorChars = SearchValues.Create(
        [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);

    private static readonly SearchValues<char> WordBoundaryChars = SearchValues.Create(
        ['_', '-', '.', ' ']);

    protected override void PostFillTreeViewNode(bool firstTime)
    {
        if (firstTime && TreeViewNode.Nodes.Count > 1)
        {
            TreeViewNode.Expand();
        }
    }

    public void CreateWorktree(IWin32Window owner)
    {
        string? mainWorktreePath = GetMainWorktreePath();
        if (UICommands.WorktreeCreate(owner, mainWorktreePath))
        {
            Refresh();
        }
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

    private string GetMainWorktreePath()
    {
        foreach (Node node in Nodes)
        {
            if (node is WorktreeNode wt)
            {
                return wt.Worktree.Path;
            }
        }

        return Module.WorkingDir;
    }
}
