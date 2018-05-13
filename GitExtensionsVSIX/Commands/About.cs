using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    public sealed class About : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("about", fileName);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.Any;
    }
}
