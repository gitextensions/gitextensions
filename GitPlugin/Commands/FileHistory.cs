using EnvDTE;

namespace GitPlugin.Commands
{
    public sealed class FileHistory : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("filehistory", fileName);
        }

        protected override CommandTarget SupportedTargets
        {
            get { return CommandTarget.SolutionExplorerFileItem; }
        }
    }
}
