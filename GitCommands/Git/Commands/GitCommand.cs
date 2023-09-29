using GitExtUtils;
using GitUIPluginInterfaces;

namespace GitCommands.Git.Commands
{
    /// <summary>
    /// Base class for structured git command.
    /// </summary>
    /// Here we can introduce methods which can operate on command structure instead of command string.
    public class GitCommand : IGitCommand
    {
        /// <value>Gets whether this command accesses a remote repository.</value>
        public bool AccessesRemote { get; }

        /// <value>The arguments for the git command.</value>
        public string Arguments { get; }

        /// <value>Gets whether executing this command will change the repo state.</value>
        public bool ChangesRepoState { get; }

        public GitCommand(bool accessesRemote, bool changesRepoState, GitArgumentBuilder arguments)
        {
            AccessesRemote = accessesRemote;
            Arguments = arguments.ToString();
            ChangesRepoState = changesRepoState;
        }

        public override string ToString() => Arguments;
    }
}
