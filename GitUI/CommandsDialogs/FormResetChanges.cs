using System;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    /// <summary>
    /// Shows a form asking if the user wants to reset their changes.
    /// </summary>
    public partial class FormResetChanges : GitExtensionsForm
    {
        // CANCEL must be placed at first position because it is the default value when
        // closing the dialog via the X button
        public enum ActionEnum
        {
            Cancel,
            Reset,
            ResetAndDelete
        }

        public ActionEnum SelectedAction { get; private set; }

        public FormResetChanges(bool hasExistingFiles, bool hasNewFiles)
        {
            InitializeComponent();
            InitializeComplete();

            if (!hasExistingFiles)
            {
                // No existing files => new files only => force the "delete new files" checkbox on.
                cbDeleteNewFilesAndDirectories.Enabled = false;
                cbDeleteNewFilesAndDirectories.Checked = true;
            }
            else if (!hasNewFiles)
            {
                // No new files => force the "delete new files" checkbox off.
                cbDeleteNewFilesAndDirectories.Enabled = false;
                cbDeleteNewFilesAndDirectories.Checked = false;
            }
            else
            {
                cbDeleteNewFilesAndDirectories.Enabled = true; // A mix of types, so enable the checkbox.
            }
        }

        /// <summary>
        /// Shows the dialog modally under the given owner, and returns the user's selection (RESET, RESET_AND_DELETE, or CANCEL).
        /// </summary>
        /// <param name="owner">Shows this form as a modal dialog with the specified owner.</param>
        /// <param name="hasExistingFiles">Are there existing (modified) files selected?</param>
        /// <param name="hasNewFiles">Are there new (untracked) files selected?</param>
        public static ActionEnum ShowResetDialog(IWin32Window owner, bool hasExistingFiles, bool hasNewFiles)
        {
            using (var form = new FormResetChanges(hasExistingFiles, hasNewFiles))
            {
                form.ShowDialog(owner);
                return form.SelectedAction;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            SelectedAction = ActionEnum.Cancel;
            Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            SelectedAction = cbDeleteNewFilesAndDirectories.Checked ? ActionEnum.ResetAndDelete : ActionEnum.Reset;
            Close();
        }
    }
}
