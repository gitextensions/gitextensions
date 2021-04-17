using GitUIPluginInterfaces;
using GitUIPluginInterfaces.Settings;

namespace GitCommands.Settings
{
    public static class SettingsSourceExtension
    {
        public static IDetailedSettings Detailed(this ISettingsSource settingsSource)
            => new DetailedSettings(settingsSource);
    }
}
