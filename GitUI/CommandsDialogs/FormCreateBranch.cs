using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormCreateBranch : GitModuleForm
    {
        private readonly TranslationString _noRevisionSelected = new TranslationString("Select 1 revision to create the branch on.");
        private readonly TranslationString _branchNameIsEmpty = new TranslationString("Enter branch name.");
        private readonly TranslationString _branchNameIsNotValud = new TranslationString("“{0}” is not valid branch name.");
        private readonly IGitBranchNameNormaliser _branchNameNormaliser;
        private readonly GitBranchNameOptions _gitBranchNameOptions = new GitBranchNameOptions(AppSettings.AutoNormaliseSymbol);


        public FormCreateBranch(GitUICommands aCommands, GitRevision revision, IGitBranchNameNormaliser branchNameNormaliser = null)
            : base(aCommands)
        {
            _branchNameNormaliser = branchNameNormaliser ?? new GitBranchNameNormaliser();
            CheckoutAfterCreation = true;
            UserAbleToChangeRevision = true;
            CouldBeOrphan = true;

            InitializeComponent();
            Translate();

            commitPickerSmallControl1.UICommandsSource = this;
            if (IsUICommandsInitialized)
            {
                commitPickerSmallControl1.SetSelectedCommitHash(revision == null ? Module.GetCurrentCheckout() : revision.Guid);
            }
        }

        public bool CheckoutAfterCreation { get; set; }
        public bool UserAbleToChangeRevision { get; set; }
        public bool CouldBeOrphan { get; set; }

        private IEnumerable<T> FindControls<T>(Control control) where T : Control
        {
            var controls = control.Controls.Cast<Control>().ToList();
            return controls.SelectMany(FindControls<T>)
                           .Concat(controls)
                           .Where(c => c.GetType() == typeof(T))
                           .Cast<T>();
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
            foreach (var label in FindControls<Label>(this))
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

            string commitGuid = commitPickerSmallControl1.SelectedCommitHash;
            var branchName = BranchNameTextBox.Text.Trim();

            if (branchName.IsNullOrWhiteSpace())
            {
                MessageBox.Show(_branchNameIsEmpty.Text, Text);
                DialogResult = DialogResult.None;
                return;
            }
            if (!Module.CheckBranchFormat(branchName))
            {
                MessageBox.Show(string.Format(_branchNameIsNotValud.Text, branchName), Text);
                DialogResult = DialogResult.None;
                return;
            }
            try
            {
                if (commitGuid == null)
                {
                    MessageBox.Show(this, _noRevisionSelected.Text, Text);
                    return;
                }

                string cmd;
                if (Orphan.Checked)
                {
                    cmd = GitCommandHelpers.CreateOrphanCmd(branchName, commitGuid);
                }
                else
                {
                    cmd = GitCommandHelpers.BranchCmd(branchName, commitGuid, chkbxCheckoutAfterCreate.Checked);
                }

                bool wasSuccessFul = FormProcess.ShowDialog(this, cmd);
                if (Orphan.Checked && wasSuccessFul && ClearOrphan.Checked)
                {// orphan AND orphan creation success AND clear
                    cmd = GitCommandHelpers.RemoveCmd();
                    FormProcess.ShowDialog(this, cmd);
                }

                if (wasSuccessFul && chkbxCheckoutAfterCreate.Checked)
                {
                    UICommands.UpdateSubmodules(this);
                }

                DialogResult = wasSuccessFul ? DialogResult.OK : DialogResult.None;
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

            chkbxCheckoutAfterCreate.Enabled = (isOrphan == false);// auto-checkout for orphan
            if (isOrphan)
            {
                chkbxCheckoutAfterCreate.Checked = true;
            }
        }
    }
}
