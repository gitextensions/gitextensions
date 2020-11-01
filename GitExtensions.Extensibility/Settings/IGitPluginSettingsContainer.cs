using GitExtensions.Core.Settings;

namespace GitExtensions.Extensibility.Settings
{
    public interface IGitPluginSettingsContainer
    {
        ISettingsSource GetSettingsSource();

        void SetSettingsSource(ISettingsSource settingsSource);
    }
}
