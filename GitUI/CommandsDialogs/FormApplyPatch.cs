using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI.CommandsDialogs
{
    public partial class FormApplyPatch : GitModuleForm
    {
        #region Translation

        private readonly TranslationString _conflictResolvedText =
            new TranslationString("Conflicts resolved");
        private readonly TranslationString _conflictMergetoolText =
            new TranslationString("Solve conflicts");

        private readonly TranslationString _selectPatchFileFilter =
            new TranslationString("Patch file (*.Patch)");
        private readonly TranslationString _selectPatchFileCaption =
            new TranslationString("Select patch file");

        private readonly TranslationString _noFileSelectedText =
            new TranslationString("Please select a patch to apply");

        private readonly TranslationString _applyPatchMsgBox =
            new TranslationString("Apply patch");

        #endregion

        /// <summary>
        /// For VS designer
        /// </summary>
        private FormApplyPatch()
            : this(null)
        {
        }

        public FormApplyPatch(GitUICommands aCommands)
            : base(true, aCommands)
        {
            InitializeComponent(); Translate();
            if (aCommands != null)
                EnableButtons();
        }

        public void SetPatchFile(string name)
        {
            PatchFileMode.Checked = true;
            PatchFile.Text = name;
        }

        public void SetPatchDir(string name)
        {
            PatchDirMode.Checked = true;
            PatchDir.Text = name;
        }

        private void EnableButtons()
        {
            if (Module.InTheMiddleOfPatch())
            {
                Apply.Enabled = false;
                IgnoreWhitespace.Enabled = false;
                PatchFileMode.Enabled = false;
                PatchDirMode.Enabled = false;
                AddFiles.Enabled = true;
                Resolved.Enabled = !Module.InTheMiddleOfConflictedMerge();
                Mergetool.Enabled = Module.InTheMiddleOfConflictedMerge();
                Skip.Enabled = true;
                Abort.Enabled = true;

                PatchFile.Enabled = false;
                PatchFile.ReadOnly = false;
                BrowsePatch.Enabled = false;
                PatchDir.Enabled = false;
                PatchDir.ReadOnly = false;
                BrowseDir.Enabled = false;
            }
            else
            {
                PatchFile.Enabled = PatchFileMode.Checked;
                PatchFile.ReadOnly = !PatchFileMode.Checked;
                BrowsePatch.Enabled = PatchFileMode.Checked;
                PatchDir.Enabled = PatchDirMode.Checked;
                PatchDir.ReadOnly = !PatchDirMode.Checked;
                BrowseDir.Enabled = PatchDirMode.Checked;

                Apply.Enabled = true;
                IgnoreWhitespace.Enabled = true;
                PatchFileMode.Enabled = true;
                PatchDirMode.Enabled = true;
                AddFiles.Enabled = false;
                Resolved.Enabled = false;
                Mergetool.Enabled = false;
                Skip.Enabled = false;
                Abort.Enabled = false;
            }

            patchGrid1.Initialize();

            SolveMergeconflicts.Visible = Module.InTheMiddleOfConflictedMerge();

            Resolved.Text = _conflictResolvedText.Text;
            Mergetool.Text = _conflictMergetoolText.Text;
            ContinuePanel.BackColor = Color.Transparent;
            MergeToolPanel.BackColor = Color.Transparent;

            if (Module.InTheMiddleOfConflictedMerge())
            {
                Mergetool.Text = ">" + _conflictMergetoolText.Text + "<";
                Mergetool.Focus();
                AcceptButton = Mergetool;
                MergeToolPanel.BackColor = Color.Black;
            }
            else if (Module.InTheMiddleOfPatch())
            {
                Resolved.Text = ">" + _conflictResolvedText.Text + "<";
                Resolved.Focus();
                AcceptButton = Resolved;
                ContinuePanel.BackColor = Color.Black;
            }
        }

        private string SelectPatchFile(string initialDirectory)
        {
            using (var dialog = new OpenFileDialog
                             {
                                 Filter = _selectPatchFileFilter.Text + "|*.Patch",
                                 InitialDirectory = initialDirectory,
                                 Title = _selectPatchFileCaption.Text
                             })
            {
                return (dialog.ShowDialog(this) == DialogResult.OK) ? dialog.FileName : PatchFile.Text;
            }
        }

        private void BrowsePatch_Click(object sender, EventArgs e)
        {
            PatchFile.Text = SelectPatchFile(@".");
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PatchFile.Text) && string.IsNullOrEmpty(PatchDir.Text))
            {
                MessageBox.Show(this, _noFileSelectedText.Text);
                return;
            }
            Cursor.Current = Cursors.WaitCursor;
            if (PatchFileMode.Checked)
                if (IgnoreWhitespace.Checked)
                {
                    FormProcess.ShowDialog(this, GitCommandHelpers.PatchCmdIgnoreWhitespace(PatchFile.Text));
                }
                else
                {
                    FormProcess.ShowDialog(this, GitCommandHelpers.PatchCmd(PatchFile.Text));
                }
            else
                if (IgnoreWhitespace.Checked)
                {
                    Module.ApplyPatch(PatchDir.Text, GitCommandHelpers.PatchDirCmdIgnoreWhitespace());
                }
                else
                {
                    Module.ApplyPatch(PatchDir.Text, GitCommandHelpers.PatchDirCmd());
                }

            UICommands.RepoChangedNotifier.Notify();

            EnableButtons();

            if (!Module.InTheMiddleOfConflictedMerge() && !Module.InTheMiddleOfRebase() && !Module.InTheMiddleOfPatch())
                Close();
            Cursor.Current = Cursors.Default;
        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            UICommands.StartResolveConflictsDialog(this);
            EnableButtons();
        }

        private void Skip_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            FormProcess.ShowDialog(this, GitCommandHelpers.SkipCmd());
            EnableButtons();
            Cursor.Current = Cursors.Default;
        }

        private void Resolved_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            FormProcess.ShowDialog(this, (GitCommandHelpers.ResolvedCmd()));
            EnableButtons();
            Cursor.Current = Cursors.Default;
        }

        private void Abort_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            FormProcess.ShowDialog(this, GitCommandHelpers.AbortCmd());
            EnableButtons();
            Cursor.Current = Cursors.Default;
        }

        private void AddFiles_Click(object sender, EventArgs e)
        {
            UICommands.StartAddFilesDialog(this);
        }

        private void MergePatch_Load(object sender, EventArgs e)
        {
            PatchFile.Select();
            
            Text = _applyPatchMsgBox.Text + " (" + Module.WorkingDir + ")";
            IgnoreWhitespace.Checked = Settings.ApplyPatchIgnoreWhitespace;
        }

        private void BrowseDir_Click(object sender, EventArgs e)
        {
            using (var browseDialog = new FolderBrowserDialog())
            {

                if (browseDialog.ShowDialog(this) == DialogResult.OK)
                {
                    PatchDir.Text = browseDialog.SelectedPath;
                }
            }
        }

        private void PatchFileMode_CheckedChanged(object sender, EventArgs e)
        {
            EnableButtons();
        }

        private void PatchDirMode_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void SolveMergeconflicts_Click(object sender, EventArgs e)
        {
            Mergetool_Click(sender, e);
        }

        private void IgnoreWhitespace_CheckedChanged(object sender, EventArgs e)
        {
            Settings.ApplyPatchIgnoreWhitespace = IgnoreWhitespace.Checked;
        }
    }
}
