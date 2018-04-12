using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace GitCommands.Repository
{
    public static class RepositoryManager
    {
        private const string KeyRecentHistory = "history";
        private const string KeyRemoteHistory = "history remote";

        private static readonly IRepositoryStorage RepositoryStorage = new RepositoryStorage();

        public static async Task<RepositoryHistory> LoadRepositoryHistoryAsync()
        {
            await TaskScheduler.Default;

            int size = AppSettings.RecentRepositoriesHistorySize;
            var repositoryHistory = new RepositoryHistory(size);

            var history = RepositoryStorage.Load(KeyRecentHistory);
            if (history == null)
            {
                return repositoryHistory;
            }

            repositoryHistory.Repositories = new BindingList<Repository>(history.ToList());
            return repositoryHistory;
        }

        public static Task RemoveRepositoryHistoryAsync(Repository repository)
        {
            // TODO:
            return Task.CompletedTask;
        }

        public static void AddMostRecentRepository(string repo)
        {
            if (PathUtil.IsUrl(repo))
            {
                ////RemoteRepositoryHistory.AddMostRecentRepository(repo);
            }
            else
            {
                ////RepositoryHistory.AddMostRecentRepository(repo);
            }
        }

        public static async Task<RepositoryHistory> LoadRepositoryRemoteHistoryAsync()
        {
            await TaskScheduler.Default;

            int size = AppSettings.RecentRepositoriesHistorySize;
            var repositoryHistory = new RepositoryHistory(size);

            var history = RepositoryStorage.Load(KeyRemoteHistory);
            if (history == null)
            {
                return repositoryHistory;
            }

            repositoryHistory.Repositories = new BindingList<Repository>(history.ToList());
            return repositoryHistory;
        }

        public static void AdjustRecentHistorySize(int recentRepositoriesHistorySize)
        {
            // TODO:
        }

        public static async Task SaveRepositoryHistoryAsync(RepositoryHistory repositoryHistory)
        {
            await TaskScheduler.Default;
            RepositoryStorage.Save(KeyRecentHistory, repositoryHistory.Repositories);
        }

        public static async Task SaveRepositoryRemoteHistoryAsync(RepositoryHistory repositoryHistory)
        {
            await TaskScheduler.Default;
            RepositoryStorage.Save(KeyRemoteHistory, repositoryHistory.Repositories);
        }
    }
}
