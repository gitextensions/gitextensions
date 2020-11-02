using GitExtensions.Core.Commands.Events;

namespace GitExtensions.Extensibility.Events
{
    /// <summary>
    /// Interface to implement if you wish to receive OnPreCommit.
    /// </summary>
    public interface IPreCommitHandler
    {
        void OnPreCommit(GitUIEventArgs e);
    }
}
