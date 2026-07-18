namespace GitUI.ConsoleEmulation.PlainText;

internal sealed class PlainTextConsoleEmulator : IConsoleEmulator
{
    public string Name => "PlainText";

    public string DisplayName => "Plain text";

    public bool IsSupportedInCurrentEnvironment => PlainTextConsoleShellRunner.IsShellAvailable;

    public IReadOnlyCollection<string> AvailableThemes => [];

    public string? DefaultTheme => null;

    public IConsoleCommandRunner CreateCommandRunner(ConsoleEmulatorSettings settings)
        => new PlainTextConsoleCommandRunner();

    public IConsoleShellRunner CreateShellRunner(ConsoleEmulatorSettings settings)
        => new PlainTextConsoleShellRunner();
}
