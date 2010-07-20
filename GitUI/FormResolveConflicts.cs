using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using GitCommands;
using GitUIPluginInterfaces;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormResolveConflicts : GitExtensionsForm
    {
        TranslationString allConflictsResolved = new TranslationString("All mergeconflicts are resolved, you can commit." + Environment.NewLine + "Do you want to commit now?");
        TranslationString allConflictsResolvedCaption = new TranslationString("Commit");
        TranslationString mergeConflictIsSubmodule = new TranslationString("The selected mergeconflict is a submodule. Mark conflict as resolved?");
        TranslationString mergeConflictIsSubmoduleCaption = new TranslationString("Submodule");
        TranslationString fileIsBinary = new TranslationString("The selected file appears to be a binary file." + Environment.NewLine + "Are you sure you want to open this file in {0}?");
        TranslationString askMergeConflictSolved = new TranslationString("Is the mergeconflict solved?");
        TranslationString askMergeConflictSolvedCaption = new TranslationString("Conflict solved?");
        TranslationString useModifiedOrDeletedFile = new TranslationString("Use modified or deleted file?");
        TranslationString modifiedButton = new TranslationString("Modified");
        TranslationString useCreatedOrDeletedFile = new TranslationString("Use created or deleted file?");
        TranslationString noMergeTool = new TranslationString("There is no mergetool configured. Please go to settings and set a mergetool!");
        TranslationString stageFilename = new TranslationString("Stage {0}");

        public FormResolveConflicts()
        {
            InitializeComponent(); Translate();
            ThereWhereMergeConflicts = GitCommands.GitCommands.InTheMiddleOfConflictedMerge();
            
        }


        private void Mergetool_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Directory.SetCurrentDirectory(GitCommands.Settings.WorkingDir);
            GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitCommand, "mergetool");
            Initialize();
        }

        public bool ThereWhereMergeConflicts { get; set; }

        private void FormResolveConflicts_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("resolve-conflicts");
        }

        private void FormResolveConflicts_Load(object sender, EventArgs e)
        {
            RestorePosition("resolve-conflicts");
            Initialize();
        }

        private void Initialize()
        {
            Cursor.Current = Cursors.WaitCursor;
            button1.Focus();

            ConflictedFiles.DataSource = GitCommands.GitCommands.GetConflictedFiles();
            InitMergetool();

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
                if (MessageBox.Show(allConflictsResolved.Text, allConflictsResolvedCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
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

            string filename = GetFileName();
            string[] filenames = GitCommands.GitCommands.GetConflictedFiles(filename);

            if (Directory.Exists(Settings.WorkingDir + filename) && !File.Exists(Settings.WorkingDir + filename))
            {
                IList<IGitSubmodule> submodules = (new GitCommands.GitCommands()).GetSubmodules();
                foreach (IGitSubmodule submodule in submodules)
                {
                    if (submodule.LocalPath.Equals(filename))
                    {
                        if (MessageBox.Show(mergeConflictIsSubmodule.Text, mergeConflictIsSubmoduleCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            stageFile(filename);
                            Initialize();
                        }
                        return;
                    }
                }
            }

            bool file1 = File.Exists(filenames[0]);
            bool file2 = File.Exists(filenames[1]);
            bool file3 = File.Exists(filenames[2]);

            string arguments = mergetoolCmd;

            if (file1 && file2 && file3)
            {
                if (FileHelper.IsBinaryFile(filename))
                {
                    if (MessageBox.Show(string.Format(fileIsBinary.Text, mergetool)) == DialogResult.No)
                        return;
                }

                arguments = arguments.Replace("$BASE", filenames[0]);
                arguments = arguments.Replace("$LOCAL", filenames[1]);
                arguments = arguments.Replace("$REMOTE", filenames[2]);
                arguments = arguments.Replace("$MERGED", filename + "");

                GitCommands.GitCommands.RunCmd(mergetoolPath, "" + arguments + "");

                if (MessageBox.Show(askMergeConflictSolved.Text, askMergeConflictSolvedCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    stageFile(filename);
                    Initialize();
                }
            }
            else
            {
                FormModifiedDeletedCreated frm = new FormModifiedDeletedCreated();
                if ((file1 && file2 && !file3) || (file1 && !file2 && file3))
                {
                    frm.Label.Text = useModifiedOrDeletedFile.Text;
                    frm.Created.Text = modifiedButton.Text;
                }
                else
                    if (!file1)
                    {
                        frm.Label.Text = useCreatedOrDeletedFile.Text;
                    }
                    else
                    {
                        File.Delete(filenames[0]);
                        File.Delete(filenames[1]);
                        File.Delete(filenames[2]);

                        Directory.SetCurrentDirectory(GitCommands.Settings.WorkingDir);
                        GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitCommand, "mergetool \"" + filename + "\"");
                        Initialize();
                        return;
                    }

                frm.ShowDialog();

                if (frm.Aborted)
                {
                    File.Delete(filenames[0]);
                    File.Delete(filenames[1]);
                    File.Delete(filenames[2]);
                    return;
                }
                else
                    if (frm.Delete)
                        GitCommands.GitCommands.RunCmd(Settings.GitCommand, "rm -- \"" + filename + "\"");
                    else
                        if (!frm.Delete)
                            stageFile(filename);

                Initialize();
            }

            File.Delete(filenames[0]);
            File.Delete(filenames[1]);
            File.Delete(filenames[2]);
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
                MessageBox.Show(noMergeTool.Text);
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
            GitCommands.GitCommands.HandleConflict_SelectBase(GetFileName());
            Initialize();
        }

        private void ContextChooseLocal_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            GitCommands.GitCommands.HandleConflict_SelectLocal(GetFileName());
            Initialize();
        }

        private void ContextChooseRemote_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            GitCommands.GitCommands.HandleConflict_SelectRemote(GetFileName());
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

            GitCommands.GitCommands.HandleConflicts_SaveSide(GetFileName(), fileName, "BASE");

            OpenWith.OpenAs(fileName);
        }

        private void ContextOpenLocalWith_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string fileName = GetFileName();
            fileName = GetShortFileName(fileName);

            fileName = Path.GetTempPath() + fileName;

            GitCommands.GitCommands.HandleConflicts_SaveSide(GetFileName(), fileName, "LOCAL");

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

            GitCommands.GitCommands.HandleConflicts_SaveSide(GetFileName(), fileName, "REMOTE");

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
                GitCommands.GitCommands.HandleConflicts_SaveSide(GetFileName(), fileDialog.FileName, side);
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
            stageFile(GetFileName());
            Initialize();
        }

        private void ConflictedFilesContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(GetFileName()))
                ConflictedFilesContextMenu.Enabled = false;
            else
                ConflictedFilesContextMenu.Enabled = true;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = GetFileName();
            System.Diagnostics.Process.Start(Settings.WorkingDir + fileName);
        }

        private void openWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = GetFileName();
            OpenWith.OpenAs(Settings.WorkingDir + fileName);
        }

        private void stageFile(string filename)
        {
            FormStatus.ProcessStart processStart = new FormStatus.ProcessStart
                (
                    delegate(FormStatus form)
                    {
                        form.AddOutput(string.Format(stageFilename.Text, filename));
                        string output = GitCommands.GitCommands.RunCmd
                            (
                            Settings.GitCommand, "add -- \"" + filename + "\""
                            );
                        form.AddOutput(output);
                        form.Done(string.IsNullOrEmpty(output));
                    }
                );
            FormStatus process = new FormStatus(processStart, null);
            process.Text = string.Format(stageFilename.Text, filename);
            process.ShowDialogOnError();
        }
    }
}
