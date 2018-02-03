using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCommands.Git
{
    public class GitDeleteRemoteBranchCmd : GitCommand
    {
        private readonly string _remote;
        private readonly string _branchName;

        public GitDeleteRemoteBranchCmd(string remote, string branchName)
        {
            _remote = remote;
            _branchName = branchName;
        }

        protected override IEnumerable<string> CollectArguments()
        {
            yield return "--delete";
            yield return _remote;
            yield return GitCommandHelpers.GetFullBranchName(_branchName);
        }

        public override string GitComandName()
        {
            return "push";
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
