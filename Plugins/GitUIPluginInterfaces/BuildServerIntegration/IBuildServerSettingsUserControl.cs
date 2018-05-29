using System.Collections.Generic;

namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildServerSettingsUserControl
    {
        void Initialize(string defaultProjectName, IEnumerable<string> remotes);

        void LoadSettings(ISettingsSource buildServerConfig);
        void SaveSettings(ISettingsSource buildServerConfig);
    }
}