using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.AutoCompletion;
using GitUI.CommandsDialogs.CommitDialog;
using GitUI.Editor;
using GitUI.HelperDialogs;
using GitUI.Properties;
using GitUI.ScriptsEngine;
using GitUI.SpellChecker;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs;

public sealed partial class FormCommit : GitModuleForm
{
    private const string _resetSoftRevision = "HEAD~1";

    #region Translation

    private readonly TranslationString _amendCommit
        = new("You are about to rewrite history." + Environment.NewLine
            + "Only use Amend if the commit has not been published yet!" + Environment.NewLine
            + Environment.NewLine
            + "Do you want to continue?");

    private readonly TranslationString _amendResetSoft
        = new("You are about to rewrite history by Soft Reset to the previous commit." + Environment.NewLine
            + "Only use Amend / Reset if the commit has not been published yet!" + Environment.NewLine
            + Environment.NewLine
            + "Do you want to continue?");

    private readonly TranslationString _amendCommitCaption = new("Amend commit");

    private readonly TranslationString _commitAndPush = new("Commit && &push");

    private readonly TranslationString _commitAndForcePush = new("Commit && force &push");

    private readonly TranslationString _enterCommitMessage = new("Please enter commit message");
    private readonly TranslationString _enterCommitMessageCaption = new("Commit message");
    private readonly TranslationString _commitMessageDisabled = new("Commit Message is requested during commit");

    private readonly TranslationString _enterCommitMessageHint = new("Enter commit message");

    private readonly TranslationString _mergeConflicts =
        new("There are unresolved merge conflicts, solve merge conflicts before committing.");

    private readonly TranslationString _mergeConflictsCaption = new("Merge conflicts");

    private readonly TranslationString _noFilesStagedAndConfirmAnEmptyMergeCommit =
        new("There are no files staged for this commit.\nAre you sure you want to commit?");
    private readonly TranslationString _noFilesStagedCommitAllFilteredUnstagedOption =
        new("Stage and commit the unstaged files that match your filter");
    private readonly TranslationString _noFilesStagedCommitAllUnstagedOption =
        new("Stage and commit all unstaged files");
    private readonly TranslationString _noFilesStagedMakeEmptyCommitOption =
        new("Make an empty commit");
    private readonly TranslationString _noFilesStagedCommitCaption =
        new("Confirm commit");
    private readonly TranslationString _noFilesStagedCommitInstructions =
        new("There aren't any changes in the staging area.\nHow do you want to proceed?");

    private readonly TranslationString _noStagedChanges = new("There are no staged changes");
    private readonly TranslationString _noUnstagedChanges = new("There are no unstaged changes");

    private readonly TranslationString _notOnBranch =
        new("This commit will be unreferenced when switching to another branch and can be lost." +
                              Environment.NewLine + Environment.NewLine + "Do you want to continue?");

    private readonly TranslationString _stageDetails = new("Stage Details");
    private readonly TranslationString _stageFiles = new("Stage {0} files");

    private readonly TranslationString _stageAll = new("Stage all");
    private readonly TranslationString _stageFiltered = new("Stage filtered");
    private readonly TranslationString _unstageAll = new("Unstage all");
    private readonly TranslationString _unstageFiltered = new("Unstage filtered");

    private readonly TranslationString _addSelectionToCommitMessage = new("Add selection to commit message");
    private readonly TranslationString _formTitle = new("Commit to {0} ({1})");

    private readonly TranslationString _selectionFilterToolTip = new("Enter a regular expression to select unstaged files.");
    private readonly TranslationString _selectionFilterErrorToolTip = new("Error {0}");

    private readonly TranslationString _commitMsgFirstLineInvalid = new("First line of commit message contains too many characters."
        + Environment.NewLine + "Do you want to continue?");

    private readonly TranslationString _commitMsgLineInvalid = new("The following line of commit message contains too many characters:"
        + Environment.NewLine + Environment.NewLine + "{0}" + Environment.NewLine + Environment.NewLine + "Do you want to continue?");

    private readonly TranslationString _commitMsgSecondLineNotEmpty = new("Second line of commit message is not empty." + Environment.NewLine + "Do you want to continue?");

    private readonly TranslationString _commitMsgRegExNotMatched = new("Commit message does not match RegEx." + Environment.NewLine + "Do you want to continue?");

    private readonly TranslationString _commitValidationCaption = new("Commit validation");

    private readonly TranslationString _commitMessageSettings = new("&Edit commit message templates and settings...");
    private readonly TranslationString _conventionalCommit = new("Conven&tional Commits");
    private readonly TranslationString _conventionalCommitDocumentation = new("Documentation...");

    private readonly TranslationString _commitAuthorInfo = new("Author");
    private readonly TranslationString _commitCommitterInfo = new("Committer");
    private readonly TranslationString _commitCommitterToolTip = new("Click to change committer information.");

    private readonly TranslationString _modifyCommitMessageButtonToolTip
        = new("If you change the first line of the commit message, git will treat this commit as an ordinary commit," + Environment.NewLine
                                + "i.e. it may no longer be a fixup or an autosquash commit.");

    private readonly TranslationString _templateNotFoundCaption = new("Template Error");
    private readonly TranslationString _templateNotFound = new($"Template not found: {{0}}.{Environment.NewLine}{Environment.NewLine}You can set your template:{Environment.NewLine}\t$ git config commit.template ./.git_commit_msg.txt{Environment.NewLine}You can unset the template:{Environment.NewLine}\t$ git config --unset commit.template");
    private readonly TranslationString _templateLoadErrorCaption = new("Template could not be loaded");

    private readonly TranslationString _statusBarBranchWithoutRemote = new("(remote not configured)");
    private readonly TranslationString _untrackedRemote = new("(untracked)");
    #endregion

    private event Action? OnStageAreaLoaded;

    private readonly ICommitTemplateManager _commitTemplateManager;
    private readonly GitRevision? _editedCommit;
    private readonly ToolStripMenuItem _addSelectionToCommitMessageToolStripMenuItem;
    private readonly AsyncLoader _unstagedLoader = new();
    private readonly bool _useFormCommitMessage = AppSettings.UseFormCommitMessage;
    private readonly CancellationTokenSequence _customDiffToolsSequence = new();
    private readonly CancellationTokenSequence _interactiveAddSequence = new();
    private readonly CancellationTokenSequence _viewChangesSequence = new();
    private readonly SplitterManager _splitterManager = new(new AppSettingsPath("CommitDialog"));
    private readonly Subject<string> _selectionFilterSubject = new();
    private readonly IFullPathResolver _fullPathResolver;
    private readonly List<string> _formattedLines = [];

    private const string _feat = "feat";

    private static readonly string[] _headerCommitTypes = ["build", "chore", "ci", "docs", _feat, "fix", "perf", "refactor", "style", "test"];
    private static readonly string[] _footerKeywords = ["BREAKING CHANGE", "Co-authored-by", "Reviewed-by"];

    private bool _insertScopeParentheses;
    private CommitKind _commitKind;
    private FileStatusList _currentFilesList;
    private bool _skipUpdate;
    private FileStatusItem? _currentItem;
    private bool _currentItemStaged;
    private readonly ICommitMessageManager _commitMessageManager;
    private string? _commitTemplate;
    private bool _isMergeCommit;
    private bool _shouldRescanChanges = true;
    private bool _shouldReloadCommitTemplates = true;
    private bool _bypassActivatedEventHandler;
    private bool _loadUnstagedOutputFirstTime = true;
    private bool _initialized;
    private IReadOnlyList<GitItemStatus>? _currentSelection;
    private int _alreadyLoadedTemplatesCount = -1;
    private EventHandler? _branchNameLabelOnClick;
    private ToolStripMenuItem _conventionalCommitItem;

    private CommitKind CommitKind
    {
        get => _commitKind;
        set
        {
            _commitKind = value;

            modifyCommitMessageButton.Visible = _useFormCommitMessage && CommitKind is not (CommitKind.Normal or CommitKind.Amend);
            modifyCommitMessageButton.ForeColor = Application.IsDarkModeEnabled ? SystemColors.ControlText : SystemColors.HotTrack;

            bool messageCanBeChanged = _useFormCommitMessage && CommitKind is (CommitKind.Normal or CommitKind.Amend);
            Message.Enabled = messageCanBeChanged;
            commitMessageToolStripMenuItem.Enabled = messageCanBeChanged;
            commitTemplatesToolStripMenuItem.Enabled = messageCanBeChanged;
            Message.EvaluateForecolor();
        }
    }

    /// <summary>
    /// Flag whether the push needs to be forced, i.e. after amending a commit or after soft reset to the previous commit.
    /// </summary>
    /// The Amend checkbox is disabled after soft reset.
    private bool PushForced => (Amend.Checked || !Amend.Enabled) && AppSettings.CommitAndPushForcedWhenAmend;

    public FormCommit(IGitUICommands commands, CommitKind commitKind = CommitKind.Normal, GitRevision? editedCommit = null, string? commitMessage = null)
        : base(commands)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        _editedCommit = editedCommit;

        InitializeComponent();

        _currentFilesList = Unstaged;

        Unstaged.BindContextMenu(RescanChanges, canAutoRefresh: true, toolStageItem.PerformClick, unstage: null);
        Staged.BindContextMenu(RescanChanges, canAutoRefresh: false, stage: null, toolUnstageItem.PerformClick);

        CommitAndPush.Text = _commitAndPush.Text;

        splitRight.Panel2MinSize = DpiUtil.Scale(100);

