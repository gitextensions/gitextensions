using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces.RepositoryHosts;
using Git.hub;

namespace Github3
{
    class GithubPullRequestDiscussion : IPullRequestDiscussion
    {
        private PullRequest pullrequest;

        public GithubPullRequestDiscussion(PullRequest pullrequest)
        {
            this.pullrequest = pullrequest;
            Entries = new List<IDiscussionEntry>();
            ForceReload();
        }

        public List<IDiscussionEntry> Entries { get; private set; }

        public void Post(string data)
        {
            throw new NotImplementedException();
        }

        public void ForceReload()
        {
            Entries.Clear();

            foreach (var commit in pullrequest.GetCommits())
            {
                Entries.Add(new GithubDiscussionCommit { Sha = commit.Sha, Author = commit.Author.Login, Created = commit.Commit.Author.Date, Body = commit.Commit.Message });
            }
        }
    }

    class GithubDiscussionCommit : ICommitDiscussionEntry
    {
        public string Sha { get; internal set; }
        public string Author { get; internal set; }
        public DateTime Created { get; internal set; }
        public string Body { get; internal set; }
    }
}
