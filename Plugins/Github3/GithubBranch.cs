using Git.hub;
using GitUIPluginInterfaces.RepositoryHosts;

namespace Github3
{
    internal class GithubBranch : IHostedBranch
    {
        private readonly Branch _branch;

        public GithubBranch(Branch branch)
        {
            _branch = branch;
        }

        public string Name => _branch.Name;
        public string Sha => _branch.Commit.Sha;
    }
}
