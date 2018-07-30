using Git.hub;
using GitUIPluginInterfaces.RepositoryHosts;

namespace GitHub3
{
    internal class GitHubBranch : IHostedBranch
    {
        private readonly Branch _branch;

        public GitHubBranch(Branch branch)
        {
            _branch = branch;
        }

        public string Name => _branch.Name;
        public string Sha => _branch.Commit.Sha;
    }
}
