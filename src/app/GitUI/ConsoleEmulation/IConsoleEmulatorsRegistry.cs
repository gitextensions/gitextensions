namespace GitUI.ConsoleEmulation;

/// <summary>
///   Registry with known console emulators.
/// </summary>
public interface IConsoleEmulatorsRegistry
{
    /// <summary>
    ///  Creates a console command controller for the configured emulator.
    /// </summary>
    IConsoleCommandController CreateCommandController();

    /// <summary>
    ///  Creates a console shell controller for the configured emulator, if available.
    /// </summary>
    IConsoleShellController? CreateShellController();

    /// <summary>
    ///  Gets the console emulators supported in the current environment.
    /// </summary>
    IReadOnlyCollection<IConsoleEmulator> AvailableConsoleEmulators { get; }
}
