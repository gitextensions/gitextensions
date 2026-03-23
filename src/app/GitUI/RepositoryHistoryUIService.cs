using System.Collections.Concurrent;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using Microsoft.VisualStudio.Threading;

namespace GitUI;

/// <summary>
///  Represents a service for managing the git repository history.
/// </summary>
public interface IRepositoryHistoryUIService
{
    /// <summary>
    ///  Occurs whenever the git module changes.
    /// </summary>
    event EventHandler<GitModuleEventArgs> GitModuleChanged;

    /// <summary>
    ///  Populates the "Favourite repositories" menu in the Dashboard.
    ///  Both the submenu to the WorkingDir button in Browse and menu in Dashboard.
    /// </summary>
    /// <param name="container">The container to populate with menu items.</param>
    void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container);

    /// <summary>
    ///  Populates the "Recent repositories" menu.
    ///  Both the WorkingDir button in Browse and menu in Dashboard.
    /// </summary>
    /// <param name="container">The container to populate with menu items.</param>
    void PopulateRecentRepositoriesMenu(ToolStripDropDownItem container);

    /// <summary>
    ///  Initiates a background update of the branch name cache for all recent and favourite repositories.
    /// </summary>
    Task UpdateBranchNameCacheAsync();
}

internal class RepositoryHistoryUIService : IRepositoryHistoryUIService
{
    private readonly IGitExecutorProvider _executorProvider;
    private readonly IRepositoryCurrentBranchNameProvider _repositoryCurrentBranchNameProvider;
    private readonly IInvalidRepositoryRemover _invalidRepositoryRemover;
    private readonly ConcurrentDictionary<string, string> _branchNameCache = new();

    // history can be loaded both by trigger and menu opening
    private volatile AsyncLazy<(IList<Repository> Recent, IList<Repository> Favourite)>? _historyLazy;
    private AsyncLazy<(IList<Repository> Recent, IList<Repository> Favourite)> HistoryLazy
        => _historyLazy ??= CreateHistoryLazy();

    public event EventHandler<GitModuleEventArgs> GitModuleChanged;

    internal RepositoryHistoryUIService(IGitExecutorProvider executorProvider, IRepositoryCurrentBranchNameProvider repositoryCurrentBranchNameProvider, IInvalidRepositoryRemover invalidRepositoryRemover)
    {
        _executorProvider = executorProvider;
        _repositoryCurrentBranchNameProvider = repositoryCurrentBranchNameProvider;
        _invalidRepositoryRemover = invalidRepositoryRemover;
    }

    public void PopulateRecentRepositoriesMenu(ToolStripDropDownItem container)
    {
        List<RecentRepoInfo> pinnedRepos = [];
        List<RecentRepoInfo> allRecentRepos = [];

        IList<Repository> repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(
            async () => (await HistoryLazy.GetValueAsync()).Recent);

        if (repositoryHistory.Count < 1)
        {
            return;
        }

        RecentRepoSplitter splitter = new()
        {
            MeasureFont = container.Font,
        };

        splitter.SplitRecentRepos(repositoryHistory, pinnedRepos, allRecentRepos);

        int number = 0;
        foreach (RecentRepoInfo repo in pinnedRepos)
        {
            AddRecentRepositories(container, repo.Repo, repo.Caption, ++number, repo.Anchored);
        }

        if (allRecentRepos.Count > 0)
        {
            if (pinnedRepos.Count > 0)
            {
                container.DropDownItems.Add(new ToolStripSeparator());
            }

            foreach (RecentRepoInfo repo in allRecentRepos)
            {
                AddRecentRepositories(container, repo.Repo, repo.Caption, ++number, repo.Anchored);
            }
        }
    }

