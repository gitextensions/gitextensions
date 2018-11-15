using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ConEmu.WinForms;
using GitCommands;
using GitCommands.Logging;
using GitCommands.Utils;

namespace GitUI.UserControls
{
    /// <summary>
    /// An output control which inserts a fully-functional console emulator window.
    /// </summary>
    public class ConsoleEmulatorOutputControl : ConsoleOutputControl
    {
        private int _nLastExitCode;

        private Panel _panel;
        private ConEmuControl _terminal;

        public ConsoleEmulatorOutputControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Controls.Add(_panel = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.Fixed3D });
        }

        public override int ExitCode => _nLastExitCode;

        public override bool IsDisplayingFullProcessOutput => true;

        public static bool IsSupportedInThisEnvironment => EnvUtils.RunningOnWindows();

        public override void AppendMessageFreeThreaded(string text)
        {
            _terminal.RunningSession?.WriteOutputTextAsync(text);
        }

        public override void KillProcess()
        {
            KillProcess(_terminal);
        }

        private static void KillProcess(ConEmuControl terminal)
        {
            terminal.RunningSession?.SendControlCAsync();
        }

        public override void Reset()
        {
            ConEmuControl oldTerminal = _terminal;

            _terminal = new ConEmuControl
            {
                Dock = DockStyle.Fill,
                IsStatusbarVisible = false
            };

            if (oldTerminal != null)
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
            var commandLine = new ArgumentBuilder { command.Quote(), arguments }.ToString();
            var outputProcessor = new ConsoleCommandLineOutputProcessor(commandLine.Length, FireDataReceived);

            var startInfo = new ConEmuStartInfo
            {
                ConsoleProcessCommandLine = commandLine,
                IsEchoingConsoleCommandLine = true,
                WhenConsoleProcessExits = WhenConsoleProcessExits.KeepConsoleEmulatorAndShowMessage,
                AnsiStreamChunkReceivedEventSink = outputProcessor.AnsiStreamChunkReceived,
                StartupDirectory = workDir
            };

            if (AppSettings.ConEmuStyle.ValueOrDefault != "Default")
            {
                startInfo.ConsoleProcessExtraArgs = " -new_console:P:\"" + AppSettings.ConEmuStyle.ValueOrDefault + "\"";
            }

            foreach (var (name, value) in envVariables)
            {
                startInfo.SetEnv(name, value);
            }

            var operation = CommandLog.LogProcessStart(command, arguments, workDir);

            startInfo.ConsoleProcessExitedEventSink = (_, args) =>
            {
                _nLastExitCode = args.ExitCode;
                operation.LogProcessEnd(_nLastExitCode);
                outputProcessor.Flush();
                FireProcessExited();
            };

            startInfo.ConsoleEmulatorClosedEventSink = (sender, _) =>
            {
                if (sender == _terminal.RunningSession)
                {
                    FireTerminated();
                }
            };

            _terminal.Start(startInfo, ThreadHelper.JoinableTaskFactory);
        }
    }

    public class ConsoleCommandLineOutputProcessor
    {
        private readonly Action<TextEventArgs> _fireDataReceived;
        private int _commandLineCharsInOutput;
        private string _lineChunk;

        public ConsoleCommandLineOutputProcessor(int commandLineCharsInOutput, Action<TextEventArgs> fireDataReceived)
        {
            _fireDataReceived = fireDataReceived;
            _commandLineCharsInOutput = commandLineCharsInOutput;
            _commandLineCharsInOutput += Environment.NewLine.Length; // for \n after the command line
        }

        private string FilterOutConsoleCommandLine(string outputChunk)
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
            string filtered = FilterOutConsoleCommandLine(text);
            if (filtered != null)
            {
                SendAsLines(filtered);
            }
        }

        private void SendAsLines(string output)
        {
            if (_lineChunk != null)
            {
                output = _lineChunk + output;
                _lineChunk = null;
            }

            string[] outputLines = Regex.Split(output, @"(?<=[\n\r])");
            int lineCount = outputLines.Length;
            if (outputLines[lineCount - 1].IsNullOrEmpty())
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
            if (_lineChunk != null)
            {
                _fireDataReceived(new TextEventArgs(_lineChunk));
                _lineChunk = null;
            }
        }
    }
}