using GitUIPluginInterfaces;

namespace GitCommands
{
    /// <summary>
    /// Base class for structured git command
    /// here we can introduce methods which can operate on command structure
    /// instead of command string
    /// </summary>
    public abstract class GitCommand : IGitCommand
    {
        public string Arguments
        {
            get
            {
                Validate();
                return BuildArguments().ToString();
            }
        }

        protected abstract ArgumentString BuildArguments();

        /// <value>Gets whether this command accesses a remote repository.</value>
        public abstract bool AccessesRemote { get; }

        /// <value>Gets whether executing this command will change the repo state.</value>
        public abstract bool ChangesRepoState { get; }

        public override string ToString() => BuildArguments().ToString();

        /// <summary>
        /// Validates if the supplied arguments are correct.
        /// </summary>
        public virtual void Validate()
        {
        }
    }
}
