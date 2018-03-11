using Newtonsoft.Json.Linq;
using RestSharp;

namespace Bitbucket
{
    internal class MergeRequestInfo
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string ProjectKey { get; set; }
        public string TargetRepo { get; set; }
    }

    internal class MergePullRequest : BitbucketRequestBase<JObject>
    {
        private readonly MergeRequestInfo _info;

        public MergePullRequest(Settings settings, MergeRequestInfo info)
            : base(settings)
        {
            _info = info;
        }

        protected override object RequestBody => "";

        protected override Method RequestMethod => Method.POST;

        protected override string ApiUrl => string.Format("rest/api/1.0/projects/{0}/repos/{1}/pull-requests/{2}/merge?version={3}",
            _info.ProjectKey, _info.TargetRepo, _info.Id, _info.Version);

        protected override JObject ParseResponse(JObject json)
        {
            return json;
        }
    }
}