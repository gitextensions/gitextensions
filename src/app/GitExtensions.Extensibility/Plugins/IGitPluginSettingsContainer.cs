using GitExtensions.Extensibility.Settings;

namespace GitExtensions.Extensibility.Plugins;

public interface IGitPluginSettingsContainer
{
    SettingsSource GetSettingsSource();

    void SetSettingsSource(SettingsSource? settingsSource);
}
