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
        /// Saves the provided repository path to history of user git repositories as the "most recent".
        /// </summary>
        /// <param name="repositoryPath">A repository path to be save as "most recent".</param>
        /// <returns>The current version of the history of user git repositories after the update.</returns>
        /// <exception cref="ArgumentException"><paramref name="repositoryPath"/> is <see langword="null"/> or <see cref="string.Empty"/>.</exception>
        [ContractAnnotation("repositoryPath:null=>halt")]
        Task<IList<Repository>> AddAsMostRecentAsync(string repositoryPath);

        /// <summary>
        /// Loads the history of user git repositories.
        /// </summary>
        /// <returns>The history of local git repositories.</returns>
        Task<IList<Repository>> LoadHistoryAsync();

        /// <summary>
        /// Removes <paramref name="repositoryPath"/> from the history of user git repositories.
        /// </summary>
        /// <param name="repositoryPath">A repository path to remove.</param>
        /// <returns>The current version of the history of user git repositories after the update.</returns>
        Task<IList<Repository>> RemoveFromHistoryAsync(string repositoryPath);

        /// <summary>
        /// Loads the history of user git repositories.
        /// </summary>
        /// <param name="repositoryHistory">A collection of local git repositories.</param>
        /// <returns>An awaitable task.</returns>
        Task SaveHistoryAsync(IEnumerable<Repository> repositoryHistory);
    }
}
