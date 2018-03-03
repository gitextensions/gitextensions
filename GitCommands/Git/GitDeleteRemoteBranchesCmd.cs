using System;
using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public sealed class GitDeleteRemoteBranchesCmd : GitCommand
    {
        private readonly string _remote;
        private readonly List<IGitRef> _branches;

        public GitDeleteRemoteBranchesCmd(string remote, IEnumerable<IGitRef> branches)
        {
            if (string.IsNullOrEmpty(remote))
                throw new ArgumentNullException(nameof(remote));

            if (branches == null)
                throw new ArgumentNullException(nameof(branches));

            _remote = remote;
            _branches = branches.ToList();

            if (_branches.Any(b => b.Remote != _remote))
            {
                throw new ArgumentException($"Branch remote mismatch. Branch {_branches.First(b => b.Remote != _remote).CompleteName} does not belong to remote {remote}");
            }
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
