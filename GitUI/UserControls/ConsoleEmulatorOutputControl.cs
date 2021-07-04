using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ConEmu.WinForms;
using GitCommands;
using GitCommands.Logging;
using GitCommands.Utils;
using GitExtUtils;
using Microsoft;

namespace GitUI.UserControls
{
    /// <summary>
    /// An output control which inserts a fully-functional console emulator window.
    /// </summary>
    public class ConsoleEmulatorOutputControl : ConsoleOutputControl
    {
        private int _nLastExitCode;

        private Panel _panel;
        private ConEmuControl? _terminal;

        public ConsoleEmulatorOutputControl()
        {
            InitializeComponent();

            Validates.NotNull(_panel);
        }

        private void InitializeComponent()
        {
            Controls.Add(_panel = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None });
        }

        public override int ExitCode => _nLastExitCode;

        public override bool IsDisplayingFullProcessOutput => true;

        public static bool IsSupportedInThisEnvironment => EnvUtils.RunningOnWindows();

        public override void AppendMessageFreeThreaded(string text)
        {
            Validates.NotNull(_terminal);
            _terminal.RunningSession?.WriteOutputTextAsync(text);
        }

        public override void KillProcess()
        {
            Validates.NotNull(_terminal);
            KillProcess(_terminal);
        }

        private static void KillProcess(ConEmuControl terminal)
        {
            terminal.RunningSession?.SendControlCAsync();
        }

        public override void Reset()
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

        public override void StartProcess(string command, string arguments, string workDir, Dictionary<string, string> envVariables)
        {
            ProcessOperation operation = CommandLog.LogProcessStart(command, arguments, workDir);

            try
            {
                var commandLine = new ArgumentBuilder { command.Quote(), arguments }.ToString();
                ConsoleCommandLineOutputProcessor outputProcessor = new(commandLine.Length, FireDataReceived);

                ConEmuStartInfo startInfo = new()
                {
                    ConsoleProcessCommandLine = commandLine,
                    IsEchoingConsoleCommandLine = true,
                    WhenConsoleProcessExits = WhenConsoleProcessExits.KeepConsoleEmulatorAndShowMessage,
                    AnsiStreamChunkReceivedEventSink = outputProcessor.AnsiStreamChunkReceived,
                    StartupDirectory = workDir
                };

                foreach (var (name, value) in envVariables)
                {
                    startInfo.SetEnv(name, value);
                }

                startInfo.ConsoleProcessExitedEventSink = (_, args) =>
                {
                    _nLastExitCode = args.ExitCode;
                    operation.LogProcessEnd(_nLastExitCode);
                    outputProcessor.Flush();
                    FireProcessExited();
                };

                startInfo.ConsoleEmulatorClosedEventSink = (sender, _) =>
                {
                    Validates.NotNull(_terminal);
                    if (sender == _terminal.RunningSession)
                    {
                        FireTerminated();
                    }
                };

                Validates.NotNull(_terminal);
                _terminal.Start(startInfo, ThreadHelper.JoinableTaskFactory, AppSettings.ConEmuStyle.Value, AppSettings.ConEmuConsoleFont.Name, AppSettings.ConEmuConsoleFont.Size.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                operation.LogProcessEnd(ex);
                throw;
            }
        }
    }

    public class ConsoleCommandLineOutputProcessor
    {
        private readonly Action<TextEventArgs> _fireDataReceived;
        private int _commandLineCharsInOutput;
        private string? _lineChunk;

        public ConsoleCommandLineOutputProcessor(int commandLineCharsInOutput, Action<TextEventArgs> fireDataReceived)
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

                string rest = outputChunk.Substring(_commandLineCharsInOutput);
                _commandLineCharsInOutput = 0;
                return rest;
            }

            return outputChunk;
        }

        public void AnsiStreamChunkReceived(object sender, AnsiStreamChunkEventArgs args)
        {
            var text = args.GetText(GitModule.SystemEncoding);
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

            string[] outputLines = Regex.Split(output, @"(?<=[\n\r])");
            int lineCount = outputLines.Length;
            if (string.IsNullOrEmpty(outputLines[lineCount - 1]))
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
                        ((outputLine[outputLine.Length - 1] == '\n') ||
                        outputLine[outputLine.Length - 1] == '\r');
                    if (!isLineCompleted)
                    {
                        _lineChunk = outputLine;
                        break;
                    }
                }

                _fireDataReceived(new TextEventArgs(outputLine));
            }
        }

        public void Flush()
        {
            if (_lineChunk is not null)
            {
                _fireDataReceived(new TextEventArgs(_lineChunk));
                _lineChunk = null;
            }
        }
    }
}
