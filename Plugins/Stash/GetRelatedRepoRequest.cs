using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Stash
{
    class Repository
    {
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

        protected override void WriteRequestBody(HttpWebRequest request)
        {
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

        protected override string RequestMethod
        {
            get { return "GET"; }
        }

        protected override List<Repository> ParseResponse(JObject json)
        {
            var result = new List<Repository>();
            //add current project/repo as the default
            result.Add(new Repository
                           {
                               RepoName = Settings.RepoSlug,
                               ProjectName = Settings.ProjectKey,
                               ProjectKey = Settings.ProjectKey
                           });

            foreach (var val in json["values"])
            {
                result.Add(new Repository
                               {
                                   RepoName = val["name"].ToString(),
                                   ProjectName = val["project"]["name"].ToString(),
                                   ProjectKey = val["project"]["key"].ToString()
                               });
            }
            return result;
        }
    }
}
