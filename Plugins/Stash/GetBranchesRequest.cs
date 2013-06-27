using Newtonsoft.Json.Linq;

namespace Stash
{
    class GetBranchesRequest : StashRequestBase<JObject>
    {
        private readonly Repository _repo;

        public GetBranchesRequest(Repository repo, Settings settings)
            : base(settings)
        {
            _repo = repo;
        }

        protected override void WriteRequestBody(System.Net.HttpWebRequest request)
        {
        }

        protected override string ApiUrl
        {
            get
            {
                return string.Format("/rest/api/1.0/projects/{0}/repos/{1}/branches?limit=1000",
                                     _repo.ProjectKey, _repo.RepoName);
            }
        }

        protected override string RequestMethod
        {
            get { return "GET"; }
        }

        protected override JObject ParseResponse(JObject json)
        {
            return json;
        }
    }
}
