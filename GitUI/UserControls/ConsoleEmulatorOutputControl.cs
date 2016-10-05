﻿using System;
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
			ConEmuSession session = _terminal.RunningSession;
			if(session != null)
				session.SendControlCAsync();
		}

        public override void Reset()
        {
            if (_terminal != null)
            {
                KillProcess();
                _panel.Controls.Remove(_terminal);
                _terminal.Dispose();
            }

            _panel.Controls.Add(_terminal = new ConEmuControl() { Dock = DockStyle.Fill, AutoStartInfo = null /* don't spawn terminal until we have gotten the command */});
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
            cmdl.Append(" -new_console:P:\"<Solarized Light>\"");

            var startinfo = new ConEmuStartInfo();
			startinfo.ConsoleProcessCommandLine = cmdl.ToString();
			startinfo.StartupDirectory = workdir;
			startinfo.WhenConsoleProcessExits = WhenConsoleProcessExits.KeepConsoleEmulatorAndShowMessage;
			startinfo.AnsiStreamChunkReceivedEventSink = (sender, args) => FireDataReceived(new TextEventArgs(args.GetText(GitModule.SystemEncoding)));
			startinfo.ConsoleProcessExitedEventSink = (sender, args) =>
			{
				_nLastExitCode = args.ExitCode;
				FireProcessExited();
			};

            startinfo.ConsoleEmulatorClosedEventSink = (s, e) =>
                {
                    if (!(s as ConEmuSession).IsConsoleProcessExited
                        && s == _terminal.RunningSession)
                    {
                        FireTerminated();
                    }
                };
			startinfo.IsEchoingConsoleCommandLine = true;

			_terminal.Start(startinfo);
		}
	}
}