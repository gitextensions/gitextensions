using System;

namespace GitCommands.Git
{
    public class RefsWarningException : GitException
    {
        public RefsWarningException(string message)
            : base(message)
        {
        }

        public RefsWarningException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}