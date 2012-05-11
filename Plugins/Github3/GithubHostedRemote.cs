using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces.RepositoryHosts;

namespace Github3
{
    class GithubHostedRemote : IHostedRemote
    {
        public IHostedRepository GetHostedRepository()
        {
            return new GithubRepo(Github3.github.getRepository(Owner, Name));
        }

        /// <summary>
        /// Local name of the remote, 'origin'
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Owner of the remote repository, in
        /// git@github.com:mabako/Git.hub.git this is 'mabako'
        /// </summary>
        public string Owner { get; internal set; }

        /// <summary>
        /// Name of the remote repository, in
        /// git@github.com:mabako/Git.hub.git this is 'Git.hub'
        /// </summary>
        public string RemoteRepositoryName { get; internal set; }

        public string Data { get { return Owner + "/" + RemoteRepositoryName; } }
        public string DisplayData { get { return Data; } }
        public bool IsOwnedByMe { get { return GithubLoginInfo.username == Owner; } }
    }
}
