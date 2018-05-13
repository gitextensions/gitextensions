using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    public sealed class CreateBranch : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("branch", fileName);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.Any;
    }
}
