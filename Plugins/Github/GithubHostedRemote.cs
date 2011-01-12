using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces.RepositoryHosts;
using ghApi=GithubSharp.Core.API;

namespace Github
{
    class GithubHostedRemote : IHostedRemote
    {
        GithubPlugin _plugin;
        public GithubHostedRemote(GithubHostedRemoteInformation info, GithubPlugin plugin)
        {
            _plugin = plugin;
            if (string.IsNullOrEmpty(info.NameAtGithub) || string.IsNullOrEmpty(info.Owner))
                throw new ArgumentException("Neither NameAtGithub or Owner can be null");
            Data = info.Owner + "/" + info.NameAtGithub;
            Name = info.Name;
        }

        private string Owner { get { return Data.Split('/')[0]; } }
        private string NameAtRemote { get { return Data.Split('/')[1]; } }
        public string Data { get; private set; }
        public string Name { get; private set; }

        public string DisplayData
        {
            get { return Data; }
        }

        public bool IsOwnedByMe
        {
            get 
            {
                return Owner == _plugin.Auth.Username;
            }
        }

        public IHostedRepository GetHostedRepository()
        {
            return _plugin.GetRepository(Owner, NameAtRemote);
        }
    }
}
