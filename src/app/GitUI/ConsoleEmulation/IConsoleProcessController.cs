namespace GitUI.ConsoleEmulation;

/// <summary>Represents a control that executes a console process and displays its output.</summary>
public interface IConsoleProcessController
{
    /// <summary>Gets the WinForms control to embed in the panel.</summary>
    Control Control { get; }

    /// <summary>
    ///  Gets a value indicating whether this control renders the full process output, so callers
    ///  do not need to duplicate selected lines or progress in the window title.
    /// </summary>
    bool IsDisplayingFullProcessOutput { get; }

    /// <summary>Occurs when the process writes output.</summary>
    event EventHandler<ConsoleOutputEventArgs> ProcessOutputReceived;

    /// <summary>Occurs when the target process exits.</summary>
    event EventHandler<ConsoleProcessExitEventArgs> ProcessExited;

    /// <summary>Occurs when the console host terminates independently of the command it runs.</summary>
    event EventHandler? ConsoleHostTerminated;

    /// <summary>Terminates the running process.</summary>
    void KillProcess();

    /// <summary>Resets the console and terminates any running target process.</summary>
    void ResetConsole();

    /// <summary>Starts a new process inside the console.</summary>
    void StartProcess(string command, string arguments, string workDir, Dictionary<string, string> envVariables);

    /// <summary>Writes text directly to the console host output.</summary>
    void WriteConsoleOutput(string text);

    /// <summary>Writes text to the process input stream.</summary>
    void WriteProcessInput(string text);
}
