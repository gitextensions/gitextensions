using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitCommands.Settings
{
    internal sealed class BuildServerSettings : IBuildServerSettings
    {
        private const string BuildServerGroupName = "BuildServer";
        private const string BuildServerTypeName = "Type";
        private const string BuildServerIntegrationEnabledName = "EnableIntegration";
        private readonly ISettingsSource _settingsSource;

        public BuildServerSettings(ISettingsSource settingsSource)
        {
            _settingsSource = settingsSource;
        }

        public ISettingsSource SettingsSource => new SettingsPath(_settingsSource, $"{BuildServerGroupName}.{ServerName}");

        public string? ServerName
        {
            get => _settingsSource.GetString($"{BuildServerGroupName}.{BuildServerTypeName}", defaultValue: null);
            set
            {
                if (ServerName == value)
                {
                    return;
                }

                _settingsSource.SetString($"{BuildServerGroupName}.{BuildServerTypeName}", value);
            }
        }

        public bool? IntegrationEnabled
        {
            get => _settingsSource.GetBool($"{BuildServerGroupName}.{BuildServerIntegrationEnabledName}");
            set
            {
                if (IntegrationEnabled == value)
                {
                    return;
                }

                _settingsSource.SetBool($"{BuildServerGroupName}.{BuildServerIntegrationEnabledName}", value);
            }
        }

        public bool? ShowBuildResultPage
        {
            get => _settingsSource.GetBool($"{BuildServerGroupName}.{nameof(ShowBuildResultPage)}");
            set
            {
                if (ShowBuildResultPage == value)
                {
                    return;
                }

                _settingsSource.SetBool($"{BuildServerGroupName}.{nameof(ShowBuildResultPage)}", value);
            }
        }
    }
}
