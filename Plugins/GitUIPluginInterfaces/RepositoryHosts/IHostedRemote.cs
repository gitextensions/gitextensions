using JetBrains.Annotations;

namespace GitUIPluginInterfaces.RepositoryHosts
{
    public interface IHostedRemote
    {
        IHostedRepository GetHostedRepository();

        /// <summary>
        /// Gets the name of the remote in the local git repository. May be null.
        /// </summary>
        [CanBeNull]
        string Name { get; }

        string Data { get; }

        string DisplayData { get; }

        bool IsOwnedByMe { get; }
    }
}
