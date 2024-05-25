using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI.HelperDialogs;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormRebase : GitExtensionsDialog
    {
        #region Mnemonics
        // Available: GHJLVWXYZ
        // A Add files
        // B Abort
        // C Continue rebase
        // D Ignore date
        // E Update dependent refs
        // F From
        // I Interactive
        // K Skip
        // M Committer date
        // N Specific range
        // O Commit...
        // P Preserve Merges
        // Q Autosquash
        // R Rebase on
        // S Solve conflicts
        // T To
        // U Auto stash
        #endregion

        #region Translation
        private readonly TranslationString _continueRebaseText = new("&Continue rebase");
        private readonly TranslationString _solveConflictsText = new("&Solve conflicts");

        private readonly TranslationString _solveConflictsText2 = new(">&Solve conflicts<");
        private readonly TranslationString _continueRebaseText2 = new(">&Continue rebase<");

        private readonly TranslationString _noBranchSelectedText = new("Please select a branch");

        private readonly TranslationString _branchUpToDateText =
            new("Current branch a is up to date." + Environment.NewLine + "Nothing to rebase.");
        private readonly TranslationString _branchUpToDateCaption = new("Rebase");

        private readonly TranslationString _hoverShowImageLabelText = new("Hover to see scenario when fast forward is possible.");
        #endregion

        private static readonly List<PatchFile> Skipped = [];

        private readonly string? _defaultBranch;
        private readonly string? _defaultToBranch;
        private readonly bool _startRebaseImmediately;

        public FormRebase(IGitUICommands commands, string? defaultBranch)
            : base(commands, enablePositionRestore: false)
        {
            _defaultBranch = defaultBranch;

            InitializeComponent();

            btnSolveMergeconflicts.BackColor = OtherColors.MergeConflictsColor;
            btnSolveMergeconflicts.SetForeColorForBackColor();
            PanelLeftImage.Image1 = Properties.Images.HelpCommandRebase.AdaptLightness();

            PanelLeftImage.Visible = !AppSettings.DontShowHelpImages;
            PanelLeftImage.IsOnHoverShowImage2NoticeText = _hoverShowImageLabelText.Text;
            PatchGrid.SetSkipped(Skipped);
            if (AppSettings.AlwaysShowAdvOpt)
            {
                ShowOptions_LinkClicked(this, null!);
            }

            if (!Module.GitVersion.SupportUpdateRefs)
            {
                checkBoxUpdateRefs.Visible = false;
            }

            InitializeComplete();
        }

        public FormRebase(IGitUICommands commands, string? from, string? to, string? defaultBranch, bool interactive = false, bool startRebaseImmediately = true)
            : this(commands, defaultBranch)
        {
            txtFrom.Text = from;
            _defaultToBranch = to;
            chkInteractive.Checked = interactive;
            chkAutosquash.Enabled = interactive;
            _startRebaseImmediately = startRebaseImmediately;
            chkSpecificRange.Checked = !string.IsNullOrEmpty(from);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (IsDesignMode)
            {
                return;
            }

            PatchGrid.SelectCurrentlyApplyingPatch();

            string selectedHead = Module.GetSelectedBranch();
            Currentbranch.Text = selectedHead;

            // Offer rebase on refs also for tags (but not stash, notes etc)
            List<GitRef> refs = _startRebaseImmediately
                ? []
                : Module.GetRefs(RefsFilter.Heads | RefsFilter.Remotes | RefsFilter.Tags).OfType<GitRef>().ToList();
            cboBranches.DataSource = refs;
            cboBranches.DisplayMember = nameof(GitRef.Name);

            if (_defaultBranch is not null)
            {
                cboBranches.Text = _defaultBranch;
            }

            cboBranches.Select();

            refs = refs.Where(h => h.IsHead).ToList();
            cboTo.DataSource = refs;
            cboTo.DisplayMember = nameof(GitRef.Name);

            cboTo.Text = _defaultToBranch ?? selectedHead;

            rebasePanel.Visible = !Module.InTheMiddleOfRebase();
            EnableButtons();

            // Honor the rebase.autosquash configuration.
            chkAutosquash.Checked = Module.GetEffectiveSetting<bool>("rebase.autosquash") is true;
            if (Module.GitVersion.SupportUpdateRefs && Module.GetEffectiveSetting<bool>("rebase.updateRefs") is true)
            {
                checkBoxUpdateRefs.Checked = true;
            }

            chkStash.Checked = AppSettings.RebaseAutoStash;
            if (_startRebaseImmediately)
            {
                OkClick(this, EventArgs.Empty);
            }
            else
            {
                ShowOptions_LinkClicked(this, null!);
            }
        }

        private void EnableButtons()
        {
            bool conflictedMerge = Module.InTheMiddleOfConflictedMerge();
            if (Module.InTheMiddleOfRebase())
            {
                if (Height < 200)
                {
                    Height = 500;
                }

                cboBranches.Enabled = false;
                btnRebase.Visible = false;
                chkStash.Enabled = false;

                btnAddFiles.Visible = true;
                btnCommit.Visible = true;
                btnEditTodo.Visible = true;
                btnContinueRebase.Visible = !conflictedMerge;
                btnSolveConflicts.Visible = conflictedMerge;
                btnSkip.Visible = true;
                btnAbort.Visible = true;
            }
            else
            {
                cboBranches.Enabled = true;
                btnRebase.Visible = true;
                btnAddFiles.Visible = false;
                btnCommit.Visible = false;
                btnEditTodo.Visible = false;
                btnContinueRebase.Visible = false;
                btnSolveConflicts.Visible = false;
                btnSkip.Visible = false;
                btnAbort.Visible = false;
                chkStash.Enabled = Module.IsDirtyDir();
            }

            btnSolveMergeconflicts.Visible = conflictedMerge;

            btnContinueRebase.Text = _continueRebaseText.Text;
            btnSolveConflicts.Text = _solveConflictsText.Text;
            btnContinueRebase.ForeColor = SystemColors.ControlText;
            btnSolveConflicts.ForeColor = SystemColors.ControlText;
            MergeToolPanel.BackColor = Color.Transparent;

            Color highlightColor = Color.Yellow.AdaptBackColor();

            if (conflictedMerge)
            {
                AcceptButton = btnSolveConflicts;
                btnSolveConflicts.Focus();
                btnSolveConflicts.Text = _solveConflictsText2.Text;
                MergeToolPanel.BackColor = highlightColor;
            }
            else if (Module.InTheMiddleOfRebase())
            {
                AcceptButton = btnContinueRebase;
                btnContinueRebase.Focus();
                btnContinueRebase.Text = _continueRebaseText2.Text;
            }
        }

        private void MergetoolClick(object sender, EventArgs e)
        {
            UICommands.StartResolveConflictsDialog(this);
            EnableButtons();
        }

        private void chkInteractive_CheckedChanged(object sender, EventArgs e)
        {
            chkAutosquash.Enabled = chkInteractive.Checked;
        }

        private void chkIgnoreDate_CheckedChanged(object sender, EventArgs e)
        {
            ToggleDateCheckboxMutualExclusions();
        }

        private void chkCommitterDateIsAuthorDate_CheckedChanged(object sender, EventArgs e)
        {
            ToggleDateCheckboxMutualExclusions();
        }

        private void ToggleDateCheckboxMutualExclusions()
        {
            chkCommitterDateIsAuthorDate.Enabled = !chkIgnoreDate.Checked;
            chkIgnoreDate.Enabled = !chkCommitterDateIsAuthorDate.Checked;
            chkInteractive.Enabled = !chkIgnoreDate.Checked && !chkCommitterDateIsAuthorDate.Checked;
            chkPreserveMerges.Enabled = !chkIgnoreDate.Checked && !chkCommitterDateIsAuthorDate.Checked;
            chkAutosquash.Enabled = chkInteractive.Checked && !chkIgnoreDate.Checked && !chkCommitterDateIsAuthorDate.Checked;
        }

        private void AddFilesClick(object sender, EventArgs e)
        {
            UICommands.StartAddFilesDialog(this);
        }

        private void ResolvedClick(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                FormProcess.ShowDialog(this, UICommands, arguments: Commands.ContinueRebase(), Module.WorkingDir, input: null, useDialogSettings: true);

                if (!Module.InTheMiddleOfRebase())
                {
                    Close();
                }

                EnableButtons();
                PatchGrid.Initialize();
            }
        }

        private void SkipClick(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                PatchFile applyingPatch = PatchGrid.PatchFiles.FirstOrDefault(p => p.IsNext);
                if (applyingPatch is not null)
                {
                    applyingPatch.IsSkipped = true;
                    Skipped.Add(applyingPatch);
                }

                FormProcess.ShowDialog(this, UICommands, arguments: Commands.SkipRebase(), Module.WorkingDir, input: null, useDialogSettings: true);

                if (!Module.InTheMiddleOfRebase())
                {
                    Close();
                }

                EnableButtons();

                PatchGrid.RefreshGrid();
            }
        }

        private void AbortClick(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                FormProcess.ShowDialog(this, UICommands, arguments: Commands.AbortRebase(), Module.WorkingDir, input: null, useDialogSettings: true);

                if (!Module.InTheMiddleOfRebase())
                {
                    Skipped.Clear();
                    Close();
                }

                EnableButtons();
                PatchGrid.Initialize();
            }
        }

        private void EditTodoClick(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                FormProcess.ShowDialog(this, UICommands, arguments: Commands.EditTodoRebase(), Module.WorkingDir, input: null, useDialogSettings: true);

                if (!Module.InTheMiddleOfRebase())
                {
                    Skipped.Clear();
                    Close();
                }

                EnableButtons();
                PatchGrid.Initialize();
            }
        }

        private void OkClick(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                if (string.IsNullOrEmpty(cboBranches.Text))
                {
                    MessageBox.Show(this, _noBranchSelectedText.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                AppSettings.RebaseAutoStash = chkStash.Checked;

                Skipped.Clear();

                bool? updateRefChoice = null;
                if (Module.GitVersion.SupportUpdateRefs && Module.GetEffectiveSetting<bool>("rebase.updateRefs") != checkBoxUpdateRefs.Checked)
                {
                    updateRefChoice = checkBoxUpdateRefs.Checked;
                }

                Commands.RebaseOptions rebaseOptions = new()
                {
                    Interactive = chkInteractive.Checked,
                    PreserveMerges = chkPreserveMerges.Checked,
                    AutoSquash = chkAutosquash.Checked,
                    AutoStash = chkStash.Checked,
                    IgnoreDate = chkIgnoreDate.Checked,
                    CommitterDateIsAuthorDate = chkCommitterDateIsAuthorDate.Checked,
                    UpdateRefs = updateRefChoice,
                };

                if (chkSpecificRange.Checked && !string.IsNullOrWhiteSpace(txtFrom.Text) && !string.IsNullOrWhiteSpace(cboTo.Text))
                {
                    // Rebase onto
                    rebaseOptions.OnTo = cboBranches.Text;
                    rebaseOptions.From = txtFrom.Text;
                    rebaseOptions.BranchName = cboTo.Text;
                }
                else
                {
                    rebaseOptions.BranchName = cboBranches.Text;
                }

                string rebaseCmd = Commands.Rebase(rebaseOptions);

                string cmdOutput = FormProcess.ReadDialog(this, UICommands, arguments: rebaseCmd, Module.WorkingDir, input: null, useDialogSettings: true);
                if (cmdOutput.Trim() == "Current branch a is up to date.")
                {
                    MessageBox.Show(this, _branchUpToDateText.Text, _branchUpToDateCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if (!Module.InTheMiddleOfAction() &&
                    !Module.InTheMiddleOfPatch())
                {
                    Close();
                }

                EnableButtons();
                PatchGrid.Initialize();
            }
        }

        private void SolveMergeConflictsClick(object sender, EventArgs e)
        {
            MergetoolClick(sender, e);
        }

        private void ShowOptions_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            llblShowOptions.Visible = false;
            flpnlOptionsPanelTop.Visible = true;
            flpnlOptionsPanelBottom.Visible = true;
        }

        private void chkUseFromOnto_CheckedChanged(object sender, EventArgs e)
        {
            txtFrom.Enabled = chkSpecificRange.Checked;
            cboTo.Enabled = chkSpecificRange.Checked;
            btnChooseFromRevision.Enabled = chkSpecificRange.Checked;
        }

        private void btnChooseFromRevision_Click(object sender, EventArgs e)
        {
            bool previousValueBranchFilterEnabled = AppSettings.BranchFilterEnabled;
            bool previousValueShowCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
            bool previousValueShowReflogReferences = AppSettings.ShowReflogReferences;
            bool previousValueShowStashes = AppSettings.ShowStashes;

            try
            {
                AppSettings.ShowStashes = false;
                ObjectId firstParent = UICommands.Module.RevParse("HEAD~");
                string preSelectedCommit = !string.IsNullOrWhiteSpace(txtFrom.Text) ? txtFrom.Text : firstParent?.ToString() ?? string.Empty;

                string mergeBaseCommitId = null;

                if (!string.IsNullOrWhiteSpace(cboBranches.Text))
                {
                    try
                    {
                        ObjectId commit1 = UICommands.Module.RevParse(cboBranches.Text);
                        ObjectId commit2 = UICommands.Module.RevParse("HEAD");
                        mergeBaseCommitId = UICommands.Module.GetMergeBase(commit1, commit2)?.ToString();
                    }
                    catch (Exception)
                    {
                        // if an exception occurs, display whole history
                    }
                }

                using FormChooseCommit chooseForm = new(UICommands, preSelectedCommit, showCurrentBranchOnly: true, lastRevisionToDisplayHash: mergeBaseCommitId);
                if (chooseForm.ShowDialog(this) == DialogResult.OK && chooseForm.SelectedRevision is not null)
                {
                    txtFrom.Text = chooseForm.SelectedRevision.ObjectId.ToShortString();
                }
            }
            finally
            {
                AppSettings.ShowStashes = previousValueShowStashes;
                AppSettings.BranchFilterEnabled.Value = previousValueBranchFilterEnabled;
                AppSettings.ShowCurrentBranchOnly.Value = previousValueShowCurrentBranchOnly;
                AppSettings.ShowReflogReferences.Value = previousValueShowReflogReferences;
            }
        }

        private void Commit_Click(object sender, EventArgs e)
        {
            UICommands.StartCommitDialog(this);
            EnableButtons();
        }

        internal TestAccessor GetTestAccessor() => new(this);

        internal readonly struct TestAccessor
        {
            private readonly FormRebase _form;

            public TestAccessor(FormRebase form)
            {
                _form = form;
            }

            public CheckBox chkInteractive => _form.chkInteractive;
            public CheckBox chkPreserveMerges => _form.chkPreserveMerges;
            public CheckBox chkAutosquash => _form.chkAutosquash;
            public CheckBox chkStash => _form.chkStash;
            public CheckBox chkIgnoreDate => _form.chkIgnoreDate;
            public CheckBox chkCommitterDateIsAuthorDate => _form.chkCommitterDateIsAuthorDate;
        }
    }
}
