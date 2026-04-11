namespace GitUI.ConsoleEmulation.ConEmu;

internal class ConEmuConsoleEmulator : IConsoleEmulator
{
    public string Name => "conemu";

    public string DisplayName => "ConEmu";

    public bool IsSupportedInCurrentEnvironment => OperatingSystem.IsWindows();

    public IReadOnlyCollection<string> AvailableThemes => [];

    public string? DefaultTheme => null;

    public IConsoleCommandRunner CreateCommandRunner(string? theme)
    {
        return new ConEmuConsoleCommandRunner();
    }

    public IConsoleShellRunner CreateShellRunner(string? theme)
    {
        return new ConEmuConsoleShellRunner();
    }
}
