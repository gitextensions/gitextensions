using System;
using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public sealed class GitDeleteBranchCmd : GitCommand
    {
        private readonly IReadOnlyCollection<IGitRef> _branches;
        private readonly bool _force;

        public GitDeleteBranchCmd(IReadOnlyCollection<IGitRef> branches, bool force)
        {
            _branches = branches ?? throw new ArgumentNullException(nameof(branches));
            _force = force;
        }

        public override bool AccessesRemote => false;
        public override bool ChangesRepoState => true;

        protected override ArgumentString BuildArguments()
        {
            var hasRemoteBranch = _branches.Any(branch => branch.IsRemote);
            var hasNonRemoteBranch = _branches.Any(branch => !branch.IsRemote);

            return new GitArgumentBuilder("branch")
            {
                { _force, "-D", "-d" },
                { hasRemoteBranch && hasNonRemoteBranch, "-a" },
                { hasRemoteBranch && !hasNonRemoteBranch, "-r" },
                _branches.Select(branch => branch.Name.Quote())
            };
        }
    }
}
