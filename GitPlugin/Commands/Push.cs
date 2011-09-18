using EnvDTE;
using GitUI;
using GitExtensions;

namespace GitPlugin.Commands
{
    public sealed class Push : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            //RunGitEx("push", fileName);
            ApplicationLoader.Load();
            GitCommands.Settings.WorkingDir = fileName;
            GitUICommands.Instance.StartPushDialog();
        }

        protected override CommandTarget SupportedTargets
        {
            get { return CommandTarget.SolutionExplorerFileItem; }
        }
    }
}
