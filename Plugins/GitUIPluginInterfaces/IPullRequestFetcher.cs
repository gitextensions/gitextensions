using System;
using System.Collections.Generic;
using System.Text;

namespace GitUIPluginInterfaces
{
    public interface IPullRequestsFetcher
    {
        List<IPullRequestInformation> Fetch();
        string Data { get; }
        string DisplayData { get; }
    }
}
