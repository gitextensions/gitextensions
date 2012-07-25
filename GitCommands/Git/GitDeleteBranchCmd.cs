using System;
using System.Collections.Generic;
using System.Linq;

namespace GitCommands
{
    public sealed class GitDeleteBranchCmd : GitCommand
    {
        private readonly ICollection<GitHead> branches;
        private readonly bool force;

        public GitDeleteBranchCmd(IEnumerable<GitHead> branches, bool force)
        {
            if (branches == null)
                throw new ArgumentNullException("branches");

            this.branches = branches.ToArray();
            this.force = force;
        }

        protected override string GitComandName()
        {
            return "branch";
        }

        protected override IEnumerable<string> CollectArguments()
        {
            yield return force ? "-D" : "-d";

            var hasRemoteBranch = branches.Any(branch => branch.IsRemote);
            var hasNonRemoteBranch = branches.Any(branch => !branch.IsRemote);
            if (hasRemoteBranch)
                yield return hasNonRemoteBranch ? "-a" : "-r";

            foreach (var branch in branches)
                yield return "\"" + branch.Name + "\"";
        }

        public override bool AccessesRemote()
        {
            return false;
        }
    }
}
