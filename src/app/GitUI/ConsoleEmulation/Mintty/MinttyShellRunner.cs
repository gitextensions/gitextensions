using GitCommands;

namespace GitUI.ConsoleEmulation.Mintty;

internal sealed class MinttyShellRunner : IConsoleShellRunner
{
    private readonly string _minttyPath;
    private readonly string _bashPath;
    private readonly string? _theme;
    private readonly MinttyControl _control;

    internal MinttyShellRunner(string minttyPath, string bashPath, string? theme)
    {
        _minttyPath = minttyPath;
        _bashPath = bashPath;
        _theme = theme;
        _control = new MinttyControl { Dock = DockStyle.Fill };
    }

    public bool IsShellRunning => _control.IsShellRunning;

    public Control Control => _control;

    public void StartShell(string workDir)
    {
        _control.StartInteractiveShell(_minttyPath, _bashPath, _theme, workDir);
    }

    public void ChangeWorkingDirectory(string path)
    {
        string changeDirCommand = $"cd {path.ToMountPath("/")}";
        _control.SendConsoleInput("\x01\x0B" + changeDirCommand + "\n");
    }

    public void FocusTerminal()
    {
        _control.FocusConsole();
    }
}
