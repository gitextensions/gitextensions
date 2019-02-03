using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitCommands.Utils;
using GitUI.CommandsDialogs.ResolveConflictsDialog;
using GitUI.Hotkey;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormResolveConflicts : GitModuleForm
    {
        #region Translation
        // ReSharper disable InconsistentNaming
        private readonly TranslationString _uskUseCustomMergeScript = new TranslationString("There is a custom merge script ({0}) for this file type." + Environment.NewLine + Environment.NewLine + "Do you want to use this custom merge script?");
        private readonly TranslationString _uskUseCustomMergeScriptCaption = new TranslationString("Custom merge script");
        private readonly TranslationString _fileUnchangedAfterMerge = new TranslationString("The file has not been modified by the merge. Usually this means that the file has been saved to the wrong location." + Environment.NewLine + Environment.NewLine + "The merge conflict will not be marked as solved. Please try again.");
        private readonly TranslationString _allConflictsResolved = new TranslationString("All merge conflicts are resolved, you can commit." + Environment.NewLine + "Do you want to commit now?");
        private readonly TranslationString _allConflictsResolvedCaption = new TranslationString("Commit");
        private readonly TranslationString _fileIsBinary = new TranslationString("The selected file appears to be a binary file." + Environment.NewLine + "Are you sure you want to open this file in {0}?");
        private readonly TranslationString _askMergeConflictSolvedAfterCustomMergeScript = new TranslationString("The merge conflict need to be solved and the result must be saved as:" + Environment.NewLine + "{0}" + Environment.NewLine + Environment.NewLine + "Is the merge conflict solved?");
        private readonly TranslationString _askMergeConflictSolved = new TranslationString("Is the merge conflict solved?");
        private readonly TranslationString _askMergeConflictSolvedCaption = new TranslationString("Conflict solved?");
        private readonly TranslationString _noMergeTool = new TranslationString("There is no mergetool configured. Please go to settings and set a mergetool!");
        private readonly TranslationString _noMergeToolConfigured = new TranslationString("The mergetool is not correctly configured. Please go to settings and configure the mergetool!");
        private readonly TranslationString _stageFilename = new TranslationString("Stage {0}");

        private readonly TranslationString _noBaseRevision = new TranslationString("There is no base revision for {0}.\nFall back to 2-way merge?");
        private readonly TranslationString _ours = new TranslationString("ours");
        private readonly TranslationString _theirs = new TranslationString("theirs");
        private readonly TranslationString _fileBinaryChooseLocalBaseRemote = new TranslationString("File ({0}) appears to be a binary file.\nChoose to keep the local({1}), remote({2}) or base file.");
        private readonly TranslationString _fileChangeLocallyAndRemotely = new TranslationString("The file has been changed both locally({0}) and remotely({1}). Merge the changes.");
        private readonly TranslationString _fileCreatedLocallyAndRemotely = new TranslationString("A file with the same name has been created locally({0}) and remotely({1}). Choose the file you want to keep or merge the files.");
        private readonly TranslationString _fileCreatedLocallyAndRemotelyLong = new TranslationString("File {0} does not have a base revision.\nA file with the same name has been created locally({1}) and remotely({2}) causing this conflict.\n\nChoose the file you want to keep, merge the files or delete the file?");
        private readonly TranslationString _fileDeletedLocallyAndModifiedRemotely = new TranslationString("The file has been deleted locally({0}) and modified remotely({1}). Choose to delete the file or keep the modified version.");
        private readonly TranslationString _fileDeletedLocallyAndModifiedRemotelyLong = new TranslationString("File {0} does not have a local revision.\nThe file has been deleted locally({1}) but modified remotely({2}).\n\nChoose to delete the file or keep the modified version.");
        private readonly TranslationString _fileModifiedLocallyAndDeletedRemotely = new TranslationString("The file has been modified locally({0}) and deleted remotely({1}). Choose to delete the file or keep the modified version.");
        private readonly TranslationString _fileModifiedLocallyAndDeletedRemotelyLong = new TranslationString("File {0} does not have a remote revision.\nThe file has been modified locally({1}) but deleted remotely({2}).\n\nChoose to delete the file or keep the modified version.");
        private readonly TranslationString _noBase = new TranslationString("no base");
        private readonly TranslationString _deleted = new TranslationString("deleted");
        private readonly TranslationString _chooseLocalButtonText = new TranslationString("Choose local");
        private readonly TranslationString _chooseRemoteButtonText = new TranslationString("Choose remote");
        private readonly TranslationString _deleteFileButtonText = new TranslationString("Delete file");
        private readonly TranslationString _keepModifiedButtonText = new TranslationString("Keep modified");
        private readonly TranslationString _keepBaseButtonText = new TranslationString("Keep base file");

        // ReSharper restore InconsistentNaming

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

        private readonly IFullPathResolver _fullPathResolver;

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
                if (ConflictedFiles.SelectedRows.Count > 0)
                {
                    oldSelectedRow = ConflictedFiles.SelectedRows[0].Index;
                }

                ConflictedFiles.DataSource = Module.GetConflicts();
                ConflictedFiles.Columns[0].DataPropertyName = nameof(ConflictData.Filename);
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
                MessageBox.Show(this, "Merge using script failed.\n" + ex);
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
                    MessageBox.Show(this, _fileUnchangedAfterMerge.Text);
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
            using (WaitCursorScope.Enter())
            {
                try
                {
                    var items = GetConflicts();

                    StartProgressBarWithMaxValue(items.Count);
                    foreach (var conflictData in items)
                    {
                        IncrementProgressBarValue();
                        ResolveItemConflict(conflictData);
                    }
                }
                finally
                {
                    StopAndHideProgressBar();
                    Initialize();
                }
            }
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
                            _binaryFileWarningCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2) == DialogResult.No)
                        {
                            BinaryFilesChooseLocalBaseRemote(item);
                            return;
                        }
                    }

                    string arguments = _mergetoolCmd;

                    // Check if there is a base file. If not, ask user to fall back to 2-way merge.
                    // git doesn't support 2-way merge, but we can try to adjust attributes to fix this.
                    // For kdiff3 this is easy; just remove the 3rd file from the arguments. Since the
                    // filenames are quoted, this takes a little extra effort. We need to remove these
                    // quotes also. For tortoise and araxis a little bit more magic is needed.
                    if (item.Base.Filename == null)
                    {
                        var text = string.Format(_noBaseRevision.Text, item.Filename);
                        DialogResult result = MessageBox.Show(this, text, _noBaseFileMergeCaption.Text,
                            MessageBoxButtons.YesNoCancel);
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

                    var res = new Executable(_mergetoolPath, Module.WorkingDir).Execute(arguments);

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
            _mergetool = Module.GetEffectiveSetting("merge.tool");
            if (string.IsNullOrEmpty(_mergetool))
            {
                MessageBox.Show(this, _noMergeTool.Text);
                return false;
            }

            using (WaitCursorScope.Enter())
            {
                _mergetoolCmd = Module.GetEffectiveSetting($"mergetool.{_mergetool}.cmd");
                _mergetoolPath = Module.GetEffectiveSetting($"mergetool.{_mergetool}.path");

                if (_mergetool == "kdiff3")
                {
                    if (string.IsNullOrEmpty(_mergetoolPath))
                    {
                        _mergetoolPath = "kdiff3";
                    }

                    _mergetoolCmd = "\"$BASE\" \"$LOCAL\" \"$REMOTE\" -o \"$MERGED\"";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(_mergetoolCmd) || string.IsNullOrWhiteSpace(_mergetoolPath))
                    {
                        MessageBox.Show(this, _noMergeToolConfigured.Text);
                        return false;
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
                }
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

            return '@' + item.ObjectId.ToShortString(8);
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
                MessageBox.Show(this, _chooseBaseFileFailedText.Text);
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
                MessageBox.Show(this, _chooseLocalFileFailedText.Text);
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
                MessageBox.Show(this, _chooseRemoteFileFailedText.Text);
            }
        }

        private void BinaryFilesChooseLocalBaseRemote(ConflictData item)
        {
            string caption = string.Format(_fileBinaryChooseLocalBaseRemote.Text,
                                            item.Local.Filename,
                                            GetLocalSideString(),
                                            GetRemoteSideString());

            using (var frm = new FormModifiedDeletedCreated(string.Format(_chooseLocalButtonText.Text + " ({0})", GetLocalSideString()),
                                                                            string.Format(_chooseRemoteButtonText.Text + " ({0})", GetRemoteSideString()),
                                                                            _keepBaseButtonText.Text,
                                                                            caption))
            {
                frm.ShowDialog(this);

                if (frm.KeepBase)
                {
                    // base
                    ChooseBaseOnConflict(item.Filename);
                }

                if (frm.KeepLocal)
                {
                    // local
                    ChooseLocalOnConflict(item.Filename);
                }

                if (frm.KeepRemote)
                {
                    // remote
                    ChooseRemoteOnConflict(item.Filename);
                }
            }
        }

        private bool CheckForBaseRevision(ConflictData item)
        {
            if (!string.IsNullOrEmpty(item.Base.Filename))
            {
                return true;
            }

            string caption = string.Format(_fileCreatedLocallyAndRemotelyLong.Text,
                item.Filename,
                GetLocalSideString(),
                GetRemoteSideString());

            using (var frm = new FormModifiedDeletedCreated(string.Format(_chooseLocalButtonText.Text + " ({0})", GetLocalSideString()),
                string.Format(_chooseRemoteButtonText.Text + " ({0})", GetRemoteSideString()),
                _deleteFileButtonText.Text,
                caption))
            {
                frm.ShowDialog(this);

                if (frm.KeepBase)
                {
                    // delete
                    var args = new GitArgumentBuilder("rm")
                    {
                        "--",
                        item.Filename.QuoteNE()
                    };
                    Module.GitExecutable.GetOutput(args);
                }

                if (frm.KeepLocal)
                {
                    // local
                    ChooseLocalOnConflict(item.Filename);
                }

                if (frm.KeepRemote)
                {
                    // remote
                    ChooseRemoteOnConflict(item.Filename);
                }
            }

            return false;
        }

        private bool CheckForLocalRevision(ConflictData item)
        {
            if (!string.IsNullOrEmpty(item.Local.Filename))
            {
                return true;
            }

            string caption = string.Format(_fileDeletedLocallyAndModifiedRemotelyLong.Text,
                item.Filename,
                GetLocalSideString(),
                GetRemoteSideString());

            using (var frm = new FormModifiedDeletedCreated(string.Format(_deleteFileButtonText.Text + " ({0})", GetLocalSideString()),
                string.Format(_keepModifiedButtonText.Text + " ({0})", GetRemoteSideString()),
                _keepBaseButtonText.Text,
                caption))
            {
                frm.ShowDialog(this);

                if (frm.KeepBase)
                {
                    // base
                    ChooseBaseOnConflict(item.Filename);
                }

                if (frm.KeepLocal)
                {
                    // delete
                    var args = new GitArgumentBuilder("rm")
                    {
                        "--",
                        item.Filename.QuoteNE()
                    };
                    Module.GitExecutable.GetOutput(args);
                }

                if (frm.KeepRemote)
                {
                    // remote
                    ChooseRemoteOnConflict(item.Filename);
                }
            }

            return false;
        }

        private bool CheckForRemoteRevision(ConflictData item)
        {
            if (!string.IsNullOrEmpty(item.Remote.Filename))
            {
                return true;
            }

            string caption = string.Format(_fileModifiedLocallyAndDeletedRemotelyLong.Text,
                item.Filename,
                GetLocalSideString(),
                GetRemoteSideString());

            using (var frm = new FormModifiedDeletedCreated(string.Format(_keepModifiedButtonText.Text + " ({0})", GetLocalSideString()),
                string.Format(_deleteFileButtonText.Text + " ({0})", GetRemoteSideString()),
                _keepBaseButtonText.Text,
                caption))
            {
                frm.ShowDialog(this);

                if (frm.KeepBase)
                {
                    // base
                    ChooseBaseOnConflict(item.Filename);
                }

                if (frm.KeepLocal)
                {
                    // delete
                    ChooseLocalOnConflict(item.Filename);
                }

                if (frm.KeepRemote)
                {
                    // remote
                    var args = new GitArgumentBuilder("rm")
                    {
                        "--",
                        item.Filename.QuoteNE()
                    };
                    Module.GitExecutable.GetOutput(args);
                }
            }

            return false;
        }

        private void OpenMergetool_Click(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                ConflictedFiles_DoubleClick(sender, e);
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
                    MessageBox.Show(this, _failureWhileOpenFile.Text);
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

                System.Drawing.Point pt = ConflictedFiles.PointToClient(Cursor.Position);
                DataGridView.HitTestInfo hti = ConflictedFiles.HitTest(pt.X, pt.Y);
                int lastRow = hti.RowIndex;
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
                    InitialDirectory = _fullPathResolver.Resolve(PathUtil.GetDirectoryName(conflictData.Filename)),
                    AddExtension = true
                })
                {
                    var ext = PathUtil.GetFileExtension(fileDialog.FileName);
                    fileDialog.DefaultExt = ext;
                    fileDialog.Filter = string.Format(_currentFormatFilter.Text, ext) + "|*." + ext + "|" + _allFilesFilter.Text + "|*.*";

                    if (fileDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        if (!Module.HandleConflictsSaveSide(conflictData.Filename, fileDialog.FileName, side))
                        {
                            MessageBox.Show(this, _failureWhileSaveFile.Text);
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
            Process.Start(_fullPathResolver.Resolve(fileName));
        }

        private void openWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = GetFileName();
            OsShellUtil.OpenAs(_fullPathResolver.Resolve(fileName));
        }

        private void StageFile(string filename)
        {
            using (var form = new FormStatus(ProcessStart, string.Format(_stageFilename.Text, filename)))
            {
                form.ShowDialogOnError(this);
            }

            void ProcessStart(FormStatus form)
            {
                form.AddMessageLine(string.Format(_stageFilename.Text, filename));
                var args = new GitArgumentBuilder("add")
                {
                    "--",
                    filename.QuoteNE()
                };
                string output = Module.GitExecutable.GetOutput(args);
                form.AddMessageLine(output);
                form.Done(isSuccess: string.IsNullOrWhiteSpace(output));
            }
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

        public static readonly string HotkeySettingsName = "FormMergeConflicts";

        internal enum Commands
        {
            Merge = 0,
            Rescan = 1,
            ChooseRemote = 2,
            ChooseLocal = 3,
            ChooseBase = 4
        }

        protected override bool ExecuteCommand(int cmd)
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
