using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using System.IO;

namespace GitUI
{
    delegate void DoneCallback();

    public partial class FormCommit : GitExtensionsForm
    {
        public FormCommit()
        {
            InitializeComponent();
        }

        ~FormCommit()  // destructor
        {
            gitGetUnstagedCommand.Kill();
        }

        private void FormCommit_Load(object sender, EventArgs e)
        {
            Initialize();
            this.Text = "Commit (" + GitCommands.Settings.WorkingDir + ")";
            Message.Text = GitCommands.GitCommands.GetMergeMessage();

        }

        GitCommands.GitCommands gitGetUnstagedCommand = new GitCommands.GitCommands();

        private bool IsLoadingUnstagedFiles()
        {
            if (gitGetUnstagedCommand.Process == null) 
                return false;

            return !gitGetUnstagedCommand.Process.HasExited;
        }

        private void Initialize()
        {
            SolveMergeconflicts.Visible = GitCommands.GitCommands.InTheMiddleOfConflictedMerge();

            //Load unstaged files
            gitGetUnstagedCommand.Exited += new EventHandler(gitCommands_Exited);
            gitGetUnstagedCommand.CmdStartProcess(Settings.GitDir + "git.cmd", GitCommands.GitCommands.GetAllChangedFilesCmd);
            Loading.Visible = true;
            AddFiles.Enabled = false;

            //Load staged files
            List<GitItemStatus> stagedFiles = GitCommands.GitCommands.GetStagedFiles();
            Staged.DataSource = stagedFiles;
        }

        // This method is passed in to the SetTextCallBack delegate
        // to set the Text property of textBox1.
        private void LoadUnstagedOutput()
        {
            Unstaged.DataSource = GitCommands.GitCommands.GetAllChangedFilesFromString(gitGetUnstagedCommand.Output.ToString());
            Loading.Visible = false;
            AddFiles.Enabled = true;
        }

        void gitCommands_Exited(object sender, EventArgs e)
        {
            if (Unstaged.InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                DoneCallback d = new DoneCallback(LoadUnstagedOutput);
                this.Invoke(d, new object[] {});
            }
            else
            {
                LoadUnstagedOutput();
            }

        }

        protected void ShowChanges(GitItemStatus item, bool staged)
        {
            

            //EditorOptions.SetSyntax(SelectedDiff, item.Name);
            SelectedDiff.SetHighlighting("Patch");
            //ICSharpCode.TextEditor.Highlight h = new ICSharpCode.TextEditor.Highlight(new ICSharpCode.TextEditor.TextLocation(1, 10), new ICSharpCode.TextEditor.TextLocation(1, 15));
            
            SelectedDiff.Text = GitCommands.GitCommands.GetCurrentChanges(item.Name, staged);
            SelectedDiff.Refresh();
        }

        private void Tracked_SelectionChanged(object sender, EventArgs e)
        {
            if (Staged.SelectedRows.Count == 0) return;

            if (Staged.SelectedRows[0].DataBoundItem is GitItemStatus)
            {
                ShowChanges((GitItemStatus)Staged.SelectedRows[0].DataBoundItem, true);
            }
        }

        private void Untracked_SelectionChanged(object sender, EventArgs e)
        {
            if (Unstaged.SelectedRows.Count == 0) return;

            if (Unstaged.SelectedRows[0].DataBoundItem is GitItemStatus)
            {
                ShowChanges((GitItemStatus)Unstaged.SelectedRows[0].DataBoundItem, false);
            }
        }

        public bool NeedRefresh = false;

        private void Commit_Click(object sender, EventArgs e)
        {
            DoCommit(false);
        }

        private void DoCommit(bool amend)
        {
            if (GitCommands.GitCommands.InTheMiddleOfConflictedMerge())
            {
                MessageBox.Show("There are unresolved mergeconflicts, solve mergeconflicts before committing", "Merge conflicts");
                return;
            }
            if (Message.Text.Length == 0)
            {
                MessageBox.Show("Please enter commit message");
                return;
            }
            
            try
            {
                new FormProcess(GitCommands.GitCommands.CommitCmd(Message.Text.Replace("\"", "'"), amend));

                //OutPut.Text = "";

                //CommitDto dto = new CommitDto(Message.Text, amend);
                //GitCommands.Commit commit = new GitCommands.Commit(dto);
                //commit.Execute();

                //if (OutPut.Text.Length == 0)
                //    OutPut.Text = "Command executed \n";

                //OutPut.Text += dto.Result;

                NeedRefresh = true;

                Close();
            }
            catch
            {
            }
        }

        private void Scan_Click(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Stage_Click(object sender, EventArgs e)
        {
            Loading.Visible = true;
            progressBar.Visible = true;
            progressBar.Maximum = Unstaged.SelectedRows.Count * 2;
            progressBar.Value = 0;

            List<GitItemStatus> files = new List<GitItemStatus>();

            foreach (DataGridViewRow row in Unstaged.SelectedRows)
            {
                if (row.DataBoundItem is GitItemStatus)
                {
                    progressBar.Value = Math.Min(progressBar.Maximum - 1, progressBar.Value + 1);
                    GitItemStatus item = (GitItemStatus)row.DataBoundItem;
                    files.Add(item);
                }
            }

            OutPut.Text = GitCommands.GitCommands.StageFiles(files);

            progressBar.Value = progressBar.Maximum;

            Initialize();
            progressBar.Visible = false;
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset all changes in the working dir?\nAll changes made to all files in the workin dir will be overwritten by the files from the current HEAD!", "WARNING!", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (MessageBox.Show("Are you really sure you want to DELETE all changes?", "WARNING! WARNING!", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    OutPut.Text = GitCommands.GitCommands.ResetHard("");
                    Initialize();
                }
            }
        }

