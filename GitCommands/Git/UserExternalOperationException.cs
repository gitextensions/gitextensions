using System;

#nullable enable

namespace GitCommands
{
    /// <summary>
    /// Represents errors that occur during execution of user-configured operation, e.g. a script.
    /// </summary>
    public class UserExternalOperationException : ExternalOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserExternalOperationException"/> class with a specified parameters
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="command">The command that led to the exception.</param>
        /// <param name="arguments">The command arguments.</param>
        /// <param name="workingDirectory">The working directory.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public UserExternalOperationException(string command, string arguments, string workingDirectory, Exception innerException)
            : base(command, arguments, workingDirectory, innerException)
        {
        }
    }
}
