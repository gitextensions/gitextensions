using EnvDTE;

namespace GitPluginShared.Commands
{
    public sealed class Blame : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            string[] arguments = null;

            var textSelection = item.DTE.ActiveDocument.Selection as TextSelection;
            if (textSelection != null)
            {
                arguments = new string[] { textSelection.CurrentLine.ToString() };
            }

            RunGitEx("blame", fileName, arguments);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.File;
    }
}
