using System;
using System.Collections.Generic;
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
using GitUI.Hotkey;
using GitUI.Script;
using PatchApply;
using ResourceManager.Translation;
using Timer = System.Windows.Forms.Timer;

namespace GitUI
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
        private readonly TranslationString _notOnBranchButtons = new TranslationString("Checkout branch|Continue");
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

        private readonly TranslationString _checkBoxAutoWrap = new TranslationString("Auto-wrap");
        #endregion

        private readonly SynchronizationContext _syncContext;
        public bool NeedRefresh;
        private GitItemStatus _currentItem;
        private bool _currentItemStaged;
        private readonly CommitKind _commitKind;
        private readonly GitRevision _editedCommit;
        private readonly ToolStripMenuItem _StageSelectedLinesToolStripMenuItem;
        private readonly ToolStripMenuItem _ResetSelectedLinesToolStripMenuItem;
        private string commitTemplate;
        private bool IsMergeCommit { get; set; }
        private bool shouldRescanChanges = true;
        private bool _shouldReloadCommitTemplates = true;
        private AsyncLoader unstagedLoader;
        private bool _useFormCommitMessage;
        private CancellationTokenSource interactiveAddBashCloseWaitCTS;
        private readonly string _indent;

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
            _syncContext = SynchronizationContext.Current;

            unstagedLoader = new AsyncLoader(_syncContext);

            _useFormCommitMessage = Settings.UseFormCommitMessage;

            InitializeComponent();
            Message.TextChanged += Message_TextChanged;
            Message.TextAssigned += Message_TextAssigned;

            Loading.Image = Properties.Resources.loadingpanel;

            Translate();

            SolveMergeconflicts.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold);

            SelectedDiff.ExtraDiffArgumentsChanged += SelectedDiffExtraDiffArgumentsChanged;

            if (IsUICommandsInitialized)
                StageInSuperproject.Visible = Module.SuperprojectModule != null;
            StageInSuperproject.Checked = Settings.StageInSuperprojectAfterCommit;
            closeDialogAfterEachCommitToolStripMenuItem.Checked = Settings.CloseCommitDialogAfterCommit;
            closeDialogAfterAllFilesCommittedToolStripMenuItem.Checked = Settings.CloseCommitDialogAfterLastCommit;
            refreshDialogOnFormFocusToolStripMenuItem.Checked = Settings.RefreshCommitDialogOnFormFocus;

            Unstaged.SetNoFilesText(_noUnstagedChanges.Text);
            Staged.SetNoFilesText(_noStagedChanges.Text);

            Message.Enabled = _useFormCommitMessage;

            commitMessageToolStripMenuItem.Enabled = _useFormCommitMessage;
            commitTemplatesToolStripMenuItem.Enabled = _useFormCommitMessage;

            Message.WatermarkText = _useFormCommitMessage
                ? _enterCommitMessageHint.Text
                : _commitMessageDisabled.Text;

            _commitKind = commitKind;
            _editedCommit = editedCommit;

            Unstaged.SelectedIndexChanged += UntrackedSelectionChanged;
            Staged.SelectedIndexChanged += TrackedSelectionChanged;

            Unstaged.DoubleClick += Unstaged_DoubleClick;
            Staged.DoubleClick += Staged_DoubleClick;

            HotkeysEnabled = true;
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);

            SelectedDiff.AddContextMenuSeparator();
            _StageSelectedLinesToolStripMenuItem = SelectedDiff.AddContextMenuEntry(_stageSelectedLines.Text, StageSelectedLinesToolStripMenuItemClick);
            _StageSelectedLinesToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys((int)Commands.StageSelectedFile).ToShortcutKeyDisplayString();
            _ResetSelectedLinesToolStripMenuItem = SelectedDiff.AddContextMenuEntry(_resetSelectedLines.Text, ResetSelectedLinesToolStripMenuItemClick);
            _ResetSelectedLinesToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys((int)Commands.ResetSelectedFiles).ToShortcutKeyDisplayString();
            _ResetSelectedLinesToolStripMenuItem.Image = Reset.Image;
            _indent = Settings.CommitValidationIndentAfterFirstLine ? "   " : String.Empty;
        }

        private void FormCommit_Load(object sender, EventArgs e)
        {
            if (Settings.CommitDialogSplitter != -1)
                splitMain.SplitterDistance = Settings.CommitDialogSplitter;
            if (Settings.CommitDialogRightSplitter != -1)
                splitRight.SplitterDistance = Settings.CommitDialogRightSplitter;
        }

        private void FormCommitFormClosing(object sender, FormClosingEventArgs e)
        {
            // Do not remember commit message of fixup or squash commits, since they have
            // a special meaning, and can be dangerous if used inappropriately.
            if (CommitKind.Normal == _commitKind)
                GitCommands.Commit.SetCommitMessage(Module, Message.Text);

            Settings.CommitDialogSplitter = splitMain.SplitterDistance;
            Settings.CommitDialogRightSplitter = splitRight.SplitterDistance;
        }

        void SelectedDiff_ContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _StageSelectedLinesToolStripMenuItem.Enabled = SelectedDiff.HasAnyPatches() || _currentItem != null && _currentItem.IsNew;
            _ResetSelectedLinesToolStripMenuItem.Enabled = _StageSelectedLinesToolStripMenuItem.Enabled;
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
            FocusFileList(Staged);
            return true;
        }

        private bool FocusUnstagedFiles()
        {
            FocusFileList(Unstaged);
            return true;
        }

        /// <summary>Helper method that moves the focus to the supplied FileStatusList</summary>
        private void FocusFileList(FileStatusList fileStatusList)
        {
            fileStatusList.Focus();
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
            if (Unstaged.Focused)
            {
                ResetSoftClick(this, null);
                return true;
            }
            else if (SelectedDiff.ContainsFocus && _ResetSelectedLinesToolStripMenuItem.Enabled)
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
            else if (SelectedDiff.ContainsFocus && !_currentItemStaged && _StageSelectedLinesToolStripMenuItem.Enabled)
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
            else if (SelectedDiff.ContainsFocus && _currentItemStaged && _StageSelectedLinesToolStripMenuItem.Enabled)
            {
                StageSelectedLinesToolStripMenuItemClick(this, null);
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
                case Commands.ToggleSelectionFilter: return ToggleSelectionFilter();
                default: return base.ExecuteCommand(cmd);
            }
        }

        #endregion

        public void ShowDialogWhenChanges()
        {
            ShowDialogWhenChanges(null);
        }

        private void ComputeUnstagedFiles(Action<IList<GitItemStatus>> onComputed, bool async)
        {
            Func < IList < GitItemStatus >> getAllChangedFilesWithSubmodulesStatus = () => Module.GetAllChangedFilesWithSubmodulesStatus(
                    !showIgnoredFilesToolStripMenuItem.Checked,
                    showUntrackedFilesToolStripMenuItem.Checked);

            if (async)
                unstagedLoader.Load(getAllChangedFilesWithSubmodulesStatus, onComputed);
            else
            {
                unstagedLoader.Cancel();
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

        private bool selectedDiffReloaded = true;

        private void StageSelectedLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            //to prevent multiple clicks
            if (!selectedDiffReloaded)
                return;

            Debug.Assert(_currentItem != null);
            // Prepare git command
            string args = "apply --cached --whitespace=nowarn";

            if (_currentItemStaged) //staged
                args += " --reverse";
            byte[] patch;
            if (!_currentItemStaged && _currentItem.IsNew)
                patch = PatchManager.GetSelectedLinesAsNewPatch(Module, _currentItem.Name, SelectedDiff.GetText(), SelectedDiff.GetSelectionPosition(), SelectedDiff.GetSelectionLength(), SelectedDiff.Encoding, false);
            else
                patch = PatchManager.GetSelectedLinesAsPatch(Module, SelectedDiff.GetText(), SelectedDiff.GetSelectionPosition(), SelectedDiff.GetSelectionLength(), _currentItemStaged, SelectedDiff.Encoding, _currentItem.IsNew);

            if (patch != null && patch.Length > 0)
            {
                string output = Module.RunGitCmd(args, patch);
                if (!string.IsNullOrEmpty(output))
                {
                    MessageBox.Show(this, output + "\n\n" + SelectedDiff.Encoding.GetString(patch));
                }
                if (_currentItemStaged)
                    Staged.StoreNextIndexToSelect();
                else
                    Unstaged.StoreNextIndexToSelect();
                ScheduleGoToLine();
                selectedDiffReloaded = false;
                RescanChanges();                
            }
        }

        private void ScheduleGoToLine()
        {
            int SelectedDifflineToSelect = SelectedDiff.GetText().Substring(0, SelectedDiff.GetSelectionPosition()).Count(c => c == '\n');
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
                            SelectedDiff.GoToLine(SelectedDifflineToSelect);
                            SelectedDiff.ScrollPos = scrollPosition;
                        }
                        SelectedDiff.TextLoaded -= textLoaded;
                        selectedDiffReloaded = true;
                    };
                SelectedDiff.TextLoaded += textLoaded;
                OnStageAreaLoaded -= stageAreaLoaded;
            };

            OnStageAreaLoaded += stageAreaLoaded;
        }

        private void ResetSelectedLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            //to prevent multiple clicks
            if (!selectedDiffReloaded)
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
               patch = PatchManager.GetSelectedLinesAsPatch(Module, SelectedDiff.GetText(), SelectedDiff.GetSelectionPosition(), SelectedDiff.GetSelectionLength(), _currentItemStaged, SelectedDiff.Encoding, _currentItem.IsNew);
           else
               if (_currentItem.IsNew)
                   patch = PatchManager.GetSelectedLinesAsNewPatch(Module, _currentItem.Name, SelectedDiff.GetText(), SelectedDiff.GetSelectionPosition(), SelectedDiff.GetSelectionLength(), SelectedDiff.Encoding, true);
               else
                   patch = PatchManager.GetResetUnstagedLinesAsPatch(Module, SelectedDiff.GetText(), SelectedDiff.GetSelectionPosition(), SelectedDiff.GetSelectionLength(), _currentItemStaged, SelectedDiff.Encoding);

            if (patch != null && patch.Length > 0)
            {
                string output = Module.RunGitCmd(args, patch);
                if (Settings.RunningOnWindows())
                {
                    //remove file mode warnings on windows
                    Regex regEx = new Regex("warning: .*has type .* expected .*", RegexOptions.Compiled);
                    output = output.RemoveLines(line => regEx.IsMatch(line));
                }
                if (!string.IsNullOrEmpty(output))
                {
                    MessageBox.Show(this, output + "\n\n" + SelectedDiff.Encoding.GetString(patch));
                }
                if (_currentItemStaged)
                    Staged.StoreNextIndexToSelect();
                else
                    Unstaged.StoreNextIndexToSelect();
                ScheduleGoToLine();
                selectedDiffReloaded = false;
                RescanChanges();
            }
        }

        private void EnableStageButtons(bool enable)
        {
            toolUnstageItem.Enabled = enable;
            toolUnstageAllItem.Enabled = enable;
            toolStageItem.Enabled = enable;
            toolStageAllItem.Enabled = enable;
            workingToolStripMenuItem.Enabled = enable;
        }

        private bool initialized = false;

        private void Initialize(bool loadUnstaged)
        {
            initialized = true;


            Cursor.Current = Cursors.WaitCursor;

            if (loadUnstaged)
            {
                Loading.Visible = true;
                LoadingStaged.Visible = true;

                Commit.Enabled = false;
                CommitAndPush.Enabled = false;
                Amend.Enabled = false;
                Reset.Enabled = false;
                EnableStageButtons(false);

                ComputeUnstagedFiles(LoadUnstagedOutput, true);
            }

            UpdateMergeHead();

            Message.TextBoxFont = Settings.CommitFont;

            // Check if commit.template is used
            string fileName = Module.GetEffectivePathSetting("commit.template");
            if (!string.IsNullOrEmpty(fileName))
            {
                using (var commitReader = new StreamReader(fileName))
                {
                    commitTemplate = commitReader.ReadToEnd().Replace("\r", "");
                }
                Message.Text = commitTemplate;
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
            Staged.GitItemStatuses = null;
            SolveMergeconflicts.Visible = Module.InTheMiddleOfConflictedMerge();
            Staged.GitItemStatuses = Module.GetStagedFilesWithSubmodulesStatus();
            Cursor.Current = Cursors.Default;
        }

        private event Action OnStageAreaLoaded;

        private bool LoadUnstagedOutputFirstTime = true;

        /// <summary>
        ///   Loads the unstaged output.
        ///   This method is passed in to the SetTextCallBack delegate
        ///   to set the Text property of textBox1.
        /// </summary>
        private void LoadUnstagedOutput(IList<GitItemStatus> allChangedFiles)
        {
            var unStagedFiles = new List<GitItemStatus>();
            var stagedFiles = new List<GitItemStatus>();

            foreach (var fileStatus in allChangedFiles)
            {
                if (fileStatus.IsStaged)
                    stagedFiles.Add(fileStatus);
                else
                    unStagedFiles.Add(fileStatus);
            }

            Unstaged.GitItemStatuses = null;
            Unstaged.GitItemStatuses = unStagedFiles;
            Staged.GitItemStatuses = null;
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
            Unstaged.SelectStoredNextIndex();
            Staged.SelectStoredNextIndex();

            if (OnStageAreaLoaded != null)
                OnStageAreaLoaded();

            if (LoadUnstagedOutputFirstTime)
            {
                if (Unstaged.GitItemStatuses.Any())
                    Unstaged.Focus();
                else if (Staged.GitItemStatuses.Any())
                    Message.Focus();
                else
                    Amend.Focus();
                LoadUnstagedOutputFirstTime = false;
            }
        }

        /// <summary>Returns if there are any changes at all, staged or unstaged.</summary>
        private bool DoChangesExist()
        {
            return (Unstaged.AllItems.Count > 0) || (Staged.AllItems.Count > 0);
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

            _StageSelectedLinesToolStripMenuItem.Text = staged ? _unstageSelectedLines.Text : _stageSelectedLines.Text;
            _StageSelectedLinesToolStripMenuItem.Image = staged ? toolUnstageItem.Image : toolStageItem.Image;
            _StageSelectedLinesToolStripMenuItem.ShortcutKeyDisplayString = 
                GetShortcutKeys((int) (staged ? Commands.UnStageSelectedFile : Commands.StageSelectedFile)).ToShortcutKeyDisplayString();
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

        private void TrackedSelectionChanged(object sender, EventArgs e)
        {
            ClearDiffViewIfNoFilesLeft();

            if (Staged.SelectedItems.Count == 0)
                return;

            Unstaged.SelectedItem = null;
            ShowChanges(Staged.SelectedItems[0], true);
        }

        private void UntrackedSelectionChanged(object sender, EventArgs e)
        {
            ClearDiffViewIfNoFilesLeft();

            Unstaged.ContextMenuStrip = null;

            if (Unstaged.SelectedItems.Count == 0)
                return;

            Staged.SelectedItem = null;
            ShowChanges(Unstaged.SelectedItems[0], false);

            GitItemStatus item = Unstaged.SelectedItems[0];
            if (!item.IsSubmodule)
                Unstaged.ContextMenuStrip = UnstagedFileContext;
            else
                Unstaged.ContextMenuStrip = UnstagedSubmoduleContext;
        }

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
            if ( amend )
            {
                // This is an amend commit.  Confirm the user understands the implications.  We don't want to prompt for an empty
                // commit, because amend may be used just to change the commit message or timestamp.
                if (!Settings.DontConfirmAmmend)
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
                    StageAll();
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
            if (_useFormCommitMessage && (string.IsNullOrEmpty(Message.Text) || Message.Text == commitTemplate))
            {
                MessageBox.Show(this, _enterCommitMessage.Text, _enterCommitMessageCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }

            if (_useFormCommitMessage && !ValidCommitMessage())
                return;

            if (Module.IsDetachedHead())
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
                        if (!UICommands.StartCheckoutBranchDialog(this, revision))
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

                ScriptManager.RunEventScripts(Module, ScriptEvent.BeforeCommit);

                var errorOccurred = !FormProcess.ShowDialog(this, Module.CommitCmd(amend, signOffToolStripMenuItem.Checked, toolAuthor.Text, _useFormCommitMessage));

                NeedRefresh = true;

                if (errorOccurred)
                    return;

                Amend.Checked = false;

                ScriptManager.RunEventScripts(Module, ScriptEvent.AfterCommit);

                Message.Text = string.Empty;
                GitCommands.Commit.SetCommitMessage(Module, string.Empty);

                bool pushCompleted = true;
                if (push)
                {
                    UICommands.StartPushDialog(this, true, out pushCompleted);
                }

                if (pushCompleted && Module.SuperprojectModule != null && Settings.StageInSuperprojectAfterCommit)
                    Module.SuperprojectModule.StageFile(Module.SubmodulePath);

                if (Settings.CloseCommitDialogAfterCommit)
                {
                    Close();
                    return;
                }

                if (Unstaged.GitItemStatuses.Any(gitItemStatus => gitItemStatus.IsTracked))
                {
                    InitializedStaged();
                    return;
                }

                if (Settings.CloseCommitDialogAfterLastCommit)
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
            if (Settings.CommitValidationMaxCntCharsFirstLine > 0)
            {
                var firstLine = Message.Text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (firstLine.Length > Settings.CommitValidationMaxCntCharsFirstLine)
                {
                    if (DialogResult.No == MessageBox.Show(this, _commitMsgFirstLineInvalid.Text, _commitValidationCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk))
                        return false;
                }
            }

            if (Settings.CommitValidationMaxCntCharsPerLine > 0)
            {
                var lines = Message.Text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.Length > Settings.CommitValidationMaxCntCharsPerLine)
                    {
                        if (DialogResult.No == MessageBox.Show(this, String.Format(_commitMsgLineInvalid.Text, line), _commitValidationCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk))
                            return false;
                    }
                }
            }

            if (Settings.CommitValidationSecondLineMustBeEmpty)
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

            if (!Settings.CommitValidationRegEx.IsNullOrEmpty())
            {
                try
                {
                    if (!Regex.IsMatch(Message.Text, Settings.CommitValidationRegEx))
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
            if (shouldRescanChanges)
            {
                toolRefreshItem.Enabled = false;
                Initialize();
                toolRefreshItem.Enabled = true;
            }
        }

        private void StageClick(object sender, EventArgs e)
        {
            Stage(Unstaged.SelectedItems);
        }

        private void StageAll()
        {
            Stage(Unstaged.GitItemStatuses);
        }

        private void Stage(IEnumerable<GitItemStatus> gitItemStatusses)
        {
            EnableStageButtons(false);
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                Unstaged.StoreNextIndexToSelect();
                toolStripProgressBar1.Visible = true;
                toolStripProgressBar1.Maximum = gitItemStatusses.Count() * 2;
                toolStripProgressBar1.Value = 0;

                var files = new List<GitItemStatus>();

                foreach (var gitItemStatus in gitItemStatusses)
                {
                    toolStripProgressBar1.Value = Math.Min(toolStripProgressBar1.Maximum - 1, toolStripProgressBar1.Value + 1);
                    if (gitItemStatus.Name.EndsWith("/"))
                        gitItemStatus.Name = gitItemStatus.Name.TrimEnd('/');
                    files.Add(gitItemStatus);
                }

                bool wereErrors = false;
                if (Settings.ShowErrorsWhenStagingFiles)
                {
                    FormStatus.ProcessStart processStart =
                        form =>
                        {
                            form.AddMessageLine(string.Format(_stageFiles.Text,
                                                         files.Count));
                            var output = Module.StageFiles(files, out wereErrors);
                            form.AddMessageLine(output);
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
                    var unStagedFiles = (List<GitItemStatus>)Unstaged.GitItemStatuses;
                    Unstaged.GitItemStatuses = null;

                    unStagedFiles.RemoveAll(item => !item.IsSubmodule && files.Exists(i => i.Name == item.Name || i.OldName == item.Name) && files.Exists(i => i.Name == item.Name));
                    unStagedFiles.RemoveAll(item => item.IsSubmodule && !item.SubmoduleStatus.IsDirty && files.Exists(i => i.Name == item.Name || i.OldName == item.Name) && files.Exists(i => i.Name == item.Name));

                    Unstaged.GitItemStatuses = unStagedFiles;
                    Unstaged.SelectStoredNextIndex();
                }                

                toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;

                toolStripProgressBar1.Visible = false;
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

            if (Settings.RevisionGraphShowWorkingDirChanges)
                NeedRefresh = true;
        }

        private void UnstageFilesClick(object sender, EventArgs e)
        {
            EnableStageButtons(false);
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (Staged.GitItemStatuses.Count() > 10 && Staged.SelectedItems.Count() == Staged.GitItemStatuses.Count())
                {
                    Loading.Visible = true;
                    LoadingStaged.Visible = true;
                    Commit.Enabled = false;
                    CommitAndPush.Enabled = false;
                    Amend.Enabled = false;
                    Reset.Enabled = false;

                    Module.ResetMixed("HEAD");
                    Initialize();
                }
                else
                {
                    toolStripProgressBar1.Visible = true;
                    toolStripProgressBar1.Maximum = Staged.SelectedItems.Count * 2;
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

                    InitializedStaged();
                    var stagedFiles = (List<GitItemStatus>)Staged.GitItemStatuses;
                    var unStagedFiles = (List<GitItemStatus>)Unstaged.GitItemStatuses;
                    Unstaged.GitItemStatuses = null;
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
                    Staged.SelectStoredNextIndex();

                    toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;
                }
                toolStripProgressBar1.Visible = false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            EnableStageButtons(true);
            Cursor.Current = Cursors.Default;

            if (Settings.RevisionGraphShowWorkingDirChanges)
                NeedRefresh = true;
        }


        private void ResetSoftClick(object sender, EventArgs e)
        {
            shouldRescanChanges = false;
            try
            {
                if (Unstaged.SelectedItem == null)
                    return;

                // Show a form asking the user if they want to reset the changes.
                FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(this, Unstaged.SelectedItems.Any(item => !item.IsNew), Unstaged.SelectedItems.Any(item => item.IsNew));
                if (resetType == FormResetChanges.ActionEnum.Cancel)
                    return;

                //remember max selected index
                Unstaged.StoreNextIndexToSelect();

                var deleteNewFiles = Unstaged.SelectedItems.Any(item => item.IsNew) && (resetType == FormResetChanges.ActionEnum.ResetAndDelete);
                var filesInUse = new List<string>();
                var output = new StringBuilder();
                foreach (var item in Unstaged.SelectedItems)
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

                if (filesInUse.Count > 0)
                    MessageBox.Show(this, "The following files are currently in use and will not be reset:" + Environment.NewLine + "\u2022 " + string.Join(Environment.NewLine + "\u2022 ", filesInUse), "Files In Use");

                if (!string.IsNullOrEmpty(output.ToString()))
                    MessageBox.Show(this, output.ToString(), _resetChangesCaption.Text);
            }
            finally
            {
                shouldRescanChanges = true;
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
            if (UICommands.StartResolveConflictsDialog(this))
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

        private void EditGitIgnoreToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartEditGitIgnoreDialog(this);
            Initialize();
        }

        private void StageAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            StageAll();
        }

        private void UnstageAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            Module.ResetMixed("HEAD");
            Initialize();
        }

        private void FormCommitShown(object sender, EventArgs e)
        {
            if (!initialized)
                Initialize();

            AcceptButton = Commit;

            string message;

            switch (_commitKind)
            {
                case CommitKind.Fixup:
                    message = string.Format("fixup! {0}", _editedCommit.Message);
                    break;
                case CommitKind.Squash:
                    message = string.Format("squash! {0}", _editedCommit.Message);
                    break;
                default:
                    message = Module.GetMergeMessage();

                    if (string.IsNullOrEmpty(message) && File.Exists(GitCommands.Commit.GetCommitMessagePath(Module)))
                        message = File.ReadAllText(GitCommands.Commit.GetCommitMessagePath(Module), Module.CommitEncoding);
                    break;
            }

            if (_useFormCommitMessage && !string.IsNullOrEmpty(message))
                Message.Text = message;

            ThreadPool.QueueUserWorkItem(
                o =>
                {
                    var text =
                        string.Format(_formTitle.Text, Module.GetSelectedBranch(),
                                      Module.WorkingDir);

                    _syncContext.Post(state1 => Text = text, null);
                });
        }

        private void SetCommitMessageFromTextBox(string commitMessageText)
        {
            //Save last commit message in settings. This way it can be used in multiple repositories.
            Settings.LastCommitMessage = commitMessageText;

            var path = Module.WorkingDirGitDir() + Settings.PathSeparator.ToString() + "COMMITMESSAGE";

            //Commit messages are UTF-8 by default unless otherwise in the config file.
            //The git manual states:
            //  git commit and git commit-tree issues a warning if the commit log message 
            //  given to it does not look like a valid UTF-8 string, unless you 
            //  explicitly say your project uses a legacy encoding. The way to say 
            //  this is to have i18n.commitencoding in .git/config file, like this:...
            Encoding encoding = Module.CommitEncoding;

            using (var textWriter = new StreamWriter(path, false, encoding))
            {
                var lineNumber = 0;
                foreach (var line in commitMessageText.Split('\n'))
                {
                    //When a committemplate is used, skip comments
                    //otherwise: "#" is probably not used for comment but for issue number
                    if (!line.StartsWith("#") ||
                        string.IsNullOrEmpty(commitTemplate))
                    {
                        if (lineNumber == 1 && !String.IsNullOrEmpty(line))
                            textWriter.WriteLine();

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

        private void CommitMessageToolStripMenuItemDropDownOpening(object sender, EventArgs e)
        {
            commitMessageToolStripMenuItem.DropDownItems.Clear();

            var msg = Settings.LastCommitMessage;

            AddCommitMessageToMenu(msg);

            foreach (var localLastCommitMessage in Module.GetPreviousCommitMessages(4))
            {
                if (!localLastCommitMessage.Trim().Equals(msg.Trim()))
                    AddCommitMessageToMenu(localLastCommitMessage);
            }
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

            int count = commitMessageToolStripMenuItem.DropDownItems.Count;
            commitMessageToolStripMenuItem.DropDownItems.Add(toolStripItem);
        }

        private void CommitMessageToolStripMenuItemDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Tag != null)
                Message.Text = ((string)e.ClickedItem.Tag).Trim();
        }

        private void generateListOfChangesInSubmodulesChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var stagedFiles = (List<GitItemStatus>)Staged.AllItems;

            List<string> modules = new List<string>();
            foreach (var item in stagedFiles.Where(it => it.IsSubmodule))
                modules.Add(item.Name);
            if (modules.Count == 0)
                return;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Submodule" + (modules.Count == 1 ? " " : "s ") +
                String.Join(", ", modules.ToArray()) + " updated.");
            sb.AppendLine();
            foreach (var item in modules)
            {
                string diff = Module.RunGitCmd(
                     string.Format("diff --cached -z -- {0}", item));
                var lines = diff.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                const string subprojCommit = "Subproject commit ";
                var from = lines.Single(s => s.StartsWith("-" + subprojCommit)).Substring(subprojCommit.Length + 1);
                var to = lines.Single(s => s.StartsWith("+" + subprojCommit)).Substring(subprojCommit.Length + 1);
                if (!String.IsNullOrEmpty(from) && !String.IsNullOrEmpty(to))
                {
                    sb.AppendLine("Submodule " + item + ":");
                    GitModule module = new GitModule(Module.WorkingDir + item + Settings.PathSeparator.ToString());//
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
            if (Unstaged.SelectedItems.Count == 0)
                return;

            SelectedDiff.Clear();
            var fileNames = Unstaged.SelectedItems.Select(item => item.Name).ToArray();
            new FormAddToGitIgnore(UICommands, fileNames).ShowDialog(this);
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

            if (list.SelectedItems.Count == 0)
                return;

            var item = list.SelectedItem;
            var fileName = item.Name;

            Process.Start((Path.Combine(Module.WorkingDir, fileName)).Replace(Settings.PathSeparatorWrong, Settings.PathSeparator));
        }

        private void OpenWithToolStripMenuItemClick(object sender, EventArgs e)
        {
            FileStatusList list;
            if (!SenderToFileStatusList(sender, out list))
                return;

            if (list.SelectedItems.Count == 0)
                return;

            var item = list.SelectedItem;
            var fileName = item.Name;

            OsShellUtil.OpenAs(Module.WorkingDir + fileName.Replace(Settings.PathSeparatorWrong, Settings.PathSeparator));
        }

        private void FilenameToClipboardToolStripMenuItemClick(object sender, EventArgs e)
        {
            FileStatusList list;
            if (!SenderToFileStatusList(sender, out list))
                return;

            if (list.SelectedItems.Count == 0)
                return;

            var fileNames = new StringBuilder();
            foreach (var item in list.SelectedItems)
            {
                //Only use appendline when multiple items are selected.
                //This to make it easier to use the text from clipboard when 1 file is selected.
                if (fileNames.Length > 0)
                    fileNames.AppendLine();

                fileNames.Append((Path.Combine(Module.WorkingDir, item.Name)).Replace(Settings.PathSeparatorWrong, Settings.PathSeparator));
            }
            Clipboard.SetText(fileNames.ToString());
        }

        private void OpenWithDifftoolToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Unstaged.SelectedItems.Count == 0)
                return;

            var item = Unstaged.SelectedItem;
            var fileName = item.Name;

            var cmdOutput = Module.OpenWithDifftool(fileName);

            if (!string.IsNullOrEmpty(cmdOutput))
                MessageBox.Show(this, cmdOutput);
        }


        private void ResetPartOfFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Unstaged.SelectedItems.Count != 1)
            {
                MessageBox.Show(this, _onlyStageChunkOfSingleFileError.Text, _resetStageChunkOfFileCaption.Text);
                return;
            }

            foreach (var gitItemStatus in Unstaged.SelectedItems)
            {
                Module.RunGitRealCmd(
                     string.Format("checkout -p \"{0}\"", gitItemStatus.Name));
                Initialize();
            }
        }

        private void ResetClick(object sender, EventArgs e)
        {
            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetAction = FormResetChanges.ShowResetDialog(this, Unstaged.AllItems.Any(item => !item.IsNew), Unstaged.AllItems.Any(item => item.IsNew));
            if (resetAction == FormResetChanges.ActionEnum.Cancel)
            {
                return;
            }

            // Reset all changes.
            Module.ResetHard("");

            // Also delete new files, if requested.
            if (resetAction == FormResetChanges.ActionEnum.ResetAndDelete)
            {
                foreach (var item in Unstaged.AllItems.Where(item => item.IsNew))
                {
                    try
                    {
                        string path = Path.Combine(Module.WorkingDir, item.Name);
                        if (File.Exists(path))
                            File.Delete(path);
                        else
                            Directory.Delete(path, true);
                    }
                    catch (System.IO.IOException) { }
                    catch (System.UnauthorizedAccessException) { }
                }
            }

            Initialize();
            NeedRefresh = true;
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

            using (var frm = new FormEditor(UICommands, fileName)) frm.ShowDialog(this);

            UntrackedSelectionChanged(null, null);
        }

        private void CommitAndPush_Click(object sender, EventArgs e)
        {
            CheckForStagedAndCommit(Amend.Checked, true);
        }

        private void FormCommitActivated(object sender, EventArgs e)
        {
            if (Settings.RefreshCommitDialogOnFormFocus)
                RescanChanges();
        }

        private bool SenderToFileStatusList(object sender, out FileStatusList list)
        {
            if (sender is ToolStripMenuItem)
            {
                ToolStripMenuItem item = sender as ToolStripMenuItem;
                if (item.Owner is ContextMenuStrip)
                {
                    ContextMenuStrip menu = item.Owner as ContextMenuStrip;
                    if (menu.SourceControl is ListBox)
                    {
                        ListBox lb = menu.SourceControl as ListBox;
                        if (lb.Parent is FileStatusList)
                        {
                            list = lb.Parent as FileStatusList;
                            return true;
                        }
                    }
                }

            }
            list = null;
            return false;
        }

        private void ViewFileHistoryMenuItem_Click(object sender, EventArgs e)
        {
            FileStatusList list;
            if (!SenderToFileStatusList(sender, out list))
                return;

            if (list.SelectedItems.Count == 1)
            {
                UICommands.StartFileHistoryDialog(this, list.SelectedItem.Name, null);
            }
            else
                MessageBox.Show(this, _selectOnlyOneFile.Text, _selectOnlyOneFileCaption.Text);
        }

        void Unstaged_DoubleClick(object sender, EventArgs e)
        {
            StageClick(sender, e);
        }

        void Staged_DoubleClick(object sender, EventArgs e)
        {
            UnstageFilesClick(sender, e);
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
            //always format from 0 to handle pasted text
            FormatAllText(0);
        }

        private void Message_TextAssigned(object sender, EventArgs e)
        {
            Message_TextChanged(sender, e);
        }

        private bool FormatLine(int line)
        {
            int limit1 = Settings.CommitValidationMaxCntCharsFirstLine;
            int limitX = Settings.CommitValidationMaxCntCharsPerLine;
            bool empty2 = Settings.CommitValidationSecondLineMustBeEmpty;

            bool textHasChanged = false;

            if (limit1 > 0 && line == 0)
            {
                ColorTextAsNecessary(line, limit1, false);
            }

            if (empty2 && line == 1)
            {
                // Ensure next line. Optionally add a bullet.
                Message.EnsureEmptyLine(Settings.CommitValidationIndentAfterFirstLine, 1);
                Message.ChangeTextColor(2, 0, Message.LineLength(2), Color.Black);
                if (FormatLine(2))
                {
                    textHasChanged = true;
                }
            }

            if (limitX > 0 && line >= (empty2 ? 2 : 1))
            {
                if (Settings.CommitValidationAutoWrap)
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
            if (!fullRefresh && formattedLines.Count > line)
            {
                offset = formattedLines[line].CommonPrefix(Message.Line(line)).Length;
            }

            int len = Math.Min(lineLimit, lineLength) - offset;

            if (0 < len)
                Message.ChangeTextColor(line, offset, len, Color.Black);

            if (lineLength > lineLimit)
            {
                offset = Math.Max(offset, lineLimit);
                len = lineLength - offset;
                if (len > 0)
                    Message.ChangeTextColor(line, offset, len, Color.Red);
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
            Settings.CloseCommitDialogAfterCommit = closeDialogAfterEachCommitToolStripMenuItem.Checked;
        }

        private void closeDialogAfterAllFilesCommittedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            closeDialogAfterAllFilesCommittedToolStripMenuItem.Checked = !closeDialogAfterAllFilesCommittedToolStripMenuItem.Checked;
            Settings.CloseCommitDialogAfterLastCommit = closeDialogAfterAllFilesCommittedToolStripMenuItem.Checked;
        }

        private void refreshDialogOnFormFocusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            refreshDialogOnFormFocusToolStripMenuItem.Checked = !refreshDialogOnFormFocusToolStripMenuItem.Checked;
            Settings.RefreshCommitDialogOnFormFocus = refreshDialogOnFormFocusToolStripMenuItem.Checked;
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
        }

        private long lastUserInputTime;
        private void FilterChanged(object sender, EventArgs e)
        {
            var currentTime = DateTime.Now.Ticks;
            if (lastUserInputTime == 0)
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
                        lastUserInputTime = 0;
                    }
                    timerLastChanged = lastUserInputTime;
                };

                timer.Start();
            }

            lastUserInputTime = currentTime;
        }

        private bool NoUserInput(long timerLastChanged)
        {
            return timerLastChanged == lastUserInputTime;
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
            GitUICommands submodulCommands = new GitUICommands(Module.WorkingDir + _currentItem.Name + Settings.PathSeparator.ToString());
            submodulCommands.StartCommitDialog(this, false);
            Initialize();
        }

        private void openSubmoduleMenuItem_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = Application.ExecutablePath;
            process.StartInfo.Arguments = "browse";
            process.StartInfo.WorkingDirectory = Module.WorkingDir + _currentItem.Name + Settings.PathSeparator.ToString();//
            process.Start();
        }

        private void resetSubmoduleChanges_Click(object sender, EventArgs e)
        {
            var unStagedFiles = (List<GitItemStatus>)Unstaged.SelectedItems;
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
            var unStagedFiles = (List<GitItemStatus>)Unstaged.SelectedItems;
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

            var unStagedFiles = (List<GitItemStatus>)Unstaged.SelectedItems;
            if (unStagedFiles.Count == 0)
                return;

            var arguments = GitCommandHelpers.StashSaveCmd(Settings.IncludeUntrackedFilesInManualStash);
            foreach (var item in unStagedFiles.Where(it => it.IsSubmodule))
            {
                GitModule module = Module.GetSubmodule(item.Name);
                FormProcess.ShowDialog(this, module, arguments);
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
            CommitTemplateItem[] commitTemplates =
                CommitTemplateItem.DeserializeCommitTemplates(Settings.CommitTemplates);

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
            openContainingFolder(Unstaged);
        }

        private void openContainingFolder(FileStatusList list)
        {
            foreach (var item in list.SelectedItems)
            {
                var fileNames = new StringBuilder();
                fileNames.Append((Path.Combine(Module.WorkingDir, item.Name)).Replace(Settings.PathSeparatorWrong, Settings.PathSeparator));

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
                string output = Module.OpenWithDifftool(item.Name, null, null, "--cached");
                if (!string.IsNullOrEmpty(output))
                    MessageBox.Show(this, output);
            }
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            openContainingFolder(Staged);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (Unstaged.SelectedItem == null)
                return;

            Process bashProcess = Module.RunBash("git add -p \"" + Unstaged.SelectedItem.Name + "\"");

            if (bashProcess != null)
            {
                // Reusing CTS if one has already been created by another unfinished interactive add
                interactiveAddBashCloseWaitCTS =
                    interactiveAddBashCloseWaitCTS ??
                    new CancellationTokenSource();
                
                var formsTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

                Task.Factory.StartNew(() =>
                {
                    bashProcess.WaitForExit();
                    using (bashProcess) { }
                }).ContinueWith(_ =>
                {
                    RescanChanges();
                },
                interactiveAddBashCloseWaitCTS.Token,
                TaskContinuationOptions.NotOnCanceled,
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
                Settings.StageInSuperprojectAfterCommit = StageInSuperproject.Checked;
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
