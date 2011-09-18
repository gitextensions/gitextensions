using EnvDTE;
using GitUI;
using GitExtensions;

namespace GitPlugin.Commands
{
    public sealed class SettingsDialog : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            //RunGitEx("settings", fileName);
            ApplicationLoader.Load();
            GitCommands.Settings.WorkingDir = fileName;
            GitUICommands.Instance.StartSettingsDialog();
        }

        protected override CommandTarget SupportedTargets
        {
            get { return CommandTarget.Any; }
        }
    }
}
