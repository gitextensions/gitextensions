using System.Collections.Generic;
using GitUIPluginInterfaces;

namespace GitExtensions.Plugins.BuildServer.Core
{
    public interface IBuildServerSettingsUserControl
    {
        void Initialize(string defaultProjectName, IEnumerable<string?> remotes);

        void LoadSettings(ISettingsSource buildServerConfig);
        void SaveSettings(ISettingsSource buildServerConfig);
    }
}
