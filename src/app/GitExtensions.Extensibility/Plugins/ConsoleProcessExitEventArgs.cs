namespace GitExtensions.Extensibility.Plugins;

public sealed class ConsoleProcessExitEventArgs : EventArgs
{
    public int ExitCode { get; }

    public ConsoleProcessExitEventArgs(int exitCode)
    {
        ExitCode = exitCode;
    }
}
