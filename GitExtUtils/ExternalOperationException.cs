using System;

namespace GitExtUtils
{
    /// <summary>
    /// Represents errors that occur during execution of an external operation,
    /// e.g. running a git operation or launching an external process.
    /// </summary>
    public class ExternalOperationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalOperationException"/> class with a specified parameters
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="operation">The external operation that led to the exception.</param>
        /// <param name="obj">The object the external operation was applied to.</param>
        /// <param name="arguments">Further arguments of the external operation.</param>
        /// <param name="directory">The directory where the external operation was attempted to execute.
        /// Either the working directory or the path, e.g. if a full path was given to a filesystem operation.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ExternalOperationException(string? operation = null, string? obj = null, string? arguments = null, string? directory = null, Exception? innerException = null)
            : base(innerException?.Message, innerException)
        {
            Operation = operation;
            Object = obj;
            Arguments = arguments;
            Directory = directory;
        }

        /// <summary>
        /// The external operation that led to the exception.
        /// </summary>
        public string? Operation { get; }

        /// <summary>
        /// The object the external operation was applied to.
        /// </summary>
        public string? Object { get; }

        /// <summary>
        /// Further arguments of the external operation.
        /// </summary>
        public string? Arguments { get; }

        /// <summary>
        /// The directory where the external operation was attempted to execute.
        /// Either the working directory or the path, e.g. if a full path was given to a filesystem operation.
        /// </summary>
        public string? Directory { get; }
    }
}
