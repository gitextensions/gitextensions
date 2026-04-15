namespace GitUI.ConsoleEmulation;

/// <summary>
///  Represents a control that executes a console command and displays its output.
/// </summary>
public interface IConsoleCommandRunner
{
    /// <summary>
    ///  Gets the WinForms control to embed in the panel.
    /// </summary>
    Control Control { get; }

    /// <summary>
    ///  Gets a value indicating whether this control renders the full process output, so callers
    ///  do not need to duplicate selected lines or progress in the window title.
    /// </summary>
    bool IsDisplayingFullProcessOutput { get; }

    /// <summary>
    ///  Occurs when the process writes output.
    /// </summary>
    event EventHandler<ConsoleOutputEventArgs> CommandOutputReceived;

    /// <summary>
    ///  Occurs when the command process exits.
    /// </summary>
    event EventHandler<ConsoleProcessExitEventArgs> CommandProcessExited;

    /// <summary>
    ///  Occurs when the console host terminates independently of the command it runs.
    /// </summary>
    event EventHandler? ConsoleHostTerminated;

    /// <summary>
    ///  Terminates the running process.
    /// </summary>
    void KillCommandProcess();

    /// <summary>
    ///  Resets the console and terminates any running target process.
    /// </summary>
    void ResetConsole();

    /// <summary>
    ///  Starts a new command process inside the console.
    /// </summary>
    void StartCommand(string command, string arguments, string workDir, Dictionary<string, string> envVariables);

    /// <summary>
    ///  Writes text directly to the console host output.
    /// </summary>
    void WriteConsoleOutput(string text);

    /// <summary>
    ///  Writes text to the process input stream.
    /// </summary>
    void WriteCommandProcessInput(string text);
}
