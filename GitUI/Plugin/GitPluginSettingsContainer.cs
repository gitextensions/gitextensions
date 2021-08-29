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

        public override string? GetValue(string name)
        {
            // for old plugin setting processing
            if (_pluginId == Guid.Empty)
            {
                return ExternalSettings.GetValue($"{_pluginName}{name}");
            }

            var value = ExternalSettings.GetValue($"{_pluginId}.{name}");

            // for old plugin setting processing
            if (value is null)
            {
                value = ExternalSettings.GetValue($"{_pluginName}{name}");
            }

            return value;
        }

        public override void SetValue(string name, string? value)
        {
            if (_pluginId == Guid.Empty)
            {
                ExternalSettings.SetValue($"{_pluginName}{name}", value);
            }
            else
            {
                ExternalSettings.SetValue($"{_pluginId}.{name}", value);
            }
        }
    }
}
