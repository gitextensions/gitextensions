using System;
using System.Collections.Generic;
using System.Linq;

namespace GitCommands.Git
{
    public sealed class GitDeleteRemoteBranchesCmd : GitCommand
    {
        private readonly string _remote;
        private readonly List<string> _branches;

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

        public override string GitComandName()
        {
            return "push";
        }

        protected override IEnumerable<string> CollectArguments()
        {
            yield return _remote;

            foreach (var branch in _branches)
            {
                yield return " :\"" + branch + "\"";
            }
        }

        public override bool AccessesRemote()
        {
            return true;
        }

        public override bool ChangesRepoState()
        {
            return true;
        }
    }
}
