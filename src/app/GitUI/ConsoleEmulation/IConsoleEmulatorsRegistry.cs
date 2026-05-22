namespace GitUI.ConsoleEmulation;

/// <summary>
///   Registry with known console emulators.
/// </summary>
public interface IConsoleEmulatorsRegistry
{
    /// <summary>
    ///  Creates a console command runner for the configured emulator.
    /// </summary>
    IConsoleCommandRunner CreateCommandController();

    /// <summary>
    ///  Creates a console shell runner for the configured emulator, if available.
    /// </summary>
    IConsoleShellRunner? CreateShellRunner();

    /// <summary>
    ///  Gets the console emulators supported in the current environment.
    /// </summary>
    IReadOnlyCollection<IConsoleEmulator> AvailableConsoleEmulators { get; }
}
