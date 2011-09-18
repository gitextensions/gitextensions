using EnvDTE;
using GitExtensions;
using GitUI;

namespace GitPlugin.Commands
{
    public sealed class Browse : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            //RunGitEx("browse", fileName);
            ApplicationLoader.Load();
            GitCommands.Settings.WorkingDir = fileName;
            GitUICommands.Instance.StartBrowseDialog();
        }

        protected override CommandTarget SupportedTargets
        {
            get { return CommandTarget.Any; }
        }
    }
}
