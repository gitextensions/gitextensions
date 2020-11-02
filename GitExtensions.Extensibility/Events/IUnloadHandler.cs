using GitExtensions.Core.Commands;

namespace GitExtensions.Extensibility.Events
{
    /// <summary>
    /// Interface to implement if you wish to receive OnLoad.
    /// Called after the plugin has finished running in system.
    /// </summary>
    public interface IUnloadHandler
    {
        void OnUnload(IGitUICommands gitUiCommands);
    }
}
