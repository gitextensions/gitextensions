using System;

using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormApplyPatch : GitExtensionsForm
    {
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
                AddFiles.Enabled = false;
                Resolved.Enabled = false;
                Mergetool.Enabled = false;
                Skip.Enabled = false;
                Abort.Enabled = false;
            }

            patchGrid1.Initialize();

            SolveMergeconflicts.Visible = GitCommandHelpers.InTheMiddleOfConflictedMerge();

            Resolved.Text = "Conflicts resolved";
            Mergetool.Text = "Solve conflicts";

            if (GitCommandHelpers.InTheMiddleOfConflictedMerge())
            {
                Mergetool.Text = ">Solve conflicts<";
                AcceptButton = Mergetool;
            }
            else
                if (GitCommandHelpers.InTheMiddleOfPatch())
                {
                    Resolved.Text = ">Conflicts resolved<";
                    AcceptButton = Resolved;
                }

        }

        

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private string SelectPatchFile(string initialDirectory)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter =
               "Patch file (*.Patch)|*.Patch";
            dialog.InitialDirectory = initialDirectory;
            dialog.Title = "Select patch file";
            return (dialog.ShowDialog() == DialogResult.OK) ? dialog.FileName : PatchFile.Text;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            PatchFile.Text = SelectPatchFile(@".");
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PatchFile.Text) && string.IsNullOrEmpty(PatchDir.Text))
            {
                MessageBox.Show("Please select a patch to apply");
                return;
            }
            Cursor.Current = Cursors.WaitCursor;
            if (PatchFileMode.Checked)
                new FormProcess(GitCommandHelpers.PatchCmd(PatchFile.Text)).ShowDialog();
            else
                new FormProcess(GitCommandHelpers.PatchDirCmd(PatchDir.Text)).ShowDialog();

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

        private void button1_Click_1(object sender, EventArgs e)
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
            RestorePosition("merge-patch");
            this.Text = "Apply patch (" + GitCommands.Settings.WorkingDir + ")";
        }

        private void BrowseDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();

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
    }
}
