using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using Current = GitCommands.UserRepositoryHistory;

namespace GitCommands.UserRepositoryHistory.Legacy
{
    /// <summary>
    /// Provides the ability to migrate the collections of categorised user's git repositories
    /// stored in the legacy format (pre v3) to the new structure.
    /// </summary>
    public interface IRepositoryHistoryMigrator
    {
        /// <summary>
        /// Migrates settings from the old legacy format into the new structure.
        /// </summary>
        /// <param name="currentHistory">The current list of favourite local git repositories.</param>
        /// <returns>
        /// The list of favourite local git repositories enriched with the legacy categorised git repositories.
        /// <c>changed</c> is <see langword="true"/>, if the migration has taken place; otherwise <see langword="false"/>.
        /// </returns>
        Task<(IList<Current.Repository> history, bool changed)> MigrateAsync(IEnumerable<Current.Repository> currentHistory);
    }

    /// <summary>
    /// Migrate the collections of categorised user's git repositories
    /// stored in the legacy format (pre v3) to the new structure.
    /// </summary>
    public sealed class RepositoryHistoryMigrator : IRepositoryHistoryMigrator
    {
        private readonly IRepositoryStorage _repositoryStorage;

        public RepositoryHistoryMigrator(IRepositoryStorage repositoryStorage)
        {
            _repositoryStorage = repositoryStorage;
        }

        public RepositoryHistoryMigrator()
            : this(new RepositoryStorage())
        {
        }

        /// <summary>
        /// Migrates settings from the old legacy format into the new structure.
        /// </summary>
        /// <param name="currentHistory">The current list of favourite local git repositories.</param>
        /// <returns>
        /// The list of favourite local git repositories enriched with the legacy categorised git repositories.
        /// <c>changed</c> is <see langword="true"/>, if the migration has taken place; otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="currentHistory"/> is <see langword="null"/>.</exception>
        [ContractAnnotation("currentHistory:null=>halt")]
        public async Task<(IList<Current.Repository> history, bool changed)> MigrateAsync(IEnumerable<Current.Repository> currentHistory)
        {
            if (currentHistory == null)
            {
                throw new ArgumentNullException(nameof(currentHistory));
            }

            var history = currentHistory.ToList();

            await TaskScheduler.Default;
            var categorised = _repositoryStorage.Load();
            if (categorised?.Count < 1)
            {
                return (history, false);
            }

            var changed = MigrateSettings(history, categorised);

            // settings have been migrated, clear the old setting
            _repositoryStorage.Save();

            return (history, changed);
        }

        private static bool MigrateSettings(IList<Current.Repository> history, IEnumerable<RepositoryCategory> categorised)
        {
            var changed = false;

            foreach (var category in categorised.Where(c => c.CategoryType == "Repositories"))
            {
                foreach (var repository in category.Repositories)
                {
                    var repo = history.FirstOrDefault(hr => hr.Path == repository.Path);
                    if (repo == null)
                    {
                        repo = new Current.Repository(repository.Path);
                        history.Add(repo);
                    }

                    repo.Anchor = GetAnchor(repository.Anchor);
                    repo.Category = category.Description;

                    changed = true;
                }
            }

            return changed;

            Current.Repository.RepositoryAnchor GetAnchor(string anchor)
            {
                if (!Enum.TryParse<Current.Repository.RepositoryAnchor>(anchor, out var repositoryAnchor))
                {
                    return Current.Repository.RepositoryAnchor.None;
                }

                return repositoryAnchor;
            }
        }
    }
}