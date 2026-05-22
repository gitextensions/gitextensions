namespace GitUI.ConsoleEmulation;

/// <summary>
///  Represents a console emulator integration.
/// </summary>
public interface IConsoleEmulator
{
    /// <summary>
    ///  Gets the emulator name.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///  Gets the emulator name shown in the UI.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    ///  Gets a value indicating whether this emulator can run in the current environment
    ///  (for example, because its required executables are present).
    /// </summary>
    bool IsSupportedInCurrentEnvironment { get; }

    /// <summary>
    ///  Creates a console process runner for command dialogs.
    /// </summary>
    IConsoleCommandRunner CreateCommandRunner();

    /// <summary>
    ///  Creates a console shell runner for the repository browser's terminal tab.
    /// </summary>
    IConsoleShellRunner CreateShellRunner();
}
