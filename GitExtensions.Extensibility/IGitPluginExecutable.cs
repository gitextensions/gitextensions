using GitExtensions.Core.Commands.Events;

namespace GitExtensions.Extensibility
{
    public interface IGitPluginExecutable
    {
        /// <summary>
        /// Run the plugin Execute method
        /// </summary>
        /// <param name="args">arguments from the UI</param>
        /// <returns>true, if the revision grid need a refresh false, otherwise </returns>
        bool Execute(GitUIEventArgs args);
    }
}
