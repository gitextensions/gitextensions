using System.Windows.Forms;
using GitCommands;

namespace GitUI.Browsing.Dialogs
{
    internal interface ISimpleDialog
    {
        DialogResult ShowOkDialog(string text, string caption, MessageBoxIcon icon);

        DialogResult ShowYesNoDialog(string text, string caption, MessageBoxIcon icon);

        /// <summary>
        /// Show dialog for file selection
        /// </summary>
        /// <returns>Selected file name</returns>
        string ShowFilePromptDialog();

        bool ShowStandardProcessDialog(string process, ArgumentString arguments, string workingDirectory, string input, bool useDialogSettings);
    }
}
