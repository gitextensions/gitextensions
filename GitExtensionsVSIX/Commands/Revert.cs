using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    public sealed class Revert : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("revert", fileName);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.SolutionExplorerFileItem;
    }
}
