using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    public sealed class Rebase : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("rebase", fileName);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.Any;
    }
}
