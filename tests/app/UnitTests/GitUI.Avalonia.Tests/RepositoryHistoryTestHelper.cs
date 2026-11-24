using System.Reflection;
using System.Runtime.CompilerServices;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using NSubstitute;

namespace GitExtensionsTests;

// parity-scaffolding: supplies isolated consumer data without adding inspection or projection APIs to the port.
internal static class RepositoryHistoryTestHelper
{
    private static readonly ConditionalWeakTable<IRepositoryHistoryUIService, SnapshotInput> Snapshots = new();
    private static readonly MethodInfo AddRecentMethod = typeof(RepositoryHistoryUIService).GetMethod(
        "AddRecentRepositories", BindingFlags.Instance | BindingFlags.NonPublic)!;

    public static void SetSnapshot(IRepositoryHistoryUIService service, RepositoryHistorySnapshot snapshot)
        => Snapshots.GetValue(service, _ => new SnapshotInput()).Value = snapshot;

    public static RepositoryHistorySnapshot GetSnapshot(IRepositoryHistoryUIService service)
        => Snapshots.GetValue(service, _ => new SnapshotInput()).Value;

    public static IRepositoryHistoryUIService CreateEmptyService()
        => new EmptyRepositoryHistoryService();

    // parity-scaffolding: isolated popup consumers supply value fixtures; real Browse tests use the public saved-history producer.
    public static void PopulateFavouriteFixture(RepositoryHistoryUIService service,
        GitUI.Compat.WinFormsControls.ToolStripDropDownItem container, IReadOnlyList<RepositoryHistoryEntry> entries)
    {
        foreach (IGrouping<string?, RepositoryHistoryEntry> group in entries.GroupBy(entry => entry.Repository.Category).OrderBy(group => group.Key))
        {
            GitUI.Compat.WinFormsControls.ToolStripMenuItem category = container.CreateMenuItem();
            category.Header = GitUI.Compat.AvaloniaTranslationUtils.ToAvaloniaMnemonics(group.Key ?? string.Empty);
            container.Items.Add(category);
            int number = 0;
            foreach (RepositoryHistoryEntry entry in group)
            {
                service.GetTestAccessor().AddRecentRepositories(category, entry.Repository, entry.Caption, ++number);
            }
        }
    }

    public static RepositoryHistorySnapshot CreateSnapshot(IList<Repository> recent, IList<Repository> favourites,
        Func<string, string?>? getBranchName = null)
    {
        IReadOnlyList<RepositoryHistoryEntry> recentEntries = Split(recent, isFavourite: false, out int topCount, getBranchName);
        return new RepositoryHistorySnapshot(
            recentEntries,
            Split(favourites, isFavourite: true, out _, getBranchName))
        {
            TopCount = topCount,
        };
    }

    public static void PopulateRecentFixture(RepositoryHistoryUIService service,
        GitUI.Compat.WinFormsControls.ToolStripDropDownItem container, RepositoryHistorySnapshot snapshot)
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

            AddRecentMethod.Invoke(service, [
                container,
                repository.Repository,
                repository.Caption,
                ++number,
                repository.IsAnchored]);
        }
    }

    public static IUserRepositoriesListController CreateEmptyController()
    {
        IUserRepositoriesListController controller = Substitute.For<IUserRepositoriesListController>();
        controller.PreRenderRepositories(Arg.Any<string>()).Returns((
            Array.Empty<RecentRepoInfo>(), Array.Empty<RecentRepoInfo>()));
        return controller;
    }

    private static IReadOnlyList<RepositoryHistoryEntry> Split(IList<Repository> repositories, bool isFavourite, out int topCount,
        Func<string, string?>? getBranchName)
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
                getBranchName?.Invoke(info.Repo.Path),
                isFavourite,
                info.Anchored)),
        ];
    }

    private sealed class SnapshotInput
    {
        public RepositoryHistorySnapshot Value { get; set; } = new([], []);
    }

    private sealed class EmptyRepositoryHistoryService : IRepositoryHistoryUIService
    {
        public event EventHandler<GitModuleEventArgs>? GitModuleChanged
        {
            add { }
            remove { }
        }

        public event EventHandler? HistoryChanged;

        public void PopulateFavouriteRepositoriesMenu(GitUI.Compat.WinFormsControls.ToolStripDropDownItem container)
            => container.Items.Clear();

        public void PopulateRecentRepositoriesMenu(GitUI.Compat.WinFormsControls.ToolStripDropDownItem container)
            => container.Items.Clear();

        public void Invalidate() => HistoryChanged?.Invoke(this, EventArgs.Empty);

        public void TriggerBranchNameCacheUpdate(bool onlyIfEmpty = false)
        {
        }
    }
}
