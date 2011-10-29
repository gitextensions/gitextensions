using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public sealed partial class FormBranchSmall : GitExtensionsForm
    {
        private readonly TranslationString _noRevisionSelected =
            new TranslationString("Select 1 revision to create the branch on.");

        public FormBranchSmall()
        {
            InitializeComponent();
            Translate();
        }

        public GitRevision Revision { get; set; }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                if (Revision == null)
                {
                    MessageBox.Show(_noRevisionSelected.Text, Text);
                    return;
                }
                var branchCmd = GitCommandHelpers.BranchCmd(BranchNameTextBox.Text, Revision.Guid,
                                                                  CheckoutAfterCreate.Checked);
                using (var formProcess = new FormProcess(branchCmd, PerFormSettingsName()))
                {
                    formProcess.ShowDialog();
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
}