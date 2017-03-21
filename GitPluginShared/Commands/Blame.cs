using EnvDTE;

namespace GitPluginShared.Commands
{
    public sealed class Blame : ItemCommandBase
    {
        protected override void OnExecute (SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx ("blame", fileName);
        }

        protected override CommandTarget SupportedTargets
        {
            get { return CommandTarget.File; }
        }
    }
}
