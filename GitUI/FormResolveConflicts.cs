﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitUI.Editor;
using ResourceManager.Translation;
using GitUI.Hotkey;

namespace GitUI
{
    public partial class FormResolveConflicts : GitExtensionsForm
    {
        readonly TranslationString uskUseCustomMergeScript = new TranslationString("There is a custom merge script({0}) for this file type." + Environment.NewLine + Environment.NewLine + "Do you want to use this custom merge script?");
        readonly TranslationString uskUseCustomMergeScriptCaption = new TranslationString("Custom merge script");
        readonly TranslationString fileUnchangedAfterMerge = new TranslationString("The file has not been modified by the merge. Usually this means that the file has been saved to the wrong location." + Environment.NewLine + Environment.NewLine + "The merge conflict will not be marked as solved. Please try again.");
        readonly TranslationString allConflictsResolved = new TranslationString("All mergeconflicts are resolved, you can commit." + Environment.NewLine + "Do you want to commit now?");
        readonly TranslationString allConflictsResolvedCaption = new TranslationString("Commit");
        readonly TranslationString mergeConflictIsSubmodule = new TranslationString("The selected mergeconflict is a submodule. Mark conflict as resolved?");
        readonly TranslationString mergeConflictIsSubmoduleCaption = new TranslationString("Submodule");
        readonly TranslationString fileIsBinary = new TranslationString("The selected file appears to be a binary file." + Environment.NewLine + "Are you sure you want to open this file in {0}?");
        readonly TranslationString askMergeConflictSolvedAfterCustomMergeScript = new TranslationString("The merge conflict need to be solved and the result must be saved as:" + Environment.NewLine + "{0}" + Environment.NewLine + Environment.NewLine + "Is the mergeconflict solved?");
        readonly TranslationString askMergeConflictSolved = new TranslationString("Is the mergeconflict solved?");
        readonly TranslationString askMergeConflictSolvedCaption = new TranslationString("Conflict solved?");
        readonly TranslationString noMergeTool = new TranslationString("There is no mergetool configured. Please go to settings and set a mergetool!");
        readonly TranslationString stageFilename = new TranslationString("Stage {0}");

        readonly TranslationString noBaseRevision = new TranslationString("There is no base revision for {0}.\nFall back to 2-way merge?");
        readonly TranslationString ours = new TranslationString("ours");
        readonly TranslationString theirs = new TranslationString("theirs");
        readonly TranslationString fileBinairyChooseLocalBaseRemote = new TranslationString("File ({0}) appears to be a binairy file.\nChoose to keep the local({1}), remote({2}) or base file.");
        readonly TranslationString fileChangeLocallyAndRemotely = new TranslationString("The file has been changed both locally({0}) and remotely({1}). Merge the changes.");
        readonly TranslationString fileCreatedLocallyAndRemotely = new TranslationString("A file with the same name has been created locally({0}) and remotely({1}). Choose the file you want to keep or merge the files.");
        readonly TranslationString fileCreatedLocallyAndRemotelyLong = new TranslationString("File {0} does not have a base revision.\nA file with the same name has been created locally({1}) and remotely({2}) causing this conflict.\n\nChoose the file you want to keep, merge the files or delete the file?");
        readonly TranslationString fileDeletedLocallyAndModifiedRemotely = new TranslationString("The file has been deleted locally({0}) and modified remotely({1}). Choose to delete the file or keep the modified version.");
        readonly TranslationString fileDeletedLocallyAndModifiedRemotelyLong = new TranslationString("File {0} does not have a local revision.\nThe file has been deleted locally({1}) but modified remotely({2}).\n\nChoose to delete the file or keep the modified version.");
        readonly TranslationString fileModifiedLocallyAndDelededRemotely = new TranslationString("The file has been modified locally({0}) and deleted remotely({1}). Choose to delete the file or keep the modified version.");
        readonly TranslationString fileModifiedLocallyAndDelededRemotelyLong = new TranslationString("File {0} does not have a remote revision.\nThe file has been modified locally({1}) but deleted remotely({2}).\n\nChoose to delete the file or keep the modified version.");
        readonly TranslationString noBase = new TranslationString("no base");
        readonly TranslationString deleted = new TranslationString("deleted");
        readonly TranslationString chooseLocalButtonText = new TranslationString("Choose local");
        readonly TranslationString chooseRemoteButtonText = new TranslationString("Choose remote");
        readonly TranslationString deleteFileButtonText = new TranslationString("Delete file");
        readonly TranslationString keepModifiedButtonText = new TranslationString("Keep modified");
        readonly TranslationString keepBaseButtonText = new TranslationString("Keep base file");

