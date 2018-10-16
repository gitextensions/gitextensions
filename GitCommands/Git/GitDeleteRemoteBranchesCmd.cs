using System;
using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    public sealed class GitDeleteRemoteBranchesCmd : GitCommand
    {
        private readonly List<string> _branches;
        private readonly string _remote;

        public GitDeleteRemoteBranchesCmd(string remote, IEnumerable<string> branchLocalNames)
        {
            if (string.IsNullOrEmpty(remote))
            {
                throw new ArgumentNullException(nameof(remote));
            }

            if (branchLocalNames == null)
            {
                throw new ArgumentNullException(nameof(branchLocalNames));
            }

            _remote = remote;
            _branches = branchLocalNames.ToList();
        }

        public override bool AccessesRemote => true;
        public override bool ChangesRepoState => true;

        protected override ArgumentString BuildArguments()
        {
            return new GitArgumentBuilder("push")
            {
                _remote,
                _branches.Select(branch => $":refs/heads/{branch.Quote()}")
            };
        }
    }
}
