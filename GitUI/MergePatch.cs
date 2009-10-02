using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GitUI
{
    public partial class MergePatch : GitExtensionsForm
    {
        public MergePatch()
        {
            InitializeComponent();
            EnableButtons();
        }

        public void SetPatchFile(string name)
        {
            PatchFile.Text = name;
        }

        private void EnableButtons()
        {
            if (GitCommands.GitCommands.InTheMiddleOfPatch())
            {
                Apply.Enabled = false;
                AddFiles.Enabled = true;
                Resolved.Enabled = !GitCommands.GitCommands.InTheMiddleOfConflictedMerge();
                Mergetool.Enabled = GitCommands.GitCommands.InTheMiddleOfConflictedMerge();
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

            SolveMergeconflicts.Visible = GitCommands.GitCommands.InTheMiddleOfConflictedMerge();

            Resolved.Text = "Conflicts resolved";
            Mergetool.Text = "Solve conflicts";

            if (GitCommands.GitCommands.InTheMiddleOfConflictedMerge())
            {
                Mergetool.Text = ">Solve conflicts<";
                AcceptButton = Mergetool;
            }
            else
                if (GitCommands.GitCommands.InTheMiddleOfPatch())
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
            Cursor.Current = Cursors.WaitCursor;
            if (string.IsNullOrEmpty(PatchFile.Text) && string.IsNullOrEmpty(PatchDir.Text))
            {
                MessageBox.Show("Please select a patch to apply");
                return;
            }

            if (PatchFileMode.Checked)
                new FormProcess(GitCommands.GitCommands.PatchCmd(PatchFile.Text));
            else
                new FormProcess(GitCommands.GitCommands.PatchCmd(PatchDir.Text + "\\*.patch"));

            EnableButtons();

            if (!GitCommands.GitCommands.InTheMiddleOfConflictedMerge() && !GitCommands.GitCommands.InTheMiddleOfRebase() && !GitCommands.GitCommands.InTheMiddleOfPatch())
                Close();
        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartResolveConflictsDialog();
            EnableButtons();
        }

        private void Skip_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess(GitCommands.GitCommands.SkipCmd());
            EnableButtons();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess(GitCommands.GitCommands.ResolvedCmd());
            EnableButtons();
        }

        private void Abort_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            new FormProcess(GitCommands.GitCommands.AbortCmd());
            EnableButtons();
        }

        private void AddFiles_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartAddFilesDialog();
        }

        private void MergePatch_Load(object sender, EventArgs e)
        {
            this.Text = "Apply patch (" + GitCommands.Settings.WorkingDir + ")";
        }

        private void MergePatch_FormClosing(object sender, FormClosingEventArgs e)
        {
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
