namespace GitUI.ConsoleEmulation.PlainText;

internal class PlainTextConsoleEmulatorsRegistry : IConsoleEmulatorsRegistry
{
    public static PlainTextConsoleEmulatorsRegistry Instance { get; } = new();

    private PlainTextConsoleEmulatorsRegistry()
    {
    }

    public IReadOnlyCollection<IConsoleEmulator> AvailableConsoleEmulators => [];

    public IConsoleCommandController CreateCommandController()
    {
        return new PlainTextConsoleCommandController();
    }

    public IConsoleShellController? CreateShellController() => null;
}
