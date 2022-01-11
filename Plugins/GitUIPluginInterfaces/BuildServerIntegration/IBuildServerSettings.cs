namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildServerSettings
    {
        private const bool IntegrationEnabledDefault = false;
        private const bool ShowBuildResultPageDefault = true;

        /// <summary>
        ///  Gets or sets the type of the build server (e.g. AppVeyor, TeamCity, etc.).
        /// </summary>
        string? ServerName { get; set; }

        /// <summary>
        ///  Gets or sets whether the integration with the build server is enabled.
        /// </summary>
        bool? IntegrationEnabled { get; set; }

        /// <summary>
        ///  If <see cref="IntegrationEnabled"/> is configured - the configured value; otherwise the default <see langword="false"/>.
        /// </summary>
        bool IntegrationEnabledOrDefault => IntegrationEnabled.GetValueOrDefault(defaultValue: IntegrationEnabledDefault);

        /// <summary>
        ///  Gets or sets whether the build server's build result page is displayed.
        /// </summary>
        bool? ShowBuildResultPage { get; set; }

        /// <summary>
        ///  If <see cref="ShowBuildResultPage"/> is configured - the configured value; otherwise the default <see langword="true"/>.
        /// </summary>
        bool ShowBuildResultPageOrDefault => ShowBuildResultPage.GetValueOrDefault(defaultValue: ShowBuildResultPageDefault);

        /// <summary>
        ///  Gets the settings source for the build server.
        /// </summary>
        ISettingsSource SettingsSource { get; }
    }
}
