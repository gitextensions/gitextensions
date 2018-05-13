using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    public sealed class Pull : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("pull", fileName);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.Any;
    }
}
