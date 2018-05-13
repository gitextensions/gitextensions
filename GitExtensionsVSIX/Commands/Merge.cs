using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    public sealed class Merge : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("merge", fileName);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.Any;
    }
}
