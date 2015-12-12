using System;
using System.Diagnostics;
using System.Windows.Forms;

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

		public abstract void Done();

		public abstract void KillProcess();

		public abstract void Reset();

		public abstract void Start();

		public abstract void StartProcess([NotNull] string command, string arguments, string workdir);

		public event DataReceivedEventHandler DataReceived;

		protected void FireDataReceived([NotNull] DataReceivedEventArgs args)
		{
			if(args == null)
				throw new ArgumentNullException("args");
			DataReceivedEventHandler evt = DataReceived;
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