        _commitMessageManager = new CommitMessageManager(this, Module.WorkingDirGitDir, Module.CommitEncoding, commitMessage);

        Message.TextChanged += Message_TextChanged;
        Message.TextAssigned += Message_TextAssigned;
        Message.AddAutoCompleteProvider(new CommitAutoCompleteProvider(() => Module));
        Message.AddAutoCompleteProvider(new CommitMessageMetadataProvider());
        _commitTemplateManager = new CommitTemplateManager(() => Module);

        SolveMergeconflicts.Font = new Font(SolveMergeconflicts.Font, FontStyle.Bold);

        StageInSuperproject.Visible = Module.SuperprojectModule is not null;
        StageInSuperproject.Checked = AppSettings.StageInSuperprojectAfterCommit;
        closeDialogAfterEachCommitToolStripMenuItem.Checked = AppSettings.CloseCommitDialogAfterCommit;
        closeDialogAfterAllFilesCommittedToolStripMenuItem.Checked = AppSettings.CloseCommitDialogAfterLastCommit;
        ShowOnlyMyMessagesToolStripMenuItem.Checked = AppSettings.CommitDialogShowOnlyMyMessages;

        Unstaged.SetNoFilesText(_noUnstagedChanges.Text);
        Unstaged.DisableSubmoduleMenuItemBold = true;
        Unstaged.FilterChanged += Unstaged_FilterChanged;
        Staged.FilterChanged += Staged_FilterChanged;
        Staged.SetNoFilesText(_noStagedChanges.Text);
        Staged.DisableSubmoduleMenuItemBold = true;

        ConfigureMessageBox();

        HotkeysEnabled = true;
        LoadHotkeys(HotkeySettingsName);