        private readonly TranslationString _conflictedFilesContextMenuText = new TranslationString("Solve");
        private readonly TranslationString _openMergeToolItemText = new TranslationString("Open in");
        private readonly TranslationString _button1Text = new TranslationString("Open in");

        private readonly TranslationString _resetItemRebaseText = new TranslationString("Abort rebase");
        private readonly TranslationString _contextChooseLocalRebaseText = new TranslationString("Choose local (theirs)");
        private readonly TranslationString _contextChooseRemoteRebaseText = new TranslationString("Choose remote (ours)");

        private readonly TranslationString _resetItemMergeText = new TranslationString("Abort merge");
        private readonly TranslationString _contextChooseLocalMergeText = new TranslationString("Choose local (theirs)");
        private readonly TranslationString _contextChooseRemoteMergeText = new TranslationString("Choose remote (ours)");

        private readonly TranslationString _binaryFileWarningCaption = new TranslationString("Warning");

        private readonly TranslationString _noBaseFileMergeCaption = new TranslationString("Merge");
        
        private readonly TranslationString _chooseBaseFileFailedText = new TranslationString("Choose base file failed.");
        private readonly TranslationString _chooseLocalFileFailedText = new TranslationString("Choose local file failed.");
        private readonly TranslationString _chooseRemoteFileFailedText = new TranslationString("Choose remote file failed.");

        private readonly TranslationString _currentFormatFilter = 
            new TranslationString("Current format (*.{0})");
        private readonly TranslationString _allFilesFilter = 
            new TranslationString("All files (*.*)");


        public FormResolveConflicts()
        {
            InitializeComponent(); Translate();
            ThereWhereMergeConflicts = GitCommandHelpers.InTheMiddleOfConflictedMerge();
            merge.Focus();
            merge.Select();

            this.HotkeysEnabled = true;
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
        }


        private void Mergetool_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Directory.SetCurrentDirectory(Settings.WorkingDir);
            GitCommandHelpers.RunRealCmd(Settings.GitCommand, "mergetool");
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

            ConflictedFiles.MultiSelect = false;
            int oldSelectedRow = 0;
            if (ConflictedFiles.SelectedRows.Count > 0)
                oldSelectedRow = ConflictedFiles.SelectedRows[0].Index;
            ConflictedFiles.DataSource = GitCommandHelpers.GetConflictedFiles();
            if (ConflictedFiles.Rows.Count > oldSelectedRow)
            {
                ConflictedFiles.Rows[oldSelectedRow].Selected = true;

                if (oldSelectedRow < ConflictedFiles.FirstDisplayedScrollingRowIndex ||
                    oldSelectedRow > (ConflictedFiles.FirstDisplayedScrollingRowIndex + ConflictedFiles.DisplayedRowCount(false)))
                    ConflictedFiles.FirstDisplayedScrollingRowIndex = oldSelectedRow;
            }

            InitMergetool();

            ConflictedFilesContextMenu.Text = _conflictedFilesContextMenuText.Text;
            OpenMergetool.Text = _openMergeToolItemText.Text + " " + mergetool;
            button1.Text = _button1Text.Text + " " + mergetool;

            if (GitCommandHelpers.InTheMiddleOfRebase())
            {
                Reset.Text = _resetItemRebaseText.Text;
                ContextChooseLocal.Text = _contextChooseLocalRebaseText.Text;
                ContextChooseRemote.Text = _contextChooseRemoteRebaseText.Text;
            }
            else
            {
                Reset.Text = _resetItemMergeText.Text;
                ContextChooseLocal.Text = _contextChooseLocalMergeText.Text;
                ContextChooseRemote.Text = _contextChooseRemoteMergeText.Text;
            }

