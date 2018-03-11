using Newtonsoft.Json.Linq;
using RestSharp;

namespace Bitbucket
{
    internal class ApprovePullRequest : BitbucketRequestBase<JObject>
    {
        private readonly MergeRequestInfo _info;

        public ApprovePullRequest(Settings settings, MergeRequestInfo info)
            : base(settings)
        {
            _info = info;
        }

        protected override object RequestBody => "";

        protected override Method RequestMethod => Method.POST;

        protected override string ApiUrl => string.Format("rest/api/1.0/projects/{0}/repos/{1}/pull-requests/{2}/approve",
            _info.ProjectKey, _info.TargetRepo, _info.Id);

        protected override JObject ParseResponse(JObject json)
        {
            return json;
        }
    }
}