using EnvDTE;

namespace GitPlugin.Commands
{
	public sealed class FileDifftool : ItemCommandBase
	{
		protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
		{
			RunGitEx("filedifftool", fileName);
		}

		protected override CommandTarget SupportedTargets
		{
			get { return CommandTarget.SolutionExplorerFileItem; }
		}
	}
}
