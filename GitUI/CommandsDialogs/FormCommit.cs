using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Utils;
using GitUI.AutoCompletion;
using GitUI.CommandsDialogs.CommitDialog;
using GitUI.HelperDialogs;
using GitUI.Hotkey;
using GitUI.Script;
using PatchApply;
using ResourceManager;
using Timer = System.Windows.Forms.Timer;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormCommit : GitModuleForm //, IHotkeyable
    {
        #region Translation
        private readonly TranslationString _amendCommit =
            new TranslationString("You are about to rewrite history." + Environment.NewLine +
                                  "Only use amend if the commit is not published yet!" + Environment.NewLine +
                                  Environment.NewLine + "Do you want to continue?");

        private readonly TranslationString _amendCommitCaption = new TranslationString("Amend commit");

        private readonly TranslationString _deleteFailed = new TranslationString("Delete file failed");

        private readonly TranslationString _deleteSelectedFiles =
            new TranslationString("Are you sure you want delete the selected file(s)?");

        private readonly TranslationString _deleteSelectedFilesCaption = new TranslationString("Delete");

        private readonly TranslationString _deleteUntrackedFiles =
            new TranslationString("Are you sure you want to delete all untracked files?");

        private readonly TranslationString _deleteUntrackedFilesCaption =
            new TranslationString("Delete untracked files.");

        private readonly TranslationString _enterCommitMessage = new TranslationString("Please enter commit message");
        private readonly TranslationString _enterCommitMessageCaption = new TranslationString("Commit message");
        private readonly TranslationString _commitMessageDisabled = new TranslationString("Commit Message is requested during commit");

        private readonly TranslationString _enterCommitMessageHint = new TranslationString("Enter commit message");

        private readonly TranslationString _mergeConflicts =
            new TranslationString("There are unresolved mergeconflicts, solve mergeconflicts before committing.");

        private readonly TranslationString _mergeConflictsCaption = new TranslationString("Merge conflicts");

        private readonly TranslationString _noFilesStagedAndNothingToCommit =
            new TranslationString("There are no files staged for this commit.");
        private readonly TranslationString _noFilesStagedButSuggestToCommitAllUnstaged =
            new TranslationString("There are no files staged for this commit. Stage and commit all unstaged files?");
        private readonly TranslationString _noFilesStagedAndConfirmAnEmptyMergeCommit =
            new TranslationString("There are no files staged for this commit.\nAre you sure you want to commit?");

        private readonly TranslationString _noStagedChanges = new TranslationString("There are no staged changes");
        private readonly TranslationString _noUnstagedChanges = new TranslationString("There are no unstaged changes");

        private readonly TranslationString _notOnBranchMainInstruction = new TranslationString("You are not working on a branch");
        private readonly TranslationString _notOnBranch =
            new TranslationString("This commit will be unreferenced when switching to another branch and can be lost." +
                                  Environment.NewLine + "" + Environment.NewLine + "Do you want to continue?");
        private readonly TranslationString _notOnBranchButtons = new TranslationString("Checkout branch|Create branch|Continue");
        private readonly TranslationString _notOnBranchCaption = new TranslationString("Not on a branch");

        private readonly TranslationString _onlyStageChunkOfSingleFileError =
            new TranslationString("You can only use this option when selecting a single file");

        private readonly TranslationString _resetChangesCaption = new TranslationString("Reset changes");

        private readonly TranslationString _resetSelectedChangesText =
            new TranslationString("Are you sure you want to reset all selected files?");

        private readonly TranslationString _resetStageChunkOfFileCaption = new TranslationString("Unstage chunk of file");
        private readonly TranslationString _stageDetails = new TranslationString("Stage Details");
        private readonly TranslationString _stageFiles = new TranslationString("Stage {0} files");
        private readonly TranslationString _selectOnlyOneFile = new TranslationString("You must have only one file selected.");
        private readonly TranslationString _selectOnlyOneFileCaption = new TranslationString("Error");

        private readonly TranslationString _stageSelectedLines = new TranslationString("Stage selected line(s)");
        private readonly TranslationString _unstageSelectedLines = new TranslationString("Unstage selected line(s)");
        private readonly TranslationString _resetSelectedLines = new TranslationString("Reset selected line(s)");
        private readonly TranslationString _resetSelectedLinesConfirmation = new TranslationString("Are you sure you want to reset the changes to the selected lines?");

        private readonly TranslationString _formTitle = new TranslationString("Commit to {0} ({1})");

        private readonly TranslationString _selectionFilterToolTip = new TranslationString("Enter a regular expression to select unstaged files.");
        private readonly TranslationString _selectionFilterErrorToolTip = new TranslationString("Error {0}");

        private readonly TranslationString _commitMsgFirstLineInvalid = new TranslationString("First line of commit message contains too many characters."
            + Environment.NewLine + "Do you want to continue?");

        private readonly TranslationString _commitMsgLineInvalid = new TranslationString("The following line of commit message contains too many characters:"
            + Environment.NewLine + Environment.NewLine + "{0}" + Environment.NewLine + Environment.NewLine + "Do you want to continue?");

        private readonly TranslationString _commitMsgSecondLineNotEmpty = new TranslationString("Second line of commit message is not empty." + Environment.NewLine + "Do you want to continue?");

        private readonly TranslationString _commitMsgRegExNotMatched = new TranslationString("Commit message does not match RegEx." + Environment.NewLine + "Do you want to continue?");

        private readonly TranslationString _commitValidationCaption = new TranslationString("Commit validation");

        private readonly TranslationString _commitTemplateSettings = new TranslationString("Settings");

        private readonly TranslationString _commitAuthorInfo = new TranslationString("Author");
        private readonly TranslationString _commitCommitterInfo = new TranslationString("Committer");
        private readonly TranslationString _commitCommitterToolTip = new TranslationString("Click to change committer information.");
        #endregion

        private FileStatusList _currentFilesList;
        private bool _skipUpdate;
        private readonly TaskScheduler _taskScheduler;
        private GitItemStatus _currentItem;
        private bool _currentItemStaged;
        private readonly CommitKind _commitKind;
        private readonly GitRevision _editedCommit;
        private readonly ToolStripMenuItem _stageSelectedLinesToolStripMenuItem;
        private readonly ToolStripMenuItem _resetSelectedLinesToolStripMenuItem;
        private string _commitTemplate;
        private bool IsMergeCommit { get; set; }
        private bool _shouldRescanChanges = true;
        private bool _shouldReloadCommitTemplates = true;
        private readonly AsyncLoader _unstagedLoader;
        private readonly bool _useFormCommitMessage;
        private CancellationTokenSource _interactiveAddBashCloseWaitCts = new CancellationTokenSource();
        private string _userName = "";
        private string _userEmail = "";
        /// <summary>
        /// For VS designer
        /// </summary>
        private FormCommit()
            : this(null)
        {
        }

        public FormCommit(GitUICommands aCommands)
            : this(aCommands, CommitKind.Normal, null)
        { }

        public FormCommit(GitUICommands aCommands, CommitKind commitKind, GitRevision editedCommit)
            : base(true, aCommands)
        {
            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            _unstagedLoader = new AsyncLoader(_taskScheduler);

            _useFormCommitMessage = AppSettings.UseFormCommitMessage;

            InitializeComponent();
            Message.TextChanged += Message_TextChanged;
            Message.TextAssigned += Message_TextAssigned;

            if (Module != null)
                Message.AddAutoCompleteProvider(new CommitAutoCompleteProvider(Module));

            Loading.Image = Properties.Resources.loadingpanel;

            Translate();

            SolveMergeconflicts.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold);

            SelectedDiff.ExtraDiffArgumentsChanged += SelectedDiffExtraDiffArgumentsChanged;

            if (IsUICommandsInitialized)
                StageInSuperproject.Visible = Module.SuperprojectModule != null;
            StageInSuperproject.Checked = AppSettings.StageInSuperprojectAfterCommit;
            closeDialogAfterEachCommitToolStripMenuItem.Checked = AppSettings.CloseCommitDialogAfterCommit;
            closeDialogAfterAllFilesCommittedToolStripMenuItem.Checked = AppSettings.CloseCommitDialogAfterLastCommit;
            refreshDialogOnFormFocusToolStripMenuItem.Checked = AppSettings.RefreshCommitDialogOnFormFocus;

            Unstaged.SetNoFilesText(_noUnstagedChanges.Text);
            Unstaged.FilterVisible = true;
            Staged.SetNoFilesText(_noStagedChanges.Text);

            Message.Enabled = _useFormCommitMessage;

            commitMessageToolStripMenuItem.Enabled = _useFormCommitMessage;
            commitTemplatesToolStripMenuItem.Enabled = _useFormCommitMessage;

            Message.WatermarkText = _useFormCommitMessage
                ? _enterCommitMessageHint.Text
                : _commitMessageDisabled.Text;

            _commitKind = commitKind;
            _editedCommit = editedCommit;

            HotkeysEnabled = true;
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);

            SelectedDiff.AddContextMenuSeparator();
            _stageSelectedLinesToolStripMenuItem = SelectedDiff.AddContextMenuEntry(_stageSelectedLines.Text, StageSelectedLinesToolStripMenuItemClick);
            _stageSelectedLinesToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys((int)Commands.StageSelectedFile).ToShortcutKeyDisplayString();
            _resetSelectedLinesToolStripMenuItem = SelectedDiff.AddContextMenuEntry(_resetSelectedLines.Text, ResetSelectedLinesToolStripMenuItemClick);
            _resetSelectedLinesToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys((int)Commands.ResetSelectedFiles).ToShortcutKeyDisplayString();
            _resetSelectedLinesToolStripMenuItem.Image = Reset.Image;
            resetChanges.ShortcutKeyDisplayString = _resetSelectedLinesToolStripMenuItem.ShortcutKeyDisplayString;
            stagedResetChanges.ShortcutKeyDisplayString = _resetSelectedLinesToolStripMenuItem.ShortcutKeyDisplayString;
            deleteFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys((int)Commands.DeleteSelectedFiles).ToShortcutKeyDisplayString();
            viewFileHistoryToolStripItem.ShortcutKeyDisplayString = GetShortcutKeys((int)Commands.ShowHistory).ToShortcutKeyDisplayString();
            toolStripMenuItem6.ShortcutKeyDisplayString = GetShortcutKeys((int)Commands.ShowHistory).ToShortcutKeyDisplayString();
            commitAuthorStatus.ToolTipText = _commitCommitterToolTip.Text;
            toolAuthor.Control.PreviewKeyDown += ToolAuthor_PreviewKeyDown;
        }

        void ToolAuthor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Alt)
                e.IsInputKey = true;
        }

        private void FormCommit_Load(object sender, EventArgs e)
        {
            if (AppSettings.CommitDialogSplitter != -1)
                splitMain.SplitterDistance = AppSettings.CommitDialogSplitter;
            if (AppSettings.CommitDialogRightSplitter != -1)
                splitRight.SplitterDistance = AppSettings.CommitDialogRightSplitter;

            Reset.Visible = AppSettings.ShowResetAllChanges;
            ResetUnStaged.Visible = AppSettings.ShowResetUnstagedChanges;
            CommitAndPush.Visible = AppSettings.ShowCommitAndPush;
            AdjustCommitButtonPanelHeight();
        }

        private void AdjustCommitButtonPanelHeight()
        {
            splitRight.Panel2MinSize = Math.Max(splitRight.Panel2MinSize, flowCommitButtons.PreferredSize.Height);
            splitRight.SplitterDistance = Math.Min(splitRight.SplitterDistance, splitRight.Height - splitRight.Panel2MinSize);
        }

        private void FormCommitFormClosing(object sender, FormClosingEventArgs e)
        {
            Message.CancelAutoComplete();

            // Do not remember commit message of fixup or squash commits, since they have
            // a special meaning, and can be dangerous if used inappropriately.
            if (CommitKind.Normal == _commitKind)
                GitCommands.CommitHelper.SetCommitMessage(Module, Message.Text);

            AppSettings.CommitDialogSplitter = splitMain.SplitterDistance;
            AppSettings.CommitDialogRightSplitter = splitRight.SplitterDistance;
        }

        void SelectedDiff_ContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _stageSelectedLinesToolStripMenuItem.Enabled = SelectedDiff.HasAnyPatches() || _currentItem != null && _currentItem.IsNew;
            _resetSelectedLinesToolStripMenuItem.Enabled = _stageSelectedLinesToolStripMenuItem.Enabled;
        }

        #region Hotkey commands

        public const string HotkeySettingsName = "Commit";

        internal enum Commands
        {
            AddToGitIgnore,
            DeleteSelectedFiles,
            FocusUnstagedFiles,
            FocusSelectedDiff,
            FocusStagedFiles,
            FocusCommitMessage,
            ResetSelectedFiles,
            StageSelectedFile,
            UnStageSelectedFile,
            ShowHistory,
            ToggleSelectionFilter
        }

        private bool AddToGitIgnore()
        {
            if (Unstaged.Focused)
            {
                AddFileTogitignoreToolStripMenuItemClick(this, null);
                return true;
            }
            return false;
        }

        private bool DeleteSelectedFiles()
        {
            if (Unstaged.Focused)
            {
                DeleteFileToolStripMenuItemClick(this, null);
                return true;
            }
            return false;
        }

        private bool FocusStagedFiles()
        {
            Staged.Focus();
            return true;
        }

        private bool FocusUnstagedFiles()
        {
            Unstaged.Focus();
            return true;
        }

        private bool FocusSelectedDiff()
        {
            SelectedDiff.Focus();
            return true;
        }

        private bool FocusCommitMessage()
        {
            Message.Focus();
            return true;
        }

        private bool ResetSelectedFiles()
        {
            if (Unstaged.Focused || Staged.Focused)
            {
                ResetSoftClick(this, null);
                return true;
            }
            else if (SelectedDiff.ContainsFocus && _resetSelectedLinesToolStripMenuItem.Enabled)
            {
                ResetSelectedLinesToolStripMenuItemClick(this, null);
                return true;
            }

            return false;
        }

        private bool StageSelectedFile()
        {
            if (Unstaged.Focused)
            {
                StageClick(this, null);
                return true;
            }
            else if (SelectedDiff.ContainsFocus && !_currentItemStaged && _stageSelectedLinesToolStripMenuItem.Enabled)
            {
                StageSelectedLinesToolStripMenuItemClick(this, null);
                return true;
            }

            return false;
        }

        private bool UnStageSelectedFile()
        {
            if (Staged.Focused)
            {
                UnstageFilesClick(this, null);
                return true;
            }
            else if (SelectedDiff.ContainsFocus && _currentItemStaged && _stageSelectedLinesToolStripMenuItem.Enabled)
            {
                StageSelectedLinesToolStripMenuItemClick(this, null);
                return true;
            }
            return false;
        }

        private bool StartFileHistoryDialog()
        {
            if (Staged.Focused || Unstaged.Focused)
            {
                if (_currentFilesList.SelectedItem != null)
                {
                    if ((!_currentFilesList.SelectedItem.IsNew) && (!_currentFilesList.SelectedItem.IsRenamed))
                    {
                        UICommands.StartFileHistoryDialog(this, _currentFilesList.SelectedItem.Name, null);
                    }
                }
                return true;
            }
            return false;
        }


        private bool ToggleSelectionFilter()
        {
            selectionFilterToolStripMenuItem.Checked = !selectionFilterToolStripMenuItem.Checked;
            toolbarSelectionFilter.Visible = selectionFilterToolStripMenuItem.Checked;
            return true;
        }

        protected override bool ExecuteCommand(int cmd)
        {
            switch ((Commands)cmd)
            {
                case Commands.AddToGitIgnore: return AddToGitIgnore();
                case Commands.DeleteSelectedFiles: return DeleteSelectedFiles();
                case Commands.FocusStagedFiles: return FocusStagedFiles();
                case Commands.FocusUnstagedFiles: return FocusUnstagedFiles();
                case Commands.FocusSelectedDiff: return FocusSelectedDiff();
                case Commands.FocusCommitMessage: return FocusCommitMessage();
                case Commands.ResetSelectedFiles: return ResetSelectedFiles();
                case Commands.StageSelectedFile: return StageSelectedFile();
                case Commands.UnStageSelectedFile: return UnStageSelectedFile();
                case Commands.ShowHistory: return StartFileHistoryDialog();
                case Commands.ToggleSelectionFilter: return ToggleSelectionFilter();
                default: return base.ExecuteCommand(cmd);
            }
        }

        #endregion

        public void ShowDialogWhenChanges()
        {
            ShowDialogWhenChanges(null);
        }

        private void ComputeUnstagedFiles(Action<IList<GitItemStatus>> onComputed, bool DoAsync)
        {
            Func<IList<GitItemStatus>> getAllChangedFilesWithSubmodulesStatus = () => Module.GetAllChangedFilesWithSubmodulesStatus(
                    !showIgnoredFilesToolStripMenuItem.Checked,
                    !showAssumeUnchangedFilesToolStripMenuItem.Checked,
                    showUntrackedFilesToolStripMenuItem.Checked ? UntrackedFilesMode.Default : UntrackedFilesMode.No);

            if (DoAsync)
                _unstagedLoader.Load(getAllChangedFilesWithSubmodulesStatus, onComputed);
            else
            {
                _unstagedLoader.Cancel();
                onComputed(getAllChangedFilesWithSubmodulesStatus());
            }
        }


        public void ShowDialogWhenChanges(IWin32Window owner)
        {
            ComputeUnstagedFiles((allChangedFiles) =>
                {
                    if (allChangedFiles.Count > 0)
                    {
                        LoadUnstagedOutput(allChangedFiles);
                        Initialize(false);
                        ShowDialog(owner);
                    }
                    else
                        Close();
#if !__MonoCS__ // animated GIFs are not supported in Mono/Linux
                    //trying to properly dispose loading image issue #1037
                    Loading.Image.Dispose();
#endif
                }, false
            );
        }

        private bool _selectedDiffReloaded = true;

        private void StageSelectedLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            //to prevent multiple clicks
            if (!_selectedDiffReloaded)
                return;

            Debug.Assert(_currentItem != null);
            // Prepare git command
            string args = "apply --cached --whitespace=nowarn";

            if (_currentItemStaged) //staged
                args += " --reverse";
            byte[] patch;
            if (!_currentItemStaged && _currentItem.IsNew)
                patch = PatchManager.GetSelectedLinesAsNewPatch(Module, _currentItem.Name,
                    SelectedDiff.GetText(), SelectedDiff.GetSelectionPosition(),
                    SelectedDiff.GetSelectionLength(), SelectedDiff.Encoding, false, SelectedDiff.FilePreabmle);
            else
                patch = PatchManager.GetSelectedLinesAsPatch(Module, SelectedDiff.GetText(),
                    SelectedDiff.GetSelectionPosition(), SelectedDiff.GetSelectionLength(),
                    _currentItemStaged, SelectedDiff.Encoding, _currentItem.IsNew);

            if (patch != null && patch.Length > 0)
            {
                string output = Module.RunGitCmd(args, null, patch);
                ProcessApplyOutput(output, patch);
            }
        }

        private void ProcessApplyOutput(string output, byte[] patch)
        {
            if (!string.IsNullOrEmpty(output))
            {
                MessageBox.Show(this, output + "\n\n" + SelectedDiff.Encoding.GetString(patch));
            }
            if (_currentItemStaged)
                Staged.StoreNextIndexToSelect();
            else
                Unstaged.StoreNextIndexToSelect();
            ScheduleGoToLine();
            _selectedDiffReloaded = false;
            RescanChanges();
        }

        private void ScheduleGoToLine()
        {
            int selectedDifflineToSelect = SelectedDiff.GetText().Substring(0, SelectedDiff.GetSelectionPosition()).Count(c => c == '\n');
            int scrollPosition = SelectedDiff.ScrollPos;
            string selectedFileName = _currentItem.Name;
            Action stageAreaLoaded = null;
            stageAreaLoaded = () =>
            {
                EventHandler textLoaded = null;
                textLoaded = (a, b) =>
                    {
                        if (_currentItem != null && _currentItem.Name.Equals(selectedFileName))
                        {
                            SelectedDiff.GoToLine(selectedDifflineToSelect);
                            SelectedDiff.ScrollPos = scrollPosition;
                        }
                        SelectedDiff.TextLoaded -= textLoaded;
                        _selectedDiffReloaded = true;
                    };
                SelectedDiff.TextLoaded += textLoaded;
                OnStageAreaLoaded -= stageAreaLoaded;
            };

            OnStageAreaLoaded += stageAreaLoaded;
        }

        private void ResetSelectedLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            //to prevent multiple clicks
            if (!_selectedDiffReloaded)
                return;

            if (MessageBox.Show(this, _resetSelectedLinesConfirmation.Text, _resetChangesCaption.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            Debug.Assert(_currentItem != null);
            // Prepare git command
            string args = "apply --whitespace=nowarn";

            if (_currentItemStaged) //staged
                args += " --reverse --index";

            byte[] patch;

            if (_currentItemStaged)
                patch = PatchManager.GetSelectedLinesAsPatch(Module, SelectedDiff.GetText(),
                    SelectedDiff.GetSelectionPosition(), SelectedDiff.GetSelectionLength(),
                    _currentItemStaged, SelectedDiff.Encoding, _currentItem.IsNew);
            else if (_currentItem.IsNew)
                patch = PatchManager.GetSelectedLinesAsNewPatch(Module, _currentItem.Name,
                    SelectedDiff.GetText(), SelectedDiff.GetSelectionPosition(), SelectedDiff.GetSelectionLength(),
                    SelectedDiff.Encoding, true, SelectedDiff.FilePreabmle);
            else
                patch = PatchManager.GetResetUnstagedLinesAsPatch(Module, SelectedDiff.GetText(),
                    SelectedDiff.GetSelectionPosition(), SelectedDiff.GetSelectionLength(),
                    _currentItemStaged, SelectedDiff.Encoding);

            if (patch != null && patch.Length > 0)
            {
                string output = Module.RunGitCmd(args, null, patch);
                if (EnvUtils.RunningOnWindows())
                {
                    //remove file mode warnings on windows
                    Regex regEx = new Regex("warning: .*has type .* expected .*", RegexOptions.Compiled);
                    output = output.RemoveLines(regEx.IsMatch);
                }
                ProcessApplyOutput(output, patch);
            }
        }

        private void EnableStageButtons(bool enable)
        {
            toolUnstageItem.Enabled = enable;
            toolUnstageAllItem.Enabled = enable;
            toolStageItem.Enabled = enable;
            toolStageAllItem.Enabled = enable;
            workingToolStripMenuItem.Enabled = enable;
            ResetUnStaged.Enabled = Unstaged.AllItems.Any();
        }

        private bool _initialized;

        private void Initialize(bool loadUnstaged)
        {
            _initialized = true;

            Task.Factory.StartNew(() => string.Format(_formTitle.Text, Module.GetSelectedBranch(),
                                      Module.WorkingDir))
                .ContinueWith(task => Text = task.Result, _taskScheduler);

            Cursor.Current = Cursors.WaitCursor;

            if (loadUnstaged)
            {
                Loading.Visible = true;
                LoadingStaged.Visible = true;

                Commit.Enabled = false;
                CommitAndPush.Enabled = false;
                Amend.Enabled = false;
                Reset.Enabled = false;
                ResetUnStaged.Enabled = false;
                EnableStageButtons(false);

                ComputeUnstagedFiles(LoadUnstagedOutput, true);
            }

            UpdateMergeHead();

            Message.TextBoxFont = AppSettings.CommitFont;

            // Check if commit.template is used
            string fileName = Module.GetEffectivePathSetting("commit.template");
            if (!string.IsNullOrEmpty(fileName))
            {
                using (var commitReader = new StreamReader(fileName))
                {
                    _commitTemplate = commitReader.ReadToEnd().Replace("\r", "");
                }
                Message.Text = _commitTemplate;
            }

            Cursor.Current = Cursors.Default;
        }

        private void Initialize()
        {
            Initialize(true);
        }

        private void UpdateMergeHead()
        {
            var mergeHead = Module.RevParse("MERGE_HEAD");
            IsMergeCommit = Regex.IsMatch(mergeHead, GitRevision.Sha1HashPattern);
        }

        private void InitializedStaged()
        {
            Cursor.Current = Cursors.WaitCursor;
            SolveMergeconflicts.Visible = Module.InTheMiddleOfConflictedMerge();
            Staged.GitItemStatuses = Module.GetStagedFilesWithSubmodulesStatus();
            Cursor.Current = Cursors.Default;
        }

        private event Action OnStageAreaLoaded;

        private bool _loadUnstagedOutputFirstTime = true;

        /// <summary>
        ///   Loads the unstaged output.
        ///   This method is passed in to the SetTextCallBack delegate
        ///   to set the Text property of textBox1.
        /// </summary>
        private void LoadUnstagedOutput(IList<GitItemStatus> allChangedFiles)
        {
            var lastSelection = new List<GitItemStatus>();
            if (_currentFilesList != null)
                lastSelection = _currentSelection;

            var unStagedFiles = new List<GitItemStatus>();
            var stagedFiles = new List<GitItemStatus>();

            foreach (var fileStatus in allChangedFiles)
            {
                if (fileStatus.IsStaged)
                    stagedFiles.Add(fileStatus);
                else
                    unStagedFiles.Add(fileStatus);
            }
            Unstaged.GitItemStatuses = unStagedFiles;
            Staged.GitItemStatuses = stagedFiles;

            Loading.Visible = false;
            LoadingStaged.Visible = false;
            Commit.Enabled = true;
            CommitAndPush.Enabled = true;
            Amend.Enabled = true;
            Reset.Enabled = DoChangesExist();

            EnableStageButtons(true);
            workingToolStripMenuItem.Enabled = true;

            var inTheMiddleOfConflictedMerge = Module.InTheMiddleOfConflictedMerge();
            SolveMergeconflicts.Visible = inTheMiddleOfConflictedMerge;

            if (Staged.IsEmpty)
            {
                _currentFilesList = Unstaged;
                if (Staged.ContainsFocus)
                    Unstaged.Focus();
            }
            else if (Unstaged.IsEmpty)
            {
                _currentFilesList = Staged;
                if (Unstaged.ContainsFocus)
                    Staged.Focus();
            }

            RestoreSelectedFiles(unStagedFiles, stagedFiles, lastSelection);

            if (OnStageAreaLoaded != null)
                OnStageAreaLoaded();

            if (_loadUnstagedOutputFirstTime)
            {
                var fc = this.FindFocusedControl();

                if (fc == this.Ok)
                {
                    if (Unstaged.GitItemStatuses.Any())
                        Unstaged.Focus();
                    else if (Staged.GitItemStatuses.Any())
                        Message.Focus();
                    else
                        Amend.Focus();
                }

                _loadUnstagedOutputFirstTime = false;
            }
        }

        private void SelectStoredNextIndex()
        {
            Unstaged.SelectStoredNextIndex(0);
            if (Unstaged.GitItemStatuses.Any())
            {
                Staged.SelectStoredNextIndex();
            }
            else
            {
                Staged.SelectStoredNextIndex(0);
            }
        }

        private void RestoreSelectedFiles(IList<GitItemStatus> unStagedFiles, IList<GitItemStatus> stagedFiles, IList<GitItemStatus> lastSelection)
        {
            if (_currentFilesList == null || _currentFilesList.IsEmpty)
            {
                SelectStoredNextIndex();
                return;
            }

            var newItems = _currentFilesList == Staged ? stagedFiles : unStagedFiles;
            var names = lastSelection.ToHashSet(x => x.Name);
            var newSelection = newItems.Where(x => names.Contains(x.Name)).ToList();

            if (newSelection.Any())
                _currentFilesList.SelectedItems = newSelection;
            else
                SelectStoredNextIndex();
        }

        /// <summary>Returns if there are any changes at all, staged or unstaged.</summary>
        private bool DoChangesExist()
        {
            return Unstaged.AllItems.Any() || Staged.AllItems.Any();
        }

        private void ShowChanges(GitItemStatus item, bool staged)
        {
            _currentItem = item;
            _currentItemStaged = staged;

            if (item == null)
                return;

            long length = GetItemLength(item.Name);
            if (length < 5 * 1024 * 1024) // 5Mb
                SetSelectedDiff(item, staged);
            else
            {
                SelectedDiff.Clear();
                SelectedDiff.Refresh();
                llShowPreview.Show();
            }

            _stageSelectedLinesToolStripMenuItem.Text = staged ? _unstageSelectedLines.Text : _stageSelectedLines.Text;
            _stageSelectedLinesToolStripMenuItem.Image = staged ? toolUnstageItem.Image : toolStageItem.Image;
            _stageSelectedLinesToolStripMenuItem.ShortcutKeyDisplayString =
                GetShortcutKeys((int)(staged ? Commands.UnStageSelectedFile : Commands.StageSelectedFile)).ToShortcutKeyDisplayString();
        }

        private long GetItemLength(string fileName)
        {
            long length = -1;
            string path = fileName;
            if (!File.Exists(fileName))
                path = Path.Combine(Module.WorkingDir, fileName);
            if (File.Exists(path))
            {
                FileInfo fi = new FileInfo(path);
                length = fi.Length;
            }
            return length;
        }

        private void llShowPreview_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            llShowPreview.Hide();
            SetSelectedDiff(_currentItem, _currentItemStaged);
        }

        private void SetSelectedDiff(GitItemStatus item, bool staged)
        {
            if (item.Name.EndsWith(".png"))
            {
                SelectedDiff.ViewFile(item.Name);
            }
            else if (item.IsTracked)
            {
                SelectedDiff.ViewCurrentChanges(item, staged);
            }
            else
            {
                SelectedDiff.ViewFile(item.Name);
            }
        }

        private List<GitItemStatus> _currentSelection;
        private void ClearDiffViewIfNoFilesLeft()
        {
            llShowPreview.Hide();
            if (Staged.IsEmpty && Unstaged.IsEmpty)
                SelectedDiff.Clear();
        }

        private void CommitClick(object sender, EventArgs e)
        {
            ExecuteCommitCommand();
        }

        private void CheckForStagedAndCommit(bool amend, bool push)
        {
            if (amend)
            {
                // This is an amend commit.  Confirm the user understands the implications.  We don't want to prompt for an empty
                // commit, because amend may be used just to change the commit message or timestamp.
                if (!AppSettings.DontConfirmAmend)
                    if (MessageBox.Show(this, _amendCommit.Text, _amendCommitCaption.Text, MessageBoxButtons.YesNo) != DialogResult.Yes)
                        return;
            }
            else if (Staged.IsEmpty)
            {
                if (IsMergeCommit)
                {
                    // it is a merge commit, so user can commit just for merging two branches even the changeset is empty,
                    // but also user may forget to add files, so only ask for confirmation that user really wants to commit an empty changeset
                    if (MessageBox.Show(this, _noFilesStagedAndConfirmAnEmptyMergeCommit.Text, _noStagedChanges.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                }
                else
                {
                    if (Unstaged.IsEmpty)
                    {
                        MessageBox.Show(this, _noFilesStagedAndNothingToCommit.Text, _noStagedChanges.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    // there are no staged files, but there are unstaged files. Most probably user forgot to stage them.
                    if (MessageBox.Show(this, _noFilesStagedButSuggestToCommitAllUnstaged.Text, _noStagedChanges.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                    StageAllAccordingToFilter();
                    // if staging failed (i.e. line endings conflict), user already got error message, don't try to commit empty changeset.
                    if (Staged.IsEmpty)
                        return;
                }
            }

            DoCommit(amend, push);
        }

        private void DoCommit(bool amend, bool push)
        {
            if (Module.InTheMiddleOfConflictedMerge())
            {
                MessageBox.Show(this, _mergeConflicts.Text, _mergeConflictsCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (_useFormCommitMessage && (string.IsNullOrEmpty(Message.Text) || Message.Text == _commitTemplate))
            {
                MessageBox.Show(this, _enterCommitMessage.Text, _enterCommitMessageCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }

            if (_useFormCommitMessage && !ValidCommitMessage())
                return;

            if (Module.IsDetachedHead() && !Module.InTheMiddleOfRebase())
            {
                int idx = PSTaskDialog.cTaskDialog.ShowCommandBox(this,
                                                        _notOnBranchCaption.Text,
                                                        _notOnBranchMainInstruction.Text,
                                                        _notOnBranch.Text,
                                                        _notOnBranchButtons.Text,
                                                        true);
                switch (idx)
                {
                    case 0:
                        string revision = _editedCommit != null ? _editedCommit.Guid : "";
                        if (!UICommands.StartCheckoutBranch(this, new[] {revision}))
                            return;
                        break;
                    case 1:
                        if (!UICommands.StartCreateBranchDialog(this, _editedCommit))
                            return;
                        break;
                    case -1:
                        return;
                }
            }

            try
            {
                if (_useFormCommitMessage)
                {
                    SetCommitMessageFromTextBox(Message.Text);
                }

                ScriptManager.RunEventScripts(this, ScriptEvent.BeforeCommit);

                var errorOccurred = !FormProcess.ShowDialog(this, Module.CommitCmd(amend, signOffToolStripMenuItem.Checked, toolAuthor.Text, _useFormCommitMessage));

                UICommands.RepoChangedNotifier.Notify();

                if (errorOccurred)
                    return;

                Amend.Checked = false;

                ScriptManager.RunEventScripts(this, ScriptEvent.AfterCommit);

                Message.Text = string.Empty;
                CommitHelper.SetCommitMessage(Module, string.Empty);

                bool pushCompleted = true;
                if (push)
                {
                    UICommands.StartPushDialog(this, true, out pushCompleted);
                }

                if (pushCompleted && Module.SuperprojectModule != null && AppSettings.StageInSuperprojectAfterCommit)
                    Module.SuperprojectModule.StageFile(Module.SubmodulePath);

                if (AppSettings.CloseCommitDialogAfterCommit)
                {
                    Close();
                    return;
                }

                if (Unstaged.GitItemStatuses.Any())
                {
                    InitializedStaged();
                    return;
                }

                if (AppSettings.CloseCommitDialogAfterLastCommit)
                    Close();
                else
                    InitializedStaged();
            }
            catch (Exception e)
            {
                MessageBox.Show(this, string.Format("Exception: {0}", e.Message));
            }
        }

        private bool ValidCommitMessage()
        {
            if (AppSettings.CommitValidationMaxCntCharsFirstLine > 0)
            {
                var firstLine = Message.Text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (firstLine.Length > AppSettings.CommitValidationMaxCntCharsFirstLine)
                {
                    if (DialogResult.No == MessageBox.Show(this, _commitMsgFirstLineInvalid.Text, _commitValidationCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk))
                        return false;
                }
            }

            if (AppSettings.CommitValidationMaxCntCharsPerLine > 0)
            {
                var lines = Message.Text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.Length > AppSettings.CommitValidationMaxCntCharsPerLine)
                    {
                        if (DialogResult.No == MessageBox.Show(this, String.Format(_commitMsgLineInvalid.Text, line), _commitValidationCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk))
                            return false;
                    }
                }
            }

            if (AppSettings.CommitValidationSecondLineMustBeEmpty)
            {
                var lines = Message.Text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                if (lines.Length > 2)
                {
                    if (lines[1].Length != 0)
                    {
                        if (DialogResult.No == MessageBox.Show(this, _commitMsgSecondLineNotEmpty.Text, _commitValidationCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk))
                            return false;
                    }
                }
            }

            if (!AppSettings.CommitValidationRegEx.IsNullOrEmpty())
            {
                try
                {
                    if (!Regex.IsMatch(Message.Text, AppSettings.CommitValidationRegEx))
                    {
                        if (DialogResult.No == MessageBox.Show(this, _commitMsgRegExNotMatched.Text, _commitValidationCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk))
                            return false;

                    }
                }
                catch
                {
                }
            }

            return true;
        }

        private void RescanChanges()
        {
            if (_shouldRescanChanges)
            {
                toolRefreshItem.Enabled = false;
                Initialize();
                toolRefreshItem.Enabled = true;
            }
        }

        private void UnstageFilesClick(object sender, EventArgs e)
        {
            if (_currentFilesList != Staged)
                return;
            Unstage();
        }

        void Staged_DoubleClick(object sender, EventArgs e)
        {
            _currentFilesList = Staged;
            Unstage();
        }

        private void UnstageAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            var lastSelection = new List<GitItemStatus>();
            if (_currentFilesList != null)
                lastSelection = _currentSelection;

            Action stageAreaLoaded = null;
            stageAreaLoaded = () =>
            {
                _currentFilesList = Unstaged;
                RestoreSelectedFiles(Unstaged.GitItemStatuses, Staged.GitItemStatuses, lastSelection);
                Unstaged.Focus();

                OnStageAreaLoaded -= stageAreaLoaded;
            };

            OnStageAreaLoaded += stageAreaLoaded;

            Module.ResetMixed("HEAD");
            Initialize();
        }

        private void UnstagedSelectionChanged(object sender, EventArgs e)
        {
            if (_currentFilesList != Unstaged || _skipUpdate)
                return;

            ClearDiffViewIfNoFilesLeft();

            Unstaged.ContextMenuStrip = null;

            if (!Unstaged.SelectedItems.Any())
                return;

            Staged.ClearSelected();

            _currentSelection = Unstaged.SelectedItems.ToList();
            GitItemStatus item = _currentSelection.FirstOrDefault();
            ShowChanges(item, false);

            if (!item.IsSubmodule)
                Unstaged.ContextMenuStrip = UnstagedFileContext;
            else
                Unstaged.ContextMenuStrip = UnstagedSubmoduleContext;
        }

        private void Unstaged_Enter(object sender, EventArgs e)
        {
            if (_currentFilesList != Unstaged)
            {
                _currentFilesList = Unstaged;
                _skipUpdate = false;
                UnstagedSelectionChanged(Unstaged, null);
            }
        }

        private void Unstage()
        {
            if (Staged.GitItemStatuses.Count() > 10 && Staged.SelectedItems.Count() == Staged.GitItemStatuses.Count())
            {
                UnstageAllToolStripMenuItemClick(null, null);
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            EnableStageButtons(false);
            try
            {
                var lastSelection = new List<GitItemStatus>();
                if (_currentFilesList != null)
                    lastSelection = _currentSelection;

                    toolStripProgressBar1.Visible = true;
                    toolStripProgressBar1.Maximum = Staged.SelectedItems.Count() * 2;
                    toolStripProgressBar1.Value = 0;
                    Staged.StoreNextIndexToSelect();

                    var files = new List<GitItemStatus>();
                    var allFiles = new List<GitItemStatus>();

                    foreach (var item in Staged.SelectedItems)
                    {
                        toolStripProgressBar1.Value = Math.Min(toolStripProgressBar1.Maximum - 1, toolStripProgressBar1.Value + 1);
                        if (!item.IsNew)
                        {
                            toolStripProgressBar1.Value = Math.Min(toolStripProgressBar1.Maximum - 1, toolStripProgressBar1.Value + 1);
                            Module.UnstageFileToRemove(item.Name);

                            if (item.IsRenamed)
                                Module.UnstageFileToRemove(item.OldName);
                        }
                        else
                        {
                            files.Add(item);
                        }
                        allFiles.Add(item);
                    }

                    Module.UnstageFiles(files);

                    _skipUpdate = true;
                    InitializedStaged();
                    var stagedFiles = Staged.GitItemStatuses.ToList();
                    var unStagedFiles = Unstaged.GitItemStatuses.ToList();
                    foreach (var item in allFiles)
                    {
                        var item1 = item;
                        if (stagedFiles.Exists(i => i.Name == item1.Name))
                            continue;

                        var item2 = item;
                        if (unStagedFiles.Exists(i => i.Name == item2.Name))
                            continue;

                        if (item.IsNew && !item.IsChanged && !item.IsDeleted)
                            item.IsTracked = false;
                        else
                            item.IsTracked = true;

                        if (item.IsRenamed)
                        {
                            var clone = new GitItemStatus
                            {
                                Name = item.OldName,
                                IsDeleted = true,
                                IsTracked = true,
                                IsStaged = false
                            };
                            unStagedFiles.Add(clone);

                            item.IsRenamed = false;
                            item.IsNew = true;
                            item.IsTracked = false;
                            item.OldName = string.Empty;
                        }

                        item.IsStaged = false;
                        unStagedFiles.Add(item);
                    }
                    Staged.GitItemStatuses = stagedFiles;
                    Unstaged.GitItemStatuses = unStagedFiles;
                    _skipUpdate = false;
                    Staged.SelectStoredNextIndex();

                    toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;

                toolStripProgressBar1.Visible = false;

                if (Staged.IsEmpty)
                {
                    _currentFilesList = Unstaged;
                    RestoreSelectedFiles(Unstaged.GitItemStatuses, Staged.GitItemStatuses, lastSelection);
                    Unstaged.Focus();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            EnableStageButtons(true);
            Cursor.Current = Cursors.Default;

            if (AppSettings.RevisionGraphShowWorkingDirChanges)
                UICommands.RepoChangedNotifier.Notify();
        }

        private void StageClick(object sender, EventArgs e)
        {
            if (_currentFilesList != Unstaged)
                return;
            Stage(Unstaged.SelectedItems.ToList());
            if (Unstaged.IsEmpty)
                Message.Focus();
        }

        void Unstaged_DoubleClick(object sender, EventArgs e)
        {
            _currentFilesList = Unstaged;
            Stage(Unstaged.SelectedItems.ToList());
            if (Unstaged.IsEmpty)
                Message.Focus();
        }

        private void StageAllAccordingToFilter()
        {
            Stage(Unstaged.GitItemFilteredStatuses);
            Unstaged.SetFilter(String.Empty);
            if (Unstaged.IsEmpty)
                Message.Focus();
            else
                Staged.Focus();
        }

        private void StageAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            StageAllAccordingToFilter();
        }

        private void StagedSelectionChanged(object sender, EventArgs e)
        {
            if (_currentFilesList != Staged || _skipUpdate)
                return;

            ClearDiffViewIfNoFilesLeft();

            if (!Staged.SelectedItems.Any())
                return;

            Unstaged.ClearSelected();
            _currentSelection = Staged.SelectedItems.ToList();
            GitItemStatus item = _currentSelection.FirstOrDefault();
            ShowChanges(item, true);
        }

        private void Staged_Enter(object sender, EventArgs e)
        {
            if (_currentFilesList != Staged)
            {
                _currentFilesList = Staged;
                _skipUpdate = false;
                StagedSelectionChanged(Staged, null);
            }
        }

        private void Stage(IList<GitItemStatus> gitItemStatuses)
        {
            EnableStageButtons(false);
            try
            {
                var lastSelection = new List<GitItemStatus>();
                if (_currentFilesList != null)
                    lastSelection = _currentSelection;

                Cursor.Current = Cursors.WaitCursor;
                Unstaged.StoreNextIndexToSelect();
                toolStripProgressBar1.Visible = true;
                toolStripProgressBar1.Maximum = gitItemStatuses.Count() * 2;
                toolStripProgressBar1.Value = 0;

                var files = new List<GitItemStatus>();

                foreach (var gitItemStatus in gitItemStatuses)
                {
                    toolStripProgressBar1.Value = Math.Min(toolStripProgressBar1.Maximum - 1, toolStripProgressBar1.Value + 1);
                    if (gitItemStatus.Name.EndsWith("/"))
                        gitItemStatus.Name = gitItemStatus.Name.TrimEnd('/');
                    files.Add(gitItemStatus);
                }

                bool wereErrors = false;
                if (AppSettings.ShowErrorsWhenStagingFiles)
                {
                    FormStatus.ProcessStart processStart =
                        form =>
                        {
                            form.AppendMessageCrossThread(string.Format(_stageFiles.Text + "\n",
                                                         files.Count));
                            var output = Module.StageFiles(files, out wereErrors);
                            form.AppendMessageCrossThread(output);
                            form.Done(string.IsNullOrEmpty(output));
                        };
                    using (var process = new FormStatus(processStart, null) { Text = _stageDetails.Text })
                        process.ShowDialogOnError(this);
                }
                else
                {
                    Module.StageFiles(files, out wereErrors);
                }
                if (wereErrors)
                    RescanChanges();
                else
                {
                    InitializedStaged();
                    var unStagedFiles = Unstaged.GitItemStatuses.ToList();
                    _skipUpdate = true;
                    var names = new HashSet<string>();
                    foreach (var item in files)
                    {
                        names.Add(item.Name);
                        names.Add(item.OldName);
                    }
                    var unstagedItems = new HashSet<GitItemStatus>();
                    foreach (var item in unStagedFiles)
                    {
                        if (names.Contains(item.Name))
                            unstagedItems.Add(item);
                    }
                    unStagedFiles.RemoveAll(item => !item.IsSubmodule && unstagedItems.Contains(item));
                    unStagedFiles.RemoveAll(item => item.IsSubmodule && item.SubmoduleStatus.IsCompleted &&
                        !item.SubmoduleStatus.Result.IsDirty && unstagedItems.Contains(item));
                    foreach (var item in unstagedItems.Where(item => item.IsSubmodule &&
                        item.SubmoduleStatus.IsCompleted && item.SubmoduleStatus.Result.IsDirty))
                    {
                        item.SubmoduleStatus.Result.Status = SubmoduleStatus.Unknown;
                    }
                    Unstaged.GitItemStatuses = unStagedFiles;
                    Unstaged.ClearSelected();
                    _skipUpdate = false;
                    Unstaged.SelectStoredNextIndex();
                }

                toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;

                toolStripProgressBar1.Visible = false;

                if (Unstaged.IsEmpty)
                {
                    _currentFilesList = Staged;
                    RestoreSelectedFiles(Unstaged.GitItemStatuses, Staged.GitItemStatuses, lastSelection);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            EnableStageButtons(true);

            Commit.Enabled = true;
            Amend.Enabled = true;
            AcceptButton = Commit;
            Cursor.Current = Cursors.Default;

            if (AppSettings.RevisionGraphShowWorkingDirChanges)
                UICommands.RepoChangedNotifier.Notify();
        }

        private void ResetSoftClick(object sender, EventArgs e)
        {
            _shouldRescanChanges = false;
            try
            {
                if (_currentFilesList == null || _currentFilesList.SelectedItems.Count() == 0)
                {
                    return;
                }

                // Show a form asking the user if they want to reset the changes.
                FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(this, _currentFilesList.SelectedItems.Any(item => !item.IsNew), _currentFilesList.SelectedItems.Any(item => item.IsNew));
                if (resetType == FormResetChanges.ActionEnum.Cancel)
                    return;

                // Unstage file first, then reset
                var files = new List<GitItemStatus>();

                foreach (var item in Staged.SelectedItems)
                {
                    toolStripProgressBar1.Value = Math.Min(toolStripProgressBar1.Maximum - 1, toolStripProgressBar1.Value + 1);
                    if (!item.IsNew)
                    {
                        toolStripProgressBar1.Value = Math.Min(toolStripProgressBar1.Maximum - 1, toolStripProgressBar1.Value + 1);
                        Module.UnstageFileToRemove(item.Name);

                        if (item.IsRenamed)
                            Module.UnstageFileToRemove(item.OldName);
                    }
                    else
                    {
                        files.Add(item);
                    }
                }

                Module.UnstageFiles(files);

                //remember max selected index
                _currentFilesList.StoreNextIndexToSelect();

                var deleteNewFiles = _currentFilesList.SelectedItems.Any(item => item.IsNew) && (resetType == FormResetChanges.ActionEnum.ResetAndDelete);
                var filesInUse = new List<string>();
                var output = new StringBuilder();
                foreach (var item in _currentFilesList.SelectedItems)
                {
                    if (item.IsNew)
                    {
                        if (deleteNewFiles)
                        {
                            try
                            {
                                string path = Path.Combine(Module.WorkingDir, item.Name);
                                if (File.Exists(path))
                                    File.Delete(path);
                                else
                                    Directory.Delete(path, true);
                            }
                            catch (System.IO.IOException)
                            {
                                filesInUse.Add(item.Name);
                            }
                            catch (System.UnauthorizedAccessException)
                            {
                            }
                        }
                    }
                    else
                    {
                        output.Append(Module.ResetFile(item.Name));
                    }
                }

                if (AppSettings.RevisionGraphShowWorkingDirChanges)
                    UICommands.RepoChangedNotifier.Notify();

                if (filesInUse.Count > 0)
                    MessageBox.Show(this, "The following files are currently in use and will not be reset:" + Environment.NewLine + "\u2022 " + string.Join(Environment.NewLine + "\u2022 ", filesInUse), "Files In Use");

                if (!string.IsNullOrEmpty(output.ToString()))
                    MessageBox.Show(this, output.ToString(), _resetChangesCaption.Text);
            }
            finally
            {
                _shouldRescanChanges = true;
            }
            Initialize();
        }

        private void DeleteFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                SelectedDiff.Clear();
                if (Unstaged.SelectedItem == null ||
                    MessageBox.Show(this, _deleteSelectedFiles.Text, _deleteSelectedFilesCaption.Text, MessageBoxButtons.YesNo) !=
                    DialogResult.Yes)
                    return;
                Unstaged.StoreNextIndexToSelect();
                foreach (var item in Unstaged.SelectedItems)
                    File.Delete(Path.Combine(Module.WorkingDir, item.Name));

                Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _deleteFailed.Text + Environment.NewLine + ex.Message);
            }
        }

        private void SolveMergeConflictsClick(object sender, EventArgs e)
        {
            if (UICommands.StartResolveConflictsDialog(this, false))
                Initialize();
        }

        private void DeleteSelectedFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, _deleteSelectedFiles.Text, _deleteSelectedFilesCaption.Text, MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
                return;

            try
            {
                foreach (var gitItemStatus in Unstaged.SelectedItems)
                    File.Delete(Path.Combine(Module.WorkingDir, gitItemStatus.Name));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _deleteFailed.Text + Environment.NewLine + ex);
            }
            Initialize();
        }

        private void ResetSelectedFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, _resetSelectedChangesText.Text, _resetChangesCaption.Text, MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
                return;

            foreach (var gitItemStatus in Unstaged.SelectedItems)
            {
                Module.ResetFile(gitItemStatus.Name);
            }
            Initialize();
        }

        private void ResetAlltrackedChangesToolStripMenuItemClick(object sender, EventArgs e)
        {
            ResetClick(null, null);
        }

        private void resetUnstagedChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
          ResetUnStagedClick(null, null);
        }

        private void EditGitIgnoreToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartEditGitIgnoreDialog(this);
            Initialize();
        }

        private void FormCommitShown(object sender, EventArgs e)
        {
            if (!_initialized)
                Initialize();

            AcceptButton = Commit;

            string message;

            switch (_commitKind)
            {
                case CommitKind.Fixup:
                    message = string.Format("fixup! {0}", _editedCommit.Subject);
                    break;
                case CommitKind.Squash:
                    message = string.Format("squash! {0}", _editedCommit.Subject);
                    break;
                default:
                    message = Module.GetMergeMessage();

                    if (string.IsNullOrEmpty(message) && File.Exists(GitCommands.CommitHelper.GetCommitMessagePath(Module)))
                        message = File.ReadAllText(GitCommands.CommitHelper.GetCommitMessagePath(Module), Module.CommitEncoding);
                    break;
            }

            if (_useFormCommitMessage && !string.IsNullOrEmpty(message))
                Message.Text = message;
        }

        private void SetCommitMessageFromTextBox(string commitMessageText)
        {
            //Save last commit message in settings. This way it can be used in multiple repositories.
            AppSettings.LastCommitMessage = commitMessageText;

            var path = Path.Combine(Module.GetGitDirectory(), "COMMITMESSAGE");

            //Commit messages are UTF-8 by default unless otherwise in the config file.
            //The git manual states:
            //  git commit and git commit-tree issues a warning if the commit log message
            //  given to it does not look like a valid UTF-8 string, unless you
            //  explicitly say your project uses a legacy encoding. The way to say
            //  this is to have i18n.commitencoding in .git/config file, like this:...
            Encoding encoding = Module.CommitEncoding;

            using (var textWriter = new StreamWriter(path, false, encoding))
            {
                var addNewlineToCommitMessageWhenMissing = AppSettings.AddNewlineToCommitMessageWhenMissing;

                var lineNumber = 0;
                foreach (var line in commitMessageText.Split('\n'))
                {
                    //When a committemplate is used, skip comments
                    //otherwise: "#" is probably not used for comment but for issue number
                    if (!line.StartsWith("#") ||
                        string.IsNullOrEmpty(_commitTemplate))
                    {
                        if (addNewlineToCommitMessageWhenMissing)
                        {
                            if (lineNumber == 1 && !String.IsNullOrEmpty(line))
                                textWriter.WriteLine();
                        }

                        textWriter.WriteLine(line);
                    }
                    lineNumber++;
                }
            }
        }

        private void DeleteAllUntrackedFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(this,
                _deleteUntrackedFiles.Text,
                _deleteUntrackedFilesCaption.Text,
                MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
                return;
            FormProcess.ShowDialog(this, "clean -f");
            Initialize();
        }

        private void ShowIgnoredFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            showIgnoredFilesToolStripMenuItem.Checked = !showIgnoredFilesToolStripMenuItem.Checked;
            RescanChanges();
        }

        private void ShowAssumeUnchangedFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            showAssumeUnchangedFilesToolStripMenuItem.Checked = !showAssumeUnchangedFilesToolStripMenuItem.Checked;
            doNotAssumeUnchangedToolStripMenuItem.Visible = showAssumeUnchangedFilesToolStripMenuItem.Checked;
            RescanChanges();
        }

        private void CommitMessageToolStripMenuItemDropDownOpening(object sender, EventArgs e)
        {
            commitMessageToolStripMenuItem.DropDownItems.Clear();

            var msg = AppSettings.LastCommitMessage;

            var prevMsgs = Module.GetPreviousCommitMessages(AppSettings.CommitDialogNumberOfPreviousMessages);

            if (!prevMsgs.Contains(msg))
            {
                prevMsgs = new string[] { msg }.Concat(prevMsgs).Take(AppSettings.CommitDialogNumberOfPreviousMessages);
            }

            foreach (var localLastCommitMessage in prevMsgs)
            {
                AddCommitMessageToMenu(localLastCommitMessage);
            }

            commitMessageToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                toolStripMenuItem1,
                generateListOfChangesInSubmodulesChangesToolStripMenuItem
            });
        }

        private void AddCommitMessageToMenu(string commitMessage)
        {
            if (string.IsNullOrEmpty(commitMessage))
                return;

            var toolStripItem =
                new ToolStripMenuItem
                {
                    Tag = commitMessage,
                    Text =
                        commitMessage.Substring(0,
                                                Math.Min(Math.Min(50, commitMessage.Length),
                                                         commitMessage.Contains("\n") ? commitMessage.IndexOf('\n') : 99)) +
                        "..."
                };

            commitMessageToolStripMenuItem.DropDownItems.Add(toolStripItem);
        }

        private void CommitMessageToolStripMenuItemDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Tag != null)
                Message.Text = ((string)e.ClickedItem.Tag).Trim();
        }

        private void generateListOfChangesInSubmodulesChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var stagedFiles = Staged.AllItems;

            Dictionary<string, string> modules = stagedFiles.Where(it => it.IsSubmodule).
                Select(item => item.Name).ToDictionary(item => Module.GetSubmoduleNameByPath(item));
            if (modules.Count == 0)
                return;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Submodule" + (modules.Count == 1 ? " " : "s ") +
                String.Join(", ", modules.Keys) + " updated");
            sb.AppendLine();
            foreach (var item in modules)
            {
                string diff = Module.RunGitCmd(
                     string.Format("diff --cached -z -- {0}", item.Value));
                var lines = diff.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                const string subprojCommit = "Subproject commit ";
                var from = lines.Single(s => s.StartsWith("-" + subprojCommit)).Substring(subprojCommit.Length + 1);
                var to = lines.Single(s => s.StartsWith("+" + subprojCommit)).Substring(subprojCommit.Length + 1);
                if (!String.IsNullOrEmpty(from) && !String.IsNullOrEmpty(to))
                {
                    sb.AppendLine("Submodule " + item.Key + ":");
                    GitModule module = new GitModule(Module.WorkingDir + item.Value.EnsureTrailingPathSeparator());
                    string log = module.RunGitCmd(
                         string.Format("log --pretty=format:\"    %m %h - %s\" --no-merges {0}...{1}", from, to));
                    if (log.Length != 0)
                        sb.AppendLine(log);
                    else
                        sb.AppendLine("    * Revision changed to " + to.Substring(0, 7));
                    sb.AppendLine();
                }
            }
            Message.Text = sb.ToString().TrimEnd();
        }

        private void AddFileTogitignoreToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!Unstaged.SelectedItems.Any())
                return;

            SelectedDiff.Clear();
            var fileNames = Unstaged.SelectedItems.Select(item => item.Name).ToArray();
            if (UICommands.StartAddToGitIgnoreDialog(this, fileNames))
                Initialize();
        }

        private void AssumeUnchangedToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!Unstaged.SelectedItems.Any())
                return;

            SelectedDiff.Clear();

            bool wereErrors;
            Module.AssumeUnchangedFiles(Unstaged.SelectedItems.ToList(), true, out wereErrors);

            Initialize();
        }

        private void DoNotAssumeUnchangedToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!Unstaged.SelectedItems.Any())
                return;

            SelectedDiff.Clear();

            bool wereErrors;
            Module.AssumeUnchangedFiles(Unstaged.SelectedItems.ToList(), false, out wereErrors);

            Initialize();
        }

        private void SelectedDiffExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ShowChanges(_currentItem, _currentItemStaged);
        }

        private void RescanChangesToolStripMenuItemClick(object sender, EventArgs e)
        {
            RescanChanges();
        }

        private void OpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            FileStatusList list = sender as FileStatusList;
            if (!SenderToFileStatusList(sender, out list))
                return;

            if (!list.SelectedItems.Any())
                return;

            var item = list.SelectedItem;
            var fileName = item.Name;

            Process.Start((Path.Combine(Module.WorkingDir, fileName)).ToNativePath());
        }

        private void OpenWithToolStripMenuItemClick(object sender, EventArgs e)
        {
            FileStatusList list;
            if (!SenderToFileStatusList(sender, out list))
                return;

            if (!list.SelectedItems.Any())
                return;

            var item = list.SelectedItem;
            var fileName = item.Name;

            OsShellUtil.OpenAs(Module.WorkingDir + fileName.ToNativePath());
        }

        private void FilenameToClipboardToolStripMenuItemClick(object sender, EventArgs e)
        {
            FileStatusList list;
            if (!SenderToFileStatusList(sender, out list))
                return;

            if (!list.SelectedItems.Any())
                return;

            var fileNames = new StringBuilder();
            foreach (var item in list.SelectedItems)
            {
                //Only use append line when multiple items are selected.
                //This to make it easier to use the text from clipboard when 1 file is selected.
                if (fileNames.Length > 0)
                    fileNames.AppendLine();

                fileNames.Append((Path.Combine(Module.WorkingDir, item.Name)).ToNativePath());
            }
            Clipboard.SetText(fileNames.ToString());
        }

        private void OpenWithDifftoolToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!Unstaged.SelectedItems.Any())
                return;

            var item = Unstaged.SelectedItem;
            var fileName = item.Name;

            var cmdOutput = Module.OpenWithDifftool(fileName);

            if (!string.IsNullOrEmpty(cmdOutput))
                MessageBox.Show(this, cmdOutput);
        }


        private void ResetPartOfFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Unstaged.SelectedItems.Count() != 1)
            {
                MessageBox.Show(this, _onlyStageChunkOfSingleFileError.Text, _resetStageChunkOfFileCaption.Text);
                return;
            }

            foreach (var gitItemStatus in Unstaged.SelectedItems)
            {
                Module.RunExternalCmdShowConsole(AppSettings.GitCommand, string.Format("checkout -p \"{0}\"", gitItemStatus.Name));
                Initialize();
            }
        }

        private void ResetClick(object sender, EventArgs e)
        {
            UICommands.StartResetChangesDialog(this, Unstaged.AllItems, false);
            Initialize();
        }

        private void ResetUnStagedClick(object sender, EventArgs e)
        {
            UICommands.StartResetChangesDialog(this, Unstaged.AllItems, true);
            Initialize();
        }

        private void ShowUntrackedFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            showUntrackedFilesToolStripMenuItem.Checked = !showUntrackedFilesToolStripMenuItem.Checked;
            RescanChanges();
        }

        private void editFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileStatusList list;
            if (!SenderToFileStatusList(sender, out list))
                return;

            var item = list.SelectedItem;
            var fileName = Path.Combine(Module.WorkingDir, item.Name);

            UICommands.StartFileEditorDialog(fileName);

            UnstagedSelectionChanged(null, null);
        }

        private void CommitAndPush_Click(object sender, EventArgs e)
        {
            CheckForStagedAndCommit(Amend.Checked, true);
        }

        private void FormCommitActivated(object sender, EventArgs e)
        {
            if (AppSettings.RefreshCommitDialogOnFormFocus)
                RescanChanges();

            updateAuthorInfo();


        }

        private void updateAuthorInfo()
        {
            GetUserSettings();
            string author = "";
            string committer = string.Format("{0} {1} <{2}>", _commitCommitterInfo.Text, _userName, _userEmail);

            if (string.IsNullOrEmpty(toolAuthor.Text) || string.IsNullOrEmpty(toolAuthor.Text.Trim()))
            {
                author = string.Format("{0} {1} <{2}>", _commitAuthorInfo.Text, _userName, _userEmail);
            }
            else
            {
                author = string.Format("{0} {1}", _commitAuthorInfo.Text, toolAuthor.Text);
            }

            if (author != string.Format("{0} {1} <{2}>", _commitAuthorInfo.Text, _userName, _userEmail))
                commitAuthorStatus.Text = string.Format("{0} {1}", committer, author);
            else
                commitAuthorStatus.Text = committer;
        }

        private void GetUserSettings()
        {
            _userName = Module.GetEffectiveSetting(SettingKeyString.UserName);
            _userEmail = Module.GetEffectiveSetting(SettingKeyString.UserEmail);



        }
        private bool SenderToFileStatusList(object sender, out FileStatusList list)
        {
            list = null;
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null)
                return false;

            ContextMenuStrip menu = item.Owner as ContextMenuStrip;
            if (menu == null)
                return false;

            ListView lv = menu.SourceControl as ListView;
            if (lv == null)
                return false;

            list = lv.Parent as FileStatusList;
            return (list != null);
        }

        private void ViewFileHistoryMenuItem_Click(object sender, EventArgs e)
        {
            FileStatusList list;
            if (!SenderToFileStatusList(sender, out list))
                return;

            if (list.SelectedItems.Count() == 1)
            {
                UICommands.StartFileHistoryDialog(this, list.SelectedItem.Name, null);
            }
            else
                MessageBox.Show(this, _selectOnlyOneFile.Text, _selectOnlyOneFileCaption.Text);
        }

        private void Message_KeyUp(object sender, KeyEventArgs e)
        {
            // Ctrl + Enter = Commit
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                ExecuteCommitCommand();
                e.Handled = true;
            }
        }

        private void ExecuteCommitCommand()
        {
            CheckForStagedAndCommit(Amend.Checked, false);
        }

        private void Message_KeyDown(object sender, KeyEventArgs e)
        {
            // Prevent adding a line break when all we want is to commit
            if (e.Control && e.KeyCode == Keys.Enter)
                e.Handled = true;
        }

        private void Message_TextChanged(object sender, EventArgs e)
        {
            // Format text, except when doing an undo, because
            // this would itself introduce more steps that
            // need to be undone.
            if( !Message.IsUndoInProgress )
            {
                // always format from 0 to handle pasted text
                FormatAllText(0);
            }
        }

        private void Message_TextAssigned(object sender, EventArgs e)
        {
            Message_TextChanged(sender, e);
        }

        private bool FormatLine(int line)
        {
            int limit1 = AppSettings.CommitValidationMaxCntCharsFirstLine;
            int limitX = AppSettings.CommitValidationMaxCntCharsPerLine;
            bool empty2 = AppSettings.CommitValidationSecondLineMustBeEmpty;

            bool textHasChanged = false;

            if (limit1 > 0 && line == 0)
            {
                ColorTextAsNecessary(line, limit1, false);
            }

            if (empty2 && line == 1)
            {
                // Ensure next line. Optionally add a bullet.
                Message.EnsureEmptyLine(AppSettings.CommitValidationIndentAfterFirstLine, 1);
                Message.ChangeTextColor(2, 0, Message.LineLength(2), Color.Black);
                if (FormatLine(2))
                {
                    textHasChanged = true;
                }
            }

            if (limitX > 0 && line >= (empty2 ? 2 : 1))
            {
                if (AppSettings.CommitValidationAutoWrap)
                {
                    if (WrapIfNecessary(line, limitX))
                    {
                        textHasChanged = true;
                    }
                }
                ColorTextAsNecessary(line, limitX, textHasChanged);
            }

            return textHasChanged;
        }

        private bool WrapIfNecessary(int line, int lineLimit)
        {
            if (Message.LineLength(line) > lineLimit)
            {
                var oldText = Message.Line(line);
                var newText = WordWrapper.WrapSingleLine(oldText, lineLimit);
                if (!String.Equals(oldText, newText))
                {
                    Message.ReplaceLine(line, newText);
                    return true;
                }
            }
            return false;
        }

        private void ColorTextAsNecessary(int line, int lineLimit, bool fullRefresh)
        {
            var lineLength = Message.LineLength(line);
            int offset = 0;
            bool textAppended = false;
            if (!fullRefresh && formattedLines.Count > line)
            {
                offset = formattedLines[line].CommonPrefix(Message.Line(line)).Length;
                textAppended = offset > 0 && offset == formattedLines[line].Length;
            }

            int len = Math.Min(lineLimit, lineLength) - offset;

            if (!textAppended && 0 < len)
                Message.ChangeTextColor(line, offset, len, Color.Black);

            if (lineLength > lineLimit)
            {
                if (offset <= lineLimit || !textAppended)
                {
                    offset = Math.Max(offset, lineLimit);
                    len = lineLength - offset;
                    if (len > 0)
                        Message.ChangeTextColor(line, offset, len, Color.Red);
                }
            }
        }

        private List<string> formattedLines = new List<string>();

        private bool DidFormattedLineChange(int lineNumber)
        {
            //line not formated yet
            if (formattedLines.Count <= lineNumber)
                return true;

            return !formattedLines[lineNumber].Equals(Message.Line(lineNumber), StringComparison.OrdinalIgnoreCase);
        }

        private void TrimFormattedLines(int lineCount)
        {
            if (formattedLines.Count > lineCount)
                formattedLines.RemoveRange(lineCount, formattedLines.Count - lineCount);
        }

        private void SetFormattedLine(int lineNumber)
        {
            //line not formated yet
            if (formattedLines.Count <= lineNumber)
            {
                Debug.Assert(formattedLines.Count == lineNumber, formattedLines.Count + ":" + lineNumber);
                formattedLines.Add(Message.Line(lineNumber));
            }
            else
                formattedLines[lineNumber] = Message.Line(lineNumber);
        }

        private void FormatAllText(int startLine)
        {
            var lineCount = Message.LineCount();
            TrimFormattedLines(lineCount);
            for (int line = startLine; line < lineCount; line++)
            {
                if (DidFormattedLineChange(line))
                {
                    bool lineChanged = FormatLine(line);
                    SetFormattedLine(line);
                    if (lineChanged)
                        FormatAllText(line);
                }
            }
        }

        private void Message_SelectionChanged(object sender, EventArgs e)
        {
            commitCursorColumn.Text = Message.CurrentColumn.ToString();
            commitCursorLine.Text = Message.CurrentLine.ToString();
        }

        private void closeDialogAfterEachCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            closeDialogAfterEachCommitToolStripMenuItem.Checked = !closeDialogAfterEachCommitToolStripMenuItem.Checked;
            AppSettings.CloseCommitDialogAfterCommit = closeDialogAfterEachCommitToolStripMenuItem.Checked;
        }

        private void closeDialogAfterAllFilesCommittedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            closeDialogAfterAllFilesCommittedToolStripMenuItem.Checked = !closeDialogAfterAllFilesCommittedToolStripMenuItem.Checked;
            AppSettings.CloseCommitDialogAfterLastCommit = closeDialogAfterAllFilesCommittedToolStripMenuItem.Checked;
        }

        private void refreshDialogOnFormFocusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            refreshDialogOnFormFocusToolStripMenuItem.Checked = !refreshDialogOnFormFocusToolStripMenuItem.Checked;
            AppSettings.RefreshCommitDialogOnFormFocus = refreshDialogOnFormFocusToolStripMenuItem.Checked;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)
                RescanChanges();

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void signOffToolStripMenuItem_Click(object snder, EventArgs e)
        {
            signOffToolStripMenuItem.Checked = !signOffToolStripMenuItem.Checked;
        }

        private void toolAuthor_TextChanged(object sender, EventArgs e)
        {
            toolAuthorLabelItem.Enabled = toolAuthorLabelItem.Checked = !string.IsNullOrEmpty(toolAuthor.Text);
        }

        private void toolAuthorLabelItem_Click(object sender, EventArgs e)
        {
            toolAuthor.Text = "";
            toolAuthorLabelItem.Enabled = toolAuthorLabelItem.Checked = false;
            updateAuthorInfo();
        }

        private long _lastUserInputTime;
        private void FilterChanged(object sender, EventArgs e)
        {
            var currentTime = DateTime.Now.Ticks;
            if (_lastUserInputTime == 0)
            {
                long timerLastChanged = currentTime;
                var timer = new Timer { Interval = 250 };
                timer.Tick += (s, a) =>
                {
                    if (NoUserInput(timerLastChanged))
                    {
                        var selectionCount = 0;
                        try
                        {
                            selectionCount = Unstaged.SetSelectionFilter(selectionFilter.Text);
                            selectionFilter.ToolTipText = _selectionFilterToolTip.Text;
                        }
                        catch (ArgumentException ae)
                        {
                            selectionFilter.ToolTipText = string.Format(_selectionFilterErrorToolTip.Text, ae.Message);
                        }

                        if (selectionCount > 0)
                        {
                            AddToSelectionFilter(selectionFilter.Text);
                        }

                        timer.Stop();
                        _lastUserInputTime = 0;
                    }
                    timerLastChanged = _lastUserInputTime;
                };

                timer.Start();
            }

            _lastUserInputTime = currentTime;
        }

        private bool NoUserInput(long timerLastChanged)
        {
            return timerLastChanged == _lastUserInputTime;
        }

        private void AddToSelectionFilter(string filter)
        {
            if (!selectionFilter.Items.Cast<string>().Any(candiate => candiate == filter))
            {
                const int SelectionFilterMaxLength = 10;
                if (selectionFilter.Items.Count == SelectionFilterMaxLength)
                {
                    selectionFilter.Items.RemoveAt(SelectionFilterMaxLength - 1);
                }
                selectionFilter.Items.Insert(0, filter);
            }
        }

        private void FilterIndexChanged(object sender, EventArgs e)
        {
            Unstaged.SetSelectionFilter(selectionFilter.Text);
        }

        private void ToogleShowSelectionFilter(object sender, EventArgs e)
        {
            toolbarSelectionFilter.Visible = selectionFilterToolStripMenuItem.Checked;
        }

        private void commitSubmoduleChanges_Click(object sender, EventArgs e)
        {
            GitUICommands submodulCommands = new GitUICommands(Module.WorkingDir + _currentItem.Name.EnsureTrailingPathSeparator());
            submodulCommands.StartCommitDialog(this, false);
            Initialize();
        }

        private void openSubmoduleMenuItem_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = Application.ExecutablePath;
            process.StartInfo.Arguments = "browse";
            process.StartInfo.WorkingDirectory = Module.WorkingDir + _currentItem.Name.EnsureTrailingPathSeparator();
            process.Start();
        }

        private void resetSubmoduleChanges_Click(object sender, EventArgs e)
        {
            var unStagedFiles = Unstaged.SelectedItems.ToList();
            if (unStagedFiles.Count == 0)
                return;

            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(this, true, true);
            if (resetType == FormResetChanges.ActionEnum.Cancel)
                return;

            foreach (var item in unStagedFiles.Where(it => it.IsSubmodule))
            {
                GitModule module = Module.GetSubmodule(item.Name);

                // Reset all changes.
                module.ResetHard("");

                // Also delete new files, if requested.
                if (resetType == FormResetChanges.ActionEnum.ResetAndDelete)
                {
                    var unstagedFiles = module.GetUnstagedFiles();
                    foreach (var file in unstagedFiles.Where(file => file.IsNew))
                    {
                        try
                        {
                            string path = Path.Combine(module.WorkingDir, file.Name);
                            if (File.Exists(path))
                                File.Delete(path);
                            else
                                Directory.Delete(path, true);
                        }
                        catch (System.IO.IOException) { }
                        catch (System.UnauthorizedAccessException) { }
                    }
                }
            }

            Initialize();
        }

        private void updateSubmoduleMenuItem_Click(object sender, EventArgs e)
        {
            var unStagedFiles = Unstaged.SelectedItems.ToList();
            if (unStagedFiles.Count == 0)
                return;

            foreach (var item in unStagedFiles.Where(it => it.IsSubmodule))
            {
                FormProcess.ShowDialog(this, GitCommandHelpers.SubmoduleUpdateCmd(item.Name));
            }

            Initialize();
        }

        private void stashSubmoduleChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var unStagedFiles = Unstaged.SelectedItems.ToList();
            if (unStagedFiles.Count == 0)
                return;

            foreach (var item in unStagedFiles.Where(it => it.IsSubmodule))
            {
                GitUICommands uiCmds = new GitUICommands(Module.GetSubmodule(item.Name));
                uiCmds.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash);
            }

            Initialize();
        }

        private void submoduleSummaryMenuItem_Click(object sender, EventArgs e)
        {
            string summary = Module.GetSubmoduleSummary(_currentItem.Name);
            using (var frm = new FormEdit(summary)) frm.ShowDialog(this);
        }

        private void viewHistoryMenuItem_Click(object sender, EventArgs e)
        {
            ViewFileHistoryMenuItem_Click(sender, e);
        }

        private void openFolderMenuItem_Click(object sender, EventArgs e)
        {
            OpenToolStripMenuItemClick(sender, e);
        }

        private void openDiffMenuItem_Click(object sender, EventArgs e)
        {
            OpenWithDifftoolToolStripMenuItemClick(sender, e);
        }

        private void copyFolderNameMenuItem_Click(object sender, EventArgs e)
        {
            FilenameToClipboardToolStripMenuItemClick(sender, e);
        }

        private void commitTemplatesConfigtoolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var frm = new FormCommitTemplateSettings()) frm.ShowDialog(this);
            _shouldReloadCommitTemplates = true;
        }

        private void LoadCommitTemplates()
        {
            CommitTemplateItem[] commitTemplates = CommitTemplateItem.LoadFromSettings();

            commitTemplatesToolStripMenuItem.DropDownItems.Clear();

            if (null != commitTemplates)
            {
                for (int i = 0; i < commitTemplates.Length; i++)
                {
                    if (!commitTemplates[i].Name.IsNullOrEmpty())
                        AddTemplateCommitMessageToMenu(commitTemplates[i], commitTemplates[i].Name);
                }
            }

            commitTemplatesToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());

            var toolStripItem = new ToolStripMenuItem(_commitTemplateSettings.Text);
            toolStripItem.Click += commitTemplatesConfigtoolStripMenuItem_Click;
            commitTemplatesToolStripMenuItem.DropDownItems.Add(toolStripItem);
        }

        private void AddTemplateCommitMessageToMenu(CommitTemplateItem item, string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            var toolStripItem =
                new ToolStripMenuItem
                {
                    Tag = item,
                    Text = name
                };

            toolStripItem.Click += commitTemplatesToolStripMenuItem_Clicked;
            commitTemplatesToolStripMenuItem.DropDownItems.Add(toolStripItem);
        }

        private void commitTemplatesToolStripMenuItem_Clicked(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem item = (ToolStripMenuItem)sender;
                CommitTemplateItem templateItem = (CommitTemplateItem)(item.Tag);
                Message.Text = templateItem.Text;
                Message.Focus();
            }
            catch
            {
                return;
            }
        }

        private void commitTemplatesToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (_shouldReloadCommitTemplates)
            {
                LoadCommitTemplates();
                _shouldReloadCommitTemplates = false;
            }
        }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenContainingFolder(Unstaged);
        }

        private void OpenContainingFolder(FileStatusList list)
        {
            foreach (var item in list.SelectedItems)
            {
                var fileNames = new StringBuilder();
                fileNames.Append((Path.Combine(Module.WorkingDir, item.Name)).ToNativePath());

                string filePath = fileNames.ToString();
                if (File.Exists(filePath))
                {
                    OsShellUtil.SelectPathInFileExplorer(filePath);
                }
            }
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            foreach (var item in Staged.SelectedItems)
            {
                string output = Module.OpenWithDifftool(item.Name, null, null, null, "--cached");
                if (!string.IsNullOrEmpty(output))
                    MessageBox.Show(this, output);
            }
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            OpenContainingFolder(Staged);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (Unstaged.SelectedItem == null)
                return;

            Process gitProcess = Module.RunExternalCmdDetachedShowConsole(AppSettings.GitCommand,
                "add -p \"" + Unstaged.SelectedItem.Name + "\"");

            if (gitProcess != null)
            {
                _interactiveAddBashCloseWaitCts.Cancel();
                _interactiveAddBashCloseWaitCts = new CancellationTokenSource();

                var formsTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

                Task.Factory.StartNew(() =>
                    {
                        gitProcess.WaitForExit();
                        gitProcess.Dispose();
                    })
                    .ContinueWith(_ => RescanChanges(),
                    _interactiveAddBashCloseWaitCts.Token,
                    TaskContinuationOptions.OnlyOnRanToCompletion,
                formsTaskScheduler);
            }
        }

        private void Amend_CheckedChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Message.Text) && Amend.Checked)
            {
                Message.Text = Module.GetPreviousCommitMessages(1).FirstOrDefault().Trim();
                return;
            }
        }

        private void StageInSuperproject_CheckedChanged(object sender, EventArgs e)
        {
            if (StageInSuperproject.Visible)
                AppSettings.StageInSuperprojectAfterCommit = StageInSuperproject.Checked;
        }

        private void commitCommitter_Click(object sender, EventArgs e)
        {

            UICommands.StartSettingsDialog(this, SettingsDialog.Pages.GitConfigSettingsPage.GetPageReference());
        }

        private void toolAuthor_Leave(object sender, EventArgs e)
        {
            updateAuthorInfo();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unstagedLoader.Cancel();
                _unstagedLoader.Dispose();
                if (_interactiveAddBashCloseWaitCts != null)
                {
                    _interactiveAddBashCloseWaitCts.Cancel();
                    _interactiveAddBashCloseWaitCts.Dispose();
                    _interactiveAddBashCloseWaitCts = null;
                }

                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Indicates the kind of commit being prepared. Used for adjusting the behavior of FormCommit.
    /// </summary>
    public enum CommitKind
    {
        Normal,
        Fixup,
        Squash
    }
}
