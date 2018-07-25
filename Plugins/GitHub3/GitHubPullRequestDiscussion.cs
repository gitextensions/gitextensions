using System;
using System.Collections.Generic;
using System.Linq;
using Git.hub;
using GitUIPluginInterfaces.RepositoryHosts;

namespace GitHub3
{
    internal class GitHubPullRequestDiscussion : IPullRequestDiscussion
    {
        private readonly PullRequest _pullRequest;

        public GitHubPullRequestDiscussion(PullRequest pullRequest)
        {
            _pullRequest = pullRequest;
            Entries = new List<IDiscussionEntry>();
            ForceReload();
        }

        public List<IDiscussionEntry> Entries { get; private set; }

        public void Post(string data)
        {
            _pullRequest.ToIssue().CreateComment(data);
        }

        public void ForceReload()
        {
            Entries.Clear();

            foreach (var commit in _pullRequest.GetCommits())
            {
                Entries.Add(new GitHubDiscussionCommit { Sha = commit.Sha, Author = commit.AuthorName.Replace("<", "&lt;").Replace(">", "&gt;"), Created = commit.Commit.Author.Date, Body = commit.Commit.Message });
            }

            foreach (var comment in _pullRequest.GetIssueComments())
            {
                Entries.Add(new GitHubDiscussionComment { Author = comment.User.Login, Created = comment.CreatedAt, Body = comment.Body });
            }

            Entries = Entries.OrderBy(entry => entry.Created).ToList();
        }
    }

    internal class GitHubDiscussionComment : IDiscussionEntry
    {
        public string Author { get; internal set; }
        public DateTime Created { get; internal set; }
        public string Body { get; internal set; }
    }

    internal class GitHubDiscussionCommit : GitHubDiscussionComment, ICommitDiscussionEntry
    {
        public string Sha { get; internal set; }
    }
}
