using GitExtensions.Extensibility.Settings;

namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildServerSettingsUserControl
    {
        void Initialize(string defaultProjectName, IEnumerable<string?> remotes);

        void LoadSettings(SettingsSource buildServerConfig);
        void SaveSettings(SettingsSource buildServerConfig);
    }
}
