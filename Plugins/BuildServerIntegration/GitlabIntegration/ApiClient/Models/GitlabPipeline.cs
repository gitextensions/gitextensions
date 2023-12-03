using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using Newtonsoft.Json;

namespace GitExtensions.Plugins.GitlabIntegration.ApiClient.Models
{
    public class GitlabPipeline
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("sha")]
        public string Sha { get; set; }
        [JsonProperty("web_url")]
        public string WebUrl { get; set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public BuildInfo ToBuildInfo()
        {
            BuildInfo result = new()
            {
                Id = Id.ToString(),
                CommitHashList = new List<ObjectId> { ObjectId.Parse(Sha) }
            };

            switch (Status)
            {
                case "running":
                    result.Status = BuildStatus.InProgress;
                    break;
                case "success":
                    result.Status = BuildStatus.Success;
                    break;
                case "failed":
                    result.Status = BuildStatus.Failure;
                    break;
                case "canceled":
                    result.Status = BuildStatus.Stopped;
                    break;
                default:
                    result.Status = BuildStatus.Unknown;
                    break;
            }

            result.Url = WebUrl;
            result.Duration = (UpdatedAt - CreatedAt).Ticks;
            result.ShowInBuildReportTab = false;

            return result;
        }
    }
}
