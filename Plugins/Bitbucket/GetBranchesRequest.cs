using Newtonsoft.Json.Linq;
using RestSharp;

namespace Bitbucket
{
    internal class GetBranchesRequest : BitbucketRequestBase<JObject>
    {
        private readonly Repository _repo;

        public GetBranchesRequest(Repository repo, Settings settings)
            : base(settings)
        {
            _repo = repo;
        }

        protected override object RequestBody => null;

        protected override Method RequestMethod => Method.GET;

        protected override string ApiUrl => string.Format("/rest/api/1.0/projects/{0}/repos/{1}/branches?limit=1000",
            _repo.ProjectKey, _repo.RepoName);

        protected override JObject ParseResponse(JObject json)
        {
            return json;
        }
    }
}
