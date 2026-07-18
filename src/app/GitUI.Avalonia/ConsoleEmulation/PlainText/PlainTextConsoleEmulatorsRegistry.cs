namespace GitUI.ConsoleEmulation.PlainText;

// Avalonia twin of PlainTextConsoleEmulatorsRegistry. Unlike the WinForms fallback, the
// Avalonia implementation also provides the portable embedded plain-text shell.
internal sealed class PlainTextConsoleEmulatorsRegistry : IConsoleEmulatorsRegistry
{
    private static readonly PlainTextConsoleEmulator _emulator = new();

    public static PlainTextConsoleEmulatorsRegistry Instance { get; } = new();

    private PlainTextConsoleEmulatorsRegistry()
    {
    }

    public IReadOnlyCollection<IConsoleEmulator> AvailableConsoleEmulators
        => _emulator.IsSupportedInCurrentEnvironment ? [_emulator] : [];

    public IConsoleCommandRunner CreateCommandController()
    {
        return new PlainTextConsoleCommandRunner();
    }

    public IConsoleShellRunner? CreateShellRunner()
        => _emulator.IsSupportedInCurrentEnvironment
            ? _emulator.CreateShellRunner(new ConsoleEmulatorSettings(Theme: null, Font: null))
            : null;
}
