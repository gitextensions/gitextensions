using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitUI.Editor;
using ResourceManager.Translation;
using System.Text;

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
            ThereWhereMergeConflicts = GitCommandHelpers.InTheMiddleOfConflictedMerge();

        }


        private void Mergetool_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Directory.SetCurrentDirectory(GitCommands.Settings.WorkingDir);
            GitCommandHelpers.RunRealCmd(GitCommands.Settings.GitCommand, "mergetool");
            Initialize();
            Cursor.Current = Cursors.Default;
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

            ConflictedFiles.DataSource = GitCommandHelpers.GetConflictedFiles();
            InitMergetool();

            ConflictedFilesContextMenu.Text = "Solve";
            OpenMergetool.Text = "Open in " + mergetool;
            button1.Text = "Open in " + mergetool;

            if (GitCommandHelpers.InTheMiddleOfRebase())
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

            if (!GitCommandHelpers.InTheMiddleOfPatch() && !GitCommandHelpers.InTheMiddleOfRebase() && !GitCommandHelpers.InTheMiddleOfConflictedMerge() && ThereWhereMergeConflicts)
            {
                if (MessageBox.Show(allConflictsResolved.Text, allConflictsResolvedCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitUICommands.Instance.StartCommitDialog();
                }

            }

            if (!GitCommandHelpers.InTheMiddleOfConflictedMerge() && ThereWhereMergeConflicts)
            {
                Close();
            }
            Cursor.Current = Cursors.Default;
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
            string[] filenames = GitCommandHelpers.GetConflictedFiles(filename);

            if (Directory.Exists(Settings.WorkingDir + filename) && !File.Exists(Settings.WorkingDir + filename))
            {
                /* BEGIN REPLACED WITH FASTER, BUT DIRTIER SUBMODULE CHECK
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
                }*/
                ConfigFile submoduleConfig = new ConfigFile(Settings.WorkingDir + ".gitmodules");
                foreach (ConfigSection configSection in submoduleConfig.GetConfigSections())
                {
                    if (configSection.GetValue("path").Trim().Equals(filename.Trim()))
                    {
                        if (MessageBox.Show(mergeConflictIsSubmodule.Text, mergeConflictIsSubmoduleCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            stageFile(filename);
                            Initialize();
                        }
                        return;
                    }
                }
                //END: REPLACED WITH FASTER, BUT DIRTIER SUBMODULE CHECK
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

                GitCommandHelpers.RunCmd(mergetoolPath, "" + arguments + "");

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
                        if (File.Exists(filenames[0]))
                            File.Delete(filenames[0]);
                        if (File.Exists(filenames[1]))
                            File.Delete(filenames[1]);
                        if (File.Exists(filenames[2]))
                            File.Delete(filenames[2]);

                        Directory.SetCurrentDirectory(GitCommands.Settings.WorkingDir);
                        GitCommandHelpers.RunRealCmd(GitCommands.Settings.GitCommand, "mergetool \"" + filename + "\"");
                        Initialize();
                        return;
                    }

                frm.ShowDialog();

                if (frm.Aborted)
                {
                    if (File.Exists(filenames[0]))
                        File.Delete(filenames[0]);
                    if (File.Exists(filenames[1]))
                        File.Delete(filenames[1]);
                    if (File.Exists(filenames[2]))
                        File.Delete(filenames[2]);
                    return;
                }
                else
                    if (frm.Delete)
                        GitCommandHelpers.RunCmd(Settings.GitCommand, "rm -- \"" + filename + "\"");
                    else
                        if (!frm.Delete)
                            stageFile(filename);

                Initialize();
            }

            if (File.Exists(filenames[0]))
                File.Delete(filenames[0]);
            if (File.Exists(filenames[1]))
                File.Delete(filenames[1]);
            if (File.Exists(filenames[2]))
                File.Delete(filenames[2]);
            Cursor.Current = Cursors.Default;
        }

        private void InitMergetool()
        {
            mergetool = GitCommandHelpers.GetSetting("merge.tool");
            if (string.IsNullOrEmpty(mergetool))
                mergetool = GitCommandHelpers.GetGlobalSetting("merge.tool");

            if (string.IsNullOrEmpty(mergetool))
            {
                MessageBox.Show(noMergeTool.Text);
                return;
            }
            Cursor.Current = Cursors.WaitCursor;

            mergetoolCmd = GitCommandHelpers.GetSetting("mergetool." + mergetool + ".cmd");
            if (string.IsNullOrEmpty(mergetoolCmd))
                mergetoolCmd = GitCommandHelpers.GetGlobalSetting("mergetool." + mergetool + ".cmd");

            mergetoolPath = GitCommandHelpers.GetSetting("mergetool." + mergetool + ".path");
            if (string.IsNullOrEmpty(mergetoolPath))
                mergetoolPath = GitCommandHelpers.GetGlobalSetting("mergetool." + mergetool + ".path");

            if (string.IsNullOrEmpty(mergetool) || mergetool == "kdiff3")
                mergetoolCmd = mergetoolPath + " \"$BASE\" \"$LOCAL\" \"$REMOTE\" -o \"$MERGED\"";

            mergetoolPath = mergetoolCmd.Substring(0, mergetoolCmd.IndexOf(".exe") + 5).Trim(new char[] { '\"', ' ' });
            mergetoolCmd = mergetoolCmd.Substring(mergetoolCmd.IndexOf(".exe") + 5);
            Cursor.Current = Cursors.Default;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            ConflictedFiles_DoubleClick(sender, e);
            Cursor.Current = Cursors.Default;
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (Abort.AbortCurrentAction())
                Close();
            Cursor.Current = Cursors.Default;
        }

        private void ConflictedFiles_SelectionChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConflictedFiles.SelectedRows.Count != 1)
            {
                baseFileName.Text = localFileName.Text = remoteFileName.Text = "";
                return;
            }

            string filename = GetFileName();
            string[] filenames = GitCommandHelpers.GetConflictedFileNames(filename);

            bool baseFileExists = !string.IsNullOrEmpty(filenames[0]);
            bool localFileExists = !string.IsNullOrEmpty(filenames[1]);
            bool remoteFileExists = !string.IsNullOrEmpty(filenames[2]);

            bool inTheMiddleOfRebase = GitCommandHelpers.InTheMiddleOfRebase();
            string remoteSide = inTheMiddleOfRebase ? "ours" : "theirs";
            string localSide = inTheMiddleOfRebase ? "theirs" : "ours";

            if (baseFileExists && localFileExists && remoteFileExists)
                conflictDescription.Text = string.Format("The file has been changed locally({0}) and remotely({1}). Merge the changes.", localSide, remoteSide);
            if (!baseFileExists && localFileExists && remoteFileExists)
                conflictDescription.Text = string.Format("A file with the same name has been created locally({0}) and remotely({1}). Choose the file you want to keep or merge the files.", localSide, remoteSide);
            if (baseFileExists && !localFileExists && remoteFileExists)
                conflictDescription.Text = string.Format("File deleted locally({0}) but modified remotely({1}). Choose to delete the file or keep the modified version.", localSide, remoteSide);
            if (baseFileExists && localFileExists && !remoteFileExists)
                conflictDescription.Text = string.Format("File modified locally({0}) but deleted remotely({1}). Choose to delete the file or keep the modified version.", localSide, remoteSide);

            baseFileName.Text = (baseFileExists ? filenames[0] : "no file");
            localFileName.Text = (localFileExists ? filenames[1] : "no file");
            remoteFileName.Text = (remoteFileExists ? filenames[2] : "no file");


            chooseBase.Enabled = !string.IsNullOrEmpty(filenames[0]);
            chooseLocal.Enabled = !string.IsNullOrEmpty(filenames[1]);
            chooseRemote.Enabled = !string.IsNullOrEmpty(filenames[2]);
        }

        private void ContextChooseBase_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (!GitCommandHelpers.HandleConflictSelectBase(GetFileName()))
                MessageBox.Show("Choose base file failed.");
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void ContextChooseLocal_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (!GitCommandHelpers.HandleConflictSelectLocal(GetFileName()))
                MessageBox.Show("Choose local file failed.");
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void ContextChooseRemote_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (!GitCommandHelpers.HandleConflictSelectRemote(GetFileName()))
                MessageBox.Show("Choose remote file failed.");
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void OpenMergetool_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            ConflictedFiles_DoubleClick(sender, e);
            Cursor.Current = Cursors.Default;
        }

        private void ContextOpenBaseWith_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string fileName = GetFileName();
            fileName = GetShortFileName(fileName);

            fileName = Path.GetTempPath() + fileName;

            GitCommandHelpers.HandleConflictsSaveSide(GetFileName(), fileName, "BASE");

            OpenWith.OpenAs(fileName);
            Cursor.Current = Cursors.Default;
        }

        private void ContextOpenLocalWith_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string fileName = GetFileName();
            fileName = GetShortFileName(fileName);

            fileName = Path.GetTempPath() + fileName;

            GitCommandHelpers.HandleConflictsSaveSide(GetFileName(), fileName, "LOCAL");

            OpenWith.OpenAs(fileName);
            Cursor.Current = Cursors.Default;
        }

        private static string GetShortFileName(string fileName)
        {
            if (fileName.Contains(Settings.PathSeparator.ToString()) && fileName.LastIndexOf(Settings.PathSeparator.ToString()) < fileName.Length)
                fileName = fileName.Substring(fileName.LastIndexOf(Settings.PathSeparator) + 1);
            if (fileName.Contains(Settings.PathSeparatorWrong.ToString()) && fileName.LastIndexOf(Settings.PathSeparatorWrong.ToString()) < fileName.Length)
                fileName = fileName.Substring(fileName.LastIndexOf(Settings.PathSeparatorWrong) + 1);
            return fileName;
        }

        private void ContextOpenRemoteWith_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string fileName = GetFileName();
            fileName = GetShortFileName(fileName);

            fileName = Path.GetTempPath() + fileName;

            GitCommandHelpers.HandleConflictsSaveSide(GetFileName(), fileName, "REMOTE");

            OpenWith.OpenAs(fileName);
            Cursor.Current = Cursors.Default;
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
            fileDialog.DefaultExt = GitCommandHelpers.GetFileExtension(fileDialog.FileName);
            fileDialog.Filter = "Current format (*." + GitCommandHelpers.GetFileExtension(fileDialog.FileName) + ")|*." + GitCommandHelpers.GetFileExtension(fileDialog.FileName) + "|All files (*.*)|*.*";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                GitCommandHelpers.HandleConflictsSaveSide(GetFileName(), fileDialog.FileName, side);
            }
            Cursor.Current = Cursors.Default;
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
            Cursor.Current = Cursors.Default;
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
                        string output = GitCommandHelpers.RunCmd
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

        private void chooseLocal_Click(object sender, EventArgs e)
        {
            ContextChooseLocal_Click(sender, e);
        }

        private void chooseBase_Click(object sender, EventArgs e)
        {
            ContextChooseBase_Click(sender, e);
        }

        private void chooseRemote_Click(object sender, EventArgs e)
        {
            ContextChooseRemote_Click(sender, e);
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }
    }
}
