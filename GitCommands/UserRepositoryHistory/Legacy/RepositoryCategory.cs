using System.Collections.Generic;

namespace GitCommands.UserRepositoryHistory.Legacy
{
    /// <summary>
    /// This type is necessary to provide backwards compatibility ONLY.
    /// </summary>
    /// <remarks>
    /// This DTO is deserialised from user settings, the order of the properties is significant.
    /// </remarks>
    public class RepositoryCategory
    {
        public List<Repository> Repositories { get; set; }
        public string Description { get; set; }
        public string CategoryType { get; set; }
    }
}