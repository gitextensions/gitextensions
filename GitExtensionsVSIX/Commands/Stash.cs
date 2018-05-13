using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    public sealed class Stash : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("stash", fileName);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.Any;
    }
}
