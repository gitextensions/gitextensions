using System;

namespace GitCommands
{
    public class ExternalOperationExceptionFactory
    {
        public enum Handling
        {
            OptionalBugReport,
            Show,
            None
        }

        /// <summary>
        /// Singleton accessor.
        /// </summary>
        public static ExternalOperationExceptionFactory Default { get; } = new ExternalOperationExceptionFactory();

        public event Action<ExternalOperationException, Handling> OnException;

        public ExternalOperationException Create(string context, string command, string workingDirectory, Exception inner, Handling handling = Handling.OptionalBugReport)
        {
            ExternalOperationException ex = new ExternalOperationException(context, command, workingDirectory, inner);
            OnException?.Invoke(ex, handling);
            return ex;
        }
    }
}
