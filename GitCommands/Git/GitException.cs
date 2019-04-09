using System;

namespace GitCommands.Git
{
    public class GitException : Exception
    {
        public GitException(string message)
            : base(message)
        {
        }

        public GitException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}