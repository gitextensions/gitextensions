using System.ComponentModel;

using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
	/// <summary>
	/// Reusable window class.
	/// </summary>
	public sealed class AnonymousGitModuleForm : GitModuleForm
	{
		public AnonymousGitModuleForm([CanBeNull] GitUICommands aCommands)
			: base(aCommands)
		{
			if(aCommands == null)
				return; // Tests
			Translate();
		}

		[NotNull]
		public readonly IContainer Components = new Container();

		protected override void Dispose(bool disposing)
		{
			if(disposing)
				Components.Dispose();
			base.Dispose(disposing);
		}
	}
}