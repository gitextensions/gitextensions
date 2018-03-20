using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using GitUI.Script;
using ResourceManager;

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

        public FormCheckoutRevision(GitUICommands commands)
            : base(true, commands)
        {
            InitializeComponent();
            Translate();
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

                var originalHash = Module.GetCurrentCheckout();
                ScriptManager.RunEventScripts(this, ScriptEvent.BeforeCheckout);

                string command = GitCommandHelpers.CheckoutCmd(commitHash, Force.Checked ? LocalChangesAction.Reset : 0);
                if (FormProcess.ShowDialog(this, command))
                {
                    if (!string.Equals(commitHash, originalHash, StringComparison.OrdinalIgnoreCase))
                    {
                        UICommands.UpdateSubmodules(this);
                    }
                }

                ScriptManager.RunEventScripts(this, ScriptEvent.AfterCheckout);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
}
