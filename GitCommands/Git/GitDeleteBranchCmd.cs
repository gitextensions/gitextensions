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
            if (branches == null)
            {
                throw new ArgumentNullException(nameof(branches));
            }

            _branches = branches;
            _force = force;
        }

        public override string GitComandName()
        {
            return "branch";
        }

        protected override IEnumerable<string> CollectArguments()
        {
            yield return _force ? "-D" : "-d";

            var hasRemoteBranch = _branches.Any(branch => branch.IsRemote);
            var hasNonRemoteBranch = _branches.Any(branch => !branch.IsRemote);

            if (hasRemoteBranch)
            {
                yield return hasNonRemoteBranch ? "-a" : "-r";
            }

            foreach (var branch in _branches)
            {
                yield return "\"" + branch.Name + "\"";
            }
        }

        public override bool AccessesRemote()
        {
            return false;
        }

        public override bool ChangesRepoState()
        {
            return true;
        }
    }
}
