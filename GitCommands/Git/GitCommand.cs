using GitUIPluginInterfaces;
using System.Collections.Generic;
using System.Linq;

namespace GitCommands
{

    /// <summary>
    /// Base class for structured git command
    /// here we can introduce methods which can operate on command structure
    /// instead of command string
    /// </summary>
    public abstract class GitCommand : IGitCommand
    {
        /// <summary>
        /// here commands should add theirs arguments
        /// </summary>
        protected abstract IEnumerable<string> CollectArguments();

        /// <summary>
        ///
        /// </summary>
        /// <returns>name of git command eg. push, pull</returns>
        public abstract string GitComandName();

        /// <summary>
        ///
        /// </summary>
        /// <returns>if command accesses remote repository</returns>
        public abstract bool AccessesRemote();

        /// <summary>
        ///
        /// </summary>
        /// <returns>true if repo state changes after executing this command</returns>
        public abstract bool ChangesRepoState();

        /// <summary>
        ///
        /// </summary>
        /// <returns>git command arguments as single line</returns>
        public virtual string ToLine()
        {
            Validate();
            return GitComandName() + " " + CollectArguments().Join(" ");
        }

        public override string ToString()
        {
            return GitComandName() + " " + CollectArguments().Join(" ");
        }

        /// <summary>
        /// Validates if the supplied arguments are correct
        /// </summary>
        public virtual void Validate()
        {

        }

    }
}
