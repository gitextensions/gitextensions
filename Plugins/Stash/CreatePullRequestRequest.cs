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
        public Repository SourceRepo { get; set; }
        public Repository TargetRepo { get; set; }
        public string SourceBranch { get; set; }
        public string TargetBranch { get; set; }
        public IEnumerable<StashUser> Reviewers { get; set; }
    }

    class CreatePullRequestRequest : StashRequestBase<JObject>
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
                                     _info.TargetRepo.ProjectKey, _info.TargetRepo.RepoName);
            }
        }

        protected override string RequestMethod
        {
            get { return "POST"; }
        }

        protected override JObject ParseResponse(JObject json)
        {
            return json;
        }

        private string GetPullRequestBody()
        {
            var resource = new JObject();
            resource["title"] = _info.Title;
            resource["description"] = _info.Description;

            resource["fromRef"] = CreatePullRequestRef(
                _info.SourceRepo.ProjectKey
                , _info.SourceRepo.RepoName, _info.SourceBranch);

            resource["toRef"] = CreatePullRequestRef(
                _info.TargetRepo.ProjectKey
                , _info.TargetRepo.RepoName, _info.TargetBranch);

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

        private JObject CreatePullRequestRef(string projectKey, string repoName, string branchName)
        {
            var reference = new JObject();
            reference["id"] = branchName;
            reference["repository"] = new JObject();
            reference["repository"]["slug"] = repoName;
            reference["repository"]["project"] = new JObject();
            reference["repository"]["project"]["key"] = projectKey;
            return reference;
        }
    }
}