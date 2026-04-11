namespace GitExtensions.Extensibility.Plugins;

/// <summary>
/// Interface for a control that executes a console process and displays its output.
/// </summary>
public interface IConsoleProcessController
{
    /// <summary>The underlying WinForms control to embed in the panel.</summary>
    Control Control { get; }

    /// <summary>
    /// Whether this output control accurately renders all of the process output, so there's
    /// no need to print select lines manually or duplicate progress in the title.
    /// </summary>
    bool IsDisplayingFullProcessOutput { get; }

    /// <summary>
    /// Fires when process prints output.
    /// </summary>
    event EventHandler<ConsoleTextEventArgs> ProcessOutputReceived;

    /// <summary>
    /// Fires when the target process exits.
    /// </summary>
    event EventHandler<ConsoleProcessExitEventArgs> ProcessExited;

    /// <summary>
    /// Fires when the output control (not the command it runs) terminates.
    /// </summary>
    event EventHandler? ConsoleHostTerminated;

    /// <summary>
    /// Resets and brings a fresh console.
    /// If there is a target process running, it will be killed.
    /// </summary>
    void ResetConsole();

    /// <summary>
    /// Start a new process inside the console.
    /// </summary>
    void StartProcess(string command, string arguments, string workDir, Dictionary<string, string> envVariables);

    /// <summary>
    /// Writes message directly to the Console Host Output.
    /// </summary>
    void WriteConsoleOutput(string text);

    /// <summary>
    /// Writes messages to the process input stream.
    /// </summary>
    void WriteProcessInput(string text);

    /// <summary>
    /// Force kill the process.
    /// </summary>
    void KillProcess();
}
