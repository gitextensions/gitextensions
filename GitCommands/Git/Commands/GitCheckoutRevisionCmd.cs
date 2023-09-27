using GitExtUtils;
using GitUIPluginInterfaces;

namespace GitCommands.Git.Commands
{
    public sealed class GitCheckoutRevisionCmd : GitCommand
    {
        public GitCheckoutRevisionCmd(
            ObjectId revision,
            LocalChangesAction localChanges = LocalChangesAction.DontChange)
        {
            Revision = revision;
            LocalChanges = localChanges == LocalChangesAction.Stash ? LocalChangesAction.DontChange : localChanges;
        }

        public override bool AccessesRemote => false;
        public override bool ChangesRepoState => true;
        public LocalChangesAction LocalChanges { get; }
        public ObjectId Revision { get; }

        protected override ArgumentString BuildArguments()
        {
            return new GitArgumentBuilder("checkout")
            {
                { LocalChanges == LocalChangesAction.Merge, "--merge" },
                { LocalChanges == LocalChangesAction.Reset, "--force" },
                Revision.ToString().QuoteNE()
            };
        }
    }
}
