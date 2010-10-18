using System;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormBranch : GitExtensionsForm
    {
        public FormBranch()
        {
            InitializeComponent(); Translate();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {

                if (RevisionGrid.GetRevisions().Count != 1)
                {
                    MessageBox.Show("Select 1 revision to create the branch on.", "Branch");
                    return;
                }

                new FormProcess(GitCommandHelpers.BranchCmd(BName.Text, RevisionGrid.GetRevisions()[0].Guid, CheckoutAfterCreate.Checked)).ShowDialog();

                Close();

            }
            catch
            {
            }
        }

        private void Checkout_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartCheckoutBranchDialog();
            MergeConflictHandler.HandleMergeConflicts();
            RevisionGrid.RefreshRevisions();
        }

        private void FormBranch_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("branch");
        }

        private void FormBranch_Load(object sender, EventArgs e)
        {
            RestorePosition("branch");
            BName.Focus();
            AcceptButton = Ok;
        }
    }
}
