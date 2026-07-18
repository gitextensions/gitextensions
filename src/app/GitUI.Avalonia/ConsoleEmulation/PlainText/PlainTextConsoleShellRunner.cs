using Avalonia.Controls;
using Avalonia.Input;

namespace GitUI.ConsoleEmulation.PlainText;

/// <summary>
///  Hosts the platform shell using the portable redirected-process console runner.
/// </summary>
internal sealed class PlainTextConsoleShellRunner : IConsoleShellRunner, IDisposable
{
    private readonly TextBox _commandInput;
    private readonly PlainTextConsoleCommandRunner _commandRunner;
    private readonly Grid _control;
    private bool _isDisposed;

    public PlainTextConsoleShellRunner()
    {
        _commandRunner = new PlainTextConsoleCommandRunner();
        _commandRunner.CommandOutputReceived += CommandRunner_CommandOutputReceived;
        _commandRunner.CommandProcessExited += CommandRunner_CommandProcessExited;
        _commandInput = new TextBox
        {
            Name = "ConsoleInput",
            FontFamily = new Avalonia.Media.FontFamily("monospace"),
            Margin = new Avalonia.Thickness(0),
        };
        _commandInput.KeyDown += CommandInput_KeyDown;

        _control = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
        };
        _control.Children.Add(_commandRunner);
        Grid.SetRow(_commandInput, 1);
        _control.Children.Add(_commandInput);
    }

    internal static bool IsShellAvailable
    {
        get
        {
            string shell = GetShellExecutable();
            return OperatingSystem.IsWindows() || File.Exists(shell);
        }
    }

    public Control Control => _control;

    public bool IsShellRunning { get; private set; }

    public void ChangeWorkingDirectory(string path)
    {
        if (!IsShellRunning)
        {
            return;
        }

        string command = OperatingSystem.IsWindows()
            ? $"cd /d \"{path}\""
            : $"cd '{path.Replace("'", "'\"'\"'")}'";
        _commandRunner.WriteCommandProcessInput(command + Environment.NewLine);
    }

    public void FocusTerminal()
    {
        _commandInput.Focus();
    }

    public void StartShell(string workDir)
    {
        if (_isDisposed || IsShellRunning)
        {
            return;
        }

        string shell = GetShellExecutable();
        string arguments = OperatingSystem.IsWindows() ? "/D /Q" : "-i";
        Dictionary<string, string> environment = OperatingSystem.IsWindows()
            ? []
            : new Dictionary<string, string> { ["TERM"] = "dumb" };
        _commandRunner.StartCommand(shell, arguments, workDir, environment);
        IsShellRunning = true;
        FocusTerminal();
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _commandInput.KeyDown -= CommandInput_KeyDown;
        _commandRunner.CommandOutputReceived -= CommandRunner_CommandOutputReceived;
        _commandRunner.CommandProcessExited -= CommandRunner_CommandProcessExited;
        _commandRunner.Dispose();
        IsShellRunning = false;
    }

    private static string GetShellExecutable()
    {
        if (OperatingSystem.IsWindows())
        {
            return Environment.GetEnvironmentVariable("COMSPEC") ?? "cmd.exe";
        }

        string? shell = Environment.GetEnvironmentVariable("SHELL");
        return !string.IsNullOrWhiteSpace(shell) && File.Exists(shell) ? shell : "/bin/sh";
    }

    private void CommandInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || !IsShellRunning)
        {
            return;
        }

        _commandRunner.WriteCommandProcessInput((_commandInput.Text ?? string.Empty) + Environment.NewLine);
        _commandInput.Text = string.Empty;
        e.Handled = true;
    }

    private void CommandRunner_CommandProcessExited(object? sender, ConsoleProcessExitEventArgs e)
    {
        IsShellRunning = false;
    }

    private void CommandRunner_CommandOutputReceived(object? sender, ConsoleOutputEventArgs e)
    {
        _commandRunner.WriteOutputText(e.Text);
    }
}
