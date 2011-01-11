using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces;
using ghApi=GithubSharp.Core.API;

namespace Github
{
    class GithubHostedRemote : IHostedRemote
    {
        GithubPlugin _plugin;
        public GithubHostedRemote(GithubHostedRemoteInformation info, GithubPlugin plugin)
        {
            _plugin = plugin;
            if (string.IsNullOrEmpty(info.Name) || string.IsNullOrEmpty(info.Owner))
                throw new ArgumentException("Neither Name or Owner can be null");
            Data = info.NameAtGithub + "/" + info.Owner;
            Name = info.Name;
        }

        public List<IPullRequestInformation> GetPullRequests()
        {
            var api = _plugin.GetPullRequestApi();
            var data = api.List(Owner, NameAtRemote);
            if (data == null)
                throw new InvalidOperationException("Could not fetch data!" + _plugin.GetLoggerData());

            return (from el in data select (IPullRequestInformation)new GithubPullRequestInformation(Owner, NameAtRemote, el, _plugin)).ToList();
        }

        private string Owner { get { return Data.Split('/')[0]; } }
        private string NameAtRemote { get { return Data.Split('/')[1]; } }
        public string Data { get; private set; }
        public string Name { get; private set; }

        public string DisplayData
        {
            get { return Data; }
        }

        public int CreatePullRequest(string myBranch, string remoteBranch, string title, string body)
        {
            var api = _plugin.GetPullRequestApi();
            var pr = api.Create(Owner, NameAtRemote, remoteBranch, myBranch, title, body);
            if (pr == null || pr.Number == 0)
                throw new InvalidOperationException("CreatePullRequest failed! \r\n" + _plugin.GetLoggerData());
            return pr.Number;
        }

        public bool IsProbablyOwnedByMe
        {
            get 
            {
                return Owner == _plugin.Auth.Username;
            }
        }
    }
}
