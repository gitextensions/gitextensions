using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    public sealed class Push : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("push", fileName);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.Any;
    }
}
