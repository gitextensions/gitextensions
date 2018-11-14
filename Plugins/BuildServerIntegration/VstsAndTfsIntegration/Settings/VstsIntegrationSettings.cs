using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace VstsAndTfsIntegration.Settings
{
    public class VstsIntegrationSettings
    {
        private const string ProjectUrlKey = "VstsTfsProjectUrl";
        private const string BuildDefinitionFilterKey = "VstsTfsBuildDefinitionNameFilter";
        private const string ApiTokenKey = "VstsTfsRestApiToken";

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

        public string ProjectUrl { get; set; } = "";

        public string BuildDefinitionFilter { get; set; } = "";

        public string ApiToken { get; set; } = "";

        public void WriteTo(ISettingsSource config)
        {
            config.SetString(ProjectUrlKey, ProjectUrl);
            config.SetString(BuildDefinitionFilterKey, BuildDefinitionFilter);
            config.SetString(ApiTokenKey, ApiToken);
        }

        public bool IsValid()
        {
            return !(string.IsNullOrWhiteSpace(ProjectUrl) || string.IsNullOrWhiteSpace(ApiToken)) && BuildServerSettingsHelper.IsRegexValid(BuildDefinitionFilter);
        }
    }
}
