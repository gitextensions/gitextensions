using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GitCommands.UserRepositoryHistory
{
    /// <summary>
    /// Provides the ability to manage the history of git repositories used by the user.
    /// </summary>
    public interface IRepositoryManager
    {
        /// <summary>
        /// Saves the provided repository path to history of user git repositories as the "most recent".
        /// </summary>
        /// <param name="repositoryPath">A repository path to be save as "most recent".</param>
        /// <returns>The current version of the history of user git repositories after the update.</returns>
        /// <exception cref="ArgumentException"><paramref name="repositoryPath"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        [ContractAnnotation("repositoryPath:null=>halt")]
        Task<RepositoryHistory> AddAsMostRecentAsync(string repositoryPath);

        /// <summary>
        /// Loads the history of user git repositories.
        /// </summary>
        /// <returns>The history of local git repositories.</returns>
        Task<RepositoryHistory> LoadHistoryAsync();

        /// <summary>
        /// Removes <paramref name="repository"/> from the history of user git repositories.
        /// </summary>
        /// <param name="repository">A repository to remove.</param>
        /// <returns>An awaitable task.</returns>
        Task RemoveFromHistoryAsync(Repository repository);

        /// <summary>
        /// Loads the history of user git repositories.
        /// </summary>
        /// <param name="repositoryHistory">A collection of local git repositories.</param>
        /// <returns>An awaitable task.</returns>
        Task SaveHistoryAsync(RepositoryHistory repositoryHistory);
    }
}
