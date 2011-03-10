using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces.RepositoryHosts;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace Github
{
    class PullRequestDiscussion : IPullRequestDiscussion
    {
        GithubPlugin _plugin;
        private string _owner;
        private string _repositoryName;
        private string _id;
        private GithubSharp.Core.Models.PullRequest _pullRequest;
        private GithubPullRequestInformation _githubPullReqInfo;

        public PullRequestDiscussion(GithubPlugin plugin, GithubPullRequestInformation githubPullReqInfo, string owner, string repositoryName, string id)
        {
            _githubPullReqInfo = githubPullReqInfo;
            _plugin = plugin;
            _owner = owner;
            _repositoryName = repositoryName;
            _id = id;

            TryLoad();
        }

        private void TryLoad()
        {
            try
            {
                Load();
            }
            catch (Exception ex)
            {
                Entries = new List<IDiscussionEntry>();
                throw;
            }
        }

        private void Load()
        {
            var pullRequestApi = _plugin.GetPullRequestApi();
            _pullRequest = pullRequestApi.GetById(_owner, _repositoryName, _id);

            Entries = new List<IDiscussionEntry>();

            GithubDiscussionEntry da = new GithubDiscussionEntry(_githubPullReqInfo.Owner, _githubPullReqInfo.Created, _githubPullReqInfo.Body);
            Entries.Add(da);

            foreach (var el in _pullRequest.Discussion)
            {
                GithubDiscussionEntry de;
                string author = GetAuthorFrom(el.User) ?? GetAuthorFrom(el.Author) ?? "!UNKNOWN!";

                if (el.Type.ToLowerInvariant() == "commit")
                    de = new GithubCommitDiscussionEntry(author, el.Created, el.Message, el.Id);
                else if (el.Type.ToLowerInvariant() == "issuecomment")
                    de = new GithubDiscussionEntry(author, el.Created, el.Body);
                else
                    de = new GithubDiscussionEntry("ERROR", DateTime.Now, "COULD NOT UNDERSTAND A DISCUSSION ENTRY");
                Entries.Add(de);
            }
        }

        private static string GetAuthorFrom(GithubSharp.Core.Models.IssueUser issueUser)
        {
            if (issueUser == null)
                return null;
            string namePart = null;
            string p2 = null;

            if (!string.IsNullOrEmpty(issueUser.Login))
            {
                namePart = issueUser.Login;
            }
            else if (!string.IsNullOrEmpty(issueUser.Name))
            {
                namePart = issueUser.Name;
                p2 = issueUser.Email;
            }
            else if (!string.IsNullOrEmpty(issueUser.Email))
                namePart = issueUser.Email;
            else
                return null;

            return p2 != null ? string.Format("{0} ({1})", namePart, p2) : namePart;
        }

        public void ForceReload()
        {
            _plugin.InvalidateCache();
            Load();
        }

        public List<IDiscussionEntry> Entries { get; private set; }

        public void Post(string data)
        {
            var issuesApi = _plugin.GetIssuesApi();
            if (!issuesApi.CommentOnIssue(_repositoryName, _owner, int.Parse(_id), data))
                throw new InvalidOperationException("Failed to post comment. Log: " + _plugin.GetLoggerData());
            _plugin.InvalidateCache();
        }
    }

    class GithubDiscussionEntry : IDiscussionEntry
    {
        public GithubDiscussionEntry(string author, DateTime created, string body)
        {
            Author = author;
            Created = created;
            Body = body;
        }

        public string Author { get; private set; }
        public DateTime Created { get; private set; }
        public string Body { get; private set; }
    }

    class GithubCommitDiscussionEntry : GithubDiscussionEntry, ICommitDiscussionEntry
    {
        public GithubCommitDiscussionEntry(string author, DateTime created, string body, string sha)
            : base(author, created, body)
        {
            if (sha == null)
                throw new ArgumentNullException("Sha can not be null!", "sha");
            Sha = sha;
        }

        public string Sha { get; private set; }
    }
}
