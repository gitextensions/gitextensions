using EnvDTE;

namespace GitPlugin.Commands
{
    public sealed class SwitchBranch : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("checkoutbranch", fileName);
        }

        protected override CommandTarget SupportedTargets
        {
            get { return CommandTarget.Any; }
        }
    }
}
