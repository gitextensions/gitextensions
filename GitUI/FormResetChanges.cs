using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ResourceManager.Translation;

namespace GitUI
{
    /// <summary>
    /// Shows a form asking if the user wants to reset their changes.
    /// </summary>
    public partial class FormResetChanges : GitExtensionsForm
    {
        public enum ResultType { RESET, RESET_AND_DELETE, CANCEL };

        public ResultType Result { get; private set; }

        public FormResetChanges(bool hasExistingFiles, bool hasNewFiles)
        {
            InitializeComponent();
            Translate();

            if (!hasExistingFiles)
            {
                // No existing files => new files only => force the "delete new files" checkbox on.
                cbDeleteNewFiles.Enabled = false;
                cbDeleteNewFiles.Checked = true;
            }            
            else if (!hasNewFiles)
            {
                // No new files => force the "delete new files" checkbox off. 
                cbDeleteNewFiles.Enabled = false;
                cbDeleteNewFiles.Checked = false;
            }
            else
                cbDeleteNewFiles.Enabled = true; // A mix of types, so enable the checkbox.
        }

        /// <summary>
        /// Shows the dialog modally under the given owner, and returns the user's selection (RESET, RESET_AND_DELETE, or CANCEL).
        /// </summary>
        /// <param name="hasExistingFiles">Are there existing (modified) files selected?</param>
        /// <param name="hasNewFiles">Are there new (untracked) files selected?</param>
        public static ResultType ShowResetDialog(IWin32Window owner, bool hasExistingFiles, bool hasNewFiles)
        {
            FormResetChanges form = new FormResetChanges(hasExistingFiles, hasNewFiles);
            form.ShowDialog(owner);
            return form.Result;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Result = ResultType.CANCEL;
            Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Result = (cbDeleteNewFiles.Checked) ? ResultType.RESET_AND_DELETE : ResultType.RESET;
            Close();
        }
    }
}
