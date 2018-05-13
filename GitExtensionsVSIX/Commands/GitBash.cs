using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    public sealed class Bash : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("gitbash", fileName);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.Any;
    }
}
