using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace VstsAndTfsIntegration.Settings
{
    /// <summary>
    /// Describes available settings of the Azure DevOps / VSTS and Team Foundation Build integration plugin
    /// </summary>
    public class VstsIntegrationSettings
    {
        private const string ProjectUrlKey = "VstsTfsProjectUrl";
        private const string BuildDefinitionFilterKey = "VstsTfsBuildDefinitionNameFilter";
        private const string ApiTokenKey = "VstsTfsRestApiToken";

        /// <summary>
        /// Reads these settings from the given <see cref="ISettingsSource"/>
        /// </summary>
        public static VstsIntegrationSettings ReadFrom(ISettingsSource config)
        {
            var projectUrl = config?.GetString(ProjectUrlKey, "") ?? "";
            var buildDefinitionFilter = config?.GetString(BuildDefinitionFilterKey, "") ?? "";
            var apiToken = config?.GetString(ApiTokenKey, "") ?? "";

            return new VstsIntegrationSettings()
            {
                ProjectUrl = projectUrl,
                BuildDefinitionFilter = buildDefinitionFilter,
                ApiToken = apiToken,
            };
        }

        /// <summary>
        /// Contains the url to the home page of a VSTS/TFS project the build server to integrate belongs to
        /// </summary>
        public string ProjectUrl { get; set; } = "";

        /// <summary>
        /// Contains a regular expression that is used to filter the displayed build results to the given build definitions
        /// </summary>
        public string BuildDefinitionFilter { get; set; } = "";

        /// <summary>
        /// Contains a authentication token which is required and used to request build result information from the VSTS/TFS instance.
        /// </summary>
        public string ApiToken { get; set; } = "";

        /// <summary>
        /// Writes these settings to the given <see cref="ISettingsSource"/>
        /// </summary>
        public void WriteTo(ISettingsSource config)
        {
            config.SetString(ProjectUrlKey, ProjectUrl);
            config.SetString(BuildDefinitionFilterKey, BuildDefinitionFilter);
            config.SetString(ApiTokenKey, ApiToken);
        }

        /// <summary>
        /// Validates if these settings are valid and good to use.
        /// </summary>
        public bool IsValid()
        {
            return !(string.IsNullOrWhiteSpace(ProjectUrl) || string.IsNullOrWhiteSpace(ApiToken)) && BuildServerSettingsHelper.IsRegexValid(BuildDefinitionFilter);
        }
    }
}
