using System.Drawing;
using GitExtensions.Extensibility.Settings;
using JetBrains.Annotations;

namespace GitExtensions.Extensibility
{
    public interface IGitPlugin
    {
        string Name { get; }

        string Description { get; }

        [CanBeNull]
        Image Icon { get; }

        IGitPluginSettingsContainer SettingsContainer { get; set; }
    }
}
