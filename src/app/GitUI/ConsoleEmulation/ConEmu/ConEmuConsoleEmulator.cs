namespace GitUI.ConsoleEmulation.ConEmu;

internal class ConEmuConsoleEmulator : IConsoleEmulator
{
    public string Name => "conemu";

    public string DisplayName => "ConEmu";

    public bool IsSupportedInCurrentEnvironment => OperatingSystem.IsWindows();

    public IConsoleProcessController CreateConsoleProcessController()
    {
        return new ConEmuConsoleProcessController();
    }

    public IConsoleShellController CreateConsoleShellController()
    {
        return new ConEmuConsoleShellController();
    }
}
