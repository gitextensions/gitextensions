using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace AzureDevOpsIntegration.Settings
{
    /// <summary>
    /// Describes available settings of the Azure DevOps (or TFS>=2015) Build integration plugin
    /// </summary>
    public class IntegrationSettings
    {
        private const string ProjectUrlKey = "ProjectUrl";
        private const string BuildDefinitionFilterKey = "BuildDefinitionNameFilter";
        private const string ApiTokenKey = "RestApiToken";

        /// <summary>
        /// Reads these settings from the given <see cref="ISettingsSource"/>
        /// </summary>
        public static IntegrationSettings ReadFrom(ISettingsSource config)
        {
            var projectUrl = config?.GetString(ProjectUrlKey, "") ?? "";
            var buildDefinitionFilter = config?.GetString(BuildDefinitionFilterKey, "") ?? "";
            var apiToken = config?.GetString(ApiTokenKey, "") ?? "";

            return new IntegrationSettings()
            {
                ProjectUrl = projectUrl,
                BuildDefinitionFilter = buildDefinitionFilter,
                ApiToken = apiToken,
            };
        }

        /// <summary>
        /// Contains the url to the home page of a Azure DevOps / TFS project the build server to integrate belongs to
        /// </summary>
        public string ProjectUrl { get; set; } = "";

        /// <summary>
        /// Contains a regular expression that is used to filter the displayed build results to the given build definitions
        /// </summary>
        public string BuildDefinitionFilter { get; set; } = "";

        /// <summary>
        /// Contains a authentication token which is required and used to request build result information from the Azure DevOps / TFS instance.
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
