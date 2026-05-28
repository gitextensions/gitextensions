using GitExtensions.Extensibility.Settings;

namespace GitCommands.Settings;

public static class BuildServerSettings
{
    private static readonly SettingsPath _settingsPath = new(parent: null, "BuildServer");

    /// <summary>
    ///  Gets the type of the build server (e.g. AppVeyor, TeamCity, etc.).
    /// </summary>
    public static StringSetting ServerName { get; } = new(_settingsPath.PathFor("Type"), defaultValue: "");

    /// <summary>
    ///  Gets whether the integration with the build server is enabled.
    /// </summary>
    public static BoolSetting IntegrationEnabled { get; } = new(_settingsPath.PathFor("EnableIntegration"), defaultValue: false);

    /// <summary>
    ///  Gets whether the build server's build result page is displayed.
    /// </summary>
    public static BoolSetting ShowBuildResultPage { get; } = new(_settingsPath.PathFor(nameof(ShowBuildResultPage)), defaultValue: false);

    /// <summary>
    ///  Gets the settings source for the build server configured in <paramref name="settingsSource"/>.
    /// </summary>
    public static SettingsSource GetSettingsSource(SettingsSource settingsSource)
        => new SettingsPath(settingsSource, _settingsPath.PathFor(ServerName.ValueOrDefault(settingsSource)));
}
