﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using GitUI.CommandsDialogs.ResolveConflictsDialog;
using GitUI.Hotkey;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormResolveConflicts : GitModuleForm
    {
        #region Translation
        private readonly TranslationString uskUseCustomMergeScript = new TranslationString("There is a custom merge script ({0}) for this file type." + Environment.NewLine + Environment.NewLine + "Do you want to use this custom merge script?");
        private readonly TranslationString uskUseCustomMergeScriptCaption = new TranslationString("Custom merge script");
        private readonly TranslationString fileUnchangedAfterMerge = new TranslationString("The file has not been modified by the merge. Usually this means that the file has been saved to the wrong location." + Environment.NewLine + Environment.NewLine + "The merge conflict will not be marked as solved. Please try again.");
        private readonly TranslationString allConflictsResolved = new TranslationString("All mergeconflicts are resolved, you can commit." + Environment.NewLine + "Do you want to commit now?");
        private readonly TranslationString allConflictsResolvedCaption = new TranslationString("Commit");
        private readonly TranslationString fileIsBinary = new TranslationString("The selected file appears to be a binary file." + Environment.NewLine + "Are you sure you want to open this file in {0}?");
        private readonly TranslationString askMergeConflictSolvedAfterCustomMergeScript = new TranslationString("The merge conflict need to be solved and the result must be saved as:" + Environment.NewLine + "{0}" + Environment.NewLine + Environment.NewLine + "Is the mergeconflict solved?");
        private readonly TranslationString askMergeConflictSolved = new TranslationString("Is the mergeconflict solved?");
        private readonly TranslationString askMergeConflictSolvedCaption = new TranslationString("Conflict solved?");
        private readonly TranslationString noMergeTool = new TranslationString("There is no mergetool configured. Please go to settings and set a mergetool!");
        private readonly TranslationString stageFilename = new TranslationString("Stage {0}");

        private readonly TranslationString noBaseRevision = new TranslationString("There is no base revision for {0}.\nFall back to 2-way merge?");
        private readonly TranslationString ours = new TranslationString("ours");
        private readonly TranslationString theirs = new TranslationString("theirs");
        private readonly TranslationString fileBinairyChooseLocalBaseRemote = new TranslationString("File ({0}) appears to be a binary file.\nChoose to keep the local({1}), remote({2}) or base file.");
        private readonly TranslationString fileChangeLocallyAndRemotely = new TranslationString("The file has been changed both locally({0}) and remotely({1}). Merge the changes.");
        private readonly TranslationString fileCreatedLocallyAndRemotely = new TranslationString("A file with the same name has been created locally({0}) and remotely({1}). Choose the file you want to keep or merge the files.");
        private readonly TranslationString fileCreatedLocallyAndRemotelyLong = new TranslationString("File {0} does not have a base revision.\nA file with the same name has been created locally({1}) and remotely({2}) causing this conflict.\n\nChoose the file you want to keep, merge the files or delete the file?");
        private readonly TranslationString fileDeletedLocallyAndModifiedRemotely = new TranslationString("The file has been deleted locally({0}) and modified remotely({1}). Choose to delete the file or keep the modified version.");
        private readonly TranslationString fileDeletedLocallyAndModifiedRemotelyLong = new TranslationString("File {0} does not have a local revision.\nThe file has been deleted locally({1}) but modified remotely({2}).\n\nChoose to delete the file or keep the modified version.");
        private readonly TranslationString fileModifiedLocallyAndDelededRemotely = new TranslationString("The file has been modified locally({0}) and deleted remotely({1}). Choose to delete the file or keep the modified version.");
        private readonly TranslationString fileModifiedLocallyAndDelededRemotelyLong = new TranslationString("File {0} does not have a remote revision.\nThe file has been modified locally({1}) but deleted remotely({2}).\n\nChoose to delete the file or keep the modified version.");
        private readonly TranslationString noBase = new TranslationString("no base");
        private readonly TranslationString deleted = new TranslationString("deleted");
        private readonly TranslationString chooseLocalButtonText = new TranslationString("Choose local");
        private readonly TranslationString chooseRemoteButtonText = new TranslationString("Choose remote");
        private readonly TranslationString deleteFileButtonText = new TranslationString("Delete file");
        private readonly TranslationString keepModifiedButtonText = new TranslationString("Keep modified");
        private readonly TranslationString keepBaseButtonText = new TranslationString("Keep base file");

        private readonly TranslationString _conflictedFilesContextMenuText = new TranslationString("Solve");
        private readonly TranslationString _openMergeToolItemText = new TranslationString("Open in");
        private readonly TranslationString _button1Text = new TranslationString("Open in");

        private readonly TranslationString _resetItemRebaseText = new TranslationString("Abort rebase");
        private readonly TranslationString _contextChooseLocalRebaseText = new TranslationString("Choose local (theirs)");
        private readonly TranslationString _contextChooseRemoteRebaseText = new TranslationString("Choose remote (ours)");

        private readonly TranslationString _resetItemMergeText = new TranslationString("Abort merge");
        private readonly TranslationString _contextChooseLocalMergeText = new TranslationString("Choose local (ours)");
        private readonly TranslationString _contextChooseRemoteMergeText = new TranslationString("Choose remote (theirs)");

        private readonly TranslationString _binaryFileWarningCaption = new TranslationString("Warning");

        private readonly TranslationString _noBaseFileMergeCaption = new TranslationString("Merge");

        private readonly TranslationString _chooseBaseFileFailedText = new TranslationString("Choose base file failed.");
        private readonly TranslationString _chooseLocalFileFailedText = new TranslationString("Choose local file failed.");
        private readonly TranslationString _chooseRemoteFileFailedText = new TranslationString("Choose remote file failed.");

        private readonly TranslationString _currentFormatFilter =
            new TranslationString("Current format (*.{0})");
        private readonly TranslationString _allFilesFilter =
            new TranslationString("All files (*.*)");

        private readonly TranslationString _abortCurrentOpperation =
            new TranslationString("You can abort the current operation by resetting changes." + Environment.NewLine +
                "All changes since the last commit will be deleted." + Environment.NewLine +
                Environment.NewLine + "Do you want to reset changes?");

        private readonly TranslationString _abortCurrentOpperationCaption = new TranslationString("Abort");

        private readonly TranslationString _areYouSureYouWantDeleteFiles =
            new TranslationString("Are you sure you want to DELETE all changes?" + Environment.NewLine +
                Environment.NewLine + "This action cannot be made undone.");

        private readonly TranslationString _areYouSureYouWantDeleteFilesCaption = new TranslationString("WARNING!");

        private readonly TranslationString _failureWhileOpenFile = new TranslationString("Open temporary file failed.");
        private readonly TranslationString _failureWhileSaveFile = new TranslationString("Save file failed.");
        #endregion

        public FormResolveConflicts(GitUICommands aCommands)
            : this(aCommands, true)
        { }

        public FormResolveConflicts(GitUICommands aCommands, bool offerCommit)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
            _offerCommit = offerCommit;
        }

        private FormResolveConflicts()
            : this(null)
        {
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);
            _thereWhereMergeConflicts = Module.InTheMiddleOfConflictedMerge();
            merge.Focus();
            merge.Select();

            this.HotkeysEnabled = true;
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Directory.SetCurrentDirectory(Module.WorkingDir);
            Module.RunExternalCmdShowConsole(AppSettings.GitCommand, "mergetool");
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private readonly bool _offerCommit;
        private bool _thereWhereMergeConflicts;

        private void FormResolveConflicts_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            Cursor.Current = Cursors.WaitCursor;

            ConflictedFiles.MultiSelect = false;
            int oldSelectedRow = 0;
            if (ConflictedFiles.SelectedRows.Count > 0)
                oldSelectedRow = ConflictedFiles.SelectedRows[0].Index;
            ConflictedFiles.DataSource = Module.GetConflicts();
            ConflictedFiles.Columns[0].DataPropertyName = "Filename";
            if (ConflictedFiles.Rows.Count > oldSelectedRow)
            {
                ConflictedFiles.Rows[oldSelectedRow].Selected = true;

                if (oldSelectedRow < ConflictedFiles.FirstDisplayedScrollingRowIndex ||
                    oldSelectedRow > (ConflictedFiles.FirstDisplayedScrollingRowIndex + ConflictedFiles.DisplayedRowCount(false)))
                {
                    try
                    {
                        ConflictedFiles.FirstDisplayedScrollingRowIndex = oldSelectedRow;
                    }
                    catch (InvalidOperationException)
                    {
                        //ignore the exception - setting the row index is not so important to crash the app
                        //see the #2975 issues for details
                    }
                }
            }

            InitMergetool();

            ConflictedFilesContextMenu.Text = _conflictedFilesContextMenuText.Text;
            OpenMergetool.Text = _openMergeToolItemText.Text + " " + _mergetool;
            openMergeToolBtn.Text = _button1Text.Text + " " + _mergetool;

            if (Module.InTheMiddleOfRebase())
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

            if (!Module.InTheMiddleOfConflictedMerge() && _thereWhereMergeConflicts)
            {
                UICommands.UpdateSubmodules(this);

                if (!Module.InTheMiddleOfPatch() && !Module.InTheMiddleOfRebase() && _offerCommit)
                {
                    if (MessageBox.Show(this, allConflictsResolved.Text, allConflictsResolvedCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        UICommands.StartCommitDialog(this);
                    }
                }

                Close();
            }

            Cursor.Current = Cursors.Default;
        }

        private void Rescan_Click(object sender, EventArgs e)
        {
            Initialize();
        }

        private string _mergetool;
        private string _mergetoolCmd;
        private string _mergetoolPath;

        private ConflictData GetConflict()
        {
            return (ConflictData)ConflictedFiles.SelectedRows[0].DataBoundItem;
        }

        private string GetFileName()
        {
            return GetConflict().Filename;
        }

        private string FixPath(string path)
        {
            return (path ?? "").ToNativePath();
        }

        private readonly Dictionary<string, string> _mergeScripts = new Dictionary<string, string>()
            {
                {".doc",  "merge-doc.js"},
                {".docx", "merge-doc.js"},
                {".docm", "merge-doc.js"},
                {".ods",  "merge-ods.vbs"},
                {".odt",  "merge-ods.vbs"},
                {".sxw",  "merge-ods.vbs"},
            };

        private bool TryMergeWithScript(string fileName, string baseFileName, string remoteFileName, string localFileName)
        {
            if (!EnvUtils.RunningOnWindows())
                return false;

            try
            {
                string extension = Path.GetExtension(fileName).ToLower();
                if (extension.Length <= 1)
                    return false;

                string dir = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Diff-Scripts").EnsureTrailingPathSeparator();
                if (Directory.Exists(dir))
                {
                    string mergeScript = "";
                    if (_mergeScripts.TryGetValue(extension, out mergeScript) &&
                        File.Exists(Path.Combine(dir, mergeScript)))
                    {
                        if (MessageBox.Show(this, string.Format(uskUseCustomMergeScript.Text, mergeScript),
                                            uskUseCustomMergeScriptCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                            DialogResult.Yes)
                        {
                            UseMergeWithScript(fileName, Path.Combine(dir, mergeScript), baseFileName, remoteFileName, localFileName);

                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Merge using script failed.\n" + ex);
            }
            return false;
        }

        private void UseMergeWithScript(string fileName, string mergeScript, string baseFileName, string remoteFileName, string localFileName)
        {
            //get timestamp of file before merge. This is an extra check to verify if merge was successfully
            DateTime lastWriteTimeBeforeMerge = DateTime.Now;
            if (File.Exists(Path.Combine(Module.WorkingDir, fileName)))
                lastWriteTimeBeforeMerge = File.GetLastWriteTime(Path.Combine(Module.WorkingDir, fileName));

            Module.RunCmd("wscript", "\"" + mergeScript + "\" \"" +
                FixPath(Module.WorkingDir + fileName) + "\" \"" + FixPath(remoteFileName) + "\" \"" +
                FixPath(localFileName) + "\" \"" + FixPath(baseFileName) + "\"");

            if (MessageBox.Show(this, string.Format(askMergeConflictSolvedAfterCustomMergeScript.Text,
                FixPath(Path.Combine(Module.WorkingDir, fileName))), askMergeConflictSolvedCaption.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {

                DateTime lastWriteTimeAfterMerge = lastWriteTimeBeforeMerge;
                if (File.Exists(Path.Combine(Module.WorkingDir, fileName)))
                    lastWriteTimeAfterMerge = File.GetLastWriteTime(Path.Combine(Module.WorkingDir, fileName));

                //The file is not modified, do not stage file and present warning
                if (lastWriteTimeBeforeMerge == lastWriteTimeAfterMerge)
                    MessageBox.Show(this, fileUnchangedAfterMerge.Text);
                else
                    StageFile(fileName);
            }

            Initialize();
            if (File.Exists(baseFileName)) File.Delete(baseFileName);
            if (File.Exists(remoteFileName)) File.Delete(remoteFileName);
            if (File.Exists(localFileName)) File.Delete(localFileName);
        }

        private void Use2WayMerge(ref string arguments)
        {
            string mergeToolLower = _mergetool.ToLowerInvariant();
            switch (mergeToolLower)
            {
                case "kdiff3":
                case "diffmerge":
                case "beyondcompare3":
                    arguments = arguments.Replace("\"$BASE\"", "");
                    break;
                case "araxis":
                    arguments = arguments.Replace("-merge -3", "-merge");
                    arguments = arguments.Replace("\"$BASE\"", "");
                    break;
                case "tortoisemerge":
                    arguments = arguments.Replace("-base:\"$BASE\"", "").Replace("/base:\"$BASE\"", "");
                    arguments = arguments.Replace("mine:\"$LOCAL\"", "base:\"$LOCAL\"");
                    break;
            }
        }

        enum ItemType
        {
            File,
            Directory,
            Submodule
        }

        private ItemType GetItemType(string filename)
        {
            string fullname = Path.Combine(Module.WorkingDir, filename);
            if (Directory.Exists(fullname) && !File.Exists(fullname))
            {
                if (Module.IsSubmodule(filename.Trim()))
                    return ItemType.Submodule;
                return ItemType.Directory;
            }
            return ItemType.File;
        }

        private void ConflictedFiles_DoubleClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConflictedFiles.SelectedRows.Count != 1)
                return;

            try
            {
                var item = GetConflict();
                var itemType = GetItemType(item.Filename);
                if (itemType == ItemType.Submodule)
                {
                    var form = new FormMergeSubmodule(UICommands, item.Filename);
                    if (form.ShowDialog() == DialogResult.OK)
                        StageFile(item.Filename);
                }
                else if (itemType == ItemType.File)
                {
                    ResolveFilesConflict(item);
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                Initialize();
            }
        }

        private void ResolveFilesConflict(ConflictData item)
        {
            string[] filenames = Module.CheckoutConflictedFiles(item);
            try
            {
                if (CheckForLocalRevision(item) &&
                    CheckForRemoteRevision(item))
                {
                    if (TryMergeWithScript(item.Filename, filenames[0], filenames[1], filenames[2]))
                    {
                        Cursor.Current = Cursors.Default;
                        return;
                    }

                    if (FileHelper.IsBinaryFile(Module, item.Local.Filename))
                    {
                        if (MessageBox.Show(this, string.Format(fileIsBinary.Text, _mergetool),
                            _binaryFileWarningCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2) == DialogResult.No)
                        {
                            BinairyFilesChooseLocalBaseRemote(item);
                            return;
                        }
                    }

                    string arguments = _mergetoolCmd;
                    //Check if there is a base file. If not, ask user to fall back to 2-way merge.
                    //git doesn't support 2-way merge, but we can try to adjust attributes to fix this.
                    //For kdiff3 this is easy; just remove the 3rd file from the arguments. Since the
                    //filenames are quoted, this takes a little extra effort. We need to remove these 
                    //quotes also. For tortoise and araxis a little bit more magic is needed.
                    if (item.Base.Filename == null)
                    {
                        var text = string.Format(noBaseRevision.Text, item.Filename);
                        DialogResult result = MessageBox.Show(this, text, _noBaseFileMergeCaption.Text, 
                            MessageBoxButtons.YesNoCancel);
                        if (result == DialogResult.Yes)
                            Use2WayMerge(ref arguments);

                        if (result == DialogResult.Cancel)
                            return;
                    }

                    arguments = arguments.Replace("$BASE", filenames[0]);
                    arguments = arguments.Replace("$LOCAL", filenames[1]);
                    arguments = arguments.Replace("$REMOTE", filenames[2]);
                    arguments = arguments.Replace("$MERGED", item.Filename);

                    //get timestamp of file before merge. This is an extra check to verify if merge was successful
                    DateTime lastWriteTimeBeforeMerge = DateTime.Now;
                    if (File.Exists(Path.Combine(Module.WorkingDir, item.Filename)))
                        lastWriteTimeBeforeMerge = File.GetLastWriteTime(Path.Combine(Module.WorkingDir, item.Filename));

                    var res = Module.RunCmdResult(_mergetoolPath, "" + arguments + "");

                    DateTime lastWriteTimeAfterMerge = lastWriteTimeBeforeMerge;
                    if (File.Exists(Path.Combine(Module.WorkingDir, item.Filename)))
                        lastWriteTimeAfterMerge = File.GetLastWriteTime(Path.Combine(Module.WorkingDir, item.Filename));

                    //Check exitcode AND timestamp of the file. If exitcode is success and
                    //time timestamp is changed, we are pretty sure the merge was done.
                    if (res.ExitCode == 0 && lastWriteTimeBeforeMerge != lastWriteTimeAfterMerge)
                    {
                        StageFile(item.Filename);
                    }

                    //If the exitcode is 1, but the file is changed, ask if the merge conflict is solved.
                    //If the exitcode is 0, but the file is not changed, ask if the merge conflict is solved.
                    if ((res.ExitCode == 1 && lastWriteTimeBeforeMerge != lastWriteTimeAfterMerge) ||
                        (res.ExitCode == 0 && lastWriteTimeBeforeMerge == lastWriteTimeAfterMerge))
                    {
                        if (MessageBox.Show(this, askMergeConflictSolved.Text, askMergeConflictSolvedCaption.Text,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            StageFile(item.Filename);
                        }
                    }
                }
            }
            finally
            {
                DeleteTemporaryFiles(filenames);
            }
        }

        private static void DeleteTemporaryFiles(string[] filenames)
        {
            if (filenames[0] != null && File.Exists(filenames[0]))
                File.Delete(filenames[0]);
            if (filenames[1] != null && File.Exists(filenames[1]))
                File.Delete(filenames[1]);
            if (filenames[2] != null && File.Exists(filenames[2]))
                File.Delete(filenames[2]);
        }

        private void InitMergetool()
        {
            _mergetool = Module.GetEffectiveSetting("merge.tool");

            if (string.IsNullOrEmpty(_mergetool))
            {
                MessageBox.Show(this, noMergeTool.Text);
                return;
            }
            Cursor.Current = Cursors.WaitCursor;

            _mergetoolCmd = Module.GetEffectivePathSetting(string.Format("mergetool.{0}.cmd", _mergetool));

            _mergetoolPath = Module.GetEffectivePathSetting(string.Format("mergetool.{0}.path", _mergetool));

            if (string.IsNullOrEmpty(_mergetool) || _mergetool == "kdiff3")
            {
                if (string.IsNullOrEmpty(_mergetoolPath))
                    _mergetoolPath = "kdiff3";
                _mergetoolCmd = "\"$BASE\" \"$LOCAL\" \"$REMOTE\" -o \"$MERGED\"";
            }
            else
            {
                //This only works when on Windows....
                const string executablePattern = ".exe";
                int idx = _mergetoolCmd.IndexOf(executablePattern);
                if (idx >= 0)
                {
                    _mergetoolPath = _mergetoolCmd.Substring(0, idx + executablePattern.Length + 1).Trim(new[] { '\"', ' ' });
                    _mergetoolCmd = _mergetoolCmd.Substring(idx + executablePattern.Length + 1);
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private bool ShowAbortMessage()
        {
            if (MessageBox.Show(_abortCurrentOpperation.Text, _abortCurrentOpperationCaption.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (MessageBox.Show(_areYouSureYouWantDeleteFiles.Text, _areYouSureYouWantDeleteFilesCaption.Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    return true;
            }
            return false;
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ShowAbortMessage())
            {
                Module.ResetHard("");
                Close();
            }
            Cursor.Current = Cursors.Default;
        }

        private string GetRemoteSideString()
        {
            bool inTheMiddleOfRebase = Module.InTheMiddleOfRebase();
            return inTheMiddleOfRebase ? ours.Text : theirs.Text;
        }
        private string GetLocalSideString()
        {
            bool inTheMiddleOfRebase = Module.InTheMiddleOfRebase();
            return inTheMiddleOfRebase ? theirs.Text : ours.Text;
        }

        private string GetShortHash(ConflictedFileData item)
        {
            if (item.Hash == null)
                return "@" + deleted.Text;
            return '@' + item.Hash.Substring(0, 8);
        }

        private void ConflictedFiles_SelectionChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConflictedFiles.SelectedRows.Count != 1)
            {
                baseFileName.Text = localFileName.Text = remoteFileName.Text = "";
                return;
            }

            var item = GetConflict();

            bool baseFileExists = !string.IsNullOrEmpty(item.Base.Filename);
            bool localFileExists = !string.IsNullOrEmpty(item.Local.Filename);
            bool remoteFileExists = !string.IsNullOrEmpty(item.Remote.Filename);

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

            baseFileName.Text = (baseFileExists ? item.Base.Filename : noBase.Text);
            localFileName.Text = (localFileExists ? item.Local.Filename : deleted.Text);
            remoteFileName.Text = (remoteFileExists ? item.Remote.Filename : deleted.Text);

            var itemType = GetItemType(item.Filename);
            if (itemType == ItemType.Submodule)
            {
                baseFileName.Text = baseFileName.Text + GetShortHash(item.Base);
                localFileName.Text = localFileName.Text + GetShortHash(item.Local);
                remoteFileName.Text = remoteFileName.Text + GetShortHash(item.Remote);
            }
        }

        private void ContextChooseBase_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            var item = GetConflict();
            if (CheckForBaseRevision(item))
            {
                ChooseBaseOnConflict(item.Base.Filename);
            }
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void ChooseBaseOnConflict(string fileName)
        {
            if (!Module.HandleConflictSelectSide(fileName, "BASE"))
                MessageBox.Show(this, _chooseBaseFileFailedText.Text);
        }

        private void ContextChooseLocal_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            
            var item = GetConflict();
            if (CheckForLocalRevision(item))
            {
                ChooseLocalOnConflict(item.Filename);
            }
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void ChooseLocalOnConflict(string fileName)
        {
            if (!Module.HandleConflictSelectSide(fileName, "LOCAL"))
                MessageBox.Show(this, _chooseLocalFileFailedText.Text);
        }

        private void ContextChooseRemote_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            var item = GetConflict();
            if (CheckForRemoteRevision(item))
            {
                ChooseRemoteOnConflict(item.Filename);
            }
            Initialize();

            Cursor.Current = Cursors.Default;
        }

        private void ChooseRemoteOnConflict(string fileName)
        {
            if (!Module.HandleConflictSelectSide(fileName, "REMOTE"))
                MessageBox.Show(this, _chooseRemoteFileFailedText.Text);
        }

        private void BinairyFilesChooseLocalBaseRemote(ConflictData item)
        {
            string caption = string.Format(fileBinairyChooseLocalBaseRemote.Text,
                                            item.Local.Filename,
                                            GetLocalSideString(),
                                            GetRemoteSideString());

            using (var frm = new FormModifiedDeletedCreated(string.Format(chooseLocalButtonText.Text + " ({0})", GetLocalSideString()),
                                                                            string.Format(chooseRemoteButtonText.Text + " ({0})", GetRemoteSideString()),
                                                                            keepBaseButtonText.Text,
                                                                            caption))
            {
                frm.ShowDialog(this);
                if (frm.KeepBase) //base
                    ChooseBaseOnConflict(item.Filename);
                if (frm.KeepLocal) //local
                    ChooseLocalOnConflict(item.Filename);
                if (frm.KeepRemote) //remote
                    ChooseRemoteOnConflict(item.Filename);
            }
        }

        private bool CheckForBaseRevision(ConflictData item)
        {
            if (!string.IsNullOrEmpty(item.Base.Filename))
                return true;

            string caption = string.Format(fileCreatedLocallyAndRemotelyLong.Text,
                item.Filename,
                GetLocalSideString(),
                GetRemoteSideString());

            using (var frm = new FormModifiedDeletedCreated(string.Format(chooseLocalButtonText.Text + " ({0})", GetLocalSideString()),
                string.Format(chooseRemoteButtonText.Text + " ({0})", GetRemoteSideString()),
                deleteFileButtonText.Text,
                caption))
            {
                frm.ShowDialog(this);
                if (frm.KeepBase) //delete
                    Module.RunGitCmd("rm -- \"" + item.Filename + "\"");
                if (frm.KeepLocal) //local
                    ChooseLocalOnConflict(item.Filename);
                if (frm.KeepRemote) //remote
                    ChooseRemoteOnConflict(item.Filename);
            }
            return false;
        }

        private bool CheckForLocalRevision(ConflictData item)
        {
            if (!string.IsNullOrEmpty(item.Local.Filename))
                return true;

            string caption = string.Format(fileDeletedLocallyAndModifiedRemotelyLong.Text,
                item.Filename,
                GetLocalSideString(),
                GetRemoteSideString());

            using (var frm = new FormModifiedDeletedCreated(string.Format(deleteFileButtonText.Text + " ({0})", GetLocalSideString()),
                string.Format(keepModifiedButtonText.Text + " ({0})", GetRemoteSideString()),
                keepBaseButtonText.Text,
                caption))
            {
                frm.ShowDialog(this);
                if (frm.KeepBase) //base
                    ChooseBaseOnConflict(item.Filename);
                if (frm.KeepLocal) //delete
                    Module.RunGitCmd("rm -- \"" + item.Filename + "\"");
                if (frm.KeepRemote) //remote
                    ChooseRemoteOnConflict(item.Filename);
            }
            return false;
        }

        private bool CheckForRemoteRevision(ConflictData item)
        {
            if (!string.IsNullOrEmpty(item.Remote.Filename))
                return true;

            string caption = string.Format(fileModifiedLocallyAndDelededRemotelyLong.Text,
                item.Filename,
                GetLocalSideString(),
                GetRemoteSideString());

            using (var frm = new FormModifiedDeletedCreated(string.Format(keepModifiedButtonText.Text + " ({0})", GetLocalSideString()),
                string.Format(deleteFileButtonText.Text + " ({0})", GetRemoteSideString()),
                keepBaseButtonText.Text,
                caption))
            {
                frm.ShowDialog(this);
                if (frm.KeepBase) //base
                    ChooseBaseOnConflict(item.Filename);
                if (frm.KeepLocal) //delete
                    ChooseLocalOnConflict(item.Filename);
                if (frm.KeepRemote) //remote
                    Module.RunGitCmd("rm -- \"" + item.Filename + "\"");
            }
            return false;
        }

        private void OpenMergetool_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            ConflictedFiles_DoubleClick(sender, e);
            Cursor.Current = Cursors.Default;
        }

        private void ContextOpenBaseWith_Click(object sender, EventArgs e)
        {
            OpenSideWith("BASE");
        }

        private void ContextOpenLocalWith_Click(object sender, EventArgs e)
        {
            OpenSideWith("LOCAL");
        }

        private void OpenSideWith(string side)
        {
            Cursor.Current = Cursors.WaitCursor;
            var conflictData = GetConflict();
            string fileName = conflictData.Filename;
            fileName = PathUtil.GetFileName(fileName);

            fileName = Path.GetTempPath() + fileName;

            if (!Module.HandleConflictsSaveSide(conflictData.Filename, fileName, side))
                MessageBox.Show(this, _failureWhileOpenFile.Text);

            OsShellUtil.OpenAs(fileName);
            Cursor.Current = Cursors.Default;
        }

        private void ContextOpenRemoteWith_Click(object sender, EventArgs e)
        {
            OpenSideWith("REMOTE");
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
            var conflictData = GetConflict();
            string fileName = conflictData.Filename;
            fileName = PathUtil.GetFileName(fileName);

            using (var fileDialog = new SaveFileDialog
                                 {
                                     FileName = fileName,
                                     InitialDirectory = Module.WorkingDir + PathUtil.GetDirectoryName(conflictData.Filename),
                                     AddExtension = true
                                 })
            {
                fileDialog.DefaultExt = GitCommandHelpers.GetFileExtension(fileDialog.FileName);
                fileDialog.Filter = string.Format(_currentFormatFilter.Text, GitCommandHelpers.GetFileExtension(fileDialog.FileName)) + "|*." +
                                    GitCommandHelpers.GetFileExtension(fileDialog.FileName) + "|" + _allFilesFilter.Text + "|*.*";

                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (!Module.HandleConflictsSaveSide(conflictData.Filename, fileDialog.FileName, side))
                        MessageBox.Show(this, _failureWhileSaveFile.Text);
                }
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
            StageFile(GetFileName());
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void ConflictedFilesContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (ConflictedFiles.SelectedRows.Count == 0)
            {
                e.Cancel = true;
                return;
            }
            string fileName = GetFileName();
            ConflictedFilesContextMenu.Enabled = !string.IsNullOrEmpty(fileName);

            if (ConflictedFilesContextMenu.Enabled)
            {
                EnableAllEntriesInConflictedFilesContextMenu();
                DisableInvalidEntriesInCoflictedFilesContextMenu(fileName);
            }
        }

        private void EnableAllEntriesInConflictedFilesContextMenu()
        {
            ContextOpenLocalWith.Enabled = true;
            ContextSaveLocalAs.Enabled = true;

            ContextOpenRemoteWith.Enabled = true;
            ContextSaveRemoteAs.Enabled = true;

            ContextOpenBaseWith.Enabled = true;
            ContextSaveBaseAs.Enabled = true;
        }

        private void DisableInvalidEntriesInCoflictedFilesContextMenu(string fileName)
        {
            var conflictedFileNames = Module.GetConflict(fileName);
            if (conflictedFileNames.Local.Filename.IsNullOrEmpty())
            {
                ContextSaveLocalAs.Enabled = false;
                ContextOpenLocalWith.Enabled = false;
            }
            if (conflictedFileNames.Remote.Filename.IsNullOrEmpty())
            {
                ContextSaveRemoteAs.Enabled = false;
                ContextOpenRemoteWith.Enabled = false;
            }
            if (conflictedFileNames.Base.Filename.IsNullOrEmpty())
            {
                ContextSaveBaseAs.Enabled = false;
                ContextOpenBaseWith.Enabled = false;
            }
        }


        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = GetFileName();
            System.Diagnostics.Process.Start(Path.Combine(Module.WorkingDir, fileName));
        }

        private void openWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = GetFileName();
            OsShellUtil.OpenAs(Path.Combine(Module.WorkingDir, fileName));
        }

        private void StageFile(string filename)
        {
            var processStart = new FormStatus.ProcessStart
                (
                    delegate(FormStatus form)
                    {
                        form.AddMessageLine(string.Format(stageFilename.Text, filename));
                        string output = Module.RunGitCmd("add -- \"" + filename + "\"");
                        form.AddMessageLine(output);
                        form.Done(string.IsNullOrEmpty(output));
                    }
                );
            using (var process = new FormStatus(processStart, null) { Text = string.Format(stageFilename.Text, filename) })
                process.ShowDialogOnError(this);
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
            UICommands.StartFileHistoryDialog(this, GetFileName());
        }

        #region Hotkey commands

        public const string HotkeySettingsName = "FormMergeConflicts";

        internal enum Commands
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
                default: return base.ExecuteCommand(cmd);
            }

            return true;
        }

        #endregion
    }
}
