using System.Collections.Generic;

namespace GitUIPluginInterfaces
{
    public interface IGitPlugin
    {
        string Name { get; }

        string Description { get; }

        IGitPluginSettingsContainer SettingsContainer { get; set; }

        IEnumerable<ISetting> GetSettings();

        void Register(IGitUICommands gitUiCommands);

        void Unregister(IGitUICommands gitUiCommands);

        bool Execute(GitUIEventArgs args);
    }
}