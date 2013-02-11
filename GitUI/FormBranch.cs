using System;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormBranch : GitModuleForm
    {
        private readonly TranslationString _selectOneRevision = new TranslationString("Select 1 revision to create the branch on.");
        private readonly TranslationString _branchCaption = new TranslationString("Branch");

        /// <summary>For VS designer</summary>
        private FormBranch()
            : this(null) { }


        public FormBranch(GitUICommands aCommands)
            : base(true, aCommands)
        {
            InitializeComponent();
            Translate();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {
                if (RevisionGrid.GetSelectedRevisions().Count != 1)
                {
                    MessageBox.Show(this, _selectOneRevision.Text, _branchCaption.Text);
                    return;
                }

                string cmd = GitCommandHelpers.BranchCmd(BName.Text, RevisionGrid.GetSelectedRevisions()[0].Guid, CheckoutAfterCreate.Checked);
                FormProcess.ShowDialog(this, cmd);
                UICommands.RepoChangedNotifier.Notify();

                Close();
            }
            catch
            {
            }
        }

        private void Checkout_Click(object sender, EventArgs e)
        {
            UICommands.StartCheckoutBranchDialog(this);
            RevisionGrid.RefreshRevisions();
        }

        private void FormBranch_Load(object sender, EventArgs e)
        {
            RevisionGrid.Load();

            BName.Focus();
            AcceptButton = Ok;
        }
    }
}
