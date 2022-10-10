﻿using System.ComponentModel;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Utils;
using GitExtUtils;
using GitUI.HelperDialogs;
using GitUI.Hotkey;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormResolveConflicts : GitModuleForm
    {
        private sealed class SortableConflictDataList : SortableBindingList<ConflictData>
        {
            static SortableConflictDataList()
            {
                AddSortableProperty(conflictData => conflictData.Filename, (x, y) => string.Compare(x.Filename, y.Filename, StringComparison.Ordinal));
            }
        }

        #region Translation
        private readonly TranslationString _uskUseCustomMergeScript = new("There is a custom merge script ({0}) for this file type." + Environment.NewLine + Environment.NewLine + "Do you want to use this custom merge script?");
        private readonly TranslationString _uskUseCustomMergeScriptCaption = new("Custom merge script");
        private readonly TranslationString _fileUnchangedAfterMerge = new("The file has not been modified by the merge. Usually this means that the file has been saved to the wrong location." + Environment.NewLine + Environment.NewLine + "The merge conflict will not be marked as solved. Please try again.");
        private readonly TranslationString _allConflictsResolved = new("All merge conflicts are resolved, you can commit." + Environment.NewLine + "Do you want to commit now?");
        private readonly TranslationString _allConflictsResolvedCaption = new("Commit");
        private readonly TranslationString _fileIsBinary = new("The selected file appears to be a binary file." + Environment.NewLine + "Are you sure you want to open this file in {0}?");
        private readonly TranslationString _askMergeConflictSolvedAfterCustomMergeScript = new("The merge conflict need to be solved and the result must be saved as:" + Environment.NewLine + "{0}" + Environment.NewLine + Environment.NewLine + "Is the merge conflict solved?");
        private readonly TranslationString _askMergeConflictSolved = new("Is the merge conflict solved?");
        private readonly TranslationString _askMergeConflictSolvedCaption = new("Conflict solved?");
        private readonly TranslationString _noMergeTool = new("There is no mergetool configured." + Environment.NewLine + "Please go to settings and set a mergetool!");
        private readonly TranslationString _noMergeToolConfigured = new("The mergetool is not correctly configured." + Environment.NewLine + "Please go to settings and configure the mergetool!");
        private readonly TranslationString _errorStartingMergetool = new("Error starting mergetool: {0}");
        private readonly TranslationString _stageFilename = new("Stage {0}");

        private readonly TranslationString _noBaseRevision = new("There is no base revision for {0}." + Environment.NewLine + "Fall back to 2-way merge?");
        private readonly TranslationString _ours = new("ours");
        private readonly TranslationString _theirs = new("theirs");
        private readonly TranslationString _fileBinaryChooseLocalBaseRemote = new("File ({0}) appears to be a binary file." + Environment.NewLine + "Choose to keep the local ({1}), remote ({2}) or base file.");
        private readonly TranslationString _fileChangeLocallyAndRemotely = new("The file has been changed both locally ({0}) and remotely ({1}). Merge the changes.");
        private readonly TranslationString _fileCreatedLocallyAndRemotely = new("A file with the same name has been created locally ({0}) and remotely ({1}). Choose the file you want to keep or merge the files.");
        private readonly TranslationString _fileCreatedLocallyAndRemotelyLong = new("File {0} does not have a base revision." + Environment.NewLine + "A file with the same name has been created locally ({1}) and remotely ({2}) causing this conflict." + Environment.NewLine + Environment.NewLine + "Choose the file you want to keep, merge the files or delete the file?");
        private readonly TranslationString _fileDeletedLocallyAndModifiedRemotely = new("The file has been deleted locally ({0}) and modified remotely ({1}). Choose to delete the file or keep the modified version.");
        private readonly TranslationString _fileDeletedLocallyAndModifiedRemotelyLong = new("File {0} does not have a local revision." + Environment.NewLine + "The file has been deleted locally ({1}) but modified remotely ({2})." + Environment.NewLine + Environment.NewLine + "Choose to delete the file or keep the modified version.");
        private readonly TranslationString _filesDeletedLocallyAndModifiedRemotelyLong = new("{0} and {1} other out of {2} selected files do not have a local revision." + Environment.NewLine + "The files have been deleted locally, but modified remotely" + Environment.NewLine + Environment.NewLine + "Choose to delete the files or keep the modified versions.");
        private readonly TranslationString _filesDeletedLocallyAndModifiedRemotelyLongNoOtherFilesSelected = new("{0} and {1} other selected file(s) do not have a local revision." + Environment.NewLine + "The files have been deleted locally, but modified remotely" + Environment.NewLine + Environment.NewLine + "Choose to delete the files or keep the modified versions.");
        private readonly TranslationString _fileModifiedLocallyAndDeletedRemotely = new("The file has been modified locally ({0}) and deleted remotely ({1}). Choose to delete the file or keep the modified version.");
        private readonly TranslationString _fileModifiedLocallyAndDeletedRemotelyLong = new("File {0} does not have a remote revision." + Environment.NewLine + "The file has been modified locally ({1}) but deleted remotely ({2})." + Environment.NewLine + Environment.NewLine + "Choose to delete the file or keep the modified version.");
        private readonly TranslationString _filesModifiedLocallyAndDeletedRemotelyLong = new("{0} and {1} other out of {2} selected files do not have a remote revision." + Environment.NewLine + "The files have been modified locally, but deleted remotely." + Environment.NewLine + Environment.NewLine + "Choose to delete the files or keep the modified versions.");
        private readonly TranslationString _filesModifiedLocallyAndDeletedRemotelyLongNoOtherFilesSelected = new("{0} and {1} other selected file(s) do not have a remote revision." + Environment.NewLine + "The files have been modified locally, but deleted remotely." + Environment.NewLine + Environment.NewLine + "Choose to delete the files or keep the modified versions.");
        private readonly TranslationString _noBase = new("no base");
        private readonly TranslationString _deleted = new("deleted");
        private readonly TranslationString _chooseLocalButtonText = new("Choose local");
        private readonly TranslationString _chooseRemoteButtonText = new("Choose remote");
        private readonly TranslationString _deleteFileButtonText = new("Delete file");
        private readonly TranslationString _keepModifiedButtonText = new("Keep modified");
        private readonly TranslationString _keepBaseButtonText = new("Keep base file");

        private readonly TranslationString _solveMergeConflictApplyToAllCheckBoxText = new("Apply to {0} and {1} other file(s)");
        private readonly TranslationString _solveMergeConflictDialogCaption = new("Solve merge conflict");

        private readonly TranslationString _conflictedFilesContextMenuText = new("Solve");
        private readonly TranslationString _openMergeToolItemText = new("Open in");
        private readonly TranslationString _button1Text = new("Open in");

        private readonly TranslationString _contextChooseLocalRebaseText = new("Choose local (theirs)");
        private readonly TranslationString _contextChooseRemoteRebaseText = new("Choose remote (ours)");

        private readonly TranslationString _contextChooseLocalMergeText = new("Choose local (ours)");
        private readonly TranslationString _contextChooseRemoteMergeText = new("Choose remote (theirs)");

        private readonly TranslationString _noBaseFileMergeCaption = new("Merge");

        private readonly TranslationString _chooseBaseFileFailedText = new("Choose base file failed.");
        private readonly TranslationString _chooseLocalFileFailedText = new("Choose local file failed.");
        private readonly TranslationString _chooseRemoteFileFailedText = new("Choose remote file failed.");

        private readonly TranslationString _currentFormatFilter =
            new("Current format (*.{0})");
        private readonly TranslationString _allFilesFilter =
            new("All files (*.*)");

        private readonly TranslationString _abortCurrentOperation =
            new("You can abort the current conflict resolution by resetting hard." + Environment.NewLine +
                "All changes since the last commit will be deleted." + Environment.NewLine +
                Environment.NewLine + "Do you want to reset the changes?");

        private readonly TranslationString _resetCaption = new("Reset");

        private readonly TranslationString _areYouSureYouWantDeleteFiles =
            new("Are you sure you want to DELETE all changes?" + Environment.NewLine +
                Environment.NewLine + "This action cannot be made undone.");

        private readonly TranslationString _areYouSureYouWantDeleteFilesCaption = new("WARNING!");

        private readonly TranslationString _failureWhileOpenFile = new("Open temporary file failed.");
        private readonly TranslationString _failureWhileSaveFile = new("Save file failed.");
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
        private string? _solveMergeConflictDialogCheckboxText;
        private int _filesDeletedLocallyAndModifiedRemotelyCount;
        private int _filesModifiedLocallyAndDeletedRemotelyCount;
        private int _filesRemainedCount;
        private int _filesDeletedLocallyAndModifiedRemotelySolved;
        private int _filesModifiedLocallyAndDeletedRemotelySolved;
        private int _conflictItemsCount;
        private readonly CancellationTokenSequence _customDiffToolsSequence = new();

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormResolveConflicts()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _customDiffToolsSequence.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
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

                SortableConflictDataList conflictDataList = new();
                conflictDataList.AddRange(ThreadHelper.JoinableTaskFactory.Run(() => Module.GetConflictsAsync()));
                ConflictedFiles.DataSource = conflictDataList;
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

                InitMergetool();

                // Update UI after tool configuration is known
                UpdateConflictedFilesMenu();

                ConflictedFilesContextMenu.Text = _conflictedFilesContextMenuText.Text;
                OpenMergetool.Text = _openMergeToolItemText.Text + " " + _mergetool;
                openMergeToolBtn.Text = _button1Text.Text + " " + _mergetool;

                if (Module.InTheMiddleOfRebase())
                {
                    ContextChooseLocal.Text = _contextChooseLocalRebaseText.Text;
                    ContextChooseRemote.Text = _contextChooseRemoteRebaseText.Text;
                }
                else
                {
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

                LoadCustomMergetools();
            }
        }

        private void Rescan_Click(object sender, EventArgs e)
        {
            Initialize();
        }

        private string? _mergetool;
        private string? _mergetoolCmd;
        private string? _mergetoolPath;

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

        private static string FixPath(string? path)
        {
            return (path ?? "").ToNativePath();
        }

        private void LoadCustomMergetools()
        {
            List<CustomDiffMergeTool> menus = new()
            {
                new(customMergetool, customMergetool_Click),
            };

            const int ToolDelay = 500;
            new CustomDiffMergeToolProvider().LoadCustomDiffMergeTools(Module, menus, components, isDiff: false, ToolDelay, cancellationToken: _customDiffToolsSequence.Next());
        }

        private readonly Dictionary<string, string> _mergeScripts = new()
        {
            { ".doc", "merge-doc.js" },
            { ".docx", "merge-doc.js" },
            { ".docm", "merge-doc.js" },
            { ".ods", "merge-ods.vbs" },
            { ".odt", "merge-ods.vbs" },
            { ".sxw", "merge-ods.vbs" },
        };

        private bool TryMergeWithScript(string fileName, string? baseFileName, string? localFileName, string? remoteFileName)
        {
            if (!EnvUtils.RunningOnWindows())
            {
                return false;
            }

            try
            {
                string? extension = Path.GetExtension(fileName)?.ToLower();
                if (extension?.Length <= 1)
                {
                    return false;
                }

                string dir = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Diff-Scripts").EnsureTrailingPathSeparator();
                if (Directory.Exists(dir))
                {
                    if (_mergeScripts.TryGetValue(extension, out var mergeScript) &&
                        File.Exists(Path.Combine(dir!, mergeScript)))
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
                MessageBox.Show(this, "Merge using script failed.\n" + ex, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        private void UseMergeWithScript(string fileName, string mergeScript, string? baseFileName, string? localFileName, string? remoteFileName)
        {
            // get timestamp of file before merge. This is an extra check to verify if merge was successfully
            var filePath = _fullPathResolver.Resolve(fileName);
            DateTime lastWriteTimeBeforeMerge = File.Exists(filePath) ? File.GetLastWriteTime(filePath) : DateTime.Now;

            ArgumentBuilder args = new()
            {
                mergeScript.Quote(),
                FixPath(filePath).Quote(),
                FixPath(remoteFileName).Quote(),
                FixPath(localFileName).Quote(),
                FixPath(baseFileName).Quote()
            };

            new Executable("wscript", Module.WorkingDir).Start(args);

            if (MessageBox.Show(this, string.Format(_askMergeConflictSolvedAfterCustomMergeScript.Text,
                FixPath(filePath)), _askMergeConflictSolvedCaption.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DateTime lastWriteTimeAfterMerge = File.Exists(filePath) ? File.GetLastWriteTime(filePath) : lastWriteTimeBeforeMerge;

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
            Validates.NotNull(_mergetool);
            string mergeToolLower = _mergetool.ToLowerInvariant();
            switch (mergeToolLower)
            {
                case "kdiff3":
                case "diffmerge":
                case "smerge":
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
            string? fullname = _fullPathResolver.Resolve(filename);
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
                FormMergeSubmodule form = new(UICommands, item.Filename);
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
                                TranslatedStrings.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
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
                    if (item.Base.Filename is null)
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
                    var filePath = _fullPathResolver.Resolve(item.Filename);
                    DateTime lastWriteTimeBeforeMerge = File.Exists(filePath) ? File.GetLastWriteTime(filePath) : DateTime.Now;

                    GitUIPluginInterfaces.ExecutionResult res;
                    try
                    {
                        res = new Executable(_mergetoolPath, Module.WorkingDir).Execute(arguments, throwOnErrorExit: false);
                    }
                    catch (Exception)
                    {
                        var text = string.Format(_errorStartingMergetool.Text, _mergetoolPath);
                        MessageBox.Show(this, text, _noBaseFileMergeCaption.Text,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    DateTime lastWriteTimeAfterMerge = File.Exists(filePath) ? File.GetLastWriteTime(filePath) : lastWriteTimeBeforeMerge;

                    // Check exitcode AND timestamp of the file. If exitcode is success and
                    // time timestamp is changed, we are pretty sure the merge was done.
                    if (res.ExitedSuccessfully && lastWriteTimeBeforeMerge != lastWriteTimeAfterMerge)
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

            void DeleteTemporaryFile(string? path)
            {
                if (path is not null && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        private bool InitMergetool()
        {
            // All _mergetool related is for native ("Windows")
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
                MessageBox.Show(this, _noMergeTool.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                if (!PathUtil.TryFindFullPath(_mergetoolPath, out string? fullPath))
                {
                    MessageBox.Show(this, _noMergeToolConfigured.Text, TranslatedStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                _mergetoolPath = fullPath;
            }

            return true;
        }

        private bool ShowAbortMessage()
        {
            if (MessageBox.Show(_abortCurrentOperation.Text, _resetCaption.Text,
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
            if (item.ObjectId is null)
            {
                return "@" + _deleted.Text;
            }

            return '@' + item.ObjectId.ToShortString();
        }

        private void ConflictedFiles_SelectionChanged(object sender, EventArgs e)
        {
            UpdateConflictedFilesMenu();
        }

        private void UpdateConflictedFilesMenu()
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
            // Disable extra GE processing if path or cmd is not set
            var mergeToolExtrasConfigured = !string.IsNullOrWhiteSpace(_mergetoolPath) || !string.IsNullOrWhiteSpace(_mergetoolCmd);
            OpenMergetool.Enabled = enabled && mergeToolExtrasConfigured;
            customMergetool.Enabled = enabled;
            openMergeToolBtn.Enabled = enabled && mergeToolExtrasConfigured;
            ContextOpenLocalWith.Enabled = enabled;
            ContextOpenRemoteWith.Enabled = enabled;
            ContextOpenBaseWith.Enabled = enabled;
            ContextSaveLocalAs.Enabled = enabled;
            ContextSaveRemoteAs.Enabled = enabled;
            ContextSaveBaseAs.Enabled = enabled;
            openToolStripMenuItem.Enabled = enabled;
            openWithToolStripMenuItem.Enabled = enabled;
            openFolderToolStripMenuItem.Enabled = enabled;
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

            conflictDescription.Text = (baseFileExists, localFileExists, remoteFileExists) switch
            {
                (true, true, true) => string.Format(_fileChangeLocallyAndRemotely.Text, localSide, remoteSide),
                (false, true, true) => string.Format(_fileCreatedLocallyAndRemotely.Text, localSide, remoteSide),
                (true, false, true) => string.Format(_fileDeletedLocallyAndModifiedRemotely.Text, localSide, remoteSide),
                (true, true, false) => string.Format(_fileModifiedLocallyAndDeletedRemotely.Text, localSide, remoteSide),
                _ => conflictDescription.Text
            };

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
                MessageBox.Show(this, _chooseBaseFileFailedText.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show(this, _chooseLocalFileFailedText.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show(this, _chooseRemoteFileFailedText.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private TaskDialogPage CreateSolveMergeConflictTaskDialogPage(string text, string instructionText, string caption, string applyToAllCheckBoxText,
            string keepLocalButtonText, string keepRemoteButtonText, string keepBaseButtonText)
        {
            TaskDialogPage page = new()
            {
                Text = text,
                Heading = instructionText,
                Caption = caption,
                Buttons = { TaskDialogButton.Cancel },
                Icon = TaskDialogIcon.Error,
                Verification = new TaskDialogVerificationCheckBox
                {
                    Text = applyToAllCheckBoxText
                },
                AllowCancel = true,
                SizeToContent = true
            };

            // Local
            TaskDialogCommandLinkButton btnKeepLocal = new(keepLocalButtonText);
            btnKeepLocal.Click += (s, e) =>
            {
                _solveMergeConflictDialogResult = ConflictResolutionPreference.KeepLocal;
            };

            // Remote
            TaskDialogCommandLinkButton btnKeepRemote = new(keepRemoteButtonText);
            btnKeepRemote.Click += (s, e) =>
            {
                _solveMergeConflictDialogResult = ConflictResolutionPreference.KeepRemote;
            };

            // Base
            TaskDialogCommandLinkButton btnKeepBase = new(keepBaseButtonText);
            btnKeepBase.Click += (s, e) =>
            {
                _solveMergeConflictDialogResult = ConflictResolutionPreference.KeepBase;
            };

            page.Buttons.Add(btnKeepLocal);
            page.Buttons.Add(btnKeepRemote);
            page.Buttons.Add(btnKeepBase);

            return page;
        }

        private void OpenSolveMergeConflictDialogAndExecuteSelectedMergeAction(Action<ConflictResolutionPreference> selectedMergeAction,
            string dialogText, string dialogInstructionText, string dialogCaption, string dialogFooterCheckboxText,
            string keepLocalButtonText, string keepRemoteButtonText, string keepBaseButtonText)
        {
            if (!_solveMergeConflictApplyToAll)
            {
                TaskDialogPage page = CreateSolveMergeConflictTaskDialogPage(dialogText, dialogInstructionText, dialogCaption, dialogFooterCheckboxText,
                    keepLocalButtonText, keepRemoteButtonText, keepBaseButtonText);

                TaskDialog.ShowDialog(Handle, page);
                _solveMergeConflictApplyToAll = page.Verification?.Checked ?? false;
            }

            selectedMergeAction(_solveMergeConflictDialogResult);
        }

        private void BinaryFilesChooseLocalBaseRemote(ConflictData item)
        {
            Validates.NotNull(_solveMergeConflictDialogCheckboxText);

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

            Validates.NotNull(_solveMergeConflictDialogCheckboxText);

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
                            GitArgumentBuilder args = new("rm")
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
                            GitArgumentBuilder args = new("rm")
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
                            GitArgumentBuilder args = new("rm")
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

                    List<ConflictData> filesDeletedLocallyAndModifiedRemotely = new();
                    List<ConflictData> filesModifiedLocallyAndDeletedRemotely = new();
                    List<ConflictData> filesRemaining = new();

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

                    StartProgressBarWithMaxValue(_conflictItemsCount);

                    _solveMergeConflictApplyToAll = false;
                    foreach (var conflictData in filesDeletedLocallyAndModifiedRemotely)
                    {
                        IncrementProgressBarValue();
                        ResolveItemConflict(conflictData);
                    }

                    _solveMergeConflictApplyToAll = false;
                    foreach (var conflictData in filesModifiedLocallyAndDeletedRemotely)
                    {
                        IncrementProgressBarValue();
                        ResolveItemConflict(conflictData);
                    }

                    _solveMergeConflictDialogCheckboxText = string.Empty; // Hide checkbox "apply to all"
                    _solveMergeConflictApplyToAll = false;
                    foreach (var conflictData in filesRemaining)
                    {
                        IncrementProgressBarValue();
                        ResolveItemConflict(conflictData);
                    }
                }
                finally
                {
                    _solveMergeConflictApplyToAll = false;
                    StopAndHideProgressBar();
                    Initialize();
                }
            }
        }

        private void OpenMergetool_Click(object sender, EventArgs e)
        {
            OpenMergeTool();
        }

        private void customMergetool_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item?.DropDownItems != null)
            {
                // "main menu" clicked, cancel dropdown manually, invoke default mergetool
                item.HideDropDown();
                item.Owner.Hide();
            }

            using (WaitCursorScope.Enter())
            {
                var customTool = item?.Tag as string;

                foreach (var conflict in GetConflicts())
                {
                    Directory.SetCurrentDirectory(Module.WorkingDir);
                    Module.RunMergeTool(fileName: conflict.Filename, customTool: customTool);
                    Initialize();
                }
            }
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
                    MessageBox.Show(this, _failureWhileOpenFile.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                string fileName = PathUtil.GetFileName(conflictData.Filename);
                var initialDirectory = _fullPathResolver.Resolve(Path.GetDirectoryName(conflictData.Filename));

                using SaveFileDialog fileDialog = new()
                {
                    FileName = fileName,
                    InitialDirectory = initialDirectory,
                    AddExtension = true
                };
                var ext = Path.GetExtension(fileDialog.FileName);
                fileDialog.DefaultExt = ext;
                fileDialog.Filter = string.Format(_currentFormatFilter.Text, ext) + "|*." + ext + "|" + _allFilesFilter.Text + "|*.*";

                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (!Module.HandleConflictsSaveSide(conflictData.Filename, fileDialog.FileName, side))
                    {
                        MessageBox.Show(this, _failureWhileSaveFile.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            string? filePath = _fullPathResolver.Resolve(fileName);
            if (filePath is not null)
            {
                OsShellUtil.Open(filePath);
            }
        }

        private void openWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = GetFileName();
            string? filePath = _fullPathResolver.Resolve(fileName);
            if (filePath is not null)
            {
                OsShellUtil.OpenAs(filePath);
            }
        }

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = GetFileName();
            string? filePath = _fullPathResolver.Resolve(fileName);
            if (filePath is not null)
            {
                OsShellUtil.SelectPathInFileExplorer(filePath.ToNativePath());
            }
        }

        private void StageFile(string filename)
        {
            GitArgumentBuilder args = new("add")
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
                case Commands.Merge: OpenMergetool_Click(this, EventArgs.Empty); break;
                case Commands.Rescan: Rescan_Click(this, EventArgs.Empty); break;
                case Commands.ChooseBase: ContextChooseBase_Click(this, EventArgs.Empty); break;
                case Commands.ChooseLocal: ContextChooseLocal_Click(this, EventArgs.Empty); break;
                case Commands.ChooseRemote: ContextChooseRemote_Click(this, EventArgs.Empty); break;
                default: return base.ExecuteCommand(cmd);
            }

            return true;
        }

        #endregion
    }
}
