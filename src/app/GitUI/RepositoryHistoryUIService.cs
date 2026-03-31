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
    ///  Start updating the branch name cache.
    /// </summary>
    void TriggerBranchNameCacheUpdate();
}

internal class RepositoryHistoryUIService : IRepositoryHistoryUIService
{
    private readonly IGitExecutorProvider _executorProvider;
    private readonly IRepositoryCurrentBranchNameCache _branchNameCache;
    private readonly IInvalidRepositoryRemover _invalidRepositoryRemover;
    private readonly CancellationTokenSequence _branchCacheSequence = new();
    private JoinableTask? _branchCacheUpdateTask;
    private bool _firstLoad = true;

    public event EventHandler<GitModuleEventArgs> GitModuleChanged;

    internal RepositoryHistoryUIService(IGitExecutorProvider executorProvider, IRepositoryCurrentBranchNameCache branchNameCache, IInvalidRepositoryRemover invalidRepositoryRemover)
    {
        _executorProvider = executorProvider;
        _branchNameCache = branchNameCache;
        _invalidRepositoryRemover = invalidRepositoryRemover;
    }

    private void AddRecentRepositories(ToolStripDropDownItem menuItemContainer, Repository repo, string? caption, int number, bool anchored = false)
    {
        string numberString = number switch { < 10 => $"&{number}", 10 => "1&0", _ => $"{number}" };
        ToolStripMenuItem item = new($"{numberString}: {caption}")
        {
            DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
            Tag = repo.Path,
        };

        if (anchored)
        {
            item.Image = Images.Pin;
        }

        menuItemContainer.DropDownItems.Add(item);

        item.Click += (_, _) => OpenRepo(repo.Path);

        if (repo.Path != caption)
        {
            item.ToolTipText = repo.Path;
        }

        if (_branchNameCache.GetCachedBranchName(repo.Path) is string cachedBranchName)
        {
            item.ShortcutKeyDisplayString = cachedBranchName;
        }
    }

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

    public void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container)
    {
        JoinableTask? branchCacheUpdateTask = _branchCacheUpdateTask;
        if (branchCacheUpdateTask is not null && branchCacheUpdateTask.IsCompleted)
        {
            try
            {
                branchCacheUpdateTask.Join();
            }
            catch (OperationCanceledException)
            {
                // OK
            }
        }

        container.DropDownItems.Clear();

        IList<Repository> repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(
            RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync);

        if (repositoryHistory.Count < 1)
        {
            return;
        }

        PopulateFavouriteRepositoriesMenu(container, repositoryHistory);
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

    public void PopulateRecentRepositoriesMenu(ToolStripDropDownItem container)
    {
        JoinableTask? branchCacheUpdateTask = _branchCacheUpdateTask;
        if (branchCacheUpdateTask is not null && branchCacheUpdateTask.IsCompleted)
        {
            try
            {
                branchCacheUpdateTask.Join();
            }
            catch (OperationCanceledException)
            {
                // OK
            }
        }

        List<RecentRepoInfo> pinnedRepos = [];
        List<RecentRepoInfo> allRecentRepos = [];

        IList<Repository> repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(
            RepositoryHistoryManager.Locals.LoadRecentHistoryAsync);

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

    public void TriggerBranchNameCacheUpdate()
    {
        // first time opening and cache not empty - filled from dashboard
        if (!_branchNameCache.IsEmpty && _firstLoad)
        {
            _firstLoad = false;
            return;
        }

        _firstLoad = false;
        _branchCacheUpdateTask = ThreadHelper.JoinableTaskFactory.RunAsync(UpdateBranchNameCacheAsync);
    }

    private async Task UpdateBranchNameCacheAsync()
    {
        CancellationToken cancellationToken = _branchCacheSequence.Next();
        IList<Repository> recentHistory = await RepositoryHistoryManager.Locals.LoadRecentHistoryAsync();
        IList<Repository> favouriteHistory = await RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync();

        string[] paths = [.. recentHistory
                .Concat(favouriteHistory)
                .Select(r => r.Path)
                .Distinct(StringComparer.InvariantCulture)];

        if (paths.Length > 0)
        {
            UpdateBranchNamesCache(paths, cancellationToken);
        }

        return;

        void UpdateBranchNamesCache(IReadOnlyList<string> paths, CancellationToken cancellationToken)
        {
            const int MaxBranchNameFetchParallelism = 4;
            paths
                .AsParallel()
                .WithCancellation(cancellationToken)
                .WithDegreeOfParallelism(Math.Min(MaxBranchNameFetchParallelism, Math.Max(1, Environment.ProcessorCount / 2)))
                .ForAll(path =>
                {
                    _branchNameCache.GetCurrentBranchName(path);
                });
        }
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor(RepositoryHistoryUIService service)
    {
        internal void AddRecentRepositories(ToolStripDropDownItem menuItemContainer, Repository repo, string? caption, int number)
            => service.AddRecentRepositories(menuItemContainer, repo, caption, number);

        internal void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container, in IList<Repository> repositoryHistory)
            => service.PopulateFavouriteRepositoriesMenu(container, repositoryHistory);
    }
}
