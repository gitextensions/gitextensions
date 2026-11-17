using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.Properties;
using Microsoft.VisualStudio.Threading;
using ToolStripDropDownItem = GitUI.Compat.WinFormsControls.ToolStripDropDownItem;
using ToolStripMenuItem = GitUI.Compat.WinFormsControls.ToolStripMenuItem;
using ToolStripSeparator = GitUI.Compat.WinFormsControls.ToolStripSeparator;

namespace GitUI;

public sealed record RepositoryHistoryEntry(
    Repository Repository,
    string Caption,
    string? BranchName,
    bool IsFavourite,
    bool IsAnchored);

public sealed record RepositoryHistorySnapshot(
    IReadOnlyList<RepositoryHistoryEntry> Recent,
    IReadOnlyList<RepositoryHistoryEntry> Favourites)
{
    /// <summary>
    ///  Gets the number of recent entries supplied by the splitter's top group.
    /// </summary>
    public int TopCount { get; init; }
}

/// <summary>
///  Represents a service for managing the git repository history.
/// </summary>
public interface IRepositoryHistoryUIService
{
    /// <summary>
    ///  Occurs whenever the git module changes.
    /// </summary>
    event EventHandler<GitModuleEventArgs> GitModuleChanged;

    event EventHandler? HistoryChanged;

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

    RepositoryHistorySnapshot LoadSnapshot();
    void Invalidate();

    /// <summary>
    ///  Start updating the branch name cache.
    /// </summary>
    /// <param name="onlyIfEmpty">Start updating only if the cache is empty.</param>
    void TriggerBranchNameCacheUpdate(bool onlyIfEmpty = false);
}

internal sealed class RepositoryHistoryUIService : IRepositoryHistoryUIService
{
    private static readonly AttachedProperty<bool> OpenInNewInstanceProperty =
        AvaloniaProperty.RegisterAttached<RepositoryHistoryUIService, MenuItem, bool>("OpenInNewInstance");

    private readonly IGitExecutorProvider _executorProvider;
    private readonly IRepositoryCurrentBranchNameCache _branchNameCache;
    private readonly IInvalidRepositoryRemover _invalidRepositoryRemover;
    private readonly CancellationTokenSequence _branchCacheSequence = new();
    private JoinableTask? _branchCacheUpdateTask;
    private bool _firstLoad = true;
    private RepositoryHistorySnapshot? _snapshot;

    public event EventHandler<GitModuleEventArgs>? GitModuleChanged;
    public event EventHandler? HistoryChanged;

    internal RepositoryHistoryUIService(
        IGitExecutorProvider executorProvider,
        IRepositoryCurrentBranchNameCache branchNameCache,
        IInvalidRepositoryRemover invalidRepositoryRemover)
    {
        _executorProvider = executorProvider;
        _branchNameCache = branchNameCache;
        _invalidRepositoryRemover = invalidRepositoryRemover;
    }

    private void AddRecentRepositories(ToolStripDropDownItem menuItemContainer, Repository repo, string? caption, int number, bool anchored = false)
    {
        string numberString = number switch
        {
            < 10 => $"&{number}",
            10 => "1&0",
            _ => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
        };
        string? branchName = _branchNameCache.GetCachedBranchName(repo.Path);
        ToolStripMenuItem item = menuItemContainer.CreateMenuItem();
        item.Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics($"{numberString}: {caption}");
        item.Tag = repo;
        item.Icon = anchored
            ? new Image { Classes = { "gitextensions-icon-16" }, Source = Images.Pin }
            : null;

        WinFormsToolStripMenuSizer.SetShortcutDisplayString(item, branchName);

        if (repo.Path != caption)
        {
            ToolTip.SetTip(item, repo.Path);
        }

        // The owned native popup supplies release/Enter modifiers synchronously around Click.
        if (!item.UseSourceMnemonicRouting)
        {
            item.PointerPressed += (_, e) =>
                item.SetValue(OpenInNewInstanceProperty, e.KeyModifiers == KeyModifiers.Control);
            item.KeyDown += (_, e) =>
                item.SetValue(OpenInNewInstanceProperty, e.KeyModifiers == KeyModifiers.Control);
        }

        item.Click += (_, _) =>
        {
            bool openInNewInstance = NativeRepositoryMenu.ActivationModifiers is KeyModifiers modifiers
                ? modifiers == KeyModifiers.Control : item.GetValue(OpenInNewInstanceProperty);
            item.ClearValue(OpenInNewInstanceProperty);
            if (openInNewInstance)
            {
                GitUICommands.LaunchBrowse(repo.Path);
                return;
            }

            OpenRepo(repo.Path);
        };
        menuItemContainer.Items.Add(item);
    }

