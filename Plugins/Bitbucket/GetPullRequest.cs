using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace GitExtensions.Plugins.Bitbucket
{
    internal sealed class PullRequest
    {
        public static PullRequest Parse(JObject json)
        {
            PullRequest request = new()
            {
                Id = json["id"].ToString(),
                Version = json["version"].ToString(),
                State = json["state"].ToString(),
                Title = json["title"].ToString(),
                Description = json["description"]?.ToString() ?? "",
                Author = json["author"]["user"]["displayName"].ToString(),
                SrcProjectName = json["fromRef"]["repository"]["project"]["name"].ToString(),
                SrcRepo = json["fromRef"]["repository"]["name"].ToString(),
                SrcBranch = json["fromRef"]["displayId"].ToString(),
                DestProjectName = json["toRef"]["repository"]["project"]["name"].ToString(),
                DestProjectKey = json["toRef"]["repository"]["project"]["key"].ToString(),
                DestRepo = json["toRef"]["repository"]["name"].ToString(),
                DestBranch = json["toRef"]["displayId"].ToString(),
                CreatedDate = Convert.ToDouble(json["createdDate"].ToString().Substring(0, 10))
            };
            var reviewers = json["reviewers"];
            var participants = json["participants"];

            if (!reviewers.HasValues)
            {
                request.Reviewers = "None";
            }
            else
            {
                request.Reviewers = "";

                foreach (var reviewer in reviewers)
                {
                    request.Reviewers += reviewer["user"]["displayName"] + " (" + reviewer["approved"] + ")" + Environment.NewLine;
                }
            }

            if (!participants.HasValues)
            {
                request.Participants = "None";
            }
            else
            {
                request.Participants = "";

                foreach (var participant in participants)
                {
                    request.Participants += participant["user"]["displayName"] + " (" + participant["approved"] + ")" + Environment.NewLine;
                }
            }

            return request;
        }

        public string? Id { get; set; }
        public string? Version { get; set; }
        public string? DestProjectKey { get; set; }
        public string? State { get; set; }
        public string? SrcProjectName { get; set; }
        public string? DestProjectName { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Reviewers { get; set; }
        public string? Participants { get; set; }
        public string? Author { get; set; }
        public string? SrcRepo { get; set; }
        public string? SrcBranch { get; set; }
        public string? DestRepo { get; set; }
        public string? DestBranch { get; set; }
        public double CreatedDate { get; set; }

        public string SrcDisplayName => $"{SrcProjectName}/{SrcRepo}";
        public string DestDisplayName => $"{DestProjectName}/{DestRepo}";
        public string DisplayName => $"#{Id}: {Title}, {ConvertFromUnixTimestamp(CreatedDate):yyyy-MM-dd}";

        private static readonly DateTime _epoch = new(1970, 1, 1, 0, 0, 0, 0);

        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            return _epoch.AddSeconds(timestamp);
        }
    }

    internal class GetPullRequest : BitbucketRequestBase<List<PullRequest>>
    {
        private readonly string _projectKey;
        private readonly string _repoName;
        public GetPullRequest(string projectKey, string repoName, Settings settings)
            : base(settings)
        {
            _projectKey = projectKey;
            _repoName = repoName;
        }

        protected override object? RequestBody => null;

        protected override Method RequestMethod => Method.GET;

        protected override string ApiUrl => string.Format("/rest/api/latest/projects/{0}/repos/{1}/pull-requests?directions=incoming",
            _projectKey, _repoName);

        protected override List<PullRequest> ParseResponse(JObject json)
        {
            List<PullRequest> result = new();
            foreach (JObject val in json["values"])
            {
                result.Add(PullRequest.Parse(val));
            }

            return result;
        }
    }
}
