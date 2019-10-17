using System.Collections.Generic;
using System.Drawing;

namespace GitUIPluginInterfaces
{
    public interface IGitPlugin
    {
        string Name { get; }

        string Description { get; }

        Image? Icon { get; }

        IGitPluginSettingsContainer SettingsContainer { get; set; }

        bool HasSettings { get; }

        IEnumerable<ISetting> GetSettings();

        void Register(IGitUICommands gitUiCommands);

        void Unregister(IGitUICommands gitUiCommands);

        bool Execute(GitUIEventArgs args);
    }
}