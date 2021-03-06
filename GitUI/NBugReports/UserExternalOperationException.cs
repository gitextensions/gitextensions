using GitExtUtils;

namespace GitUI.NBugReports
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
        /// <param name="context">The command that led to the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public UserExternalOperationException(string? context, ExternalOperationException innerException)
            : base(innerException.Operation, innerException.Object, innerException.Arguments, innerException.Directory, innerException.InnerException)
        {
            Context = context;
        }

        public string? Context { get; }
    }
}
