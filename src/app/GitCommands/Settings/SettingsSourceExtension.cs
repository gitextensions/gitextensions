#nullable enable

using GitExtensions.Extensibility.Settings;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitCommands.Settings;

public static class SettingsSourceExtension
{
    public static IDetailedSettings Detailed(this SettingsSource settingsSource)
        => new DetailedSettings(settingsSource);

    public static IBuildServerSettings GetBuildServerSettings(this SettingsSource settingsSource)
        => new BuildServerSettings(settingsSource);

    public static IDetachedSettings Detached(this SettingsSource settingsSource)
        => new DetachedSettings(settingsSource);
}
