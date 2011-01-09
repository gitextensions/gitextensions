using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces;

namespace Github
{
    class PullRequestDiscussion : IPullRequestDiscussion
    {
        GithubPlugin _plugin;
        private string _owner;
        private string _repositoryName;
        private string _id;
        private GithubSharp.Core.Models.PullRequest _pullRequest;

        public PullRequestDiscussion(GithubPlugin plugin, string owner, string repositoryName, string id)
        {
            _plugin = plugin;
            _owner = owner;
            _repositoryName = repositoryName;
            _id = id;

            Load();
        }

        private void Load()
        {
            var pullRequestApi = _plugin.GetPullRequestApi();
            _pullRequest = pullRequestApi.GetById(_owner, _repositoryName, _id);

            Entries = new List<IDiscussionEntry>();
            foreach (var el in _pullRequest.Discussion)
            {
                GithubDiscussionEntry de;
                string author;
                if (el.User == null)
                    author = string.Format("{0} ({1})", el.Author, el.Email);
                else
                    author = el.User.Login;


                if (el.Type.ToLowerInvariant() == "commit")
                    de = new GithubCommitDiscussionEntry(author, el.Created, el.Subject, el.Sha);
                else if (el.Type.ToLowerInvariant() == "issuecomment")
                    de = new GithubDiscussionEntry(author, el.Created, el.Body);
                else
                    de = new GithubDiscussionEntry("ERROR", DateTime.Now, "COULD NOT UNDERSTAND A DISCUSSION ENTRY");
                Entries.Add(de);
            }
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
        public GithubCommitDiscussionEntry(string author, DateTime created, string body, string sha) : base(author, created, body)
        {
            Sha = sha;
        }

        public string Sha { get; private set; }
    }
}
