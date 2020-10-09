using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Utils;
using GitExtUtils;
using GitUI.HelperDialogs;
using GitUI.Hotkey;
using Microsoft.WindowsAPICodePack.Dialogs;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormResolveConflicts : GitModuleForm
    {
        #region Translation
        private readonly TranslationString _uskUseCustomMergeScript = new TranslationString("There is a custom merge script ({0}) for this file type." + Environment.NewLine + Environment.NewLine + "Do you want to use this custom merge script?");
        private readonly TranslationString _uskUseCustomMergeScriptCaption = new TranslationString("Custom merge script");
        private readonly TranslationString _fileUnchangedAfterMerge = new TranslationString("The file has not been modified by the merge. Usually this means that the file has been saved to the wrong location." + Environment.NewLine + Environment.NewLine + "The merge conflict will not be marked as solved. Please try again.");
        private readonly TranslationString _allConflictsResolved = new TranslationString("All merge conflicts are resolved, you can commit." + Environment.NewLine + "Do you want to commit now?");
        private readonly TranslationString _allConflictsResolvedCaption = new TranslationString("Commit");
        private readonly TranslationString _fileIsBinary = new TranslationString("The selected file appears to be a binary file." + Environment.NewLine + "Are you sure you want to open this file in {0}?");
        private readonly TranslationString _askMergeConflictSolvedAfterCustomMergeScript = new TranslationString("The merge conflict need to be solved and the result must be saved as:" + Environment.NewLine + "{0}" + Environment.NewLine + Environment.NewLine + "Is the merge conflict solved?");
        private readonly TranslationString _askMergeConflictSolved = new TranslationString("Is the merge conflict solved?");
        private readonly TranslationString _askMergeConflictSolvedCaption = new TranslationString("Conflict solved?");
        private readonly TranslationString _noMergeTool = new TranslationString("There is no mergetool configured." + Environment.NewLine + "Please go to settings and set a mergetool!");
        private readonly TranslationString _noMergeToolConfigured = new TranslationString("The mergetool is not correctly configured." + Environment.NewLine + "Please go to settings and configure the mergetool!");
        private readonly TranslationString _errorStartingMergetool = new TranslationString("Error starting mergetool: {0}");
        private readonly TranslationString _stageFilename = new TranslationString("Stage {0}");

        private readonly TranslationString _noBaseRevision = new TranslationString("There is no base revision for {0}." + Environment.NewLine + "Fall back to 2-way merge?");
        private readonly TranslationString _ours = new TranslationString("ours");
        private readonly TranslationString _theirs = new TranslationString("theirs");
        private readonly TranslationString _fileBinaryChooseLocalBaseRemote = new TranslationString("File ({0}) appears to be a binary file." + Environment.NewLine + "Choose to keep the local ({1}), remote ({2}) or base file.");
        private readonly TranslationString _fileChangeLocallyAndRemotely = new TranslationString("The file has been changed both locally ({0}) and remotely ({1}). Merge the changes.");
        private readonly TranslationString _fileCreatedLocallyAndRemotely = new TranslationString("A file with the same name has been created locally ({0}) and remotely ({1}). Choose the file you want to keep or merge the files.");
        private readonly TranslationString _fileCreatedLocallyAndRemotelyLong = new TranslationString("File {0} does not have a base revision." + Environment.NewLine + "A file with the same name has been created locally ({1}) and remotely ({2}) causing this conflict." + Environment.NewLine + Environment.NewLine + "Choose the file you want to keep, merge the files or delete the file?");
        private readonly TranslationString _fileDeletedLocallyAndModifiedRemotely = new TranslationString("The file has been deleted locally ({0}) and modified remotely ({1}). Choose to delete the file or keep the modified version.");
        private readonly TranslationString _fileDeletedLocallyAndModifiedRemotelyLong = new TranslationString("File {0} does not have a local revision." + Environment.NewLine + "The file has been deleted locally ({1}) but modified remotely ({2})." + Environment.NewLine + Environment.NewLine + "Choose to delete the file or keep the modified version.");
        private readonly TranslationString _filesDeletedLocallyAndModifiedRemotelyLong = new TranslationString("{0} and {1} other out of {2} selected files do not have a local revision." + Environment.NewLine + "The files have been deleted locally, but modified remotely" + Environment.NewLine + Environment.NewLine + "Choose to delete the files or keep the modified versions.");
        private readonly TranslationString _filesDeletedLocallyAndModifiedRemotelyLongNoOtherFilesSelected = new TranslationString("{0} and {1} other selected file(s) do not have a local revision." + Environment.NewLine + "The files have been deleted locally, but modified remotely" + Environment.NewLine + Environment.NewLine + "Choose to delete the files or keep the modified versions.");
        private readonly TranslationString _fileModifiedLocallyAndDeletedRemotely = new TranslationString("The file has been modified locally ({0}) and deleted remotely ({1}). Choose to delete the file or keep the modified version.");
        private readonly TranslationString _fileModifiedLocallyAndDeletedRemotelyLong = new TranslationString("File {0} does not have a remote revision." + Environment.NewLine + "The file has been modified locally ({1}) but deleted remotely ({2})." + Environment.NewLine + Environment.NewLine + "Choose to delete the file or keep the modified version.");
        private readonly TranslationString _filesModifiedLocallyAndDeletedRemotelyLong = new TranslationString("{0} and {1} other out of {2} selected files do not have a remote revision." + Environment.NewLine + "The files have been modified locally, but deleted remotely." + Environment.NewLine + Environment.NewLine + "Choose to delete the files or keep the modified versions.");
        private readonly TranslationString _filesModifiedLocallyAndDeletedRemotelyLongNoOtherFilesSelected = new TranslationString("{0} and {1} other selected file(s) do not have a remote revision." + Environment.NewLine + "The files have been modified locally, but deleted remotely." + Environment.NewLine + Environment.NewLine + "Choose to delete the files or keep the modified versions.");
        private readonly TranslationString _noBase = new TranslationString("no base");
        private readonly TranslationString _deleted = new TranslationString("deleted");
        private readonly TranslationString _chooseLocalButtonText = new TranslationString("Choose local");
        private readonly TranslationString _chooseRemoteButtonText = new TranslationString("Choose remote");
        private readonly TranslationString _deleteFileButtonText = new TranslationString("Delete file");
        private readonly TranslationString _keepModifiedButtonText = new TranslationString("Keep modified");
        private readonly TranslationString _keepBaseButtonText = new TranslationString("Keep base file");

        private readonly TranslationString _solveMergeConflictApplyToAllCheckBoxText = new TranslationString("Apply to {0} and {1} other file(s)");
        private readonly TranslationString _solveMergeConflictDialogCaption = new TranslationString("Solve merge conflict");

        private readonly TranslationString _conflictedFilesContextMenuText = new TranslationString("Solve");
        private readonly TranslationString _openMergeToolItemText = new TranslationString("Open in");
        private readonly TranslationString _button1Text = new TranslationString("Open in");

        private readonly TranslationString _resetItemRebaseText = new TranslationString("Abort rebase");
        private readonly TranslationString _contextChooseLocalRebaseText = new TranslationString("Choose local (theirs)");
        private readonly TranslationString _contextChooseRemoteRebaseText = new TranslationString("Choose remote (ours)");

        private readonly TranslationString _resetItemMergeText = new TranslationString("Abort merge");
        private readonly TranslationString _contextChooseLocalMergeText = new TranslationString("Choose local (ours)");
        private readonly TranslationString _contextChooseRemoteMergeText = new TranslationString("Choose remote (theirs)");

        private readonly TranslationString _noBaseFileMergeCaption = new TranslationString("Merge");

        private readonly TranslationString _chooseBaseFileFailedText = new TranslationString("Choose base file failed.");
        private readonly TranslationString _chooseLocalFileFailedText = new TranslationString("Choose local file failed.");
        private readonly TranslationString _chooseRemoteFileFailedText = new TranslationString("Choose remote file failed.");

        private readonly TranslationString _currentFormatFilter =
            new TranslationString("Current format (*.{0})");
        private readonly TranslationString _allFilesFilter =
            new TranslationString("All files (*.*)");

        private readonly TranslationString _abortCurrentOperation =
            new TranslationString("You can abort the current operation by resetting changes." + Environment.NewLine +
                "All changes since the last commit will be deleted." + Environment.NewLine +
                Environment.NewLine + "Do you want to reset changes?");

        private readonly TranslationString _abortCurrentOperationCaption = new TranslationString("Abort");

        private readonly TranslationString _areYouSureYouWantDeleteFiles =
            new TranslationString("Are you sure you want to DELETE all changes?" + Environment.NewLine +
                Environment.NewLine + "This action cannot be made undone.");

        private readonly TranslationString _areYouSureYouWantDeleteFilesCaption = new TranslationString("WARNING!");

        private readonly TranslationString _failureWhileOpenFile = new TranslationString("Open temporary file failed.");
        private readonly TranslationString _failureWhileSaveFile = new TranslationString("Save file failed.");
        #endregion

        private enum ConflictResolutionPreference
        {
            None = 0,
            KeepLocal = 1,
            KeepRemote = 2,
            KeepBase = 3
        }

        private readonly IFullPathResolver _fullPathResolver;
        private ConflictResolutionPreference _solveMergeConflictDialogResult;
        private bool _solveMergeConflictApplyToAll;
        private string _solveMergeConflictDialogCheckboxText;
        private ConflictResolutionPreference _solveMergeConflictLastAction;
        private int _filesDeletedLocallyAndModifiedRemotelyCount;
        private int _filesModifiedLocallyAndDeletedRemotelyCount;
        private int _filesRemainedCount;
        private int _filesDeletedLocallyAndModifiedRemotelySolved;
        private int _filesModifiedLocallyAndDeletedRemotelySolved;
        private int _filesRemainedCountSolved;
        private int _conflictItemsCount;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormResolveConflicts()
        {
            InitializeComponent();
        }

        public FormResolveConflicts(GitUICommands commands, bool offerCommit = true)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
            _offerCommit = offerCommit;
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);

            FileName.DataPropertyName = nameof(ConflictData.Filename);
            authorDataGridViewTextBoxColumn1.DataPropertyName = "Author"; // TODO this property does not exist on the target type
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);
            _thereWhereMergeConflicts = Module.InTheMiddleOfConflictedMerge();
            merge.Focus();
            merge.Select();

            HotkeysEnabled = true;
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                Directory.SetCurrentDirectory(Module.WorkingDir);
                Module.RunMergeTool();
                Initialize();
            }
        }

        private readonly bool _offerCommit;
        private bool _thereWhereMergeConflicts;

        private void FormResolveConflicts_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            using (WaitCursorScope.Enter())
            {
                int oldSelectedRow = 0;
                bool isLastRow = false;
                if (ConflictedFiles.SelectedRows.Count > 0)
                {
                    oldSelectedRow = ConflictedFiles.SelectedRows[0].Index;
                    isLastRow = ConflictedFiles.Rows.Count - 1 == oldSelectedRow;
                }

                ConflictedFiles.DataSource = ThreadHelper.JoinableTaskFactory.Run(() => Module.GetConflictsAsync());
                ConflictedFiles.Columns[0].DataPropertyName = nameof(ConflictData.Filename);

                // if the last row was previously selected, select the last row again
                if (isLastRow && oldSelectedRow >= ConflictedFiles.Rows.Count)
                {
                    oldSelectedRow = Math.Max(0, ConflictedFiles.Rows.Count - 1);
                }

                if (ConflictedFiles.Rows.Count > oldSelectedRow)
                {
                    // as part of the databinding event, the fist row is selected automatically
                    // if previously another row was selected, we need to reset the selection,
                    // and select the desired row
                    if (oldSelectedRow > 0)
                    {
                        if (ConflictedFiles.SelectedRows.Count > 0)
                        {
                            ConflictedFiles.SelectionChanged -= ConflictedFiles_SelectionChanged;
                            ConflictedFiles.SelectedRows[0].Selected = false;
                            ConflictedFiles.SelectionChanged += ConflictedFiles_SelectionChanged;
                        }

                        // select the desired row
                        ConflictedFiles.Rows[oldSelectedRow].Selected = true;
                    }

                    if (oldSelectedRow < ConflictedFiles.FirstDisplayedScrollingRowIndex ||
                        oldSelectedRow > (ConflictedFiles.FirstDisplayedScrollingRowIndex + ConflictedFiles.DisplayedRowCount(false)))
                    {
                        try
                        {
                            ConflictedFiles.FirstDisplayedScrollingRowIndex = oldSelectedRow;
                        }
                        catch (InvalidOperationException)
                        {
                            // ignore the exception - setting the row index is not so important to crash the app
                            // see the #2975 issues for details
                        }
                    }
                }

                if (!InitMergetool())
                {
                    Close();
                    return;
                }

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
                        if (AppSettings.DontConfirmCommitAfterConflictsResolved ||
                            MessageBox.Show(this, _allConflictsResolved.Text, _allConflictsResolvedCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            UICommands.StartCommitDialog(this);
                        }
                    }

                    Close();
                }
            }
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

        private IReadOnlyList<ConflictData> GetConflicts()
        {
            return ConflictedFiles.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(selectedRow => (ConflictData)selectedRow.DataBoundItem)
                .ToArray();
        }

        private string GetFileName()
        {
            return GetConflict().Filename;
        }

        private static string FixPath(string path)
        {
            return (path ?? "").ToNativePath();
        }

        private readonly Dictionary<string, string> _mergeScripts = new Dictionary<string, string>
        {
            { ".doc", "merge-doc.js" },
            { ".docx", "merge-doc.js" },
            { ".docm", "merge-doc.js" },
            { ".ods", "merge-ods.vbs" },
            { ".odt", "merge-ods.vbs" },
            { ".sxw", "merge-ods.vbs" },
        };

        private bool TryMergeWithScript(string fileName, string baseFileName, string localFileName, string remoteFileName)
        {
            if (!EnvUtils.RunningOnWindows())
            {
                return false;
            }

            try
            {
                string extension = Path.GetExtension(fileName).ToLower();
                if (extension.Length <= 1)
                {
                    return false;
                }

                string dir = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Diff-Scripts").EnsureTrailingPathSeparator();
                if (Directory.Exists(dir))
                {
                    if (_mergeScripts.TryGetValue(extension, out var mergeScript) &&
                        File.Exists(Path.Combine(dir, mergeScript)))
                    {
                        if (MessageBox.Show(this, string.Format(_uskUseCustomMergeScript.Text, mergeScript),
                                            _uskUseCustomMergeScriptCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                            DialogResult.Yes)
                        {
                            UseMergeWithScript(fileName, Path.Combine(dir, mergeScript), baseFileName, localFileName, remoteFileName);

                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Merge using script failed.\n" + ex, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        private void UseMergeWithScript(string fileName, string mergeScript, string baseFileName, string localFileName, string remoteFileName)
        {
            // get timestamp of file before merge. This is an extra check to verify if merge was successfully
            DateTime lastWriteTimeBeforeMerge = DateTime.Now;
            if (File.Exists(Path.Combine(Module.WorkingDir, fileName)))
            {
                lastWriteTimeBeforeMerge = File.GetLastWriteTime(_fullPathResolver.Resolve(fileName));
            }

            var args = new ArgumentBuilder
            {
                mergeScript.Quote(),
                FixPath(_fullPathResolver.Resolve(fileName)).Quote(),
                FixPath(remoteFileName).Quote(),
                FixPath(localFileName).Quote(),
                FixPath(baseFileName).Quote()
            };

            new Executable("wscript", Module.WorkingDir).Start(args);

            if (MessageBox.Show(this, string.Format(_askMergeConflictSolvedAfterCustomMergeScript.Text,
                FixPath(_fullPathResolver.Resolve(fileName))), _askMergeConflictSolvedCaption.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DateTime lastWriteTimeAfterMerge = lastWriteTimeBeforeMerge;
                if (File.Exists(_fullPathResolver.Resolve(fileName)))
                {
                    lastWriteTimeAfterMerge = File.GetLastWriteTime(_fullPathResolver.Resolve(fileName));
                }

                // The file is not modified, do not stage file and present warning
                if (lastWriteTimeBeforeMerge == lastWriteTimeAfterMerge)
                {
                    MessageBox.Show(this, _fileUnchangedAfterMerge.Text, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    StageFile(fileName);
                }
            }

            Initialize();
            if (File.Exists(baseFileName))
            {
                File.Delete(baseFileName);
            }

            if (File.Exists(remoteFileName))
            {
                File.Delete(remoteFileName);
            }

            if (File.Exists(localFileName))
            {
                File.Delete(localFileName);
            }
        }

        private void Use2WayMerge(ref string arguments)
        {
            string mergeToolLower = _mergetool.ToLowerInvariant();
            switch (mergeToolLower)
            {
                case "kdiff3":
                case "diffmerge":
                    arguments = arguments.Replace("\"$BASE\"", "");
                    break;
                case "tortoisemerge":
                    arguments = arguments.Replace("-base:\"$BASE\"", "").Replace("/base:\"$BASE\"", "");
                    arguments = arguments.Replace("mine:\"$LOCAL\"", "base:\"$LOCAL\"");
                    break;
            }
        }

        private enum ItemType
        {
            File,
            Directory,
            Submodule
        }

        private ItemType GetItemType(string filename)
        {
            string fullname = _fullPathResolver.Resolve(filename);
            if (Directory.Exists(fullname) && !File.Exists(fullname))
            {
                if (Module.IsSubmodule(filename.Trim()))
                {
                    return ItemType.Submodule;
                }

                return ItemType.Directory;
            }

            return ItemType.File;
        }

        private void ConflictedFiles_DoubleClick(object sender, EventArgs e)
        {
            OpenMergeTool();
        }

        private void StopAndHideProgressBar()
        {
            progressBar.Visible = false;
        }

        private void IncrementProgressBarValue()
        {
            progressBar.Value++;
        }

        private void StartProgressBarWithMaxValue(int maximum)
        {
            progressBar.Minimum = 0;
            progressBar.Maximum = maximum;
            progressBar.Value = 0;
            progressBar.Visible = true;
        }

        private void ResolveItemConflict(ConflictData item)
        {
            var itemType = GetItemType(item.Filename);
            if (itemType == ItemType.Submodule)
            {
                var form = new FormMergeSubmodule(UICommands, item.Filename);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    StageFile(item.Filename);
                }
            }
            else if (itemType == ItemType.File)
            {
                ResolveFilesConflict(item);
            }
        }

        private void ResolveFilesConflict(ConflictData item)
        {
            var (baseFile, localFile, remoteFile) = Module.CheckoutConflictedFiles(item);

            try
            {
                if (CheckForLocalRevision(item) &&
                    CheckForRemoteRevision(item))
                {
                    if (TryMergeWithScript(item.Filename, baseFile, localFile, remoteFile))
                    {
                        Cursor.Current = Cursors.Default;
                        return;
                    }

                    if (FileHelper.IsBinaryFileName(Module, item.Local.Filename))
                    {
                        if (MessageBox.Show(this, string.Format(_fileIsBinary.Text, _mergetool),
                                Strings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                                MessageBoxDefaultButton.Button2) == DialogResult.No)
                        {
                            BinaryFilesChooseLocalBaseRemote(item);
                            return;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(_mergetoolCmd) || string.IsNullOrWhiteSpace(_mergetoolPath))
                    {
                        // mergetool is set, but arguments cannot be manipulated
                        Module.RunMergeTool(item.Filename);

                        // git-mergetool does not provide exit status, do not stage
                        return;
                    }

                    string arguments = _mergetoolCmd;

                    // Check if there is a base file. If not, ask user to fall back to 2-way merge.
                    // git doesn't support 2-way merge, but we can try to adjust attributes to fix this.
                    // For kdiff3 this is easy; just remove the 3rd file from the arguments. Since the
                    // filenames are quoted, this takes a little extra effort. We need to remove these
                    // quotes also. For other tools a little bit more magic is needed.
                    if (item.Base.Filename == null)
                    {
                        var text = string.Format(_noBaseRevision.Text, item.Filename);
                        DialogResult result = MessageBox.Show(this, text, _noBaseFileMergeCaption.Text,
                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            Use2WayMerge(ref arguments);
                        }

                        if (result == DialogResult.Cancel)
                        {
                            return;
                        }
                    }

                    arguments = arguments.Replace("$BASE", baseFile);
                    arguments = arguments.Replace("$LOCAL", localFile);
                    arguments = arguments.Replace("$REMOTE", remoteFile);
                    arguments = arguments.Replace("$MERGED", item.Filename);

                    // get timestamp of file before merge. This is an extra check to verify if merge was successful
                    DateTime lastWriteTimeBeforeMerge = DateTime.Now;
                    if (File.Exists(_fullPathResolver.Resolve(item.Filename)))
                    {
                        lastWriteTimeBeforeMerge = File.GetLastWriteTime(_fullPathResolver.Resolve(item.Filename));
                    }

                    GitUIPluginInterfaces.ExecutionResult res;
                    try
                    {
                        res = new Executable(_mergetoolPath, Module.WorkingDir).Execute(arguments);
                    }
                    catch (Exception)
                    {
                        var text = string.Format(_errorStartingMergetool.Text, _mergetoolPath);
                        MessageBox.Show(this, text, _noBaseFileMergeCaption.Text,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    DateTime lastWriteTimeAfterMerge = lastWriteTimeBeforeMerge;
                    if (File.Exists(_fullPathResolver.Resolve(item.Filename)))
                    {
                        lastWriteTimeAfterMerge = File.GetLastWriteTime(_fullPathResolver.Resolve(item.Filename));
                    }

                    // Check exitcode AND timestamp of the file. If exitcode is success and
                    // time timestamp is changed, we are pretty sure the merge was done.
                    if (res.ExitCode == 0 && lastWriteTimeBeforeMerge != lastWriteTimeAfterMerge)
                    {
                        StageFile(item.Filename);
                    }

                    // If the exitcode is 1, but the file is changed, ask if the merge conflict is solved.
                    // If the exitcode is 0, but the file is not changed, ask if the merge conflict is solved.
                    if ((res.ExitCode == 1 && lastWriteTimeBeforeMerge != lastWriteTimeAfterMerge) ||
                        (res.ExitCode == 0 && lastWriteTimeBeforeMerge == lastWriteTimeAfterMerge))
                    {
                        if (MessageBox.Show(this, _askMergeConflictSolved.Text, _askMergeConflictSolvedCaption.Text,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            StageFile(item.Filename);
                        }
                    }
                }
            }
            finally
            {
                DeleteTemporaryFile(baseFile);
                DeleteTemporaryFile(localFile);
                DeleteTemporaryFile(remoteFile);
            }

            return;

            void DeleteTemporaryFile(string path)
            {
                if (path != null && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        private bool InitMergetool()
        {
            if (GitVersion.Current.SupportGuiMergeTool)
            {
                _mergetool = Module.GetEffectiveSetting(SettingKeyString.MergeToolKey);
            }

            // Fallback and older Git
            if (string.IsNullOrEmpty(_mergetool))
            {
                _mergetool = Module.GetEffectiveSetting(SettingKeyString.MergeToolNoGuiKey);
            }

            if (string.IsNullOrEmpty(_mergetool))
            {
                MessageBox.Show(this, _noMergeTool.Text, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            using (WaitCursorScope.Enter())
            {
                _mergetoolCmd = Module.GetEffectiveSetting($"mergetool.{_mergetool}.cmd");
                _mergetoolPath = Module.GetEffectiveSetting($"mergetool.{_mergetool}.path");

                // Temporary compatibility with GE <3.3
                if (_mergetool == "kdiff3")
                {
                    if (string.IsNullOrEmpty(_mergetoolPath))
                    {
                        _mergetoolPath = "kdiff3";
                    }

                    if (string.IsNullOrEmpty(_mergetoolCmd))
                    {
                        _mergetoolCmd = "\"$BASE\" \"$LOCAL\" \"$REMOTE\" -o \"$MERGED\"";
                    }
                }

                if (EnvUtils.RunningOnWindows())
                {
                    // This only works when on Windows....
                    const string executablePattern = ".exe";
                    int idx = _mergetoolCmd.IndexOf(executablePattern);
                    if (idx >= 0)
                    {
                        _mergetoolPath = _mergetoolCmd.Substring(0, idx + executablePattern.Length + 1).Trim('\"', ' ');
                        _mergetoolCmd = _mergetoolCmd.Substring(idx + executablePattern.Length + 1);
                    }
                }

                if (!PathUtil.TryFindFullPath(_mergetoolPath, out string fullPath))
                {
                    MessageBox.Show(this, _noMergeToolConfigured.Text, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                _mergetoolPath = fullPath;
            }

            return true;
        }

        private bool ShowAbortMessage()
        {
            if (MessageBox.Show(_abortCurrentOperation.Text, _abortCurrentOperationCaption.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (AppSettings.DontConfirmSecondAbortConfirmation ||
                    MessageBox.Show(_areYouSureYouWantDeleteFiles.Text, _areYouSureYouWantDeleteFilesCaption.Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    return true;
                }
            }

            return false;
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                if (ShowAbortMessage())
                {
                    Module.Reset(ResetMode.Hard);
                    Close();
                }
            }
        }

        private string GetRemoteSideString()
        {
            bool inTheMiddleOfRebase = Module.InTheMiddleOfRebase();
            return inTheMiddleOfRebase ? _ours.Text : _theirs.Text;
        }

        private string GetLocalSideString()
        {
            bool inTheMiddleOfRebase = Module.InTheMiddleOfRebase();
            return inTheMiddleOfRebase ? _theirs.Text : _ours.Text;
        }

        private string GetShortHash(ConflictedFileData item)
        {
            if (item.ObjectId == null)
            {
                return "@" + _deleted.Text;
            }

            return '@' + item.ObjectId.ToShortString();
        }

        private void ConflictedFiles_SelectionChanged(object sender, EventArgs e)
        {
            // NOTE we cannot use WaitCursorScope here as there is no lexical scope for the operation
            Cursor.Current = Cursors.WaitCursor;
            baseFileName.Text = localFileName.Text = remoteFileName.Text = "";
            if (HasMultipleRowsSelected())
            {
                HandleMultipleSelect();
            }
            else if (HasOneRowSelected())
            {
                HandleSingleSelect();
            }
        }

        private bool HasOneRowSelected()
        {
            return ConflictedFiles.SelectedRows.Count == 1;
        }

        private bool HasMultipleRowsSelected()
        {
            return ConflictedFiles.SelectedRows.Count > 1;
        }

        private void HandleMultipleSelect()
        {
            SetAvailableCommands(false);
        }

        private void SetAvailableCommands(bool enabled)
        {
            OpenMergetool.Enabled = enabled;
            openMergeToolBtn.Enabled = enabled;
            ContextOpenLocalWith.Enabled = enabled;
            ContextOpenRemoteWith.Enabled = enabled;
            ContextOpenBaseWith.Enabled = enabled;
            ContextSaveLocalAs.Enabled = enabled;
            ContextSaveRemoteAs.Enabled = enabled;
            ContextSaveBaseAs.Enabled = enabled;
            openToolStripMenuItem.Enabled = enabled;
            openWithToolStripMenuItem.Enabled = enabled;
            fileHistoryToolStripMenuItem.Enabled = enabled;
        }

        private void HandleSingleSelect()
        {
            SetAvailableCommands(true);

            var item = GetConflict();

            bool baseFileExists = !string.IsNullOrEmpty(item.Base.Filename);
            bool localFileExists = !string.IsNullOrEmpty(item.Local.Filename);
            bool remoteFileExists = !string.IsNullOrEmpty(item.Remote.Filename);

            string remoteSide = GetRemoteSideString();
            string localSide = GetLocalSideString();

            if (baseFileExists && localFileExists && remoteFileExists)
            {
                conflictDescription.Text = string.Format(_fileChangeLocallyAndRemotely.Text, localSide, remoteSide);
            }

            if (!baseFileExists && localFileExists && remoteFileExists)
            {
                conflictDescription.Text = string.Format(_fileCreatedLocallyAndRemotely.Text, localSide, remoteSide);
            }

            if (baseFileExists && !localFileExists && remoteFileExists)
            {
                conflictDescription.Text = string.Format(_fileDeletedLocallyAndModifiedRemotely.Text, localSide, remoteSide);
            }

            if (baseFileExists && localFileExists && !remoteFileExists)
            {
                conflictDescription.Text = string.Format(_fileModifiedLocallyAndDeletedRemotely.Text, localSide, remoteSide);
            }

            baseFileName.Text = baseFileExists ? item.Base.Filename : _noBase.Text;
            localFileName.Text = localFileExists ? item.Local.Filename : _deleted.Text;
            remoteFileName.Text = remoteFileExists ? item.Remote.Filename : _deleted.Text;

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
            using (WaitCursorScope.Enter())
            {
                var conflictItems = GetConflicts();

                StartProgressBarWithMaxValue(conflictItems.Count);
                foreach (var conflictItem in conflictItems)
                {
                    if (CheckForBaseRevision(conflictItem))
                    {
                        ChooseBaseOnConflict(conflictItem.Base.Filename);
                    }

                    IncrementProgressBarValue();
                }

                StopAndHideProgressBar();
                Initialize();
            }
        }

        private void ChooseBaseOnConflict(string fileName)
        {
            if (!Module.HandleConflictSelectSide(fileName, "BASE"))
            {
                MessageBox.Show(this, _chooseBaseFileFailedText.Text, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ContextChooseLocal_Click(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                var conflictItems = GetConflicts();
                StartProgressBarWithMaxValue(conflictItems.Count);
                foreach (var conflictItem in conflictItems)
                {
                    if (CheckForLocalRevision(conflictItem))
                    {
                        ChooseLocalOnConflict(conflictItem.Filename);
                    }

                    IncrementProgressBarValue();
                }

                StopAndHideProgressBar();
                Initialize();
            }
        }

        private void ChooseLocalOnConflict(string fileName)
        {
            if (!Module.HandleConflictSelectSide(fileName, "LOCAL"))
            {
                MessageBox.Show(this, _chooseLocalFileFailedText.Text, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ContextChooseRemote_Click(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                var conflictItems = GetConflicts();
                StartProgressBarWithMaxValue(conflictItems.Count);
                foreach (var conflictItem in conflictItems)
                {
                    if (CheckForRemoteRevision(conflictItem))
                    {
                        ChooseRemoteOnConflict(conflictItem.Filename);
                    }

                    IncrementProgressBarValue();
                }

                StopAndHideProgressBar();
                Initialize();
            }
        }

        private void ChooseRemoteOnConflict(string fileName)
        {
            if (!Module.HandleConflictSelectSide(fileName, "REMOTE"))
            {
                MessageBox.Show(this, _chooseRemoteFileFailedText.Text, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private TaskDialog CreateSolveMergeConflictTaskDialog(IntPtr handle, string text, string instructionText, string caption, string applyToAllCheckBoxText,
            string keepLocalButtonText, string keepRemoteButtonText, string keepBaseButtonText)
        {
            TaskDialog dialog = new TaskDialog
            {
                OwnerWindowHandle = handle,
                Text = text,
                InstructionText = instructionText,
                Caption = caption,
                StandardButtons = TaskDialogStandardButtons.Cancel,
                Icon = TaskDialogStandardIcon.Error,
                FooterCheckBoxText = applyToAllCheckBoxText,
                FooterIcon = TaskDialogStandardIcon.Information,
                StartupLocation = TaskDialogStartupLocation.CenterOwner,
                Cancelable = true
            };

            // Local
            var btnKeepLocal = new TaskDialogCommandLink("KeepLocal", null, keepLocalButtonText);
            btnKeepLocal.Click += (s, e) =>
            {
                _solveMergeConflictDialogResult = ConflictResolutionPreference.KeepLocal;
                dialog.Close();
            };

            // Remote
            var btnKeepRemote = new TaskDialogCommandLink("KeepRemote", null, keepRemoteButtonText);
            btnKeepRemote.Click += (s, e) =>
            {
                _solveMergeConflictDialogResult = ConflictResolutionPreference.KeepRemote;
                dialog.Close();
            };

            // Base
            var btnKeepBase = new TaskDialogCommandLink("KeepBase", null, keepBaseButtonText);
            btnKeepBase.Click += (s, e) =>
            {
                _solveMergeConflictDialogResult = ConflictResolutionPreference.KeepBase;
                dialog.Close();
            };

            dialog.Controls.Add(btnKeepLocal);
            dialog.Controls.Add(btnKeepRemote);
            dialog.Controls.Add(btnKeepBase);

            return dialog;
        }

        private void OpenSolveMergeConflictDialogAndExecuteSelectedMergeAction(Action<ConflictResolutionPreference> selectedMergeAction,
            string dialogText, string dialogInstructionText, string dialogCaption, string dialogFooterCheckboxText,
            string keepLocalButtonText, string keepRemoteButtonText, string keepBaseButtonText)
        {
            if (!_solveMergeConflictApplyToAll)
            {
                using var dialog = CreateSolveMergeConflictTaskDialog(Handle, dialogText, dialogInstructionText, dialogCaption, dialogFooterCheckboxText,
                    keepLocalButtonText, keepRemoteButtonText, keepBaseButtonText);

                dialog.Show();
                _solveMergeConflictApplyToAll = dialog.FooterCheckBoxChecked ?? false;
                _solveMergeConflictLastAction = _solveMergeConflictDialogResult; // The value of _solveMergeConflictDialogResult is changed in the method CreateSolveMergeConflictTaskDialog(...)
            }

            selectedMergeAction(_solveMergeConflictDialogResult);
        }

        private void BinaryFilesChooseLocalBaseRemote(ConflictData item)
        {
            // solveMergeConflictDialogResult gets a value inside of the method "OpenSolveMergeConflictDialogAndExecuteSelectedMergeAction"
            OpenSolveMergeConflictDialogAndExecuteSelectedMergeAction((solveMergeConflictDialogResult) =>
                {
                    switch (solveMergeConflictDialogResult)
                    {
                        case ConflictResolutionPreference.KeepLocal:
                            // local
                            ChooseLocalOnConflict(item.Filename);
                            break;
                        case ConflictResolutionPreference.KeepRemote:
                            // remote
                            ChooseRemoteOnConflict(item.Filename);
                            break;
                        case ConflictResolutionPreference.KeepBase:
                            // base
                            ChooseBaseOnConflict(item.Filename);
                            break;
                        default:
                            break;
                    }
                },
                string.Format(_fileBinaryChooseLocalBaseRemote.Text, item.Local.Filename, GetLocalSideString(), GetRemoteSideString()),
                string.Empty,
                _solveMergeConflictDialogCaption.Text,
                _solveMergeConflictDialogCheckboxText,
                string.Format(_chooseLocalButtonText.Text + " ({0})", GetLocalSideString()),
                string.Format(_chooseRemoteButtonText.Text + " ({0})", GetRemoteSideString()),
                _keepBaseButtonText.Text);
        }

        private bool CheckForBaseRevision(ConflictData item)
        {
            if (!string.IsNullOrEmpty(item.Base.Filename))
            {
                return true;
            }

            // solveMergeConflictDialogResult gets a value inside of the method "OpenSolveMergeConflictDialogAndExecuteSelectedMergeAction"
            OpenSolveMergeConflictDialogAndExecuteSelectedMergeAction((solveMergeConflictDialogResult) =>
                {
                    switch (solveMergeConflictDialogResult)
                    {
                        case ConflictResolutionPreference.KeepLocal:
                            // local
                            ChooseLocalOnConflict(item.Filename);
                            break;
                        case ConflictResolutionPreference.KeepRemote:
                            // remote
                            ChooseRemoteOnConflict(item.Filename);
                            break;
                        case ConflictResolutionPreference.KeepBase:
                            // delete
                            var args = new GitArgumentBuilder("rm")
                            {
                                "--",
                                item.Filename.QuoteNE()
                            };
                            Module.GitExecutable.GetOutput(args);
                            break;
                        default:
                            break;
                    }
                },
                string.Format(_fileCreatedLocallyAndRemotelyLong.Text, item.Filename, GetLocalSideString(), GetRemoteSideString()),
                string.Empty,
                _solveMergeConflictDialogCaption.Text,
                _solveMergeConflictDialogCheckboxText,
                string.Format(_chooseLocalButtonText.Text + " ({0})", GetLocalSideString()),
                string.Format(_chooseRemoteButtonText.Text + " ({0})", GetRemoteSideString()),
                _deleteFileButtonText.Text);

            return false;
        }

        private bool CheckForLocalRevision(ConflictData item)
        {
            if (!string.IsNullOrEmpty(item.Local.Filename))
            {
                return true;
            }

            string dialogText = "";
            if (_solveMergeConflictApplyToAll == false && _filesDeletedLocallyAndModifiedRemotelySolved == 1)
            {
                dialogText = string.Format(_fileDeletedLocallyAndModifiedRemotelyLong.Text,
                        item.Filename,
                        GetLocalSideString(),
                        GetRemoteSideString());
            }

            if (_solveMergeConflictApplyToAll == false && _filesDeletedLocallyAndModifiedRemotelySolved > 1)
            {
                if (_filesModifiedLocallyAndDeletedRemotelyCount == 0 && _filesRemainedCount == 0)
                {
                    // Task Dialog shows the name of a current file, so the amount of the left files should be minus one. That's why _filesModifiedLocallyAndDeletedRemotelySolved - 1
                    dialogText = string.Format(_filesDeletedLocallyAndModifiedRemotelyLongNoOtherFilesSelected.Text, item.Filename, _filesDeletedLocallyAndModifiedRemotelySolved - 1);
                }
                else
                {
                    dialogText = string.Format(_filesDeletedLocallyAndModifiedRemotelyLong.Text, item.Filename, _filesDeletedLocallyAndModifiedRemotelySolved - 1, _conflictItemsCount);
                }
            }

            // solveMergeConflictDialogResult gets a value inside of the method "OpenSolveMergeConflictDialogAndExecuteSelectedMergeAction"
            OpenSolveMergeConflictDialogAndExecuteSelectedMergeAction((solveMergeConflictDialogResult) =>
                {
                    switch (solveMergeConflictDialogResult)
                    {
                        case ConflictResolutionPreference.KeepLocal:
                            // delete
                            var args = new GitArgumentBuilder("rm")
                            {
                                "--",
                                item.Filename.QuoteNE()
                            };
                            Module.GitExecutable.GetOutput(args);
                            break;
                        case ConflictResolutionPreference.KeepRemote:
                            // remote
                            ChooseRemoteOnConflict(item.Filename);
                            break;
                        case ConflictResolutionPreference.KeepBase:
                            // base
                            ChooseBaseOnConflict(item.Filename);
                            break;
                        default:
                            break;
                    }

                    _filesDeletedLocallyAndModifiedRemotelySolved--;
                },
                dialogText,
                string.Empty,
                _solveMergeConflictDialogCaption.Text,
                (_filesDeletedLocallyAndModifiedRemotelySolved > 1) ? string.Format(_solveMergeConflictApplyToAllCheckBoxText.Text, item.Filename, _filesDeletedLocallyAndModifiedRemotelySolved - 1) : string.Empty,
                string.Format(_deleteFileButtonText.Text + " ({0})", GetLocalSideString()),
                string.Format(_keepModifiedButtonText.Text + " ({0})", GetRemoteSideString()),
                string.Format(_keepBaseButtonText.Text + " ({0})", GetLocalSideString()));

            return false;
        }

        private bool CheckForRemoteRevision(ConflictData item)
        {
            if (!string.IsNullOrEmpty(item.Remote.Filename))
            {
                return true;
            }

            string dialogText = "";
            if (_solveMergeConflictApplyToAll == false && _filesModifiedLocallyAndDeletedRemotelySolved == 1)
            {
                dialogText = string.Format(_fileModifiedLocallyAndDeletedRemotelyLong.Text,
                    item.Filename,
                    GetLocalSideString(),
                    GetRemoteSideString());
            }

            if (_solveMergeConflictApplyToAll == false && _filesModifiedLocallyAndDeletedRemotelySolved > 1)
            {
                if (_filesDeletedLocallyAndModifiedRemotelyCount == 0 && _filesRemainedCount == 0)
                {
                    // Task Dialog shows the name of a current file, so the amount of the left files should be minus one. That's why _filesModifiedLocallyAndDeletedRemotelySolved - 1
                    dialogText = string.Format(_filesModifiedLocallyAndDeletedRemotelyLongNoOtherFilesSelected.Text, item.Filename, _filesModifiedLocallyAndDeletedRemotelySolved - 1);
                }
                else
                {
                    dialogText = string.Format(_filesModifiedLocallyAndDeletedRemotelyLong.Text, item.Filename, _filesModifiedLocallyAndDeletedRemotelySolved - 1, _conflictItemsCount);
                }
            }

            // solveMergeConflictDialogResult gets a value inside of the method "OpenSolveMergeConflictDialogAndExecuteSelectedMergeAction"
            OpenSolveMergeConflictDialogAndExecuteSelectedMergeAction((solveMergeConflictDialogResult) =>
                {
                    switch (solveMergeConflictDialogResult)
                    {
                        case ConflictResolutionPreference.KeepLocal:
                            // delete
                            ChooseLocalOnConflict(item.Filename);
                            break;
                        case ConflictResolutionPreference.KeepRemote:
                            // remote
                            var args = new GitArgumentBuilder("rm")
                            {
                                "--",
                                item.Filename.QuoteNE()
                            };
                            Module.GitExecutable.GetOutput(args);
                            break;
                        case ConflictResolutionPreference.KeepBase:
                            // base
                            ChooseBaseOnConflict(item.Filename);
                            break;
                        default:
                            break;
                    }

                    _filesModifiedLocallyAndDeletedRemotelySolved--;
                },
                dialogText,
                string.Empty,
                _solveMergeConflictDialogCaption.Text,
                (_filesModifiedLocallyAndDeletedRemotelySolved > 1) ? string.Format(_solveMergeConflictApplyToAllCheckBoxText.Text, item.Filename, _filesModifiedLocallyAndDeletedRemotelySolved - 1) : string.Empty,
                string.Format(_keepModifiedButtonText.Text + " ({0})", GetLocalSideString()),
                string.Format(_deleteFileButtonText.Text + " ({0})", GetRemoteSideString()),
                _keepBaseButtonText.Text);

            return false;
        }

        private void OpenMergeTool()
        {
            using (WaitCursorScope.Enter())
            {
                try
                {
                    var items = GetConflicts();
                    _conflictItemsCount = items.Count;

                    List<ConflictData> filesDeletedLocallyAndModifiedRemotely = new List<ConflictData>();
                    List<ConflictData> filesModifiedLocallyAndDeletedRemotely = new List<ConflictData>();
                    List<ConflictData> filesRemaining = new List<ConflictData>();

                    // Insert(0, conflictData) is needed the task dialog shows the same order of files as selected in the grid
                    foreach (var conflictData in items)
                    {
                        if (string.IsNullOrEmpty(conflictData.Local.Filename) && !string.IsNullOrEmpty(conflictData.Remote.Filename))
                        {
                            filesDeletedLocallyAndModifiedRemotely.Insert(0, conflictData);
                        }
                        else if (!string.IsNullOrEmpty(conflictData.Local.Filename) && string.IsNullOrEmpty(conflictData.Remote.Filename))
                        {
                            filesModifiedLocallyAndDeletedRemotely.Insert(0, conflictData);
                        }
                        else
                        {
                            filesRemaining.Insert(0, conflictData);
                        }
                    }

                    _filesDeletedLocallyAndModifiedRemotelyCount = filesDeletedLocallyAndModifiedRemotely.Count;
                    _filesModifiedLocallyAndDeletedRemotelyCount = filesModifiedLocallyAndDeletedRemotely.Count;
                    _filesRemainedCount = filesRemaining.Count;
                    _filesDeletedLocallyAndModifiedRemotelySolved = _filesDeletedLocallyAndModifiedRemotelyCount;
                    _filesModifiedLocallyAndDeletedRemotelySolved = _filesModifiedLocallyAndDeletedRemotelyCount;
                    _filesRemainedCountSolved = _filesRemainedCount;

                    StartProgressBarWithMaxValue(_conflictItemsCount);

                    _solveMergeConflictApplyToAll = false;
                    _solveMergeConflictLastAction = ConflictResolutionPreference.None;
                    foreach (var conflictData in filesDeletedLocallyAndModifiedRemotely)
                    {
                        IncrementProgressBarValue();
                        ResolveItemConflict(conflictData);
                    }

                    _solveMergeConflictApplyToAll = false;
                    _solveMergeConflictLastAction = ConflictResolutionPreference.None;
                    foreach (var conflictData in filesModifiedLocallyAndDeletedRemotely)
                    {
                        IncrementProgressBarValue();
                        ResolveItemConflict(conflictData);
                    }

                    _solveMergeConflictDialogCheckboxText = string.Empty; // Hide checkbox "apply to all"
                    _solveMergeConflictApplyToAll = false;
                    _solveMergeConflictLastAction = ConflictResolutionPreference.None;
                    foreach (var conflictData in filesRemaining)
                    {
                        IncrementProgressBarValue();
                        ResolveItemConflict(conflictData);
                    }
                }
                finally
                {
                    _solveMergeConflictApplyToAll = false;
                    _solveMergeConflictLastAction = ConflictResolutionPreference.None;
                    StopAndHideProgressBar();
                    Initialize();
                }
            }
        }

        private void OpenMergetool_Click(object sender, EventArgs e)
        {
            OpenMergeTool();
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
            using (WaitCursorScope.Enter())
            {
                var conflictData = GetConflict();
                string fileName = conflictData.Filename;
                fileName = PathUtil.GetFileName(fileName);

                fileName = Path.GetTempPath() + fileName;

                if (!Module.HandleConflictsSaveSide(conflictData.Filename, fileName, side))
                {
                    MessageBox.Show(this, _failureWhileOpenFile.Text, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                OsShellUtil.OpenAs(fileName);
            }
        }

        private void ContextOpenRemoteWith_Click(object sender, EventArgs e)
        {
            OpenSideWith("REMOTE");
        }

        private void ConflictedFiles_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (HasMultipleRowsSelected())
                {
                    // do nothing, choices are limited commands already
                    return;
                }

                int lastRow = e.RowIndex;
                ConflictedFiles.ClearSelection();
                if (lastRow >= 0 && ConflictedFiles.Rows.Count > lastRow)
                {
                    ConflictedFiles.Rows[lastRow].Selected = true;
                }

                SetAvailableCommands(true);
            }
        }

        private void SaveAs(string side)
        {
            using (WaitCursorScope.Enter())
            {
                var conflictData = GetConflict();
                string fileName = conflictData.Filename;
                fileName = PathUtil.GetFileName(fileName);

                using (var fileDialog = new SaveFileDialog
                {
                    FileName = fileName,
                    InitialDirectory = _fullPathResolver.Resolve(Path.GetDirectoryName(conflictData.Filename)),
                    AddExtension = true
                })
                {
                    var ext = Path.GetExtension(fileDialog.FileName);
                    fileDialog.DefaultExt = ext;
                    fileDialog.Filter = string.Format(_currentFormatFilter.Text, ext) + "|*." + ext + "|" + _allFilesFilter.Text + "|*.*";

                    if (fileDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        if (!Module.HandleConflictsSaveSide(conflictData.Filename, fileDialog.FileName, side))
                        {
                            MessageBox.Show(this, _failureWhileSaveFile.Text, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
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
            using (WaitCursorScope.Enter())
            {
                StageFile(GetFileName());
                Initialize();
            }
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
                if (HasMultipleRowsSelected())
                {
                    SetAvailableCommands(false);
                }
                else
                {
                    SetAvailableCommands(true);
                    DisableInvalidEntriesInConflictedFilesContextMenu(fileName);
                }
            }
        }

        private void DisableInvalidEntriesInConflictedFilesContextMenu(string fileName)
        {
            var conflictedFileNames = ThreadHelper.JoinableTaskFactory.Run(() =>
                Module.GetConflictAsync(fileName));
            if (string.IsNullOrEmpty(conflictedFileNames.Local.Filename))
            {
                ContextSaveLocalAs.Enabled = false;
                ContextOpenLocalWith.Enabled = false;
            }

            if (string.IsNullOrEmpty(conflictedFileNames.Remote.Filename))
            {
                ContextSaveRemoteAs.Enabled = false;
                ContextOpenRemoteWith.Enabled = false;
            }

            if (string.IsNullOrEmpty(conflictedFileNames.Base.Filename))
            {
                ContextSaveBaseAs.Enabled = false;
                ContextOpenBaseWith.Enabled = false;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = GetFileName();
            Process.Start(_fullPathResolver.Resolve(fileName));
        }

        private void openWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = GetFileName();
            OsShellUtil.OpenAs(_fullPathResolver.Resolve(fileName));
        }

        private void StageFile(string filename)
        {
            var args = new GitArgumentBuilder("add")
            {
                "--",
                filename.QuoteNE()
            };
            string output = Module.GitExecutable.GetOutput(args);

            if (string.IsNullOrWhiteSpace(output))
            {
                return;
            }

            string text = string.Format(_stageFilename.Text, filename);
            FormStatus.ShowErrorDialog(this, text, text, output);
        }

        private void merge_Click(object sender, EventArgs e)
        {
            OpenMergeTool();
        }

        private void ConflictedFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OpenMergeTool();
                e.Handled = true;
            }
        }

        private void fileHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UICommands.StartFileHistoryDialog(this, GetFileName());
        }

        #region Hotkey commands

        public static readonly string HotkeySettingsName = "FormMergeConflicts";

        internal enum Commands
        {
            Merge = 0,
            Rescan = 1,
            ChooseRemote = 2,
            ChooseLocal = 3,
            ChooseBase = 4
        }

        protected override CommandStatus ExecuteCommand(int cmd)
        {
            var command = (Commands)cmd;

            switch (command)
            {
                case Commands.Merge: OpenMergetool_Click(null, null); break;
                case Commands.Rescan: Rescan_Click(null, null); break;
                case Commands.ChooseBase: ContextChooseBase_Click(null, null); break;
                case Commands.ChooseLocal: ContextChooseLocal_Click(null, null); break;
                case Commands.ChooseRemote: ContextChooseRemote_Click(null, null); break;
                default: return base.ExecuteCommand(cmd);
            }

            return true;
        }

        #endregion
    }
}
