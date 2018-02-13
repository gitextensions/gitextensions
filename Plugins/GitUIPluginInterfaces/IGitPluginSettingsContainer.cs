namespace GitUIPluginInterfaces
{
    public interface IGitPluginSettingsContainer
    {
        ISettingsSource GetSettingsSource();
        void SetSettingsSource(ISettingsSource settingsSource);
    }
}