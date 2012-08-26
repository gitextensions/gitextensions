using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormRebase : GitExtensionsForm
    {
        private readonly TranslationString _currentBranchText = new TranslationString("Current branch:");

        private readonly TranslationString _continueRebaseText = new TranslationString("Continue rebase");
        private readonly TranslationString _solveConflictsText = new TranslationString("Solve conflicts");

        private readonly TranslationString _solveConflictsText2 = new TranslationString(">Solve conflicts<");
        private readonly TranslationString _continueRebaseText2 = new TranslationString(">Continue rebase<");

        private readonly TranslationString _noBranchSelectedText = new TranslationString("Please select a branch");

        private readonly TranslationString _branchUpToDateText =
            new TranslationString("Current branch a is up to date." + Environment.NewLine + "Nothing to rebase.");
        private readonly TranslationString _branchUpToDateCaption = new TranslationString("Rebase");

        private readonly string _defaultBranch;

        public FormRebase(string defaultBranch)
            : base(true)
        {
            InitializeComponent();
            Translate();
            _defaultBranch = defaultBranch;
        }

        private void FormRebaseLoad(object sender, EventArgs e)
        {
            var selectedHead = GitModule.Current.GetSelectedBranch();
            Currentbranch.Text = _currentBranchText.Text + " " + selectedHead;

            Branches.DisplayMember = "Name";
            Branches.DataSource = GitModule.Current.GetHeads(true, true);

            if (_defaultBranch != null)
                Branches.Text = _defaultBranch;

            Branches.Select();

            splitContainer2.SplitterDistance = GitModule.Current.InTheMiddleOfRebase() ? 0 : 70;
            EnableButtons();

            // Honor the rebase.autosquash configuration.
            var autosquashSetting = GitModule.Current.GetEffectiveSetting("rebase.autosquash");
            chkAutosquash.Checked = "true" == autosquashSetting.Trim().ToLower();
        }

        private void EnableButtons()
        {
            if (GitModule.Current.InTheMiddleOfRebase())
            {
                if (Height < 200)
                    Height = 500;

                Branches.Enabled = false;
                Ok.Enabled = false;

                AddFiles.Enabled = true;
                Resolved.Enabled = !GitModule.Current.InTheMiddleOfConflictedMerge();
                Mergetool.Enabled = GitModule.Current.InTheMiddleOfConflictedMerge();
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

            SolveMergeconflicts.Visible = GitModule.Current.InTheMiddleOfConflictedMerge();

            Resolved.Text = _continueRebaseText.Text;
            Mergetool.Text = _solveConflictsText.Text;
            ContinuePanel.BackColor = Color.Transparent;
            MergeToolPanel.BackColor = Color.Transparent;

            if (GitModule.Current.InTheMiddleOfConflictedMerge())
            {
                AcceptButton = Mergetool;
                Mergetool.Focus();
                Mergetool.Text = _solveConflictsText2.Text;
                MergeToolPanel.BackColor = Color.Black;
            }
            else if (GitModule.Current.InTheMiddleOfRebase())
            {
                AcceptButton = Resolved;
                Resolved.Focus();
                Resolved.Text = _continueRebaseText2.Text;
                ContinuePanel.BackColor = Color.Black;
            }
        }

        private void MergetoolClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartResolveConflictsDialog(this);
            EnableButtons();
        }

        private void InteractiveRebaseClick(object sender, EventArgs e)
        {
            chkAutosquash.Enabled = chkInteractive.Checked;
        }

        private void AddFilesClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartAddFilesDialog(this);
        }

        private void ResolvedClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            FormProcess.ShowDialog(this, GitCommandHelpers.ContinueRebaseCmd());

            if (!GitModule.Current.InTheMiddleOfRebase())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void SkipClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            FormProcess.ShowDialog(this, GitCommandHelpers.SkipRebaseCmd());

            if (!GitModule.Current.InTheMiddleOfRebase())
                Close();

            EnableButtons();
            patchGrid1.Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void AbortClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            FormProcess.ShowDialog(this, GitCommandHelpers.AbortRebaseCmd());

            if (!GitModule.Current.InTheMiddleOfRebase())
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

            var rebaseCmd = GitCommandHelpers.RebaseCmd(Branches.Text, chkInteractive.Checked, chkPreserveMerges.Checked, chkAutosquash.Checked);
            var dialogResult = FormProcess.ReadDialog(this, rebaseCmd);
            if (dialogResult.Trim() == "Current branch a is up to date.")
                MessageBox.Show(this, _branchUpToDateText.Text, _branchUpToDateCaption.Text);

            if (!GitModule.Current.InTheMiddleOfConflictedMerge() &&
                !GitModule.Current.InTheMiddleOfRebase() &&
                !GitModule.Current.InTheMiddleOfPatch())
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
            splitContainer2.SplitterDistance = 100;
        }
    }
}
