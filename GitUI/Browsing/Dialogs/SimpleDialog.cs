using System.Windows.Forms;
using GitUI.Script;

namespace GitUI.Browsing.Dialogs
{
    internal sealed class SimpleDialog : ISimpleDialog
    {
        private readonly IWin32Window _owner;

        public SimpleDialog(IWin32Window owner)
        {
            _owner = owner;
        }

        public DialogResult ShowOkDialog(string text, string caption, MessageBoxIcon icon)
        {
            return MessageBox.Show(_owner, text, caption, MessageBoxButtons.OK, icon);
        }

        public DialogResult ShowYesNoDialog(string text, string caption, MessageBoxIcon icon)
        {
            return MessageBox.Show(_owner, text, caption, MessageBoxButtons.YesNo, icon);
        }

        public DialogResult ShowFilePromptDialog()
        {
            using (var filePromptForm = new FormFilePrompt())
            {
                return filePromptForm.ShowDialog(_owner);
            }
        }
    }
}
