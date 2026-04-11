using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using GitCommands;
using GitCommands.Logging;
using GitExtensions.Extensibility;

namespace GitUI.ConsoleEmulation.Mintty;

internal sealed partial class MinttyCommandRunner : IConsoleCommandRunner
{
    private readonly string _minttyPath;
    private readonly string _bashPath;
    private readonly string? _theme;

    private MinttyControl? _terminal;
    private readonly Panel _panel;

    internal MinttyCommandRunner(string minttyPath, string bashPath, string? theme)
    {
        _minttyPath = minttyPath;
        _bashPath = bashPath;
        _theme = theme;

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
                ConsoleProcessCommandLine = commandLine,
                StartupDirectory = workDir,
                ProcessExitedCallback = exitCode =>
                {
                    operation.LogProcessEnd(exitCode);
                    SafeBeginInvoke(() => CommandProcessExited?.Invoke(this, new ConsoleProcessExitEventArgs(exitCode)));
                },
                ConsoleClosedCallback = () => SafeBeginInvoke(() => ConsoleHostTerminated?.Invoke(this, EventArgs.Empty)),
                AnsiOutputLineCallback = line =>
                {
                    line = StripAnsiCodesRegex().Replace(line, string.Empty);
                    CommandOutputReceived?.Invoke(this, new ConsoleOutputEventArgs(line));
                },
            };

            foreach ((string name, string value) in envVariables)
            {
                startInfo.SetEnv(name, value);
            }

            _terminal.StartCommand(startInfo, _minttyPath, _bashPath, _theme);
        }
        catch (Exception ex)
        {
            operation.LogProcessEnd(ex);
            throw;
        }
    }

    private void SafeBeginInvoke(Action action)
    {
        if (_panel.IsHandleCreated)
        {
            try
            {
                _panel.BeginInvoke(action);
                return;
            }
            catch (InvalidOperationException)
            {
                // Handle was destroyed between IsHandleCreated and BeginInvoke.
            }
        }

        // Process.Exited fires on a thread-pool thread; running the action inline
        // would push UI updates off the UI thread. Hop via JTF instead.
        ThreadHelper.FileAndForget(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            action();
        });
    }

    [GeneratedRegex(@"\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])")]
    private static partial Regex StripAnsiCodesRegex();
}
