namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildServerSettingsUserControl
    {
        void Initialize(string defaultProjectName);

        void LoadSettings(ISettingsSource buildServerConfig);
        void SaveSettings(ISettingsSource buildServerConfig);
    }
}