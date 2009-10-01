using System;
using System.Collections.Generic;
using System.Text;

namespace GitUI
{
    public interface IGitPlugin
    {
        string Description { get; }

        void Register(GitUICommands gitUICommands, IGitPluginSettingsContainer settings);
        void Execute(GitUIEventArgs gitUICommands);
    }

    public interface IGitPluginSettingsContainer
    {
        void AddSetting(string name, string defaultValue);
        string GetSetting(string name);
    }
}
