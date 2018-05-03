using System;
using System.Collections.Generic;
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
        /// Saves the provided repository path to the list of recently used git repositories as the top entry.
        /// </summary>
        /// <param name="repositoryPath">A repository path to be save as "most recent".</param>
        /// <returns>The current version of the list of recently used git repositories after the update.</returns>
        /// <exception cref="ArgumentException"><paramref name="repositoryPath"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        [ContractAnnotation("repositoryPath:null=>halt")]
        Task<IList<Repository>> AddAsMostRecentAsync(string repositoryPath);

        /// <summary>
        /// Loads the list of recently used git repositories.
        /// </summary>
        /// <returns>The list of recently used git repositories.</returns>
        Task<IList<Repository>> LoadRecentHistoryAsync();

        /// <summary>
        /// Removes <paramref name="repositoryPath"/> from the list of recently used git repositories.
        /// </summary>
        /// <param name="repositoryPath">A repository path to remove.</param>
        /// <returns>The current version of the list of recently used git repositories after the update.</returns>
        Task<IList<Repository>> RemoveRecentAsync(string repositoryPath);

        /// <summary>
        /// Saves the list of recently used git repositories.
        /// </summary>
        /// <param name="repositoryHistory">A list of recently used git repositories.</param>
        /// <returns>An awaitable task.</returns>
        Task SaveRecentHistoryAsync(IEnumerable<Repository> repositoryHistory);
    }
}
