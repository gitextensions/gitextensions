using Newtonsoft.Json.Linq;
using RestSharp;

namespace Bitbucket
{
    class GetRepoRequest : BitbucketRequestBase<Repository>
    {
        private readonly string _projectKey;
        private readonly string _repoName;

        public GetRepoRequest(string projectKey, string repoName, Settings settings) : base(settings)
        {
            _projectKey = projectKey;
            _repoName = repoName;
        }

        protected override object RequestBody
        {
            get { return null; }
        }

        protected override Method RequestMethod
        {
            get { return Method.GET; }
        }

        protected override string ApiUrl
        {
            get
            {
                return string.Format("/rest/api/latest/projects/{0}/repos/{1}",
                                     _projectKey, _repoName);
            }
        }

        protected override Repository ParseResponse(JObject json)
        {
            return Repository.Parse(json);
        }
    }
}
