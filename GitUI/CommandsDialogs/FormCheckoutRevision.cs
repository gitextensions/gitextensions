using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI.CommandsDialogs
{
    public partial class FormCheckoutRevision : GitModuleForm
    {
        private readonly TranslationString _noRevisionSelectedMsgBox =
            new TranslationString("Select 1 revision to checkout.");
        private readonly TranslationString _noRevisionSelectedMsgBoxCaption =
            new TranslationString("Checkout");

        /// <summary>
        /// For VS designer
        /// </summary>
        private FormCheckoutRevision()
            : this(null)
        {
        }

        public FormCheckoutRevision(GitUICommands aCommands)
            : base(true, aCommands)
        {
            InitializeComponent();
            Translate();

            cbUpdateSubmodules.Visible = Module.GetSubmodulesLocalPathes().Any();
            cbUpdateSubmodules.Checked = AppSettings.UpdateSubmodulesOnCheckout;
        }

        private void FormCheckoutLoad(object sender, EventArgs e)
        {

        }

        public void SetRevision(string commitHash)
        {
            commitPickerSmallControl1.SetSelectedCommitHash(commitHash);
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                var commitHash = commitPickerSmallControl1.SelectedCommitHash;

                if (commitHash.IsNullOrEmpty())
                {
                    MessageBox.Show(this, _noRevisionSelectedMsgBox.Text, _noRevisionSelectedMsgBoxCaption.Text);
                    return;
                }

                string command = GitCommandHelpers.CheckoutCmd(commitHash, Force.Checked ? LocalChangesAction.Reset : 0);

                FormProcess.ShowDialog(this, command);
                
                if (Module.GetSubmodulesLocalPathes().Any())
                {
                    if (cbUpdateSubmodules.Checked)
                        UICommands.StartUpdateSubmodulesDialog(this);

                    AppSettings.UpdateSubmodulesOnCheckout = cbUpdateSubmodules.Checked;
                }

                DialogResult = System.Windows.Forms.DialogResult.OK;

                Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
}