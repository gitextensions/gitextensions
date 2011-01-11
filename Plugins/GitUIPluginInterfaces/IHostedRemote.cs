using System;
using System.Collections.Generic;
using System.Text;

namespace GitUIPluginInterfaces
{
    public interface IHostedRemote
    {
        List<IPullRequestInformation> GetPullRequests();
        int CreatePullRequest(string myBranch, string remoteBranch, string title, string body);

        string Name { get; } //This is the name of the remote in the local git repository. This might be null
        string Data { get; }
        string DisplayData { get; }

        bool IsProbablyOwnedByMe { get; }
    }
}
