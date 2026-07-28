using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitUI.CommandsDialogs;

namespace GitUI;

public sealed record RepositoryHistoryEntry(
    Repository Repository,
    string Caption,
    string? BranchName,
    bool IsFavourite,
    bool IsAnchored);

public sealed record RepositoryHistorySnapshot(
    IReadOnlyList<RepositoryHistoryEntry> Recent,
    IReadOnlyList<RepositoryHistoryEntry> Favourites);

public interface IRepositoryHistoryUIService
{
    event EventHandler? HistoryChanged;

    RepositoryHistorySnapshot LoadSnapshot();
    IList<Repository> AddAsMostRecent(string path);
    bool CanOpenRepository(string path);
    void Invalidate();
    void TriggerBranchNameCacheUpdate(bool onlyIfEmpty = false);
}

internal sealed class RepositoryHistoryUIService(
    IRepositoryCurrentBranchNameCache branchNameCache,
    IInvalidRepositoryRemover invalidRepositoryRemover) : IRepositoryHistoryUIService
{
    private readonly CancellationTokenSequence _branchCacheSequence = new();
    private RepositoryHistorySnapshot? _snapshot;

    public event EventHandler? HistoryChanged;

    public RepositoryHistorySnapshot LoadSnapshot()
    {
        if (_snapshot is not null)
        {
            return _snapshot;
        }

        IList<Repository> recent = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Locals.LoadRecentHistoryAsync);
        IList<Repository> favourites = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync);
        _snapshot = new RepositoryHistorySnapshot(
            Split(recent, isFavourite: false),
            Split(favourites, isFavourite: true));
        return _snapshot;
    }

    public IList<Repository> AddAsMostRecent(string path)
    {
        IList<Repository> repositories = ThreadHelper.JoinableTaskFactory.Run(
            () => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(path));
        Invalidate();
        return repositories;
    }

    public bool CanOpenRepository(string path)
    {
        if (GitModule.IsValidGitWorkingDir(path))
        {
            return true;
        }

        if (invalidRepositoryRemover.ShowDeleteInvalidRepositoryDialog(path))
        {
            Invalidate();
        }

        return false;
    }

    public void Invalidate()
    {
        _branchCacheSequence.CancelCurrent();
        _snapshot = null;
        branchNameCache.InvalidateAll();
        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public void TriggerBranchNameCacheUpdate(bool onlyIfEmpty = false)
    {
        if (onlyIfEmpty && !branchNameCache.IsEmpty)
        {
            return;
        }

        CancellationToken cancellationToken = _branchCacheSequence.Next();
        ThreadHelper.FileAndForget(async () =>
        {
            try
            {
                RepositoryHistorySnapshot snapshot = LoadSnapshot();
                string[] paths =
                [
                    .. snapshot.Recent.Concat(snapshot.Favourites)
                        .Select(entry => entry.Repository.Path)
                        .Distinct(GetPathComparer()),
                ];
                await Task.Run(
                    () => paths.AsParallel()
                        .WithCancellation(cancellationToken)
                        .WithDegreeOfParallelism(Math.Min(4, Math.Max(1, Environment.ProcessorCount / 2)))
                        .ForAll(path => branchNameCache.GetUpdatedBranchName(path)),
                    cancellationToken);
                _snapshot = null;
                HistoryChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // A newer repository-history request owns the cache now.
            }
        });
    }

    private IReadOnlyList<RepositoryHistoryEntry> Split(IList<Repository> repositories, bool isFavourite)
    {
        List<RecentRepoInfo> top = [];
        List<RecentRepoInfo> recent = [];
        RecentRepoSplitter splitter = new()
        {
            MeasureFont = AppSettings.Font,
        };
        splitter.SplitRecentRepos(repositories, top, recent);
        return
        [
            .. top.Concat(recent).Select(info => new RepositoryHistoryEntry(
                info.Repo,
                info.Caption ?? info.Repo.Path,
                branchNameCache.GetCachedBranchName(info.Repo.Path),
                isFavourite,
                info.Anchored)),
        ];
    }

    private static StringComparer GetPathComparer()
        => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
}
