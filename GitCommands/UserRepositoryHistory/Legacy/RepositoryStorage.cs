using System;
using System.Collections.Generic;

namespace GitCommands.UserRepositoryHistory.Legacy
{
    /// <summary>
    /// Provides the ability to persist and retrieve collections of user's git repositories.
    /// </summary>
    public interface IRepositoryStorage
    {
        /// <summary>
        /// Loads a collection of categorised user's git repositories (legacy).
        /// </summary>
        /// <returns>A collection of categorised user's git repositories.</returns>
        IReadOnlyList<RepositoryCategory> Load();

        /// <summary>
        /// Removes the legacy categorised user's git repositories history
        /// after it has been migrated to the new format.
        /// </summary>
        void Save();
    }

    /// <summary>
    /// Persists and retrieves collections of user's git repositories.
    /// </summary>
    public sealed class RepositoryStorage : IRepositoryStorage
    {
        private const string KeyHistory = "repositories";
        private const string KeyHistoryBackup = "repositories-backup";
        private readonly IRepositorySerialiser<RepositoryCategory> _repositoryCategorySerialiser;

        public RepositoryStorage(IRepositorySerialiser<RepositoryCategory> repositoryCategorySerialiser)
        {
            _repositoryCategorySerialiser = repositoryCategorySerialiser;
        }

        public RepositoryStorage()
            : this(new RepositoryCategoryXmlSerialiser())
        {
        }

        /// <summary>
        /// Loads a collection of categorised user's git repositories (legacy).
        /// </summary>
        /// <returns>A collection of categorised user's git repositories.</returns>
        public IReadOnlyList<RepositoryCategory> Load()
        {
            string legacySetting = AppSettings.GetString(KeyHistory, null);
            if (legacySetting == null)
            {
                return Array.Empty<RepositoryCategory>();
            }

            // backup the original setting
            AppSettings.SetString(KeyHistoryBackup, legacySetting);

            var history = _repositoryCategorySerialiser.Deserialize(legacySetting);
            if (history == null)
            {
                return Array.Empty<RepositoryCategory>();
            }

            return history;
        }

        /// <summary>
        /// Removes the legacy collection of user's git repositories.
        /// </summary>
        public void Save()
        {
            // remove the legacy setting
            AppSettings.SetString(KeyHistory, "");
        }
    }
}