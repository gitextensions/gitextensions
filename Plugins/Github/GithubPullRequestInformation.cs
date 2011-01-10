using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces;
using GithubSharp.Core.Models;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace Github
{
    class GithubPullRequestInformation : IPullRequestInformation
    {
        private PullRequest _pullRequestDetails;
        private GithubPlugin _plugin;

        internal GithubPullRequestInformation(string issueOwner, string repoName, PullRequest pullRequestDetails, GithubPlugin plugin)
        {
            _pullRequestDetails = pullRequestDetails;
            _plugin = plugin;

            Owner = pullRequestDetails.User.Login;
            Created = pullRequestDetails.Created;
            Title = pullRequestDetails.Title;
            Body = pullRequestDetails.Body;
            Id = pullRequestDetails.Number.ToString();
            IssueOwner = issueOwner;
            RepositoryName = repoName;

            BaseRepo = GithubHostedRepo.Convert(_plugin, _pullRequestDetails.Base.Repository);
            BaseSha = _pullRequestDetails.Base.Sha;
            BaseRef = _pullRequestDetails.Base.Ref;
            HeadRepo = GithubHostedRepo.Convert(_plugin, _pullRequestDetails.Head.Repository);
            HeadSha = _pullRequestDetails.Head.Sha;
            HeadRef = _pullRequestDetails.Head.Ref;
        }

        public string Owner { get; private set; }
        public string IssueOwner { get; set; }
        public string RepositoryName { get; private set; }
        public DateTime Created { get; private set; }
        public string Id { get; private set; }

        public string DetailedInfo
        {
            get
            {
                return string.Format("Base repo owner: {0} Head Repo Owner: {1}\r\vIssue votes: {2}", _pullRequestDetails.Base.User.Login, _pullRequestDetails.Head.User.Login, _pullRequestDetails.Votes);
            }
        }

        private string _diffData;
        public string DiffData 
        { 
            get
            {
                if (_diffData == null)
                {
                    HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(_pullRequestDetails.DiffUrl);
                    using (var response = wr.GetResponse())
                    using (var respStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        _diffData = respStream.ReadToEnd();
                    }
                }
                return _diffData;
            }
        }

        public string Title { get; private set; }
        public string Body { get; private set; }
        public IHostedGitRepo BaseRepo { get; private set; }
        public IHostedGitRepo HeadRepo { get; private set; }
        public string BaseSha { get; private set; }
        public string HeadSha { get; private set; }
        public string BaseRef { get; private set; }
        public string HeadRef { get; private set; }


        public void Close()
        {
            var issueApi = _plugin.GetIssuesApi();
            issueApi.Close(RepositoryName, IssueOwner, int.Parse(Id));
            _plugin.InvalidateCache();
        }

        PullRequestDiscussion _discussion;
        public IPullRequestDiscussion Discussion
        {
            get 
            {
                if (_discussion == null)
                    _discussion = new PullRequestDiscussion(_plugin, this, IssueOwner, RepositoryName, Id);
                return _discussion;
            }
        }

        public override bool Equals(object obj)
        {
            GithubPullRequestInformation pri = obj as GithubPullRequestInformation;
            return pri != null && Owner == pri.Owner && RepositoryName == pri.RepositoryName && Id == pri.Id;
        }

        public override int GetHashCode()
        {
            return (Owner ?? "").GetHashCode() + (RepositoryName ?? "").GetHashCode() + (Id ?? "").GetHashCode();
        }
    }
}