        private void UnstageFiles_Click(object sender, EventArgs e)
        {

            if (Staged.SelectedRows.Count == Staged.RowCount)
            {
                OutPut.Text = GitCommands.GitCommands.ResetMixed("HEAD");
            } else
            {
                Loading.Visible = true;
                progressBar.Visible = true;
                progressBar.Maximum = Staged.SelectedRows.Count * 2;
                progressBar.Value = 0;

                List<GitItemStatus> files = new List<GitItemStatus>();
                string result = "";
                foreach (DataGridViewRow row in Staged.SelectedRows)
                {
                    if (row.DataBoundItem is GitItemStatus)
                    {
                        progressBar.Value = Math.Min(progressBar.Maximum - 1, progressBar.Value + 1);
                        GitItemStatus item = (GitItemStatus)row.DataBoundItem;
                        if (!item.IsNew)
                        {
                            progressBar.Value = Math.Min(progressBar.Maximum - 1, progressBar.Value + 1);
                            result = GitCommands.GitCommands.UnstageFileToRemove(item.Name);
                        }
                        else
                        {
                            files.Add(item);
                        }
                    }
                }

                OutPut.Text = result + "\n" + GitCommands.GitCommands.UnstageFiles(files);

                progressBar.Value = progressBar.Maximum;
            }
            Initialize();
            progressBar.Visible = false;

            /*
            Loading.Visible = true;
            string result = "";
            progressBar.Visible = true;
            progressBar.Maximum = Staged.SelectedRows.Count;
            progressBar.Value = 0;
            foreach (DataGridViewRow row in Staged.SelectedRows)
            {
                progressBar.Value = Math.Min(progressBar.Maximum - 1, progressBar.Value + 1);
                if (row.DataBoundItem is GitItemStatus)
                {
                    GitItemStatus item = (GitItemStatus)row.DataBoundItem;

                    if (!item.IsNew)
                        result = GitCommands.GitCommands.UnstageFileToRemove(item.Name);
                    else
                        result = GitCommands.GitCommands.UnstageFile(item.Name);
                    if (result.Length > 0)
                        OutPut.Text += result;  
                }
            }
           
            Initialize();
            progressBar.Visible = false;*/
        }

        private void splitContainer8_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void Staged_Click(object sender, EventArgs e)
        {
            Tracked_SelectionChanged(sender, e);
        }

        private void Unstaged_Click(object sender, EventArgs e)
        {
            Untracked_SelectionChanged(sender, e);
        }

        private void AddManyFiles_Click(object sender, EventArgs e)
        {
            new FormAddFiles().ShowDialog();
            Initialize();
        }

        private void Amend_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("You are about to rewite history.\nOnly use amend if the commit is not published yet!\n\nDo you want to continue?", "Amend commit", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DoCommit(true);
                Close();
            }
        }

        private void ResetSoft_Click(object sender, EventArgs e)
        {
            if (Unstaged.Rows.Count > LastRow && LastRow >= 0 && MessageBox.Show("Are you sure you want to reset the changes of this?", "Reset", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                GitItemStatus item = (GitItemStatus)Unstaged.Rows[LastRow].DataBoundItem;
                string output = GitCommands.GitCommands.ResetFile(item.Name);

                if (!string.IsNullOrEmpty(output))
                    MessageBox.Show(output, "Reset changes");

                Initialize();
            }
        }

        int LastRow;

        private void Unstaged_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                System.Drawing.Point pt = Unstaged.PointToClient(Cursor.Position);
                DataGridView.HitTestInfo hti = Unstaged.HitTest(pt.X, pt.Y);
                LastRow = hti.RowIndex;
                Unstaged.ClearSelection();
                if (LastRow >= 0 && Unstaged.Rows.Count > LastRow)
                    Unstaged.Rows[LastRow].Selected = true;
            }
        }

        private void deleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Unstaged.Rows.Count > LastRow && LastRow >= 0 && MessageBox.Show("Are you sure you want delete this file?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                GitItemStatus item = (GitItemStatus)Unstaged.Rows[LastRow].DataBoundItem;
                File.Delete(GitCommands.Settings.WorkingDir + item.Name);
                Initialize();
            }

        }

        private void SolveMergeconflicts_Click(object sender, EventArgs e)
        {
            new FormResolveConflicts().ShowDialog();
            Initialize();
        }

        private void deleteSelectedFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all selected files?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                foreach (DataGridViewRow row in Unstaged.Rows)
                {
                    GitItemStatus item = (GitItemStatus)row.DataBoundItem;
                    File.Delete(GitCommands.Settings.WorkingDir + item.Name);
                }
                Initialize();
            }
        }

        private void resetSelectedFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset all selected files?", "Reset", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

                foreach (DataGridViewRow row in Unstaged.Rows)
                {
                    GitItemStatus item = (GitItemStatus)row.DataBoundItem;

                    string output = GitCommands.GitCommands.ResetFile(item.Name);
                }
                Initialize();
            }
        }

        private void resetAlltrackedChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Reset_Click(null, null);
        }

        private void eToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormGitIgnore().ShowDialog();
            Initialize();
        }
    }
}
