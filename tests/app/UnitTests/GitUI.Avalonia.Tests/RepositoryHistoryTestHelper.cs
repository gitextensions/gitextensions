using GitCommands.UserRepositoryHistory;
using GitUI;

namespace GitExtensionsTests;

internal static class RepositoryHistoryTestHelper
{
    public static IRepositoryHistoryUIService CreateEmptyService()
        => new EmptyRepositoryHistoryService();

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
