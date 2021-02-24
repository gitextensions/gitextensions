using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Commands;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormCreateBranch : GitExtensionsDialog
    {
        private readonly TranslationString _noRevisionSelected = new("Select 1 revision to create the branch on.");
        private readonly TranslationString _branchNameIsEmpty = new("Enter branch name.");
        private readonly TranslationString _branchNameIsNotValid = new("“{0}” is not valid branch name.");
        private readonly IGitBranchNameNormaliser _branchNameNormaliser = new GitBranchNameNormaliser();
        private readonly GitBranchNameOptions _gitBranchNameOptions = new(AppSettings.AutoNormaliseSymbol);

        public bool CheckoutAfterCreation { get; set; } = true;
        public bool UserAbleToChangeRevision { get; set; } = true;
        public bool CouldBeOrphan { get; set; } = true;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormCreateBranch()
        {
            InitializeComponent();
        }

        public FormCreateBranch(GitUICommands commands, ObjectId? objectId, string? newBranchNamePrefix = null)
            : base(commands, enablePositionRestore: false)
        {
            InitializeComponent();

            // work-around the designer bug that can't add controls to FlowLayoutPanel
            ControlsPanel.Controls.Add(Ok);
            AcceptButton = Ok;

            InitializeComplete();

            groupBox1.AutoSize = true;

            if (objectId is not null && objectId.IsArtificial)
            {
                objectId = null;
            }

            commitSummaryUserControl1.Revision = null;

            objectId ??= Module.GetCurrentCheckout();
            if (objectId is not null)
            {
                commitPickerSmallControl1.SetSelectedCommitHash(objectId.ToString());

                if (string.IsNullOrWhiteSpace(newBranchNamePrefix))
                {
                    GitRevision revision = Module.GetRevision(objectId, shortFormat: true, loadRefs: true);
                    IGitRef firstRef = revision.Refs.FirstOrDefault(r => !r.IsTag) ?? revision.Refs.FirstOrDefault(r => r.IsTag);
                    newBranchNamePrefix = firstRef?.LocalName;

                    commitSummaryUserControl1.Revision = revision;
                }
            }

            if (!string.IsNullOrWhiteSpace(newBranchNamePrefix))
            {
                BranchNameTextBox.Text = newBranchNamePrefix;
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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

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
            if (objectId is null)
            {
                MessageBox.Show(this, _noRevisionSelected.Text, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }

            var branchName = BranchNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(branchName))
            {
                MessageBox.Show(_branchNameIsEmpty.Text, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }

            if (!Module.CheckBranchFormat(branchName))
            {
                MessageBox.Show(string.Format(_branchNameIsNotValid.Text, branchName), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }

            try
            {
                var originalHash = Module.GetCurrentCheckout();

                var command = Orphan.Checked
                    ? GitCommandHelpers.CreateOrphanCmd(branchName, objectId)
                    : GitCommandHelpers.BranchCmd(branchName, objectId.ToString(), chkbxCheckoutAfterCreate.Checked);

                bool success = FormProcess.ShowDialog(this, process: null, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);
                if (Orphan.Checked && success && ClearOrphan.Checked)
                {
                    // orphan AND orphan creation success AND clear
                    FormProcess.ShowDialog(this, process: null, arguments: GitCommandHelpers.RemoveCmd(), Module.WorkingDir, input: null, useDialogSettings: true);
                }

                if (success && chkbxCheckoutAfterCreate.Checked && objectId != originalHash)
                {
                    UICommands.UpdateSubmodules(this);
                }

                DialogResult = success ? DialogResult.OK : DialogResult.None;
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

        private void commitPickerSmallControl1_SelectedObjectIdChanged(object sender, EventArgs e)
        {
            GitRevision revision = Module.GetRevision(commitPickerSmallControl1.SelectedObjectId, shortFormat: true, loadRefs: true);
            commitSummaryUserControl1.Revision = revision;
        }
    }
}
