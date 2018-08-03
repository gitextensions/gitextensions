using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormCreateBranch : GitModuleForm
    {
        private readonly TranslationString _noRevisionSelected = new TranslationString("Select 1 revision to create the branch on.");
        private readonly TranslationString _branchNameIsEmpty = new TranslationString("Enter branch name.");
        private readonly TranslationString _branchNameIsNotValid = new TranslationString("“{0}” is not valid branch name.");
        private readonly IGitBranchNameNormaliser _branchNameNormaliser = new GitBranchNameNormaliser();
        private readonly GitBranchNameOptions _gitBranchNameOptions = new GitBranchNameOptions(AppSettings.AutoNormaliseSymbol);

        public bool CheckoutAfterCreation { get; set; } = true;
        public bool UserAbleToChangeRevision { get; set; } = true;
        public bool CouldBeOrphan { get; set; } = true;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormCreateBranch()
        {
            InitializeComponent();
        }

        public FormCreateBranch(GitUICommands commands, ObjectId objectId)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();

            groupBox1.AutoSize = true;

            objectId = objectId ?? Module.GetCurrentCheckout();
            if (objectId != null)
            {
                commitPickerSmallControl1.SetSelectedCommitHash(objectId.ToString());
            }
        }

        private void BranchNameTextBox_Leave(object sender, EventArgs e)
        {
            if (!AppSettings.AutoNormaliseBranchName || !BranchNameTextBox.Text.Any(GitBranchNameNormaliser.IsValidChar))
            {
                return;
            }

            var caretPosition = BranchNameTextBox.SelectionStart;
            var branchName = _branchNameNormaliser.Normalise(BranchNameTextBox.Text, _gitBranchNameOptions);
            BranchNameTextBox.Text = branchName;
            BranchNameTextBox.SelectionStart = caretPosition;
        }

        private void FormCreateBranch_Shown(object sender, EventArgs e)
        {
            // ensure all labels are wrapped if required
            // this must happen only after the label texts have been set
            foreach (var label in this.FindDescendantsOfType<Label>())
            {
                label.AutoSize = true;
            }

            chkbxCheckoutAfterCreate.Checked = CheckoutAfterCreation;
            commitPickerSmallControl1.Enabled = UserAbleToChangeRevision;
            groupBox1.Enabled = CouldBeOrphan;

            BranchNameTextBox.Focus();
        }

        private void OkClick(object sender, EventArgs e)
        {
            // Ok button set as the "AcceptButton" for the form
            // if the user hits [Enter] at any point, we need to trigger BranchNameTextBox Leave event
            Ok.Focus();

            var objectId = commitPickerSmallControl1.SelectedObjectId;
            if (objectId == null)
            {
                MessageBox.Show(this, _noRevisionSelected.Text, Text);
                DialogResult = DialogResult.None;
                return;
            }

            var branchName = BranchNameTextBox.Text.Trim();
            if (branchName.IsNullOrWhiteSpace())
            {
                MessageBox.Show(_branchNameIsEmpty.Text, Text);
                DialogResult = DialogResult.None;
                return;
            }

            if (!Module.CheckBranchFormat(branchName))
            {
                MessageBox.Show(string.Format(_branchNameIsNotValid.Text, branchName), Text);
                DialogResult = DialogResult.None;
                return;
            }

            try
            {
                var originalHash = Module.GetCurrentCheckout();

                var cmd = Orphan.Checked
                    ? GitCommandHelpers.CreateOrphanCmd(branchName, objectId)
                    : GitCommandHelpers.BranchCmd(branchName, objectId.ToString(), chkbxCheckoutAfterCreate.Checked);

                bool wasSuccessful = FormProcess.ShowDialog(this, cmd);
                if (Orphan.Checked && wasSuccessful && ClearOrphan.Checked)
                {
                    // orphan AND orphan creation success AND clear
                    FormProcess.ShowDialog(this, GitCommandHelpers.RemoveCmd());
                }

                if (wasSuccessful && chkbxCheckoutAfterCreate.Checked && objectId != originalHash)
                {
                    UICommands.UpdateSubmodules(this);
                }

                DialogResult = wasSuccessful ? DialogResult.OK : DialogResult.None;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void Orphan_CheckedChanged(object sender, EventArgs e)
        {
            bool isOrphan = Orphan.Checked;
            ClearOrphan.Enabled = isOrphan;

            chkbxCheckoutAfterCreate.Enabled = isOrphan == false; // auto-checkout for orphan
            if (isOrphan)
            {
                chkbxCheckoutAfterCreate.Checked = true;
            }
        }
    }
}
