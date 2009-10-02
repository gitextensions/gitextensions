using System;
using System.Collections.Generic;
using System.Text;

namespace GitUIPluginInterfaces
{
    public interface IGitPlugin
    {
        string Description { get; }

        IGitPluginSettingsContainer Settings { get; set;  }

        void Register(IGitUICommands gitUICommands);
        void Execute(IGitUIEventArgs gitUICommands);
    }

    public interface IGitPluginSettingsContainer
    {
        void AddSetting(string name, string defaultValue);
        void SetSetting(string name, string value);
        string GetSetting(string name);
        IList<string> GetAvailableSettings();
    }
}
