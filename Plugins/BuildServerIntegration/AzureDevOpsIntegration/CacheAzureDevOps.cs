using System.Collections.Generic;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace AzureDevOpsIntegration
{
    public class CacheAzureDevOps
    {
        public string Id { get; set; }
        public string BuildDefinitions { get; set; }
        public List<BuildInfo> FinishedBuilds { get; set; } = new List<BuildInfo>();
    }
}
