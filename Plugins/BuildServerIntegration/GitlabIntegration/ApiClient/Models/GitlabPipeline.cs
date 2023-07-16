using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitExtensions.Plugins.GitlabIntegration.ApiClient.Models
{
    internal class GitlabPipeline
    {
        public BuildInfo ToBuildInfo()
        {
            return new BuildInfo();
        }
    }
}
