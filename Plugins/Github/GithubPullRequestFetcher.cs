using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces;
using ghApi=GithubSharp.Core.API;

namespace Github
{
    class GithubPullRequestFetcher : IPullRequestsFetcher
    {
        GithubPlugin _plugin;
        public GithubPullRequestFetcher(GithubRepositoryInformation info, GithubPlugin plugin)
        {
            _plugin = plugin;
            if (string.IsNullOrEmpty(info.Name) || string.IsNullOrEmpty(info.Owner))
                throw new ArgumentException("Neither Name or Owner can be null");
            Data = info.Name + "/" + info.Owner;
        }

        public List<IPullRequestInformation> Fetch()
        {
            var api = _plugin.GetPullRequestApi();
            var data = api.List(Owner, Name);
            if (data == null)
                throw new InvalidOperationException("Could not fetch data!" + _plugin.GetLoggerData());

            return (from el in data select (IPullRequestInformation)new GithubPullRequestInformation(Owner, Name, el, _plugin)).ToList();
        }

        private string Owner { get { return Data.Split('/')[0]; } }
        private string Name { get { return Data.Split('/')[1]; } }

        public string Data { get; private set; }

        public string DisplayData
        {
            get { return Data; }
        }
    }
}
