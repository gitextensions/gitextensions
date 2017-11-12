using System;
using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public sealed class GitDeleteRemoteBranchesCmd : GitCommand
    {
        private readonly string remote;
        private readonly List<IGitRef> branches;

        public GitDeleteRemoteBranchesCmd(string remote, IEnumerable<IGitRef> branches)
        {
            if (string.IsNullOrEmpty(remote))
                throw new ArgumentNullException("remote");

            if (branches == null)
                throw new ArgumentNullException("branches");

            this.remote = remote;
            this.branches = branches.ToList();

            if (this.branches.Any(b => b.Remote != this.remote))
            {
                throw new ArgumentException($"Branch remote mismatch. Branch {this.branches.First(b => b.Remote != this.remote).CompleteName} does not belong to remote {remote}");
            }
        }

        public override string GitComandName()
        {
            return "push";
        }

        protected override IEnumerable<string> CollectArguments()
        {
            yield return remote;

            foreach (var branch in branches)
            {
                yield return " :\"" + branch.LocalName + "\"";
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
