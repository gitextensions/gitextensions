using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using GitUI.CommandsDialogs.ResolveConflictsDialog;
using GitUI.Hotkey;
using ResourceManager.Translation;

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
        private readonly TranslationString mergeConflictIsSubmodule = new TranslationString("The selected mergeconflict is a submodule." + Environment.NewLine + "Stage current submodule commit?");
        private readonly TranslationString mergeConflictIsSubmoduleCaption = new TranslationString("Submodule");
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

        private bool _offerCommit;
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
            ConflictedFiles.DataSource = Module.GetConflictedFiles();
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
            openMergeToolBtn.Text = _button1Text.Text + " " + mergetool;

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

            if (!Module.InTheMiddleOfPatch() && !Module.InTheMiddleOfRebase() &&
                !Module.InTheMiddleOfConflictedMerge() && _thereWhereMergeConflicts && _offerCommit)
            {
                if (MessageBox.Show(this, allConflictsResolved.Text, allConflictsResolvedCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    UICommands.StartCommitDialog(this);
                }
            }

            if (!Module.InTheMiddleOfConflictedMerge() && _thereWhereMergeConflicts)
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

        private string FixPath(string path)
        {
            return (path ?? "").Replace(AppSettings.PathSeparatorWrong, AppSettings.PathSeparator);
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

                string dir = Path.GetDirectoryName(Application.ExecutablePath) +
                    AppSettings.PathSeparator + "Diff-Scripts" + AppSettings.PathSeparator;
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

            int exitCode;
            Module.RunCmd("wscript", "\"" + mergeScript + "\" \"" +
                FixPath(Module.WorkingDir + fileName) + "\" \"" + FixPath(remoteFileName) + "\" \"" +
                FixPath(localFileName) + "\" \"" + FixPath(baseFileName) + "\"", out exitCode);

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
                    stageFile(fileName);
            }

            Initialize();
            if (File.Exists(baseFileName)) File.Delete(baseFileName);
            if (File.Exists(remoteFileName)) File.Delete(remoteFileName);
            if (File.Exists(localFileName)) File.Delete(localFileName);
        }

        private void Use2WayMerge(ref string arguments)
        {
            string mergeToolLower = mergetool.ToLowerInvariant();
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

        private void ConflictedFiles_DoubleClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConflictedFiles.SelectedRows.Count != 1)
                return;

            try
            {
                string filename = GetFileName();
                string fullname = Path.Combine(Module.WorkingDir, filename);
                if (Directory.Exists(fullname) && !File.Exists(fullname))
                {
                    if (Module.GetSubmodulesLocalPathes().Contains(filename.Trim()))
                    {
                        var form = new FormMergeSubmodule(UICommands, filename);
                        if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            stageFile(filename);

                    }
                }
                else
                {
                    ResolveFilesConflict(filename);
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                Initialize();
            }
        }

        private void ResolveFilesConflict(string filename)
        {
            string[] filenames = Module.GetConflictedFiles(filename);
            try
            {
                if (CheckForLocalRevision(filename) &&
                    CheckForRemoteRevision(filename))
                {
                    if (TryMergeWithScript(filename, filenames[0], filenames[2], filenames[1]))
                    {
                        Cursor.Current = Cursors.Default;
                        return;
                    }

                    if (FileHelper.IsBinaryFile(Module, filename))
                    {
                        if (MessageBox.Show(this, string.Format(fileIsBinary.Text, mergetool), _binaryFileWarningCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                        {
                            BinairyFilesChooseLocalBaseRemote(filename);
                            return;
                        }
                    }

                    string arguments = mergetoolCmd;
                    //Check if there is a base file. If not, ask user to fall back to 2-way merge.
                    //git doesn't support 2-way merge, but we can try to adjust attributes to fix this.
                    //For kdiff3 this is easy; just remove the 3rd file from the arguments. Since the
                    //filenames are quoted, this takes a little extra effort. We need to remove these 
                    //quotes also. For tortoise and araxis a little bit more magic is needed.
                    if (filenames[0] == null)
                    {
                        DialogResult result = MessageBox.Show(this, string.Format(noBaseRevision.Text, filename), _noBaseFileMergeCaption.Text, MessageBoxButtons.YesNoCancel);
                        if (result == DialogResult.Yes)
                            Use2WayMerge(ref arguments);

                        if (result == DialogResult.Cancel)
                            return;
                    }

                    arguments = arguments.Replace("$BASE", filenames[0]);
                    arguments = arguments.Replace("$LOCAL", filenames[1]);
                    arguments = arguments.Replace("$REMOTE", filenames[2]);
                    arguments = arguments.Replace("$MERGED", filename);

                    //get timestamp of file before merge. This is an extra check to verify if merge was successful
                    DateTime lastWriteTimeBeforeMerge = DateTime.Now;
                    if (File.Exists(Path.Combine(Module.WorkingDir, filename)))
                        lastWriteTimeBeforeMerge = File.GetLastWriteTime(Path.Combine(Module.WorkingDir, filename));

                    int exitCode;
                    Module.RunCmd(mergetoolPath, "" + arguments + "", out exitCode);

                    DateTime lastWriteTimeAfterMerge = lastWriteTimeBeforeMerge;
                    if (File.Exists(Path.Combine(Module.WorkingDir, filename)))
                        lastWriteTimeAfterMerge = File.GetLastWriteTime(Path.Combine(Module.WorkingDir, filename));

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
                        if (MessageBox.Show(this, askMergeConflictSolved.Text, askMergeConflictSolvedCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            stageFile(filename);
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
            mergetool = Module.GetEffectiveSetting("merge.tool");

            if (string.IsNullOrEmpty(mergetool))
            {
                MessageBox.Show(this, noMergeTool.Text);
                return;
            }
            Cursor.Current = Cursors.WaitCursor;

            mergetoolCmd = Module.GetEffectivePathSetting(string.Format("mergetool.{0}.cmd", mergetool));

            mergetoolPath = Module.GetEffectivePathSetting(string.Format("mergetool.{0}.path", mergetool));

            if (string.IsNullOrEmpty(mergetool) || mergetool == "kdiff3")
            {
                if (string.IsNullOrEmpty(mergetoolPath))
                    mergetoolPath = "kdiff3";
                mergetoolCmd = "\"$BASE\" \"$LOCAL\" \"$REMOTE\" -o \"$MERGED\"";
            }
            else
            {
                //This only works when on Windows....
                const string executablePattern = ".exe";
                int idx = mergetoolCmd.IndexOf(executablePattern);
                if (idx >= 0)
                {
                    mergetoolPath = mergetoolCmd.Substring(0, idx + executablePattern.Length + 1).Trim(new[] { '\"', ' ' });
                    mergetoolCmd = mergetoolCmd.Substring(idx + executablePattern.Length + 1);
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

        private void ConflictedFiles_SelectionChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (ConflictedFiles.SelectedRows.Count != 1)
            {
                baseFileName.Text = localFileName.Text = remoteFileName.Text = "";
                return;
            }

            string filename = GetFileName();
            string[] filenames = Module.GetConflictedFileNames(filename);

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

            string fileName = GetFileName();

            if (CheckForBaseRevision(fileName))
            {
                ChooseBaseOnConflict(fileName);
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

            string fileName = GetFileName();
            if (CheckForLocalRevision(fileName))
            {
                ChooseLocalOnConflict(fileName);
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

            string fileName = GetFileName();
            if (CheckForRemoteRevision(fileName))
            {
                ChooseRemoteOnConflict(fileName);
            }
            Initialize();

            Cursor.Current = Cursors.Default;
        }

        private void ChooseRemoteOnConflict(string fileName)
        {
            if (!Module.HandleConflictSelectSide(fileName, "REMOTE"))
                MessageBox.Show(this, _chooseRemoteFileFailedText.Text);
        }

        private void BinairyFilesChooseLocalBaseRemote(string filename)
        {
            string caption = string.Format(fileBinairyChooseLocalBaseRemote.Text,
                                            filename,
                                            GetLocalSideString(),
                                            GetRemoteSideString());

            using (var frm = new FormModifiedDeletedCreated(string.Format(chooseLocalButtonText.Text + " ({0})", GetLocalSideString()),
                                                                            string.Format(chooseRemoteButtonText.Text + " ({0})", GetRemoteSideString()),
                                                                            keepBaseButtonText.Text,
                                                                            caption))
            {
                frm.ShowDialog(this);
                if (frm.KeepBase) //base
                    ChooseBaseOnConflict(GetFileName());
                if (frm.KeepLocal) //local
                    ChooseLocalOnConflict(GetFileName());
                if (frm.KeepRemote) //remote
                    ChooseRemoteOnConflict(GetFileName());
            }
        }

        private bool CheckForBaseRevision(string filename)
        {
            if (string.IsNullOrEmpty(Module.GetConflictedFileNames(filename)[0]))
            {
                string caption = string.Format(fileCreatedLocallyAndRemotelyLong.Text,
                                                filename,
                                                GetLocalSideString(),
                                                GetRemoteSideString());

                using (var frm = new FormModifiedDeletedCreated(string.Format(chooseLocalButtonText.Text + " ({0})", GetLocalSideString()),
                                                                                string.Format(chooseRemoteButtonText.Text + " ({0})", GetRemoteSideString()),
                                                                                deleteFileButtonText.Text,
                                                                                caption))
                {
                    frm.ShowDialog(this);
                    if (frm.KeepBase) //delete
                        Module.RunGitCmd("rm -- \"" + filename + "\"");
                    if (frm.KeepLocal) //local
                        ChooseLocalOnConflict(GetFileName());
                    if (frm.KeepRemote) //remote
                        ChooseRemoteOnConflict(GetFileName());
                }
                return false;
            }
            return true;
        }

        private bool CheckForLocalRevision(string filename)
        {
            if (string.IsNullOrEmpty(Module.GetConflictedFileNames(filename)[1]))
            {
                string caption = string.Format(fileDeletedLocallyAndModifiedRemotelyLong.Text,
                                                filename,
                                                GetLocalSideString(),
                                                GetRemoteSideString());

                using (var frm = new FormModifiedDeletedCreated(string.Format(deleteFileButtonText.Text + " ({0})", GetLocalSideString()),
                                                                                string.Format(keepModifiedButtonText.Text + " ({0})", GetRemoteSideString()),
                                                                                keepBaseButtonText.Text,
                                                                                caption))
                {
                    frm.ShowDialog(this);
                    if (frm.KeepBase) //base
                        ChooseBaseOnConflict(GetFileName());
                    if (frm.KeepLocal) //delete
                        Module.RunGitCmd("rm -- \"" + filename + "\"");
                    if (frm.KeepRemote) //remote
                        ChooseRemoteOnConflict(GetFileName());
                }
                return false;
            }
            return true;
        }

        private bool CheckForRemoteRevision(string filename)
        {
            if (string.IsNullOrEmpty(Module.GetConflictedFileNames(filename)[2]))
            {
                string caption = string.Format(fileModifiedLocallyAndDelededRemotelyLong.Text,
                                                filename,
                                                GetLocalSideString(),
                                                GetRemoteSideString());

                using (var frm = new FormModifiedDeletedCreated(string.Format(keepModifiedButtonText.Text + " ({0})", GetLocalSideString()),
                                                                                string.Format(deleteFileButtonText.Text + " ({0})", GetRemoteSideString()),
                                                                                keepBaseButtonText.Text,
                                                                                caption))
                {
                    frm.ShowDialog(this);
                    if (frm.KeepBase) //base
                        ChooseBaseOnConflict(GetFileName());
                    if (frm.KeepLocal) //delete
                        ChooseLocalOnConflict(GetFileName());
                    if (frm.KeepRemote) //remote
                        Module.RunGitCmd("rm -- \"" + filename + "\"");
                }
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
            OpenSideWith("BASE");
        }

        private void ContextOpenLocalWith_Click(object sender, EventArgs e)
        {
            OpenSideWith("LOCAL");
        }

        private void OpenSideWith(string side)
        {
            Cursor.Current = Cursors.WaitCursor;
            string fileName = GetFileName();
            fileName = GetShortFileName(fileName);

            fileName = Path.GetTempPath() + fileName;

            if (!Module.HandleConflictsSaveSide(GetFileName(), fileName, side))
                MessageBox.Show(this, _failureWhileOpenFile.Text);

            OsShellUtil.OpenAs(fileName);
            Cursor.Current = Cursors.Default;
        }

        
        private static string GetShortFileName(string fileName)
        {
            if (fileName.Contains(AppSettings.PathSeparator.ToString()) && fileName.LastIndexOf(AppSettings.PathSeparator.ToString()) < fileName.Length)
                fileName = fileName.Substring(fileName.LastIndexOf(AppSettings.PathSeparator) + 1);
            if (fileName.Contains(AppSettings.PathSeparatorWrong.ToString()) && fileName.LastIndexOf(AppSettings.PathSeparatorWrong.ToString()) < fileName.Length)
                fileName = fileName.Substring(fileName.LastIndexOf(AppSettings.PathSeparatorWrong) + 1);
            return fileName;
        }

        private static string GetDirectoryFromFileName(string fileName)
        {
            if (fileName.Contains(AppSettings.PathSeparator.ToString()) && fileName.LastIndexOf(AppSettings.PathSeparator.ToString()) < fileName.Length)
                fileName = fileName.Substring(0, fileName.LastIndexOf(AppSettings.PathSeparator));
            if (fileName.Contains(AppSettings.PathSeparatorWrong.ToString()) && fileName.LastIndexOf(AppSettings.PathSeparatorWrong.ToString()) < fileName.Length)
                fileName = fileName.Substring(0, fileName.LastIndexOf(AppSettings.PathSeparatorWrong));
            return fileName;
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
            string fileName = GetFileName();
            fileName = GetShortFileName(fileName);

            using (var fileDialog = new SaveFileDialog
                                 {
                                     FileName = fileName,
                                     InitialDirectory = Module.WorkingDir + GetDirectoryFromFileName(GetFileName()),
                                     AddExtension = true
                                 })
            {
                fileDialog.DefaultExt = GitCommandHelpers.GetFileExtension(fileDialog.FileName);
                fileDialog.Filter = string.Format(_currentFormatFilter.Text, GitCommandHelpers.GetFileExtension(fileDialog.FileName)) + "|*." +
                                    GitCommandHelpers.GetFileExtension(fileDialog.FileName) + "|" + _allFilesFilter.Text + "|*.*";

                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (!Module.HandleConflictsSaveSide(GetFileName(), fileDialog.FileName, side))
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
            stageFile(GetFileName());
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void ConflictedFilesContextMenu_Opening(object sender, CancelEventArgs e)
        {
            var fileName = GetFileName();
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
            var conflictedFileNames = Module.GetConflictedFileNames(fileName);
            if (conflictedFileNames[1].IsNullOrEmpty())
            {
                ContextSaveLocalAs.Enabled = false;
                ContextOpenLocalWith.Enabled = false;
            }
            if (conflictedFileNames[2].IsNullOrEmpty())
            {
                ContextSaveRemoteAs.Enabled = false;
                ContextOpenRemoteWith.Enabled = false;
            }
            if (conflictedFileNames[0].IsNullOrEmpty())
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

        private void stageFile(string filename)
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
