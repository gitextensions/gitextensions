using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.AutoCompletion;
using GitUI.CommandsDialogs.CommitDialog;
using GitUI.Compat;
using GitUI.Editor;
using GitUI.HelperDialogs;
using GitUI.ScriptsEngine;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public sealed partial class FormCommit : GitModuleForm
{
    private const string _resetSoftRevision = "HEAD~1";

    private readonly TranslationString _amendCommit = new(
        "You are about to rewrite history." + Environment.NewLine
        + "Only use Amend if the commit has not been published yet!" + Environment.NewLine
        + Environment.NewLine
        + "Do you want to continue?");
    private readonly TranslationString _amendResetSoft = new(
        "You are about to rewrite history by Soft Reset to the previous commit." + Environment.NewLine
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
    private readonly TranslationString _mergeConflicts = new("There are unresolved merge conflicts, solve merge conflicts before committing.");
    private readonly TranslationString _mergeConflictsCaption = new("Merge conflicts");
    private readonly TranslationString _noFilesStagedAndConfirmAnEmptyMergeCommit = new("There are no files staged for this commit.\nAre you sure you want to commit?");
    private readonly TranslationString _noFilesStagedCommitAllFilteredUnstagedOption = new("Stage and commit the unstaged files that match your filter");
    private readonly TranslationString _noFilesStagedCommitAllUnstagedOption = new("Stage and commit all unstaged files");
    private readonly TranslationString _noFilesStagedMakeEmptyCommitOption = new("Make an empty commit");
    private readonly TranslationString _noFilesStagedCommitCaption = new("Confirm commit");
    private readonly TranslationString _noFilesStagedCommitInstructions = new("There aren't any changes in the staging area.\nHow do you want to proceed?");
    private readonly TranslationString _noStagedChanges = new("There are no staged changes");
    private readonly TranslationString _noUnstagedChanges = new("There are no unstaged changes");
    private readonly TranslationString _notOnBranch = new(
        "This commit will be unreferenced when switching to another branch and can be lost."
        + Environment.NewLine + Environment.NewLine + "Do you want to continue?");
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
    private readonly TranslationString _commitMsgFirstLineInvalid = new(
        "First line of commit message contains too many characters."
        + Environment.NewLine + "Do you want to continue?");
    private readonly TranslationString _commitMsgLineInvalid = new(
        "The following line of commit message contains too many characters:"
        + Environment.NewLine + Environment.NewLine + "{0}" + Environment.NewLine + Environment.NewLine + "Do you want to continue?");
    private readonly TranslationString _commitMsgSecondLineNotEmpty = new(
        "Second line of commit message is not empty." + Environment.NewLine + "Do you want to continue?");
    private readonly TranslationString _commitMsgRegExNotMatched = new(
        "Commit message does not match RegEx." + Environment.NewLine + "Do you want to continue?");
    private readonly TranslationString _commitValidationCaption = new("Commit validation");
    private readonly TranslationString _commitMessageSettings = new("&Edit commit message templates and settings...");
    private readonly TranslationString _conventionalCommit = new("Conven&tional Commits");
    private readonly TranslationString _conventionalCommitDocumentation = new("Documentation...");
    private readonly TranslationString _commitAuthorInfo = new("Author");
    private readonly TranslationString _commitCommitterInfo = new("Committer");
    private readonly TranslationString _commitCommitterToolTip = new("Click to change committer information.");
    private readonly TranslationString _modifyCommitMessageButtonToolTip = new(
        "If you change the first line of the commit message, git will treat this commit as an ordinary commit,"
        + Environment.NewLine + "i.e. it may no longer be a fixup or an autosquash commit.");
    private readonly TranslationString _templateNotFoundCaption = new("Template Error");
    private readonly TranslationString _templateNotFound = new(
        $"Template not found: {{0}}.{Environment.NewLine}{Environment.NewLine}You can set your template:{Environment.NewLine}\t$ git config commit.template ./.git_commit_msg.txt{Environment.NewLine}You can unset the template:{Environment.NewLine}\t$ git config --unset commit.template");
    private readonly TranslationString _templateLoadErrorCaption = new("Template could not be loaded");
    private readonly TranslationString _statusBarBranchWithoutRemote = new("(remote not configured)");
    private readonly TranslationString _untrackedRemote = new("(untracked)");

    private readonly TranslationString _wordWrapCommitMessageBody = new("&Word wrap (except subject line)");
    private event Action? OnStageAreaLoaded;

    private readonly ICommitTemplateManager _commitTemplateManager = null!;
    private readonly WinFormsShims.Control _commitMessageManagerOwner = null!;
    private readonly GitRevision? _editedCommit;
    private readonly MenuItem _addSelectionToCommitMessageToolStripMenuItem = null!;
    private readonly AsyncLoader _unstagedLoader = new();
    private readonly bool _useFormCommitMessage = AppSettings.UseFormCommitMessage;
    private readonly CancellationTokenSequence _customDiffToolsSequence = new();
    private readonly CancellationTokenSequence _interactiveAddSequence = new();
    private readonly CancellationTokenSequence _viewChangesSequence = new();
    private readonly List<Task> _viewTasks = [];
    private readonly List<Task> _lifecycleTasks = [];
    private readonly CancellationTokenSequence _commitSequence = new();
    private readonly SplitterManager _splitterManager = new(new AppSettingsPath("CommitDialog"));
    private readonly Subject<string> _selectionFilterSubject = new();
    private readonly IFullPathResolver _fullPathResolver = null!;
    private readonly List<string> _formattedLines = [];

    private const string _feat = "feat";

    private static readonly string[] _headerCommitTypes = ["build", "chore", "ci", "docs", _feat, "fix", "perf", "refactor", "style", "test"];
    private static readonly string[] _footerKeywords = ["BREAKING CHANGE", "Co-authored-by", "Reviewed-by"];

    private readonly ObservableCollection<string> _selectionFilterHistory = [];
    private readonly IDisposable? _selectionFilterSubscription;
    private Task _unstagedTask = Task.CompletedTask;
    private Task _closePersistenceTask = Task.CompletedTask;
    private bool _insertScopeParentheses;
    private CommitKind _commitKind;
    private bool _changingSelection;
    private bool _commitInProgress;
    private bool _indexOperationInProgress;
    private FileStatusList _currentFilesList = null!;
    private bool _skipUpdate;
    private FileStatusItem? _currentItem;
    private bool _currentItemStaged;
    private readonly ICommitMessageManager _commitMessageManager = null!;
    private bool _assigningInitialMessage;
    private string? _commitTemplate;
    private bool _messageEditedByUser;
    private bool _messageConsumed;
    private bool _isMergeCommit;
    private bool _shouldRescanChanges = true;
    private bool _shouldReloadCommitTemplates = true;
    private bool _bypassActivatedEventHandler;
    private bool _loadUnstagedOutputFirstTime = true;
    private bool _commitMessageInitialized;
    private bool _subscribedToRepositoryChanges;
    private bool _suppressRepositoryChangeReload;
    private bool _initialized;
    private IReadOnlyList<GitItemStatus>? _currentSelection;
    private int _alreadyLoadedTemplatesCount = -1;
    private EventHandler? _branchNameLabelOnClick;
    private MenuItem? _conventionalCommitItem;

    /// <summary>
    /// Regex to find message replace pattern: {{ group1 }}[ group2 ]
    /// </summary>
    [GeneratedRegex(@"\{\{(?<pattern>.*?)\}\}(?:\[(?<index>\d+)\])?", RegexOptions.ExplicitCapture)]
    private static partial Regex ReplaceMessageRegex();

    private CommitKind CommitKind
    {
        get => _commitKind;
        set
        {
            _commitKind = value;
            ApplyCommitKind();
        }
    }

    public FormCommit()
    {
        InitializeComponent();
        toolStripStatusBranchIcon.Source = Properties.Images.Branch.AdaptLightness();
        InitializeComplete();
    }

    public FormCommit(
        IGitUICommands commands,
        CommitKind commitKind = CommitKind.Normal,
        GitRevision? editedCommit = null,
        string? commitMessage = null)
        : base(commands, enablePositionRestore: true)
    {
        _commitKind = commitKind;
        _editedCommit = editedCommit;

        InitializeComponent();
        RestoreSplitters();
        toolStripStatusBranchIcon.Source = Properties.Images.Branch.AdaptLightness();

        _currentFilesList = Unstaged;
        _unstagedLoader.LoadingError += (_, args) => WinFormsShims.Application.OnThreadException(args.Exception);
        Unstaged.SelectionMode = SelectionMode.Multiple;
        Staged.SelectionMode = SelectionMode.Multiple;
        Unstaged.BindContextMenu(() => ReloadChanges(), canAutoRefresh: true, StageSelected, unstage: null);
        Staged.BindContextMenu(() => ReloadChanges(), canAutoRefresh: false, stage: null, UnstageSelected);
        SelectedDiff.LinePatchingBlocksUntilReload = true;
        SelectedDiff.ExtraDiffArgumentsChanged += SelectedDiffExtraDiffArgumentsChanged;
        SelectedDiff.PatchApplied += SelectedDiff_PatchApplied;
        SelectedDiff.TopScrollReached += FileViewer_TopScrollReached;
        SelectedDiff.BottomScrollReached += FileViewer_BottomScrollReached;
        SelectedDiff.EscapePressed += () => Close();
        SelectedDiff.AddContextMenuSeparator();
        _addSelectionToCommitMessageToolStripMenuItem = SelectedDiff.AddContextMenuEntry(_addSelectionToCommitMessage.Text, (_, _) => AddSelectionToCommitMessage());
        Message.ContextMenuPopulating += Message_ContextMenuPopulating;
        Unstaged.SelectedIndexChanged += UnstagedSelectionChanged;
        Staged.SelectedIndexChanged += StagedSelectionChanged;
        Unstaged.Enter += Unstaged_Enter;
        Staged.Enter += Staged_Enter;
        Unstaged.FilterChanged += Unstaged_FilterChanged;
        Staged.FilterChanged += Staged_FilterChanged;
        Unstaged.SetNoFilesText(_noUnstagedChanges.Text);
        Staged.SetNoFilesText(_noStagedChanges.Text);
        Unstaged.DisableSubmoduleMenuItemBold = true;
        Staged.DisableSubmoduleMenuItemBold = true;
        Unstaged.DoubleClick += Unstaged_DoubleClick;
        Staged.DoubleClick += Staged_DoubleClick;
        Unstaged.DataSourceChanged += Staged_DataSourceChanged;
        Staged.DataSourceChanged += Staged_DataSourceChanged;
        toolStageItem.Click += StageClick;
        toolStageAllItem.Click += toolStageAllItem_Click;
        toolUnstageItem.Click += UnstageFilesClick;
        toolUnstageAllItem.Click += toolUnstageAllItem_Click;
        btnResetAllChanges.Click += btnResetAllChanges_Click;
        btnResetUnstagedChanges.Click += btnResetUnstagedChanges_Click;
        Commit.Click += CommitClick;
        CommitAndPush.Click += CommitAndPush_Click;
        StashStaged.Click += StashStagedClick;
        Message.TextChanged += Message_TextChanged;
        Message.TextAssigned += Message_TextAssigned;
        Message.KeyDown += Message_KeyDown;
        Message.GotFocus += Message_Enter;
        Message.SelectionChanged += Message_SelectionChanged;
        Message.AddAutoCompleteProvider(new CommitAutoCompleteProvider(() => Module));
        Message.AddAutoCompleteProvider(new CommitMessageMetadataProvider());
        Amend.IsCheckedChanged += Amend_CheckedChanged;
        ResetSoft.Click += ResetSoftClick;
        StageInSuperproject.IsCheckedChanged += StageInSuperproject_CheckedChanged;
        modifyCommitMessageButton.Click += modifyCommitMessageButton_Click;
        SolveMergeconflicts.Click += SolveMergeConflictsClick;
        createBranchToolStripButton.Click += createBranchToolStripButton_Click;
        toolAuthor.TextChanged += toolAuthor_TextChanged;
        toolAuthor.LostFocus += toolAuthor_Leave;
        toolAuthorLabelItem.Click += toolAuthorLabelItem_Click;
        commitAuthorStatus.Click += commitCommitter_Click;
        gpgSignCommitToolStripComboBox.SelectionChanged += gpgSignCommitChanged;
        closeDialogAfterEachCommitToolStripMenuItem.Click += closeDialogAfterEachCommitToolStripMenuItem_Click;
        closeDialogAfterAllFilesCommittedToolStripMenuItem.Click += closeDialogAfterAllFilesCommittedToolStripMenuItem_Click;
        refreshDialogOnFormFocusToolStripMenuItem.Click += refreshDialogOnFormFocusToolStripMenuItem_Click;
        tsmiSelectStagedOnEnterMessage.Click += tsmiSelectStagedOnEnterMessage_Click;
        signOffToolStripMenuItem.Click += signOffToolStripMenuItem_Click;
        ShowOnlyMyMessagesToolStripMenuItem.Click += ShowOnlyMyMessagesToolStripMenuItem_CheckedChanged;
        generateListOfChangesInSubmodulesChangesToolStripMenuItem.Click += generateListOfChangesInSubmodulesChangesToolStripMenuItem_Click;
        ((MenuFlyout)commitMessageToolStripMenuItem.Flyout!).Opening += CommitMessageToolStripMenuItemDropDownOpening;
        ((MenuFlyout)commitTemplatesToolStripMenuItem.Flyout!).Opening += commitTemplatesToolStripMenuItem_DropDownOpening;
        ((Flyout)tsmiOptions.Flyout!).Opening += Options_DropDownOpening;
        commitTemplatesOverflowMenuItem.Click += (_, _) => commitTemplatesToolStripMenuItem.Flyout!.ShowAt(toolbarCommitOverflow);
        createBranchOverflowMenuItem.Click += createBranchToolStripButton_Click;
        toolbarCommit.SizeChanged += (_, _) => UpdateToolbarCommitOverflow();
        _branchNameLabelOnClick = (_, _) => UICommands.StartRemotesDialog(this, preselectLocal: Module.GetSelectedBranch());
        remoteNameLabel.Click += (_, _) => _branchNameLabelOnClick(remoteNameLabel, EventArgs.Empty);
        _commitMessageManagerOwner = new WinFormsShims.Control();
        _commitMessageManager = new CommitMessageManager(
            _commitMessageManagerOwner,
            Module.WorkingDirGitDir,
            Module.CommitEncoding,
            commitMessage);
        _commitTemplateManager = new CommitTemplateManager(() => Module);
        _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);

        Message.TextBoxFont = AppSettings.CommitFont;
        Message.WatermarkText = _useFormCommitMessage ? _enterCommitMessageHint.Text : _commitMessageDisabled.Text;
        HotkeysEnabled = true;
        LoadHotkeys(HotkeySettingsName);

        selectionFilter.ItemsSource = _selectionFilterHistory;
        selectionFilter.PropertyChanged += OnSelectionFilterTextChanged;
        selectionFilter.SelectionChanged += OnSelectionFilterIndexChanged;
        _selectionFilterSubscription = _selectionFilterSubject
            .Throttle(TimeSpan.FromMilliseconds(250))
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(filterText => TaskManager.HandleExceptions(
                () => ApplySelectionFilter(filterText),
                WinFormsShims.Application.OnThreadException));

        bool closeAfterCommit = AppSettings.CloseCommitDialogAfterCommit;
        bool closeAfterLastCommit = AppSettings.CloseCommitDialogAfterLastCommit;
        bool refreshOnFocus = AppSettings.RefreshArtificialCommitOnApplicationActivated;
        bool selectStagedOnEnter = AppSettings.CommitDialogSelectStagedOnEnterMessage.Value;
        _skipUpdate = true;
        closeDialogAfterEachCommitToolStripMenuItem.IsChecked = closeAfterCommit;
        closeDialogAfterAllFilesCommittedToolStripMenuItem.IsChecked = closeAfterLastCommit;
        refreshDialogOnFormFocusToolStripMenuItem.IsChecked = refreshOnFocus;
        tsmiSelectStagedOnEnterMessage.IsChecked = selectStagedOnEnter;
        _skipUpdate = false;
        ShowOnlyMyMessagesToolStripMenuItem.IsChecked = AppSettings.CommitDialogShowOnlyMyMessages;
        toolbarSelectionFilter.IsVisible = AppSettings.CommitDialogSelectionFilter;
        btnResetAllChanges.IsVisible = AppSettings.ShowResetAllChanges;
        btnResetUnstagedChanges.IsVisible = AppSettings.ShowResetWorkTreeChanges;
        CommitAndPush.IsVisible = AppSettings.ShowCommitAndPush;
        StashStaged.IsVisible = Module.GitVersion.SupportStashStaged;
        StageInSuperproject.IsVisible = Module.SuperprojectModule is not null;
        StageInSuperproject.IsChecked = AppSettings.StageInSuperprojectAfterCommit;
        ApplyCommitKind();
        UpdateToolbarCommitOverflow();
        ReloadChanges();

        InitializeComplete();
    }

    /// <summary>
    /// Flag whether the push needs to be forced, i.e. after amending a commit or after soft reset to the previous commit.
    /// </summary>
    /// The Amend checkbox is disabled after soft reset.
    private bool PushForced => (Amend.IsChecked == true || !Amend.IsEnabled) && AppSettings.CommitAndPushForcedWhenAmend;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
    }

    private void RestoreSplitters()
    {
        _splitterManager.AddSplitter(splitMain, nameof(splitMain), defaultDistance: 397);
        _splitterManager.AddSplitter(splitRight, nameof(splitRight), defaultDistance: 412);
        _splitterManager.AddSplitter(splitLeft, nameof(splitLeft), defaultDistance: 268);
        _splitterManager.RestoreSplitters();
    }

    protected override void OnShown(EventArgs e)
    {
        UpdateToolbarCommitOverflow();

        if (!_initialized)
        {
            Initialize();
        }

        if (_commitMessageInitialized || _commitMessageManager is null)
        {
            base.OnShown(e);
            return;
        }

        _commitMessageInitialized = true;
        TrackLifecycleTask(InitializeCommitMessageAsync());
        UpdateAuthorInfo();
        TrackLifecycleTask(UpdateBranchNameDisplayAsync());
        base.OnShown(e);
    }

    protected override void OnApplicationActivated()
    {
        if (!_bypassActivatedEventHandler && AppSettings.RefreshArtificialCommitOnApplicationActivated)
        {
            RescanChanges();
        }

        base.OnApplicationActivated();
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (KeysMapper.ToKeys(e) == (WinFormsShims.Keys.Control | WinFormsShims.Keys.Enter)
            && !Message.IsKeyboardFocusWithin)
        {
            FocusCommitMessage();
            e.Handled = true;
        }

        base.OnKeyUp(e);
    }

    protected override void OnUICommandsChanged(GitUICommandsChangedEventArgs e)
    {
        e.OldCommands?.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
        if (TryGetUICommands(out IGitUICommands? commands))
        {
            UICommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
            UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
            _subscribedToRepositoryChanges = true;
        }

        base.OnUICommandsChanged(e);
    }

    private async Task InitializeCommitMessageAsync()
    {
        string message;
        bool amend = false;
        switch (_commitKind)
        {
            case CommitKind.Fixup:
            case CommitKind.Squash:
                ArgumentNullException.ThrowIfNull(_editedCommit);
                message = AddCommitKindPrefix(_editedCommit.Subject);
                break;
            case CommitKind.Amend:
                ArgumentNullException.ThrowIfNull(_editedCommit);
                message = $"{AddCommitKindPrefix(_editedCommit.Subject)}{Environment.NewLine}{Environment.NewLine}{_editedCommit.Body}";
                break;
            default:
                message = await _commitMessageManager.GetMergeOrCommitMessageAsync();
                amend = !_commitMessageManager.IsMergeCommit && await _commitMessageManager.GetAmendStateAsync();
                break;
        }

        if (_useFormCommitMessage && string.IsNullOrEmpty(message))
        {
            try
            {
                message = _commitTemplateManager.LoadGitCommitTemplate() ?? string.Empty;
                _commitTemplate = message;
            }
            catch (FileNotFoundException ex)
            {
                await this.SwitchToMainThreadAsync();
                MessageBoxes.Show(this, string.Format(_templateNotFound.Text, ex.FileName), _templateNotFoundCaption.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
                message = string.Empty;
            }
            catch (Exception ex)
            {
                await this.SwitchToMainThreadAsync();
                MessageBoxes.Show(this, ex.Message, _templateLoadErrorCaption.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
                message = string.Empty;
            }
        }

        await this.SwitchToMainThreadAsync();
        if (!_messageEditedByUser)
        {
            _assigningInitialMessage = true;
            Message.Text = message;
            _assigningInitialMessage = false;
        }

        Amend.IsChecked = amend;
        ApplyCommitKind();
        UpdateStageButtons();

        string AddCommitKindPrefix(string subject)
        {
            string prefix = _commitKind.GetPrefix();
            return subject.StartsWith(prefix, StringComparison.Ordinal) ? subject : $"{prefix} {subject}";
        }
    }

    private void ApplyCommitKind()
    {
        bool canEditMessage = _useFormCommitMessage && _commitKind is CommitKind.Normal or CommitKind.Amend;
        Message.IsEnabled = canEditMessage;
        commitMessageToolStripMenuItem.IsEnabled = canEditMessage;
        commitTemplatesToolStripMenuItem.IsEnabled = canEditMessage;
        modifyCommitMessageButton.IsVisible = _useFormCommitMessage && _commitKind is CommitKind.Fixup or CommitKind.Squash;
    }

    private void ReloadChanges(bool preferStaged = false)
    {
        _currentSelection = _currentFilesList?.SelectedGitItems;
        if (preferStaged)
        {
            _currentFilesList = Staged;
        }

        Message.RefreshAutoCompleteWords();
        Initialize();
    }

    private void UpdateToolbarCommitOverflow()
    {
        commitTemplatesToolStripMenuItem.Measure(Avalonia.Size.Infinity);
        createBranchToolStripButton.Measure(Avalonia.Size.Infinity);
        double inlineWidth = commitTemplatesToolStripMenuItem.DesiredSize.Width + createBranchToolStripButton.DesiredSize.Width;
        double availableInlineWidth = toolbarCommit.Bounds.Width
            - commitMessageToolStripMenuItem.Width
            - tsmiOptions.Width
            - toolbarCommitOverflow.Width;
        bool showInline = availableInlineWidth >= inlineWidth;
        toolbarCommitInlineItems.IsVisible = showInline;
        toolbarCommitOverflow.IsVisible = !showInline;
    }

    private void ComputeUnstagedFiles(Action<IReadOnlyList<GitItemStatus>> onComputed, bool doAsync)
    {
        // Avalonia controls enforce thread affinity, so snapshot the original menu state before AsyncLoader switches threads.
        bool excludeIgnoredFiles = !Unstaged.tsmiShowIgnoredFiles.IsChecked;
        bool excludeAssumeUnchangedFiles = !Unstaged.tsmiShowAssumeUnchangedFiles.IsChecked;
        bool excludeSkipWorktreeFiles = !Unstaged.tsmiShowSkipWorktreeFiles.IsChecked;
        UntrackedFilesMode untrackedFilesMode = Unstaged.tsmiShowUntrackedFiles.IsChecked ? UntrackedFilesMode.Default : UntrackedFilesMode.No;

        IReadOnlyList<GitItemStatus> GetAllChangedFilesWithSubmodulesStatus(CancellationToken cancellationToken)
        {
            return Module.GetAllChangedFilesWithSubmodulesStatus(
                excludeIgnoredFiles,
                excludeAssumeUnchangedFiles,
                excludeSkipWorktreeFiles,
                untrackedFilesMode,
                cancellationToken);
        }

        if (doAsync)
        {
            _unstagedTask = _unstagedLoader.LoadAsync(GetAllChangedFilesWithSubmodulesStatus, onComputed);
#pragma warning disable VSTHRD003 // The hot task is started on this context immediately above and retained so close can join its cancellation.
            ThreadHelper.FileAndForget(() => _unstagedTask);
#pragma warning restore VSTHRD003
        }
        else
        {
            _unstagedLoader.Cancel();
            onComputed(GetAllChangedFilesWithSubmodulesStatus(CancellationToken.None));
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
        }, doAsync: false);
    }

    private void EnableStageButtons(bool enable)
    {
        _indexOperationInProgress = !enable;
        UpdateStageButtons();
    }

    private void Initialize(bool loadUnstaged = true)
    {
        _initialized = true;
        TrackLifecycleTask(UpdateBranchNameDisplayAsync());

        if (loadUnstaged)
        {
            Loading.IsVisible = true;
            Loading.IsAnimating = true;
            LoadingStaged.IsVisible = true;
            Commit.IsEnabled = false;
            CommitAndPush.IsEnabled = false;
            btnResetAllChanges.IsEnabled = false;
            btnResetUnstagedChanges.IsEnabled = false;
            EnableStageButtons(enable: false);
            ComputeUnstagedFiles(LoadUnstagedOutput, doAsync: true);
        }

        _isMergeCommit = !Module.RevParse("MERGE_HEAD").IsZero;
        Message.TextBoxFont = AppSettings.CommitFont;
    }

    private void InitializedStaged()
    {
        SolveMergeconflicts.IsVisible = Module.InTheMiddleOfConflictedMerge();
        UpdateButtonStates();
    }

    /// <summary>
    /// Loads the unstaged output.
    /// This method is passed in to the SetTextCallBack delegate
    /// to set the Text property of textBox1.
    /// </summary>
    private void LoadUnstagedOutput(IReadOnlyList<GitItemStatus> allChangedFiles)
    {
        IReadOnlyList<GitItemStatus> lastSelection = _currentSelection ?? [];
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

        (GitRevision? headRev, GitRevision indexRev, GitRevision workTreeRev) = GetHeadRevisions();
        _changingSelection = true;
        Unstaged.SetDiffs(indexRev, workTreeRev, unstagedFiles);
        Staged.SetDiffs(headRev, indexRev, stagedFiles);
        _changingSelection = false;

        Loading.IsVisible = false;
        Loading.IsAnimating = false;
        LoadingStaged.IsVisible = false;
        Commit.IsEnabled = true;
        CommitAndPush.IsEnabled = true;
        EnableStageButtons(enable: true);
        commitStagedCount.Text = $"{stagedFiles.Count}/{stagedFiles.Count + unstagedFiles.Count}";
        InitializedStaged();

        if (Staged.IsEmpty)
        {
            _currentFilesList = Unstaged;
        }
        else if (Unstaged.IsEmpty)
        {
            _currentFilesList = Staged;
        }

        RestoreSelectedFiles(unstagedFiles, stagedFiles, lastSelection);
        if (_currentFilesList == Staged && Staged.SelectedFileStatusItem is FileStatusItem stagedItem)
        {
            _currentSelection = Staged.SelectedGitItems;
            ShowChanges(stagedItem, staged: true);
        }
        else if (Unstaged.SelectedFileStatusItem is FileStatusItem unstagedItem)
        {
            _currentFilesList = Unstaged;
            _currentSelection = Unstaged.SelectedGitItems;
            ShowChanges(unstagedItem, staged: false);
        }

        OnStageAreaLoaded?.Invoke();

        if (_loadUnstagedOutputFirstTime)
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

            _loadUnstagedOutputFirstTime = false;
        }
    }

    private void RestoreSelectedFiles(
        IReadOnlyList<GitItemStatus> unstagedFiles,
        IReadOnlyList<GitItemStatus> stagedFiles,
        IReadOnlyList<GitItemStatus>? lastSelection)
    {
        if (_currentFilesList.IsEmpty)
        {
            SelectStoredNextIndex();
            return;
        }

        Validates.NotNull(lastSelection);
        IReadOnlyList<GitItemStatus> newItems = _currentFilesList == Staged ? stagedFiles : unstagedFiles;
        HashSet<string> names = [.. lastSelection.Select(item => item.Name)];
        List<GitItemStatus> newSelection = [.. newItems.Where(item => names.Contains(item.Name))];
        if (newSelection.Count != 0)
        {
            _currentFilesList.SelectedGitItems = newSelection;
        }
        else
        {
            SelectStoredNextIndex();
        }

        void SelectStoredNextIndex()
        {
            Unstaged.SelectStoredNextItem(orSelectFirst: true);
            Staged.SelectStoredNextItem(orSelectFirst: !Unstaged.GitItemStatuses.Any());
        }
    }

    private (GitRevision? headRev, GitRevision indexRev, GitRevision workTreeRev) GetHeadRevisions()
    {
        ObjectId headId = Module.GetCurrentCheckout();
        GitRevision? headRevision = headId.IsZero ? null : new GitRevision(headId);
        GitRevision indexRevision = headId.IsZero
            ? new GitRevision(ObjectId.IndexId)
            : new GitRevision(ObjectId.IndexId) { ParentIds = [headId] };
        GitRevision workTreeRevision = new(ObjectId.WorkTreeId) { ParentIds = [ObjectId.IndexId] };
        return (headRevision, indexRevision, workTreeRevision);
    }

    public override bool ProcessHotkey(WinFormsShims.Keys keyData)
    {
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

    private void FileViewer_TopScrollReached(object? sender, EventArgs e)
    {
        FileStatusList fileStatus = _currentItemStaged ? Staged : Unstaged;
        fileStatus.SelectPreviousVisibleItem();
        SelectedDiff.ScrollToBottom();
    }

    private void FileViewer_BottomScrollReached(object? sender, EventArgs e)
    {
        FileStatusList fileStatus = _currentItemStaged ? Staged : Unstaged;
        fileStatus.SelectNextVisibleItem();
        SelectedDiff.ScrollToTop();
    }

    private void MoveSelection(bool backwards)
    {
        if (Message.IsKeyboardFocusWithin)
        {
            _currentFilesList = Staged;
        }

        _currentFilesList.SelectNextItem(backwards, loop: true);
    }

    public static readonly string HotkeySettingsName = "Commit";

    internal enum Command
    {
        /* obsolete: AddToGitIgnore = 0, */
        /* obsolete: DeleteSelectedFiles = 1, */
        /* obsolete: ResetSelectedFiles = 6, */
        /* obsolete: StageSelectedFile = 7, */
        /* obsolete: UnStageSelectedFile = 8, */
        /* obsolete: ShowHistory = 9, */
        /* obsolete: OpenFile = 13, */
        /* obsolete: OpenFileWith = 14, */
        /* obsolete: EditFile = 15, */
        FocusUnstagedFiles = 2,
        FocusSelectedDiff = 3,
        FocusStagedFiles = 4,
        FocusCommitMessage = 5,
        ToggleSelectionFilter = 10,
        StageAll = 11,
        OpenWithDifftool = 12,
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
        if (!SelectedDiff.IsKeyboardFocusWithin)
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

    private bool ToggleSelectionFilter()
    {
        bool visible = !toolbarSelectionFilter.IsVisible;
        SetVisibilityOfSelectionFilter(visible);
        if (visible)
        {
            selectionFilter.Focus();
        }
        else if (selectionFilter.IsKeyboardFocusWithin)
        {
            Unstaged.Focus();
        }

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
            case Command.CreateBranch: createBranchToolStripButton_Click(this, EventArgs.Empty); return true;
            case Command.Refresh: ReloadChanges(); return true;
            case Command.SelectNext:
            case Command.SelectNext_AlternativeHotkey1:
            case Command.SelectNext_AlternativeHotkey2: MoveSelection(backwards: false); return true;
            case Command.SelectPrevious:
            case Command.SelectPrevious_AlternativeHotkey1:
            case Command.SelectPrevious_AlternativeHotkey2: MoveSelection(backwards: true); return true;
            default: return base.ExecuteCommand(cmd);
        }
    }

    public override IScriptOptionsProvider GetScriptOptionsProvider()
    {
        return new ScriptOptionsProvider(
            _currentFilesList,
            () => SelectedDiff.CurrentFileLine,
            () => SelectedDiff.CurrentFileColumn);
    }

    private async Task UpdateBranchNameDisplayAsync()
    {
        string currentBranchName = Module.GetSelectedBranch();
        IGitRef? currentBranch = Module.GetRefs(RefsFilter.Heads).FirstOrDefault(branch => branch.LocalName == currentBranchName);
        string pushTo;
        if (currentBranch is null)
        {
            pushTo = string.Empty;
        }
        else if (string.IsNullOrEmpty(currentBranch.TrackingRemote))
        {
            string? defaultRemote = Module.GetRemoteNames().FirstOrDefault(remote => remote == "origin")
                ?? Module.GetRemoteNames().OrderBy(remote => remote).FirstOrDefault();
            pushTo = defaultRemote is null
                ? _statusBarBranchWithoutRemote.Text
                : $"{defaultRemote}/{currentBranchName} {_untrackedRemote.Text}";
        }
        else
        {
            pushTo = $"{currentBranch.TrackingRemote}/{currentBranch.MergeWith}";
        }

        await this.SwitchToMainThreadAsync();
        branchNameLabel.Text = string.IsNullOrEmpty(pushTo) ? currentBranchName : $"{currentBranchName} {char.ConvertFromUtf32(0x2192)}";
        remoteNameLabel.Content = pushTo;
        Title = string.Format(_formTitle.Text, currentBranchName, PathUtil.GetDisplayPath(Module.WorkingDir));
    }

    private static bool CanStage(GitItemStatus item)
        => !item.IsAssumeUnchanged && !item.IsSkipWorktree;

    private void StageSelected()
        => Stage([.. Unstaged.SelectedGitItems.Where(CanStage)]);

    private void UnstageSelected()
        => Unstage();

    private void ShowChanges(FileStatusItem? item, bool staged)
    {
        _currentItem = item;
        _currentItemStaged = staged;
        if (item is null)
        {
            SelectedDiff.ViewPatch(string.Empty);
            return;
        }

        Task viewTask = SelectedDiff.ViewChangesAsync(
            item,
            OpenWithDiffTool,
            _viewChangesSequence.Next());
        TrackViewTask(viewTask);
#pragma warning disable VSTHRD003 // The hot task starts on this context immediately above and is retained for close-time cancellation.
        SelectedDiff.InvokeAndForget(() => viewTask);
#pragma warning restore VSTHRD003
    }

    private void TrackViewTask(Task task)
    {
        _viewTasks.RemoveAll(viewTask => viewTask.IsCompleted);
        _viewTasks.Add(task);
    }

    private async Task CompleteViewTasksAsync()
    {
        try
        {
            await Task.WhenAll(_viewTasks);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void TrackLifecycleTask(Task task)
    {
        _lifecycleTasks.RemoveAll(lifecycleTask => lifecycleTask.IsCompleted);
        _lifecycleTasks.Add(task);
#pragma warning disable VSTHRD003 // The hot task starts on this context at each call site and is retained for close-time joining.
        ThreadHelper.FileAndForget(() => task);
#pragma warning restore VSTHRD003
    }

    private async Task CompleteLifecycleTasksAsync()
    {
        try
        {
            await Task.WhenAll(_lifecycleTasks);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void CommitClick(object? sender, EventArgs e)
    {
        ExecuteCommitCommand();
    }

    private void ExecuteCommitCommand()
    {
        CheckForStagedAndCommit(push: false);
    }

    private void CheckForStagedAndCommit(bool push)
    {
        this.InvokeAndForget(() => CheckForStagedAndCommitAsync(push));
    }

    private void RunIndexOperation(IReadOnlyList<GitItemStatus> items, bool stage)
    {
        if (_indexOperationInProgress || items.Count == 0 || Module.IsBareRepository())
        {
            return;
        }

        _indexOperationInProgress = true;
        UpdateStageButtons();

        CancellationToken cancellationToken = _interactiveAddSequence.Next();
        IGitModule module = Module;
        ThreadHelper.FileAndForget(async () =>
        {
            bool success;
            string output;
            try
            {
                success = stage
                    ? module.StageFiles(items, out output)
                    : module.UnstageFiles(items, out output);
            }
            catch (Exception ex)
            {
                success = false;
                output = ex.Message;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            if (!success && AppSettings.ShowErrorsWhenStagingFiles)
            {
                FormStatus.ShowErrorDialog(
                    this,
                    UICommands,
                    _stageDetails.Text,
                    stage ? string.Format(_stageFiles.Text + "\n", items.Count) : string.Empty,
                    output);
            }

            if (success && AppSettings.RevisionGraphShowArtificialCommits)
            {
                try
                {
                    _suppressRepositoryChangeReload = true;
                    UICommands.RepoChangedNotifier.Notify();
                }
                finally
                {
                    _suppressRepositoryChangeReload = false;
                }
            }

            ReloadChanges(preferStaged: stage);
        });
    }

    private void UnstageFilesClick(object? sender, EventArgs e) => UnstageSelected();

    private void Staged_DoubleClick(object? sender, EventArgs e) => UnstageSelected();

    private void toolUnstageAllItem_Click(object? sender, EventArgs e)
    {
        UnstageAllFiles();
    }

    private void UnstageAllFiles()
    {
        IReadOnlyList<GitItemStatus> lastSelection = _currentSelection ?? [];
        OnStageAreaLoaded += StageAreaLoaded;

        if (_isMergeCommit)
        {
            UnstageItems(Staged.GitItemStatuses);
        }
        else if (Staged.IsFilterActive)
        {
            UnstageItems(Staged.GitItemFilteredStatuses);
            Staged.SetFilter(string.Empty);
        }
        else
        {
            Module.Reset(ResetMode.Mixed);
            ReloadChanges();
        }

        void StageAreaLoaded()
        {
            _currentFilesList = Unstaged;
            RestoreSelectedFiles(Unstaged.GitItemStatuses, Staged.GitItemStatuses, lastSelection);
            Unstaged.Focus();
            OnStageAreaLoaded -= StageAreaLoaded;
        }
    }

    private void UnstagedSelectionChanged(object? sender, EventArgs e)
    {
        if (_changingSelection || Unstaged.SelectedFileStatusItem is not FileStatusItem item)
        {
            return;
        }

        _currentFilesList = Unstaged;
        _changingSelection = true;
        Staged.ClearSelected();
        _changingSelection = false;
        UpdateStageButtons();
        ShowChanges(item, staged: false);
    }

    private void UpdateStageButtons()
    {
        bool actionsEnabled = !_indexOperationInProgress && !_commitInProgress;
        toolStageItem.IsEnabled = actionsEnabled && Unstaged.SelectedGitItems.Any(CanStage);
        toolStageAllItem.IsEnabled = actionsEnabled && Unstaged.GitItemFilteredStatuses.Any(CanStage);
        toolUnstageItem.IsEnabled = actionsEnabled && Staged.SelectedGitItems.Count > 0;
        toolUnstageAllItem.IsEnabled = actionsEnabled && Staged.GitItemFilteredStatuses.Count > 0;
        btnResetUnstagedChanges.IsEnabled = actionsEnabled && Unstaged.GitItemStatuses.Count > 0;
        btnResetAllChanges.IsEnabled = actionsEnabled && (Unstaged.GitItemStatuses.Count > 0 || Staged.GitItemStatuses.Count > 0);

        Commit.IsEnabled = actionsEnabled;
        bool hasChanges = Unstaged.GitItemStatuses.Count > 0 || Staged.GitItemStatuses.Count > 0;
        bool pushOnly = _commitMessageManager is not null && !hasChanges && Amend.IsChecked != true;
        CommitAndPush.IsEnabled = actionsEnabled;
        string commitAndPushText = PushForced
            ? _commitAndForcePush.Text
            : pushOnly ? TranslatedStrings.ButtonPush : _commitAndPush.Text;
        CommitAndPush.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(commitAndPushText);
    }

    private void UpdateButtonStates()
    {
        btnResetAllChanges.IsEnabled = Unstaged.AllItems.Any() || Staged.AllItems.Any();
        bool pushOnly = !btnResetAllChanges.IsEnabled && Amend.IsChecked != true;
        string text = PushForced
            ? _commitAndForcePush.Text
            : pushOnly ? TranslatedStrings.ButtonPush : _commitAndPush.Text;
        CommitAndPush.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(text);
    }

    private void Unstaged_Enter(object? sender, EnterEventArgs e)
    {
        _currentFilesList = Unstaged;
        _changingSelection = false;
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

    private void Unstaged_FilterChanged(object? sender, EventArgs e)
    {
        ToolTip.SetTip(toolStageAllItem, Unstaged.IsFilterActive ? _stageFiltered.Text : _stageAll.Text);
        UpdateStageButtons();
    }

    private void Staged_FilterChanged(object? sender, EventArgs e)
    {
        ToolTip.SetTip(toolUnstageAllItem, Staged.IsFilterActive ? _unstageFiltered.Text : _unstageAll.Text);
        UpdateStageButtons();
    }

    private void Unstage(bool canUseUnstageAll = true)
    {
        if (Module.IsBareRepository())
        {
            return;
        }

        IReadOnlyList<GitItemStatus> items = Staged.SelectedGitItems;
        if (canUseUnstageAll && items.Count > 10 && items.Count == Staged.GitItemStatuses.Count)
        {
            UnstageAllFiles();
            return;
        }

        UnstageItems(items);
    }

    private void UnstageItems(IReadOnlyList<GitItemStatus> items)
        => RunIndexOperation(items, stage: false);

    private void StageClick(object? sender, EventArgs e) => StageSelected();

    private void Unstaged_DoubleClick(object? sender, EventArgs e) => StageSelected();

    private void StageAllAccordingToFilter()
    {
        Stage([.. Unstaged.GitItemFilteredStatuses.Where(CanStage)]);
        Unstaged.SetFilter(string.Empty);
    }

    private void toolStageAllItem_Click(object? sender, EventArgs e)
    {
        StageAllAccordingToFilter();
    }

    private void StagedSelectionChanged(object? sender, EventArgs e)
    {
        if (_changingSelection || Staged.SelectedFileStatusItem is not FileStatusItem item)
        {
            return;
        }

        _currentFilesList = Staged;
        _changingSelection = true;
        Unstaged.ClearSelected();
        _changingSelection = false;
        UpdateStageButtons();
        ShowChanges(item, staged: true);
    }

    private void Staged_DataSourceChanged(object? sender, EventArgs e)
    {
        int stagedCount = Staged.UnfilteredItemsCount;
        int totalFilesCount = stagedCount + Unstaged.UnfilteredItemsCount;
        commitStagedCount.Text = stagedCount + "/" + totalFilesCount;
    }

    private void ApplySelectionFilter(string filterText)
    {
        int matchCount = 0;
        try
        {
            matchCount = Unstaged.SetSelectionFilter(filterText);
            selectionFilter.Classes.Set("file-filter-invalid", false);
            ToolTip.SetTip(selectionFilter, _selectionFilterToolTip.Text);
        }
        catch (ArgumentException exception)
        {
            selectionFilter.Classes.Set("file-filter-invalid", true);
            ToolTip.SetTip(selectionFilter, string.Format(_selectionFilterErrorToolTip.Text, exception.Message));
        }

        if (matchCount > 0 && filterText.Length > 0 && !_selectionFilterHistory.Contains(filterText))
        {
            const int SelectionFilterMaxLength = 10;
            while (_selectionFilterHistory.Count >= SelectionFilterMaxLength)
            {
                _selectionFilterHistory.RemoveAt(SelectionFilterMaxLength - 1);
            }

            _selectionFilterHistory.Insert(0, filterText);
        }
    }

    private void Staged_Enter(object? sender, EnterEventArgs e)
    {
        SelectStaged();
    }

    private void SelectStaged()
    {
        _currentFilesList = Staged;
        _changingSelection = false;
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
        => RunIndexOperation(items, stage: true);

    private void ResetSoftClick(object? sender, EventArgs e)
    {
        if (!MessageBoxes.ConfirmSuppressible(this, _amendResetSoft.Text, _amendCommitCaption.Text, AppSettings.DontConfirmAmend, icon: TaskDialogIcon.Warning))
        {
            return;
        }

        try
        {
            Module.GitExecutable.RunCommand(Commands.Reset(ResetMode.Soft, _resetSoftRevision));
            Amend.IsEnabled = false;
            Amend.IsChecked = false;
            Message.Focus();
        }
        finally
        {
            UICommands.RepoChangedNotifier.Notify();
            ReloadChanges(preferStaged: true);
        }
    }

    private void UpdateCursorPosition()
    {
        string text = Message.Text ?? string.Empty;
        int caret = Math.Clamp(Message.CaretIndex, 0, text.Length);
        int line = 1;
        int column = 1;
        for (int index = 0; index < caret; index++)
        {
            if (text[index] == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }
        }

        commitCursorLine.Text = line.ToString();
        commitCursorColumn.Text = column.ToString();
    }

    private void SolveMergeConflictsClick(object? sender, EventArgs e)
    {
        if (UICommands.StartResolveConflictsDialog(this, offerCommit: false))
        {
            ReloadChanges();
        }
    }

    private void generateListOfChangesInSubmodulesChangesToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        IEnumerable<GitItemStatus> stagedFiles = Staged.GitItemStatuses;
        ISubmodulesConfigFile configFile;
        try
        {
            configFile = Module.GetSubmodulesConfigFile();
        }
        catch (GitConfigurationException exception)
        {
            MessageBoxes.ShowGitConfigurationExceptionMessage(this, exception);
            return;
        }

        Dictionary<string, string> modules = stagedFiles
            .Where(item => item.IsSubmodule
                           && Directory.Exists(_fullPathResolver.Resolve(item.Name))
                           && configFile.ConfigSections.FirstOrDefault(section => section.GetValue("path").Trim() == item.Name)?.SubSection is not null)
            .Select(item => item.Name)
            .ToDictionary(localPath =>
            {
                IConfigSection? submodule = configFile.ConfigSections.FirstOrDefault(section => section.GetValue("path").Trim() == localPath);
                Validates.NotNull(submodule?.SubSection);
                return submodule.SubSection.Trim();
            });

        if (modules.Count == 0)
        {
            return;
        }

        StringBuilder message = new();
        message.AppendLine("Submodule" + (modules.Count == 1 ? " " : "s ") + string.Join(", ", modules.Keys) + " updated");
        message.AppendLine();
        foreach ((string path, string name) in modules)
        {
            GitArgumentBuilder arguments = new("diff")
            {
                "--no-ext-diff",
                "--cached",
                "-z",
                "--",
                name.QuoteNE(),
            };
            string diff = Module.GitExecutable.GetOutput(arguments);
            string[] lines = diff.Split(Delimiters.LineFeed, StringSplitOptions.RemoveEmptyEntries);
            const string SubprojectCommit = "Subproject commit ";
            string from = lines.Single(line => line.StartsWith("-" + SubprojectCommit, StringComparison.Ordinal))[(SubprojectCommit.Length + 1)..];
            string to = lines.Single(line => line.StartsWith("+" + SubprojectCommit, StringComparison.Ordinal))[(SubprojectCommit.Length + 1)..];
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            {
                continue;
            }

            message.AppendLine("Submodule " + path + ":");
            GitModule module = new(UICommands.GetRequiredService<IGitExecutorProvider>(), _fullPathResolver.Resolve(name.EnsureTrailingPathSeparator()));
            arguments = new GitArgumentBuilder("log")
            {
                "--pretty=format:\"    %m %h - %s\"",
                "--no-merges",
                $"{from}...{to}".Quote(),
            };
            string log = module.GitExecutable.GetOutput(arguments);
            message.AppendLine(log.Length != 0 ? log : "    * Revision changed to " + to[..7]);
            message.AppendLine();
        }

        ReplaceMessage(message.ToString().TrimEnd());
    }

    private void SelectedDiffExtraDiffArgumentsChanged(object? sender, EventArgs e)
    {
        ShowChanges(_currentItem, _currentItemStaged);
    }

    private void SelectedDiff_PatchApplied(object? sender, EventArgs e)
    {
        if (_currentItemStaged)
        {
            Staged.StoreNextItemToSelect();
        }
        else
        {
            Unstaged.StoreNextItemToSelect();
        }

        ReloadChanges();
    }

    private void RescanChangesToolStripMenuItemClick(object? sender, EventArgs e)
    {
        RescanChanges();
    }

    private void OpenFilesWithDiffTool(IEnumerable<FileStatusItem> items, string? toolName = null)
    {
        foreach (FileStatusItem item in items)
        {
            GitRevision?[] revisions = [item.SecondRevision, item.FirstRevision];
            UICommands.OpenWithDifftool(
                this,
                revisions,
                item.Item.Name,
                item.Item.OldName,
                RevisionDiffKind.DiffAB,
                item.Item.IsTracked,
                customTool: toolName);
        }
    }

    private void OpenWithDiffTool()
    {
        OpenFilesWithDiffTool(_currentItemStaged ? Staged.SelectedItems : Unstaged.SelectedItems);
    }

    private void btnResetAllChanges_Click(object sender, EventArgs e) => ResetChanges(onlyWorkTree: false);

    private void btnResetUnstagedChanges_Click(object sender, EventArgs e) => ResetChanges(onlyWorkTree: true);

    private void ResetChanges(bool onlyWorkTree)
    {
        BypassFormActivatedEventHandler(() => UICommands.StartResetChangesDialog(this, Unstaged.GitItemStatuses, onlyWorkTree));
        Initialize();
    }

    private void StashStagedClick(object? sender, EventArgs e)
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

    private void CommitAndPush_Click(object? sender, EventArgs e)
    {
        if (Equals(CommitAndPush.Content, AvaloniaTranslationUtils.ToAvaloniaMnemonics(TranslatedStrings.ButtonPush)))
        {
            UICommands.StartPushDialog(owner: this, pushOnShow: true, forceWithLease: PushForced, out _);
            return;
        }

        CheckForStagedAndCommit(push: true);
    }

    private void UpdateAuthorInfo()
    {
        string author = toolAuthor.Text ?? string.Empty;
        TrackLifecycleTask(UpdateAuthorInfoAsync());

        async Task UpdateAuthorInfoAsync()
        {
            string userName = Module.GetEffectiveSetting(SettingKeyString.UserName, defaultValue: string.Empty);
            string userEmail = Module.GetEffectiveSetting(SettingKeyString.UserEmail, defaultValue: string.Empty);
            string committer = $"{_commitCommitterInfo.Text} {userName} <{userEmail}>";
            await this.SwitchToMainThreadAsync();
            commitAuthorStatus.Content = string.IsNullOrWhiteSpace(author)
                ? committer
                : $"{committer}  {_commitAuthorInfo.Text} {author}";
        }
    }

    private void Message_ContextMenuPopulating(object? sender, ContextMenu menu)
    {
        if (menu.ItemsSource is not IList<object> items)
        {
            return;
        }

        object? firstSeparator = items.OfType<Separator>().FirstOrDefault();
        int insertAt = firstSeparator is null ? 0 : items.IndexOf(firstSeparator);
        MenuItem wordWrap = new() { Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_wordWrapCommitMessageBody.Text) };
        wordWrap.Click += (_, _) => WordWrapCommitMessageBody();
        items.Insert(insertAt, wordWrap);

        return;

        void WordWrapCommitMessageBody()
        {
            const int DefaultBodyLineLimit = 72;
            int lineLimit = AppSettings.CommitValidationMaxCntCharsPerLine > 0
                ? AppSettings.CommitValidationMaxCntCharsPerLine
                : DefaultBodyLineLimit;

            for (int line = 1; line < Message.LineCount(); line++)
            {
                WordWrapCommitMessageLineIfNecessary(line, lineLimit);
            }
        }
    }

    private void Message_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            ExecuteCommitCommand();
        }
    }

    private async Task CheckForStagedAndCommitAsync(bool push)
    {
        if (_commitInProgress)
        {
            return;
        }

        bool createAmendCommit = Amend.IsChecked == true;
        bool allowEmpty = createAmendCommit;
        bool pushForced = PushForced;

        if (createAmendCommit
            && !MessageBoxes.ConfirmSuppressible(this, _amendCommit.Text, _amendCommitCaption.Text, AppSettings.DontConfirmAmend, icon: TaskDialogIcon.Warning))
        {
            return;
        }

        if (!createAmendCommit && Staged.GitItemStatuses.Count == 0)
        {
            if (_isMergeCommit)
            {
                if (MessageBoxes.Show(this, _noFilesStagedAndConfirmAnEmptyMergeCommit.Text, _noStagedChanges.Text, WinFormsShims.MessageBoxButtons.YesNo, WinFormsShims.MessageBoxIcon.Question) != WinFormsShims.DialogResult.Yes)
                {
                    return;
                }

                allowEmpty = true;
            }
            else
            {
                bool stageAll = false;
                TaskDialogPage page = new()
                {
                    AllowCancel = true,
                    Caption = _noFilesStagedCommitCaption.Text,
                    Icon = TaskDialogIcon.Error,
                    Heading = _noFilesStagedCommitInstructions.Text,
                    SizeToContent = true,
                };
                page.Buttons.Add(TaskDialogButton.Cancel);
                if (Unstaged.GitItemFilteredStatuses.Any(CanStage))
                {
                    string stageText = Unstaged.IsFilterActive
                        ? _noFilesStagedCommitAllFilteredUnstagedOption.Text
                        : _noFilesStagedCommitAllUnstagedOption.Text;
                    TaskDialogCommandLinkButton stageButton = new(stageText);
                    stageButton.Click += (_, _) => stageAll = true;
                    page.Buttons.Add(stageButton);
                }

                page.Buttons.Add(new TaskDialogCommandLinkButton(_noFilesStagedMakeEmptyCommitOption.Text));
                if (TaskDialog.ShowDialog(this, page) == TaskDialogButton.Cancel)
                {
                    return;
                }

                if (stageAll)
                {
                    GitItemStatus[] files = [.. Unstaged.GitItemFilteredStatuses.Where(CanStage)];
                    (bool success, string output) = await Task.Run(() =>
                    {
                        bool staged = Module.StageFiles(files, out string stageOutput);
                        return (staged, stageOutput);
                    });
                    await this.SwitchToMainThreadAsync();
                    if (!success)
                    {
                        FormStatus.ShowErrorDialog(this, UICommands, Text ?? string.Empty, output);
                        ReloadChanges();
                        return;
                    }
                }
                else
                {
                    allowEmpty = true;
                }
            }
        }

        if (Module.InTheMiddleOfConflictedMerge())
        {
            MessageBoxes.Show(this, _mergeConflicts.Text, _mergeConflictsCaption.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        string message = Message.Text ?? string.Empty;
        if (AppSettings.UseFormCommitMessage && (string.IsNullOrWhiteSpace(message) || message == _commitTemplate))
        {
            MessageBoxes.Show(this, _enterCommitMessage.Text, _enterCommitMessageCaption.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Asterisk);
            return;
        }

        if (AppSettings.UseFormCommitMessage && !IsCommitMessageValid(message))
        {
            return;
        }

        if (!ConfirmDetachedHead())
        {
            return;
        }

        _commitInProgress = true;
        UpdateStageButtons();
        CancellationToken cancellationToken = _commitSequence.Next();
        try
        {
            if (AppSettings.UseFormCommitMessage)
            {
                AppSettings.LastCommitMessage = message;
                await _commitMessageManager.WriteCommitMessageToFileAsync(
                    message,
                    CommitMessageType.Normal,
                    usingCommitTemplate: !string.IsNullOrEmpty(_commitTemplate),
                    ensureCommitMessageSecondLineEmpty: AppSettings.EnsureCommitMessageSecondLineEmpty,
                    cancellationToken);
            }

            bool success = ScriptsRunner.RunEventScripts(ScriptEvent.BeforeCommit, this);
            if (!success)
            {
                return;
            }

            ArgumentString commitArguments = CreateCommitArguments(createAmendCommit, allowEmpty);
            success = FormProcess.ShowDialog(
                this,
                UICommands,
                arguments: commitArguments,
                Module.WorkingDir,
                input: null,
                useDialogSettings: true);

            UICommands.RepoChangedNotifier.Notify();
            if (!success)
            {
                return;
            }

            ScriptsRunner.RunEventScripts(ScriptEvent.AfterCommit, this);

            await _commitMessageManager.ResetCommitMessageAsync();
            _commitTemplate = null;
            CommitKind = CommitKind.Normal;
            _assigningInitialMessage = true;
            Message.Text = string.Empty;
            _assigningInitialMessage = false;
            _messageConsumed = true;
            Amend.IsEnabled = true;
            Amend.IsChecked = false;
            noVerifyToolStripMenuItem.IsChecked = false;

            bool pushCompleted = true;
            if (push)
            {
                UICommands.StartPushDialog(this, pushOnShow: true, forceWithLease: pushForced, out pushCompleted);
            }

            if (pushCompleted
                && Module.SuperprojectModule is not null
                && StageInSuperproject.IsChecked == true
                && !string.IsNullOrWhiteSpace(Module.SubmodulePath))
            {
                Module.SuperprojectModule.StageFile(Module.SubmodulePath);
            }

            if (AppSettings.CloseCommitDialogAfterCommit)
            {
                DialogResult = WinFormsShims.DialogResult.OK;
                Close();
                return;
            }

            ReloadChanges();
            if (AppSettings.CloseCommitDialogAfterLastCommit
                && Module.GetAllChangedFilesWithSubmodulesStatus(CancellationToken.None).Count == 0)
            {
                DialogResult = WinFormsShims.DialogResult.OK;
                Close();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            MessageBoxes.Show(this, $"Exception: {ex.Message}", TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
        }
        finally
        {
            _commitInProgress = false;
            if (IsVisible)
            {
                UpdateStageButtons();
            }
        }
    }

    private ArgumentString CreateCommitArguments(bool amend, bool allowEmpty)
    {
        bool? gpgSign = gpgSignCommitToolStripComboBox.SelectedIndex switch
        {
            0 => null,
            1 => false,
            _ => true,
        };
        string gpgKey = gpgSignCommitToolStripComboBox.SelectedIndex == 3
            ? toolStripGpgKeyTextBox.Text ?? string.Empty
            : string.Empty;

        return Commands.Commit(
            amend,
            signOffToolStripMenuItem.IsChecked == true,
            toolAuthor.Text ?? string.Empty,
            AppSettings.UseFormCommitMessage,
            _commitMessageManager.CommitMessagePath,
            Module.GetPathForGitExecution,
            noVerifyToolStripMenuItem.IsChecked == true,
            gpgSign,
            gpgKey,
            allowEmpty,
            amend && ResetAuthor.IsChecked == true);
    }

    private bool ConfirmDetachedHead()
    {
        if (AppSettings.DontConfirmCommitIfNoBranch || !Module.IsDetachedHead() || Module.InTheMiddleOfRebase())
        {
            return true;
        }

        TaskDialogPage page = new()
        {
            Text = _notOnBranch.Text,
            Heading = TranslatedStrings.ErrorInstructionNotOnBranch,
            Caption = TranslatedStrings.ErrorCaptionNotOnBranch,
            Icon = TaskDialogIcon.Error,
            AllowCancel = true,
            SizeToContent = true,
        };
        page.Buttons.Add(TaskDialogButton.Cancel);
        TaskDialogCommandLinkButton checkout = new(TranslatedStrings.ButtonCheckoutBranch);
        TaskDialogCommandLinkButton create = new(TranslatedStrings.ButtonCreateBranch);
        TaskDialogCommandLinkButton continueButton = new(TranslatedStrings.ButtonContinue);
        page.Buttons.Add(checkout);
        page.Buttons.Add(create);
        page.Buttons.Add(continueButton);

        TaskDialogButton result = TaskDialog.ShowDialog(this, page);
        if (result == TaskDialogButton.Cancel)
        {
            return false;
        }

        if (result == checkout)
        {
            ObjectId[]? objectIds = _editedCommit is null ? null : [_editedCommit.ObjectId];
            return UICommands.StartCheckoutBranch(this, objectIds);
        }

        return result != create || UICommands.StartCreateBranchDialog(this, _editedCommit?.ObjectId ?? default);
    }

    private bool IsCommitMessageValid(string message)
    {
        if (AppSettings.CommitValidationMaxCntCharsFirstLine > 0)
        {
            string firstLine = message.Split(Delimiters.NewLines, StringSplitOptions.None)[0];
            if (firstLine.Length > AppSettings.CommitValidationMaxCntCharsFirstLine
                && !ConfirmInvalidMessage(_commitMsgFirstLineInvalid.Text))
            {
                return false;
            }
        }

        if (AppSettings.CommitValidationMaxCntCharsPerLine > 0)
        {
            foreach (string line in message.Split(Delimiters.NewLines, StringSplitOptions.None))
            {
                if (line.Length > AppSettings.CommitValidationMaxCntCharsPerLine
                    && !ConfirmInvalidMessage(string.Format(_commitMsgLineInvalid.Text, line)))
                {
                    return false;
                }
            }
        }

        if (AppSettings.CommitValidationSecondLineMustBeEmpty)
        {
            string[] lines = message.Split(Delimiters.NewLines, StringSplitOptions.None);
            if (lines.Length > 2 && lines[1].Length != 0
                && !ConfirmInvalidMessage(_commitMsgSecondLineNotEmpty.Text))
            {
                return false;
            }
        }

        if (!string.IsNullOrEmpty(AppSettings.CommitValidationRegEx)
            && !message.StartsWith(CommitKind.Fixup.GetPrefix(), StringComparison.Ordinal)
            && !message.StartsWith(CommitKind.Squash.GetPrefix(), StringComparison.Ordinal))
        {
            try
            {
                if (!Regex.IsMatch(
                        GetTextToValidate(message),
                        AppSettings.CommitValidationRegEx,
                        RegexOptions.None,
                        TimeSpan.FromSeconds(1))
                    && !ConfirmInvalidMessage(_commitMsgRegExNotMatched.Text))
                {
                    return false;
                }
            }
            catch (Exception exception) when (exception is ArgumentException or RegexMatchTimeoutException)
            {
            }
        }

        return true;

        bool ConfirmInvalidMessage(string text)
            => MessageBoxes.Show(this, text, _commitValidationCaption.Text, WinFormsShims.MessageBoxButtons.YesNo, WinFormsShims.MessageBoxIcon.Asterisk) != WinFormsShims.DialogResult.No;

        static string GetTextToValidate(string text)
        {
            if (!text.StartsWith(CommitKind.Amend.GetPrefix(), StringComparison.Ordinal))
            {
                return text;
            }

            string[] lines = text.Split(Delimiters.NewLines, StringSplitOptions.None);
            return lines.Length > 2 && lines[1].Length == 0
                ? string.Join(Environment.NewLine, lines.AsSpan(2))
                : text;
        }
    }

    private void OpenConventionalCommitMenu(bool insertScope)
    {
        _insertScopeParentheses = insertScope;
        commitTemplatesToolStripMenuItem_DropDownOpening(commitTemplatesToolStripMenuItem, EventArgs.Empty);
        commitTemplatesToolStripMenuItem.Flyout?.ShowAt(commitTemplatesToolStripMenuItem);
    }

    private void Message_TextChanged(object? sender, EventArgs e)
    {
        // Format text, except when doing an undo, because
        // this would itself introduce more steps that
        // need to be undone.
        if (!Message.IsUndoInProgress)
        {
            // always format from 0 to handle pasted text
            FormatAllText(0);
        }

        if (!_assigningInitialMessage)
        {
            _messageEditedByUser = true;
            _messageConsumed = false;
        }

        UpdateStageButtons();
        UpdateCursorPosition();
    }

    private void Message_TextAssigned(object? sender, EventArgs e)
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

        if (_formattedLines.Count > lineCount)
        {
            _formattedLines.RemoveRange(lineCount, _formattedLines.Count - lineCount);
        }

        for (int line = startLine; line < lineCount; line++)
        {
            if (_formattedLines.Count > line
                && _formattedLines[line].Equals(Message.Line(line), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            bool lineChanged = FormatLine(line);
            if (_formattedLines.Count <= line)
            {
                // line not formatted yet
                _formattedLines.Add(Message.Line(line));
            }
            else
            {
                _formattedLines[line] = Message.Line(line);
            }

            if (lineChanged)
            {
                FormatAllText(line);
                return;
            }
        }

        bool FormatLine(int line)
        {
            bool changed = false;
            if (limit1 > 0 && line == 0)
            {
                ColorTextAsNecessary(limit1);
            }

            if (empty2 && line == 1)
            {
                // Ensure next line. Optionally add a bullet.
                Message.EnsureEmptyLine(commitValidationIndentAfterFirstLine, 1);
                if (Message.LineCount() > 2)
                {
                    Message.ChangeTextColor(2, 0, Message.LineLength(2), System.Drawing.SystemColors.WindowText);
                    changed |= FormatLine(2);
                }
            }

            if (limitX > 0 && line >= (empty2 ? 2 : 1))
            {
                if (commitValidationAutoWrap && WordWrapCommitMessageLineIfNecessary(line, limitX))
                {
                    changed = true;
                }

                ColorTextAsNecessary(limitX);
            }

            return changed;

            void ColorTextAsNecessary(int lineLimit)
            {
                int lineLength = Message.LineLength(line);
                int validLength = Math.Min(lineLimit, lineLength);
                if (validLength > 0)
                {
                    Message.ChangeTextColor(line, 0, validLength, System.Drawing.SystemColors.WindowText);
                }

                if (lineLength > lineLimit)
                {
                    Message.ChangeTextColor(line, lineLimit, lineLength - lineLimit, System.Drawing.Color.Red);
                }
            }
        }
    }

    private void Message_SelectionChanged(object? sender, EventArgs e) => UpdateCursorPosition();

    private void CommitMessageToolStripMenuItemDropDownOpening(object? sender, EventArgs e)
    {
        string authorPattern = string.Empty;
        if (ShowOnlyMyMessagesToolStripMenuItem.IsChecked == true)
        {
            string userName = Module.GetEffectiveSetting(SettingKeyString.UserName);
            string userEmail = Module.GetEffectiveSetting(SettingKeyString.UserEmail);
            authorPattern = $"^{Regex.Escape(userName)} <{Regex.Escape(userEmail)}>$";
        }

        int maxCount = AppSettings.CommitDialogNumberOfPreviousMessages;
        List<string> messages = [.. Module.GetPreviousCommitMessages(maxCount, "HEAD", authorPattern)
            .WhereNotNull()
            .Select(message => message.TrimEnd('\n'))
            .Where(message => !string.IsNullOrWhiteSpace(message))];
        string lastMessage = AppSettings.LastCommitMessage;
        if (!string.IsNullOrWhiteSpace(lastMessage) && !messages.Contains(lastMessage))
        {
            if (messages.Count == maxCount && maxCount > 0)
            {
                messages.RemoveAt(maxCount - 1);
            }

            messages.Insert(0, lastMessage);
        }

        MenuFlyout flyout = (MenuFlyout)commitMessageToolStripMenuItem.Flyout!;
        flyout.Items.Clear();
        foreach (string commitMessage in messages)
        {
            string label = commitMessage.Split('\n')[0].ShortenTo(72);
            MenuItem item = new() { Header = label, Tag = commitMessage };
            item.Click += (_, _) => ReplaceMessage((string)item.Tag!);
            flyout.Items.Add(item);
        }

        if (messages.Count > 0)
        {
            flyout.Items.Add(new Separator());
        }

        flyout.Items.Add(generateListOfChangesInSubmodulesChangesToolStripMenuItem);
        flyout.Items.Add(new Separator());
        flyout.Items.Add(ShowOnlyMyMessagesToolStripMenuItem);
    }

    private void commitTemplatesToolStripMenuItem_DropDownOpening(object? sender, EventArgs e)
    {
        int registeredTemplatesCount = _commitTemplateManager.RegisteredTemplates.Count();
        if (!_shouldReloadCommitTemplates && _alreadyLoadedTemplatesCount == registeredTemplatesCount)
        {
            return;
        }

        _shouldReloadCommitTemplates = false;
        _alreadyLoadedTemplatesCount = registeredTemplatesCount;
        MenuFlyout flyout = (MenuFlyout)commitTemplatesToolStripMenuItem.Flyout!;
        flyout.Items.Clear();
        bool addedTemplate = false;
        foreach (CommitTemplateItem template in _commitTemplateManager.RegisteredTemplates.Concat(CommitTemplateItem.LoadFromSettings() ?? []))
        {
            if (string.IsNullOrEmpty(template.Name))
            {
                continue;
            }

            MenuItem item = new() { Header = template.Name };
            item.Click += (_, _) => ReplaceMessage(template.Text, template.IsRegex);
            flyout.Items.Add(item);
            addedTemplate = true;
        }

        if (addedTemplate)
        {
            flyout.Items.Add(new Separator());
        }

        _conventionalCommitItem = new() { Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_conventionalCommit.Text) };
        foreach (string keyword in _headerCommitTypes)
        {
            MenuItem item = new() { Header = keyword };
            item.Click += (_, _) =>
            {
                (string title, int selectionStart) = PrefixOrReplaceKeyword(keyword);
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
            };
            _conventionalCommitItem.Items.Add(item);
        }

        _conventionalCommitItem.Items.Add(new Separator());
        foreach (string footer in _footerKeywords)
        {
            MenuItem item = new() { Header = footer };
            item.Click += (_, _) => AddFooter($"{footer}: ", keepCursorPosition: false);
            _conventionalCommitItem.Items.Add(item);
        }

        MenuItem skipCi = new() { Header = "[skip ci]" };
        skipCi.Click += (_, _) => AddFooter("[skip ci]", keepCursorPosition: true);
        _conventionalCommitItem.Items.Add(skipCi);
        _conventionalCommitItem.Items.Add(new Separator());
        MenuItem documentation = new() { Header = _conventionalCommitDocumentation.Text };
        documentation.Click += (_, _) => OsShellUtil.OpenUrlInDefaultBrowser("https://www.conventionalcommits.org");
        _conventionalCommitItem.Items.Add(documentation);
        flyout.Items.Add(_conventionalCommitItem);

        flyout.Items.Add(new Separator());
        MenuItem settingsItem = new()
        {
            Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_commitMessageSettings.Text),
            Icon = new Image { Width = 16, Height = 16, Source = Properties.Images.Settings },
        };
        settingsItem.Click += (_, _) =>
        {
            using FormCommitTemplateSettings frm = new(UICommands);
            frm.ShowDialog(this);
            _shouldReloadCommitTemplates = true;
        };
        flyout.Items.Add(settingsItem);

        void AddFooter(string messageText, bool keepCursorPosition)
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
        }
    }

    private (string message, int selectionStart) PrefixOrReplaceKeyword(string keyword)
    {
        int currentPosition = Message.SelectionStart;
        string scope = _insertScopeParentheses ? "()" : "";
        int scopePosition = keyword.Length + 1;
        int titlePosition = keyword.Length + (scope.Length / 2) + 2;
        string currentTitle = string.IsNullOrWhiteSpace(Message.Text) ? string.Empty : Message.Line(0);

        foreach (string key in _headerCommitTypes)
        {
            if (!currentTitle.StartsWith(key, StringComparison.Ordinal))
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
                if (nextChar is ':' or '(' or '!')
                {
                    return ReplaceKeyword(_ => titlePosition);
                }
            }
            else if (nextChar is ':' or '!')
            {
                return ($"{keyword}(){currentTitle[key.Length..]}", scopePosition);
            }
            else if (nextChar == '(')
            {
                return ReplaceKeyword(newTitle => 2 + Math.Max(newTitle.IndexOf(':'), newTitle.IndexOf('(')));
            }

            (string message, int selectionStart) ReplaceKeyword(Func<string, int> maxPosition)
            {
                string newTitle = $"{keyword}{currentTitle[key.Length..]}";
                int newMessageLength = Message.Text.Length + newTitle.Length - currentTitle.Length;
                return (newTitle, Math.Min(newMessageLength, Math.Max(maxPosition(newTitle), currentPosition + keyword.Length - key.Length)));
            }
        }

        return ($"{keyword}{scope}: {currentTitle}", _insertScopeParentheses ? scopePosition : titlePosition + currentPosition);
    }

    /// <summary>
    /// replace the Message.Text in an undo-able way.
    /// </summary>
    /// <param name="message">the new message.</param>
    private void ReplaceMessage(string message)
    {
        if (Message.Text != message)
        {
            Message.SelectAll();
            Message.SelectedText = message;
        }
    }

    /// <summary>
    /// replace the Message.Text in an undo-able way.
    /// </summary>
    /// <param name="message">the new message.</param>
    /// <param name="regexEnabled">regex replace is enabled</param>
    private void ReplaceMessage(string message, bool regexEnabled)
    {
        if (regexEnabled)
        {
            try
            {
                foreach (Match match in ReplaceMessageRegex().Matches(message))
                {
                    int groupIndex = int.TryParse(match.Groups["index"].Value, out int parsedIndex) ? parsedIndex : 1;
                    Match branchMatch = Regex.Match(Module.GetSelectedBranch(), match.Groups["pattern"].Value);
                    string replacement = branchMatch.Success && branchMatch.Groups.Count > groupIndex
                        ? branchMatch.Groups[groupIndex].Value
                        : string.Empty;
                    message = message.Replace(match.Value, replacement, StringComparison.Ordinal);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ReplaceMessage with regex replace exception: {ex}");
            }
        }

        ReplaceMessage(message);
    }

    private void toolAuthor_TextChanged(object? sender, EventArgs e)
    {
        bool hasAuthor = !string.IsNullOrEmpty(toolAuthor.Text);
        toolAuthorLabelItem.IsEnabled = hasAuthor;
        toolAuthorLabelItem.IsChecked = hasAuthor;
        UpdateAuthorInfo();
    }

    private void toolAuthorLabelItem_Click(object? sender, EventArgs e)
    {
        toolAuthor.Text = string.Empty;
        toolAuthorLabelItem.IsEnabled = false;
        toolAuthorLabelItem.IsChecked = false;
        UpdateAuthorInfo();
    }

    private void gpgSignCommitChanged(object? sender, EventArgs e)
    {
        toolStripGpgKeyTextBox.IsVisible = gpgSignCommitToolStripComboBox.SelectedIndex == 3;

        // Change the icon for commit button
        Commit.Icon = gpgSignCommitToolStripComboBox.SelectedIndex >= 2
            ? Properties.Images.Key
            : Properties.Images.RepoStateClean;
    }

    private void SetVisibilityOfSelectionFilter(bool visible)
    {
        toolbarSelectionFilter.IsVisible = visible;
    }

    private void OnSelectionFilterTextChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ComboBox.TextProperty)
        {
            return;
        }

        _selectionFilterSubject.OnNext(selectionFilter.Text ?? string.Empty);
    }

    private void OnSelectionFilterIndexChanged(object? sender, EventArgs e)
    {
        if (selectionFilter.SelectedItem is string selected)
        {
            selectionFilter.Text = selected;
        }

        Unstaged.SetSelectionFilter(selectionFilter.Text ?? string.Empty);
    }

    private void Amend_CheckedChanged(object? sender, EventArgs e)
    {
        bool amend = Amend.IsChecked == true;
        AmendPanel.IsVisible = amend;
        if (!amend)
        {
            ResetAuthor.IsChecked = false;
        }
        else if (string.IsNullOrEmpty(Message.Text))
        {
            string previousMessage = Module.GetPreviousCommitMessages(1, "HEAD", string.Empty).FirstOrDefault()?.Trim() ?? string.Empty;
            ReplaceMessage(previousMessage);
        }

        ResetSoft.IsEnabled = amend && !Module.RevParse(_resetSoftRevision).IsZero;
        UpdateStageButtons();
    }

    private void StageInSuperproject_CheckedChanged(object? sender, EventArgs e)
    {
        if (StageInSuperproject.IsVisible)
        {
            AppSettings.StageInSuperprojectAfterCommit = StageInSuperproject.IsChecked == true;
        }
    }

    private void commitCommitter_Click(object? sender, EventArgs e)
    {
        UICommands.StartSettingsDialog(this, SettingsDialog.Pages.GitConfigSettingsPage.GetPageReference());
    }

    private void toolAuthor_Leave(object? sender, EventArgs e) => UpdateAuthorInfo();

    private void createBranchToolStripButton_Click(object? sender, EventArgs e)
    {
        if (UICommands.StartCreateBranchDialog(this))
        {
            TrackLifecycleTask(UpdateBranchNameDisplayAsync());
        }
    }

    private void closeDialogAfterEachCommitToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (_skipUpdate)
        {
            return;
        }

        AppSettings.CloseCommitDialogAfterCommit = closeDialogAfterEachCommitToolStripMenuItem.IsChecked == true;
    }

    private void closeDialogAfterAllFilesCommittedToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (_skipUpdate)
        {
            return;
        }

        AppSettings.CloseCommitDialogAfterLastCommit = closeDialogAfterAllFilesCommittedToolStripMenuItem.IsChecked == true;
    }

    private void refreshDialogOnFormFocusToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (_skipUpdate)
        {
            return;
        }

        AppSettings.RefreshArtificialCommitOnApplicationActivated = refreshDialogOnFormFocusToolStripMenuItem.IsChecked == true;
    }

    private void signOffToolStripMenuItem_Click(object? sender, EventArgs e)
    {
    }

    private void tsmiSelectStagedOnEnterMessage_Click(object? sender, EventArgs e)
    {
        if (_skipUpdate)
        {
            return;
        }

        AppSettings.CommitDialogSelectStagedOnEnterMessage.Value = tsmiSelectStagedOnEnterMessage.IsChecked == true;
    }

    private void Message_Enter(object? sender, EventArgs e)
    {
        if (AppSettings.CommitDialogSelectStagedOnEnterMessage.Value)
        {
            SelectStaged();
        }
    }

    private void ShowOnlyMyMessagesToolStripMenuItem_CheckedChanged(object? sender, EventArgs e)
    {
        AppSettings.CommitDialogShowOnlyMyMessages = ShowOnlyMyMessagesToolStripMenuItem.IsChecked == true;
    }

    private void modifyCommitMessageButton_Click(object? sender, EventArgs e)
    {
        CommitKind = CommitKind.Normal;
        Message.Focus();
    }

    private void UICommands_PostRepositoryChanged(object? sender, GitUIEventArgs e)
    {
        if (!_skipUpdate && !_bypassActivatedEventHandler && !_suppressRepositoryChangeReload)
        {
            RescanChanges();
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

    internal TestAccessor GetTestAccessor() => new(this);

    protected override void OnClosed(EventArgs e)
    {
        if (_subscribedToRepositoryChanges)
        {
            UICommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
        }

        _customDiffToolsSequence.Dispose();
        _viewChangesSequence.CancelCurrent();
        DispatcherPump.Wait(async () =>
        {
            await CompleteViewTasksAsync();
            return true;
        });
        _viewChangesSequence.Dispose();
        _interactiveAddSequence.Dispose();
        _commitSequence.Dispose();
        _unstagedLoader.Dispose();
        _selectionFilterSubscription?.Dispose();
        _selectionFilterSubject.Dispose();
        _splitterManager.SaveSplitters();

        DispatcherPump.Wait(async () =>
        {
            await CompleteLifecycleTasksAsync();
            return true;
        });

        if (!_messageConsumed
            && !_commitInProgress
            && _commitMessageManager is not null
            && CommitKind is CommitKind.Normal or CommitKind.Amend)
        {
            string message = Message.Text ?? string.Empty;
            bool amend = Amend.IsChecked == true;
            _closePersistenceTask = PersistCommitMessageAsync();
#pragma warning disable VSTHRD003 // The persistence task is created immediately above on this context and retained until it completes.
            ThreadHelper.FileAndForget(() => _closePersistenceTask);
#pragma warning restore VSTHRD003

            async Task PersistCommitMessageAsync()
            {
                try
                {
                    await _commitMessageManager.SetMergeOrCommitMessageAsync(message);
                    await _commitMessageManager.SetAmendStateAsync(amend);
                }
                finally
                {
                    _commitMessageManagerOwner.Dispose();
                }
            }
        }
        else
        {
            _commitMessageManagerOwner?.Dispose();
        }

        _closePersistenceTask = Task.WhenAll(_closePersistenceTask, _unstagedTask);

        base.OnClosed(e);
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        commitTemplatesOverflowMenuItem.Header = commitTemplatesToolStripMenuItem.Content;
        createBranchOverflowMenuItem.Header = createBranchToolStripButton.Content;
        ToolTip.SetTip(toolStageAllItem, _stageAll.Text);
        ToolTip.SetTip(toolUnstageAllItem, _unstageAll.Text);
        ToolTip.SetTip(modifyCommitMessageButton, _modifyCommitMessageButtonToolTip.Text);
        ToolTip.SetTip(commitAuthorStatus, _commitCommitterToolTip.Text);
        ToolTip.SetTip(selectionFilter, _selectionFilterToolTip.Text);
        UpdateStageButtons();
    }

    private void Options_DropDownOpening(object? sender, EventArgs e)
    {
        _skipUpdate = true;
        refreshDialogOnFormFocusToolStripMenuItem.IsChecked = AppSettings.RefreshArtificialCommitOnApplicationActivated;
        tsmiSelectStagedOnEnterMessage.IsChecked = AppSettings.CommitDialogSelectStagedOnEnterMessage.Value;
        _skipUpdate = false;
    }

    private bool WordWrapCommitMessageLineIfNecessary(int line, int lineLimit)
    {
        if (Message.LineLength(line) <= lineLimit)
        {
            return false;
        }

        string oldText = Message.Line(line);
        string newText = WordWrapper.WrapSingleLine(oldText, lineLimit);
        if (string.Equals(oldText, newText, StringComparison.Ordinal))
        {
            return false;
        }

        Message.ReplaceLine(line, newText);
        return true;
    }

    internal readonly struct TestAccessor(FormCommit form)
    {
        internal SpellChecker.EditNetSpell Message => form.Message;
        internal MenuFlyout CommitMessageFlyout => (MenuFlyout)form.commitMessageToolStripMenuItem.Flyout!;
        internal MenuFlyout CommitTemplatesFlyout => (MenuFlyout)form.commitTemplatesToolStripMenuItem.Flyout!;
        internal Task ClosePersistenceTask => form._closePersistenceTask;
        internal ComboBox SelectionFilter => form.selectionFilter;
        internal bool SelectionFilterVisible => form.toolbarSelectionFilter.IsVisible;

        internal ArgumentString CreateCommitArguments(bool amend, bool allowEmpty)
            => form.CreateCommitArguments(amend, allowEmpty);

        internal void ApplySelectionFilter() => form.ApplySelectionFilter(form.selectionFilter.Text ?? string.Empty);
        internal bool ExecuteCommand(Command command) => form.ExecuteCommand((int)command);
        internal bool IsCommitMessageValid(string message) => form.IsCommitMessageValid(message);
        internal void PopulateCommitMessageHistory() => form.CommitMessageToolStripMenuItemDropDownOpening(form.commitMessageToolStripMenuItem, EventArgs.Empty);
        internal void PopulateCommitTemplates() => form.commitTemplatesToolStripMenuItem_DropDownOpening(form.commitTemplatesToolStripMenuItem, EventArgs.Empty);
        internal (string message, int selectionStart) PrefixOrReplaceKeyword(string keyword) => form.PrefixOrReplaceKeyword(keyword);
        internal bool IncludeFeatureParentheses { set => form._insertScopeParentheses = value; }

        internal void SetMessageState(string text, int position)
        {
            form.Message.Text = text;
            form.Message.SelectionStart = position;
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
