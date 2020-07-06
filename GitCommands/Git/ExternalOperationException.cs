using System;

namespace GitCommands
{
    /// <summary>
    /// Exception class for failures of external operations,
    /// e.g. file access or start of other executables, which always can fail due to removed or locked files.
    /// </summary>
    public class ExternalOperationException : Exception
    {
        public ExternalOperationException(string context, string command, string workingDirectory, Exception inner)
            : base(inner?.Message, inner)
        {
            Context = context;
            Command = command;
            WorkingDirectory = workingDirectory;
        }

        public string Context { get; }
        public string Command { get; }
        public string WorkingDirectory { get; }

        /// <summary>
        /// Flag whether this exception has already been handled, e.g. shown to the user,
        /// and shall not be reported as a bug.
        /// </summary>
        public bool Handled { get; set; } = false;
    }
}
