using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    public sealed class OpenWithDifftool : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("difftool", fileName);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.SolutionExplorerFileItem;
    }
}
