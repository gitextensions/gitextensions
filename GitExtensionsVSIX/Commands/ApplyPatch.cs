using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    public sealed class ApplyPatch : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("applypatch", fileName);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.SolutionExplorerFileItem;
    }
}
