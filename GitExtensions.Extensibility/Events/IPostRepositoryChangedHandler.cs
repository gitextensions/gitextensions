using GitExtensions.Core.Commands.Events;

namespace GitExtensions.Extensibility.Events
{
    /// <summary>
    /// Interface to implement if you wish to receive OnPostRepositoryChanged.
    /// </summary>
    public interface IPostRepositoryChangedHandler
    {
        void OnPostRepositoryChanged(GitUIEventArgs e);
    }
}
