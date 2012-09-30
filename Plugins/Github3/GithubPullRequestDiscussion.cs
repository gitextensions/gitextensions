using System;
using System.Collections.Generic;
using System.Linq;
using Git.hub;
using GitUIPluginInterfaces.RepositoryHosts;

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
            pullrequest.ToIssue().CreateComment(data);
        }

        public void ForceReload()
        {
            Entries.Clear();

            foreach (var commit in pullrequest.GetCommits())
            {
                Entries.Add(new GithubDiscussionCommit { Sha = commit.Sha, Author = commit.AuthorName.Replace("<", "&lt;").Replace(">", "&gt;"), Created = commit.Commit.Author.Date, Body = commit.Commit.Message });
            }

            foreach (var comment in pullrequest.GetIssueComments())
            {
                Entries.Add(new GithubDiscussionComment { Author = comment.User.Login, Created = comment.CreatedAt, Body = comment.Body });
            }

            Entries = Entries.OrderBy(entry => entry.Created).ToList();

        }
    }

    class GithubDiscussionComment : IDiscussionEntry
    {
        public string Author { get; internal set; }
        public DateTime Created { get; internal set; }
        public string Body { get; internal set; }
    }

    class GithubDiscussionCommit : GithubDiscussionComment, ICommitDiscussionEntry
    {
        public string Sha { get; internal set; }
    }
}