        SelectedDiff.AddContextMenuSeparator();
        _addSelectionToCommitMessageToolStripMenuItem = SelectedDiff.AddContextMenuEntry(_addSelectionToCommitMessage.Text, (s, e) => AddSelectionToCommitMessage());
        _addSelectionToCommitMessageToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.AddSelectionToCommitMessage);
        fileTooltip.SetToolTip(modifyCommitMessageButton, _modifyCommitMessageButtonToolTip.Text);
        commitAuthorStatus.ToolTipText = _commitCommitterToolTip.Text;
        toolStageAllItem.ToolTipText = _stageAll.Text;
        toolUnstageAllItem.ToolTipText = _unstageAll.Text;

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

        gpgSignCommitToolStripComboBox.ResizeDropDownWidth(minWidth: 50, maxWidth: 250);

        ((ToolStripDropDownMenu)commitTemplatesToolStripMenuItem.DropDown).ShowImageMargin = true;
        ((ToolStripDropDownMenu)commitTemplatesToolStripMenuItem.DropDown).ShowCheckMargin = false;

        ((ToolStripDropDownMenu)commitMessageToolStripMenuItem.DropDown).ShowImageMargin = false;
        ((ToolStripDropDownMenu)commitMessageToolStripMenuItem.DropDown).ShowCheckMargin = true;

        selectionFilter.Size = DpiUtil.Scale(selectionFilter.Size);
        toolStripStatusBranchIcon.Width = DpiUtil.Scale(toolStripStatusBranchIcon.Width);

        if (!Module.GitVersion.SupportStashStaged)
        {
            flowCommitButtons.Controls.Remove(StashStaged);
        }

        SetVisibilityOfSelectionFilter(AppSettings.CommitDialogSelectionFilter);
        btnResetAllChanges.Visible = AppSettings.ShowResetAllChanges;
        btnResetUnstagedChanges.Visible = AppSettings.ShowResetWorkTreeChanges;
        CommitAndPush.Visible = AppSettings.ShowCommitAndPush;
        splitRight.Panel2MinSize = Math.Max(splitRight.Panel2MinSize, flowCommitButtons.PreferredSize.Height);
        splitRight.SplitterDistance = Math.Min(splitRight.SplitterDistance, splitRight.Height - splitRight.Panel2MinSize);

        SelectedDiff.EscapePressed += () => DialogResult = DialogResult.Cancel;
        SelectedDiff.TopScrollReached += FileViewer_TopScrollReached;
        SelectedDiff.BottomScrollReached += FileViewer_BottomScrollReached;
        SelectedDiff.LinePatchingBlocksUntilReload = true;

        SolveMergeconflicts.BackColor = OtherColors.MergeConflictsColor;
        SolveMergeconflicts.SetForeColorForBackColor();

        if (AppSettings.DontConfirmAmend)
        {
            ResetSoft.BackColor = OtherColors.AmendButtonForcedColor;
            ResetSoft.SetForeColorForBackColor();
        }

        toolStripStatusBranchIcon.AdaptImageLightness();

        splitLeft.Panel1.BackColor = OtherColors.PanelBorderColor;
        splitLeft.Panel2.BackColor = OtherColors.PanelBorderColor;
        splitRight.Panel1.BackColor = OtherColors.PanelBorderColor;
        splitRight.Panel2.BackColor = OtherColors.PanelBorderColor;

        BackColor = OtherColors.BackgroundColor;

        WorkaroundPaddingIncreaseBug();

        InitializeComplete();

        // By calling this in the constructor, we prevent flickering caused by resizing the
        // form, for example when it is restored to maximised, but first drawn as a smaller window.
        RestorePosition();

        // TODO this code is very similar to code in FileStatusList
        _selectionFilterSubject
            .Throttle(TimeSpan.FromMilliseconds(250))
            .ObserveOn(SynchronizationContext.Current)
            .Subscribe(filterText => TaskManager.HandleExceptions(() => Update(filterText), Application.OnThreadException));

        UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;

        return;

        void Update(string filterText)
        {
            ThreadHelper.AssertOnUIThread();

            int matchCount = 0;
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
                AddToSelectionFilter(filterText);
            }
        }

        void AddToSelectionFilter(string filterText)
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

        void ConfigureMessageBox()
        {
            CommitKind = commitKind;

            Message.WatermarkText = _useFormCommitMessage
                ? _enterCommitMessageHint.Text
                : _commitMessageDisabled.Text;
        }

        void WorkaroundPaddingIncreaseBug()
        {
            Padding padding = new(1);

            splitLeft.Panel1.Padding = padding;
            splitLeft.Panel2.Padding = padding;
            splitRight.Panel1.Padding = padding;
            splitRight.Panel2.Padding = padding;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!IsDesignMode && !IsUnitTestActive)
            {
                UICommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
            }

            _unstagedLoader.Dispose();
            _customDiffToolsSequence.Dispose();
            _interactiveAddSequence.Dispose();
            _viewChangesSequence.Dispose();
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
        if (!_bypassActivatedEventHandler && AppSettings.RefreshArtificialCommitOnApplicationActivated)
        {
            RescanChanges();
        }

        base.OnApplicationActivated();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        // Do not attempt to store again if the form has already been closed. Unfortunately, OnFormClosed is always called by Close.
        if (Visible)
        {
            _splitterManager.SaveSplitters();

            // Do not remember commit message of fixup or squash commits, since they have
            // a special meaning, and can be dangerous if used inappropriately.
            if (CommitKind is (CommitKind.Normal or CommitKind.Amend))
            {
                // Run async as we're closing the form
                string message = Message.Text;
                bool isAmend = Amend.Checked;
                ThreadHelper.FileAndForget(async () =>
                {
                    await _commitMessageManager.SetMergeOrCommitMessageAsync(message);
                    await _commitMessageManager.SetAmendStateAsync(isAmend);
                });
            }
        }

        base.OnFormClosed(e);
    }

    protected override void OnLoad(EventArgs e)
    {
        MinimizeBox = Owner is null;

        base.OnLoad(e);
    }

    private void RestoreSplitters()
    {
        _splitterManager.AddSplitter(splitMain, nameof(splitMain));
        _splitterManager.AddSplitter(splitRight, nameof(splitRight));
        _splitterManager.AddSplitter(splitLeft, nameof(splitLeft));
        _splitterManager.RestoreSplitters();
    }

    protected override void OnShown(EventArgs e)
    {
        if (!_initialized)
        {
            Initialize();
        }

        UpdateAuthorInfo();

        string message;

        switch (CommitKind)
        {
            case CommitKind.Fixup:
                Validates.NotNull(_editedCommit);
                message = TryAddPrefix(_editedCommit.Subject);
                break;
            case CommitKind.Squash:
                Validates.NotNull(_editedCommit);
                message = TryAddPrefix(_editedCommit.Subject);
                break;
            case CommitKind.Amend:
                Validates.NotNull(_editedCommit);
                message = $"{TryAddPrefix(_editedCommit.Subject)}{Environment.NewLine}{Environment.NewLine}{_editedCommit.Body}";
                break;
            default:
                (string retrievedMessage, bool retrievedAmendState) = ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    string m = await _commitMessageManager.GetMergeOrCommitMessageAsync();
                    bool a = await _commitMessageManager.GetAmendStateAsync();
                    return (m, a);
                });
                message = retrievedMessage;
                Amend.Checked = !_commitMessageManager.IsMergeCommit && retrievedAmendState;
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

        string TryAddPrefix(string suffix)
        {
            string prefix = CommitKind.GetPrefix();

            return suffix.StartsWith(prefix) ? suffix : $"{prefix} {suffix}";
        }

        void AssignCommitMessageFromTemplate()
        {
            string text = "";
            try
            {
                text = _commitTemplateManager.LoadGitCommitTemplate() ?? "";
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
        }

        base.OnKeyUp(e);

        return;
    }

    protected override void OnUICommandsChanged(GitUICommandsChangedEventArgs e)
    {
        IGitUICommands oldCommands = e.OldCommands;

        if (oldCommands is not null)
        {
            oldCommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
        }

        UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;

        base.OnUICommandsChanged(e);
    }

    public override bool ProcessHotkey(Keys keyData)
    {
        if (IsDesignMode || !HotkeysEnabled)
        {
            return false;
        }

        // generic handling of this form's hotkeys (upstream)
        if (base.ProcessHotkey(keyData))
        {
            return true;
        }

        // downstream (without keys for quick search and without keys for text selection and copy e.g. in commit message)
        if (GitExtensionsControl.IsTextEditKey(keyData, multiLine: true))
        {
            return false;
        }

        // route to visible controls which have their own hotkeys
        return _currentFilesList.ProcessHotkey(keyData)
            || SelectedDiff.ProcessHotkey(keyData);
    }

    private void FileViewer_TopScrollReached(object sender, EventArgs e)
    {
        FileStatusList fileStatus = _currentItemStaged ? Staged : Unstaged;
        fileStatus.SelectPreviousVisibleItem();
        SelectedDiff.ScrollToBottom();
    }

    private void FileViewer_BottomScrollReached(object sender, EventArgs e)
    {
        FileStatusList fileStatus = _currentItemStaged ? Staged : Unstaged;
        fileStatus.SelectNextVisibleItem();
        SelectedDiff.ScrollToTop();
    }

    private void MoveSelection(bool backwards)
    {
        if (Message.Focused)
        {
            _currentFilesList = Staged;
        }

        _currentFilesList.SelectNextItem(backwards, loop: true);
    }

    #region Hotkey commands

    public static readonly string HotkeySettingsName = "Commit";

    internal enum Command
    {
        /* obsolete: AddToGitIgnore = 0, */
        /* obsolete: DeleteSelectedFiles = 1, */
        FocusUnstagedFiles = 2,
        FocusSelectedDiff = 3,
        FocusStagedFiles = 4,
        FocusCommitMessage = 5,
        /* obsolete: ResetSelectedFiles = 6, */
        /* obsolete: StageSelectedFile = 7, */
        /* obsolete: UnStageSelectedFile = 8, */
        /* obsolete: ShowHistory = 9, */
        ToggleSelectionFilter = 10,
        StageAll = 11,
        OpenWithDifftool = 12,
        /* obsolete: OpenFile = 13, */
        /* obsolete: OpenFileWith = 14, */
        /* obsolete: EditFile = 15, */
        AddSelectionToCommitMessage = 16,
        CreateBranch = 17,
        Refresh = 18,
        SelectNext = 19, // Ctrl+N
        SelectNext_AlternativeHotkey1 = 20, // Alt+Down
        SelectNext_AlternativeHotkey2 = 21, // Alt+Right
        SelectPrevious = 22, // Ctrl+P
        SelectPrevious_AlternativeHotkey1 = 23, // Alt+Up
        SelectPrevious_AlternativeHotkey2 = 24, // Alt+Left
        ConventionalCommit_PrefixMessage = 25, // Ctrl+T
        ConventionalCommit_PrefixMessageWithScope = 26, // Ctrl+Shift+T
    }

    private bool AddSelectionToCommitMessage()
    {
        if (!SelectedDiff.ContainsFocus)
        {
            return false;
        }

        string selectedText = SelectedDiff.GetSelectedText();
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

    private bool StageAllFiles()
    {
        if (Unstaged.IsEmpty)
        {
            return false;
        }

        StageAllAccordingToFilter();
        return true;
    }

    private bool ToggleSelectionFilter()
    {
        bool visible = !toolbarSelectionFilter.Visible;
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

    private void ShowOnlyMyMessagesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
    {
        AppSettings.CommitDialogShowOnlyMyMessages = ShowOnlyMyMessagesToolStripMenuItem.Checked;
    }

    protected override bool ExecuteCommand(int cmd)
    {
        switch ((Command)cmd)
        {
            case Command.ConventionalCommit_PrefixMessage: OpenConventionalCommitMenu(insertScope: false); return true;
            case Command.ConventionalCommit_PrefixMessageWithScope: OpenConventionalCommitMenu(insertScope: true); return true;
            case Command.FocusStagedFiles: return FocusStagedFiles();
            case Command.FocusUnstagedFiles: return FocusUnstagedFiles();
            case Command.FocusSelectedDiff: return FocusSelectedDiff();
            case Command.FocusCommitMessage: return FocusCommitMessage();
            case Command.ToggleSelectionFilter: return ToggleSelectionFilter();
            case Command.StageAll: return StageAllFiles();
            case Command.OpenWithDifftool: OpenWithDiffTool(); return true;
            case Command.AddSelectionToCommitMessage: return AddSelectionToCommitMessage();
            case Command.CreateBranch: createBranchToolStripButton.PerformClick(); return true;
            case Command.Refresh: RescanChanges(); return true;
            case Command.SelectNext:
            case Command.SelectNext_AlternativeHotkey1:
            case Command.SelectNext_AlternativeHotkey2: MoveSelection(backwards: false); return true;
            case Command.SelectPrevious:
            case Command.SelectPrevious_AlternativeHotkey1:
            case Command.SelectPrevious_AlternativeHotkey2: MoveSelection(backwards: true); return true;
            default: return base.ExecuteCommand(cmd);
        }
    }

    public override IScriptOptionsProvider? GetScriptOptionsProvider()
    {
        return new ScriptOptionsProvider(_currentFilesList, () => SelectedDiff.CurrentFileLine);
    }

    #endregion

    private void ComputeUnstagedFiles(Action<IReadOnlyList<GitItemStatus>> onComputed, bool doAsync)
    {
        IReadOnlyList<GitItemStatus> GetAllChangedFilesWithSubmodulesStatus()
        {
            return Module.GetAllChangedFilesWithSubmodulesStatus(
                !Unstaged.tsmiShowIgnoredFiles.Checked,
                !Unstaged.tsmiShowAssumeUnchangedFiles.Checked,
                !Unstaged.tsmiShowSkipWorktreeFiles.Checked,
                Unstaged.tsmiShowUntrackedFiles.Checked ? UntrackedFilesMode.Default : UntrackedFilesMode.No,
                cancellationToken: default);
        }

        if (doAsync)
        {
            ThreadHelper.FileAndForget(() => _unstagedLoader.LoadAsync(GetAllChangedFilesWithSubmodulesStatus, onComputed));
        }
        else
        {
            _unstagedLoader.Cancel();
            onComputed(GetAllChangedFilesWithSubmodulesStatus());
        }
    }

    public void ShowDialogWhenChanges(IWin32Window? owner = null)
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

    private void EnableStageButtons(bool enable)
    {
        toolUnstageItem.Enabled = enable;
        toolUnstageAllItem.Enabled = enable;
        toolStageItem.Enabled = enable;
        toolStageAllItem.Enabled = enable;
        btnResetUnstagedChanges.Enabled = Unstaged.AllItems.Any();
    }

    private async Task UpdateBranchNameDisplayAsync()
    {
        string currentBranchName = Module.GetSelectedBranch();
        if (_branchNameLabelOnClick is not null)
        {
            remoteNameLabel.Click -= _branchNameLabelOnClick;
        }

        IGitRef currentBranch = Module.GetRefs(RefsFilter.Heads).FirstOrDefault(r => r.LocalName == currentBranchName);
        if (currentBranch is null)
        {
            await this.SwitchToMainThreadAsync();

            branchNameLabel.Text = currentBranchName;
            remoteNameLabel.Text = string.Empty;
            return;
        }

        string pushTo;
        if (string.IsNullOrEmpty(currentBranch.TrackingRemote) || string.IsNullOrEmpty(currentBranch.MergeWith))
        {
            string defaultRemote = Module.GetRemoteNames().FirstOrDefault(r => r == "origin") ?? Module.GetRemoteNames().OrderBy(r => r).FirstOrDefault();

            pushTo = defaultRemote is not null
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

        _branchNameLabelOnClick = (object sender, EventArgs e) => this.InvokeAndForget(async () =>
        {
            UICommands.StartRemotesDialog(this, null, currentBranchName);
            await TaskScheduler.Default;
            await UpdateBranchNameDisplayAsync();
        });
        remoteNameLabel.Click += _branchNameLabelOnClick;
        Text = string.Format(_formTitle.Text, currentBranchName, PathUtil.GetDisplayPath(Module.WorkingDir));
    }

    private void Initialize(bool loadUnstaged = true)
    {
        _initialized = true;

        ThreadHelper.FileAndForget(UpdateBranchNameDisplayAsync);

        using (WaitCursorScope.Enter())
        {
            if (loadUnstaged)
            {
                Loading.Visible = true;
                Loading.IsAnimating = true;
                LoadingStaged.Visible = true;

                Commit.Enabled = false;
                CommitAndPush.Enabled = false;
                btnResetAllChanges.Enabled = false;
                btnResetUnstagedChanges.Enabled = false;
                EnableStageButtons(false);

                ComputeUnstagedFiles(LoadUnstagedOutput, true);
            }

            UpdateMergeHead();

            Message.TextBoxFont = AppSettings.CommitFont;
        }

        void UpdateMergeHead()
        {
            _isMergeCommit = Module.RevParse("MERGE_HEAD") is not null;
        }
    }

    private void InitializedStaged()
    {
        using (WaitCursorScope.Enter())
        {
            SolveMergeconflicts.Visible = Module.InTheMiddleOfConflictedMerge();
            (GitRevision headRev, GitRevision indexRev, GitRevision _) = GetHeadRevisions();
            Staged.SetDiffs(headRev, indexRev, Module.GetIndexFilesWithSubmodulesStatus());
        }

        UpdateButtonStates();
    }

    /// <summary>
    ///   Loads the unstaged output.
    ///   This method is passed in to the SetTextCallBack delegate
    ///   to set the Text property of textBox1.
    /// </summary>
    private void LoadUnstagedOutput(IReadOnlyList<GitItemStatus> allChangedFiles)
    {
        IReadOnlyList<GitItemStatus> lastSelection = _currentSelection ?? Array.Empty<GitItemStatus>();

        List<GitItemStatus> unstagedFiles = [];
        List<GitItemStatus> stagedFiles = [];

        foreach (GitItemStatus fileStatus in allChangedFiles)
        {
            if (fileStatus.Staged == StagedStatus.WorkTree || fileStatus.IsStatusOnly)
            {
                // Present status only errors in unstaged
                unstagedFiles.Add(fileStatus);
            }
            else if (fileStatus.Staged == StagedStatus.Index)
            {
                stagedFiles.Add(fileStatus);
            }
        }

        (GitRevision headRev, GitRevision indexRev, GitRevision workTreeRev) = GetHeadRevisions();
        Unstaged.SetDiffs(indexRev, workTreeRev, unstagedFiles);
        Staged.SetDiffs(headRev, indexRev, stagedFiles);

        Loading.Visible = false;
        Loading.IsAnimating = false;
        LoadingStaged.Visible = false;
        Commit.Enabled = true;
        CommitAndPush.Enabled = true;
        UpdateButtonStates();

        EnableStageButtons(true);

        bool inTheMiddleOfConflictedMerge = Module.InTheMiddleOfConflictedMerge();
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
            Control fc = this.FindFocusedControl();

            if (fc is null || fc == Ok)
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

    private void RestoreSelectedFiles(IReadOnlyList<GitItemStatus> unstagedFiles, IReadOnlyList<GitItemStatus> stagedFiles, IReadOnlyList<GitItemStatus>? lastSelection)
    {
        if (_currentFilesList.IsEmpty)
        {
            SelectStoredNextIndex();
            return;
        }

        Validates.NotNull(lastSelection);
        IReadOnlyList<GitItemStatus> newItems = _currentFilesList == Staged ? stagedFiles : unstagedFiles;
        HashSet<string> names = lastSelection.Select(x => x.Name).ToHashSet();
        List<GitItemStatus> newSelection = newItems.Where(x => names.Contains(x.Name)).ToList();

        if (newSelection.Any())
        {
            _currentFilesList.SelectedGitItems = newSelection;
        }
        else
        {
            SelectStoredNextIndex();
        }

        return;

        void SelectStoredNextIndex()
        {
            Unstaged.SelectStoredNextItem(orSelectFirst: true);
            if (Unstaged.GitItemStatuses.Any())
            {
                Staged.SelectStoredNextItem();
            }
            else
            {
                Staged.SelectStoredNextItem(orSelectFirst: true);
            }
        }
    }

    private void ShowChanges(FileStatusItem? item, bool staged)
    {
        _currentItem = item;
        _currentItemStaged = staged;

        if (item?.Item is null)
        {
            SelectedDiff.Clear();
            return;
        }

        SelectedDiff.InvokeAndForget(() => SelectedDiff.ViewChangesAsync(item, openWithDiffTool: OpenWithDiffTool, cancellationToken: _viewChangesSequence.Next()));
    }

    private void CommitClick(object sender, EventArgs e)
    {
        ExecuteCommitCommand();
    }

    private void CheckForStagedAndCommit(bool push)
    {
        bool createAmendCommit = Amend.Checked;
        bool resetAuthor = Amend.Checked && ResetAuthor.Checked;
        bool pushForced = PushForced;

        BypassFormActivatedEventHandler(() =>
        {
            if (ConfirmOrStageCommit())
            {
                DoCommit();
            }
        });

        bool ConfirmOrStageCommit()
        {
            if (createAmendCommit)
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
                    if (MessageBox.Show(this, _amendCommit.Text, _amendCommitCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
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
                bool mustStageAll = false;
                TaskDialogPage page = new()
                {
                    AllowCancel = true,
                    Caption = _noFilesStagedCommitCaption.Text,
                    Icon = TaskDialogIcon.Error,
                    Heading = _noFilesStagedCommitInstructions.Text,
                    Buttons = { TaskDialogButton.Cancel },
                    SizeToContent = true
                };

                // Option 1: there are no staged files, but there are unstaged files. Most probably user forgot to stage them.
                string text = Unstaged.IsFilterActive ? _noFilesStagedCommitAllFilteredUnstagedOption.Text : _noFilesStagedCommitAllUnstagedOption.Text;
                TaskDialogCommandLinkButton lnkStageAndCommit = new(text, enabled: !Unstaged.IsEmpty);
                lnkStageAndCommit.Click += (s, e) =>
                {
                    mustStageAll = true;
                };
                page.Buttons.Add(lnkStageAndCommit);

                // Option 2: the user just wants to make an empty commit
                TaskDialogCommandLinkButton lnkEmptyCommit = new(_noFilesStagedMakeEmptyCommitOption.Text);
                page.Buttons.Add(lnkEmptyCommit);

                TaskDialogButton result = TaskDialog.ShowDialog(Handle, page);
                if (result == TaskDialogButton.Cancel)
                {
                    return false;
                }

                if (mustStageAll)
                {
                    StageAllAccordingToFilter();

                    if (Staged.IsEmpty)
                    {
                        // if staging failed (i.e. line endings conflict), user already got error message, don't try to commit empty changeset.
                        return false;
                    }
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
                TaskDialogPage page = new()
                {
                    Text = _notOnBranch.Text,
                    Heading = TranslatedStrings.ErrorInstructionNotOnBranch,
                    Caption = TranslatedStrings.ErrorCaptionNotOnBranch,
                    Buttons = { TaskDialogButton.Cancel },
                    Icon = TaskDialogIcon.Error,
                    AllowCancel = true,
                    SizeToContent = true
                };
                TaskDialogCommandLinkButton btnCheckout = new(TranslatedStrings.ButtonCheckoutBranch);
                TaskDialogCommandLinkButton btnCreate = new(TranslatedStrings.ButtonCreateBranch);
                TaskDialogCommandLinkButton btnContinue = new(TranslatedStrings.ButtonContinue);
                page.Buttons.Add(btnCheckout);
                page.Buttons.Add(btnCreate);
                page.Buttons.Add(btnContinue);

                TaskDialogButton result = TaskDialog.ShowDialog(Handle, page);
                if (result == TaskDialogButton.Cancel)
                {
                    return;
                }

                if (result == btnCheckout)
                {
                    ObjectId[] revisions = _editedCommit is not null ? new[] { _editedCommit.ObjectId } : null;
                    if (!UICommands.StartCheckoutBranch(this, revisions))
                    {
                        return;
                    }
                }
                else if (result == btnCreate)
                {
                    if (!UICommands.StartCreateBranchDialog(this, _editedCommit?.ObjectId))
                    {
                        return;
                    }
                }
            }

            try
            {
                if (_useFormCommitMessage)
                {
                    // Save last commit message in settings. This way it can be used in multiple repositories.
                    AppSettings.LastCommitMessage = Message.Text;
                    ThreadHelper.JoinableTaskFactory.Run(
                        () => _commitMessageManager.WriteCommitMessageToFileAsync(Message.Text, CommitMessageType.Normal,
                                                                                  usingCommitTemplate: !string.IsNullOrEmpty(_commitTemplate),
                                                                                  ensureCommitMessageSecondLineEmpty: AppSettings.EnsureCommitMessageSecondLineEmpty));
                }

                bool success = ScriptsRunner.RunEventScripts(ScriptEvent.BeforeCommit, this);

                if (!success)
                {
                    return;
                }

                ArgumentString commitCmd = Commands.Commit(
                    createAmendCommit,
                    signOffToolStripMenuItem.Checked,
                    toolAuthor.Text,
                    _useFormCommitMessage,
                    _commitMessageManager.CommitMessagePath,
                    Module.GetPathForGitExecution,
                    noVerifyToolStripMenuItem.Checked,
                    gpgSignCommitToolStripComboBox.SelectedIndex == 0 ? null : gpgSignCommitToolStripComboBox.SelectedIndex > 1,
                    toolStripGpgKeyTextBox.Text,
                    Staged.IsEmpty,
                    resetAuthor);

                success = FormProcess.ShowDialog(this, UICommands, arguments: commitCmd, Module.WorkingDir, input: null, useDialogSettings: true);

                UICommands.RepoChangedNotifier.Notify();

                if (!success)
                {
                    return;
                }

                ScriptsRunner.RunEventScripts(ScriptEvent.AfterCommit, this);

                // Message.Text has been used and stored
                ThreadHelper.JoinableTaskFactory.Run(_commitMessageManager.ResetCommitMessageAsync);

                CommitKind = CommitKind.Normal;
                bool pushCompleted = true;
                try
                {
                    if (push)
                    {
                        UICommands.StartPushDialog(owner: this, pushOnShow: true, forceWithLease: pushForced, out pushCompleted);
                    }
                }
                finally
                {
                    Message.Text = string.Empty;
                    Amend.Enabled = true;
                    Amend.Checked = false;
                    noVerifyToolStripMenuItem.Checked = false;
                }

                if (pushCompleted && Module.SuperprojectModule is not null &&
                    AppSettings.StageInSuperprojectAfterCommit &&
                    !string.IsNullOrWhiteSpace(Module.SubmodulePath))
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
                MessageBox.Show(this, $"Exception: {e.Message}", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return;

            bool IsCommitMessageValid()
            {
                if (AppSettings.CommitValidationMaxCntCharsFirstLine > 0)
                {
                    string firstLine = Message.Text.Split(Delimiters.NewLines, StringSplitOptions.RemoveEmptyEntries)[0];
                    if (firstLine.Length > AppSettings.CommitValidationMaxCntCharsFirstLine &&
                        MessageBox.Show(this, _commitMsgFirstLineInvalid.Text, _commitValidationCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.No)
                    {
                        return false;
                    }
                }

                if (AppSettings.CommitValidationMaxCntCharsPerLine > 0)
                {
                    string[] lines = Message.Text.Split(Delimiters.NewLines, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
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
                    string[] lines = Message.Text.Split(Delimiters.NewLines, StringSplitOptions.None);
                    if (lines.Length > 2 &&
                        lines[1].Length != 0 &&
                        MessageBox.Show(this, _commitMsgSecondLineNotEmpty.Text, _commitValidationCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.No)
                    {
                        return false;
                    }
                }

                if (!string.IsNullOrEmpty(AppSettings.CommitValidationRegEx))
                {
                    try
                    {
                        if (!Message.Text.StartsWith(CommitKind.Fixup.GetPrefix()) &&
                            !Message.Text.StartsWith(CommitKind.Squash.GetPrefix()) &&
                            !Regex.IsMatch(GetTextToValidate(Message.Text), AppSettings.CommitValidationRegEx) &&
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

            static string GetTextToValidate(string text)
            {
                string[] lines = text.Split(Delimiters.NewLines, StringSplitOptions.None);
                if (text.StartsWith(CommitKind.Amend.GetPrefix()) && lines.Length > 2 && lines[1].Length == 0)
                {
                    return string.Join(Environment.NewLine, lines.Skip(2));
                }

                return text;
            }
        }
    }

    /// <summary>
    /// replace the Message.Text in an undo-able way.
    /// </summary>
    /// <param name="message">the new message.</param>
    private void ReplaceMessage(string? message)
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
            Initialize();
            Message.RefreshAutoCompleteWords();
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

    private void toolUnstageAllItem_Click(object sender, EventArgs e)
    {
        UnstageAllFiles();
    }

    private void UnstageAllFiles()
    {
        IReadOnlyList<GitItemStatus> lastSelection = _currentSelection ?? Array.Empty<GitItemStatus>();

        OnStageAreaLoaded += StageAreaLoaded;

        if (_isMergeCommit)
        {
            Staged.SelectAll();
            Unstage(canUseUnstageAll: false);
        }
        else if (Staged.IsFilterActive)
        {
            Staged.SelectedGitItems = Staged.AllItems.Items().ToArray();
            Unstage(canUseUnstageAll: false);
            Staged.SetFilter(string.Empty);
        }
        else
        {
            Module.Reset(ResetMode.Mixed);
        }

        Initialize();
        return;

        void StageAreaLoaded()
        {
            _currentFilesList = Unstaged;
            RestoreSelectedFiles(Unstaged.GitItemStatuses, Staged.GitItemStatuses, lastSelection);
            Unstaged.Focus();

            OnStageAreaLoaded -= StageAreaLoaded;
        }
    }

    private void UnstagedSelectionChanged(object sender, EventArgs e)
    {
        if (_currentFilesList != Unstaged || _skipUpdate)
        {
            return;
        }

        Staged.ClearSelected();

        _currentSelection = Unstaged.SelectedItems.Items().ToList();
        FileStatusItem? item = Unstaged.SelectedItem;
        ShowChanges(item, staged: false);
    }

    private void Unstaged_Enter(object sender, EnterEventArgs e)
    {
        _currentFilesList = Unstaged;
        _skipUpdate = false;
        if (!Unstaged.HasSelection)
        {
            if (Unstaged.FocusedItem is null)
            {
                Unstaged.SelectFirstVisibleItem();
                if (!Unstaged.HasSelection)
                {
                    UnstagedSelectionChanged(Unstaged, EventArgs.Empty);
                }
            }
            else
            {
                Unstaged.SelectedItems = [Unstaged.FocusedItem];
            }
        }
        else
        {
            UnstagedSelectionChanged(Unstaged, EventArgs.Empty);
        }
    }

    private void Unstaged_FilterChanged(object sender, EventArgs e)
    {
        if (Unstaged.IsFilterActive)
        {
            toolStageAllItem.ToolTipText = _stageFiltered.Text;
            toolStageAllItem.Image = Images.StageAllFiltered;
        }
        else
        {
            toolStageAllItem.ToolTipText = _stageAll.Text;
            toolStageAllItem.Image = Images.StageAll;
        }
    }

    private void Staged_FilterChanged(object sender, EventArgs e)
    {
        if (Staged.IsFilterActive)
        {
            toolUnstageAllItem.ToolTipText = _unstageFiltered.Text;
            toolUnstageAllItem.Image = Images.UnstageAllFiltered;
        }
        else
        {
            toolUnstageAllItem.ToolTipText = _unstageAll.Text;
            toolUnstageAllItem.Image = Images.UnstageAll;
        }
    }

    private void Unstage(bool canUseUnstageAll = true)
    {
        if (Module.IsBareRepository())
        {
            return;
        }

        // Staged.SelectedItems.Items() is needed only once, so we can safely convert to list here
        List<GitItemStatus> allFiles = Staged.SelectedItems.Items().ToList();
        if (allFiles.Count == 0)
        {
            return;
        }

        // Getting Staged.GitItemStatuses will cause multiple enumerations, but this is intended for side-effects
        // because we need to refresh git item statuses later.
        // We can optimize a little bit here by querying only once for staged count
        int initialStagedCount = Staged.GitItemStatuses.Count;
        if (canUseUnstageAll && initialStagedCount > 10 && allFiles.Count == initialStagedCount)
        {
            UnstageAllFiles();
            return;
        }

        using (WaitCursorScope.Enter())
        {
            EnableStageButtons(false);
            try
            {
                toolStripProgressBar1.Visible = true;
                toolStripProgressBar1.Value = 0;

                // Before batch execution is introduced, rename file counts as 1
                // When executing in batch execution, rename must be interpreted as 2 different file arguments in command line arguments
                // because Windows max command line have 32767 characters limitations, we can not separate the two.
                // So the only solution for exact progress bar update would be changing Maximum value to include renamed file count as 2 files
                toolStripProgressBar1.Maximum = allFiles.Aggregate(0, (count, item) =>
                {
                    return item.IsRenamed ? count + 2 : count + 1;
                });

                Staged.StoreNextItemToSelect();
                bool shouldRescanChanges = Module.BatchUnstageFiles(allFiles, (eventArgs) =>
                {
                    toolStripProgressBar1.Value = Math.Min(toolStripProgressBar1.Maximum - 1, toolStripProgressBar1.Value + eventArgs.ProcessedCount);
                });

                _skipUpdate = true;
                InitializedStaged();
                List<GitItemStatus> stagedFiles = Staged.GitItemStatuses.ToList();
                List<GitItemStatus> unstagedFiles = Unstaged.GitItemStatuses.ToList();
                foreach (GitItemStatus item in allFiles)
                {
                    GitItemStatus item1 = item;
                    if (stagedFiles.Exists(i => i.Name == item1.Name))
                    {
                        continue;
                    }

                    item.IsTracked = !item.IsNew || item.IsChanged || item.IsDeleted;
                    int index = unstagedFiles.FindIndex(i => i.Name == item.Name);

                    if (index >= 0)
                    {
                        unstagedFiles[index].IsNew = item.IsNew;
                        unstagedFiles[index].IsDeleted = item.IsDeleted;
                        unstagedFiles[index].IsTracked = item.IsTracked;
                        unstagedFiles[index].IsChanged = item.IsChanged;
                        continue;
                    }

                    if (item.IsRenamed)
                    {
                        Validates.NotNull(item.OldName);

                        GitItemStatus clone = new(item.OldName)
                        {
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

                (GitRevision headRev, GitRevision indexRev, GitRevision workTreeRev) = GetHeadRevisions();
                Unstaged.SetDiffs(indexRev, workTreeRev, unstagedFiles);
                Staged.SetDiffs(headRev, indexRev, stagedFiles);
                _skipUpdate = false;
                Staged.SelectStoredNextItem();

                toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;

                toolStripProgressBar1.Visible = false;

                if (Staged.IsEmpty)
                {
                    IReadOnlyList<GitItemStatus> lastSelection = _currentSelection ?? Array.Empty<GitItemStatus>();

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

        if (AppSettings.RevisionGraphShowArtificialCommits)
        {
            UICommands.RepoChangedNotifier.Notify();
        }
    }

    private (GitRevision? headRev, GitRevision indexRev, GitRevision workTreeRev) GetHeadRevisions()
    {
        GitRevision? headRev;
        GitRevision indexRev;
        ObjectId headId = Module.RevParse("HEAD");
        if (headId is not null)
        {
            headRev = new GitRevision(headId);
            indexRev = new GitRevision(ObjectId.IndexId) { ParentIds = new[] { headId } };
        }
        else
        {
            headRev = null;
            indexRev = new GitRevision(ObjectId.IndexId);
        }

        GitRevision workTreeRev = new(ObjectId.WorkTreeId) { ParentIds = new[] { ObjectId.IndexId } };
        return (headRev, indexRev, workTreeRev);
    }

    private void StageClick(object sender, EventArgs e)
    {
        if (_currentFilesList != Unstaged || Module.IsBareRepository())
        {
            return;
        }

        Stage(Unstaged.SelectedItems.Items().Where(s => !s.IsAssumeUnchanged && !s.IsSkipWorktree).ToList());
        if (Unstaged.IsEmpty)
        {
            Message.Focus();
        }
    }

    private void Unstaged_DoubleClick(object sender, EventArgs e)
    {
        _currentFilesList = Unstaged;
        Stage(Unstaged.SelectedItems.Items().ToList());
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

    private void toolStageAllItem_Click(object sender, EventArgs e)
    {
        StageAllAccordingToFilter();
    }

    private void StagedSelectionChanged(object sender, EventArgs e)
    {
        if (_currentFilesList != Staged || _skipUpdate)
        {
            return;
        }

        Unstaged.ClearSelected();

        _currentSelection = Staged.SelectedItems.Items().ToList();
        FileStatusItem? item = Staged.SelectedItem;
        ShowChanges(item, staged: true);
    }

    private void Staged_DataSourceChanged(object sender, EventArgs e)
    {
        int stagedCount = Staged.UnfilteredItemsCount;
        int totalFilesCount = stagedCount + Unstaged.UnfilteredItemsCount;
        commitStagedCount.Text = stagedCount + "/" + totalFilesCount;
    }

    private void Staged_Enter(object sender, EnterEventArgs e)
    {
        SelectStaged();
    }

    private void SelectStaged()
    {
        _currentFilesList = Staged;
        _skipUpdate = false;
        if (!Staged.HasSelection)
        {
            if (Staged.FocusedItem is null)
            {
                Staged.SelectFirstVisibleItem();
                if (!Staged.HasSelection)
                {
                    StagedSelectionChanged(Staged, EventArgs.Empty);
                }
            }
            else
            {
                Staged.SelectedItems = [Staged.FocusedItem];
            }
        }
        else
        {
            StagedSelectionChanged(Staged, EventArgs.Empty);
        }
    }

    private void Stage(IReadOnlyList<GitItemStatus> items)
    {
        using (WaitCursorScope.Enter())
        {
            EnableStageButtons(false);
            try
            {
                IReadOnlyList<GitItemStatus> lastSelection = _currentSelection ?? Array.Empty<GitItemStatus>();

                Unstaged.StoreNextItemToSelect();
                toolStripProgressBar1.Visible = true;
                toolStripProgressBar1.Maximum = items.Count * 2;
                toolStripProgressBar1.Value = 0;

                List<GitItemStatus> files = [];

                foreach (GitItemStatus item in items)
                {
                    toolStripProgressBar1.Value = Math.Min(toolStripProgressBar1.Maximum - 1, toolStripProgressBar1.Value + 1);
                    files.Add(item);
                }

                bool wereErrors = !Module.StageFiles(files, out string output);
                if (wereErrors && AppSettings.ShowErrorsWhenStagingFiles)
                {
                    FormStatus.ShowErrorDialog(this, UICommands, _stageDetails.Text, string.Format(_stageFiles.Text + "\n", files.Count), output);
                }

                if (wereErrors)
                {
                    RescanChanges();
                }
                else
                {
                    InitializedStaged();
                    List<GitItemStatus> unstagedFiles = Unstaged.GitItemStatuses.ToList();
                    _skipUpdate = true;
                    HashSet<string?> names = [];
                    foreach (GitItemStatus item in files)
                    {
                        names.Add(item.Name);
                        names.Add(item.OldName);
                    }

                    HashSet<GitItemStatus> unstagedItems = [];

                    foreach (GitItemStatus item in unstagedFiles)
                    {
                        if (names.Contains(item.Name))
                        {
                            unstagedItems.Add(item);
                        }
                    }

                    unstagedFiles.RemoveAll(item => !item.IsSubmodule && unstagedItems.Contains(item));

                    unstagedFiles.RemoveAll(
                        item =>
                        {
                            if (!item.IsSubmodule
                                || item.GetSubmoduleStatusAsync() is not Task<GitSubmoduleStatus> statusTask
                                || statusTask is null
                                || !statusTask.IsCompleted)
                            {
                                return false;
                            }

                            GitSubmoduleStatus? status = statusTask.CompletedResult();
                            return status is null || (!status.IsDirty && unstagedItems.Contains(item));
                        });

                    foreach (GitItemStatus item in unstagedItems)
                    {
                        if (!item.IsSubmodule)
                        {
                            continue;
                        }

                        GitSubmoduleStatus? gitSubmoduleStatus = ThreadHelper.JoinableTaskFactory.Run(() =>
                            item.GetSubmoduleStatusAsync() ?? Task.FromResult<GitSubmoduleStatus?>(null));

                        if (gitSubmoduleStatus is null || !gitSubmoduleStatus.IsDirty)
                        {
                            continue;
                        }

                        gitSubmoduleStatus.Status = SubmoduleStatus.Unknown;
                    }

                    (GitRevision _, GitRevision indexRev, GitRevision workTreeRev) = GetHeadRevisions();
                    Unstaged.SetDiffs(indexRev, workTreeRev, unstagedFiles);
                    Unstaged.ClearSelected();
                    _skipUpdate = false;
                    Unstaged.SelectStoredNextItem();
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
        }

        if (AppSettings.RevisionGraphShowArtificialCommits)
        {
            UICommands.RepoChangedNotifier.Notify();
        }
    }

    private void ResetSoftClick(object sender, EventArgs e)
    {
        if (!AppSettings.DontConfirmAmend)
        {
            if (MessageBox.Show(this, _amendResetSoft.Text, _amendCommitCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }
        }

        try
        {
            ArgumentString cmd = Commands.Reset(ResetMode.Soft, _resetSoftRevision);
            Module.GitExecutable.RunCommand(cmd);
            Amend.Enabled = false;
            Amend.Checked = false;
            Message.Focus();
        }
        finally
        {
            UICommands.RepoChangedNotifier.Notify();
            Initialize();
        }
    }

    private void SolveMergeConflictsClick(object sender, EventArgs e)
    {
        if (UICommands.StartResolveConflictsDialog(this, false))
        {
            Initialize();
        }
    }

    private void CommitMessageToolStripMenuItemDropDownOpening(object sender, EventArgs e)
    {
        string msg = AppSettings.LastCommitMessage;
        int maxCount = AppSettings.CommitDialogNumberOfPreviousMessages;
        string authorPattern = string.Empty;

        if (ShowOnlyMyMessagesToolStripMenuItem.Checked)
        {
            string userName = Module.GetEffectiveSetting(SettingKeyString.UserName);
            string userEmail = Module.GetEffectiveSetting(SettingKeyString.UserEmail);
            authorPattern = $"^{Regex.Escape(userName)} <{Regex.Escape(userEmail)}>$";
        }

        List<string> prevMessages = Module.GetPreviousCommitMessages(maxCount, "HEAD", authorPattern)
            .WhereNotNull()
            .Select(message => message.TrimEnd('\n'))
            .Where(message => !string.IsNullOrWhiteSpace(message))
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

        commitMessageToolStripMenuItem.DropDown.SuspendLayout();
        commitMessageToolStripMenuItem.DropDownItems.Clear();

        foreach (string prevMsg in prevMessages)
        {
            AddCommitMessageToMenu(prevMsg);
        }

        commitMessageToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
        {
            toolStripMenuItem1,
            generateListOfChangesInSubmodulesChangesToolStripMenuItem,
            ShowOnlyMyMessagesToolStripMenuItem
        });
        commitMessageToolStripMenuItem.DropDown.ResumeLayout();

        void AddCommitMessageToMenu(string commitMessage)
        {
            const int maxLabelLength = 72;

            string label = commitMessage;
            int newlineIndex = label.IndexOf('\n');

            if (newlineIndex != -1)
            {
                label = label[..newlineIndex];
            }

            if (label.Length > maxLabelLength)
            {
                label = label.ShortenTo(maxLabelLength);
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
        if (e.ClickedItem.Tag is not null)
        {
            ReplaceMessage(((string)e.ClickedItem.Tag).Trim());
        }
    }

    private void generateListOfChangesInSubmodulesChangesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        IEnumerable<GitItemStatus> stagedFiles = Staged.AllItems.Select(i => i.Item);

        ISubmodulesConfigFile configFile;
        try
        {
            configFile = Module.GetSubmodulesConfigFile();
        }
        catch (GitConfigurationException ex)
        {
            MessageBoxes.ShowGitConfigurationExceptionMessage(this, ex);
            return;
        }

        Dictionary<string, string> modules = stagedFiles
            .Where(item => item.IsSubmodule
                           && Directory.Exists(_fullPathResolver.Resolve(item.Name))
                           && configFile.ConfigSections.FirstOrDefault(section => section.GetValue("path").Trim() == item.Name)?.SubSection is not null)
            .Select(item => item.Name)
            .ToDictionary(localPath =>
            {
                IConfigSection submodule = configFile.ConfigSections.FirstOrDefault(section => section.GetValue("path").Trim() == localPath);
                Validates.NotNull(submodule?.SubSection);
                return submodule.SubSection.Trim();
            });

        if (modules.Count == 0)
        {
            return;
        }

        StringBuilder sb = new();
        sb.AppendLine("Submodule" + (modules.Count == 1 ? " " : "s ") +
            string.Join(", ", modules.Keys) + " updated");
        sb.AppendLine();

        foreach ((string path, string name) in modules)
        {
            GitArgumentBuilder args = new("diff")
            {
                "--no-ext-diff",
                "--cached",
                "-z",
                "--",
                name.QuoteNE()
            };
            string diff = Module.GitExecutable.GetOutput(args);
            string[] lines = diff.Split(Delimiters.LineFeed, StringSplitOptions.RemoveEmptyEntries);
            const string subprojectCommit = "Subproject commit ";
            string from = lines.Single(s => s.StartsWith("-" + subprojectCommit))[(subprojectCommit.Length + 1)..];
            string to = lines.Single(s => s.StartsWith("+" + subprojectCommit))[(subprojectCommit.Length + 1)..];
            if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
            {
                sb.AppendLine("Submodule " + path + ":");
                GitModule module = new(_fullPathResolver.Resolve(name.EnsureTrailingPathSeparator()));
                args = new GitArgumentBuilder("log")
                {
                    "--pretty=format:\"    %m %h - %s\"",
                    "--no-merges",
                    $"{from}...{to}".Quote()
                };

                string log = module.GitExecutable.GetOutput(args);

                if (log.Length != 0)
                {
                    sb.AppendLine(log);
                }
                else
                {
                    sb.AppendLine("    * Revision changed to " + to[..7]);
                }

                sb.AppendLine();
            }
        }

        ReplaceMessage(sb.ToString().TrimEnd());
    }

    private void SelectedDiffExtraDiffArgumentsChanged(object sender, EventArgs e)
    {
        ShowChanges(_currentItem, _currentItemStaged);
    }

    private void SelectedDiff_PatchApplied(object sender, EventArgs e)
    {
        if (_currentItemStaged)
        {
            Staged.StoreNextItemToSelect();
        }
        else
        {
            Unstaged.StoreNextItemToSelect();
        }

        RescanChanges();
    }

    private void RescanChangesToolStripMenuItemClick(object sender, EventArgs e)
    {
        RescanChanges();
    }

    private void OpenFilesWithDiffTool(IEnumerable<FileStatusItem> items, string? toolName = null)
    {
        foreach (FileStatusItem item in items)
        {
            GitRevision?[] revs = { item.SecondRevision, item.FirstRevision };
            UICommands.OpenWithDifftool(this, revs, item.Item.Name, item.Item.OldName, RevisionDiffKind.DiffAB, item.Item.IsTracked, customTool: toolName);
        }
    }

    private void OpenWithDiffTool()
    {
        OpenFilesWithDiffTool(_currentItemStaged ? Staged.SelectedItems : Unstaged.SelectedItems);
    }

    private void btnResetAllChanges_Click(object sender, EventArgs e)
    {
        ResetChanges(onlyWorkTree: false);
    }

    private void btnResetUnstagedChanges_Click(object sender, EventArgs e)
    {
        ResetChanges(onlyWorkTree: true);
    }

    private void ResetChanges(bool onlyWorkTree)
    {
        BypassFormActivatedEventHandler(() => UICommands.StartResetChangesDialog(this, Unstaged.AllItems.Select(i => i.Item).ToList(), onlyWorkTree));
        Initialize();
    }

    private void StashStagedClick(object sender, EventArgs e)
    {
        BypassFormActivatedEventHandler(() => UICommands.StashStaged(owner: this));
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

    private void CommitAndPush_Click(object sender, EventArgs e)
    {
        if (CommitAndPush.Text == TranslatedStrings.ButtonPush)
        {
            bool pushForced = CommitAndPush.BackColor == OtherColors.AmendButtonForcedColor;
            UICommands.StartPushDialog(owner: this, pushOnShow: true, forceWithLease: pushForced, out _);
            return;
        }

        CheckForStagedAndCommit(push: true);
    }

    private void UpdateAuthorInfo()
    {
        ThreadHelper.FileAndForget(async () =>
            {
                string committer = $"{_commitCommitterInfo.Text} {GetSetting(SettingKeyString.UserName)} <{GetSetting(SettingKeyString.UserEmail)}>";

                await this.SwitchToMainThreadAsync();
                commitAuthorStatus.Text = string.IsNullOrWhiteSpace(toolAuthor.Text)
                    ? committer
                    : $"{committer} {_commitAuthorInfo.Text} {toolAuthor.Text}";

                return;

                // Do not cache results in order to update the info on FormActivate
                string GetSetting(string key) => Module.GetEffectiveSetting(key, defaultValue: $"/{string.Format(TranslatedStrings.NotConfigured, key)}/");
            });
    }

    private void ExecuteCommitCommand()
    {
        CheckForStagedAndCommit(push: false);
    }

    private void Message_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.Enter)
        {
            ExecuteCommitCommand();
            e.Handled = true;
        }
    }

    private void OpenConventionalCommitMenu(bool insertScope)
    {
        commitTemplatesToolStripMenuItem.ShowDropDown();
        _conventionalCommitItem.ShowDropDown();
        _conventionalCommitItem.DropDownItems.Cast<ToolStripItem>().First(i => i.Text == _feat).Select();
        _insertScopeParentheses = insertScope;
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

        int lineCount = Message.LineCount();

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
            bool changed = false;

            if (limit1 > 0 && line == 0)
            {
                ColorTextAsNecessary(limit1, fullRefresh: false);
            }

            if (empty2 && line == 1)
            {
                // Ensure next line. Optionally add a bullet.
                Message.EnsureEmptyLine(commitValidationIndentAfterFirstLine, 1);
                Message.ChangeTextColor(2, 0, Message.LineLength(2), SystemColors.ControlText);
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
                int lineLength = Message.LineLength(line);
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
                    Message.ChangeTextColor(line, offset, len, SystemColors.WindowText);
                }

                if (lineLength > lineLimit)
                {
                    if (offset <= lineLimit || !textAppended)
                    {
                        offset = Math.Max(offset, lineLimit);
                        len = lineLength - offset;
                        if (len > 0)
                        {
                            Message.ChangeTextColor(line, offset, len, Color.Red.AdaptTextColor());
                        }
                    }
                }
            }

            bool WrapIfNecessary()
            {
                if (Message.LineLength(line) > limitX)
                {
                    string oldText = Message.Line(line);
                    string newText = WordWrapper.WrapSingleLine(oldText, limitX);
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
                DebugHelpers.Assert(_formattedLines.Count == lineNumber, $"{_formattedLines.Count}:{lineNumber}");
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
        AppSettings.RefreshArtificialCommitOnApplicationActivated = refreshDialogOnFormFocusToolStripMenuItem.Checked;
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

    #endregion

    private void commitTemplatesToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
    {
        int registeredTemplatesCount = _commitTemplateManager.RegisteredTemplates.Count();
        if (_shouldReloadCommitTemplates || _alreadyLoadedTemplatesCount != registeredTemplatesCount)
        {
            LoadCommitTemplates();
            _shouldReloadCommitTemplates = false;
            _alreadyLoadedTemplatesCount = registeredTemplatesCount;
        }

        return;

        void LoadCommitTemplates()
        {
            commitTemplatesToolStripMenuItem.DropDown.SuspendLayout();
            commitTemplatesToolStripMenuItem.DropDownItems.Clear();

            // Add registered templates
            bool isItemAdded = false;
            foreach (CommitTemplateItem item in _commitTemplateManager.RegisteredTemplates)
            {
                isItemAdded |= CreateToolStripItem(item);
            }

            if (isItemAdded)
            {
                AddSeparator();
                isItemAdded = false;
            }

            // Add templates from settings
            foreach (CommitTemplateItem item in CommitTemplateItem.LoadFromSettings() ?? Array.Empty<CommitTemplateItem>())
            {
                isItemAdded |= CreateToolStripItem(item);
            }

            if (isItemAdded)
            {
                AddSeparator();
            }

            AddConventionalCommitsItems();

            AddSeparator();

            AddSettingsItem();
            commitTemplatesToolStripMenuItem.DropDown.ResumeLayout();

            return;

            bool CreateToolStripItem(CommitTemplateItem item)
            {
                if (string.IsNullOrEmpty(item.Name))
                {
                    return false;
                }

                ToolStripMenuItem toolStripItem = new(item.Name, item.Icon);
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
                return true;
            }

            void AddSeparator()
            {
                commitTemplatesToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            }

            void AddSettingsItem()
            {
                ToolStripMenuItem settingsItem = new(_commitMessageSettings.Text, Images.Settings);
                settingsItem.Click += delegate
                {
                    using FormCommitTemplateSettings frm = new(UICommands);
                    frm.ShowDialog(this);

                    _shouldReloadCommitTemplates = true;
                };
                commitTemplatesToolStripMenuItem.DropDownItems.Add(settingsItem);
            }

            void AddConventionalCommitsItems()
            {
                _conventionalCommitItem = new(_conventionalCommit.Text, Images.GitCommandLog);

                foreach (string conventionKeyword in _headerCommitTypes)
                {
                    ToolStripMenuItem commitTypeMenuItem = new(conventionKeyword, null, (_, _) =>
                    {
                        (string title, int selectionStart) = PrefixOrReplaceKeyword(conventionKeyword);

                        if (Message.Text.Length == 0)
                        {
                            Message.Text = title;
                        }
                        else
                        {
                            Message.ReplaceLine(0, title);
                        }

                        Message.SelectionStart = selectionStart;
                        Message.Focus();
                    });

                    if (commitTypeMenuItem.Text == _feat)
                    {
                        string hotkey1 = GetShortcutKeyDisplayString(Command.ConventionalCommit_PrefixMessage);
                        string hotkey2 = GetShortcutKeyDisplayString(Command.ConventionalCommit_PrefixMessageWithScope);

                        commitTypeMenuItem.ShortcutKeyDisplayString = hotkey1;

                        if (!string.IsNullOrEmpty(hotkey1) && !string.IsNullOrEmpty(hotkey2))
                        {
                            commitTypeMenuItem.ShortcutKeyDisplayString += ", ";
                        }

                        commitTypeMenuItem.ShortcutKeyDisplayString += hotkey2;
                    }

                    _conventionalCommitItem.DropDownItems.Add(commitTypeMenuItem);
                }

                _conventionalCommitItem.DropDownItems.Add(new ToolStripSeparator());

                foreach (string footerKeyword in _footerKeywords)
                {
                    AddFooter(footerKeyword, $"{footerKeyword}: ");
                }

                AddFooter("[skip ci]", keepCursorPosition: true);

                void AddFooter(string itemText, string messageText = null, bool keepCursorPosition = false)
                {
                    messageText ??= itemText;
                    _conventionalCommitItem.DropDownItems.Add(itemText, null, (_, _) =>
                    {
                        int lineCount = Message.LineCount();
                        if (lineCount == 0)
                        {
                            Message.Text = $"{Environment.NewLine}{messageText}";
                        }
                        else
                        {
                            int lastLine = lineCount - 1;
                            string currentLastLine = Message.Line(lastLine);
                            Message.ReplaceLine(lastLine, $"{currentLastLine}{Environment.NewLine}{messageText}");
                        }

                        if (!keepCursorPosition)
                        {
                            Message.SelectionStart = Message.Text.Length;
                        }

                        Message.Focus();
                    });
                }

                _conventionalCommitItem.DropDownItems.Add(new ToolStripSeparator());

                _conventionalCommitItem.DropDownItems.Add(_conventionalCommitDocumentation.Text, Images.Information, (_, _)
                    => OsShellUtil.OpenUrlInDefaultBrowser("https://www.conventionalcommits.org"));

                commitTemplatesToolStripMenuItem.DropDownItems.Add(_conventionalCommitItem);
            }
        }
    }

    private (string message, int selectionStart) PrefixOrReplaceKeyword(string keyword)
    {
        int currentPosition = Message.SelectionStart;
        string scope = _insertScopeParentheses ? "()" : "";
        int scopePosition = keyword.Length + 1;
        int titlePosition = keyword.Length + (scope.Length / 2) + 2;

        string currentTitle = string.IsNullOrWhiteSpace(Message.Text) ? string.Empty : Message.Line(0);

        // Replacing current keyword
        foreach (string key in _headerCommitTypes)
        {
            if (!currentTitle.StartsWith(key))
            {
                continue;
            }

            if (currentTitle.Length == key.Length)
            {
                return ($"{keyword}{scope}: ", _insertScopeParentheses ? scopePosition : titlePosition);
            }

            char nextChar = currentTitle[key.Length];
            if (!_insertScopeParentheses)
            {
                if (nextChar == ':' || nextChar == '(' || nextChar == '!')
                {
                    return ReplaceKeyword(_ => titlePosition);
                }
            }
            else
            {
                if (nextChar == ':' || nextChar == '!')
                {
                    return ($"{keyword}(){currentTitle.Substring(key.Length)}", scopePosition);
                }

                if (nextChar == '(')
                {
                    return ReplaceKeyword(newTitle => 2 + Math.Max(newTitle.IndexOf(":"), newTitle.IndexOf("(")));
                }
            }

            (string message, int selectionStart) ReplaceKeyword(Func<string, int> maxPosition)
            {
                string newTitle = $"{keyword}{currentTitle.Substring(key.Length)}";
                int newMessageLength = Message.Text.Length + newTitle.Length - currentTitle.Length;
                return (newTitle, Math.Min(newMessageLength, Math.Max(maxPosition(newTitle), currentPosition + keyword.Length - key.Length)));
            }
        }

        // Append current keyword
        return ($"{keyword}{scope}: {currentTitle}", _insertScopeParentheses ? scopePosition : titlePosition + currentPosition);
    }

    private void Amend_CheckedChanged(object sender, EventArgs e)
    {
        AmendPanel.Visible = Amend.Checked;

        if (!Amend.Checked && ResetAuthor.Checked)
        {
            ResetAuthor.Checked = false;
        }

        if (string.IsNullOrEmpty(Message.Text) && Amend.Checked)
        {
            ReplaceMessage(Module.GetPreviousCommitMessages(count: 1, revision: "HEAD", authorPattern: string.Empty).FirstOrDefault()?.Trim());
        }

        ResetSoft.Enabled = ResetSoft.Visible && Amend.Checked && Module.RevParse(_resetSoftRevision) is not null;

        if (AppSettings.CommitAndPushForcedWhenAmend)
        {
            CommitAndPush.BackColor = PushForced
                ? OtherColors.AmendButtonForcedColor
                : SystemColors.ButtonFace;

            CommitAndPush.SetForeColorForBackColor();
        }

        UpdateButtonStates();

        SelectStaged();
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
        bool branchCreated = UICommands.StartCreateBranchDialog(this);
        if (!branchCreated)
        {
            return;
        }

        ThreadHelper.FileAndForget(UpdateBranchNameDisplayAsync);
    }

    private void Message_Enter(object sender, EventArgs e)
    {
        SelectStaged();
    }

    private void modifyCommitMessageButton_Click(object sender, EventArgs e)
    {
        CommitKind = CommitKind.Normal;
        Message.Focus();
    }

    private void UpdateButtonStates()
    {
        btnResetAllChanges.Enabled = Unstaged.AllItems.Any() || Staged.AllItems.Any();
        CommitAndPush.Text = PushForced ? _commitAndForcePush.Text
            : btnResetAllChanges.Enabled || Amend.Checked ? _commitAndPush.Text
            : TranslatedStrings.ButtonPush;
    }

    private void UICommands_PostRepositoryChanged(object sender, GitUIEventArgs e)
    {
        if (!_skipUpdate && !_bypassActivatedEventHandler)
        {
            ThreadHelper.FileAndForget(async () =>
                {
                    await this.SwitchToMainThreadAsync();
                    RescanChanges();
                });
        }
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    private void Options_DropDownOpening(object sender, EventArgs e)
    {
        refreshDialogOnFormFocusToolStripMenuItem.Checked = AppSettings.RefreshArtificialCommitOnApplicationActivated;
    }

    internal readonly struct TestAccessor
    {
        private readonly FormCommit _formCommit;

        internal TestAccessor(FormCommit formCommit)
        {
            _formCommit = formCommit;
        }

        internal ToolStripMenuItem EditFileToolStripMenuItem => _formCommit._currentFilesList.tsmiEditWorkingDirectoryFile;

        internal ToolStripButton StageAllToolItem => _formCommit.toolStageAllItem;

        internal ToolStripButton UnstageAllToolItem => _formCommit.toolUnstageAllItem;

        internal FileStatusList UnstagedList => _formCommit.Unstaged;

        internal FileStatusList StagedList => _formCommit.Staged;

        internal EditNetSpell Message => _formCommit.Message;

        internal FileViewer SelectedDiff => _formCommit.SelectedDiff;

        internal SplitContainer MainSplitter => _formCommit.splitMain;

        internal ToolStripDropDownButton CommitMessageToolStripMenuItem => _formCommit.commitMessageToolStripMenuItem;

        internal ToolStripStatusLabel CommitAuthorStatusToolStripStatusLabel => _formCommit.commitAuthorStatus;

        internal ToolStripStatusLabel CurrentBranchNameLabelStatus => _formCommit.branchNameLabel;

        internal ToolStripStatusLabel RemoteNameLabelStatus => _formCommit.remoteNameLabel;

        internal bool ExecuteCommand(Command command) => _formCommit.ExecuteCommand((int)command);

        internal bool ExecuteCommand(RevisionDiffControl.Command command) => _formCommit._currentFilesList.ExecuteCommand(command);

        internal Rectangle Bounds => _formCommit.Bounds;

        internal CheckBox ResetAuthor => _formCommit.ResetAuthor;

        internal CheckBox Amend => _formCommit.Amend;

        internal Button Commit => _formCommit.Commit;

        internal Button CommitAndPush => _formCommit.CommitAndPush;

        internal string CommitAndForcePushText => _formCommit._commitAndForcePush.Text;

        internal Button ResetSoft => _formCommit.ResetSoft;

        internal void RescanChanges() => _formCommit.RescanChanges();

        internal (string message, int selectionStart) PrefixOrReplaceKeyword(string keyword)
            => _formCommit.PrefixOrReplaceKeyword(keyword);

        internal bool IncludeFeatureParentheses { set => _formCommit._insertScopeParentheses = value; }
        internal void SetMessageState(string text, int position)
        {
            _formCommit.Message.Text = text;
            _formCommit.Message.SelectionStart = position;
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
    Squash,
    Amend,
}

public static class CommitKindExtensions
{
    public static string GetPrefix(this CommitKind commitKind)
        => commitKind switch
        {
            CommitKind.Fixup => "fixup!",
            CommitKind.Squash => "squash!",
            CommitKind.Amend => "amend!",
            CommitKind.Normal => string.Empty,
            _ => throw new System.ComponentModel.InvalidEnumArgumentException(nameof(commitKind), (int)commitKind, typeof(CommitKind)),
        };
}
