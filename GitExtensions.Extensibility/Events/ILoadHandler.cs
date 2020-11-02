using GitExtensions.Core.Commands;

namespace GitExtensions.Extensibility.Events
{
    /// <summary>
    /// Interface to implement if you wish to receive OnLoad.
    /// Called before the plugin starts running in system.
    /// </summary>
    public interface ILoadHandler
    {
        void OnLoad(IGitUICommands gitUiCommands);
    }
}