    public void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container)
    {
        container.DropDownItems.Clear();

        IList<Repository> repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(
            async () => (await HistoryLazy.GetValueAsync()).Favourite);

        if (repositoryHistory.Count < 1)
        {
            return;
        }

        PopulateFavouriteRepositoriesMenu(container, repositoryHistory);
    }

    public async Task UpdateBranchNameCacheAsync()
    {
        _historyLazy = CreateHistoryLazy();

        (IList<Repository> recentHistory, IList<Repository> favouriteHistory) = await HistoryLazy.GetValueAsync();

        string[] paths = [.. recentHistory
            .Concat(favouriteHistory)
            .Select(r => r.Path)
            .Distinct(StringComparer.InvariantCulture)];

        if (paths.Length > 0)
        {
            ThreadHelper.FileAndForget(() => UpdateBranchNamesCache(paths));
        }
    }

    private void AddRecentRepositories(ToolStripDropDownItem menuItemContainer, Repository repo, string? caption, int number, bool anchored = false)
    {
        string numberString = number switch { < 10 => $"&{number}", 10 => "1&0", _ => $"{number}" };
        ToolStripMenuItem item = new($"{numberString}: {caption}")
        {
            DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
        };

        if (anchored)
        {
            item.Image = Images.Pin;
        }

        menuItemContainer.DropDownItems.Add(item);

        item.Click += (obj, args) =>
        {
            OpenRepo(repo.Path);
        };

        if (repo.Path != caption)
        {
            item.ToolTipText = repo.Path;
        }

        if (_branchNameCache.TryGetValue(repo.Path, out string? cachedBranchName))
        {
            item.ShortcutKeyDisplayString = cachedBranchName;
        }
    }

    private void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container, in IList<Repository> repositoryHistory)
    {
        List<RecentRepoInfo> pinnedRepos = [];
        List<RecentRepoInfo> allRecentRepos = [];

        RecentRepoSplitter splitter = new()
        {
            MeasureFont = container.Font,
        };

        splitter.SplitRecentRepos(repositoryHistory, pinnedRepos, allRecentRepos);

        foreach (IGrouping<string, RecentRepoInfo> repo in pinnedRepos.Union(allRecentRepos).GroupBy(k => k.Repo.Category).OrderBy(k => k.Key))
        {
            AddFavouriteRepositories(repo.Key, repo);
        }

        return;

        void AddFavouriteRepositories(string? category, IEnumerable<RecentRepoInfo> repos)
        {
            ToolStripMenuItem menuItemCategory = new(category);
            container.DropDownItems.Add(menuItemCategory);

            menuItemCategory.DropDown.SuspendLayout();
            int number = 0;
            foreach (RecentRepoInfo r in repos)
            {
                AddRecentRepositories(menuItemCategory, r.Repo, r.Caption, ++number);
            }

            menuItemCategory.DropDown.ResumeLayout();
        }
    }

    private void UpdateBranchNamesCache(IReadOnlyList<string> paths)
    {
        (string Path, string BranchName)[] fetched = [.. paths
            .AsParallel()
            .Select(path => (path, BranchName: _repositoryCurrentBranchNameProvider.GetCurrentBranchName(path)))];

        foreach ((string path, string branchName) in fetched)
        {
            if (string.IsNullOrWhiteSpace(branchName))
            {
                _branchNameCache.TryRemove(path, out _);
            }
            else
            {
                _branchNameCache[path] = branchName;
            }
        }
    }

    private static async Task<(IList<Repository> Recent, IList<Repository> Favourite)> LoadHistoryAsync()
    {
        IList<Repository> recent = await RepositoryHistoryManager.Locals.LoadRecentHistoryAsync();
        IList<Repository> favourite = await RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync();
        return (recent, favourite);
    }

    private static AsyncLazy<(IList<Repository> Recent, IList<Repository> Favourite)> CreateHistoryLazy()
        => new(LoadHistoryAsync, ThreadHelper.JoinableTaskFactory);

    private void ChangeWorkingDir(string path)
    {
        GitModule module = new(_executorProvider, path);
        if (module.IsValidGitWorkingDir())
        {
            GitModuleChanged?.Invoke(this, new GitModuleEventArgs(module));
            return;
        }

        _invalidRepositoryRemover.ShowDeleteInvalidRepositoryDialog(path);
    }

    private void OpenRepo(string repoPath)
    {
        if (Control.ModifierKeys != Keys.Control)
        {
            ChangeWorkingDir(repoPath);
            return;
        }

        GitUICommands.LaunchBrowse(repoPath);
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor
    {
        private readonly RepositoryHistoryUIService _service;

        public TestAccessor(RepositoryHistoryUIService service)
        {
            _service = service;
        }

        internal void AddRecentRepositories(ToolStripDropDownItem menuItemContainer, Repository repo, string? caption, int number)
            => _service.AddRecentRepositories(menuItemContainer, repo, caption, number);

        internal void UpdateBranchNames(IReadOnlyList<string> paths)
            => _service.UpdateBranchNamesCache(paths);

        internal void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container, in IList<Repository> repositoryHistory)
            => _service.PopulateFavouriteRepositoriesMenu(container, repositoryHistory);
    }
}
