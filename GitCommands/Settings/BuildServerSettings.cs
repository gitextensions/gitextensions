using GitUIPluginInterfaces;
using GitUIPluginInterfaces.Settings;

namespace GitCommands.Settings
{
    internal sealed class BuildServerSettings : IBuildServerSettings
    {
        private const string? TypeDefault = null;
        private const bool EnableIntegrationDefault = false;
        private const bool ShowBuildResultPageDefault = true;

        private readonly ISettingsSource _settingsSource;

        public BuildServerSettings(ISettingsSource settingsSource)
        {
            _settingsSource = settingsSource;
        }

        public string? Type
        {
            get => _settingsSource.GetString(nameof(Type), TypeDefault);
            set
            {
                if (Type == value)
                {
                    return;
                }

                _settingsSource.SetString(nameof(Type), value);
            }
        }

        public bool EnableIntegration
        {
            get => _settingsSource.GetBool(nameof(EnableIntegration), EnableIntegrationDefault);
            set
            {
                if (EnableIntegration == value)
                {
                    return;
                }

                _settingsSource.SetBool(nameof(EnableIntegration), value);
            }
        }

        public bool ShowBuildResultPage
        {
            get => _settingsSource.GetBool(nameof(ShowBuildResultPage), ShowBuildResultPageDefault);
            set
            {
                if (ShowBuildResultPage == value)
                {
                    return;
                }

                _settingsSource.SetBool(nameof(ShowBuildResultPage), value);
            }
        }
    }
}
