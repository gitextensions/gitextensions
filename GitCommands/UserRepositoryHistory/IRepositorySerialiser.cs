using System.Collections.Generic;

namespace GitCommands.UserRepositoryHistory
{
    /// <summary>
    /// Provides the ability to serialise and deserialise collections of user's git repositories.
    /// </summary>
    public interface IRepositorySerialiser<T>
    {
        /// <summary>
        /// Restores a list of user's git repositories from the supplied string.
        /// </summary>
        /// <param name="serialised">A serialised list of user's git repositories.</param>
        /// <returns>A list of user's git repositories.</returns>
        IReadOnlyList<T> Deserialize(string serialised);

        /// <summary>
        /// Serialises the given list of user's git repositories.
        /// </summary>
        /// <param name="repositories">A list of user's git repositories.</param>
        /// <returns>A serialised list of user's git repositories.</returns>
        string Serialize(IEnumerable<T> repositories);
    }
}