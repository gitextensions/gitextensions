using Nini.Config;

namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildServerSettingsUserControl
    {
        void Initialize(string defaultProjectName);

        void LoadSettings(IConfig buildServerConfig);
        void SaveSettings(IConfig buildServerConfig);
    }
}