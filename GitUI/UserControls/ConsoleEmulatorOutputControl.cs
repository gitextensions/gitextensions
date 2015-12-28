using System;
using System.Threading;
using System.Windows.Forms;

using ConEmu.WinForms;

using GitCommands;
using GitCommands.Utils;

using JetBrains.Annotations;

using Microsoft.Build.Utilities;

namespace GitUI.UserControls
{
	/// <summary>
	/// An output control which inserts a fully-functional console emulator window.
	/// </summary>
	public class ConsoleEmulatorOutputControl : ConsoleOutputControl
	{
		[NotNull]
		private readonly ConEmuControl _terminal;

		public ConsoleEmulatorOutputControl()
		{
			Controls.Add(_terminal = new ConEmuControl() {Dock = DockStyle.Fill, AutoStartInfo = null /* don't spawn terminal until we have gotten the command */});
		}

		public override int ExitCode
		{
			get
			{
				return _terminal.LastExitCode;
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
			// TODO: write output text into the terminal
		}

		public override void Done()
		{
			// TODO: remove this method
		}

		public override void KillProcess()
		{
			// TODO: kill the payload process when we know it
			ConEmuSession session = _terminal.RunningSession;
			if(session != null)
				session.KillConsoleEmulator();
		}

		public override void Reset()
		{
			ConEmuSession session = _terminal.RunningSession;
			if(session != null)
				session.KillConsoleEmulator();
		}

		public override void Start()
		{
			// TODO: remove this method
		}

		public override void StartProcess(string command, string arguments, string workdir)
		{
			var cmdl = new CommandLineBuilder();
			cmdl.AppendFileNameIfNotNull(command /* do the escaping for it */);
			cmdl.AppendSwitch(arguments /* expecting to be already escaped */);

			var startinfo = new ConEmuStartInfo();
			startinfo.ConsoleCommandLine = cmdl.ToString();
			startinfo.StartupDirectory = workdir;
			startinfo.WhenPayloadProcessExits = WhenPayloadProcessExits.KeepTerminalAndShowMessage;
			startinfo.AnsiStreamChunkReceivedEventSink = (sender, args) => FireDataReceived(new TextEventArgs(args.GetText(GitModule.SystemEncoding)));
			startinfo.PayloadExitedEventSink = delegate { FireProcessExited(); };
			startinfo.ConsoleEmulatorExitedEventSink = delegate { FireTerminated(); };

			_terminal.Start(startinfo);
		}
	}
}