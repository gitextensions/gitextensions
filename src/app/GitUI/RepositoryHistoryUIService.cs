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
    ///  Mark the branch name cache as requiring an update.
    /// </summary>
    void MarkBranchNameCacheForUpdate();

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
    ///  If the branch name cache is marked for update, start the background update.
    /// </summary>
    void TriggerBranchNameCacheUpdateIfNeeded();
}

internal class RepositoryHistoryUIService : IRepositoryHistoryUIService
{
    private readonly IGitExecutorProvider _executorProvider;
    private readonly IRepositoryCurrentBranchNameCache _branchNameCache;
    private readonly IInvalidRepositoryRemover _invalidRepositoryRemover;
    private readonly CancellationTokenSequence _branchCacheSequence = new();
    private WeakReference<ToolStripDropDownItem>? _recentMenuContainer;

    // history can be loaded both by trigger and menu opening
    private volatile AsyncLazy<(IList<Repository> Recent, IList<Repository> Favourite)>? _historyLazy;
    private AsyncLazy<(IList<Repository> Recent, IList<Repository> Favourite)> HistoryLazy
        => _historyLazy ??= CreateHistoryLazy();
    private static bool _triggerBranchNameCacheUpdate = true;

    public event EventHandler<GitModuleEventArgs> GitModuleChanged;

    internal RepositoryHistoryUIService(IGitExecutorProvider executorProvider, IRepositoryCurrentBranchNameCache branchNameCache, IInvalidRepositoryRemover invalidRepositoryRemover)
    {
        _executorProvider = executorProvider;
        _branchNameCache = branchNameCache;
        _invalidRepositoryRemover = invalidRepositoryRemover;
    }

    public void MarkBranchNameCacheForUpdate()
       => _triggerBranchNameCacheUpdate = true;

    public void PopulateRecentRepositoriesMenu(ToolStripDropDownItem container)
    {
        // Do not rebuild while the dropdown is open — Clear() would close it.
        // The next open will pick up fresh data via HistoryLazy.
        if (container.DropDown.Visible)
        {
            return;
        }

        _recentMenuContainer = new WeakReference<ToolStripDropDownItem>(container);

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

    public void TriggerBranchNameCacheUpdateIfNeeded()
    {
        if (!_triggerBranchNameCacheUpdate)
        {
            return;
        }

        _triggerBranchNameCacheUpdate = false;
        ThreadHelper.FileAndForget(UpdateBranchNameCacheAsync);
    }

    public async Task UpdateBranchNameCacheAsync()
    {
        CancellationToken cancellationToken = _branchCacheSequence.Next();
        (IList<Repository> recentHistory, IList<Repository> favouriteHistory) = await HistoryLazy.GetValueAsync(cancellationToken);

        string[] paths = [.. recentHistory
                .Concat(favouriteHistory)
                .Select(r => r.Path)
                .Distinct(StringComparer.InvariantCulture)];

        if (paths.Length > 0)
        {
            // Capture the parent form on the UI thread before handing off to the background thread.
            Form? parentForm = Form.ActiveForm;
            UpdateBranchNamesCache(paths, parentForm, cancellationToken);
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

    private void RefreshBranchNamesInMenu(ToolStripDropDownItem container)
    {
        ToolStripDropDown dropDown = container.DropDown;
        bool layoutSuspended = false;

        foreach (ToolStripItem dropDownItem in dropDown.Items)
        {
            if (dropDownItem is ToolStripMenuItem menuItem
                && menuItem.Tag is string path
                && _branchNameCache.GetCachedBranchName(path) is string branchName
                && menuItem.ShortcutKeyDisplayString != branchName)
            {
                if (!layoutSuspended)
                {
                    dropDown.SuspendLayout();
                    layoutSuspended = true;
                }

                menuItem.ShortcutKeyDisplayString = branchName;
            }
        }

        if (layoutSuspended)
        {
            dropDown.ResumeLayout(false);
        }
    }

    private void UpdateBranchNamesCache(IReadOnlyList<string> paths, Form? parentForm, CancellationToken cancellationToken)
    {
        _branchNameCache.InvalidateAll();
        const int MaxBranchNameFetchParallelism = 4;
        paths
            .AsParallel()
            .WithCancellation(cancellationToken)
            .WithDegreeOfParallelism(Math.Min(MaxBranchNameFetchParallelism, Math.Max(1, Environment.ProcessorCount / 2)))
            .ForAll(path =>
            {
                _branchNameCache.GetCurrentBranchName(path);
            });

        // After all paths are fetched, push one update to the UI thread if the menu container is still alive.
        if (parentForm is not null
            && parentForm.IsHandleCreated
            && _recentMenuContainer?.TryGetTarget(out ToolStripDropDownItem? menu) is true)
        {
            parentForm.BeginInvoke(() => RefreshBranchNamesInMenu(menu));
        }

        void RefreshBranchNamesInMenu(ToolStripDropDownItem container)
        {
            ToolStripDropDown dropDown = container.DropDown;
            bool layoutSuspended = false;

            foreach (ToolStripItem dropDownItem in dropDown.Items)
            {
                if (dropDownItem is ToolStripMenuItem menuItem
                    && menuItem.Tag is string path
                    && _branchNameCache.GetCachedBranchName(path) is string branchName
                    && menuItem.ShortcutKeyDisplayString != branchName)
                {
                    if (!layoutSuspended)
                    {
                        dropDown.SuspendLayout();
                        layoutSuspended = true;
                    }

                    menuItem.ShortcutKeyDisplayString = branchName;
                }
            }

            if (layoutSuspended)
            {
                dropDown.ResumeLayout(false);
            }
        }
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor(RepositoryHistoryUIService service)
    {
        internal void AddRecentRepositories(ToolStripDropDownItem menuItemContainer, Repository repo, string? caption, int number)
            => service.AddRecentRepositories(menuItemContainer, repo, caption, number);

        internal void UpdateBranchNames(IReadOnlyList<string> paths)
            => service.UpdateBranchNamesCache(paths, parentForm: null, CancellationToken.None);

        internal void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container, in IList<Repository> repositoryHistory)
            => service.PopulateFavouriteRepositoriesMenu(container, repositoryHistory);
    }
}
