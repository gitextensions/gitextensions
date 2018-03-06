using Newtonsoft.Json.Linq;
using RestSharp;

namespace Bitbucket
{
    internal class GetRepoRequest : BitbucketRequestBase<Repository>
    {
        private readonly string _projectKey;
        private readonly string _repoName;

        public GetRepoRequest(string projectKey, string repoName, Settings settings) : base(settings)
        {
            _projectKey = projectKey;
            _repoName = repoName;
        }

        protected override object RequestBody => null;

        protected override Method RequestMethod => Method.GET;

        protected override string ApiUrl => string.Format("/rest/api/latest/projects/{0}/repos/{1}",
            _projectKey, _repoName);

        protected override Repository ParseResponse(JObject json)
        {
            return Repository.Parse(json);
        }
    }
}