    private void ChangeWorkingDir(string path)
    {
        GitModule module = new(_executorProvider, path);
        if (module.IsValidGitWorkingDir())
        {
            GitModuleChanged?.Invoke(this, new GitModuleEventArgs(module));
            return;
        }

        if (_invalidRepositoryRemover.ShowDeleteInvalidRepositoryDialog(path))
        {
            Invalidate();
        }
    }

    private void OpenRepo(string repoPath)
    {
        ChangeWorkingDir(repoPath);
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

        container.Items.Clear();

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

        foreach (IGrouping<string?, RecentRepoInfo> repo in pinnedRepos.Union(allRecentRepos).GroupBy(k => k.Repo.Category).OrderBy(k => k.Key))
        {
            AddFavouriteRepositories(repo.Key, repo);
        }

        return;

        void AddFavouriteRepositories(string? category, IEnumerable<RecentRepoInfo> repos)
        {
            ToolStripMenuItem menuItemCategory = container.CreateMenuItem();
            menuItemCategory.Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(category ?? string.Empty);
            container.Items.Add(menuItemCategory);

            int number = 0;
            foreach (RecentRepoInfo r in repos)
            {
                AddRecentRepositories(menuItemCategory, r.Repo, r.Caption, ++number);
            }
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
        foreach (RecentRepoInfo repo in CollectionsMarshal.AsSpan(pinnedRepos))
        {
            AddRecentRepositories(container, repo.Repo, repo.Caption, ++number, repo.Anchored);
        }

        if (allRecentRepos.Count > 0)
        {
            if (pinnedRepos.Count > 0)
            {
                container.Items.Add(container.CreateSeparator());
            }

            foreach (RecentRepoInfo repo in CollectionsMarshal.AsSpan(allRecentRepos))
            {
                AddRecentRepositories(container, repo.Repo, repo.Caption, ++number, repo.Anchored);
            }
        }
    }

    private void PopulateRecentRepositoriesMenu(ToolStripDropDownItem container, RepositoryHistorySnapshot snapshot)
    {
        int number = 0;
        foreach (RepositoryHistoryEntry repository in snapshot.Recent)
        {
            // A top group can contain unanchored entries, and a recent group can
            // contain pinned entries. Only the splitter's boundary determines the separator.
            if (number > 0 && number == snapshot.TopCount)
            {
                container.Items.Add(container.CreateSeparator());
            }

            AddRecentRepositories(
                container,
                repository.Repository,
                repository.Caption,
                ++number,
                repository.IsAnchored);
        }
    }

    public RepositoryHistorySnapshot LoadSnapshot()
    {
        if (_snapshot is not null)
        {
            return _snapshot;
        }

        IList<Repository> recent = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Locals.LoadRecentHistoryAsync);
        IList<Repository> favourites = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync);
        _snapshot = CreateSnapshot(recent, favourites);
        return _snapshot;
    }

    public void Invalidate()
    {
        _branchCacheSequence.CancelCurrent();
        _snapshot = null;
        _branchNameCache.InvalidateAll();
        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public void TriggerBranchNameCacheUpdate(bool onlyIfEmpty = false)
    {
        // Race condition for OnLoad vs OnRevisionsLoaded
        // (onlyIfEmpty: true by OnLoad, false by OnRevisionsLoaded)
        bool skipUpdate;
        if (_branchNameCache.IsEmpty)
        {
            // first OnLoad or OnRevisionsLoaded, mark cache as non empty
            skipUpdate = false;
            const string invalidPath = ":::invalid:::";
            _branchNameCache.UpdateCache(invalidPath, string.Empty);
        }
        else if (_firstLoad)
        {
            // cache exists so either Dashbord filled it or 'other trigger' started
            skipUpdate = true;

            // suppress second load if OnLoad is first
            // (if OnRevisionsLoaded is first load will be done twice but the the load is very quick).
            _firstLoad = onlyIfEmpty;
        }
        else
        {
            // Following OnRevisionsLoaded
            skipUpdate = onlyIfEmpty;
        }

        if (skipUpdate)
        {
            return;
        }

        _branchCacheUpdateTask = ThreadHelper.JoinableTaskFactory.RunAsync(UpdateBranchNameCacheAsync);

        async Task UpdateBranchNameCacheAsync()
        {
            CancellationToken cancellationToken = _branchCacheSequence.Next();
            IList<Repository> recentHistory = await RepositoryHistoryManager.Locals.LoadRecentHistoryAsync();
            IList<Repository> favouriteHistory = await RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync();
            string[] paths =
            [
                .. recentHistory.Concat(favouriteHistory)
                    .Select(repository => repository.Path)
                    .Distinct(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal),
            ];

            if (paths.Length > 0)
            {
                const int MaxBranchNameFetchParallelism = 4;
                paths.AsParallel()
                    .WithCancellation(cancellationToken)
                    .WithDegreeOfParallelism(Math.Min(MaxBranchNameFetchParallelism, Math.Max(1, Environment.ProcessorCount / 2)))
                    .ForAll(path => _ = _branchNameCache.GetUpdatedBranchName(path));
            }

            _snapshot = null;
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private RepositoryHistorySnapshot CreateSnapshot(IList<Repository> recent, IList<Repository> favourites)
    {
        IReadOnlyList<RepositoryHistoryEntry> recentEntries = Split(recent, isFavourite: false, out int topCount);
        return new RepositoryHistorySnapshot(
            recentEntries,
            Split(favourites, isFavourite: true, out _))
        {
            TopCount = topCount,
        };
    }

    private IReadOnlyList<RepositoryHistoryEntry> Split(IList<Repository> repositories, bool isFavourite, out int topCount)
    {
        List<RecentRepoInfo> top = [];
        List<RecentRepoInfo> recent = [];
        RecentRepoSplitter splitter = new()
        {
            MeasureFont = AppSettings.Font,
        };
        splitter.SplitRecentRepos(repositories, top, recent);
        topCount = top.Count;

        // Source favourites deduplicate shared RecentRepoInfo instances between
        // groups. Projecting first would change Union's reference-identity semantics.
        IEnumerable<RecentRepoInfo> ordered = isFavourite ? top.Union(recent) : top.Concat(recent);
        return
        [
            .. ordered.Select(info => new RepositoryHistoryEntry(
                info.Repo,
                info.Caption ?? info.Repo.Path,
                _branchNameCache.GetCachedBranchName(info.Repo.Path),
                isFavourite,
                info.Anchored)),
        ];
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor(RepositoryHistoryUIService service)
    {
        internal void AddRecentRepositories(ToolStripDropDownItem menuItemContainer, Repository repo, string? caption, int number)
            => service.AddRecentRepositories(menuItemContainer, repo, caption, number);

        internal RepositoryHistorySnapshot CreateSnapshot(IList<Repository> recent, IList<Repository> favourites)
            => service.CreateSnapshot(recent, favourites);

        internal void PopulateRecentRepositoriesMenu(ToolStripDropDownItem container, RepositoryHistorySnapshot snapshot)
            => service.PopulateRecentRepositoriesMenu(container, snapshot);

        internal void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container, in IList<Repository> repositoryHistory)
            => service.PopulateFavouriteRepositoriesMenu(container, repositoryHistory);
    }
}
