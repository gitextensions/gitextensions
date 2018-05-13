using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    public sealed class SwitchBranch : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("checkoutbranch", fileName);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.Any;
    }
}
