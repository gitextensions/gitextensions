namespace GitExtensions.Extensibility.Plugins;

/// <summary>
/// Represents an interactive terminal session embedded in the repository browser's terminal tab.
/// </summary>
public interface IConsoleShellController
{
    /// <summary>The underlying WinForms control to embed in the tab panel.</summary>
    Control Control { get; }

    /// <summary>Returns true while the shell running inside the terminal has not yet exited.</summary>
    bool IsShellRunning { get; }

    /// <summary>Starts (or restarts) an interactive shell in the terminal for <paramref name="workDir"/>.</summary>
    void StartShell(string workDir);

    /// <summary>
    /// Changes current working directory.
    /// </summary>
    void ChangeWorkingDirectory(string path);

    /// <summary>Focuses the terminal window.</summary>
    void FocusTerminal();
}
