using System.Collections.Generic;
using System.Xml.Serialization;

namespace GitCommands.UserRepositoryHistory.Legacy
{
    /// <summary>
    /// The surrogate is necessary to provide backwards compatibility.
    /// The original implementation persisted user's git repositories under "RepositoryHistory" root node.
    /// </summary>
    [XmlRoot("RepositoryHistory")]
    public class RepositoryHistorySurrogate
    {
        public RepositoryHistorySurrogate(IEnumerable<UserRepositoryHistory.Repository> repositories)
        {
            Repositories.AddRange(repositories);
        }

        public RepositoryHistorySurrogate()
        {
        }

        public List<UserRepositoryHistory.Repository> Repositories { get; } = new List<UserRepositoryHistory.Repository>();
    }
}