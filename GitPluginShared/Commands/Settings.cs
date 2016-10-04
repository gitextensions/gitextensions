using EnvDTE;

namespace GitPluginShared.Commands
{
    public sealed class Settings : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("settings", fileName);
        }

        protected override CommandTarget SupportedTargets
        {
            get { return CommandTarget.Any; }
        }
    }
}
