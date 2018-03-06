using System;
using System.Collections.Generic;

namespace GitCommands.Git
{
    public class GitCheckoutBranchCmd : GitCommand
    {
        public enum NewBranch
        {
            DontCreate,
            Create,
            Reset
        }

        public string BranchName { get; set; }
        public string NewBranchName { get; set; }
        public bool Remote { get; set; }
        private LocalChangesAction _localChanges;
        public LocalChangesAction LocalChanges
        {
            get => _localChanges;
            set
            {
                if (value == LocalChangesAction.Stash)
                {
                    _localChanges = LocalChangesAction.DontChange;
                }
                else
                {
                    _localChanges = value;
                }
            }
        }

        public NewBranch NewBranchAction { get; set; }

        public GitCheckoutBranchCmd(string branchName, bool remote)
        {
            BranchName = branchName;
            Remote = remote;
        }

        public override string GitComandName()
        {
            return "checkout";
        }

        protected override IEnumerable<string> CollectArguments()
        {
            if (LocalChanges == LocalChangesAction.Merge)
            {
                yield return "--merge";
            }
            else if (LocalChanges == LocalChangesAction.Reset)
            {
                yield return "--force";
            }

            if (Remote)
            {
                if (NewBranchAction == NewBranch.Create)
                {
                    yield return "-b " + NewBranchName.Quote();
                }
                else if (NewBranchAction == NewBranch.Reset)
                {
                    yield return "-B " + NewBranchName.Quote();
                }
            }

            yield return BranchName.QuoteNE();
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
