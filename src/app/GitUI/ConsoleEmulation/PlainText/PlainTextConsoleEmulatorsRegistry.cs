namespace GitUI.ConsoleEmulation.PlainText;

internal class PlainTextConsoleEmulatorsRegistry : IConsoleEmulatorsRegistry
{
    public static PlainTextConsoleEmulatorsRegistry Instance { get; } = new();

    private PlainTextConsoleEmulatorsRegistry()
    {
    }

    public IReadOnlyCollection<IConsoleEmulator> AvailableConsoleEmulators => [];

    public IConsoleCommandRunner CreateCommandController()
    {
        return new PlainTextConsoleCommandRunner();
    }

    public IConsoleShellRunner? CreateShellRunner() => null;
}
