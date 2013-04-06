using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitUI.HelperDialogs;
using ResourceManager.Translation;

namespace GitUI.CommandsDialogs
{
    public partial class FormRebase : GitModuleForm
    {
        private readonly TranslationString _continueRebaseText = new TranslationString("Continue rebase");
        private readonly TranslationString _solveConflictsText = new TranslationString("Solve conflicts");

        private readonly TranslationString _solveConflictsText2 = new TranslationString(">Solve conflicts<");
        private readonly TranslationString _continueRebaseText2 = new TranslationString(">Continue rebase<");

        private readonly TranslationString _noBranchSelectedText = new TranslationString("Please select a branch");

        private readonly TranslationString _branchUpToDateText =
            new TranslationString("Current branch a is up to date." + Environment.NewLine + "Nothing to rebase.");
        private readonly TranslationString _branchUpToDateCaption = new TranslationString("Rebase");

        private readonly string _defaultBranch;
        private readonly string _defaultToBranch;

        private FormRebase()
            : this(null)
        { }

        private FormRebase(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
            helpImageDisplayUserControl1.Visible = !Settings.DontShowHelpImages;
        }

        public FormRebase(GitUICommands aCommands, string defaultBranch)
            : this(aCommands)
        {
            _defaultBranch = defaultBranch;
        }

        public FormRebase(GitUICommands aCommands, string from, string to, string defaultBranch)
            : this(aCommands, defaultBranch)
        {
            txtFrom.Text = from;
            _defaultToBranch = to;
        }

        private void FormRebaseLoad(object sender, EventArgs e)
        {
            var selectedHead = Module.GetSelectedBranch();
            Currentbranch.Text = selectedHead;

            Branches.DisplayMember = "Name";
            Branches.DataSource = Module.GetRefs(true, true);

            if (_defaultBranch != null)
                Branches.Text = _defaultBranch;

            Branches.Select();

            cboTo.DisplayMember = "Name";
            cboTo.DataSource = Module.GetRefs(false, true);

            if (_defaultToBranch != null)
                cboTo.Text = _defaultToBranch;
            else
                cboTo.Text = selectedHead;

            rebasePanel.Visible = !Module.InTheMiddleOfRebase();
            EnableButtons();

            // Honor the rebase.autosquash configuration.
            var autosquashSetting = Module.GetEffectiveSetting("rebase.autosquash");
            chkAutosquash.Checked = "true" == autosquashSetting.Trim().ToLower();
        }

        private void EnableButtons()
        {
            if (Module.InTheMiddleOfRebase())
            {
                if (Height < 200)
                    Height = 500;

                Branches.Enabled = false;
                Ok.Enabled = false;

                AddFiles.Enabled = true;
                Resolved.Enabled = !Module.InTheMiddleOfConflictedMerge();
                Mergetool.Enabled = Module.InTheMiddleOfConflictedMerge();
                Skip.Enabled = true;
                Abort.Enabled = true;
            }
            else
            {
                Branches.Enabled = true;
                Ok.Enabled = true;
                AddFiles.Enabled = false;
                Resolved.Enabled = false;
                Mergetool.Enabled = false;
                Skip.Enabled = false;
                Abort.Enabled = false;
            }

            SolveMergeconflicts.Visible = Module.InTheMiddleOfConflictedMerge();

            Resolved.Text = _continueRebaseText.Text;
            Mergetool.Text = _solveConflictsText.Text;
            ContinuePanel.BackColor = Color.Transparent;
            MergeToolPanel.BackColor = Color.Transparent;

            if (Module.InTheMiddleOfConflictedMerge())
            {
                AcceptButton = Mergetool;
                Mergetool.Focus();
                Mergetool.Text = _solveConflictsText2.Text;
                MergeToolPanel.BackColor = Color.Yellow;
            }
            else if (Module.InTheMiddleOfRebase())
            {
                AcceptButton = Resolved;
                Resolved.Focus();
                Resolved.Text = _continueRebaseText2.Text;
                ContinuePanel.BackColor = Color.Yellow;
            }
        }

        private void MergetoolClick(object sender, EventArgs e)
        {
            UICommands.StartResolveConflictsDialog(this);
            EnableButtons();
        }

        private void InteractiveRebaseClick(object sender, EventArgs e)
        {
            chkAutosquash.Enabled = chkInteractive.Checked;
        }

        private void AddFilesClick(object sender, EventArgs e)
        {
            UICommands.StartAddFilesDialog(this);
        }

        private void ResolvedClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            FormProcess.ShowDialog(this, GitCommandHelpers.ContinueRebaseCmd());

            if (!Module.InTheMiddleOfRebase())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void SkipClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            FormProcess.ShowDialog(this, GitCommandHelpers.SkipRebaseCmd());

            if (!Module.InTheMiddleOfRebase())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void AbortClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            FormProcess.ShowDialog(this, GitCommandHelpers.AbortRebaseCmd());

            if (!Module.InTheMiddleOfRebase())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void OkClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (string.IsNullOrEmpty(Branches.Text))
            {
                MessageBox.Show(this, _noBranchSelectedText.Text);
                return;
            }

            string rebaseCmd;
            if (chkSpecificRange.Checked && !String.IsNullOrWhiteSpace(txtFrom.Text) && !String.IsNullOrWhiteSpace(cboTo.Text))
            {
                rebaseCmd = GitCommandHelpers.RebaseRangeCmd(txtFrom.Text, cboTo.Text, Branches.Text,
                                                             chkInteractive.Checked, chkPreserveMerges.Checked,
                                                             chkAutosquash.Checked);
            }
            else
            {
                rebaseCmd = GitCommandHelpers.RebaseCmd(Branches.Text, chkInteractive.Checked, chkPreserveMerges.Checked, chkAutosquash.Checked);
            }

            var dialogResult = FormProcess.ReadDialog(this, rebaseCmd);
            if (dialogResult.Trim() == "Current branch a is up to date.")
                MessageBox.Show(this, _branchUpToDateText.Text, _branchUpToDateCaption.Text);

            if (!Module.InTheMiddleOfConflictedMerge() &&
                !Module.InTheMiddleOfRebase() &&
                !Module.InTheMiddleOfPatch())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void SolveMergeconflictsClick(object sender, EventArgs e)
        {
            MergetoolClick(sender, e);
        }

        private void chkPreserveMerges_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ShowOptions_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShowOptions.Visible = false;
            OptionsPanel.Visible = true;
        }

        private void chkUseFromOnto_CheckedChanged(object sender, EventArgs e)
        {
            txtFrom.Enabled = chkSpecificRange.Checked;
            cboTo.Enabled = chkSpecificRange.Checked;
            btnChooseFromRevision.Enabled = chkSpecificRange.Checked;
        }

        private void btnChooseFromRevision_Click(object sender, EventArgs e)
        {
            using(var chooseForm = new FormChooseCommit(UICommands, txtFrom.Text))
            {
                if (chooseForm.ShowDialog(this) == DialogResult.OK && chooseForm.SelectedRevision != null)
                {
                    txtFrom.Text = chooseForm.SelectedRevision.Guid.Substring(0, 8);
                }
            }
        }
    }
}
