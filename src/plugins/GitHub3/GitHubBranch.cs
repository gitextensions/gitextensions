using Git.hub;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;

namespace GitExtensions.Plugins.GitHub3
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
