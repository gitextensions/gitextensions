namespace GitUI.ConsoleEmulation;

internal interface IConsoleControllersFactory
{
    /// <summary>
    ///  Creates a console process controller for the configured emulator.
    /// </summary>
    IConsoleProcessController CreateConsoleProcessController(bool useConsoleEmulation, string configuredConsoleEmulator);

    /// <summary>
    ///  Creates a console shell controller for the configured emulator, if available.
    /// </summary>
    IConsoleShellController? CreateConsoleShellControl(string configuredConsoleEmulator);

    /// <summary>
    ///  Gets the console emulators supported in the current environment.
    /// </summary>
    IReadOnlyCollection<IConsoleEmulator> AvailableConsoleEmulators { get; }
}
