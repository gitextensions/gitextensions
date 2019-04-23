using System;
using System.Windows.Forms;
using GitCommands;
using GitUI.Script;
using GitUI.UserControls;

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

    internal sealed class SimpleDialog : ISimpleDialog
    {
        private readonly IWin32Window _owner;

        public SimpleDialog(IWin32Window owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public DialogResult ShowOkDialog(string text, string caption, MessageBoxIcon icon)
        {
            return MessageBox.Show(_owner, text, caption, MessageBoxButtons.OK, icon);
        }

        public DialogResult ShowYesNoDialog(string text, string caption, MessageBoxIcon icon)
        {
            return MessageBox.Show(_owner, text, caption, MessageBoxButtons.YesNo, icon);
        }

        /// <inheritdoc />
        public string ShowFilePromptDialog()
        {
            using (var filePromptForm = new FormFilePrompt())
            {
                var dialogResult = filePromptForm
                    .ShowDialog(_owner);

                if (dialogResult != DialogResult.OK)
                {
                    return null;
                }

                return filePromptForm.FileInput;
            }
        }

        public bool ShowStandardProcessDialog(string process, ArgumentString arguments, string workingDirectory, string input, bool useDialogSettings)
        {
            var outputCtrl = new EditboxBasedConsoleOutputControl();

            using (var formProcess = new FormProcess(outputCtrl, process, arguments, workingDirectory, input, useDialogSettings))
            {
                formProcess.ShowDialog(_owner);

                return !formProcess.ErrorOccurred();
            }
        }
    }
}
