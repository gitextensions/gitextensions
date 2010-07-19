using System.Collections.Generic;

namespace GitUIPluginInterfaces
{
    public interface IGitPluginSettingsContainer
    {
        void AddSetting(string name, string defaultValue);
        void SetSetting(string name, string value);
        string GetSetting(string name);
        IList<string> GetAvailableSettings();
    }
}