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

		/// <summary>
		/// While the feature is experimental and has certain problems with binding output and errorlevels, allow it for select critical uses only, like the clone dialog.
		/// </summary>
		/// <returns></returns>
		[NotNull]
		public static IDisposable TemporarilyAllowExperimentalFeature()	// TODO: remove when stabilized
		{
			return new TemporarilyAllowExperimentalFeatureHelper();
		}

		class TemporarilyAllowExperimentalFeatureHelper : IDisposable // TODO: remove when stabilized
		{
			public TemporarilyAllowExperimentalFeatureHelper()
			{
				Interlocked.Increment(ref _allowExperimental);
			}

			public static int _allowExperimental;
			void IDisposable.Dispose()
			{
				Interlocked.Decrement(ref _allowExperimental);
			}
		}

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

		public static bool IsSupportedInThisEnvironment
		{
			get
			{
				if(TemporarilyAllowExperimentalFeatureHelper._allowExperimental == 0)
					return false;	// TODO: remove when stable

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

			var startinfo = new ConEmuStartInfo() {ConsoleCommandLine = cmdl.ToString(), StartupDirectory = workdir, IsKeepingTerminalOnCommandExit = true};
			startinfo.AnsiStreamChunkReceivedEventSink = (sender, args) => FireDataReceived(new TextEventArgs(args.GetText(GitModule.SystemEncoding)));
			startinfo.PayloadExitedEventSink = delegate { FireProcessExited(); };

			_terminal.Start(startinfo);
		}
	}
}