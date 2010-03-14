using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using System.IO;
using System.Collections;
using System.Collections.Specialized;

namespace GitUI
{
    delegate void DoneCallback();

    public partial class FormCommit : GitExtensionsForm
    {
        public FormCommit()
        {
            InitializeComponent();
            SelectedDiff.ExtraDiffArgumentsChanged += new EventHandler<EventArgs>(SelectedDiff_ExtraDiffArgumentsChanged);

            CloseCommitDialogTooltip.SetToolTip(CloseDialogAfterCommit, "When checked the commit dialog is closed after each commit.\nOtherwise the dialog will only close when there are no modified files left.");

            CloseDialogAfterCommit.Checked = Settings.CloseCommitDialogAfterCommit;

            Unstaged.MouseMove += new MouseEventHandler(Unstaged_MouseMove);
            Unstaged.MouseDown += new MouseEventHandler(Unstaged_MouseDown);
        }

        private Rectangle dragBoxFromMouseDown;

        void Unstaged_MouseDown(object sender, MouseEventArgs e)
        {
            System.Drawing.Point pt = Unstaged.PointToClient(Cursor.Position);
            DataGridView.HitTestInfo hti = Unstaged.HitTest(pt.X, pt.Y);
            LastRow = hti.RowIndex;

            if (e.Button == MouseButtons.Right)
            {
                Unstaged.ClearSelection();

                if (LastRow >= 0 && Unstaged.Rows.Count > LastRow)
                    Unstaged.Rows[LastRow].Selected = true;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (LastRow != -1)
                {
                    // Remember the point where the mouse down occurred. 
                    // The DragSize indicates the size that the mouse can move 
                    // before a drag event should be started.               
                    Size dragSize = SystemInformation.DragSize;

                    // Create a rectangle using the DragSize, with the mouse position being
                    // at the center of the rectangle.
                    dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                                   e.Y - (dragSize.Height / 2)),
                                                            dragSize);
                }
                else
                    // Reset the rectangle if the mouse is not over an item in the ListBox.
                    dragBoxFromMouseDown = Rectangle.Empty;
            }
        }

        void Unstaged_MouseMove(object sender, MouseEventArgs e)
        {
            // If the mouse moves outside the rectangle, start the drag.
            if (dragBoxFromMouseDown != Rectangle.Empty &&
                !dragBoxFromMouseDown.Contains(e.X, e.Y))
            {
                if (Unstaged.SelectedRows.Count > 0)
                {
                    StringCollection fileList = new StringCollection();

                    foreach (DataGridViewRow row in Unstaged.SelectedRows)
                    {
                        GitItemStatus item = (GitItemStatus)row.DataBoundItem;
                        string fileName = Settings.WorkingDir + item.Name;

                        fileList.Add(fileName.Replace('/', '\\'));
                    }

                    DataObject obj = new DataObject();
                    obj.SetFileDropList(fileList);

                    // Proceed with the drag and drop, passing in the list item.                   
                    DragDropEffects dropEffect = Unstaged.DoDragDrop(
                                                     obj,
                                                     DragDropEffects.Copy);
                    dragBoxFromMouseDown = Rectangle.Empty;
                }
            }
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
            gitGetUnstagedCommand.CmdStartProcess(Settings.GitCommand, GitCommands.GitCommands.GetAllChangedFilesCmd(!showIgnoredFilesToolStripMenuItem.Checked, showUntrackedFilesToolStripMenuItem.Checked));
            Loading.Visible = true;
            AddFiles.Enabled = false;

            InitializedStaged();

            Commit.Focus();
            AcceptButton = Commit;
        }

        private void InitializedStaged()
        {
            Cursor.Current = Cursors.WaitCursor;

            ThreadPool.QueueUserWorkItem(delegate
            {
                bool inTheMiddleOfConflictedMerge = GitCommands.GitCommands.InTheMiddleOfConflictedMerge();
                List<GitItemStatus> stagedFiles = GitCommands.GitCommands.GetStagedFiles();

                BeginInvoke((Action)delegate
                {
                    SolveMergeconflicts.Visible = inTheMiddleOfConflictedMerge;
                    Staged.DataSource = stagedFiles;
                });
            });
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

        private GitItemStatus currentItem;
        private bool currentItemStaged;
        protected void ShowChanges(GitItemStatus item, bool staged)
        {
            currentItem = item;
            currentItemStaged = staged;

            if (item == null)
                return;

            if (item.Name.EndsWith(".png"))
            {
                SelectedDiff.ViewFile(item.Name);
            } else
            if (item.IsTracked)
            {
                SelectedDiff.ViewCurrentChanges(item.Name, staged);
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
                using (StreamWriter textWriter = new StreamWriter(GitCommands.Settings.WorkingDirGitDir() + "\\COMMITMESSAGE", false, Settings.Encoding))
                {
                    int lineNumber = 0;
                    foreach (string line in Message.Text.Split('\n'))
                    {
                        if (lineNumber == 1 && !string.IsNullOrEmpty(line))
                            textWriter.WriteLine();

                        textWriter.WriteLine(line);
                        lineNumber++;
                    }
                }

                FormProcess form = new FormProcess(GitCommands.GitCommands.CommitCmd(amend));
                
                NeedRefresh = true;

                if (!form.ErrorOccured())
                {
                    Message.Text = string.Empty;

                    if (CloseDialogAfterCommit.Checked)
                    {
                        Close();
                    }
                    else
                    {
                        foreach (DataGridViewRow row in Unstaged.Rows)
                        {
                            if (((GitItemStatus)row.DataBoundItem).IsTracked)
                            {
                                InitializedStaged();
                                return;
                            }
                        }

                        Close();
                    }
                        
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

                /*OutPut.Text = */GitCommands.GitCommands.StageFiles(files);

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
            if (Abort.AbortCurrentAction())
            {
                Initialize();
                NeedRefresh = true;
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
                    /*OutPut.Text =*/ GitCommands.GitCommands.ResetMixed("HEAD");
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

                    /*OutPut.Text = result + Environment.NewLine + */GitCommands.GitCommands.UnstageFiles(files);

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

        private void Amend_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            if (string.IsNullOrEmpty(Message.Text))
            {
                Message.Text = GitCommands.GitCommands.GetPreviousCommitMessage(0);
                return;
            }

            if (MessageBox.Show("You are about to rewrite history." + Environment.NewLine + "Only use amend if the commit is not published yet!" + Environment.NewLine + Environment.NewLine + "Do you want to continue?", "Amend commit", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DoCommit(true);
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
                try
                {
                    foreach (DataGridViewRow row in Unstaged.SelectedRows)
                    {
                        GitItemStatus item = (GitItemStatus)row.DataBoundItem;
                        File.Delete(GitCommands.Settings.WorkingDir + item.Name);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Delete failed:" + Environment.NewLine + ex.ToString());
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
            /*OutPut.Text =*/ GitCommands.GitCommands.ResetMixed("HEAD");
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

            ThreadPool.QueueUserWorkItem(delegate
            {
                string text = "Commit to " + GitCommands.GitCommands.GetSelectedBranch() + " (" + Settings.WorkingDir + ")";
                string message = GitCommands.GitCommands.GetMergeMessage();

                if (string.IsNullOrEmpty(message) && File.Exists(Settings.WorkingDirGitDir() + "\\COMMITMESSAGE"))
                    message = File.ReadAllText(Settings.WorkingDirGitDir() + "\\COMMITMESSAGE", Settings.Encoding);

                BeginInvoke((Action)delegate
                {
                    Text = text;
                    Message.Text = message;
                });
            });
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
            if (!string.IsNullOrEmpty(Message.Text))
            {
                using (StreamWriter textWriter = new StreamWriter(GitCommands.Settings.WorkingDirGitDir() + "\\COMMITMESSAGE", false))
                {
                    textWriter.Write(Message.Text);
                    textWriter.Close();
                }
            }
            else
            {
                File.Delete(GitCommands.Settings.WorkingDirGitDir() + "\\COMMITMESSAGE");
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

                GitCommands.GitCommands.RunRealCmd(Settings.GitCommand, "add -p \"" + item.Name + "\"");
                Initialize();
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void showIgnoredFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showIgnoredFilesToolStripMenuItem.Checked = !showIgnoredFilesToolStripMenuItem.Checked;
            Scan_Click(null, null);
        }

        private void commitMessageToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            commitMessageToolStripMenuItem.DropDownItems.Clear();
            AddCommitMessageToMenu(0);
            AddCommitMessageToMenu(1);
            AddCommitMessageToMenu(2);
            AddCommitMessageToMenu(3);
        }

        private void AddCommitMessageToMenu(int numberBack)
        {
            string commitMessage = GitCommands.GitCommands.GetPreviousCommitMessage(numberBack);
            if (string.IsNullOrEmpty(commitMessage))
                return;
            
            ToolStripMenuItem toolStripItem = new ToolStripMenuItem();
            toolStripItem.Tag = commitMessage;
            toolStripItem.Text = commitMessage.Substring(0, Math.Min(Math.Min(50, commitMessage.Length), commitMessage.IndexOf('\n'))) + "...";
            
            commitMessageToolStripMenuItem.DropDownItems.Add(toolStripItem);
        }

        private void commitMessageToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Message.Text = (string)e.ClickedItem.Tag;
        }

        private void addFileTogitignoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Unstaged.Rows.Count > LastRow && LastRow >= 0)
            {
                SelectedDiff.ViewText("", "");
                GitItemStatus item = (GitItemStatus)Unstaged.Rows[LastRow].DataBoundItem;
                new FormAddToGitIgnore(item.Name).ShowDialog();
                Initialize();
            }
        }

        void SelectedDiff_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ShowChanges(currentItem, currentItemStaged);
        }

        private void splitContainer3_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void showUntrackedFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showUntrackedFilesToolStripMenuItem.Checked = !showUntrackedFilesToolStripMenuItem.Checked;
            Scan_Click(null, null);
        }

        private void commitMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void rescanChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Scan_Click(null, null);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Unstaged.Rows.Count > LastRow && LastRow >= 0)
            {
                GitItemStatus item = (GitItemStatus)Unstaged.Rows[LastRow].DataBoundItem;
                string fileName = item.Name;

                System.Diagnostics.Process.Start(Settings.WorkingDir + fileName);
            }
        }

        private void openWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Unstaged.Rows.Count > LastRow && LastRow >= 0)
            {
                GitItemStatus item = (GitItemStatus)Unstaged.Rows[LastRow].DataBoundItem;
                string fileName = item.Name;

                OpenWith.OpenAs(Settings.WorkingDir + fileName);
            }
        }

        private void CloseDialogAfterCommit_CheckedChanged(object sender, EventArgs e)
        {
            Settings.CloseCommitDialogAfterCommit = CloseDialogAfterCommit.Checked;
        }

        private void filenameToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Unstaged.Rows.Count > LastRow && LastRow >= 0)
            {
                GitItemStatus item = (GitItemStatus)Unstaged.Rows[LastRow].DataBoundItem;
                string fileName = Settings.WorkingDir + item.Name;

                Clipboard.SetText(fileName.Replace('/', '\\'));
            }
        }

        private void openWithDifftoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Unstaged.Rows.Count > LastRow && LastRow >= 0)
            {
                GitItemStatus item = (GitItemStatus)Unstaged.Rows[LastRow].DataBoundItem;
                string fileName = item.Name;

                string cmdOutput = GitCommands.GitCommands.OpenWithDifftool(fileName);

                if (!string.IsNullOrEmpty(cmdOutput))
                    MessageBox.Show(cmdOutput);
            }
        }

    }
}
