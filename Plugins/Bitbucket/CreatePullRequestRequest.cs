using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Bitbucket
{
    internal class PullRequestInfo
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Repository SourceRepo { get; set; }
        public Repository TargetRepo { get; set; }
        public string SourceBranch { get; set; }
        public string TargetBranch { get; set; }
        public IEnumerable<BitbucketUser> Reviewers { get; set; }
    }

    internal class CreatePullRequestRequest : BitbucketRequestBase<JObject>
    {
        private readonly PullRequestInfo _info;

        public CreatePullRequestRequest(Settings settings, PullRequestInfo info)
            : base(settings)
        {
            _info = info;
        }

        protected override object RequestBody => GetPullRequestBody();

        protected override Method RequestMethod => Method.POST;

        protected override string ApiUrl => string.Format("/projects/{0}/repos/{1}/pull-requests",
            _info.TargetRepo.ProjectKey, _info.TargetRepo.RepoName);

        protected override JObject ParseResponse(JObject json)
        {
            return json;
        }

        private string GetPullRequestBody()
        {
            var resource = new JObject
            {
                ["title"] = _info.Title,
                ["description"] = _info.Description,

                ["fromRef"] = CreatePullRequestRef(
                _info.SourceRepo.ProjectKey,
                _info.SourceRepo.RepoName, _info.SourceBranch),

                ["toRef"] = CreatePullRequestRef(
                _info.TargetRepo.ProjectKey,
                _info.TargetRepo.RepoName, _info.TargetBranch)
            };

            var reviewers = new JArray();
            foreach (var reviewer in _info.Reviewers)
            {
                var r = new JObject
                {
                    ["user"] = new JObject
                    {
                        ["name"] = reviewer.Slug
                    }
                };

                reviewers.Add(r);
            }

            resource["reviewers"] = reviewers;

            return resource.ToString();
        }

        private static JObject CreatePullRequestRef(string projectKey, string repoName, string branchName)
        {
            return new JObject
            {
                ["id"] = branchName,
                ["repository"] = new JObject
                {
                    ["slug"] = repoName,
                    ["project"] = new JObject
                    {
                        ["key"] = projectKey
                    }
                }
            };
        }
    }
}