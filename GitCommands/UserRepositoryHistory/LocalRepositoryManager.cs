using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitCommands.UserRepositoryHistory.Legacy;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace GitCommands.UserRepositoryHistory
{
    public interface ILocalRepositoryManager : IRepositoryManager
    {
        /// <summary>
        /// Categorises the given <paramref name="repository"/> and assigns to the list of favourites,
        /// if the <paramref name="category"/> is supplied; otherwise removes the repository from
        /// the list of favourites.
        /// </summary>
        /// <param name="repository">The repository to categorise.</param>
        /// <param name="category">The new category, if it is supplied.</param>
        /// <returns>The current version of the list of favourite git repositories after the update.</returns>
        [ContractAnnotation("repository:null=>halt")]
        Task<IList<Repository>> AssignCategoryAsync(Repository repository, string category);

        /// <summary>
        /// Loads the list of favourite local git repositories from a persistent storage.
        /// </summary>
        /// <returns>The list of favourite local git repositories.</returns>
        Task<IList<Repository>> LoadFavouriteHistoryAsync();

        /// <summary>
        /// Removes <paramref name="repositoryPath"/> from the the list of favourite local git repositories in a persistent storage.
        /// </summary>
        /// <param name="repositoryPath">A repository path to remove.</param>
        /// <returns>The current version of the list of favourite git repositories after the update.</returns>
        [ContractAnnotation("repositoryPath:null=>halt")]
        Task<IList<Repository>> RemoveFavouriteAsync(string repositoryPath);

        /// <summary>
        /// Saves the list of favourite local git repositories to a persistent storage.
        /// </summary>
        /// <param name="repositoryHistory">A list of favourite git repositories.</param>
        /// <returns>An awaitable task.</returns>
        [ContractAnnotation("repositoryHistory:null=>halt")]
        Task SaveFavouriteHistoryAsync(IEnumerable<Repository> repositoryHistory);

        /// <summary>
        /// Removes the invalid repositories from both recent and favourite local git repositories in a persistent storage.
        /// </summary>
        /// <param name="predicate">A predicate to check against for the validity of the repositories.</param>
        /// <returns>An awaitable task.</returns>
        [ContractAnnotation("predicate:null=>halt")]
        Task RemoveInvalidRepositoriesAsync(Func<string, bool> predicate);
    }

    /// <summary>
    /// Manages the history of local git repositories.
    /// </summary>
    public sealed class LocalRepositoryManager : ILocalRepositoryManager
    {
        //// configuration key under which user's recent local git repository history is persisted
        private const string KeyRecentHistory = "history";
        //// configuration key under which user's favourite local git repository history is persisted
        private const string KeyFavouriteHistory = "history-favourite";
        private readonly IRepositoryStorage _repositoryStorage;
        private readonly IRepositoryHistoryMigrator _repositoryHistoryMigrator;

        public LocalRepositoryManager(IRepositoryStorage repositoryStorage, IRepositoryHistoryMigrator repositoryHistoryMigrator)
        {
            _repositoryStorage = repositoryStorage;
            _repositoryHistoryMigrator = repositoryHistoryMigrator;
        }

        /// <summary>
        /// <para>Saves the provided repository path to the list of recently used git repositories as the top entry.</para>
        /// <para>If the history contains an entry for the provided path, the entry is physically moved
        /// to the top of the history list.</para>
        /// </summary>
        /// <remarks>
        /// The history is loaded from the persistent storage to ensure the most current version of
        /// the history is updated, as it may have been updated by another instance of GE.
        /// </remarks>
        /// <param name="repositoryPath">A repository path to be save as "most recent".</param>
        /// <returns>The current version of the list of recently used git repositories after the update.</returns>
        /// <exception cref="ArgumentException"><paramref name="repositoryPath"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        /// <exception cref="NotSupportedException"><paramref name="repositoryPath"/> is a URL.</exception>
        [ContractAnnotation("repositoryPath:null=>halt")]
        public async Task<IList<Repository>> AddAsMostRecentAsync(string repositoryPath)
        {
            if (string.IsNullOrWhiteSpace(repositoryPath))
            {
                throw new ArgumentException(nameof(repositoryPath));
            }

            if (PathUtil.IsUrl(repositoryPath))
            {
                // TODO: throw a specific exception
                throw new NotSupportedException();
            }

            repositoryPath = repositoryPath.Trim()
                                           .ToNativePath()
                                           .EnsureTrailingPathSeparator();

            // awaiting instead of passing through as per review comment
            // https://github.com/gitextensions/gitextensions/pull/4766/files/03bae27e850fb70450a0dcc3390ea16666ec2983#r179945808
            return await AddAsMostRecentRepositoryAsync(repositoryPath);

            async Task<IList<Repository>> AddAsMostRecentRepositoryAsync(string path)
            {
                await TaskScheduler.Default;
                var repositoryHistory = await LoadRecentHistoryAsync();

                var repository = repositoryHistory.FirstOrDefault(r => r.Path.Equals(path, StringComparison.CurrentCultureIgnoreCase));
                if (repository != null)
                {
                    if (repositoryHistory[0] == repository)
                    {
                        return repositoryHistory;
                    }

                    repositoryHistory.Remove(repository);
                }
                else
                {
                    repository = new Repository(path);
                }

                repositoryHistory.Insert(0, repository);

                await SaveRecentHistoryAsync(repositoryHistory);

                return repositoryHistory;
            }
        }

        /// <summary>
        /// Categorises the given <paramref name="repository"/> and assigns to the list of favourites,
        /// if the <paramref name="category"/> is supplied; otherwise removes the repository from
        /// the list of favourites.
        /// </summary>
        /// <param name="repository">The repository to categorise.</param>
        /// <param name="category">The new category, if it is supplied.</param>
        /// <returns>The current version of the list of favourite git repositories after the update.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
        [ContractAnnotation("repository:null=>halt")]
        public async Task<IList<Repository>> AssignCategoryAsync(Repository repository, string category)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            await TaskScheduler.Default;

            var favourites = await LoadFavouriteHistoryAsync();
            var favourite = favourites.FirstOrDefault(f => string.Equals(f.Path, repository.Path, StringComparison.OrdinalIgnoreCase));

            if (favourite == null)
            {
                if (!string.IsNullOrWhiteSpace(category))
                {
                    repository.Category = category;
                    favourites.Add(repository);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    favourites.Remove(favourite);
                }
                else
                {
                    favourite.Category = category;
                }
            }

            await SaveFavouriteHistoryAsync(favourites);

            return favourites;
        }

        /// <summary>
        /// Loads the list of favourite local git repositories from a persistent storage.
        /// </summary>
        /// <returns>The list of favourite local git repositories.</returns>
        public async Task<IList<Repository>> LoadFavouriteHistoryAsync()
        {
            await TaskScheduler.Default;

            var history = _repositoryStorage.Load(KeyFavouriteHistory) ?? Array.Empty<Repository>();

            // backwards compatibility - port the existing user's categorised repositories
            var (migrated, changed) = await _repositoryHistoryMigrator.MigrateAsync(history);
            if (changed)
            {
                _repositoryStorage.Save(KeyFavouriteHistory, migrated);
            }

            return migrated ?? new List<Repository>();
        }

        /// <summary>
        /// Loads the list of recently used local git repositories from a persistent storage.
        /// </summary>
        /// <returns>The list of recently used local git repositories.</returns>
        public async Task<IList<Repository>> LoadRecentHistoryAsync()
        {
            await TaskScheduler.Default;

            int size = AppSettings.RecentRepositoriesHistorySize;

            var history = _repositoryStorage.Load(KeyRecentHistory);
            if (history == null)
            {
                return Array.Empty<Repository>();
            }

            return AdjustHistorySize(history, size).ToList();
        }

        /// <summary>
        /// Removes <paramref name="repositoryPath"/> from the the list of favourite local git repositories in a persistent storage.
        /// </summary>
        /// <param name="repositoryPath">A repository path to remove.</param>
        /// <returns>The current version of the list of favourite git repositories after the update.</returns>
        /// <exception cref="ArgumentException"><paramref name="repositoryPath"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        [ContractAnnotation("repositoryPath:null=>halt")]
        public async Task<IList<Repository>> RemoveFavouriteAsync(string repositoryPath)
        {
            if (string.IsNullOrWhiteSpace(repositoryPath))
            {
                throw new ArgumentException(nameof(repositoryPath));
            }

            await TaskScheduler.Default;
            var repositoryHistory = await LoadFavouriteHistoryAsync();
            var repository = repositoryHistory.FirstOrDefault(r => r.Path.Equals(repositoryPath, StringComparison.CurrentCultureIgnoreCase));
            if (repository == null)
            {
                return repositoryHistory;
            }

            if (!repositoryHistory.Remove(repository))
            {
                return repositoryHistory;
            }

            await SaveFavouriteHistoryAsync(repositoryHistory);
            return repositoryHistory;
        }

        /// <summary>
        /// Removes <paramref name="repositoryPath"/> from the the list of recently used local git repositories in a persistent storage.
        /// </summary>
        /// <param name="repositoryPath">A repository path to remove.</param>
        /// <returns>The current version of the list of recently used git repositories after the update.</returns>
        /// <exception cref="ArgumentException"><paramref name="repositoryPath"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        [ContractAnnotation("repositoryPath:null=>halt")]
        public async Task<IList<Repository>> RemoveRecentAsync(string repositoryPath)
        {
            if (string.IsNullOrWhiteSpace(repositoryPath))
            {
                throw new ArgumentException(nameof(repositoryPath));
            }

            await TaskScheduler.Default;
            var repositoryHistory = await LoadRecentHistoryAsync();
            var repository = repositoryHistory.FirstOrDefault(r => r.Path.Equals(repositoryPath, StringComparison.CurrentCultureIgnoreCase));
            if (repository == null)
            {
                return repositoryHistory;
            }

            if (!repositoryHistory.Remove(repository))
            {
                return repositoryHistory;
            }

            await SaveRecentHistoryAsync(repositoryHistory);
            return repositoryHistory;
        }

        /// <summary>
        /// Saves the list of favourite local git repositories to a persistent storage.
        /// </summary>
        /// <param name="repositoryHistory">A list of favourite git repositories.</param>
        /// <returns>An awaitable task.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="repositoryHistory"/> is <see langword="null"/>.</exception>
        [ContractAnnotation("repositoryHistory:null=>halt")]
        public async Task SaveFavouriteHistoryAsync(IEnumerable<Repository> repositoryHistory)
        {
            if (repositoryHistory == null)
            {
                throw new ArgumentNullException(nameof(repositoryHistory));
            }

            await TaskScheduler.Default;
            _repositoryStorage.Save(KeyFavouriteHistory, repositoryHistory);
        }

        /// <summary>
        /// Saves the list of recently used local git repositories to a persistent storage.
        /// </summary>
        /// <param name="repositoryHistory">A list of recently used git repositories.</param>
        /// <returns>An awaitable task.</returns>
        /// <remarks>The size of the history will be adjusted as per <see cref="AppSettings.RecentRepositoriesHistorySize"/> setting.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="repositoryHistory"/> is <see langword="null"/>.</exception>
        [ContractAnnotation("repositoryHistory:null=>halt")]
        public async Task SaveRecentHistoryAsync(IEnumerable<Repository> repositoryHistory)
        {
            if (repositoryHistory == null)
            {
                throw new ArgumentNullException(nameof(repositoryHistory));
            }

            await TaskScheduler.Default;
            _repositoryStorage.Save(KeyRecentHistory, AdjustHistorySize(repositoryHistory, AppSettings.RecentRepositoriesHistorySize));
        }

        private static IEnumerable<Repository> AdjustHistorySize(IEnumerable<Repository> repositories, int recentRepositoriesHistorySize)
        {
            return repositories.Take(recentRepositoriesHistorySize);
        }

        public async Task RemoveInvalidRepositoriesAsync(Func<string, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            await TaskScheduler.Default;

            var recentRepositoryHistory = await LoadRecentHistoryAsync();
            var existingRecentCount = recentRepositoryHistory.Count;
            var invalidRecentRepositories = recentRepositoryHistory
                                            .Where(repo => !predicate(repo.Path))
                                            .ToList();

            foreach (var repo in invalidRecentRepositories)
            {
                recentRepositoryHistory.Remove(repo);
            }

            if (existingRecentCount != recentRepositoryHistory.Count)
            {
                await SaveRecentHistoryAsync(recentRepositoryHistory);
            }

            var favouriteRepositoryHistory = await LoadFavouriteHistoryAsync();
            var existingFavouriteCount = favouriteRepositoryHistory.Count;
            var invalidFavouriteRepositories = favouriteRepositoryHistory
                                                .Where(repo => !predicate(repo.Path))
                                                .ToList();

            foreach (var repo in invalidFavouriteRepositories)
            {
                favouriteRepositoryHistory.Remove(repo);
            }

            if (existingFavouriteCount != favouriteRepositoryHistory.Count)
            {
                await SaveFavouriteHistoryAsync(favouriteRepositoryHistory);
            }
        }
    }
}