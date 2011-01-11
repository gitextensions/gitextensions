using System;
using System.Collections.Generic;
using System.Text;

namespace GitUIPluginInterfaces
{
    public interface IHostedRemote
    {
        List<IPullRequestInformation> GetPullRequests();
        void CreatePullRequest(string title, string body);

        string Data { get; }
        string DisplayData { get; }

        bool IsProbablyOwnedByMe { get; }
    }
}
