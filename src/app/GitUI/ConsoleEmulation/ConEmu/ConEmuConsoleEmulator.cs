namespace GitUI.ConsoleEmulation.ConEmu;

internal class ConEmuConsoleEmulator : IConsoleEmulator
{
    public string Name => "conemu";

    public string DisplayName => "ConEmu";

    public bool IsSupportedInCurrentEnvironment => OperatingSystem.IsWindows();

    public IConsoleCommandController CreateCommandController()
    {
        return new ConEmuConsoleCommandController();
    }

    public IConsoleShellController CreateShellController()
    {
        return new ConEmuConsoleShellController();
    }
}
