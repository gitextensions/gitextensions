using GitCommands;

namespace GitUI.ConsoleEmulation.Mintty;

internal sealed class MinttyShellRunner : IConsoleShellRunner
{
    private readonly string _minttyPath;
    private readonly string _bashPath;
    private readonly ConsoleEmulatorSettings _settings;
    private readonly MinttyControl _control;

    internal MinttyShellRunner(string minttyPath, string bashPath, ConsoleEmulatorSettings settings)
    {
        _minttyPath = minttyPath;
        _bashPath = bashPath;
        _settings = settings;
        _control = new MinttyControl { Dock = DockStyle.Fill };
    }

    public bool IsShellRunning => _control.IsShellRunning;

    public Control Control => _control;

    public void StartShell(string workDir)
    {
        _control.StartInteractiveShell(_minttyPath, _bashPath, _settings.Theme, workDir, _settings.Font);
    }

    public void ChangeWorkingDirectory(string path)
    {
        string changeDirCommand = $"cd \"{path.ToMountPath("/")}\"";
        _control.SendConsoleInput($"\x1\xb{changeDirCommand}\n");
    }

    public void FocusTerminal()
    {
        _control.FocusConsole();
    }
}
