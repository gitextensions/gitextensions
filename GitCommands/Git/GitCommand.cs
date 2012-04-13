using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCommands
{

    /// <summary>
    /// Base class for structured git command
    /// here we can introduce methods which can operate on command structure
    /// instead of command string
    /// </summary>
    public abstract class GitCommand
    {
        /// <summary>
        /// here commands should add theirs arguments
        /// </summary>
        /// <param name="argumentsList"></param>
        public abstract void CollectArguments(List<string> argumentsList);

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
        /// <returns>git command arguments as single line</returns>
        public virtual string ToLine()
        {
            List<string> argumentsList = new List<string>();
            CollectArguments(argumentsList);
            String args = null;
            args = string.Join(" ", argumentsList);
            return string.Join(" ", GitComandName(), args);           
        }

        public override string ToString()
        {
            return ToLine();
        }

    }
}
