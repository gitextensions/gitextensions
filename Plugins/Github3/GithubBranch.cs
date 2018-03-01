using Git.hub;
using GitUIPluginInterfaces.RepositoryHosts;

namespace Github3
{
    class GithubBranch : IHostedBranch
    {
        private Branch branch;

        public GithubBranch(Branch branch)
        {
            this.branch = branch;
        }

        public string Name => branch.Name;
        public string Sha => branch.Commit.Sha;
    }
}
