using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitExtensions.Plugins.GitlabIntegration.ApiClient.Models
{
    internal class GitlabPipeline
    {
        public string status { get; set; }
        public string sha { get; set; }

        public BuildInfo ToBuildInfo()
        {
            return new BuildInfo
            {
                CommitHashList = new List<ObjectId> { ObjectId.Parse(sha) },
                Status = BuildInfo.BuildStatus.Success,
                Description = "test",
                Tooltip = "tooltip",
                Url = "example.com",
                Duration = 10,
                ShowInBuildReportTab = true
            };
        }
    }
}
