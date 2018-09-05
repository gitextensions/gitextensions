using Git.hub;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;

namespace GitHub3
{
    internal sealed class GitHubBranch : IHostedBranch
    {
        public GitHubBranch(Branch branch)
        {
            Name = branch.Name;
            Sha = ObjectId.Parse(branch.Commit.Sha);
        }

        public string Name { get; }
        public ObjectId Sha { get; }
    }
}
