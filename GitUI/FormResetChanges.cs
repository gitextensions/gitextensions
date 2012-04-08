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

        public FormResetChanges()
        {
            InitializeComponent();
            Translate();
        }

        /// <summary>
        /// Shows the dialog modally under the given owner, and returns the user's selection (RESET, RESET_AND_DELETE, or CANCEL).
        /// </summary>
        public static ResultType ShowResetDialog(IWin32Window owner)
        {
            FormResetChanges form = new FormResetChanges();
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
