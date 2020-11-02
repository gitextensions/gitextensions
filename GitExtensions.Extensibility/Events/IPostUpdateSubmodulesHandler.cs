using GitExtensions.Core.Commands.Events;

namespace GitExtensions.Extensibility.Events
{
    /// <summary>
    /// Interface to implement if you wish to receive OnPostUpdateSubmodules.
    /// </summary>
    public interface IPostUpdateSubmodulesHandler
    {
        void OnPostUpdateSubmodules(GitUIPostActionEventArgs e);
    }
}
