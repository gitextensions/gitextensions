using JetBrains.Annotations;

namespace GitExtensions.Extensibility.RepositoryHosts
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

        string Owner { get; }

        string RemoteRepositoryName { get; }

        string RemoteUrl { get; }
        GitProtocol CloneProtocol { get; }

        string GetBlameUrl(string commitHash, string fileName, int lineIndex);
    }
}
