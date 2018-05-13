using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    public sealed class FileHistory : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("filehistory", fileName);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.SolutionExplorerFileItem;
    }
}
