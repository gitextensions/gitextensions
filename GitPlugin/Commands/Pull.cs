using EnvDTE;
using GitExtensions;
using GitUI;

namespace GitPlugin.Commands
{
    public sealed class Pull : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            //RunGitEx("pull", fileName);
            ApplicationLoader.Load();
            GitCommands.Settings.WorkingDir = fileName;
            GitUICommands.Instance.StartPullDialog();
        }

        protected override CommandTarget SupportedTargets
        {
            get { return CommandTarget.SolutionExplorerFileItem; }
        }
    }
}
