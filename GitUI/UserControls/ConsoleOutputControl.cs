using System;
using System.Windows.Forms;

using GitCommands;

using JetBrains.Annotations;

namespace GitUI.UserControls
{
	/// <summary>
	///     <para>Base control for executing a console process, as used by the <see cref="FormProcess" />.</para>
	///     <para>Switches between the basic impl which redirects stdout and integration of a real interactive terminal window into the form, if available.</para>
	/// </summary>
	public abstract class ConsoleOutputControl : ContainerControl
	{
		public abstract int ExitCode { get; }

		public abstract void AppendMessageFreeThreaded([NotNull] string text);

		/// <summary>
		/// Creates the instance best fitting the current environment.
		/// </summary>
		[NotNull]
		public static ConsoleOutputControl CreateInstance()
		{
			if((ConsoleEmulatorOutputControl.IsSupportedInThisEnvironment) && (AppSettings.UseConsoleEmulatorForCommands))
				return new ConsoleEmulatorOutputControl();
			return new EditboxBasedConsoleOutputControl();
		}

		public abstract void Done();

		public abstract void KillProcess();

		public abstract void Reset();

		public abstract void Start();

		public abstract void StartProcess([NotNull] string command, string arguments, string workdir);

		public event EventHandler<TextEventArgs> DataReceived;

		protected void FireDataReceived([NotNull] TextEventArgs args)
		{
			if(args == null)
				throw new ArgumentNullException("args");
			EventHandler<TextEventArgs> evt = DataReceived;
			if(evt != null)
				evt.Invoke(this, args);
		}

		protected void FireProcessExited()
		{
			EventHandler evt = ProcessExited;
			if(evt != null)
				evt(this, EventArgs.Empty);
		}

		public event EventHandler ProcessExited;
	}
}