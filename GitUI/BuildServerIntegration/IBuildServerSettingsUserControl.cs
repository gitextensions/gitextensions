using Nini.Config;

namespace GitUI.BuildServerIntegration
{
    public interface IBuildServerSettingsUserControl
    {
        void Initialize(string defaultProjectName);

        void LoadSettings(IConfig buildServerConfig);
        void SaveSettings(IConfig buildServerConfig);
    }
}