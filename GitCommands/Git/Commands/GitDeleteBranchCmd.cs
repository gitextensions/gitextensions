using GitExtUtils;
using GitUIPluginInterfaces;

namespace GitCommands.Git.Commands
{
    public sealed class GitDeleteBranchCmd : GitCommand
    {
        private readonly IReadOnlyCollection<IGitRef> _branches;
        private readonly bool _force;

        public GitDeleteBranchCmd(IReadOnlyCollection<IGitRef> branches, bool force)
        {
            _branches = branches ?? throw new ArgumentNullException(nameof(branches));
            if (_branches.Count == 0)
            {
                throw new ArgumentException("At least one branch is required.", nameof(branches));
            }

            _force = force;
        }

        public override bool AccessesRemote => false;
        public override bool ChangesRepoState => true;

        protected override ArgumentString BuildArguments()
        {
            bool hasRemoteBranch = _branches.Any(branch => branch.IsRemote);
            bool hasNonRemoteBranch = _branches.Any(branch => !branch.IsRemote);

            return new GitArgumentBuilder("branch")
            {
                { "--delete" },
                { _force, "--force" },
                { hasRemoteBranch && hasNonRemoteBranch, "--all" },
                { hasRemoteBranch && !hasNonRemoteBranch, "--remotes" },
                _branches.Select(branch => branch.Name.Quote())
            };
        }
    }
}
