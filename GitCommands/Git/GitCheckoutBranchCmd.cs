using System;
using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    public enum CheckoutNewBranchMode
    {
        DontCreate,
        Create,
        Reset
    }

    public sealed class GitCheckoutBranchCmd : GitCommand
    {
        public string BranchName { get; }
        public bool Remote { get; }
        public LocalChangesAction LocalChanges { get; }
        public string NewBranchName { get; }
        public CheckoutNewBranchMode NewBranchMode { get; }

        public GitCheckoutBranchCmd(
            string branchName,
            bool remote,
            LocalChangesAction localChanges = LocalChangesAction.DontChange,
            CheckoutNewBranchMode newBranchMode = CheckoutNewBranchMode.DontCreate,
            string newBranchName = null)
        {
            BranchName = branchName;
            Remote = remote;
            LocalChanges = localChanges == LocalChangesAction.Stash ? LocalChangesAction.DontChange : localChanges;
            NewBranchMode = newBranchMode;
            NewBranchName = newBranchName;
        }

        public override bool AccessesRemote => false;
        public override bool ChangesRepoState => true;

        protected override ArgumentString BuildArguments()
        {
            return new GitArgumentBuilder("checkout")
            {
                { LocalChanges == LocalChangesAction.Merge, "--merge" },
                { LocalChanges == LocalChangesAction.Reset, "--force" },
                { Remote && NewBranchMode == CheckoutNewBranchMode.Create, $"-b {NewBranchName.Quote()}" },
                { Remote && NewBranchMode == CheckoutNewBranchMode.Reset, $"-B {NewBranchName.Quote()}" },
                BranchName.QuoteNE()
            };
        }
    }
}
