using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.HelperDialogs;
using ResourceManager;

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

        private readonly TranslationString _hoverShowImageLabelText = new TranslationString("Hover to see scenario when fast forward is possible.");

        private readonly string _defaultBranch;
        private readonly string _defaultToBranch;
        private readonly bool _startRebaseImmediately;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormRebase()
        {
            InitializeComponent();
        }

        public FormRebase(GitUICommands commands, string defaultBranch)
            : base(commands)
        {
            _defaultBranch = defaultBranch;
            InitializeComponent();
            InitializeComplete();
            helpImageDisplayUserControl1.Visible = !AppSettings.DontShowHelpImages;
            helpImageDisplayUserControl1.IsOnHoverShowImage2NoticeText = _hoverShowImageLabelText.Text;
            if (AppSettings.AlwaysShowAdvOpt)
            {
                ShowOptions_LinkClicked(null, null);
            }
        }

        public FormRebase(GitUICommands commands, string from, string to, string defaultBranch, bool interactive = false,
            bool startRebaseImmediately = true)
            : this(commands, defaultBranch)
        {
            txtFrom.Text = from;
            _defaultToBranch = to;
            chkInteractive.Checked = interactive;
            chkAutosquash.Enabled = interactive;
            _startRebaseImmediately = startRebaseImmediately;
        }

        private void FormRebaseLoad(object sender, EventArgs e)
        {
            var selectedHead = Module.GetSelectedBranch();
            Currentbranch.Text = selectedHead;

            var refs = Module.GetRefs(true, true).OfType<GitRef>().ToList();
            Branches.DataSource = refs;
            Branches.DisplayMember = nameof(GitRef.Name);

            if (_defaultBranch != null)
            {
                Branches.Text = _defaultBranch;
            }

            Branches.Select();

            refs = Module.GetRefs(false, true).OfType<GitRef>().ToList();
            cboTo.DataSource = refs;
            cboTo.DisplayMember = nameof(GitRef.Name);

            cboTo.Text = _defaultToBranch ?? selectedHead;

            rebasePanel.Visible = !Module.InTheMiddleOfRebase();
            EnableButtons();

            // Honor the rebase.autosquash configuration.
            var autosquashSetting = Module.GetEffectiveSetting("rebase.autosquash");
            chkAutosquash.Checked = autosquashSetting.Trim().ToLower() == "true";

            chkStash.Checked = AppSettings.RebaseAutoStash;
            if (_startRebaseImmediately)
            {
                OkClick(null, null);
            }
            else
            {
                ShowOptions_LinkClicked(null, null);
            }
        }

        private void EnableButtons()
        {
            if (Module.InTheMiddleOfRebase())
            {
                if (Height < 200)
                {
                    Height = 500;
                }

                Branches.Enabled = false;
                Ok.Enabled = false;
                chkStash.Enabled = false;

                AddFiles.Enabled = true;
                Commit.Enabled = true;
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
                Commit.Enabled = false;
                Resolved.Enabled = false;
                Mergetool.Enabled = false;
                Skip.Enabled = false;
                Abort.Enabled = false;
                chkStash.Enabled = Module.IsDirtyDir();
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
            using (WaitCursorScope.Enter())
            {
                FormProcess.ShowDialog(this, GitCommandHelpers.ContinueRebaseCmd());

                if (!Module.InTheMiddleOfRebase())
                {
                    Close();
                }

                EnableButtons();
                patchGrid1.Initialize();
            }
        }

        private void SkipClick(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                FormProcess.ShowDialog(this, GitCommandHelpers.SkipRebaseCmd());

                if (!Module.InTheMiddleOfRebase())
                {
                    Close();
                }

                EnableButtons();
                patchGrid1.Initialize();
            }
        }

        private void AbortClick(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                FormProcess.ShowDialog(this, GitCommandHelpers.AbortRebaseCmd());

                if (!Module.InTheMiddleOfRebase())
                {
                    Close();
                }

                EnableButtons();
                patchGrid1.Initialize();
            }
        }

        private void OkClick(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                if (string.IsNullOrEmpty(Branches.Text))
                {
                    MessageBox.Show(this, _noBranchSelectedText.Text);
                    return;
                }

                AppSettings.RebaseAutoStash = chkStash.Checked;

                string rebaseCmd;
                if (chkSpecificRange.Checked && !string.IsNullOrWhiteSpace(txtFrom.Text) && !string.IsNullOrWhiteSpace(cboTo.Text))
                {
                    rebaseCmd = GitCommandHelpers.RebaseCmd(
                        cboTo.Text, chkInteractive.Checked, chkPreserveMerges.Checked,
                        chkAutosquash.Checked, chkStash.Checked, txtFrom.Text, Branches.Text);
                }
                else
                {
                    rebaseCmd = GitCommandHelpers.RebaseCmd(
                        Branches.Text, chkInteractive.Checked,
                        chkPreserveMerges.Checked, chkAutosquash.Checked, chkStash.Checked);
                }

                var dialogResult = FormProcess.ReadDialog(this, rebaseCmd);
                if (dialogResult.Trim() == "Current branch a is up to date.")
                {
                    MessageBox.Show(this, _branchUpToDateText.Text, _branchUpToDateCaption.Text);
                }

                if (!Module.InTheMiddleOfAction() &&
                    !Module.InTheMiddleOfPatch())
                {
                    Close();
                }

                EnableButtons();
                patchGrid1.Initialize();
            }
        }

        private void SolveMergeConflictsClick(object sender, EventArgs e)
        {
            MergetoolClick(sender, e);
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
            using (var chooseForm = new FormChooseCommit(UICommands, txtFrom.Text))
            {
                if (chooseForm.ShowDialog(this) == DialogResult.OK && chooseForm.SelectedRevision != null)
                {
                    txtFrom.Text = chooseForm.SelectedRevision.ObjectId.ToShortString(8);
                }
            }
        }

        private void CommitButtonClick(object sender, EventArgs e)
        {
            UICommands.StartCommitDialog(this);
            EnableButtons();
        }
    }
}
