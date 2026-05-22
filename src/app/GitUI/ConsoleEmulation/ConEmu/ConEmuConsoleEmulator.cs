namespace GitUI.ConsoleEmulation.ConEmu;

internal class ConEmuConsoleEmulator : IConsoleEmulator
{
    public string Name => "conemu";

    public string DisplayName => "ConEmu";

    public bool IsSupportedInCurrentEnvironment => OperatingSystem.IsWindows();

    public IConsoleCommandRunner CreateCommandRunner()
    {
        return new ConEmuConsoleCommandRunner();
    }

    public IConsoleShellRunner CreateShellRunner()
    {
        return new ConEmuConsoleShellRunner();
    }
}
