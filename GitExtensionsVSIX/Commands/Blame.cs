using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    public sealed class Blame : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            string[] arguments = null;

            if (item.DTE.ActiveDocument.Selection is TextSelection textSelection)
            {
                arguments = new[] { textSelection.CurrentLine.ToString() };
            }

            RunGitEx("blame", fileName, arguments);
        }

        protected override CommandTarget SupportedTargets => CommandTarget.File;
    }
}
