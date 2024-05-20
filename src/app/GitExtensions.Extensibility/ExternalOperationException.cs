namespace GitExtensions.Extensibility;

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
    /// <param name="command">The command that led to the exception.</param>
    /// <param name="arguments">The command arguments.</param>
    /// <param name="workingDirectory">The working directory.</param>
    /// <param name="exitCode">The exit code of an executed process.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ExternalOperationException(
        string? command = null,
        string? arguments = null,
        string? workingDirectory = null,
        int? exitCode = null,
        Exception? innerException = null)
        : base(innerException?.Message, innerException)
    {
        Command = command;
        Arguments = arguments;
        WorkingDirectory = workingDirectory;
        ExitCode = exitCode;
    }

    /// <summary>
    /// The command that led to the exception.
    /// </summary>
    public string? Command { get; }

    /// <summary>
    /// The command arguments.
    /// </summary>
    public string? Arguments { get; }

    /// <summary>
    /// The working directory.
    /// </summary>
    public string? WorkingDirectory { get; }

    /// <summary>
    /// The exit code of an executed process.
    /// </summary>
    public int? ExitCode { get; }
}
