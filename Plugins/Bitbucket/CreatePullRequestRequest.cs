using System.Collections.Generic;
using Microsoft;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace GitExtensions.Plugins.Bitbucket
{
    internal class PullRequestInfo
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public Repository? SourceRepo { get; set; }
        public Repository? TargetRepo { get; set; }
        public string? SourceBranch { get; set; }
        public string? TargetBranch { get; set; }
        public IEnumerable<BitbucketUser>? Reviewers { get; set; }
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

        protected override string ApiUrl
        {
            get
            {
                Validates.NotNull(_info.TargetRepo);
                return string.Format(
                    "/projects/{0}/repos/{1}/pull-requests",
                    _info.TargetRepo.ProjectKey, _info.TargetRepo.RepoName);
            }
        }

        protected override JObject ParseResponse(JObject json)
        {
            return json;
        }

        private string GetPullRequestBody()
        {
            Validates.NotNull(_info.SourceRepo);
            Validates.NotNull(_info.SourceRepo.ProjectKey);
            Validates.NotNull(_info.SourceRepo.RepoName);
            Validates.NotNull(_info.SourceBranch);
            Validates.NotNull(_info.TargetRepo);
            Validates.NotNull(_info.TargetRepo.ProjectKey);
            Validates.NotNull(_info.TargetRepo.RepoName);
            Validates.NotNull(_info.TargetBranch);
            Validates.NotNull(_info.Reviewers);

            JObject resource = new();
            resource["title"] = _info.Title;
            resource["description"] = _info.Description;

            resource["fromRef"] = CreatePullRequestRef(
                _info.SourceRepo.ProjectKey,
                _info.SourceRepo.RepoName, _info.SourceBranch);

            resource["toRef"] = CreatePullRequestRef(
                _info.TargetRepo.ProjectKey,
                _info.TargetRepo.RepoName, _info.TargetBranch);

            JArray reviewers = new();
            foreach (var reviewer in _info.Reviewers)
            {
                JObject r = new();
                r["user"] = new JObject();
                r["user"]["name"] = reviewer.Slug;

                reviewers.Add(r);
            }

            resource["reviewers"] = reviewers;

            return resource.ToString();
        }

        private static JObject CreatePullRequestRef(string projectKey, string repoName, string branchName)
        {
            JObject reference = new();
            reference["id"] = branchName;
            reference["repository"] = new JObject();
            reference["repository"]["slug"] = repoName;
            reference["repository"]["project"] = new JObject();
            reference["repository"]["project"]["key"] = projectKey;
            return reference;
        }
    }
}
