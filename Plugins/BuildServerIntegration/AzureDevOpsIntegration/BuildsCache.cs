using GitUIPluginInterfaces.BuildServerIntegration;

namespace AzureDevOpsIntegration
{
    public class BuildsCache
    {
        public string? Id { get; set; }
        public string? BuildDefinitions { get; init; }
        public List<BuildInfo> FinishedBuilds { get; } = new List<BuildInfo>();
        public DateTime LastCall { get; set; } = DateTime.MinValue;
    }
}
