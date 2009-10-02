using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using System.IO;
using System.Collections;

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
            UnstageFiles.Enabled = false;
            AddFiles.Enabled = false;
            filesListedToCommitToolStripMenuItem.Enabled = false;
            workingToolStripMenuItem.Enabled = false;

            Cursor.Current = Cursors.WaitCursor;

            //Load unstaged files
            gitGetUnstagedCommand.Exited += new EventHandler(gitCommands_Exited);
            gitGetUnstagedCommand.CmdStartProcess(Settings.GitDir + "git.cmd", GitCommands.GitCommands.GetAllChangedFilesCmd);
            Loading.Visible = true;
            AddFiles.Enabled = false;

            InitializedStaged();

            Commit.Focus();
            AcceptButton = Commit;
        }

        private void InitializedStaged()
        {
            Cursor.Current = Cursors.WaitCursor;
            SolveMergeconflicts.Visible = GitCommands.GitCommands.InTheMiddleOfConflictedMerge();

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

            UnstageFiles.Enabled = true;
            AddFiles.Enabled = true;
            filesListedToCommitToolStripMenuItem.Enabled = true;
            workingToolStripMenuItem.Enabled = true;
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
            if (item.Name.EndsWith(".png"))
            {
                SelectedDiff.ViewFile(item.Name);
            } else
            if (item.IsTracked)
            {
                SelectedDiff.ViewCurrentChanges(item.Name, "Patch", staged);
            }
            else
            {
                SelectedDiff.ViewFile(item.Name);
            }
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
            if (Staged.RowCount == 0)
            {
                if (MessageBox.Show("There are no files staged for this commit. Are you sure you want to commit?", "No files staged", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

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

            if (GitCommands.GitCommands.GetSelectedBranch().CompareTo("(no branch)") == 0)
            {
                if (MessageBox.Show("You are not working on a branch." + Environment.NewLine + "This commit will be unreferenced when switching to another brach and can be lost." + Environment.NewLine + "" + Environment.NewLine + "Do you want to continue?", "Not on a branch.", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
            }

            try
            {
                using (StreamWriter textWriter = new StreamWriter(GitCommands.Settings.WorkingDirGitDir() + "\\COMMITMESSAGE", false))
                {
                    textWriter.Write(Message.Text);
                    textWriter.Flush();
                    textWriter.Close();
                }

                FormProcess form = new FormProcess(GitCommands.GitCommands.CommitCmd(amend));
                
                NeedRefresh = true;

                if (!form.ErrorOccured())
                {
                    Close();
                    File.Delete(GitCommands.Settings.WorkingDirGitDir() + "\\COMMITMESSAGE");
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("Exception: " + e.Message);
            }
        }

        private void Scan_Click(object sender, EventArgs e)
        {
            Scan.Enabled = false;
            Initialize();
            Scan.Enabled = true;
        }

        private void Stage_Click(object sender, EventArgs e)
        {
            IList rows = Unstaged.SelectedRows;
            Stage(rows);
        }

        private void Stage(IList rows)
        {
            UnstageFiles.Enabled = false;
            AddFiles.Enabled = false;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                //Loading.Visible = true;
                progressBar.Visible = true;
                progressBar.Maximum = Unstaged.SelectedRows.Count * 2;
                progressBar.Value = 0;

                List<GitItemStatus> files = new List<GitItemStatus>();

                foreach (DataGridViewRow row in rows)
                {
                    if (row.DataBoundItem is GitItemStatus)
                    {
                        progressBar.Value = Math.Min(progressBar.Maximum - 1, progressBar.Value + 1);
                        GitItemStatus item = (GitItemStatus)row.DataBoundItem;
                        files.Add(item);
                    }
                }

                OutPut.Text = GitCommands.GitCommands.StageFiles(files);

                InitializedStaged();
                List<GitItemStatus> stagedFiles = (List<GitItemStatus>)Staged.DataSource;
                List<GitItemStatus> unStagedFiles = (List<GitItemStatus>)Unstaged.DataSource;
                Unstaged.DataSource = null;

                unStagedFiles.RemoveAll(item => stagedFiles.Exists(i => i.Name == item.Name));
                /*foreach (GitItemStatus item in files)
                {
                    if (stagedFiles.Exists(i => i.Name == item.Name))
                    {
                        if (unStagedFiles.Contains(item))
                            unStagedFiles.RemoveAll(i => stagedFiles.Exists(i => i.Name == item.Name));
                    }
                }*/
                Unstaged.DataSource = unStagedFiles;

                progressBar.Value = progressBar.Maximum;

                //Initialize();
                progressBar.Visible = false;
            }
            catch
            {
            }
            UnstageFiles.Enabled = true;
            AddFiles.Enabled = true;

            Commit.Enabled = true;
            Amend.Enabled = true;
            AcceptButton = Commit;
            Commit.Focus();
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset all changes in the working dir?" + Environment.NewLine + "All changes made to all files in the workin dir will be overwritten by the files from the current HEAD!", "WARNING!", MessageBoxButtons.YesNo) == DialogResult.Yes)
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
            UnstageFiles.Enabled = false;
            AddFiles.Enabled = false;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (Staged.SelectedRows.Count == Staged.RowCount && Staged.RowCount > 10)
                {
                    Loading.Visible = true;
                    OutPut.Text = GitCommands.GitCommands.ResetMixed("HEAD");
                    Initialize();
                }
                else
                {
                    //Loading.Visible = true;
                    progressBar.Visible = true;
                    progressBar.Maximum = Staged.SelectedRows.Count * 2;
                    progressBar.Value = 0;

                    List<GitItemStatus> files = new List<GitItemStatus>();
                    List<GitItemStatus> allFiles = new List<GitItemStatus>();
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
                            allFiles.Add(item);

                        }
                    }

                    OutPut.Text = result + Environment.NewLine + GitCommands.GitCommands.UnstageFiles(files);

                    InitializedStaged();
                    List<GitItemStatus> stagedFiles = (List<GitItemStatus>)Staged.DataSource;
                    List<GitItemStatus> unStagedFiles = (List<GitItemStatus>)Unstaged.DataSource;
                    Unstaged.DataSource = null;
                    foreach (GitItemStatus item in allFiles)
                    {
                        if (!stagedFiles.Exists(i => i.Name == item.Name))
                        {
                            if (!unStagedFiles.Exists(i => i.Name == item.Name))
                            {
                                if (item.IsNew && !item.IsChanged && !item.IsDeleted)
                                    item.IsTracked = false;
                                else
                                    item.IsTracked = true;

                                unStagedFiles.Add(item);
                            }
                        }
                    }
                    Staged.DataSource = stagedFiles;
                    Unstaged.DataSource = unStagedFiles;

                    progressBar.Value = progressBar.Maximum;
                }
                //Initialize();
                progressBar.Visible = false;
            }
            catch
            {
            }
            UnstageFiles.Enabled = true;
            AddFiles.Enabled = true;

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
            if (GitUICommands.Instance.StartAddFilesDialog())
               Initialize();
        }

        private void Amend_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (MessageBox.Show("You are about to rewrite history." + Environment.NewLine + "Only use amend if the commit is not published yet!" + Environment.NewLine + Environment.NewLine + "Do you want to continue?", "Amend commit", MessageBoxButtons.YesNo) == DialogResult.Yes)
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
            try
            {
                SelectedDiff.ViewText("", "");
                if (Unstaged.Rows.Count > LastRow && LastRow >= 0 && MessageBox.Show("Are you sure you want delete this file?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitItemStatus item = (GitItemStatus)Unstaged.Rows[LastRow].DataBoundItem;
                    File.Delete(GitCommands.Settings.WorkingDir + item.Name);
                    Initialize();
                }
            }
            catch
            {
                MessageBox.Show("Delete file failed");
            }
        }

        private void SolveMergeconflicts_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartResolveConflictsDialog())
                Initialize();
        }

        private void deleteSelectedFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all selected files?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                foreach (DataGridViewRow row in Unstaged.SelectedRows)
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

                foreach (DataGridViewRow row in Unstaged.SelectedRows)
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
            GitUICommands.Instance.StartEditGitIgnoreDialog();
            Initialize();
        }

        private void stageAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList rows = Unstaged.Rows;
            Stage(rows);
        }

        private void unstageAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OutPut.Text = GitCommands.GitCommands.ResetMixed("HEAD");
            Initialize();
        }

        private void Unstaged_DoubleClick(object sender, EventArgs e)
        {
            if (Unstaged == null || Unstaged.SelectedRows.Count == 0)
                return;

            if (Unstaged.SelectedRows[0].DataBoundItem is GitItemStatus)
            {
                {
                    GitUICommands.Instance.StartFileHistoryDialog(((GitItemStatus)Unstaged.SelectedRows[0].DataBoundItem).Name);
                }
            }
        }

        private void FormCommit_Shown(object sender, EventArgs e)
        {
            Initialize();
            this.Text = "Commit (" + GitCommands.Settings.WorkingDir + ")";
            Message.Text = GitCommands.GitCommands.GetMergeMessage();

            if (string.IsNullOrEmpty(Message.Text) && File.Exists(GitCommands.Settings.WorkingDirGitDir() + "\\COMMITMESSAGE"))
                Message.Text = File.ReadAllText(GitCommands.Settings.WorkingDirGitDir() + "\\COMMITMESSAGE");

        }

        private void Staged_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void SpellCheck_Click(object sender, EventArgs e)
        {
            Message.CheckSpelling();
        }

        private void Message_Load(object sender, EventArgs e)
        {

        }

        private void FormCommit_FormClosing(object sender, FormClosingEventArgs e)
        {
            using (StreamWriter textWriter = new StreamWriter(GitCommands.Settings.WorkingDirGitDir() + "\\COMMITMESSAGE", false))
            {
                textWriter.Write(Message.Text);
                textWriter.Close();
            }
        }

        private void deleteAllUntrackedFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all untracked?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                new FormProcess("clean -f");
                Initialize();
            }
        }

        private void stageChunkOfFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Unstaged.SelectedRows.Count != 1)
            {
                MessageBox.Show("You can only use this option when selecting a single file", "Stage chunk");
                return;
            }

            foreach (DataGridViewRow row in Unstaged.SelectedRows)
            {
                GitItemStatus item = (GitItemStatus)row.DataBoundItem;

                GitCommands.GitCommands.RunRealCmd(Settings.GitDir + "git.cmd", "add -p \"" + item.Name + "\"");
                Initialize();
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
