namespace GitUIPluginInterfaces
{
    public interface IGitPluginSettingsContainer
    {
        SettingsSource GetSettingsSource();

        void SetSettingsSource(SettingsSource? settingsSource);
    }
}
