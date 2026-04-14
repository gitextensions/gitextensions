namespace GitUI.ConsoleEmulation.ConEmu;

internal class ConEmuConsoleEmulator : IConsoleEmulator
{
    public string Name => "conemu";

    public string DisplayName => "ConEmu";

    public bool IsSupportedInCurrentEnvironment => OperatingSystem.IsWindows();

    public IConsoleCommandController CreateConsoleProcessController()
    {
        return new ConEmuConsoleCommandController();
    }

    public IConsoleShellController CreateConsoleShellController()
    {
        return new ConEmuConsoleShellController();
    }
}
