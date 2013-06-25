namespace Stash
{
    class GetBranchesRequest : StashRequestBase
    {
        public GetBranchesRequest(Settings settings)
            : base(settings)
        {
        }

        protected override void WriteRequestBody(System.Net.HttpWebRequest request)
        {
        }

        protected override string ApiUrl
        {
            get
            {
                return string.Format("/rest/api/1.0/projects/{0}/repos/{1}/branches?limit=1000",
                                     Settings.ProjectKey, Settings.RepoSlug);
            }
        }

        protected override string RequestMethod
        {
            get { return "GET"; }
        }
    }
}
