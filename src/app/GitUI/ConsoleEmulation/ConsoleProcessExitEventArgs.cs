namespace GitUI.ConsoleEmulation;

/// <summary>
///  Represents the exit of a console process.
/// </summary>
public sealed class ConsoleProcessExitEventArgs(int exitCode) : EventArgs
{
    /// <summary>
    ///  Gets the process exit code.
    /// </summary>
    public int ExitCode { get; } = exitCode;
}
