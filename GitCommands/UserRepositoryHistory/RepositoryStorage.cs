using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GitCommands.UserRepositoryHistory
{
    /// <summary>
    /// Provides the ability to persist and retrieve collections of user's git repositories.
    /// </summary>
    public interface IRepositoryStorage
    {
        /// <summary>
        /// Loads a collection of user's git repositories.
        /// </summary>
        /// <param name="key">A setting key which contains the persisted collection.</param>
        /// <returns>A collection of user's git repositories.</returns>
        IReadOnlyList<Repository> Load(string key);

        /// <summary>
        /// Persists the given collection of user's git repositories.
        /// </summary>
        /// <param name="key">A setting key which contains the persisted collection.</param>
        /// <param name="repositories">A collection of user's git repositories.</param>
        void Save(string key, IEnumerable<Repository> repositories);
    }

    /// <summary>
    /// Persists and retrieves collections of user's git repositories.
    /// </summary>
    public sealed class RepositoryStorage : IRepositoryStorage
    {
        private readonly IRepositorySerialiser<Repository> _repositorySerialiser;

        public RepositoryStorage(IRepositorySerialiser<Repository> repositorySerialiser)
        {
            _repositorySerialiser = repositorySerialiser;
        }

        public RepositoryStorage()
            : this(new RepositoryXmlSerialiser())
        {
        }

        /// <summary>
        /// Loads a collection of user's git repositories.
        /// </summary>
        /// <param name="key">A setting key which contains the persisted collection.</param>
        /// <returns>A collection of user's git repositories, if successful;
        /// otherwise an empty list, if the setting does not exist or the persisted value cannot be deserialised.</returns>
        /// <exception cref="ArgumentException"><paramref name="key"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        [ContractAnnotation("key:null=>halt")]
        public IReadOnlyList<Repository> Load([NotNull]string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(nameof(key));
            }

            string setting = AppSettings.GetString(key, null);
            if (setting == null)
            {
                return Array.Empty<Repository>();
            }

            var history = _repositorySerialiser.Deserialize(setting);
            if (history == null)
            {
                return Array.Empty<Repository>();
            }

            return history;
        }

        /// <summary>
        /// Persists the given collection of user's git repositories.
        /// </summary>
        /// <param name="key">A setting key which contains the persisted collection.</param>
        /// <param name="repositories">A collection of user's git repositories.</param>
        /// <exception cref="ArgumentException"><paramref name="key"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="repositories"/> is <see langword="null"/>.</exception>
        [ContractAnnotation("key:null=>halt;repositories:null=>halt")]
        public void Save([NotNull]string key, [NotNull]IEnumerable<Repository> repositories)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(nameof(key));
            }

            if (repositories == null)
            {
                throw new ArgumentNullException(nameof(repositories));
            }

            var xml = _repositorySerialiser.Serialize(repositories);
            if (xml == null)
            {
                return;
            }

            AppSettings.SetString(key, xml);
        }
    }
}