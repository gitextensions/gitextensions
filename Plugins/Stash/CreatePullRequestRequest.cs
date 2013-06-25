using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Stash
{
    class PullRequestInfo
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string SourceBranch { get; set; }
        public string TargetBranch { get; set; }
        public IEnumerable<StashUser> Reviewers { get; set; }
    }

    class CreatePullRequestRequest : StashRequestBase
    {
        private readonly PullRequestInfo _info;

        public CreatePullRequestRequest(Settings settings, PullRequestInfo info)
            : base(settings)
        {
            _info = info;
        }

        protected override void WriteRequestBody(HttpWebRequest request)
        {
            using (var bodyStream = request.GetRequestStream())
            {
                using (var writer = new StreamWriter(bodyStream))
                {
                    writer.Write(GetPullRequestBody());
                }
            }
        }

        protected override string ApiUrl
        {
            get
            {
                return string.Format("/projects/{0}/repos/{1}/pull-requests",
                                     Settings.ProjectKey, Settings.RepoSlug);
            }
        }

        protected override string RequestMethod
        {
            get { return "POST"; }
        }

        private string GetPullRequestBody()
        {
            var project = new JObject();
            project["key"] = "CCA";

            var repository = new JObject();
            repository["slug"] = "mycca";
            repository["project"] = project;

            var fromRef = new JObject();
            fromRef["id"] = _info.SourceBranch;
            fromRef["repository"] = repository;

            var toRef = new JObject();
            toRef["id"] = _info.TargetBranch;
            toRef["repository"] = repository;

            var resource = new JObject();
            resource["title"] = _info.Title;
            resource["description"] = _info.Description;
            resource["fromRef"] = fromRef;
            resource["toRef"] = toRef;

            var reviewers = new JArray();
            foreach (var reviewer in _info.Reviewers)
            {
                var r = new JObject();
                r["user"] = new JObject();
                r["user"]["name"] = reviewer.Slug;

                reviewers.Add(r);
            }
            resource["reviewers"] = reviewers;

            return resource.ToString();
        }

    }
}