using System.Buffers;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.CommandsDialogs.WorktreeDialog;
using GitUI.UserControls;
using Microsoft.VisualStudio.Threading;

namespace GitUI.LeftPanel;

internal sealed class WorktreeTree(TreeNode treeNode, IGitUICommandsSource uiCommands) : Tree(treeNode, uiCommands)
{
    // Whether each folder was expanded before the refresh, by full path.
    private Dictionary<string, bool>? _foldersBeforeRefresh;

    internal void Refresh()
    {
        if (!IsAttached)
        {
            return;
        }

        RememberFolders();
        ReloadNodesDetached((_, cancellationToken) => LoadNodesAsync(cancellationToken), getRefs: null!);
    }

    /// <summary>
    ///  Remembers the expanded folders by their full path, which stays the same when a compacted chain is split or merged
    ///  by a refresh, unlike their display path, which the expanded state of the tree view is restored by.
    /// </summary>
    private void RememberFolders()
    {
        _foldersBeforeRefresh = TreeViewNode.TreeView is { InvokeRequired: false }
            ? DepthEnumerator<WorktreeFolderNode>().ToDictionary(
                folder => folder.FullPath,
                folder => folder.TreeViewNode.IsExpanded,
                StringComparer.OrdinalIgnoreCase)
            : null;
    }

    private void RestoreFolders()
    {
        if (_foldersBeforeRefresh is not { } foldersBefore)
        {
            return;
        }

        _foldersBeforeRefresh = null;

        foreach (WorktreeFolderNode folder in DepthEnumerator<WorktreeFolderNode>())
        {
            // A folder split out of an expanded chain stays expanded.
            bool expand = foldersBefore.TryGetValue(folder.FullPath, out bool wasExpanded)
                ? wasExpanded
                : foldersBefore.Any(pair => pair.Value && pair.Key.StartsWith($"{folder.FullPath}/", StringComparison.OrdinalIgnoreCase));

            if (expand)
            {
                folder.TreeViewNode.Expand();
            }
            else
            {
                folder.TreeViewNode.Collapse(ignoreChildren: true);
            }
        }
    }

    private async Task<Nodes> LoadNodesAsync(CancellationToken token)
    {
        await TaskScheduler.Default;
        token.ThrowIfCancellationRequested();

        return FillWorktreeTree(token);
    }

    private Nodes FillWorktreeTree(CancellationToken token)
        => FillWorktreeTree(Module.GetWorktrees(), Module.WorkingDir, token);

    private Nodes FillWorktreeTree(IReadOnlyList<GitWorktree> worktrees, string workingDir, CancellationToken token)
    {
        Nodes nodes = new(this);
        string currentWorkingDir = workingDir.TrimEnd(Path.DirectorySeparatorChar);

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

        // The remaining directories become folders, like branch folders. Folder paths use '/', the separator of BasePathNode.
        List<string[]> worktreeSegments = [];
        Dictionary<string, HashSet<string>> folderChildren = new(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < worktreeInfos.Count; i++)
        {
            (GitWorktree worktree, _, string relativePath) = worktreeInfos[i];

            string displayPath = i > 0
                && commonPrefix.Length > 0
                && relativePath.StartsWith(commonPrefix, StringComparison.OrdinalIgnoreCase)
                ? relativePath[commonPrefix.Length..]
                : relativePath;

            // The main worktree always stays first and at the top level.
            string[] segments = i == 0 ? [displayPath] : GetPathSegments(displayPath, worktree.Path);
            worktreeSegments.Add(segments);

            string folderPath = "";
            for (int s = 0; s < segments.Length - 1; s++)
            {
                folderPath = s == 0 ? segments[s] : $"{folderPath}/{segments[s]}";
                if (!folderChildren.TryGetValue(folderPath, out HashSet<string>? children))
                {
                    folderChildren.Add(folderPath, children = new(StringComparer.OrdinalIgnoreCase));
                }

                // Tell folders from worktrees with the same name.
                children.Add(s < segments.Length - 2 ? $"{segments[s + 1]}/" : segments[s + 1]);
            }
        }

        Dictionary<string, WorktreeFolderNode> folders = new(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < worktreeInfos.Count; i++)
        {
            token.ThrowIfCancellationRequested();

            (GitWorktree worktree, bool isCurrent, _) = worktreeInfos[i];
            string[] segments = worktreeSegments[i];

            Nodes parentNodes = nodes;
            string folderPath = "";
            string folderName = "";
            for (int s = 0; s < segments.Length - 1; s++)
            {
                folderPath = s == 0 ? segments[s] : $"{folderPath}/{segments[s]}";
                folderName = folderName.Length == 0 ? segments[s] : $"{folderName}/{segments[s]}";

                // Compact a chain of single-child folders into one folder "a/b/c", as in the Submodules tree.
                if (folderChildren[folderPath] is { Count: 1 } children && children.First().EndsWith('/'))
                {
                    continue;
                }

                if (!folders.TryGetValue(folderPath, out WorktreeFolderNode? folder))
                {
                    folder = new WorktreeFolderNode(this, folderPath, folderName);
                    folders.Add(folderPath, folder);
                    parentNodes.AddNode(folder);
                }

                parentNodes = folder.Nodes;
                folderName = "";
            }

            parentNodes.AddNode(new WorktreeNode(this, worktree, isCurrent, segments[^1]));
        }

        return nodes;
    }

