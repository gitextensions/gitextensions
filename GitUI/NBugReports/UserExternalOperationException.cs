using GitExtUtils;

namespace GitUI.NBugReports
{
    /// <summary>
    /// Represents errors that occur during execution of user-configured operation, e.g. a script.
    /// </summary>
    public class UserExternalOperationException : ExternalOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserExternalOperationException"/> class with the specified context information
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="context">The command that led to the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public UserExternalOperationException(string context, ExternalOperationException innerException)
            : base(innerException.Command, innerException.Arguments, innerException.WorkingDirectory, innerException.ExitCode, innerException.InnerException)
        {
            Context = context;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserExternalOperationException"/> class with
        /// a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public UserExternalOperationException(Exception innerException)
            : base(innerException: innerException)
        {
        }

        public string? Context { get; }
    }
}