            if (!GitCommandHelpers.InTheMiddleOfPatch() && !GitCommandHelpers.InTheMiddleOfRebase() && !GitCommandHelpers.InTheMiddleOfConflictedMerge() && ThereWhereMergeConflicts)
            {
                if (MessageBox.Show(allConflictsResolved.Text, allConflictsResolvedCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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

        private bool TryMergeWithScript(string fileName, string baseFileName, string remoteFileName, string localFileName)
        {
            if (!Settings.RunningOnWindows())
                return false;

            try
            {
                int extensionsSeperator = fileName.LastIndexOf('.');
                if (!(extensionsSeperator > 0) || extensionsSeperator + 1 >= fileName.Length)
                    return false;

                string[] mergeScripts = Directory.GetFiles(Settings.GetInstallDir() + Settings.PathSeparator + "Diff-Scripts" + Settings.PathSeparator, "merge-" + fileName.Substring(extensionsSeperator + 1) + ".*");

                if (mergeScripts.Length > 0)
                {
                    if (MessageBox.Show(string.Format(uskUseCustomMergeScript.Text, mergeScripts[0].Replace(Settings.PathSeparator + Settings.PathSeparator.ToString(), Settings.PathSeparator.ToString())), uskUseCustomMergeScriptCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        //get timestamp of file before merge. This is an extra check to verify if merge was successfull
                        DateTime lastWriteTimeBeforeMerge = DateTime.Now;
                        if (File.Exists(Settings.WorkingDir + fileName))
                            lastWriteTimeBeforeMerge = File.GetLastWriteTime(Settings.WorkingDir + fileName);

                        int exitCode;
                        GitCommandHelpers.RunCmd("wscript", "\"" + mergeScripts[0] + "\" \"" + (Settings.WorkingDir + fileName).Replace(Settings.PathSeparatorWrong, Settings.PathSeparator) + "\" \"" + remoteFileName.Replace(Settings.PathSeparatorWrong, Settings.PathSeparator) + "\" \"" + localFileName.Replace(Settings.PathSeparatorWrong, Settings.PathSeparator) + "\" \"" + baseFileName.Replace(Settings.PathSeparatorWrong, Settings.PathSeparator) + "\"", out exitCode);

                        if (MessageBox.Show(string.Format(askMergeConflictSolvedAfterCustomMergeScript.Text, (Settings.WorkingDir + fileName).Replace(Settings.PathSeparatorWrong, Settings.PathSeparator)), askMergeConflictSolvedCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {

                            DateTime lastWriteTimeAfterMerge = lastWriteTimeBeforeMerge;
                            if (File.Exists(Settings.WorkingDir + fileName))
                                lastWriteTimeAfterMerge = File.GetLastWriteTime(Settings.WorkingDir + fileName);

                            //The file is not modified, do not stage file and present warning
                            if (lastWriteTimeBeforeMerge == lastWriteTimeAfterMerge)
                                MessageBox.Show(fileUnchangedAfterMerge.Text);
                            else
                                stageFile(fileName);
                        }

                        Initialize();
                        if (File.Exists(baseFileName)) File.Delete(baseFileName);
                        if (File.Exists(remoteFileName)) File.Delete(remoteFileName);
                        if (File.Exists(localFileName)) File.Delete(localFileName);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Merge using script failed.\n" + ex);
            }
            return false;
        }

        private void ConflictedFiles_DoubleClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConflictedFiles.SelectedRows.Count != 1)
                return;

            string filename = GetFileName();
            string[] filenames = GitCommandHelpers.GetConflictedFiles(filename);

            try
            {
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
                    var submoduleConfig = new ConfigFile(Settings.WorkingDir + ".gitmodules");
                    if (submoduleConfig.GetConfigSections().Any(configSection => configSection.GetValue("path").Trim().Equals(filename.Trim())))
                    {
                        if (MessageBox.Show(mergeConflictIsSubmodule.Text, mergeConflictIsSubmoduleCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        {
                            stageFile(filename);
                        }
                        return;
                    }
                    //END: REPLACED WITH FASTER, BUT DIRTIER SUBMODULE CHECK
                }

                string arguments = mergetoolCmd;

                if (CheckForLocalRevision(filename) &&
                    CheckForRemoteRevision(filename))
                {
                    if (TryMergeWithScript(filename, filenames[0], filenames[2], filenames[1]))
                    {
                        Cursor.Current = Cursors.Default;
                        return;
                    }

                    if (FileHelper.IsBinaryFile(filename))
                    {
                        if (MessageBox.Show(string.Format(fileIsBinary.Text, mergetool), _binaryFileWarningCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                        {
                            BinairyFilesChooseLocalBaseRemote(filename);
                            return;
                        }
                    }

                    //Check if there is a base file. If not, ask user to fall back to 2-way merge.
                    //git doesn't support 2-way merge, but we can try to adjust attributes to fix this.
                    //For kdiff3 this is easy; just remove the 3rd file from the arguments. Since the
                    //filenames are quoted, this takes a little extra effort. We need to remove these 
                    //quotes also. For tortoise and araxis a little bit more magic is needed.
                    if (filenames[0] == null)
                    {
                        DialogResult result = MessageBox.Show(string.Format(noBaseRevision.Text, filename), _noBaseFileMergeCaption.Text, MessageBoxButtons.YesNoCancel);
                        if (result == DialogResult.Yes)
                        {
                            arguments = arguments.Replace("-merge -3", "-merge");
                            arguments = arguments.Replace("/base:\"$BASE\"", "");
                            arguments = arguments.Replace("\"$BASE\"", "");
                        }

                        if (result == DialogResult.Cancel)
                        {
                            return;
                        }
                    }

                    arguments = arguments.Replace("$BASE", filenames[0]);
                    arguments = arguments.Replace("$LOCAL", filenames[1]);
                    arguments = arguments.Replace("$REMOTE", filenames[2]);
                    arguments = arguments.Replace("$MERGED", filename + "");

                    //get timestamp of file before merge. This is an extra check to verify if merge was successfull
                    DateTime lastWriteTimeBeforeMerge = DateTime.Now;
                    if (File.Exists(Settings.WorkingDir + filename))
                        lastWriteTimeBeforeMerge = File.GetLastWriteTime(Settings.WorkingDir + filename);

                    int exitCode;
                    GitCommandHelpers.RunCmd(mergetoolPath, "" + arguments + "", out exitCode);

                    DateTime lastWriteTimeAfterMerge = lastWriteTimeBeforeMerge;
                    if (File.Exists(Settings.WorkingDir + filename))
                        lastWriteTimeAfterMerge = File.GetLastWriteTime(Settings.WorkingDir + filename);

                    //Check exitcode AND timestamp of the file. If exitcode is success and
                    //time timestamp is changed, we are pretty sure the merge was done.
                    if (exitCode == 0 && lastWriteTimeBeforeMerge != lastWriteTimeAfterMerge)
                    {
                        stageFile(filename);
                    }

                    //If the exitcode is 1, but the file is changed, ask if the merge conflict is solved.
                    //If the exitcode is 0, but the file is not changed, ask if the merge conflict is solved.
                    if ((exitCode == 1 && lastWriteTimeBeforeMerge != lastWriteTimeAfterMerge) ||
                        (exitCode == 0 && lastWriteTimeBeforeMerge == lastWriteTimeAfterMerge))
                    {
                        if (MessageBox.Show(askMergeConflictSolved.Text, askMergeConflictSolvedCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            stageFile(filename);
                        }
                    }
                }
            }
            finally
            {
                if (filenames[0] != null && File.Exists(filenames[0])) File.Delete(filenames[0]);
                if (filenames[1] != null && File.Exists(filenames[1])) File.Delete(filenames[1]);
                if (filenames[2] != null && File.Exists(filenames[2])) File.Delete(filenames[2]);
                Cursor.Current = Cursors.Default;
                Initialize();
            }
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

            mergetoolPath = mergetoolCmd.Substring(0, mergetoolCmd.IndexOf(".exe") + 5).Trim(new[] { '\"', ' ' });
            mergetoolCmd = mergetoolCmd.Substring(mergetoolCmd.IndexOf(".exe") + 5);
            Cursor.Current = Cursors.Default;
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (Abort.AbortCurrentAction())
                Close();
            Cursor.Current = Cursors.Default;
        }

        private string GetRemoteSideString()
        {
            bool inTheMiddleOfRebase = GitCommandHelpers.InTheMiddleOfRebase();
            return inTheMiddleOfRebase ? ours.Text : theirs.Text;
        }
        private string GetLocalSideString()
        {
            bool inTheMiddleOfRebase = GitCommandHelpers.InTheMiddleOfRebase();
            return inTheMiddleOfRebase ? theirs.Text : ours.Text;
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

            string remoteSide = GetRemoteSideString();
            string localSide = GetLocalSideString();

            if (baseFileExists && localFileExists && remoteFileExists)
                conflictDescription.Text = string.Format(fileChangeLocallyAndRemotely.Text, localSide, remoteSide);
            if (!baseFileExists && localFileExists && remoteFileExists)
                conflictDescription.Text = string.Format(fileCreatedLocallyAndRemotely.Text, localSide, remoteSide);
            if (baseFileExists && !localFileExists && remoteFileExists)
                conflictDescription.Text = string.Format(fileDeletedLocallyAndModifiedRemotely.Text, localSide, remoteSide);
            if (baseFileExists && localFileExists && !remoteFileExists)
                conflictDescription.Text = string.Format(fileModifiedLocallyAndDelededRemotely.Text, localSide, remoteSide);

            baseFileName.Text = (baseFileExists ? filenames[0] : noBase.Text);
            localFileName.Text = (localFileExists ? filenames[1] : deleted.Text);
            remoteFileName.Text = (remoteFileExists ? filenames[2] : deleted.Text);
        }

        private void ContextChooseBase_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            string filename = GetFileName();

            if (CheckForBaseRevision(filename))
            {
                if (!GitCommandHelpers.HandleConflictSelectBase(filename))
                    MessageBox.Show(_chooseBaseFileFailedText.Text);
            }
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void ContextChooseLocal_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            string filename = GetFileName();
            if (CheckForLocalRevision(filename))
            {
                if (!GitCommandHelpers.HandleConflictSelectLocal(GetFileName()))
                    MessageBox.Show(_chooseLocalFileFailedText.Text);
            }
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void ContextChooseRemote_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string filename = GetFileName();
            if (CheckForRemoteRevision(filename))
            {
                if (!GitCommandHelpers.HandleConflictSelectRemote(GetFileName()))
                    MessageBox.Show(_chooseRemoteFileFailedText.Text);
            }
            Initialize();

            Cursor.Current = Cursors.Default;
        }

        private void BinairyFilesChooseLocalBaseRemote(string filename)
        {
            string caption = string.Format(fileBinairyChooseLocalBaseRemote.Text,
                                            filename,
                                            GetLocalSideString(),
                                            GetRemoteSideString());

            var frm = new FormModifiedDeletedCreated(string.Format(chooseLocalButtonText.Text + " ({0})", GetLocalSideString()),
                                                                            string.Format(chooseRemoteButtonText.Text + " ({0})", GetRemoteSideString()),
                                                                            keepBaseButtonText.Text,
                                                                            caption);
            frm.ShowDialog();
            if (frm.KeepBase) //base
                GitCommandHelpers.HandleConflictSelectBase(GetFileName());
            if (frm.KeepLocal) //local
                GitCommandHelpers.HandleConflictSelectLocal(GetFileName());
            if (frm.KeepRemote) //remote
                GitCommandHelpers.HandleConflictSelectRemote(GetFileName());
        }

        private bool CheckForBaseRevision(string filename)
        {
            if (string.IsNullOrEmpty(GitCommandHelpers.GetConflictedFileNames(filename)[0]))
            {
                string caption = string.Format(fileCreatedLocallyAndRemotelyLong.Text,
                                                filename,
                                                GetLocalSideString(),
                                                GetRemoteSideString());

                var frm = new FormModifiedDeletedCreated(string.Format(chooseLocalButtonText.Text + " ({0})", GetLocalSideString()),
                                                                                string.Format(chooseRemoteButtonText.Text + " ({0})", GetRemoteSideString()),
                                                                                deleteFileButtonText.Text,
                                                                                caption);
                frm.ShowDialog();
                if (frm.KeepBase) //delete
                    GitCommandHelpers.RunCmd(Settings.GitCommand, "rm -- \"" + filename + "\"");
                if (frm.KeepLocal) //local
                    GitCommandHelpers.HandleConflictSelectLocal(GetFileName());
                if (frm.KeepRemote) //remote
                    GitCommandHelpers.HandleConflictSelectRemote(GetFileName());
                return false;
            }
            return true;
        }

        private bool CheckForLocalRevision(string filename)
        {
            if (string.IsNullOrEmpty(GitCommandHelpers.GetConflictedFileNames(filename)[1]))
            {
                string caption = string.Format(fileDeletedLocallyAndModifiedRemotelyLong.Text,
                                                filename,
                                                GetLocalSideString(),
                                                GetRemoteSideString());

                var frm = new FormModifiedDeletedCreated(string.Format(deleteFileButtonText.Text + " ({0})", GetLocalSideString()),
                                                                                string.Format(keepModifiedButtonText.Text + " ({0})", GetRemoteSideString()),
                                                                                keepBaseButtonText.Text,
                                                                                caption);
                frm.ShowDialog();
                if (frm.KeepBase) //base
                    GitCommandHelpers.HandleConflictSelectBase(GetFileName());
                if (frm.KeepLocal) //delete
                    GitCommandHelpers.RunCmd(Settings.GitCommand, "rm -- \"" + filename + "\"");
                if (frm.KeepRemote) //remote
                    GitCommandHelpers.HandleConflictSelectRemote(GetFileName());
                return false;
            }
            return true;
        }

        private bool CheckForRemoteRevision(string filename)
        {
            if (string.IsNullOrEmpty(GitCommandHelpers.GetConflictedFileNames(filename)[2]))
            {
                string caption = string.Format(fileModifiedLocallyAndDelededRemotelyLong.Text,
                                                filename,
                                                GetLocalSideString(),
                                                GetRemoteSideString());

                var frm = new FormModifiedDeletedCreated(string.Format(keepModifiedButtonText.Text + " ({0})", GetLocalSideString()),
                                                                                string.Format(deleteFileButtonText.Text + " ({0})", GetRemoteSideString()),
                                                                                keepBaseButtonText.Text,
                                                                                caption);
                frm.ShowDialog();
                if (frm.KeepBase) //base
                    GitCommandHelpers.HandleConflictSelectBase(GetFileName());
                if (frm.KeepLocal) //delete
                    GitCommandHelpers.HandleConflictSelectLocal(GetFileName());
                if (frm.KeepRemote) //remote
                    GitCommandHelpers.RunCmd(Settings.GitCommand, "rm -- \"" + filename + "\"");
                return false;
            }
            return true;
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

        private static string GetDirectoryFromFileName(string fileName)
        {
            if (fileName.Contains(Settings.PathSeparator.ToString()) && fileName.LastIndexOf(Settings.PathSeparator.ToString()) < fileName.Length)
                fileName = fileName.Substring(0, fileName.LastIndexOf(Settings.PathSeparator));
            if (fileName.Contains(Settings.PathSeparatorWrong.ToString()) && fileName.LastIndexOf(Settings.PathSeparatorWrong.ToString()) < fileName.Length)
                fileName = fileName.Substring(0, fileName.LastIndexOf(Settings.PathSeparatorWrong));
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

            var fileDialog = new SaveFileDialog
                                 {
                                     FileName = fileName,
                                     InitialDirectory = Settings.WorkingDir + GetDirectoryFromFileName(GetFileName()),
                                     AddExtension = true
                                 };
            fileDialog.DefaultExt = GitCommandHelpers.GetFileExtension(fileDialog.FileName);
            fileDialog.Filter = string.Format(_currentFormatFilter.Text, GitCommandHelpers.GetFileExtension(fileDialog.FileName)) + "|*." + 
                                GitCommandHelpers.GetFileExtension(fileDialog.FileName) + "|" + _allFilesFilter.Text + "|*.*";
            
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
            ConflictedFilesContextMenu.Enabled = !string.IsNullOrEmpty(GetFileName());
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
            var processStart = new FormStatus.ProcessStart
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
            var process = new FormStatus(processStart, null) { Text = string.Format(stageFilename.Text, filename) };
            process.ShowDialogOnError();
        }


        private void conflictDescription_Click(object sender, EventArgs e)
        {

        }

        private void merge_Click(object sender, EventArgs e)
        {
            OpenMergetool_Click(sender, e);
        }


        private void ConflictedFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OpenMergetool_Click(sender, e);
                e.Handled = true;
            }
        }

        private void fileHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormFileHistory(GetFileName()).ShowDialog();
        }

        #region Hotkey commands

        public const string HotkeySettingsName = "FormMergeConflicts";

        internal enum Commands : int
        {
            Merge,
            Rescan,
            ChooseRemote,
            ChooseLocal,
            ChooseBase
        }

        protected override bool ExecuteCommand(int cmd)
        {
            Commands command = (Commands)cmd;

            switch (command)
            {
                case Commands.Merge: this.OpenMergetool_Click(null, null); break;
                case Commands.Rescan: this.Rescan_Click(null, null); break;
                case Commands.ChooseBase: this.ContextChooseBase_Click(null, null); break;
                case Commands.ChooseLocal: this.ContextChooseLocal_Click(null, null); break;
                case Commands.ChooseRemote: this.ContextChooseRemote_Click(null, null); break;
                default: ExecuteScriptCommand(cmd, Keys.None); break;
            }

            return true;
        }

        #endregion
    }
}
