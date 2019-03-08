using System.Windows.Forms;

namespace GitUI.Browsing.Dialogs
{
    internal interface ISimpleDialog
    {
        DialogResult ShowOkDialog(string text, string caption, MessageBoxIcon icon);

        DialogResult ShowYesNoDialog(string text, string caption, MessageBoxIcon icon);

        DialogResult ShowFilePromptDialog();
    }
}
