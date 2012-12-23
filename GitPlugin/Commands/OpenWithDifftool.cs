using EnvDTE;

namespace GitPlugin.Commands
{
    public sealed class OpenWithDiftool : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("difftool", fileName);
        }

        protected override CommandTarget SupportedTargets
        {
            get { return CommandTarget.SolutionExplorerFileItem; }
        }
    }
}
