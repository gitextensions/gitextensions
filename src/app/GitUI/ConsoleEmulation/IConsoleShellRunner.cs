namespace GitUI.ConsoleEmulation;

/// <summary>
///  Represents an interactive terminal session embedded in the repository browser's terminal tab.
/// </summary>
public interface IConsoleShellRunner
{
    /// <summary>
    ///  Gets the WinForms control to embed in the tab panel.
    /// </summary>
    Control Control { get; }

    /// <summary>
    ///  Gets a value indicating whether the shell running inside the terminal is still active.
    /// </summary>
    bool IsShellRunning { get; }

    /// <summary>
    ///  Changes the current working directory.
    /// </summary>
    void ChangeWorkingDirectory(string path);

    /// <summary>
    ///  Focuses the terminal window.
    /// </summary>
    void FocusTerminal();

    /// <summary>
    ///  Starts or restarts an interactive shell in the terminal for <paramref name="workDir"/>.
    /// </summary>
    void StartShell(string workDir);
}
