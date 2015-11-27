using EnvDTE;

namespace GitPlugin.Commands
{
    public sealed class Merge : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("merge", fileName);
        }

        protected override CommandTarget SupportedTargets
        {
            get { return CommandTarget.Any; }
        }
    }
}
