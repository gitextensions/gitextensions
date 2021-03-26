using System;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI
{
    public class GitPluginSettingsContainer : ISettingsSource, IGitPluginSettingsContainer
    {
        private readonly Guid _pluginId;
        private readonly string _pluginName;
        private ISettingsSource? _settingsSource;

        public GitPluginSettingsContainer(Guid pluginId, string pluginName)
        {
            _pluginId = pluginId;
            _pluginName = pluginName;
        }

        public ISettingsSource GetSettingsSource()
        {
            return this;
        }

        public void SetSettingsSource(ISettingsSource? settingsSource)
        {
            _settingsSource = settingsSource;
        }

        private ISettingsSource ExternalSettings => _settingsSource ?? AppSettings.SettingsContainer;

        public override SettingLevel SettingLevel
        {
            get => ExternalSettings.SettingLevel;
            set => throw new InvalidOperationException(nameof(SettingLevel));
        }

        public override T GetValue<T>(string name, T defaultValue, Func<string, T> decode)
        {
            // for old plugin setting processing
            if (_pluginId == Guid.Empty)
            {
                return ExternalSettings.GetValue($"{_pluginName}{name}", defaultValue, decode);
            }

            var value = ExternalSettings.GetValue($"{_pluginId}.{name}", defaultValue, decode);

            // for old plugin setting processing
            if (value is null || value.Equals(defaultValue))
            {
                value = ExternalSettings.GetValue($"{_pluginName}{name}", defaultValue, decode);
            }

            return value;
        }

        public override void SetValue<T>(string name, T value, Func<T, string?> encode)
        {
            if (_pluginId == Guid.Empty)
            {
                ExternalSettings.SetValue($"{_pluginName}{name}", value, encode);
            }
            else
            {
                ExternalSettings.SetValue($"{_pluginId}.{name}", value, encode);
            }
        }
    }
}
