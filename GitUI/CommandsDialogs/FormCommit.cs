using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Patches;
using GitCommands.Utils;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.AutoCompletion;
using GitUI.Browsing.Dialogs;
using GitUI.CommandsDialogs.CommitDialog;
using GitUI.Editor;
using GitUI.HelperDialogs;
using GitUI.Hotkey;
using GitUI.Properties;
using GitUI.Script;
using GitUI.SpellChecker;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormCommit : GitModuleForm
    {
        #region Translation

        private readonly TranslationString _error = new TranslationString("Error");
        private readonly TranslationString _amendCommit =
            new TranslationString("You are about to rewrite history." + Environment.NewLine +
                                  "Only use amend if the commit is not published yet!" + Environment.NewLine +
                                  Environment.NewLine + "Do you want to continue?");

        private readonly TranslationString _amendCommitCaption = new TranslationString("Amend commit");

        private readonly TranslationString _deleteFailed = new TranslationString("Delete file failed");

        private readonly TranslationString _deleteSelectedFiles =
            new TranslationString("Are you sure you want to delete the selected file(s)?");

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
            new TranslationString("There are unresolved merge conflicts, solve merge conflicts before committing.");

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
                                  Environment.NewLine + Environment.NewLine + "Do you want to continue?");
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

        private readonly TranslationString _addSelectionToCommitMessage = new TranslationString("Add selection to commit message");
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

        private readonly TranslationString _templateNotFoundCaption = new TranslationString("Template Error");
        private readonly TranslationString _templateNotFound = new TranslationString($"Template not found: {{0}}.{Environment.NewLine}{Environment.NewLine}You can set your template:{Environment.NewLine}\t$ git config commit.template ./.git_commit_msg.txt{Environment.NewLine}You can unset the template:{Environment.NewLine}\t$ git config --unset commit.template");
        private readonly TranslationString _templateLoadErrorCaption = new TranslationString("Template could not be loaded");

        private readonly TranslationString _skipWorktreeToolTip = new TranslationString("Hide already tracked files that will change but that you don\'t want to commit."
            + Environment.NewLine + "Suitable for some config files modified locally.");
        private readonly TranslationString _assumeUnchangedToolTip = new TranslationString("Tell git to not check the status of this file for performance benefits."
            + Environment.NewLine + "Use this feature when a file is big and never change."
            + Environment.NewLine + "Git will never check if the file has changed that will improve status check performance.");
        private readonly TranslationString _stopTrackingFail = new TranslationString("Fail to stop tracking the file '{0}'.");

        private readonly TranslationString _statusBarBranchWithoutRemote = new TranslationString("(remote not configured)");
        private readonly TranslationString _untrackedRemote = new TranslationString("(untracked)");
        #endregion

        private event Action OnStageAreaLoaded;

        private readonly ICommitTemplateManager _commitTemplateManager;
        private readonly CommitKind _commitKind;
        [CanBeNull] private readonly GitRevision _editedCommit;
        private readonly ToolStripMenuItem _addSelectionToCommitMessageToolStripMenuItem;
        private readonly ToolStripMenuItem _stageSelectedLinesToolStripMenuItem;
        private readonly ToolStripMenuItem _resetSelectedLinesToolStripMenuItem;
        private readonly AsyncLoader _unstagedLoader = new AsyncLoader();
        private readonly bool _useFormCommitMessage = AppSettings.UseFormCommitMessage;
        private readonly CancellationTokenSequence _interactiveAddSequence = new CancellationTokenSequence();
        private readonly SplitterManager _splitterManager = new SplitterManager(new AppSettingsPath("CommitDialog"));
        private readonly Subject<string> _selectionFilterSubject = new Subject<string>();
        private readonly IFullPathResolver _fullPathResolver;
        private readonly List<string> _formattedLines = new List<string>();
        private readonly IScriptManager _scriptManager;
        private readonly IScriptRunner _scriptRunner;

        private FileStatusList _currentFilesList;
        private bool _skipUpdate;
        private GitItemStatus _currentItem;
        private bool _currentItemStaged;
        private ICommitMessageManager _commitMessageManager;
        private string _commitTemplate;
        private bool _isMergeCommit;
        private bool _shouldRescanChanges = true;
        private bool _shouldReloadCommitTemplates = true;
        private bool _bypassActivatedEventHandler;
        private bool _loadUnstagedOutputFirstTime = true;
        private bool _initialized;
        private bool _selectedDiffReloaded = true;
        [CanBeNull] private IReadOnlyList<GitItemStatus> _currentSelection;
        private int _alreadyLoadedTemplatesCount = -1;
        private EventHandler _branchNameLabelOnClick;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormCommit()
        {
            InitializeComponent();
        }

        public FormCommit([NotNull] GitUICommands commands, CommitKind commitKind = CommitKind.Normal, GitRevision editedCommit = null)
            : base(commands)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _commitKind = commitKind;
            _editedCommit = editedCommit;

            InitializeComponent();

            _scriptManager = new ScriptManager();

            var gitUIEventArgs = new GitUIEventArgs(this, UICommands);
            var simpleDialog = new SimpleDialog(this);
            var scriptOptionsParser = new ScriptOptionsParser(simpleDialog, () => Module);

            _scriptRunner = new ScriptRunner(() => Module, gitUIEventArgs, scriptOptionsParser, simpleDialog, _scriptManager);

            splitRight.Panel2MinSize = DpiUtil.Scale(100);

            _commitMessageManager = new CommitMessageManager(Module.WorkingDirGitDir, Module.CommitEncoding);

            Message.TextChanged += Message_TextChanged;
            Message.TextAssigned += Message_TextAssigned;
            Message.AddAutoCompleteProvider(new CommitAutoCompleteProvider(Module));
            _commitTemplateManager = new CommitTemplateManager(Module);

            SolveMergeconflicts.Font = new Font(SolveMergeconflicts.Font, FontStyle.Bold);

            SelectedDiff.ExtraDiffArgumentsChanged += SelectedDiffExtraDiffArgumentsChanged;
            StageInSuperproject.Visible = Module.SuperprojectModule != null;
            StageInSuperproject.Checked = AppSettings.StageInSuperprojectAfterCommit;
            closeDialogAfterEachCommitToolStripMenuItem.Checked = AppSettings.CloseCommitDialogAfterCommit;
            closeDialogAfterAllFilesCommittedToolStripMenuItem.Checked = AppSettings.CloseCommitDialogAfterLastCommit;
            refreshDialogOnFormFocusToolStripMenuItem.Checked = AppSettings.RefreshCommitDialogOnFormFocus;

            Unstaged.SetNoFilesText(_noUnstagedChanges.Text);
            Unstaged.FilterVisible = true;
            Staged.SetNoFilesText(_noStagedChanges.Text);

            ConfigureMessageBox();

            HotkeysEnabled = true;
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);

            openToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenFile);
            openWithToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenFileWith);
            editFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.EditFile);
            stagedOpenToolStripMenuItem7.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenFile);
            stagedOpenWithToolStripMenuItem8.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenFileWith);
            stagedEditFileToolStripMenuItem11.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.EditFile);

            SelectedDiff.AddContextMenuSeparator();
            _stageSelectedLinesToolStripMenuItem = SelectedDiff.AddContextMenuEntry(_stageSelectedLines.Text, StageSelectedLinesToolStripMenuItemClick);
            _stageSelectedLinesToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.StageSelectedFile);
            _resetSelectedLinesToolStripMenuItem = SelectedDiff.AddContextMenuEntry(_resetSelectedLines.Text, ResetSelectedLinesToolStripMenuItemClick);
            _resetSelectedLinesToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.ResetSelectedFiles);
            _resetSelectedLinesToolStripMenuItem.Image = Reset.Image;
            _addSelectionToCommitMessageToolStripMenuItem = SelectedDiff.AddContextMenuEntry(_addSelectionToCommitMessage.Text, (s, e) => AddSelectionToCommitMessage());
            _addSelectionToCommitMessageToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.AddSelectionToCommitMessage);
            resetChanges.ShortcutKeyDisplayString = _resetSelectedLinesToolStripMenuItem.ShortcutKeyDisplayString;
            stagedResetChanges.ShortcutKeyDisplayString = _resetSelectedLinesToolStripMenuItem.ShortcutKeyDisplayString;
            deleteFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.DeleteSelectedFiles);
            viewFileHistoryToolStripItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.ShowHistory);
            stagedFileHistoryToolStripMenuItem6.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.ShowHistory);
            commitAuthorStatus.ToolTipText = _commitCommitterToolTip.Text;
            skipWorktreeToolStripMenuItem.ToolTipText = _skipWorktreeToolTip.Text;
            assumeUnchangedToolStripMenuItem.ToolTipText = _assumeUnchangedToolTip.Text;
            toolAuthor.Control.PreviewKeyDown += (_, e) =>
            {
                if (e.Alt)
                {
                    e.IsInputKey = true;
                }
            };
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);

            /* If not changed, by default show "no sign commit" */
            if (gpgSignCommitToolStripComboBox.SelectedIndex == -1)
            {
                gpgSignCommitToolStripComboBox.SelectedIndex = 0;
            }

            ((ToolStripDropDownMenu)commitTemplatesToolStripMenuItem.DropDown).ShowImageMargin = false;
            ((ToolStripDropDownMenu)commitTemplatesToolStripMenuItem.DropDown).ShowCheckMargin = false;

            ((ToolStripDropDownMenu)commitMessageToolStripMenuItem.DropDown).ShowImageMargin = false;
            ((ToolStripDropDownMenu)commitMessageToolStripMenuItem.DropDown).ShowCheckMargin = false;

            selectionFilter.Size = DpiUtil.Scale(selectionFilter.Size);
            toolStripStatusBranchIcon.Width = DpiUtil.Scale(toolStripStatusBranchIcon.Width);

            _splitterManager.AddSplitter(splitMain, "splitMain");
            _splitterManager.AddSplitter(splitRight, "splitRight");
            _splitterManager.AddSplitter(splitLeft, "splitLeft");
            _splitterManager.RestoreSplitters();

            SetVisibilityOfSelectionFilter(AppSettings.CommitDialogSelectionFilter);
            Reset.Visible = AppSettings.ShowResetAllChanges;
            ResetUnStaged.Visible = AppSettings.ShowResetWorkTreeChanges;
            CommitAndPush.Visible = AppSettings.ShowCommitAndPush;
            splitRight.Panel2MinSize = Math.Max(splitRight.Panel2MinSize, flowCommitButtons.PreferredSize.Height);
            splitRight.SplitterDistance = Math.Min(splitRight.SplitterDistance, splitRight.Height - splitRight.Panel2MinSize);

            SelectedDiff.EscapePressed += () => DialogResult = DialogResult.Cancel;

            InitializeComplete();

            // By calling this in the constructor, we prevent flickering caused by resizing the
            // form, for example when it is restored to maximised, but first drawn as a smaller window.
            RestorePosition();

            // TODO this code is very similar to code in FileStatusList
            _selectionFilterSubject
                .Throttle(TimeSpan.FromMilliseconds(250))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(
                    filterText =>
                    {
                        ThreadHelper.AssertOnUIThread();

                        var matchCount = 0;
                        try
                        {
                            matchCount = Unstaged.SetSelectionFilter(filterText);
                            selectionFilter.ToolTipText = _selectionFilterToolTip.Text;
                        }
                        catch (ArgumentException ae)
                        {
                            selectionFilter.ToolTipText = string.Format(_selectionFilterErrorToolTip.Text, ae.Message);
                        }

                        if (matchCount > 0)
                        {
                            AddToSelectionFilter();
                        }

                        void AddToSelectionFilter()
                        {
                            if (selectionFilter.Items.Cast<string>().Any(s => s == filterText))
                            {
                                // Item is already in the list
                                return;
                            }

                            const int SelectionFilterMaxLength = 10;

                            while (selectionFilter.Items.Count >= SelectionFilterMaxLength)
                            {
                                // Remove the last item
                                selectionFilter.Items.RemoveAt(SelectionFilterMaxLength - 1);
                            }

                            // Insert the next term at the start of the filter control
                            selectionFilter.Items.Insert(0, filterText);
                        }
                    });

            return;

            void ConfigureMessageBox()
            {
                Amend.Enabled = _commitKind == CommitKind.Normal;

                bool messageCanBeChanged = _useFormCommitMessage && _commitKind == CommitKind.Normal;

                Message.Enabled = messageCanBeChanged;

                commitMessageToolStripMenuItem.Enabled = messageCanBeChanged;
                commitTemplatesToolStripMenuItem.Enabled = messageCanBeChanged;

                Message.WatermarkText = _useFormCommitMessage
                    ? _enterCommitMessageHint.Text
                    : _commitMessageDisabled.Text;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unstagedLoader.Dispose();
                _interactiveAddSequence.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
            const int TVS_EX_DOUBLEBUFFER = 0x0004;

            SendMessage(Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            base.OnHandleCreated(e);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wp, IntPtr lp);

        protected override void OnApplicationActivated()
        {
            if (!_bypassActivatedEventHandler && AppSettings.RefreshCommitDialogOnFormFocus)
            {
                RescanChanges();
            }

            base.OnApplicationActivated();
        }

        protected override void OnActivated(EventArgs e)
        {
            if (!_bypassActivatedEventHandler)
            {
                UpdateAuthorInfo();
            }

            base.OnActivated(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Message.CancelAutoComplete();

            // Do not remember commit message of fixup or squash commits, since they have
            // a special meaning, and can be dangerous if used inappropriately.
            if (_commitKind == CommitKind.Normal)
            {
                _commitMessageManager.MergeOrCommitMessage = Message.Text;
                _commitMessageManager.AmendState = Amend.Checked;
            }

            _splitterManager.SaveSplitters();

            base.OnFormClosing(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            showUntrackedFilesToolStripMenuItem.Checked = Module.EffectiveConfigFile.GetValue("status.showUntrackedFiles") != "no";
            MinimizeBox = Owner == null;
            base.OnLoad(e);
        }

        protected override void OnShown(EventArgs e)
        {
            if (!_initialized)
            {
                Initialize();
            }

            UpdateAuthorInfo();

            string message;

            switch (_commitKind)
            {
                case CommitKind.Fixup:
                    message = TryAddPrefix("fixup!", _editedCommit.Subject);
                    break;
                case CommitKind.Squash:
                    message = TryAddPrefix("squash!", _editedCommit.Subject);
                    break;
                default:
                    message = _commitMessageManager.MergeOrCommitMessage;
                    Amend.Checked = !_commitMessageManager.IsMergeCommit && _commitMessageManager.AmendState;
                    break;
            }

            if (_useFormCommitMessage && !string.IsNullOrEmpty(message))
            {
                Message.Text = message; // initial assignment
            }
            else
            {
                AssignCommitMessageFromTemplate();
            }

            base.OnShown(e);

            return;

            string TryAddPrefix(string prefix, string suffix)
            {
                return suffix.StartsWith(prefix) ? suffix : $"{prefix} {suffix}";
            }

            void AssignCommitMessageFromTemplate()
            {
                var text = "";
                try
                {
                    text = _commitTemplateManager.LoadGitCommitTemplate();
                }
                catch (FileNotFoundException ex)
                {
                    MessageBox.Show(this, string.Format(_templateNotFound.Text, ex.FileName),
                        _templateNotFoundCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message,
                        _templateLoadErrorCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                Message.Text = text; // initial assignment
                _commitTemplate = text;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Control | Keys.Enter when !Message.ContainsFocus:
                    {
                        FocusCommitMessage();
                        e.Handled = true;
                        break;
                    }

                case Keys.Control | Keys.P:
                case Keys.Alt | Keys.Up:
                case Keys.Alt | Keys.Left:
                    {
                        MoveSelection(-1);
                        e.Handled = true;
                        break;
                    }

                case Keys.Control | Keys.N:
                case Keys.Alt | Keys.Down:
                case Keys.Alt | Keys.Right:
                    {
                        MoveSelection(+1);
                        e.Handled = true;
                        break;
                    }
            }

            base.OnKeyUp(e);

            return;

            void MoveSelection(int direction)
            {
                var list = Message.Focused ? Staged : _currentFilesList;
                _currentFilesList = list;
                var itemsCount = list.AllItemsCount;
                if (itemsCount != 0)
                {
                    list.SelectedIndex = (list.SelectedIndex + direction + itemsCount) % itemsCount;
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)
            {
                RescanChanges();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void SelectedDiff_ContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _stageSelectedLinesToolStripMenuItem.Enabled = SelectedDiff.HasAnyPatches() || (_currentItem != null && _currentItem.IsNew);
            _resetSelectedLinesToolStripMenuItem.Enabled = _stageSelectedLinesToolStripMenuItem.Enabled;
        }

        private void SelectedDiff_TextLoaded(object sender, EventArgs e)
        {
            _selectedDiffReloaded = true;
        }

        #region Hotkey commands

        public static readonly string HotkeySettingsName = "Commit";

        internal enum Command
        {
            AddToGitIgnore = 0,
            DeleteSelectedFiles = 1,
            FocusUnstagedFiles = 2,
            FocusSelectedDiff = 3,
            FocusStagedFiles = 4,
            FocusCommitMessage = 5,
            ResetSelectedFiles = 6,
            StageSelectedFile = 7,
            UnStageSelectedFile = 8,
            ShowHistory = 9,
            ToggleSelectionFilter = 10,
            StageAll = 11,
            OpenWithDifftool = 12,
            OpenFile = 13,
            OpenFileWith = 14,
            EditFile = 15,
            AddSelectionToCommitMessage = 16
        }

        private string GetShortcutKeyDisplayString(Command cmd)
        {
            return GetShortcutKeys((int)cmd).ToShortcutKeyDisplayString();
        }

        private bool AddSelectionToCommitMessage()
        {
            if (!SelectedDiff.ContainsFocus)
            {
                return false;
            }

            var selectedText = SelectedDiff.GetSelectedText();
            if (string.IsNullOrEmpty(selectedText))
            {
                return false;
            }

            if (Message.SelectionLength == 0)
            {
                selectedText += '\n';
            }

            int selectionStart = Message.SelectionStart;

            Message.SelectedText = selectedText;

            Message.SelectionStart = selectionStart + selectedText.Length;

            return true;
        }

        private bool AddToGitIgnore()
        {
            if (Unstaged.Focused)
            {
                AddFileToGitIgnoreToolStripMenuItemClick(this, null);
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

        private bool StageAllFiles()
        {
            if (Unstaged.IsEmpty)
            {
                return false;
            }
            else
            {
                StageAllToolStripMenuItemClick(this, null);
                return true;
            }
        }

        private bool StartFileHistoryDialog()
        {
            if (Staged.Focused || Unstaged.Focused)
            {
                if (_currentFilesList.SelectedItem != null)
                {
                    if ((!_currentFilesList.SelectedItem.IsNew) && (!_currentFilesList.SelectedItem.IsRenamed))
                    {
                        UICommands.StartFileHistoryDialog(this, _currentFilesList.SelectedItem.Name);
                    }
                }

                return true;
            }

            return false;
        }

        private bool ToggleSelectionFilter()
        {
            var visible = !selectionFilterToolStripMenuItem.Checked;
            SetVisibilityOfSelectionFilter(visible);
            if (visible)
            {
                selectionFilter.Focus();
            }
            else if (selectionFilter.Focused)
            {
                Unstaged.Focus();
            }

            return true;
        }

        protected override CommandStatus ExecuteCommand(int cmd)
        {
            switch ((Command)cmd)
            {
                case Command.AddToGitIgnore: return AddToGitIgnore();
                case Command.DeleteSelectedFiles: return DeleteSelectedFiles();
                case Command.FocusStagedFiles: return FocusStagedFiles();
                case Command.FocusUnstagedFiles: return FocusUnstagedFiles();
                case Command.FocusSelectedDiff: return FocusSelectedDiff();
                case Command.FocusCommitMessage: return FocusCommitMessage();
                case Command.ResetSelectedFiles: return ResetSelectedFiles();
                case Command.StageSelectedFile: return StageSelectedFile();
                case Command.UnStageSelectedFile: return UnStageSelectedFile();
                case Command.ShowHistory: return StartFileHistoryDialog();
                case Command.ToggleSelectionFilter: return ToggleSelectionFilter();
                case Command.StageAll: return StageAllFiles();
                case Command.OpenWithDifftool: SelectedDiff.OpenWithDifftool?.Invoke(); return true;
                case Command.OpenFile: openToolStripMenuItem.PerformClick(); return true;
                case Command.OpenFileWith: openWithToolStripMenuItem.PerformClick(); return true;
                case Command.EditFile: editFileToolStripMenuItem.PerformClick(); return true;
                case Command.AddSelectionToCommitMessage: return AddSelectionToCommitMessage();
                default: return base.ExecuteCommand(cmd);
            }
        }

        #endregion

        private void ComputeUnstagedFiles(Action<IReadOnlyList<GitItemStatus>> onComputed, bool doAsync)
        {
            IReadOnlyList<GitItemStatus> GetAllChangedFilesWithSubmodulesStatus()
            {
                return Module.GetAllChangedFilesWithSubmodulesStatus(
                    !showIgnoredFilesToolStripMenuItem.Checked,
                    !showAssumeUnchangedFilesToolStripMenuItem.Checked,
                    !showSkipWorktreeFilesToolStripMenuItem.Checked,
                    showUntrackedFilesToolStripMenuItem.Checked ? UntrackedFilesMode.Default : UntrackedFilesMode.No);
            }

            if (doAsync)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(() =>
                {
                    return _unstagedLoader.LoadAsync(GetAllChangedFilesWithSubmodulesStatus, onComputed);
                });
            }
            else
            {
                _unstagedLoader.Cancel();
                onComputed(GetAllChangedFilesWithSubmodulesStatus());
            }
        }

        public void ShowDialogWhenChanges(IWin32Window owner = null)
        {
            ComputeUnstagedFiles(allChangedFiles =>
                {
                    if (allChangedFiles.Count > 0)
                    {
                        LoadUnstagedOutput(allChangedFiles);
                        Initialize(loadUnstaged: false);
                        ShowDialog(owner);
                    }
                    else
                    {
                        Close();
                    }

                    Loading.IsAnimating = false;
                }, false);
        }

        private void StageSelectedLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            // to prevent multiple clicks
            if (!_selectedDiffReloaded)
            {
                return;
            }

            // File no longer selected
            if (_currentItem == null)
            {
                return;
            }

            byte[] patch;
            if (!_currentItemStaged && _currentItem.IsNew)
            {
                patch = PatchManager.GetSelectedLinesAsNewPatch(Module, _currentItem.Name,
                    SelectedDiff.GetText(), SelectedDiff.GetSelectionPosition(),
                    SelectedDiff.GetSelectionLength(), SelectedDiff.Encoding, false, SelectedDiff.FilePreamble);
            }
            else
            {
                patch = PatchManager.GetSelectedLinesAsPatch(
                    SelectedDiff.GetText(),
                    SelectedDiff.GetSelectionPosition(), SelectedDiff.GetSelectionLength(),
                    _currentItemStaged, SelectedDiff.Encoding, _currentItem.IsNew);
            }

            if (patch != null && patch.Length > 0)
            {
                var args = new GitArgumentBuilder("apply")
                {
                    "--cached",
                    "--whitespace=nowarn",
                    { _currentItemStaged,  "--reverse" }
                };

                string output = Module.GitExecutable.GetOutput(args, patch);

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
            {
                Staged.StoreNextIndexToSelect();
            }
            else
            {
                Unstaged.StoreNextIndexToSelect();
            }

            _selectedDiffReloaded = false;
            RescanChanges();
        }

        private void ResetSelectedLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            // to prevent multiple clicks
            if (!_selectedDiffReloaded)
            {
                return;
            }

            // File no longer selected
            if (_currentItem == null)
            {
                return;
            }

            if (MessageBox.Show(this, _resetSelectedLinesConfirmation.Text, _resetChangesCaption.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            byte[] patch;
            if (_currentItemStaged)
            {
                patch = PatchManager.GetSelectedLinesAsPatch(
                    SelectedDiff.GetText(),
                    SelectedDiff.GetSelectionPosition(), SelectedDiff.GetSelectionLength(),
                    _currentItemStaged, SelectedDiff.Encoding, _currentItem.IsNew);
            }
            else if (_currentItem.IsNew)
            {
                patch = PatchManager.GetSelectedLinesAsNewPatch(Module, _currentItem.Name,
                    SelectedDiff.GetText(), SelectedDiff.GetSelectionPosition(), SelectedDiff.GetSelectionLength(),
                    SelectedDiff.Encoding, true, SelectedDiff.FilePreamble);
            }
            else
            {
                patch = PatchManager.GetResetWorkTreeLinesAsPatch(Module, SelectedDiff.GetText(),
                    SelectedDiff.GetSelectionPosition(), SelectedDiff.GetSelectionLength(), SelectedDiff.Encoding);
            }

            if (patch != null && patch.Length > 0)
            {
                var args = new GitArgumentBuilder("apply")
                {
                    "--whitespace=nowarn",
                    { _currentItemStaged,  "--reverse --index" }
                };

                string output = Module.GitExecutable.GetOutput(args, patch);

                if (EnvUtils.RunningOnWindows())
                {
                    // remove file mode warnings on windows
                    var regEx = new Regex("warning: .*has type .* expected .*", RegexOptions.Compiled);
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

        private async Task UpdateBranchNameDisplayAsync()
        {
            await TaskScheduler.Default;

            var currentBranchName = Module.GetSelectedBranch();
            if (_branchNameLabelOnClick != null)
            {
                remoteNameLabel.Click -= _branchNameLabelOnClick;
            }

            var currentBranch = Module.GetRefs(false, true).FirstOrDefault(r => r.LocalName == currentBranchName);
            if (currentBranch == null)
            {
                branchNameLabel.Text = currentBranchName;
                remoteNameLabel.Text = string.Empty;
                return;
            }

            string pushTo;
            if (string.IsNullOrEmpty(currentBranch.TrackingRemote) || string.IsNullOrEmpty(currentBranch.MergeWith))
            {
                string defaultRemote = Module.GetRemoteNames().FirstOrDefault(r => r == "origin") ?? Module.GetRemoteNames().OrderBy(r => r).FirstOrDefault();

                pushTo = defaultRemote != null
                    ? $"{defaultRemote}/{currentBranchName} {_untrackedRemote.Text}"
                    : _statusBarBranchWithoutRemote.Text;
            }
            else
            {
                pushTo = $"{currentBranch.TrackingRemote}/{currentBranch.MergeWith}";
            }

            await this.SwitchToMainThreadAsync();

            branchNameLabel.Text = $"{currentBranchName} {char.ConvertFromUtf32(0x2192)}";
            remoteNameLabel.Text = pushTo;

            _branchNameLabelOnClick = (object sender, EventArgs e) => ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                UICommands.StartRemotesDialog(this, null, currentBranchName);
                await UpdateBranchNameDisplayAsync();
            });
            remoteNameLabel.Click += _branchNameLabelOnClick;
            Text = string.Format(_formTitle.Text, currentBranchName, PathUtil.GetDisplayPath(Module.WorkingDir));
        }

        private void Initialize(bool loadUnstaged = true)
        {
            _initialized = true;

            ThreadHelper.JoinableTaskFactory.RunAsync(() => UpdateBranchNameDisplayAsync());

            using (WaitCursorScope.Enter())
            {
                if (loadUnstaged)
                {
                    Loading.Visible = true;
                    Loading.IsAnimating = true;
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
            }

            void UpdateMergeHead()
            {
                _isMergeCommit = Module.RevParse("MERGE_HEAD") != null;
            }
        }

        private void InitializedStaged()
        {
            using (WaitCursorScope.Enter())
            {
                SolveMergeconflicts.Visible = Module.InTheMiddleOfConflictedMerge();
                Staged.SetDiffs(
                    new GitRevision(ObjectId.IndexId),
                    GetHeadRevision(),
                    Module.GetIndexFilesWithSubmodulesStatus());
            }
        }

        /// <summary>
        ///   Loads the unstaged output.
        ///   This method is passed in to the SetTextCallBack delegate
        ///   to set the Text property of textBox1.
        /// </summary>
        private void LoadUnstagedOutput(IReadOnlyList<GitItemStatus> allChangedFiles)
        {
            var lastSelection = _currentFilesList != null
                ? _currentSelection ?? Array.Empty<GitItemStatus>()
                : Array.Empty<GitItemStatus>();

            var unstagedFiles = new List<GitItemStatus>();
            var stagedFiles = new List<GitItemStatus>();

            foreach (var fileStatus in allChangedFiles)
            {
                if (fileStatus.Staged == StagedStatus.Index)
                {
                    stagedFiles.Add(fileStatus);
                }
                else if (fileStatus.Staged == StagedStatus.WorkTree)
                {
                    unstagedFiles.Add(fileStatus);
                }
            }

            Unstaged.SetDiffs(new GitRevision(ObjectId.WorkTreeId), new GitRevision(ObjectId.IndexId), unstagedFiles);
            Staged.SetDiffs(new GitRevision(ObjectId.IndexId), GetHeadRevision(), stagedFiles);

            var doChangesExist = Unstaged.AllItems.Any() || Staged.AllItems.Any();

            Loading.Visible = false;
            Loading.IsAnimating = false;
            LoadingStaged.Visible = false;
            Commit.Enabled = true;
            CommitAndPush.Enabled = true;
            Amend.Enabled = _commitKind == CommitKind.Normal;
            Reset.Enabled = doChangesExist;

            EnableStageButtons(true);
            workingToolStripMenuItem.Enabled = true;

            var inTheMiddleOfConflictedMerge = Module.InTheMiddleOfConflictedMerge();
            SolveMergeconflicts.Visible = inTheMiddleOfConflictedMerge;

            if (Staged.IsEmpty)
            {
                _currentFilesList = Unstaged;
                if (Staged.ContainsFocus)
                {
                    Unstaged.Focus();
                }
            }
            else if (Unstaged.IsEmpty)
            {
                _currentFilesList = Staged;
                if (Unstaged.ContainsFocus)
                {
                    Staged.Focus();
                }
            }

            RestoreSelectedFiles(unstagedFiles, stagedFiles, lastSelection);

            OnStageAreaLoaded?.Invoke();

            if (_loadUnstagedOutputFirstTime)
            {
                var fc = this.FindFocusedControl();

                if (fc == Ok)
                {
                    if (Unstaged.GitItemStatuses.Any())
                    {
                        Unstaged.Focus();
                    }
                    else if (Staged.GitItemStatuses.Any())
                    {
                        Message.Focus();
                    }
                    else
                    {
                        Amend.Focus();
                    }
                }

                _loadUnstagedOutputFirstTime = false;
            }
        }

        private void RestoreSelectedFiles(IReadOnlyList<GitItemStatus> unstagedFiles, IReadOnlyList<GitItemStatus> stagedFiles, IReadOnlyList<GitItemStatus> lastSelection)
        {
            if (_currentFilesList == null || _currentFilesList.IsEmpty)
            {
                SelectStoredNextIndex();
                return;
            }

            var newItems = _currentFilesList == Staged ? stagedFiles : unstagedFiles;
            var names = lastSelection.ToHashSet(x => x.Name);
            var newSelection = newItems.Where(x => names.Contains(x.Name)).ToList();

            if (newSelection.Any())
            {
                _currentFilesList.SelectedItems = newSelection;
            }
            else
            {
                SelectStoredNextIndex();
            }

            return;

            void SelectStoredNextIndex()
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
        }

        private void ShowChanges(GitItemStatus item, bool staged)
        {
            _currentItem = item;
            _currentItemStaged = staged;

            if (item == null)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await SetSelectedDiffAsync(item, staged);
                _selectedDiffReloaded = true;
            }).FileAndForget();

            _stageSelectedLinesToolStripMenuItem.Text = staged ? _unstageSelectedLines.Text : _stageSelectedLines.Text;
            _stageSelectedLinesToolStripMenuItem.Image = staged ? toolUnstageItem.Image : toolStageItem.Image;
            _stageSelectedLinesToolStripMenuItem.ShortcutKeyDisplayString =
                GetShortcutKeyDisplayString(staged ? Command.UnStageSelectedFile : Command.StageSelectedFile);

            return;
        }

        private async Task SetSelectedDiffAsync(GitItemStatus item, bool staged)
        {
            Action openWithDiffTool = () => (staged ? stagedOpenDifftoolToolStripMenuItem9 : openWithDifftoolToolStripMenuItem).PerformClick();
            if (!item.IsTracked || FileHelper.IsImage(item.Name))
            {
                var guid = staged ? ObjectId.IndexId : ObjectId.WorkTreeId;
                await SelectedDiff.ViewGitItemRevisionAsync(item.Name, guid, openWithDiffTool);
            }
            else
            {
                SelectedDiff.ViewCurrentChanges(item, staged, openWithDiffTool);
            }
        }

        private void ClearDiffViewIfNoFilesLeft()
        {
            if ((Staged.IsEmpty && Unstaged.IsEmpty) || (!Unstaged.SelectedItems.Any() && !Staged.SelectedItems.Any()))
            {
                SelectedDiff.Clear();
            }
        }

        private void CommitClick(object sender, EventArgs e)
        {
            ExecuteCommitCommand();
        }

        private void CheckForStagedAndCommit(bool amend, bool push)
        {
            BypassFormActivatedEventHandler(() =>
            {
                if (ConfirmOrStageCommit())
                {
                    DoCommit();
                }
            });

            bool ConfirmOrStageCommit()
            {
                if (amend)
                {
                    return ConfirmAmendCommit();
                }

                if (Staged.IsEmpty)
                {
                    return _isMergeCommit ? ConfirmEmptyMergeCommit() : ConfirmAndStageAllUnstaged();
                }

                return true;

                bool ConfirmAmendCommit()
                {
                    // This is an amend commit.  Confirm the user understands the implications.  We don't want to prompt for an empty
                    // commit, because amend may be used just to change the commit message or timestamp.
                    if (!AppSettings.DontConfirmAmend)
                    {
                        if (MessageBox.Show(this, _amendCommit.Text, _amendCommitCaption.Text, MessageBoxButtons.YesNo) != DialogResult.Yes)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                bool ConfirmEmptyMergeCommit()
                {
                    // it is a merge commit, so user can commit just for merging two branches even the changeset is empty,
                    // but also user may forget to add files, so only ask for confirmation that user really wants to commit an empty changeset
                    if (MessageBox.Show(this, _noFilesStagedAndConfirmAnEmptyMergeCommit.Text, _noStagedChanges.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        return false;
                    }

                    return true;
                }

                bool ConfirmAndStageAllUnstaged()
                {
                    if (Unstaged.IsEmpty)
                    {
                        MessageBox.Show(this, _noFilesStagedAndNothingToCommit.Text, _noStagedChanges.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }

                    // there are no staged files, but there are unstaged files. Most probably user forgot to stage them.
                    if (MessageBox.Show(this, _noFilesStagedButSuggestToCommitAllUnstaged.Text, _noStagedChanges.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        return false;
                    }

                    StageAllAccordingToFilter();

                    // if staging failed (i.e. line endings conflict), user already got error message, don't try to commit empty changeset.
                    if (Staged.IsEmpty)
                    {
                        return false;
                    }

                    return true;
                }
            }

            void DoCommit()
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

                if (_useFormCommitMessage && !IsCommitMessageValid())
                {
                    return;
                }

                if (!AppSettings.DontConfirmCommitIfNoBranch && Module.IsDetachedHead() && !Module.InTheMiddleOfRebase())
                {
                    int idx = PSTaskDialog.cTaskDialog.ShowCommandBox(
                        this,
                        _notOnBranchCaption.Text,
                        _notOnBranchMainInstruction.Text,
                        _notOnBranch.Text,
                        _notOnBranchButtons.Text,
                        true);
                    switch (idx)
                    {
                        case 0:
                            var revisions = _editedCommit != null ? new[] { _editedCommit.ObjectId } : null;
                            if (!UICommands.StartCheckoutBranch(this, revisions))
                            {
                                return;
                            }

                            break;
                        case 1:
                            if (!UICommands.StartCreateBranchDialog(this, _editedCommit?.ObjectId))
                            {
                                return;
                            }

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

                    var scripts = _scriptManager.GetScripts()
                        .Where(x => x.Enabled && x.OnEvent == ScriptEvent.BeforeCommit)
                        .Where(x => x.OnEvent == ScriptEvent.BeforeCheckout);

                    foreach (var script in scripts)
                    {
                        _scriptRunner.RunScript(script);
                    }

                    var commitCmd = Module.CommitCmd(
                        amend,
                        signOffToolStripMenuItem.Checked,
                        toolAuthor.Text,
                        _useFormCommitMessage,
                        noVerifyToolStripMenuItem.Checked,
                        gpgSignCommitToolStripComboBox.SelectedIndex > 0,
                        toolStripGpgKeyTextBox.Text);
                    var errorOccurred = !FormProcess.ShowDialog(this, commitCmd);

                    UICommands.RepoChangedNotifier.Notify();

                    if (errorOccurred)
                    {
                        return;
                    }

                    Amend.Checked = false;
                    noVerifyToolStripMenuItem.Checked = false;

                    scripts = _scriptManager.GetScripts()
                        .Where(x => x.Enabled && x.OnEvent == ScriptEvent.AfterCommit)
                        .Where(x => x.OnEvent == ScriptEvent.BeforeCheckout);

                    foreach (var script in scripts)
                    {
                        _scriptRunner.RunScript(script);
                    }

                    Message.Text = string.Empty; // Message.Text has been used and stored
                    _commitMessageManager.ResetCommitMessage();

                    bool pushCompleted = true;
                    if (push)
                    {
                        bool pushForced = AppSettings.CommitAndPushForcedWhenAmend && amend;
                        UICommands.StartPushDialog(this, true, pushForced, out pushCompleted);
                    }

                    if (pushCompleted && Module.SuperprojectModule != null && AppSettings.StageInSuperprojectAfterCommit)
                    {
                        Module.SuperprojectModule.StageFile(Module.SubmodulePath);
                    }

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
                    {
                        Close();
                    }
                    else
                    {
                        InitializedStaged();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(this, $"Exception: {e.Message}");
                }

                return;

                bool IsCommitMessageValid()
                {
                    if (AppSettings.CommitValidationMaxCntCharsFirstLine > 0)
                    {
                        var firstLine = Message.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
                        if (firstLine.Length > AppSettings.CommitValidationMaxCntCharsFirstLine &&
                            MessageBox.Show(this, _commitMsgFirstLineInvalid.Text, _commitValidationCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.No)
                        {
                            return false;
                        }
                    }

                    if (AppSettings.CommitValidationMaxCntCharsPerLine > 0)
                    {
                        var lines = Message.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                        {
                            if (line.Length > AppSettings.CommitValidationMaxCntCharsPerLine &&
                                MessageBox.Show(this, string.Format(_commitMsgLineInvalid.Text, line), _commitValidationCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.No)
                            {
                                return false;
                            }
                        }
                    }

                    if (AppSettings.CommitValidationSecondLineMustBeEmpty)
                    {
                        var lines = Message.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                        if (lines.Length > 2 &&
                            lines[1].Length != 0 &&
                            MessageBox.Show(this, _commitMsgSecondLineNotEmpty.Text, _commitValidationCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.No)
                        {
                            return false;
                        }
                    }

                    if (!AppSettings.CommitValidationRegEx.IsNullOrEmpty())
                    {
                        try
                        {
                            if (!Regex.IsMatch(Message.Text, AppSettings.CommitValidationRegEx) &&
                                MessageBox.Show(this, _commitMsgRegExNotMatched.Text, _commitValidationCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.No)
                            {
                                return false;
                            }
                        }
                        catch
                        {
                        }
                    }

                    return true;
                }
            }
        }

        /// <summary>
        /// replace the Message.Text in an undo-able way
        /// </summary>
        /// <param name="message">the new message</param>
        private void ReplaceMessage(string message)
        {
            if (Message.Text != message)
            {
                Message.SelectAll();
                Message.SelectedText = message;
            }
        }

        private void RescanChanges()
        {
            if (_shouldRescanChanges)
            {
                toolRefreshItem.Enabled = false;
                Initialize();
                Message.RefreshAutoCompleteWords();
                toolRefreshItem.Enabled = true;
            }
        }

        private void UnstageFilesClick(object sender, EventArgs e)
        {
            if (_currentFilesList != Staged)
            {
                return;
            }

            Unstage();
        }

        private void Staged_DoubleClick(object sender, EventArgs e)
        {
            if (Module.IsBareRepository())
            {
                return;
            }

            _currentFilesList = Staged;
            Unstage();
        }

        private void UnstageAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            var lastSelection = _currentFilesList != null
                ? _currentSelection
                : Array.Empty<GitItemStatus>();

            void StageAreaLoaded()
            {
                _currentFilesList = Unstaged;
                RestoreSelectedFiles(Unstaged.GitItemStatuses, Staged.GitItemStatuses, lastSelection);
                Unstaged.Focus();

                OnStageAreaLoaded -= StageAreaLoaded;
            }

            OnStageAreaLoaded += StageAreaLoaded;

            if (_isMergeCommit)
            {
                Staged.SelectAll();
                Unstage(canUseUnstageAll: false);
            }
            else
            {
                Module.Reset(ResetMode.Mixed);
            }

            Initialize();
        }

        private void UnstagedSelectionChanged(object sender, EventArgs e)
        {
            if (_currentFilesList != Unstaged || _skipUpdate)
            {
                return;
            }

            ClearDiffViewIfNoFilesLeft();

            if (!Unstaged.SelectedItems.Any())
            {
                return;
            }

            Staged.ClearSelected();

            _currentSelection = Unstaged.SelectedItems.ToList();
            GitItemStatus item = Unstaged.SelectedItem;
            ShowChanges(item, false);

            if (!item.IsSubmodule)
            {
                Unstaged.ContextMenuStrip = UnstagedFileContext;
            }
            else
            {
                Unstaged.ContextMenuStrip = UnstagedSubmoduleContext;
            }
        }

        private void UnstagedFileContext_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Do not show if no item selected
            e.Cancel = !Unstaged.SelectedItems.Any() || Module.IsBareRepository();

            var isTrackedSelected = Unstaged.SelectedItems.Any(s => s.IsTracked);
            var isSkipWorktreeExist = Unstaged.SelectedItems.Any(s => s.IsSkipWorktree);
            var isAssumeUnchangedExist = Unstaged.SelectedItems.Any(s => s.IsAssumeUnchanged);
            var isAssumeUnchangedAll = Unstaged.SelectedItems.All(s => s.IsAssumeUnchanged);
            var isSkipWorktreeAll = Unstaged.SelectedItems.All(s => s.IsSkipWorktree);

            openWithDifftoolToolStripMenuItem.Enabled = isTrackedSelected;
            assumeUnchangedToolStripMenuItem.Visible = isTrackedSelected && !isSkipWorktreeExist && !isAssumeUnchangedAll;
            doNotAssumeUnchangedToolStripMenuItem.Visible = showAssumeUnchangedFilesToolStripMenuItem.Checked && !isSkipWorktreeExist && isAssumeUnchangedExist;
            skipWorktreeToolStripMenuItem.Visible = isTrackedSelected && !isAssumeUnchangedExist && !isSkipWorktreeAll;
            doNotSkipWorktreeToolStripMenuItem.Visible = showSkipWorktreeFilesToolStripMenuItem.Checked && !isAssumeUnchangedExist && isSkipWorktreeExist;
            viewFileHistoryToolStripItem.Enabled = isTrackedSelected;
        }

        private void StagedFileContext_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Do not show if no item selected
            e.Cancel = !Staged.SelectedItems.Any() || Module.IsBareRepository();

            var isNewSelected = Staged.SelectedItems.Any(s => s.IsNew);

            stagedFileHistoryToolStripMenuItem6.Enabled = !isNewSelected;
            stagedOpenDifftoolToolStripMenuItem9.Enabled = !isNewSelected;
        }

        private void Unstaged_Enter(object sender, EventArgs e)
        {
            if (_currentFilesList != Unstaged)
            {
                _currentFilesList = Unstaged;
                _skipUpdate = false;
                if (Unstaged.AllItems.Count() != 0 && Unstaged.SelectedIndex == -1)
                {
                    Unstaged.SelectedIndex = 0;
                }

                UnstagedSelectionChanged(Unstaged, null);
            }
        }

        private void Unstage(bool canUseUnstageAll = true)
        {
            if (Module.IsBareRepository())
            {
                return;
            }

            if (canUseUnstageAll &&
                Staged.GitItemStatuses.Count > 10 &&
                Staged.SelectedItems.Count() == Staged.GitItemStatuses.Count)
            {
                UnstageAllToolStripMenuItemClick(null, null);
                return;
            }

            using (WaitCursorScope.Enter())
            {
                EnableStageButtons(false);
                try
                {
                    var lastSelection = _currentFilesList != null
                        ? _currentSelection
                        : Array.Empty<GitItemStatus>();

                    toolStripProgressBar1.Visible = true;
                    toolStripProgressBar1.Maximum = Staged.SelectedItems.Count() * 2;
                    toolStripProgressBar1.Value = 0;
                    Staged.StoreNextIndexToSelect();

                    var files = new List<GitItemStatus>();
                    var allFiles = new List<GitItemStatus>();

                    var shouldRescanChanges = false;
                    foreach (var item in Staged.SelectedItems)
                    {
                        toolStripProgressBar1.Value = Math.Min(toolStripProgressBar1.Maximum - 1, toolStripProgressBar1.Value + 1);
                        if (!item.IsNew)
                        {
                            toolStripProgressBar1.Value = Math.Min(toolStripProgressBar1.Maximum - 1, toolStripProgressBar1.Value + 1);
                            Module.UnstageFileToRemove(item.Name);

                            if (item.IsRenamed)
                            {
                                Module.UnstageFileToRemove(item.OldName);
                            }

                            if (item.IsDeleted)
                            {
                                shouldRescanChanges = true;
                            }
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
                    var unstagedFiles = Unstaged.GitItemStatuses.ToList();
                    foreach (var item in allFiles)
                    {
                        var item1 = item;
                        if (stagedFiles.Exists(i => i.Name == item1.Name))
                        {
                            continue;
                        }

                        var item2 = item;
                        if (unstagedFiles.Exists(i => i.Name == item2.Name))
                        {
                            continue;
                        }

                        if (item.IsNew && !item.IsChanged && !item.IsDeleted)
                        {
                            item.IsTracked = false;
                        }
                        else
                        {
                            item.IsTracked = true;
                        }

                        if (item.IsRenamed)
                        {
                            var clone = new GitItemStatus
                            {
                                Name = item.OldName,
                                IsDeleted = true,
                                IsTracked = true,
                                Staged = StagedStatus.WorkTree
                            };
                            unstagedFiles.Add(clone);

                            item.IsRenamed = false;
                            item.IsNew = true;
                            item.IsTracked = false;
                            item.OldName = string.Empty;
                        }

                        item.Staged = StagedStatus.WorkTree;
                        unstagedFiles.Add(item);
                    }

                    Unstaged.SetDiffs(new GitRevision(ObjectId.WorkTreeId), new GitRevision(ObjectId.IndexId), unstagedFiles);
                    Staged.SetDiffs(new GitRevision(ObjectId.IndexId), GetHeadRevision(), stagedFiles);
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

                    if (shouldRescanChanges)
                    {
                        RescanChanges();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }

                EnableStageButtons(true);
            }

            if (AppSettings.RevisionGraphShowWorkingDirChanges)
            {
                UICommands.RepoChangedNotifier.Notify();
            }
        }

        [CanBeNull]
        private GitRevision GetHeadRevision()
        {
            var headId = Module.RevParse("HEAD");
            if (headId != null)
            {
                return new GitRevision(headId);
            }

            return null;
        }

        private void StageClick(object sender, EventArgs e)
        {
            if (_currentFilesList != Unstaged || Module.IsBareRepository())
            {
                return;
            }

            Stage(Unstaged.SelectedItems.Where(s => !s.IsAssumeUnchanged && !s.IsSkipWorktree).ToList());
            if (Unstaged.IsEmpty)
            {
                Message.Focus();
            }
        }

        private void Unstaged_DoubleClick(object sender, EventArgs e)
        {
            _currentFilesList = Unstaged;
            Stage(Unstaged.SelectedItems.ToList());
            if (Unstaged.IsEmpty)
            {
                Message.Focus();
            }
        }

        private void StageAllAccordingToFilter()
        {
            Stage(Unstaged.GitItemFilteredStatuses.Where(s => !s.IsAssumeUnchanged && !s.IsSkipWorktree).ToList());
            Unstaged.SetFilter(string.Empty);
            if (Unstaged.IsEmpty)
            {
                Message.Focus();
            }
            else
            {
                Staged.Focus();
            }
        }

        private void StageAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            StageAllAccordingToFilter();
        }

        private void StagedSelectionChanged(object sender, EventArgs e)
        {
            if (_currentFilesList != Staged || _skipUpdate)
            {
                return;
            }

            ClearDiffViewIfNoFilesLeft();

            Unstaged.ClearSelected();
            _currentSelection = Staged.SelectedItems.ToList();
            GitItemStatus item = Staged.SelectedItem;
            ShowChanges(item, true);
        }

        private void Staged_DataSourceChanged(object sender, EventArgs e)
        {
            int stagedCount = Staged.UnfilteredItemsCount;
            int totalFilesCount = stagedCount + Unstaged.UnfilteredItemsCount;
            commitStagedCount.Text = stagedCount + "/" + totalFilesCount;
        }

        private void Staged_Enter(object sender, EventArgs e)
        {
            if (_currentFilesList != Staged)
            {
                _currentFilesList = Staged;
                _skipUpdate = false;
                if (Staged.AllItems.Count() != 0 && Staged.SelectedIndex == -1)
                {
                    Staged.SelectedIndex = 0;
                }

                StagedSelectionChanged(Staged, null);
            }
        }

        private void Stage(IReadOnlyList<GitItemStatus> items)
        {
            using (WaitCursorScope.Enter())
            {
                EnableStageButtons(false);
                try
                {
                    var lastSelection = _currentFilesList != null
                        ? _currentSelection
                        : Array.Empty<GitItemStatus>();

                    Unstaged.StoreNextIndexToSelect();
                    toolStripProgressBar1.Visible = true;
                    toolStripProgressBar1.Maximum = items.Count * 2;
                    toolStripProgressBar1.Value = 0;

                    var files = new List<GitItemStatus>();

                    foreach (var item in items)
                    {
                        toolStripProgressBar1.Value = Math.Min(toolStripProgressBar1.Maximum - 1, toolStripProgressBar1.Value + 1);
                        item.Name = item.Name.TrimEnd('/');
                        files.Add(item);
                    }

                    bool wereErrors = false;
                    if (AppSettings.ShowErrorsWhenStagingFiles)
                    {
                        using (var form = new FormStatus(ProcessStart, _stageDetails.Text))
                        {
                            form.ShowDialogOnError(this);
                        }

                        void ProcessStart(FormStatus form)
                        {
                            form.AppendMessageCrossThread(
                                string.Format(
                                    _stageFiles.Text + "\n", files.Count));
                            var output = Module.StageFiles(files, out wereErrors);
                            form.AppendMessageCrossThread(output);
                            form.Done(isSuccess: string.IsNullOrWhiteSpace(output));
                        }
                    }
                    else
                    {
                        Module.StageFiles(files, out wereErrors);
                    }

                    if (wereErrors)
                    {
                        RescanChanges();
                    }
                    else
                    {
                        InitializedStaged();
                        var unstagedFiles = Unstaged.GitItemStatuses.ToList();
                        _skipUpdate = true;
                        var names = new HashSet<string>();
                        foreach (var item in files)
                        {
                            names.Add(item.Name);
                            names.Add(item.OldName);
                        }

                        var unstagedItems = new HashSet<GitItemStatus>();
                        foreach (var item in unstagedFiles)
                        {
                            if (names.Contains(item.Name))
                            {
                                unstagedItems.Add(item);
                            }
                        }

                        unstagedFiles.RemoveAll(item => !item.IsSubmodule && unstagedItems.Contains(item));
                        unstagedFiles.RemoveAll(
                            item => item.IsSubmodule && item.GetSubmoduleStatusAsync().IsCompleted &&
                                    (item.GetSubmoduleStatusAsync().CompletedResult() == null ||
                                     (!item.GetSubmoduleStatusAsync().CompletedResult().IsDirty && unstagedItems.Contains(item))));
                        foreach (var item in unstagedItems.Where(
                            item => item.IsSubmodule &&
                                    (ThreadHelper.JoinableTaskFactory.Run(() => item.GetSubmoduleStatusAsync()) == null ||
                                     (item.GetSubmoduleStatusAsync().IsCompleted && item.GetSubmoduleStatusAsync().CompletedResult().IsDirty))))
                        {
                            item.GetSubmoduleStatusAsync().CompletedResult().Status = SubmoduleStatus.Unknown;
                        }

                        Unstaged.SetDiffs(new GitRevision(ObjectId.WorkTreeId), new GitRevision(ObjectId.IndexId), unstagedFiles);
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
                Amend.Enabled = _commitKind == CommitKind.Normal;
            }

            if (AppSettings.RevisionGraphShowWorkingDirChanges)
            {
                UICommands.RepoChangedNotifier.Notify();
            }
        }

        private void ResetSoftClick(object sender, EventArgs e)
        {
            _shouldRescanChanges = false;
            try
            {
                if (_currentFilesList == null || !_currentFilesList.SelectedItems.Any())
                {
                    return;
                }

                // Show a form asking the user if they want to reset the changes.
                FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(this, _currentFilesList.SelectedItems.Any(item => !item.IsNew), _currentFilesList.SelectedItems.Any(item => item.IsNew));
                if (resetType == FormResetChanges.ActionEnum.Cancel)
                {
                    return;
                }

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
                        {
                            Module.UnstageFileToRemove(item.OldName);
                        }
                    }
                    else
                    {
                        files.Add(item);
                    }
                }

                Module.UnstageFiles(files);

                // remember max selected index
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
                                string path = _fullPathResolver.Resolve(item.Name);
                                if (File.Exists(path))
                                {
                                    File.Delete(path);
                                }
                                else if (Directory.Exists(path))
                                {
                                    Directory.Delete(path, recursive: true);
                                }
                            }
                            catch (IOException)
                            {
                                filesInUse.Add(item.Name);
                            }
                            catch (UnauthorizedAccessException)
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
                {
                    UICommands.RepoChangedNotifier.Notify();
                }

                if (filesInUse.Count > 0)
                {
                    MessageBox.Show(this, "The following files are currently in use and will not be reset:" + Environment.NewLine + "\u2022 " + string.Join(Environment.NewLine + "\u2022 ", filesInUse), "Files In Use");
                }

                if (!string.IsNullOrEmpty(output.ToString()))
                {
                    MessageBox.Show(this, output.ToString(), _resetChangesCaption.Text);
                }
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
                if (Unstaged.SelectedItem == null ||
                    MessageBox.Show(this, _deleteSelectedFiles.Text, _deleteSelectedFilesCaption.Text, MessageBoxButtons.YesNo) !=
                    DialogResult.Yes)
                {
                    return;
                }

                SelectedDiff.Clear();

                Unstaged.StoreNextIndexToSelect();
                foreach (var item in Unstaged.SelectedItems)
                {
                    File.Delete(_fullPathResolver.Resolve(item.Name));
                }

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
            {
                Initialize();
            }
        }

        private void DeleteSelectedFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, _deleteSelectedFiles.Text, _deleteSelectedFilesCaption.Text, MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
            {
                return;
            }

            try
            {
                foreach (var gitItemStatus in Unstaged.SelectedItems)
                {
                    File.Delete(_fullPathResolver.Resolve(gitItemStatus.Name));
                }
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
            {
                return;
            }

            foreach (var gitItemStatus in Unstaged.SelectedItems)
            {
                Module.ResetFile(gitItemStatus.Name);
            }

            Initialize();
        }

        private void ResetAllTrackedChangesToolStripMenuItemClick(object sender, EventArgs e)
        {
            ResetClick(null, null);
        }

        private void resetUnstagedChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetUnStagedClick(null, null);
        }

        private void EditGitIgnoreToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartEditGitIgnoreDialog(this, localExcludes: false);
            Initialize();
        }

        private void EditGitInfoExcludeToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartEditGitIgnoreDialog(this, localExcludes: true);
            Initialize();
        }

        private static string FormatCommitMessageFromTextBox(
            string commitMessageText, bool usingCommitTemplate, bool ensureCommitMessageSecondLineEmpty)
        {
            if (commitMessageText == null)
            {
                return string.Empty;
            }

            var formattedCommitMessage = new StringBuilder();

            var lineNumber = 1;
            foreach (var line in commitMessageText.Split('\n'))
            {
                string nonNullLine = line ?? string.Empty;

                // When a committemplate is used, skip comments and do not count them as line.
                // otherwise: "#" is probably not used for comment but for issue number
                if (usingCommitTemplate && nonNullLine.StartsWith("#"))
                {
                    continue;
                }

                if (ensureCommitMessageSecondLineEmpty && lineNumber == 2 && !string.IsNullOrEmpty(nonNullLine))
                {
                    formattedCommitMessage.AppendLine();
                }

                formattedCommitMessage.AppendLine(nonNullLine);

                lineNumber++;
            }

            return formattedCommitMessage.ToString();
        }

        private void SetCommitMessageFromTextBox(string commitMessageText)
        {
            // Save last commit message in settings. This way it can be used in multiple repositories.
            AppSettings.LastCommitMessage = commitMessageText;

            var path = _commitMessageManager.CommitMessagePath;

            var formattedCommitMessage = FormatCommitMessageFromTextBox(
                commitMessageText, usingCommitTemplate: !string.IsNullOrEmpty(_commitTemplate), AppSettings.EnsureCommitMessageSecondLineEmpty);

            // Commit messages are UTF-8 by default unless otherwise in the config file.
            // The git manual states:
            //  git commit and git commit-tree issues a warning if the commit log message
            //  given to it does not look like a valid UTF-8 string, unless you
            //  explicitly say your project uses a legacy encoding. The way to say
            //  this is to have i18n.commitencoding in .git/config file, like this:...
            Encoding encoding = Module.CommitEncoding;

            File.WriteAllText(path, formattedCommitMessage, encoding);
        }

        private void DeleteAllUntrackedFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(this,
                _deleteUntrackedFiles.Text,
                _deleteUntrackedFilesCaption.Text,
                MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
            {
                return;
            }

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
            RescanChanges();
        }

        private void ShowSkipWorktreeFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            showSkipWorktreeFilesToolStripMenuItem.Checked = !showSkipWorktreeFilesToolStripMenuItem.Checked;
            RescanChanges();
        }

        private void CommitMessageToolStripMenuItemDropDownOpening(object sender, EventArgs e)
        {
            var msg = AppSettings.LastCommitMessage;
            var maxCount = AppSettings.CommitDialogNumberOfPreviousMessages;

            var prevMessages = Module.GetPreviousCommitMessages(maxCount)
                .Select(message => message.TrimEnd('\n'))
                .ToList();

            if (!string.IsNullOrWhiteSpace(msg) && !prevMessages.Contains(msg))
            {
                // If the list is already full
                if (prevMessages.Count == maxCount)
                {
                    // Remove the last item
                    prevMessages.RemoveAt(maxCount - 1);
                }

                // Insert the last commit message as the first entry
                prevMessages.Insert(0, msg);
            }

            commitMessageToolStripMenuItem.DropDownItems.Clear();

            foreach (var prevMsg in prevMessages)
            {
                AddCommitMessageToMenu(prevMsg);
            }

            commitMessageToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                toolStripMenuItem1,
                generateListOfChangesInSubmodulesChangesToolStripMenuItem
            });

            void AddCommitMessageToMenu(string commitMessage)
            {
                const int maxLabelLength = 50;

                string label;

                if (commitMessage.Length <= maxLabelLength)
                {
                    label = commitMessage;
                }
                else
                {
                    var newlineIndex = commitMessage.IndexOf('\n');

                    if (newlineIndex != -1 && newlineIndex <= maxLabelLength)
                    {
                        label = commitMessage.Substring(0, newlineIndex);
                    }
                    else
                    {
                        label = commitMessage.ShortenTo(maxLabelLength);
                    }
                }

                commitMessageToolStripMenuItem.DropDownItems.Add(new ToolStripMenuItem
                {
                    Tag = commitMessage,
                    Text = label
                });
            }
        }

        private void CommitMessageToolStripMenuItemDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Tag != null)
            {
                ReplaceMessage(((string)e.ClickedItem.Tag).Trim());
            }
        }

        private void generateListOfChangesInSubmodulesChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var stagedFiles = Staged.AllItems;

            var configFile = Module.GetSubmoduleConfigFile();

            Dictionary<string, string> modules = stagedFiles
                .Where(item => item.IsSubmodule)
                .Select(item => item.Name)
                .ToDictionary(localPath =>
                {
                    var submodule = configFile.ConfigSections.FirstOrDefault(section => section.GetValue("path").Trim() == localPath);
                    return submodule?.SubSection.Trim();
                });

            if (modules.Count == 0)
            {
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Submodule" + (modules.Count == 1 ? " " : "s ") +
                string.Join(", ", modules.Keys) + " updated");
            sb.AppendLine();

            foreach (var (path, name) in modules)
            {
                var args = new GitArgumentBuilder("diff")
                {
                    "--cached",
                    "-z",
                    "--",
                    name.QuoteNE()
                };
                string diff = Module.GitExecutable.GetOutput(args);
                var lines = diff.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                const string subprojectCommit = "Subproject commit ";
                var from = lines.Single(s => s.StartsWith("-" + subprojectCommit)).Substring(subprojectCommit.Length + 1);
                var to = lines.Single(s => s.StartsWith("+" + subprojectCommit)).Substring(subprojectCommit.Length + 1);
                if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
                {
                    sb.AppendLine("Submodule " + path + ":");
                    var module = new GitModule(_fullPathResolver.Resolve(name.EnsureTrailingPathSeparator()));
                    args = new GitArgumentBuilder("log")
                    {
                        "--pretty=format:\"    %m %h - %s\"",
                        "--no-merges",
                        $"{from}...{to}"
                    };

                    string log = module.GitExecutable.GetOutput(args);

                    if (log.Length != 0)
                    {
                        sb.AppendLine(log);
                    }
                    else
                    {
                        sb.AppendLine("    * Revision changed to " + to.Substring(0, 7));
                    }

                    sb.AppendLine();
                }
            }

            ReplaceMessage(sb.ToString().TrimEnd());
        }

        private void AddFileToGitIgnoreToolStripMenuItemClick(object sender, EventArgs e)
        {
            HandleExcludeFileClick(false);
        }

        private void AddFileToGitInfoExcludeToolStripMenuItemClick(object sender, EventArgs e)
        {
            HandleExcludeFileClick(true);
        }

        private void HandleExcludeFileClick(bool localExclude)
        {
            if (!Unstaged.SelectedItems.Any())
            {
                return;
            }

            SelectedDiff.Clear();
            var fileNames = Unstaged.SelectedItems.Select(item => "/" + item.Name).ToArray();
            if (UICommands.StartAddToGitIgnoreDialog(this, localExclude, fileNames))
            {
                Initialize();
            }
        }

        private void AssumeUnchangedToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!Unstaged.SelectedItems.Any())
            {
                return;
            }

            SelectedDiff.Clear();

            Module.AssumeUnchangedFiles(Unstaged.SelectedItems.ToList(), true, out _);

            Initialize();
        }

        private void DoNotAssumeUnchangedToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!Unstaged.SelectedItems.Any())
            {
                return;
            }

            SelectedDiff.Clear();

            Module.AssumeUnchangedFiles(Unstaged.SelectedItems.ToList(), false, out _);

            Initialize();
        }

        private void SkipWorktreeToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!Unstaged.SelectedItems.Any())
            {
                return;
            }

            SelectedDiff.Clear();

            Module.SkipWorktreeFiles(Unstaged.SelectedItems.ToList(), true);

            Initialize();
        }

        private void DoNotSkipWorktreeToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!Unstaged.SelectedItems.Any())
            {
                return;
            }

            SelectedDiff.Clear();

            Module.SkipWorktreeFiles(Unstaged.SelectedItems.ToList(), false);

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
            if (!SenderToFileStatusList(sender, out var list) || !list.SelectedItems.Any())
            {
                return;
            }

            var fileName = list.SelectedItem.Name;
            var path = _fullPathResolver.Resolve(fileName).ToNativePath();

            try
            {
                Process.Start(path);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                OsShellUtil.OpenAs(path);
            }
        }

        private void OpenWithToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!SenderToFileStatusList(sender, out var list) || !list.SelectedItems.Any())
            {
                return;
            }

            var item = list.SelectedItem;
            var fileName = item.Name;

            OsShellUtil.OpenAs(_fullPathResolver.Resolve(fileName.ToNativePath()));
        }

        private void FilenameToClipboardToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!SenderToFileStatusList(sender, out var list) || !list.SelectedItems.Any())
            {
                return;
            }

            var fileNames = new StringBuilder();
            foreach (var item in list.SelectedItems)
            {
                // Only use append line when multiple items are selected.
                // This to make it easier to use the text from clipboard when 1 file is selected.
                if (fileNames.Length > 0)
                {
                    fileNames.AppendLine();
                }

                fileNames.Append(_fullPathResolver.Resolve(item.Name).ToNativePath());
            }

            ClipboardUtil.TrySetText(fileNames.ToString());
        }

        private void OpenFilesWithDiffTool(IEnumerable<GitItemStatus> items, string firstRevision, string secondRevision)
        {
            foreach (var item in items)
            {
                string output = Module.OpenWithDifftool(item.Name, null, firstRevision, secondRevision, "", item.IsTracked);
                if (!string.IsNullOrEmpty(output))
                {
                    MessageBox.Show(this, output);
                }
            }
        }

        private void OpenWithDifftoolToolStripMenuItemClick(object sender, EventArgs e)
        {
            OpenFilesWithDiffTool(Unstaged.SelectedItems, GitRevision.IndexGuid, GitRevision.WorkTreeGuid);
        }

        private void ResetPartOfFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            var items = Unstaged.SelectedItems.ToList();

            if (items.Count != 1)
            {
                MessageBox.Show(this, _onlyStageChunkOfSingleFileError.Text, _resetStageChunkOfFileCaption.Text);
                return;
            }

            var item = items.Single();

            ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        await TaskScheduler.Default;
                        await Module.ResetInteractiveAsync(item);
                        await this.SwitchToMainThreadAsync();
                        Initialize();
                    })
                .FileAndForget();
        }

        private void ResetClick(object sender, EventArgs e)
        {
            HandleResetButton(onlyUnstaged: false);
        }

        private void ResetUnStagedClick(object sender, EventArgs e)
        {
            HandleResetButton(onlyUnstaged: true);
        }

        private void HandleResetButton(bool onlyUnstaged)
        {
            BypassFormActivatedEventHandler(() => UICommands.StartResetChangesDialog(this, Unstaged.AllItems.ToList(), onlyWorkTree: onlyUnstaged));
            Initialize();
        }

        private void BypassFormActivatedEventHandler(Action action)
        {
            try
            {
                _bypassActivatedEventHandler = true;

                action();
            }
            finally
            {
                _bypassActivatedEventHandler = false;
            }
        }

        private void ShowUntrackedFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            showUntrackedFilesToolStripMenuItem.Checked = !showUntrackedFilesToolStripMenuItem.Checked;
            RescanChanges();
        }

        private void editFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!SenderToFileStatusList(sender, out var list))
            {
                return;
            }

            var item = list.SelectedItem;
            if (item == null)
            {
                return;
            }

            var fileName = _fullPathResolver.Resolve(item.Name);
            UICommands.StartFileEditorDialog(fileName);

            UnstagedSelectionChanged(null, null);
        }

        private void CommitAndPush_Click(object sender, EventArgs e)
        {
            CheckForStagedAndCommit(Amend.Checked, push: true);
        }

        private void UpdateAuthorInfo()
        {
            var userName = Module.GetEffectiveSetting(SettingKeyString.UserName);
            var userEmail = Module.GetEffectiveSetting(SettingKeyString.UserEmail);

            var committer = $"{_commitCommitterInfo.Text} {userName} <{userEmail}>";

            commitAuthorStatus.Text = string.IsNullOrEmpty(toolAuthor.Text?.Trim())
                ? committer
                : $"{committer} {_commitAuthorInfo.Text} {toolAuthor.Text}";
        }

        private bool SenderToFileStatusList(object sender, out FileStatusList list)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            ContextMenuStrip menu = item?.Owner as ContextMenuStrip;
            ListView lv = menu?.SourceControl as ListView;

            list = lv?.Parent as FileStatusList;
            if (list == null /* menu action triggered directly by hotkey */)
            {
                // The inactive list's selection has been cleared.
                list = Staged.SelectedItems.Any() ? Staged :
                    Unstaged.SelectedItems.Any() ? Unstaged : null;
            }

            return list != null;
        }

        private void ViewFileHistoryMenuItem_Click(object sender, EventArgs e)
        {
            if (!SenderToFileStatusList(sender, out var list))
            {
                return;
            }

            if (list.SelectedItems.Count() == 1)
            {
                UICommands.StartFileHistoryDialog(this, list.SelectedItem.Name);
            }
            else
            {
                MessageBox.Show(this, _selectOnlyOneFile.Text, _error.Text);
            }
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
            CheckForStagedAndCommit(Amend.Checked, push: false);
        }

        private void Message_KeyDown(object sender, KeyEventArgs e)
        {
            // Prevent adding a line break when all we want is to commit
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
            }
        }

        private void Message_TextChanged(object sender, EventArgs e)
        {
            // Format text, except when doing an undo, because
            // this would itself introduce more steps that
            // need to be undone.
            if (!Message.IsUndoInProgress)
            {
                // always format from 0 to handle pasted text
                FormatAllText(0);
            }
        }

        private void Message_TextAssigned(object sender, EventArgs e)
        {
            Message_TextChanged(sender, e);
        }

        private void FormatAllText(int startLine)
        {
            int limit1 = AppSettings.CommitValidationMaxCntCharsFirstLine;
            int limitX = AppSettings.CommitValidationMaxCntCharsPerLine;
            bool empty2 = AppSettings.CommitValidationSecondLineMustBeEmpty;
            bool commitValidationAutoWrap = AppSettings.CommitValidationAutoWrap;
            bool commitValidationIndentAfterFirstLine = AppSettings.CommitValidationIndentAfterFirstLine;

            var lineCount = Message.LineCount();

            TrimFormattedLines();

            for (int line = startLine; line < lineCount; line++)
            {
                if (DidFormattedLineChange(line))
                {
                    bool lineChanged = FormatLine(line);
                    SetFormattedLine(line);
                    if (lineChanged)
                    {
                        FormatAllText(line);
                    }
                }
            }

            return;

            void TrimFormattedLines()
            {
                if (_formattedLines.Count > lineCount)
                {
                    _formattedLines.RemoveRange(lineCount, _formattedLines.Count - lineCount);
                }
            }

            bool DidFormattedLineChange(int lineNumber)
            {
                return _formattedLines.Count <= lineNumber ||
                       !_formattedLines[lineNumber].Equals(Message.Line(lineNumber), StringComparison.OrdinalIgnoreCase);
            }

            bool FormatLine(int line)
            {
                var changed = false;

                if (limit1 > 0 && line == 0)
                {
                    ColorTextAsNecessary(limit1, fullRefresh: false);
                }

                if (empty2 && line == 1)
                {
                    // Ensure next line. Optionally add a bullet.
                    Message.EnsureEmptyLine(commitValidationIndentAfterFirstLine, 1);
                    Message.ChangeTextColor(2, 0, Message.LineLength(2), Color.Black);
                    if (FormatLine(2))
                    {
                        changed = true;
                    }
                }

                if (limitX > 0 && line >= (empty2 ? 2 : 1))
                {
                    if (commitValidationAutoWrap && WrapIfNecessary())
                    {
                        changed = true;
                    }

                    ColorTextAsNecessary(limitX, changed);
                }

                return changed;

                void ColorTextAsNecessary(int lineLimit, bool fullRefresh)
                {
                    var lineLength = Message.LineLength(line);
                    int offset = 0;
                    bool textAppended = false;
                    if (!fullRefresh && _formattedLines.Count > line)
                    {
                        offset = _formattedLines[line].CommonPrefix(Message.Line(line)).Length;
                        textAppended = offset > 0 && offset == _formattedLines[line].Length;
                    }

                    int len = Math.Min(lineLimit, lineLength) - offset;

                    if (!textAppended && len > 0)
                    {
                        Message.ChangeTextColor(line, offset, len, Color.Black);
                    }

                    if (lineLength > lineLimit)
                    {
                        if (offset <= lineLimit || !textAppended)
                        {
                            offset = Math.Max(offset, lineLimit);
                            len = lineLength - offset;
                            if (len > 0)
                            {
                                Message.ChangeTextColor(line, offset, len, Color.Red);
                            }
                        }
                    }
                }

                bool WrapIfNecessary()
                {
                    if (Message.LineLength(line) > limitX)
                    {
                        var oldText = Message.Line(line);
                        var newText = WordWrapper.WrapSingleLine(oldText, limitX);
                        if (!string.Equals(oldText, newText))
                        {
                            Message.ReplaceLine(line, newText);
                            return true;
                        }
                    }

                    return false;
                }
            }

            void SetFormattedLine(int lineNumber)
            {
                // line not formatted yet
                if (_formattedLines.Count <= lineNumber)
                {
                    Debug.Assert(_formattedLines.Count == lineNumber, $"{_formattedLines.Count}:{lineNumber}");
                    _formattedLines.Add(Message.Line(lineNumber));
                }
                else
                {
                    _formattedLines[lineNumber] = Message.Line(lineNumber);
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

        private void signOffToolStripMenuItem_Click(object sender, EventArgs e)
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
            UpdateAuthorInfo();
        }

        private void gpgSignCommitChanged(object sender, EventArgs e)
        {
            // Change the icon for commit button
            Commit.Image = gpgSignCommitToolStripComboBox.SelectedIndex > 0
                ? Images.Key
                : Images.RepoStateClean;

            toolStripGpgKeyTextBox.Visible = gpgSignCommitToolStripComboBox.SelectedIndex == 2;
        }

        #region Selection filtering

        private void SetVisibilityOfSelectionFilter(bool visible)
        {
            selectionFilterToolStripMenuItem.Checked = visible;
            toolbarSelectionFilter.Visible = visible;
        }

        private void OnSelectionFilterTextChanged(object sender, EventArgs e)
        {
            _selectionFilterSubject.OnNext(selectionFilter.Text);
        }

        private void OnSelectionFilterIndexChanged(object sender, EventArgs e)
        {
            Unstaged.SetSelectionFilter(selectionFilter.Text);
        }

        private void ToggleShowSelectionFilter(object sender, EventArgs e)
        {
            var visible = !AppSettings.CommitDialogSelectionFilter;

            AppSettings.CommitDialogSelectionFilter = visible;
            toolbarSelectionFilter.Visible = visible;
        }

        #endregion

        private void commitSubmoduleChanges_Click(object sender, EventArgs e)
        {
            var submoduleCommands = new GitUICommands(_fullPathResolver.Resolve(_currentItem.Name.EnsureTrailingPathSeparator()));
            submoduleCommands.StartCommitDialog(this);
            Initialize();
        }

        private void openSubmoduleMenuItem_Click(object sender, EventArgs e)
        {
            var submoduleName = Unstaged.SelectedItem.Name;

            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    var status = await Unstaged.SelectedItem.GetSubmoduleStatusAsync().ConfigureAwait(false);

                    var process = new Process
                    {
                        StartInfo =
                        {
                            FileName = Application.ExecutablePath,
                            Arguments = "browse -commit=" + status.Commit,
                            WorkingDirectory = _fullPathResolver.Resolve(submoduleName.EnsureTrailingPathSeparator())
                        }
                    };

                    process.Start();
                });
        }

        private void resetSubmoduleChanges_Click(object sender, EventArgs e)
        {
            var unstagedFiles = Unstaged.SelectedItems.ToList();
            if (unstagedFiles.Count == 0)
            {
                return;
            }

            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(this, true, true);
            if (resetType == FormResetChanges.ActionEnum.Cancel)
            {
                return;
            }

            foreach (var item in unstagedFiles.Where(it => it.IsSubmodule))
            {
                GitModule module = Module.GetSubmodule(item.Name);

                // Reset all changes.
                module.Reset(ResetMode.Hard);

                // Also delete new files, if requested.
                if (resetType == FormResetChanges.ActionEnum.ResetAndDelete)
                {
                    module.Clean(CleanMode.OnlyNonIgnored, directories: true);
                }
            }

            Initialize();
        }

        private void updateSubmoduleMenuItem_Click(object sender, EventArgs e)
        {
            var unstagedFiles = Unstaged.SelectedItems.ToList();
            if (unstagedFiles.Count == 0)
            {
                return;
            }

            foreach (var item in unstagedFiles.Where(it => it.IsSubmodule))
            {
                FormProcess.ShowDialog(this, GitCommandHelpers.SubmoduleUpdateCmd(item.Name));
            }

            Initialize();
        }

        private void stashSubmoduleChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var unstagedFiles = Unstaged.SelectedItems.ToList();
            if (unstagedFiles.Count == 0)
            {
                return;
            }

            foreach (var item in unstagedFiles.Where(it => it.IsSubmodule))
            {
                var commands = new GitUICommands(Module.GetSubmodule(item.Name));
                commands.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash);
            }

            Initialize();
        }

        private void submoduleSummaryMenuItem_Click(object sender, EventArgs e)
        {
            string summary = Module.GetSubmoduleSummary(_currentItem.Name);
            using (var frm = new FormEdit(UICommands, summary))
            {
                frm.ShowDialog(this);
            }
        }

        private void commitTemplatesToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var registeredTemplatesCount = _commitTemplateManager.RegisteredTemplates.Count();
            if (_shouldReloadCommitTemplates || _alreadyLoadedTemplatesCount != registeredTemplatesCount)
            {
                LoadCommitTemplates();
                _shouldReloadCommitTemplates = false;
                _alreadyLoadedTemplatesCount = registeredTemplatesCount;
            }

            return;

            void LoadCommitTemplates()
            {
                commitTemplatesToolStripMenuItem.DropDownItems.Clear();

                // Add registered templates
                foreach (var item in _commitTemplateManager.RegisteredTemplates)
                {
                    CreateToolStripItem(item);
                }

                AddSeparator();

                // Add templates from settings
                foreach (var item in CommitTemplateItem.LoadFromSettings() ?? Array.Empty<CommitTemplateItem>())
                {
                    CreateToolStripItem(item);
                }

                AddSeparator();

                // Add a settings item
                AddSettingsItem();

                return;

                void CreateToolStripItem(CommitTemplateItem item)
                {
                    if (string.IsNullOrEmpty(item.Name))
                    {
                        return;
                    }

                    var toolStripItem = new ToolStripMenuItem(item.Name);
                    toolStripItem.Click += delegate
                    {
                        try
                        {
                            ReplaceMessage(item.Text);
                            Message.Focus();
                        }
                        catch
                        {
                        }
                    };
                    commitTemplatesToolStripMenuItem.DropDownItems.Add(toolStripItem);
                }

                void AddSeparator()
                {
                    if (commitTemplatesToolStripMenuItem.DropDownItems.Count != 0)
                    {
                        commitTemplatesToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
                    }
                }

                void AddSettingsItem()
                {
                    var settingsItem = new ToolStripMenuItem(_commitTemplateSettings.Text);
                    settingsItem.Click += delegate
                    {
                        using (var frm = new FormCommitTemplateSettings())
                        {
                            frm.ShowDialog(this);
                        }

                        _shouldReloadCommitTemplates = true;
                    };
                    commitTemplatesToolStripMenuItem.DropDownItems.Add(settingsItem);
                }
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
                fileNames.Append(_fullPathResolver.Resolve(item.Name).ToNativePath());

                string filePath = fileNames.ToString();
                if (File.Exists(filePath))
                {
                    OsShellUtil.SelectPathInFileExplorer(filePath);
                }
            }
        }

        private void stagedOpenDifftoolToolStripMenuItem9_Click(object sender, EventArgs e)
        {
            OpenFilesWithDiffTool(Staged.SelectedItems, "HEAD", GitRevision.IndexGuid);
        }

        private void openFolderToolStripMenuItem10_Click(object sender, EventArgs e)
        {
            OpenContainingFolder(Staged);
        }

        private void interactiveAddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = Unstaged.SelectedItem;

            if (item == null)
            {
                return;
            }

            var token = _interactiveAddSequence.Next();

            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    await TaskScheduler.Default;
                    await Module.AddInteractiveAsync(item);
                    await this.SwitchToMainThreadAsync(token);
                    RescanChanges();
                })
                .FileAndForget();
        }

        private void Amend_CheckedChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Message.Text) && Amend.Checked)
            {
                ReplaceMessage(Module.GetPreviousCommitMessages(1).FirstOrDefault()?.Trim());
            }

            if (AppSettings.CommitAndPushForcedWhenAmend)
            {
                CommitAndPush.BackColor = Amend.Checked ? Color.Salmon : SystemColors.ButtonFace;
            }
        }

        private void StageInSuperproject_CheckedChanged(object sender, EventArgs e)
        {
            if (StageInSuperproject.Visible)
            {
                AppSettings.StageInSuperprojectAfterCommit = StageInSuperproject.Checked;
            }
        }

        private void commitCommitter_Click(object sender, EventArgs e)
        {
            UICommands.StartSettingsDialog(this, SettingsDialog.Pages.GitConfigSettingsPage.GetPageReference());
        }

        private void toolAuthor_Leave(object sender, EventArgs e)
        {
            UpdateAuthorInfo();
        }

        private void createBranchToolStripButton_Click(object sender, EventArgs e)
        {
            var branchCreated = UICommands.StartCreateBranchDialog(this);
            if (!branchCreated)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(() => UpdateBranchNameDisplayAsync());
        }

        private void Message_Enter(object sender, EventArgs e)
        {
            if (Staged.AllItemsCount != 0 && !Staged.SelectedItems.Any())
            {
                _currentFilesList = Staged;
                Staged.SelectedIndex = 0;
                StagedSelectionChanged(null, null);
            }
        }

        internal TestAccessor GetTestAccessor()
            => new TestAccessor(this);

        internal readonly struct TestAccessor
        {
            private readonly FormCommit _formCommit;

            internal TestAccessor(FormCommit formCommit)
            {
                _formCommit = formCommit;
            }

            internal ToolStripMenuItem EditFileToolStripMenuItem => _formCommit.editFileToolStripMenuItem;

            internal FileStatusList UnstagedList => _formCommit.Unstaged;

            internal EditNetSpell Message => _formCommit.Message;

            internal FileViewer SelectedDiff => _formCommit.SelectedDiff;

            internal ToolStripDropDownButton CommitMessageToolStripMenuItem => _formCommit.commitMessageToolStripMenuItem;

            internal ToolStripStatusLabel CommitAuthorStatusToolStripStatusLabel => _formCommit.commitAuthorStatus;

            internal ToolStripStatusLabel CurrentBranchNameLabelStatus => _formCommit.branchNameLabel;

            internal ToolStripStatusLabel RemoteNameLabelStatus => _formCommit.remoteNameLabel;

            internal CommandStatus ExecuteCommand(Command command) => _formCommit.ExecuteCommand((int)command);

            internal static string FormatCommitMessageFromTextBox(
                string commitMessageText, bool usingCommitTemplate, bool ensureCommitMessageSecondLineEmpty)
                => FormCommit.FormatCommitMessageFromTextBox(commitMessageText, usingCommitTemplate, ensureCommitMessageSecondLineEmpty);
        }

        private void stopTrackingThisFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Unstaged.SelectedItem == null || !Unstaged.SelectedItem.IsTracked)
            {
                return;
            }

            var filename = Unstaged.SelectedItem.Name;

            if (Module.StopTrackingFile(filename))
            {
                RescanChanges();
            }
            else
            {
                MessageBox.Show(string.Format(_stopTrackingFail.Text, filename), _error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
