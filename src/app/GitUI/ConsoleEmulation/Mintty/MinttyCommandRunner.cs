using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using GitCommands.Logging;
using GitExtensions.Extensibility;

namespace GitUI.ConsoleEmulation.Mintty;

internal sealed partial class MinttyCommandRunner : IConsoleCommandRunner
{
    private readonly string _minttyPath;
    private readonly string _bashPath;
    private readonly ConsoleEmulatorSettings _settings;

    private MinttyControl? _terminal;
    private readonly Panel _panel;

    internal MinttyCommandRunner(string minttyPath, string bashPath, ConsoleEmulatorSettings settings)
    {
        _minttyPath = minttyPath;
        _bashPath = bashPath;
        _settings = settings;

        _panel = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None };
    }

    public Control Control => _panel;

    public event EventHandler<ConsoleOutputEventArgs>? CommandOutputReceived;
    public event EventHandler<ConsoleProcessExitEventArgs>? CommandProcessExited;
    public event EventHandler? ConsoleHostTerminated;

    public void WriteCommandProcessInput(string text)
    {
        _terminal?.SendConsoleInput(text);
    }

    public void KillCommandProcess()
    {
        _terminal?.RunningSession?.Kill();
    }

    [MemberNotNull(nameof(_terminal))]
    public void ResetConsole()
    {
        MinttyControl? oldTerminal = _terminal;

        _terminal = new MinttyControl { Dock = DockStyle.Fill };

        if (oldTerminal is not null)
        {
            _panel.Controls.Remove(oldTerminal);
            oldTerminal.Dispose();
        }

        _panel.Controls.Add(_terminal);
    }

    public void StartCommand(string command, string arguments, string workDir, Dictionary<string, string> envVariables)
    {
        if (_terminal is null)
        {
            ResetConsole();
        }

        ProcessOperation operation = CommandLog.LogProcessStart(command, arguments, workDir);

        try
        {
            string commandLine = new ArgumentBuilder { command.Quote(), arguments }.ToString();

            MinttyStartInfo startInfo = new()
            {
                ProcessOperation = operation,
                ConsoleProcessCommandLine = commandLine,
                StartupDirectory = workDir,
                ProcessExitedCallback = exitCode =>
                {
                    operation.LogProcessEnd(exitCode);
                    Control.InvokeAndForget(() => CommandProcessExited?.Invoke(this, new ConsoleProcessExitEventArgs(exitCode)));
                },
                ConsoleClosedCallback = () =>
                {
                    Control.InvokeAndForget(() => ConsoleHostTerminated?.Invoke(this, EventArgs.Empty));
                },
                AnsiOutputLineCallback = line =>
                {
                    line = StripAnsiCodesRegex().Replace(line, string.Empty);
                    Control.InvokeAndForget(() => CommandOutputReceived?.Invoke(this, new ConsoleOutputEventArgs(line)));
                }
            };

            foreach ((string name, string value) in envVariables)
            {
                startInfo.EnvironmentVariables[name] = value;
            }

            _terminal.StartCommand(startInfo, _minttyPath, _bashPath, _settings);
        }
        catch (Exception ex)
        {
            operation.LogProcessEnd(ex);
            throw;
        }
    }

    [GeneratedRegex(@"\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])")]
    private static partial Regex StripAnsiCodesRegex();
}
