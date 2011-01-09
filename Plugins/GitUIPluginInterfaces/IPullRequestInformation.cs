using System;
using System.Collections.Generic;
using System.Text;

namespace GitUIPluginInterfaces
{
    public interface IPullRequestInformation
    {
        string Title { get; }
        string Body { get; }
        string Owner { get; }
        DateTime Created { get; }

        string DiffData { get; }

        IHostedGitRepo BaseRepo { get; }
        IHostedGitRepo HeadRepo { get; }
        string BaseSha { get; }
        string HeadSha { get; }

        string BaseRef { get; }
        string HeadRef { get; }

        string Id { get; }
        string DetailedInfo { get; }

        void Close();

        IPullRequestDiscussion Discussion { get; }
    }

    public interface IDiscussionEntry
    {
        string Author { get; }
        DateTime Created { get; }
        string Body { get; }
    }

    public interface ICommitDiscussionEntry : IDiscussionEntry
    {
        string Sha { get; }
    }

    public interface IPullRequestDiscussion
    {
        List<IDiscussionEntry> Entries { get; }
        void Post(string data);
        void ForceReload();
    }
}
