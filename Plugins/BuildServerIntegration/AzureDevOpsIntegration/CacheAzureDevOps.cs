using System;
using System.Collections.Generic;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace AzureDevOpsIntegration
{
    public class CacheAzureDevOps
    {
        public bool ShouldBeKept { get; set; }
        public string Id { get; set; }
        public string BuildDefinitions { get; set; }
        public List<BuildInfo> FinishedBuilds { get; set; } = new List<BuildInfo>();
        public DateTime LastCall { get; set; } = DateTime.MinValue;
    }
}
