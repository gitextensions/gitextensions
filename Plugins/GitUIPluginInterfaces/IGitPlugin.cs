using System.Collections.Generic;

namespace GitUIPluginInterfaces
{
    public interface IGitPlugin
    {
        string Description { get; }

        ISettingsSource Settings { get; set; }

        IEnumerable<ISetting> GetSettings();

        void Register(IGitUICommands gitUiCommands);

        void Unregister(IGitUICommands gitUiCommands);

        bool Execute(GitUIBaseEventArgs gitUiCommands);
    }
}