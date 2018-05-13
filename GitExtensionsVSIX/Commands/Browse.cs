using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    public sealed class Browse : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("browse", fileName);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.Any;
    }
}
