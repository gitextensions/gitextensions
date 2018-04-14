using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace GitCommands.UserRepositoryHistory
{
    public static class RepositoryManager
    {
        private const string KeyRecentHistory = "history";
        private const string KeyRemoteHistory = "history remote";

        private static readonly IRepositoryStorage RepositoryStorage = new RepositoryStorage();

        public static async Task<RepositoryHistory> LoadLocalHistoryAsync()
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

        public static Task RemoveFromHistoryAsync(Repository repository)
        {
            // TODO:
            return Task.CompletedTask;
        }

        public static async Task<RepositoryHistory> AddAsMostRecentLocalHistoryAsync(string repositoryPath)
        {
            if (PathUtil.IsUrl(repositoryPath))
            {
                // TODO: throw a specific exception
                throw new NotSupportedException();
            }

            repositoryPath = repositoryPath.ToNativePath().EnsureTrailingPathSeparator();
            return await AddAsMostRecentRepositoryAsync(repositoryPath, LoadLocalHistoryAsync, SaveLocalHistoryAsync);
        }

        public static async Task<RepositoryHistory> AddAsMostRecentRemoteHistoryAsync(string repositoryPath)
        {
            if (!PathUtil.IsUrl(repositoryPath))
            {
                // TODO: throw a specific exception
                throw new NotSupportedException();
            }

            return await AddAsMostRecentRepositoryAsync(repositoryPath, LoadRemoteHistoryAsync, SaveRemoteHistoryAsync);
        }

        private static async Task<RepositoryHistory> AddAsMostRecentRepositoryAsync(string repositoryPath, Func<Task<RepositoryHistory>> loadRepositoryHistoryAsync, Func<RepositoryHistory, Task> saveRepositoryHistoryAsync)
        {
            // load save repositories from the file to ensure we are updating the most current version
            // (the history may have been updated by another instance of GE)
            // if the given repository path is invalid then return the loaded list to the caller.
            // otherwise move the given path to the top of the list, save the changes and
            // return the loaded list to the caller.

            var repositoryHistory = await loadRepositoryHistoryAsync();

            if (string.IsNullOrWhiteSpace(repositoryPath))
            {
                return repositoryHistory;
            }

            repositoryPath = repositoryPath.Trim();
            var repository = repositoryHistory.Repositories.FirstOrDefault(r => r.Path.Equals(repositoryPath, StringComparison.CurrentCultureIgnoreCase));
            if (repository != null)
            {
                repositoryHistory.Repositories.Remove(repository);
            }
            else
            {
                repository = new Repository(repositoryPath);
            }

            repositoryHistory.Repositories.Insert(0, repository);

            await saveRepositoryHistoryAsync(repositoryHistory);

            return repositoryHistory;
        }

        public static async Task<RepositoryHistory> LoadRemoteHistoryAsync()
        {
            await TaskScheduler.Default;

            // BUG: this must be a separate settings
            // TODO: to be addressed separately
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

        public static void AdjustRepositoryHistorySize(IList<Repository> repositories, int recentRepositoriesHistorySize)
        {
            // TODO:
        }

        public static async Task SaveLocalHistoryAsync(RepositoryHistory repositoryHistory)
        {
            await TaskScheduler.Default;
            AdjustRepositoryHistorySize(repositoryHistory.Repositories, AppSettings.RecentRepositoriesHistorySize);
            RepositoryStorage.Save(KeyRecentHistory, repositoryHistory.Repositories);
        }

        public static async Task SaveRemoteHistoryAsync(RepositoryHistory repositoryHistory)
        {
            await TaskScheduler.Default;

            // BUG: this must be a separate settings
            // TODO: to be addressed separately
            AdjustRepositoryHistorySize(repositoryHistory.Repositories, AppSettings.RecentRepositoriesHistorySize);
            RepositoryStorage.Save(KeyRemoteHistory, repositoryHistory.Repositories);
        }
    }
}
