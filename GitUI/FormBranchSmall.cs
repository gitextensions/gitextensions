using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormBranchSmall : GitExtensionsForm
    {
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
                    MessageBox.Show("Select 1 revision to create the branch on.", "Branch");
                    return;
                }
                var branchCmd = GitCommandHelpers.BranchCmd(BranchNameTextBox.Text, Revision.Guid,
                                                                  CheckoutAfterCreate.Checked);
                new FormProcess(branchCmd).ShowDialog();

                Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void BranchNameTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                OkClick(null, null);
        }
    }
}