using System.Drawing;
using JetBrains.Annotations;

namespace GitUIPluginInterfaces
{
    public interface IGitPlugin
    {
        string Name { get; }

        string Description { get; }

        [CanBeNull]
        Image Icon { get; }

        IGitPluginSettingsContainer SettingsContainer { get; set; }

        /// <summary>
        /// Runs the plugin and returns whether the RevisionGrid should be refreshed.
        /// </summary>
        bool Execute(GitUIEventArgs args);
    }
}
