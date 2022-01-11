using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using GitUIPluginInterfaces.Settings;

namespace GitCommands.Settings
{
    public static class SettingsSourceExtension
    {
        public static ISettingsSource ByPath(this ISettingsSource settingsSource, string pathName)
            => new SettingsPath(settingsSource, pathName);

        public static IDetailedSettings Detailed(this ISettingsSource settingsSource)
            => new DetailedSettings(settingsSource);

        public static IBuildServerSettings GetBuildServerSettings(this ISettingsSource settingsSource)
            => new BuildServerSettings(settingsSource);

        public static IDetachedSettings Detached(this ISettingsSource settingsSource)
            => new DetachedSettings(settingsSource);
    }
}