    /// <summary>
    ///  Splits a display path into its folders and the worktree's own name.
    /// </summary>
    /// <remarks>
    ///  A path outside the common root (on another drive or through "..") is not split: it is shown in full at the top level.
    /// </remarks>
    private static string[] GetPathSegments(string displayPath, string worktreePath)
    {
        string[] segments = displayPath.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length > 0 && !Path.IsPathRooted(displayPath) && !segments.Contains(".."))
        {
            return segments;
        }

        try
        {
            return [AbbreviateKnownFolder(Path.GetFullPath(worktreePath).TrimEnd(Path.DirectorySeparatorChar))];
        }
        catch
        {
            return [worktreePath];
        }
    }

    /// <summary>
    ///  Replaces a leading special folder with its environment variable, e.g. "%LOCALAPPDATA%\tool\worktree".
    /// </summary>
    /// <remarks>
    ///  The longest matching folder wins, e.g. %TEMP% inside %LOCALAPPDATA%, itself inside %USERPROFILE%.
    /// </remarks>
    private static string AbbreviateKnownFolder(string path)
    {
        string? bestVariable = null;
        int bestLength = 0;

        foreach (string variable in KnownFolderVariables)
        {
            string folder = Environment.GetEnvironmentVariable(variable)?.TrimEnd(Path.DirectorySeparatorChar) ?? "";
            if (folder.Length > bestLength
                && path.Length > folder.Length
                && path[folder.Length] == Path.DirectorySeparatorChar
                && path.StartsWith(folder, StringComparison.OrdinalIgnoreCase))
            {
                bestVariable = variable;
                bestLength = folder.Length;
            }
        }

        return bestVariable is null ? path : $"%{bestVariable}%{path[bestLength..]}";
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

    // Environment variables of special folders, which can be pasted into File Explorer.
    private static readonly string[] KnownFolderVariables =
    [
        "TEMP",
        "LOCALAPPDATA",
        "APPDATA",
        "ONEDRIVE",
        "USERPROFILE",
        "PUBLIC",
        "PROGRAMDATA",
        "PROGRAMFILES",
        "PROGRAMFILES(X86)",
        "SYSTEMROOT",
    ];

    protected override void PostFillTreeViewNode(bool firstTime)
    {
        if (!firstTime)
        {
            RestoreFolders();
            return;
        }

        _foldersBeforeRefresh = null;

        if (TreeViewNode.Nodes.Count > 1)
        {
            TreeViewNode.Expand();
        }

        // Keep the current worktree visible, as the current branch is.
        WorktreeNode? current = Nodes.DepthEnumerator<WorktreeNode>().FirstOrDefault(node => node.IsCurrent);
        for (TreeNode? folder = current?.TreeViewNode.Parent; folder is not null && folder != TreeViewNode; folder = folder.Parent)
        {
            folder.Expand();
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

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor(WorktreeTree tree)
    {
        public void FillTree(IReadOnlyList<GitWorktree> worktrees, string workingDir, bool firstTime = true)
        {
            if (!firstTime)
            {
                tree.RememberFolders();
            }

            HashSet<string> expandedNodesState = firstTime ? [] : tree.TreeViewNode.GetExpandedNodesState();
            tree.Nodes.Clear();
            tree.Nodes.AddNodes(tree.FillWorktreeTree(worktrees, workingDir, CancellationToken.None));
            tree.Nodes.FillTreeViewNode(tree.TreeViewNode);
            tree.PostFillTreeViewNode(firstTime);
            tree.TreeViewNode.RestoreExpandedNodesState(expandedNodesState);
        }
    }
}
