using System;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI
{
    public class GitPluginSettingsContainer : ISettingsSource, IGitPluginSettingsContainer
    {
        private readonly string pluginName;
        private ISettingsSource _settingsSource;

        public GitPluginSettingsContainer(string pluginName)
        {
            this.pluginName = pluginName;
        }

        public ISettingsSource GetSettingsSource()
        {
            return this;
        }

        public void SetSettingsSource(ISettingsSource settingsSource)
        {
            _settingsSource = settingsSource;
        }

        private ISettingsSource ExternalSettings
        {
            get
            {
                return _settingsSource ?? AppSettings.SettingsContainer;
            }
        }

        public override T GetValue<T>(string name, T defaultValue, Func<string, T> decode)
        {
            return ExternalSettings.GetValue(pluginName + name, defaultValue, decode);
        }

        public override void SetValue<T>(string name, T value, Func<T, string> encode)
        {
            ExternalSettings.SetValue(pluginName + name, value, encode);
        }
    }
}
