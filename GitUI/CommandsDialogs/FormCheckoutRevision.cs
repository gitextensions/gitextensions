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
        private readonly TranslationString _noRevisionSelectedMsgBox = new TranslationString("Select 1 revision to checkout.");
        private readonly TranslationString _noRevisionSelectedMsgBoxCaption = new TranslationString("Checkout");

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormCheckoutRevision()
        {
            InitializeComponent();
        }

        public FormCheckoutRevision(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
        }

        public void SetRevision(string commitHash)
        {
            commitPickerSmallControl1.SetSelectedCommitHash(commitHash);
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                var selectedObjectId = commitPickerSmallControl1.SelectedObjectId;

                if (selectedObjectId == null)
                {
                    MessageBox.Show(this, _noRevisionSelectedMsgBox.Text, _noRevisionSelectedMsgBoxCaption.Text);
                    return;
                }

                var checkedOutObjectId = Module.GetCurrentCheckout();

                Debug.Assert(checkedOutObjectId != null, "checkedOutObjectId != null");

                ScriptManager.RunEventScripts(this, ScriptEvent.BeforeCheckout);

                string command = GitCommandHelpers.CheckoutCmd(selectedObjectId.ToString(), Force.Checked ? LocalChangesAction.Reset : 0);
                if (FormProcess.ShowDialog(this, command))
                {
                    if (selectedObjectId != checkedOutObjectId)
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
