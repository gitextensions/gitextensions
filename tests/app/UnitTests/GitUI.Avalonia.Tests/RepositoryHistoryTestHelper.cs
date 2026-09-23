using GitCommands.UserRepositoryHistory;
using GitUI;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using NSubstitute;

namespace GitExtensionsTests;

internal static class RepositoryHistoryTestHelper
{
    public static IRepositoryHistoryUIService CreateEmptyService()
        => new EmptyRepositoryHistoryService();

    public static IUserRepositoriesListController CreateEmptyController()
    {
        IUserRepositoriesListController controller = Substitute.For<IUserRepositoriesListController>();
        controller.PreRenderRepositories(Arg.Any<string>()).Returns((
            Array.Empty<RecentRepoInfo>(), Array.Empty<RecentRepoInfo>()));
        return controller;
    }

    private sealed class EmptyRepositoryHistoryService : IRepositoryHistoryUIService
    {
        public event EventHandler? HistoryChanged;

        public RepositoryHistorySnapshot LoadSnapshot() => new([], []);

        public IList<Repository> AddAsMostRecent(string path) => [];

        public bool CanOpenRepository(string path) => false;

        public void Invalidate() => HistoryChanged?.Invoke(this, EventArgs.Empty);

        public void TriggerBranchNameCacheUpdate(bool onlyIfEmpty = false)
        {
        }
    }
}
