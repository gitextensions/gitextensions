using GitUIPluginInterfaces.RepositoryHosts;

namespace Github3
{
    class GithubHostedRemote : IHostedRemote
    {
        private GithubRepo repo;
        public GithubHostedRemote(string Name, string Owner, string RemoteRepositoryName)
        {
            this.Name = Name;
            this.Owner = Owner;
            this.RemoteRepositoryName = RemoteRepositoryName;
        }

        public IHostedRepository GetHostedRepository()
        {
            if(repo == null)
                repo = new GithubRepo(Github3Plugin.github.getRepository(Owner, RemoteRepositoryName));

            return repo;
        }

        /// <summary>
        /// Local name of the remote, 'origin'
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Owner of the remote repository, in
        /// git@github.com:mabako/Git.hub.git this is 'mabako'
        /// </summary>
        public string Owner { get; private set; }

        /// <summary>
        /// Name of the remote repository, in
        /// git@github.com:mabako/Git.hub.git this is 'Git.hub'
        /// </summary>
        public string RemoteRepositoryName { get; private set; }

        public string Data { get { return Owner + "/" + RemoteRepositoryName; } }
        public string DisplayData { get { return Data; } }
        public bool IsOwnedByMe { get { return GithubLoginInfo.username == Owner; } }
    }
}
