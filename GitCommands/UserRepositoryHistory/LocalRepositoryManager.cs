using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace GitCommands.UserRepositoryHistory
{
    /// <summary>
    /// Manages the history of local git repositories.
    /// </summary>
    public sealed class LocalRepositoryManager : IRepositoryManager
    {
        private const string KeyRecentHistory = "history";
        private readonly IRepositoryStorage _repositoryStorage;

        public LocalRepositoryManager(IRepositoryStorage repositoryStorage)
        {
            _repositoryStorage = repositoryStorage;
        }

        /// <summary>
        /// <para>Saves the provided repository path to history of local git repositories as the "most recent".</para>
        /// <para>If the history contains an entry for the provided path, the entry is physically moved
        /// to the top of the history list.</para>
        /// </summary>
        /// <remarks>
        /// The history is loaded from the persistent storage to ensure the most current version of
        /// the history is updated, as it may have been updated by another instance of GE.
        /// </remarks>
        /// <param name="repositoryPath">A repository path to be save as "most recent".</param>
        /// <returns>The current version of the history of local git repositories after the update.</returns>
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
                var repositoryHistory = await LoadHistoryAsync();

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

                await SaveHistoryAsync(repositoryHistory);

                return repositoryHistory;
            }
        }

        /// <summary>
        /// Loads the history of local git repositories from a persistent storage.
        /// </summary>
        /// <returns>The history of local git repositories.</returns>
        public async Task<IList<Repository>> LoadHistoryAsync()
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
        /// Removes <paramref name="repositoryPath"/> from the history of local git repositories in a persistent storage.
        /// </summary>
        /// <param name="repositoryPath">A repository path to remove.</param>
        /// <returns>The current version of the history of local git repositories after the update.</returns>
        /// <exception cref="ArgumentException"><paramref name="repositoryPath"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        [ContractAnnotation("repositoryPath:null=>halt")]
        public async Task<IList<Repository>> RemoveFromHistoryAsync(string repositoryPath)
        {
            if (string.IsNullOrWhiteSpace(repositoryPath))
            {
                throw new ArgumentException(nameof(repositoryPath));
            }

            await TaskScheduler.Default;
            var repositoryHistory = await LoadHistoryAsync();
            var repository = repositoryHistory.FirstOrDefault(r => r.Path.Equals(repositoryPath, StringComparison.CurrentCultureIgnoreCase));
            if (repository == null)
            {
                return repositoryHistory;
            }

            if (!repositoryHistory.Remove(repository))
            {
                return repositoryHistory;
            }

            await SaveHistoryAsync(repositoryHistory);
            return repositoryHistory;
        }

        /// <summary>
        /// Loads the history of local git repositories to a persistent storage.
        /// </summary>
        /// <param name="repositoryHistory">A collection of local git repositories.</param>
        /// <returns>An awaitable task.</returns>
        /// <remarks>The size of the history will be adjusted as per <see cref="AppSettings.RecentRepositoriesHistorySize"/> setting.</remarks>
        public async Task SaveHistoryAsync(IEnumerable<Repository> repositoryHistory)
        {
            await TaskScheduler.Default;
            _repositoryStorage.Save(KeyRecentHistory, AdjustHistorySize(repositoryHistory, AppSettings.RecentRepositoriesHistorySize));
        }

        private static IEnumerable<Repository> AdjustHistorySize(IEnumerable<Repository> repositories, int recentRepositoriesHistorySize)
        {
            return repositories.Take(recentRepositoriesHistorySize);
        }
    }
}