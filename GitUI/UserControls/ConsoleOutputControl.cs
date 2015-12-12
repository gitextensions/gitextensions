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
		public abstract void AppendMessageFreeThreaded([NotNull] string text);

		public abstract void Done();

		public abstract void Reset();

		public abstract void Start();
	}
}