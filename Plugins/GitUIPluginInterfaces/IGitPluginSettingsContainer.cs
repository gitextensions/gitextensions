using System.Collections.Generic;

namespace GitUIPluginInterfaces
{
    public interface IGitPluginSettingsContainer
    {
        ISettingsSource GetSettingsSource();
        void SetSettingsSource(ISettingsSource settingsSource);
    }
}