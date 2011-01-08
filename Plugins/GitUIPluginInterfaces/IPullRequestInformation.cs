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
    }
}
