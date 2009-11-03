using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI
{
    public partial class FormResolveConflicts : GitExtensionsForm
    {
        public FormResolveConflicts()
        {
            InitializeComponent();
            ThereWhereMergeConflicts = GitCommands.GitCommands.InTheMiddleOfConflictedMerge();
        }

        void ConflictedFiles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex==SolveConflictButton.Index)
                ConflictedFilesContextMenu.Show(Cursor.Position);
        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Directory.SetCurrentDirectory(GitCommands.Settings.WorkingDir);
            GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.cmd", "mergetool");
            Initialize();
        }

        public bool ThereWhereMergeConflicts { get; set; }

        private void FormResolveConflicts_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            Cursor.Current = Cursors.WaitCursor;
            button1.Focus();

            ConflictedFiles.DataSource = GitCommands.GitCommands.GetConflictedFiles();
            InitMergetool();

            ConflictedFiles.CellClick += new DataGridViewCellEventHandler(ConflictedFiles_CellClick);
            ConflictedFilesContextMenu.Text = "Solve";
            OpenMergetool.Text = "Open in " + mergetool;
            button1.Text = "Open in " + mergetool;

            if (GitCommands.GitCommands.InTheMiddleOfRebase())
            {
                Reset.Text = "Abort rebase";
                ContextChooseLocal.Text = "Choose local (theirs)";
                ContextChooseRemote.Text = "Choose remote (ours)";
            }
            else
            {
                Reset.Text = "Abort merge";
                ContextChooseLocal.Text = "Choose local (ours)";
                ContextChooseRemote.Text = "Choose remote (theirs)";
            }

            if (!GitCommands.GitCommands.InTheMiddleOfPatch() && !GitCommands.GitCommands.InTheMiddleOfRebase() && !GitCommands.GitCommands.InTheMiddleOfConflictedMerge() && ThereWhereMergeConflicts)
            {
                if (MessageBox.Show("All mergeconflicts are resolved, you can commit." + Environment.NewLine + "Do you want to commit now?", "Commit", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitUICommands.Instance.StartCommitDialog();
                }

            }

            if (!GitCommands.GitCommands.InTheMiddleOfConflictedMerge() && ThereWhereMergeConflicts)
            {
                Close();
            }
        }

        private void Rescan_Click(object sender, EventArgs e)
        {
            Initialize();
        }

        private string mergetool;
        private string mergetoolCmd;
        private string mergetoolPath;

        private string GetFileName()
        {
            if (ConflictedFiles.SelectedRows.Count != 1)
                return null;

            DataGridViewRow row = ConflictedFiles.SelectedRows[0];
            return ((GitItem)row.DataBoundItem).FileName;
        }

        private void ConflictedFiles_DoubleClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConflictedFiles.SelectedRows.Count != 1)
                return;

            string filename = GitCommands.GitCommands.GetConflictedFiles(GetFileName());


            if (Directory.Exists(Settings.WorkingDir + filename) && !File.Exists(Settings.WorkingDir + filename))
            {
                IList<IGitSubmodule> submodules = (new GitCommands.GitCommands()).GetSubmodules();
                foreach (IGitSubmodule submodule in submodules)
                {
                    if (submodule.LocalPath.Equals(filename))
                    {
                        if (MessageBox.Show("The selected mergeconflict is a submodule. Mark conflict as resolved?", "Merge", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            GitCommands.GitCommands.RunCmd(Settings.GitDir + "git.cmd", "add -- \"" + filename + "\"");
                            Initialize();
                        }
                        return;
                    }
                }
            }

            bool file1 = File.Exists(GitCommands.Settings.WorkingDir + filename + ".BASE");
            bool file2 = File.Exists(GitCommands.Settings.WorkingDir + filename + ".LOCAL");
            bool file3 = File.Exists(GitCommands.Settings.WorkingDir + filename + ".REMOTE");

            string arguments = mergetoolCmd;

            if (file1 && file2 && file3)
            {
                if (FileHelper.IsBinaryFile(filename))
                {
                    if (MessageBox.Show("The selected file appears to be a binary file." + Environment.NewLine + "Are you sure you want to open this file in " + mergetool + "?") == DialogResult.No)
                        return;
                }

                arguments = arguments.Replace("$BASE", filename + ".BASE");
                arguments = arguments.Replace("$LOCAL", filename + ".LOCAL");
                arguments = arguments.Replace("$REMOTE", filename + ".REMOTE");
                arguments = arguments.Replace("$MERGED", filename + "");

                GitCommands.GitCommands.RunCmd(mergetoolPath, "" + arguments + "");

                if (MessageBox.Show("Is the mergeconflict solved?", "Merge", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitCommands.GitCommands.RunCmd(Settings.GitDir + "git.cmd", "add -- \"" + filename + "\"");
                    Initialize();

                }
            }
            else
            {
                FormModifiedDeletedCreated frm = new FormModifiedDeletedCreated();
                if ((file1 && file2 && !file3) || (file1 && !file2 && file3))
                {
                    frm.Label.Text = "Use modified or deleted file?";
                    frm.Created.Text = "Modified";
                }
                else
                    if (!file1)
                    {
                        frm.Label.Text = "Use created or delete file?";
                    }
                    else
                    {
                        File.Delete(Settings.WorkingDir + filename + ".BASE");
                        File.Delete(Settings.WorkingDir + filename + ".LOCAL");
                        File.Delete(Settings.WorkingDir + filename + ".REMOTE");

                        Directory.SetCurrentDirectory(GitCommands.Settings.WorkingDir);
                        GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.cmd", "mergetool \"" + filename + "\"");
                        Initialize();
                        return;
                    }

                frm.ShowDialog();

                if (frm.Aborted)
                {
                    File.Delete(Settings.WorkingDir + filename + ".BASE");
                    File.Delete(Settings.WorkingDir + filename + ".LOCAL");
                    File.Delete(Settings.WorkingDir + filename + ".REMOTE");
                    return;
                }
                else
                    if (frm.Delete)
                        GitCommands.GitCommands.RunCmd(Settings.GitDir + "git.cmd", "rm -- \"" + filename + "\"");
                    else
                        if (!frm.Delete)
                            GitCommands.GitCommands.RunCmd(Settings.GitDir + "git.cmd", "add -- \"" + filename + "\"");

                Initialize();
            }

            File.Delete(Settings.WorkingDir + filename + ".BASE");
            File.Delete(Settings.WorkingDir + filename + ".LOCAL");
            File.Delete(Settings.WorkingDir + filename + ".REMOTE");
        }

        private void InitMergetool()
        {
            Cursor.Current = Cursors.WaitCursor;
            GitCommands.GitCommands gitCommands = new GitCommands.GitCommands();

            mergetool = GitCommands.GitCommands.GetSetting("merge.tool");
            if (string.IsNullOrEmpty(mergetool))
                mergetool = gitCommands.GetGlobalSetting("merge.tool");

            if (string.IsNullOrEmpty(mergetool))
            {
                MessageBox.Show("There is no mergetool configured. Please go to settings and set a mergetool!");
                return;
            }

            mergetoolCmd = GitCommands.GitCommands.GetSetting("mergetool." + mergetool + ".cmd");
            if (string.IsNullOrEmpty(mergetoolCmd))
                mergetoolCmd = gitCommands.GetGlobalSetting("mergetool." + mergetool + ".cmd");

            mergetoolPath = GitCommands.GitCommands.GetSetting("mergetool." + mergetool + ".path");
            if (string.IsNullOrEmpty(mergetoolPath))
                mergetoolPath = gitCommands.GetGlobalSetting("mergetool." + mergetool + ".path");

            if (string.IsNullOrEmpty(mergetool) || mergetool == "kdiff3")
                mergetoolCmd = mergetoolPath + " \"$BASE\" \"$LOCAL\" \"$REMOTE\" -o \"$MERGED\"";

            mergetoolPath = mergetoolCmd.Substring(0, mergetoolCmd.IndexOf(".exe") + 5).Trim(new char[]{'\"', ' '});
            mergetoolCmd = mergetoolCmd.Substring(mergetoolCmd.IndexOf(".exe") + 5);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            ConflictedFiles_DoubleClick(sender, e);
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (Abort.AbortCurrentAction())
                Close();
        }

        private void ConflictedFiles_SelectionChanged(object sender, EventArgs e)
        {
            
        }

        private void ContextChooseBase_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            GitCommands.GitCommands.HandleConflice_SelectBase(GetFileName());
            Initialize();
        }

        private void ContextChooseLocal_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            GitCommands.GitCommands.HandleConflice_SelectLocal(GetFileName());
            Initialize();
        }

        private void ContextChooseRemote_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            GitCommands.GitCommands.HandleConflice_SelectRemote(GetFileName());
            Initialize();
        }

        private void OpenMergetool_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            ConflictedFiles_DoubleClick(sender, e);
        }

        private void ContextOpenBaseWith_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string fileName = GetFileName();
            fileName = GetShortFileName(fileName);

            fileName = Path.GetTempPath() + fileName;

            GitCommands.GitCommands.HandeConflicts_SaveSide(GetFileName(), fileName, "BASE");

            OpenWith.OpenAs(fileName);
        }

        private void ContextOpenLocalWith_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string fileName = GetFileName();
            fileName = GetShortFileName(fileName);

            fileName = Path.GetTempPath() + fileName;

            GitCommands.GitCommands.HandeConflicts_SaveSide(GetFileName(), fileName, "LOCAL");

            OpenWith.OpenAs(fileName);
        }

        private static string GetShortFileName(string fileName)
        {
            if (fileName.Contains("\\") && fileName.LastIndexOf("\\") < fileName.Length)
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
            if (fileName.Contains("/") && fileName.LastIndexOf("/") < fileName.Length)
                fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);
            return fileName;
        }

        private void ContextOpenRemoteWith_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string fileName = GetFileName();
            fileName = GetShortFileName(fileName);

            fileName = Path.GetTempPath() + fileName;

            GitCommands.GitCommands.HandeConflicts_SaveSide(GetFileName(), fileName, "REMOTE");

            OpenWith.OpenAs(fileName);
        }

        private void ConflictedFiles_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                System.Drawing.Point pt = ConflictedFiles.PointToClient(Cursor.Position);
                DataGridView.HitTestInfo hti = ConflictedFiles.HitTest(pt.X, pt.Y);
                int LastRow = hti.RowIndex;
                ConflictedFiles.ClearSelection();
                if (LastRow >= 0 && ConflictedFiles.Rows.Count > LastRow)
                    ConflictedFiles.Rows[LastRow].Selected = true;
            }
        }

        private void SaveAs(string side)
        {
            Cursor.Current = Cursors.WaitCursor;
            string fileName = GetFileName();
            fileName = GetShortFileName(fileName);

            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.FileName = Settings.WorkingDir + fileName;
            fileDialog.AddExtension = true;
            fileDialog.DefaultExt = GitCommands.GitCommands.GetFileExtension(fileDialog.FileName);
            fileDialog.Filter = "Current format (*." + GitCommands.GitCommands.GetFileExtension(fileDialog.FileName) + ")|*." + GitCommands.GitCommands.GetFileExtension(fileDialog.FileName) + "|All files (*.*)|*.*";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                GitCommands.GitCommands.HandeConflicts_SaveSide(GetFileName(), fileDialog.FileName, side);
            }
        }

        private void ContextSaveBaseAs_Click(object sender, EventArgs e)
        {
            SaveAs("BASE");
        }

        private void ContextSaveLocalAs_Click(object sender, EventArgs e)
        {
            SaveAs("LOCAL");
        }

        private void ContextSaveRemoteAs_Click(object sender, EventArgs e)
        {
            SaveAs("REMOTE");
        }

        private void ContextMarkAsSolved_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            GitCommands.GitCommands.RunCmd(Settings.GitDir + "git.cmd", "add -- \"" + GetFileName() + "\"");
            Initialize();
        }
    }
}
