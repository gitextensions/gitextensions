using System.Globalization;
using System.Text.RegularExpressions;
using ConEmu.WinForms;
using GitCommands;
using GitCommands.Logging;
using GitExtensions.Extensibility;
using Microsoft;

namespace GitUI.ConsoleEmulation.ConEmu;

/// <summary>
///  Embeds a ConEmu terminal in the output panel so command dialogs can host an interactive console.
/// </summary>
internal class ConEmuConsoleCommandRunner : ContainerControl, IConsoleCommandRunner
{
    private int _nLastExitCode;

    private Panel _panel;
    private ConEmuControl? _terminal;

    public ConEmuConsoleCommandRunner()
    {
        InitializeComponent();

        Validates.NotNull(_panel);
    }

    private void InitializeComponent()
    {
        Controls.Add(_panel = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None });
    }

    public Control Control => this;

    public event EventHandler<ConsoleOutputEventArgs>? CommandOutputReceived;
    public event EventHandler<ConsoleProcessExitEventArgs>? CommandProcessExited;
    public event EventHandler? ConsoleHostTerminated;

    private void WriteConsoleOutput(string text)
    {
        Validates.NotNull(_terminal);
        _terminal.RunningSession?.WriteOutputTextAsync(text);
    }

    public void WriteCommandProcessInput(string text)
    {
        Validates.NotNull(_terminal);
        this.InvokeAndForget(() => _terminal.RunningSession?.WriteInputTextAsync(text)!);
    }

    public void KillCommandProcess()
    {
        Validates.NotNull(_terminal);
        KillProcess(_terminal);
    }

    private static void KillProcess(ConEmuControl terminal)
    {
        terminal.RunningSession?.SendControlCAsync();
    }

    public void ResetConsole()
    {
        ConEmuControl? oldTerminal = _terminal;

        _terminal = new ConEmuControl
        {
            Dock = DockStyle.Fill,
            IsStatusbarVisible = false
        };

        if (oldTerminal is not null)
        {
            KillProcess(oldTerminal);
            _panel.Controls.Remove(oldTerminal);
            oldTerminal.Dispose();
        }

        _panel.Controls.Add(_terminal);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _terminal?.Dispose();
        }

        base.Dispose(disposing);
    }

    public void StartCommand(string command, string arguments, string workDir, Dictionary<string, string> envVariables)
    {
        ProcessOperation operation = CommandLog.LogProcessStart(command, arguments, workDir);

        WriteConsoleOutput($"{command.Quote()} {arguments}{Environment.NewLine}");

        try
        {
            string commandLine = new ArgumentBuilder { command.Quote(), arguments }.ToString();
            ConsoleCommandLineOutputProcessor outputProcessor = new(commandLine.Length, args => CommandOutputReceived?.Invoke(this, args));

            ConEmuStartInfo startInfo = new()
            {
                ConsoleProcessCommandLine = commandLine,
                IsEchoingConsoleCommandLine = true,
                WhenConsoleProcessExits = WhenConsoleProcessExits.KeepConsoleEmulatorAndShowMessage,
                AnsiStreamChunkReceivedEventSink = outputProcessor.AnsiStreamChunkReceived,
                StartupDirectory = workDir
            };

            foreach ((string name, string value) in envVariables)
            {
                startInfo.SetEnv(name, value);
            }

            startInfo.ConsoleProcessExitedEventSink = (_, args) =>
            {
                _nLastExitCode = args.ExitCode;
                operation.LogProcessEnd(_nLastExitCode);
                outputProcessor.Flush();
                WriteConsoleOutput("Done");
                CommandProcessExited?.Invoke(this, new ConsoleProcessExitEventArgs(args.ExitCode));
            };

            startInfo.ConsoleEmulatorClosedEventSink = (sender, _) =>
            {
                Validates.NotNull(_terminal);
                if (sender == _terminal.RunningSession)
                {
                    ConsoleHostTerminated?.Invoke(this, EventArgs.Empty);
                }
            };

            Validates.NotNull(_terminal);
            _terminal.Start(startInfo, ThreadHelper.JoinableTaskFactory, AppSettings.GetEffectiveConEmuStyle(), AppSettings.ConEmuConsoleFont.Name, AppSettings.ConEmuConsoleFont.Size.ToString("F0", CultureInfo.InvariantCulture));
        }
        catch (Exception ex)
        {
            operation.LogProcessEnd(ex);
            throw;
        }
    }
}

public partial class ConsoleCommandLineOutputProcessor
{
    private readonly Action<ConsoleOutputEventArgs> _fireDataReceived;
    private int _commandLineCharsInOutput;
    private string? _lineChunk;

    [GeneratedRegex(@"(?<=[\n\r])", RegexOptions.ExplicitCapture)]
    private static partial Regex NewLineRegex { get; }

    public ConsoleCommandLineOutputProcessor(int commandLineCharsInOutput, Action<ConsoleOutputEventArgs> fireDataReceived)
    {
        _fireDataReceived = fireDataReceived;
        _commandLineCharsInOutput = commandLineCharsInOutput;
        _commandLineCharsInOutput += Environment.NewLine.Length; // for \n after the command line
    }

    private string? FilterOutConsoleCommandLine(string outputChunk)
    {
        if (_commandLineCharsInOutput > 0)
        {
            if (_commandLineCharsInOutput >= outputChunk.Length)
            {
                _commandLineCharsInOutput -= outputChunk.Length;
                return null;
            }

            string rest = outputChunk[_commandLineCharsInOutput..];
            _commandLineCharsInOutput = 0;
            return rest;
        }

        return outputChunk;
    }

    public void AnsiStreamChunkReceived(object? sender, AnsiStreamChunkEventArgs args)
    {
        string text = args.GetText(GitModule.SystemEncoding);
        string? filtered = FilterOutConsoleCommandLine(text);
        if (filtered is not null)
        {
            SendAsLines(filtered);
        }
    }

    private void SendAsLines(string output)
    {
        if (_lineChunk is not null)
        {
            output = _lineChunk + output;
            _lineChunk = null;
        }

        string[] outputLines = NewLineRegex.Split(output);
        int lineCount = outputLines.Length;
        if (string.IsNullOrEmpty(outputLines[^1]))
        {
            lineCount--;
        }

        for (int i = 0; i < lineCount; i++)
        {
            string outputLine = outputLines[i];
            bool isTheLastLine = i == lineCount - 1;
            if (isTheLastLine)
            {
                bool isLineCompleted = outputLine.Length > 0 &&
                    ((outputLine[^1] == '\n') ||
                    outputLine[^1] == '\r');
                if (!isLineCompleted)
                {
                    _lineChunk = outputLine;
                    break;
                }
            }

            _fireDataReceived(new ConsoleOutputEventArgs(outputLine));
        }
    }

    public void Flush()
    {
        if (_lineChunk is not null)
        {
            _fireDataReceived(new ConsoleOutputEventArgs(_lineChunk));
            _lineChunk = null;
        }
    }
}
