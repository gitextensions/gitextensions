using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitExtensions.Plugins.GitlabIntegration.ApiClient.Models
{
    internal class GitlabPipeline
    {
        public string id { get; set; }
        public string status { get; set; }
        public string sha { get; set; }

        public string web_url { get; set; }

        public DateTime created_at { get; set; }

        public DateTime updated_at { get; set; }

        public BuildInfo ToBuildInfo()
        {
            BuildInfo result = new();

            result.Id = id;
            result.CommitHashList = new List<ObjectId> { ObjectId.Parse(sha) };

            switch (status)
            {
                case "running":
                    result.Status = BuildInfo.BuildStatus.InProgress;
                    break;
                case "success":
                    result.Status = BuildInfo.BuildStatus.Success;
                    break;
                case "failed":
                    result.Status = BuildInfo.BuildStatus.Failure;
                    break;
                case "canceled":
                    result.Status = BuildInfo.BuildStatus.Stopped;
                    break;
                default:
                    result.Status = BuildInfo.BuildStatus.Unknown;
                    break;
            }

            result.Url = web_url;
            result.Duration = (updated_at - created_at).Ticks;
            result.ShowInBuildReportTab = true;

            return result;
        }
    }
}
