using System;
using System.Windows.Forms;
using GitCommands;
using System.Drawing;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormApplyPatch : GitExtensionsForm
    {
        #region TranslationStrings

        private readonly TranslationString _conflictResolvedText =
            new TranslationString("Conflicts resolved");
        private readonly TranslationString _conflictMergetoolText =
            new TranslationString("Solve conflicts");
        private readonly TranslationString _conflictMergetoolText2 =
            new TranslationString(">Solve conflicts<");
        private readonly TranslationString _conflictResolvedText2 = 
            new TranslationString(">Conflicts resolved<");

        private readonly TranslationString _selectPatchFileFilter =
            new TranslationString("Patch file (*.Patch)");
        private readonly TranslationString _selectPatchFileCaption =
            new TranslationString("Select patch file");

        private readonly TranslationString _noFileSelectedText =
            new TranslationString("Please select a patch to apply");

        private readonly TranslationString _applyPatchMsgBox =
            new TranslationString("Apply patch");
                    
        #endregion

        public FormApplyPatch()
        {
            InitializeComponent(); Translate();
            EnableButtons();
        }

        public void SetPatchFile(string name)
        {
            PatchFile.Text = name;
        }

        private void EnableButtons()
        {
            if (GitCommandHelpers.InTheMiddleOfPatch())
            {
                Apply.Enabled = false;
                IgnoreWhitespace.Enabled = false;
                PatchFileMode.Enabled = false;
                PatchDirMode.Enabled = false;
                AddFiles.Enabled = true;
                Resolved.Enabled = !GitCommandHelpers.InTheMiddleOfConflictedMerge();
                Mergetool.Enabled = GitCommandHelpers.InTheMiddleOfConflictedMerge();
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

            SolveMergeconflicts.Visible = GitCommandHelpers.InTheMiddleOfConflictedMerge();

            Resolved.Text = _conflictResolvedText.Text;
            Mergetool.Text = _conflictMergetoolText.Text;
            ContinuePanel.BackColor = Color.Transparent;
            MergeToolPanel.BackColor = Color.Transparent;

            if (GitCommandHelpers.InTheMiddleOfConflictedMerge())
            {
                Mergetool.Text = _conflictMergetoolText2.Text;
                Mergetool.Focus();
                AcceptButton = Mergetool;
                MergeToolPanel.BackColor = Color.Black;
            }
            else
                if (GitCommandHelpers.InTheMiddleOfPatch())
                {
                    Resolved.Text = _conflictResolvedText2.Text;
                    Resolved.Focus();
                    AcceptButton = Resolved;
                    ContinuePanel.BackColor = Color.Black;
                }

        }

        private string SelectPatchFile(string initialDirectory)
        {
            var dialog = new OpenFileDialog
                             {
                                 Filter = _selectPatchFileFilter.Text + "|*.Patch",
                                 InitialDirectory = initialDirectory,
                                 Title = _selectPatchFileCaption.Text
                             };
            return (dialog.ShowDialog() == DialogResult.OK) ? dialog.FileName : PatchFile.Text;
        }


        private void BrowsePatch_Click(object sender, EventArgs e)
        {
            PatchFile.Text = SelectPatchFile(@".");
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PatchFile.Text) && string.IsNullOrEmpty(PatchDir.Text))
            {
                MessageBox.Show(_noFileSelectedText.Text);
                return;
            }
            Cursor.Current = Cursors.WaitCursor;
            if (PatchFileMode.Checked)
                if (IgnoreWhitespace.Checked)
                {
                    new FormProcess(GitCommandHelpers.PatchCmdIgnoreWhitespace(PatchFile.Text)).ShowDialog();
                }
                else
                {
                    new FormProcess(GitCommandHelpers.PatchCmd(PatchFile.Text)).ShowDialog();
                }
            else
                if (IgnoreWhitespace.Checked)
                {
                    new FormProcess(GitCommandHelpers.PatchDirCmdIgnoreWhitespace(PatchDir.Text)).ShowDialog();
                }
                else
                {
                    new FormProcess(GitCommandHelpers.PatchDirCmd(PatchDir.Text)).ShowDialog();
                }

            EnableButtons();

            if (!GitCommandHelpers.InTheMiddleOfConflictedMerge() && !GitCommandHelpers.InTheMiddleOfRebase() && !GitCommandHelpers.InTheMiddleOfPatch())
                Close();
            Cursor.Current = Cursors.Default;
        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartResolveConflictsDialog();
            EnableButtons();
        }

        private void Skip_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess(GitCommandHelpers.SkipCmd()).ShowDialog();
            EnableButtons();
            Cursor.Current = Cursors.Default;
        }

        private void Resolved_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess(GitCommandHelpers.ResolvedCmd()).ShowDialog();
            EnableButtons();
            Cursor.Current = Cursors.Default;
        }

        private void Abort_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess(GitCommandHelpers.AbortCmd()).ShowDialog();
            EnableButtons();
            Cursor.Current = Cursors.Default;
        }

        private void AddFiles_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartAddFilesDialog();
        }

        private void MergePatch_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("merge-patch");
        }

        private void MergePatch_Load(object sender, EventArgs e)
        {
            PatchFile.Select();
            RestorePosition("merge-patch");
            Text = _applyPatchMsgBox.Text + " (" + Settings.WorkingDir + ")";
            IgnoreWhitespace.Checked = Settings.ApplyPatchIgnoreWhitespace;
        }

        private void BrowseDir_Click(object sender, EventArgs e)
        {
            var browseDialog = new FolderBrowserDialog();

            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                PatchDir.Text = browseDialog.SelectedPath;
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
