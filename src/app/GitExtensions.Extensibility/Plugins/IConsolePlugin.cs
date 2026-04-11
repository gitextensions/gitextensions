namespace GitExtensions.Extensibility.Plugins;

/// <summary>
/// A plugin that provides a custom console output control to replace the built-in terminal
/// </summary>
public interface IConsolePlugin : IGitPlugin
{
    /// <summary>
    /// Returns <c>true</c> when this plugin can run in the current environment
    /// (e.g. required executables are present).
    /// </summary>
    bool IsSupportedInCurrentEnvironment { get; }

    /// <summary>
    /// Creates a new <see cref="IConsoleProcessController"/> instance for embedding in FormProcess / FormStatus.
    /// </summary>
    IConsoleProcessController CreateConsoleProcessController();

    /// <summary>
    /// Creates an interactive terminal control for the repository browser's terminal tab.
    /// </summary>
    IConsoleShellController CreateConsoleShellController();
}
