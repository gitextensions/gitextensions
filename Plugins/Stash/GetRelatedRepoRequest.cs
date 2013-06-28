using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Stash
{
    class Repository
    {
        public static Repository Parse(JObject json)
        {
            return new Repository
                       {
                           Id = json["id"].ToString(),
                           RepoName = json["name"].ToString(),
                           ProjectName = json["project"]["name"].ToString(),
                           ProjectKey = json["project"]["key"].ToString()
                       };
        }
        public string Id { get; set; }
        public string ProjectKey { get; set; }
        public string ProjectName { get; set; }
        public string RepoName { get; set; }
        public string DisplayName
        {
            get { return string.Format("{0}/{1}", ProjectName, RepoName); }
        }
    }
    
    class GetRelatedRepoRequest : StashRequestBase<List<Repository>>
    {
        public GetRelatedRepoRequest(Settings settings) : base(settings)
        {
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
                return string.Format(
                    "/rest/api/latest/projects/{0}/repos/{1}/related?start=0&limit=20",
                    Settings.ProjectKey, Settings.RepoSlug);
            }
        }

        protected override List<Repository> ParseResponse(JObject json)
        {
            var result = new List<Repository>();
            foreach (JObject val in json["values"])
            {
                result.Add(Repository.Parse(val));
            }
            return result;
        }
    }
}
