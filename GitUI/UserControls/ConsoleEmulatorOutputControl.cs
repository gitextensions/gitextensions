using System;
using System.Text;
using System.Windows.Forms;

using ConEmu.WinForms;

using GitCommands;
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
            Controls.Add(_panel = new Panel() { Dock = DockStyle.Fill, BorderStyle = BorderStyle.Fixed3D });
        }

        public override int ExitCode
        {
            get
            {
                return _nLastExitCode;
            }
        }

        public override bool IsDisplayingFullProcessOutput
        {
            get
            {
                return true;
            }
        }

        public static bool IsSupportedInThisEnvironment
        {
            get
            {
                return EnvUtils.RunningOnWindows(); // ConEmu only works in WinNT
            }
        }

        public override void AppendMessageFreeThreaded(string text)
        {
            ConEmuSession session = _terminal.RunningSession;
            if(session != null)
                session.WriteOutputText(text);
        }

        public override void KillProcess()
        {
            KillProcess(_terminal);
        }

        private static void KillProcess(ConEmuControl terminal)
        {
            ConEmuSession session = terminal.RunningSession;
            if (session != null)
                session.SendControlCAsync();
        }

        public override void Reset()
        {
            ConEmuControl oldTerminal = _terminal;

            _terminal = new ConEmuControl()
            {
                Dock = DockStyle.Fill,
                AutoStartInfo = null, /* don't spawn terminal until we have gotten the command */
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
            base.Dispose(disposing);
            if (disposing && _terminal != null)
            {
                _terminal.Dispose();
            }
        }

        public override void StartProcess(string command, string arguments, string workdir)
        {
            var cmdl = new StringBuilder();
            if (command != null)
            {
                cmdl.Append(command.Quote() /* do the escaping for it */);
                cmdl.Append(" ");
            }
            cmdl.Append(arguments /* expecting to be already escaped */);

            var startinfo = new ConEmuStartInfo();
            startinfo.ConsoleProcessCommandLine = cmdl.ToString();
            startinfo.ConsoleProcessExtraArgs = " -cur_console:P:\"<Solarized Light>\"";
            startinfo.StartupDirectory = workdir;
            startinfo.WhenConsoleProcessExits = WhenConsoleProcessExits.KeepConsoleEmulatorAndShowMessage;
            startinfo.AnsiStreamChunkReceivedEventSink = (sender, args) =>
            {
                var text = args.GetText(GitModule.SystemEncoding);
                if (EnvUtils.RunningOnWindows())
                    text = text.Replace("\n", Environment.NewLine);
                FireDataReceived(new TextEventArgs(text));
            };
            startinfo.ConsoleProcessExitedEventSink = (sender, args) =>
            {
                _nLastExitCode = args.ExitCode;
                FireProcessExited();
            };

            startinfo.ConsoleEmulatorClosedEventSink = (s, e) =>
                {
                    if (s == _terminal.RunningSession)
                    {
                        FireTerminated();
                    }
                };
            startinfo.IsEchoingConsoleCommandLine = true;

            _terminal.Start(startinfo);
        }
    }
}