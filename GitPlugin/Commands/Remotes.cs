using EnvDTE;

namespace GitPlugin.Commands
{
    public sealed class Remotes : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("remotes", fileName);
        }

        protected override CommandTarget SupportedTargets
        {
            get { return CommandTarget.Any; }
        }
    }
}
