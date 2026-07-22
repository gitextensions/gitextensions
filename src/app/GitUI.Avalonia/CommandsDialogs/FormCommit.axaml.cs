using System.Diagnostics;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.Compat;
using GitUI.Editor;
using GitUI.HelperDialogs;
using GitUI.ScriptsEngine;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public sealed partial class FormCommit : GitModuleForm
{
    private const string ResetSoftRevision = "HEAD~1";
    private const string FeatCommitType = "feat";

    private static readonly string[] HeaderCommitTypes = ["build", "chore", "ci", "docs", FeatCommitType, "fix", "perf", "refactor", "style", "test"];
    private static readonly string[] FooterKeywords = ["BREAKING CHANGE", "Co-authored-by", "Reviewed-by"];
    private static readonly Regex TemplateReplaceRegex = new(@"\{\{(?<pattern>.*?)\}\}(?:\[(?<index>\d+)\])?", RegexOptions.ExplicitCapture);

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
    private readonly TranslationString _mergeConflicts = new("There are unresolved merge conflicts, solve merge conflicts before committing.");
    private readonly TranslationString _mergeConflictsCaption = new("Merge conflicts");
    private readonly TranslationString _noFilesStagedAndConfirmAnEmptyMergeCommit = new("There are no files staged for this commit.\nAre you sure you want to commit?");
    private readonly TranslationString _noFilesStagedCommitAllFilteredUnstagedOption = new("Stage and commit the unstaged files that match your filter");
    private readonly TranslationString _noFilesStagedCommitAllUnstagedOption = new("Stage and commit all unstaged files");
    private readonly TranslationString _noFilesStagedMakeEmptyCommitOption = new("Make an empty commit");
    private readonly TranslationString _noFilesStagedCommitCaption = new("Confirm commit");
    private readonly TranslationString _noFilesStagedCommitInstructions = new("There aren't any changes in the staging area.\nHow do you want to proceed?");
    private readonly TranslationString _noStagedChanges = new("There are no staged changes");
    private readonly TranslationString _notOnBranch = new(
        "This commit will be unreferenced when switching to another branch and can be lost."
        + Environment.NewLine + Environment.NewLine + "Do you want to continue?");
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
    private readonly TranslationString _conventionalCommit = new("Conven&tional Commits");
    private readonly TranslationString _commitAuthorInfo = new("Author");
    private readonly TranslationString _commitCommitterInfo = new("Committer");
    private readonly TranslationString _commitCommitterToolTip = new("Click to change committer information.");
    private readonly TranslationString _formTitle = new("Commit to {0} ({1})");
    private readonly TranslationString _modifyCommitMessageButtonToolTip = new(
        "If you change the first line of the commit message, git will treat this commit as an ordinary commit,"
        + Environment.NewLine + "i.e. it may no longer be a fixup or an autosquash commit.");
    private readonly TranslationString _templateNotFoundCaption = new("Template Error");
    private readonly TranslationString _templateNotFound = new(
        $"Template not found: {{0}}.{Environment.NewLine}{Environment.NewLine}You can set your template:{Environment.NewLine}\t$ git config commit.template ./.git_commit_msg.txt{Environment.NewLine}You can unset the template:{Environment.NewLine}\t$ git config --unset commit.template");
    private readonly TranslationString _templateLoadErrorCaption = new("Template could not be loaded");
    private readonly TranslationString _statusBarBranchWithoutRemote = new("(remote not configured)");
    private readonly TranslationString _untrackedRemote = new("(untracked)");
    private readonly TranslationString _stageAll = new("Stage all");
    private readonly TranslationString _unstageAll = new("Unstage all");
    private readonly ICommitMessageManager _commitMessageManager = null!;
    private readonly WinFormsShims.Control _commitMessageManagerOwner = null!;
    private readonly ICommitTemplateManager _commitTemplateManager = null!;
    private readonly GitRevision? _editedCommit;
    private readonly CancellationTokenSequence _commitSequence = new();
    private readonly CancellationTokenSequence _indexOperationSequence = new();
    private readonly CancellationTokenSequence _refreshSequence = new();
    private readonly CancellationTokenSequence _viewChangesSequence = new();
    private Task _closePersistenceTask = Task.CompletedTask;
    private CommitKind _commitKind;
    private string? _commitTemplate;
    private bool _changingSelection;
    private bool _commitInProgress;
    private bool _indexOperationInProgress;
    private bool _initialized;
    private bool _isMergeCommit;
    private bool _assigningInitialMessage;
    private bool _messageEditedByUser;
    private bool _messageConsumed;
    private FileStatusItem? _selectedDiffItem;
    private bool _selectedDiffItemStaged;
    private bool _subscribedToRepositoryChanges;
    private bool _updatingCommitOptions;

    public FormCommit()
    {
        InitializeComponent();
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

        Unstaged.SelectionMode = SelectionMode.Multiple;
        Staged.SelectionMode = SelectionMode.Multiple;
        Unstaged.BindContextMenu(() => ReloadChanges(), canAutoRefresh: true, StageSelected, unstage: null);
        Staged.BindContextMenu(() => ReloadChanges(), canAutoRefresh: false, stage: null, UnstageSelected);
        SelectedDiff.LinePatchingBlocksUntilReload = true;
        SelectedDiff.ExtraDiffArgumentsChanged += (_, _) => ShowChanges(_selectedDiffItem, _selectedDiffItemStaged);
        SelectedDiff.PatchApplied += (_, _) => ReloadChanges();
        Unstaged.SelectedIndexChanged += Unstaged_SelectedIndexChanged;
        Staged.SelectedIndexChanged += Staged_SelectedIndexChanged;
        Unstaged.DoubleClick += Unstaged_DoubleClick;
        Staged.DoubleClick += Staged_DoubleClick;
        toolStageItem.Click += StageClick;
        toolStageAllItem.Click += toolStageAllItem_Click;
        toolUnstageItem.Click += UnstageFilesClick;
        toolUnstageAllItem.Click += toolUnstageAllItem_Click;
        btnResetAllChanges.Click += btnResetAllChanges_Click;
        btnResetUnstagedChanges.Click += btnResetUnstagedChanges_Click;
        Commit.Click += CommitClick;
        CommitAndPush.Click += CommitAndPushClick;
        Message.TextChanged += Message_TextChanged;
        Message.KeyDown += Message_KeyDown;
        Message.SelectionChanged += Message_SelectionChanged;
        Amend.IsCheckedChanged += Amend_CheckedChanged;
        ResetSoft.Click += ResetSoftClick;
        StageInSuperproject.IsCheckedChanged += StageInSuperproject_CheckedChanged;
        modifyCommitMessageButton.Click += modifyCommitMessageButton_Click;
        SolveMergeconflicts.Click += SolveMergeConflictsClick;
        createBranchToolStripButton.Click += createBranchToolStripButton_Click;
        toolAuthor.TextChanged += toolAuthor_TextChanged;
        toolAuthor.LostFocus += (_, _) => UpdateAuthorInfo();
        gpgSignCommitToolStripComboBox.SelectionChanged += gpgSignCommitChanged;
        closeDialogAfterEachCommitToolStripMenuItem.IsCheckedChanged += CommitOptionChanged;
        closeDialogAfterAllFilesCommittedToolStripMenuItem.IsCheckedChanged += CommitOptionChanged;
        refreshDialogOnFormFocusToolStripMenuItem.IsCheckedChanged += CommitOptionChanged;
        tsmiSelectStagedOnEnterMessage.IsCheckedChanged += CommitOptionChanged;
        ShowOnlyMyMessagesToolStripMenuItem.Click += ShowOnlyMyMessagesToolStripMenuItem_Click;
        ((MenuFlyout)commitMessageToolStripMenuItem.Flyout!).Opening += (_, _) => PopulateCommitMessageHistory();
        ((MenuFlyout)commitTemplatesToolStripMenuItem.Flyout!).Opening += (_, _) => PopulateCommitTemplates();
        remoteNameLabel.Click += (_, _) => UICommands.StartRemotesDialog(this, preselectLocal: Module.GetSelectedBranch());
        UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
        _subscribedToRepositoryChanges = true;

        _commitMessageManagerOwner = new WinFormsShims.Control();
        _commitMessageManager = new CommitMessageManager(
            _commitMessageManagerOwner,
            Module.WorkingDirGitDir,
            Module.CommitEncoding,
            commitMessage);
        _commitTemplateManager = new CommitTemplateManager(() => Module);

        bool closeAfterCommit = AppSettings.CloseCommitDialogAfterCommit;
        bool closeAfterLastCommit = AppSettings.CloseCommitDialogAfterLastCommit;
        bool refreshOnFocus = AppSettings.RefreshArtificialCommitOnApplicationActivated;
        bool selectStagedOnEnter = AppSettings.CommitDialogSelectStagedOnEnterMessage.Value;
        _updatingCommitOptions = true;
        closeDialogAfterEachCommitToolStripMenuItem.IsChecked = closeAfterCommit;
        closeDialogAfterAllFilesCommittedToolStripMenuItem.IsChecked = closeAfterLastCommit;
        refreshDialogOnFormFocusToolStripMenuItem.IsChecked = refreshOnFocus;
        tsmiSelectStagedOnEnterMessage.IsChecked = selectStagedOnEnter;
        _updatingCommitOptions = false;
        ShowOnlyMyMessagesToolStripMenuItem.IsChecked = AppSettings.CommitDialogShowOnlyMyMessages;
        StageInSuperproject.IsVisible = Module.SuperprojectModule is not null;
        StageInSuperproject.IsChecked = AppSettings.StageInSuperprojectAfterCommit;
        ApplyCommitKind();
        ReloadChanges();

        InitializeComplete();
    }

    private bool PushForced => (Amend.IsChecked == true || !Amend.IsEnabled) && AppSettings.CommitAndPushForcedWhenAmend;

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        if (_initialized || _commitMessageManager is null)
        {
            return;
        }

        _initialized = true;
        ThreadHelper.FileAndForget(InitializeCommitMessageAsync);
        UpdateAuthorInfo();
        ThreadHelper.FileAndForget(UpdateBranchNameDisplayAsync);
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

        if (AppSettings.UseFormCommitMessage && string.IsNullOrEmpty(message))
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
        bool canEditMessage = AppSettings.UseFormCommitMessage && _commitKind is CommitKind.Normal or CommitKind.Amend;
        Message.IsEnabled = canEditMessage;
        commitMessageToolStripMenuItem.IsEnabled = canEditMessage;
        commitTemplatesToolStripMenuItem.IsEnabled = canEditMessage;
        modifyCommitMessageButton.IsVisible = AppSettings.UseFormCommitMessage && _commitKind is CommitKind.Fixup or CommitKind.Squash;
    }

    private void ReloadChanges(bool preferStaged = false)
    {
        CancellationToken cancellationToken = _refreshSequence.Next();
        IGitModule module = Module;
        ThreadHelper.FileAndForget(async () =>
        {
            IReadOnlyList<GitItemStatus> changedFiles = module.GetAllChangedFilesWithSubmodulesStatus(cancellationToken);
            GitItemStatus[] unstaged = [.. changedFiles.Where(item => item.Staged == StagedStatus.WorkTree || item.IsStatusOnly)];
            GitItemStatus[] staged = [.. changedFiles.Where(item => item.Staged == StagedStatus.Index && !item.IsStatusOnly)];
            ObjectId headId = module.GetCurrentCheckout();
            GitRevision? headRevision = headId.IsZero ? null : new GitRevision(headId);
            GitRevision indexRevision = headId.IsZero
                ? new GitRevision(ObjectId.IndexId)
                : new GitRevision(ObjectId.IndexId) { ParentIds = [headId] };
            GitRevision workTreeRevision = new(ObjectId.WorkTreeId) { ParentIds = [ObjectId.IndexId] };
            bool hasConflicts = module.InTheMiddleOfConflictedMerge();
            bool isMergeCommit = !module.RevParse("MERGE_HEAD").IsZero;

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            _changingSelection = true;
            Unstaged.SetDiffs(indexRevision, workTreeRevision, unstaged);
            Staged.SetDiffs(headRevision, indexRevision, staged);
            commitStagedCount.Text = $"{staged.Length}/{staged.Length + unstaged.Length}";
            SolveMergeconflicts.IsVisible = hasConflicts;
            _isMergeCommit = isMergeCommit;
            _indexOperationInProgress = false;
            UpdateStageButtons();
            _changingSelection = false;

            if (preferStaged && Staged.SelectedFileStatusItem is not null)
            {
                Unstaged.ClearSelected();
                ShowChanges(Staged.SelectedFileStatusItem, staged: true);
            }
            else if (Unstaged.SelectedFileStatusItem is not null)
            {
                Staged.ClearSelected();
                ShowChanges(Unstaged.SelectedFileStatusItem, staged: false);
            }
            else if (Staged.SelectedFileStatusItem is not null)
            {
                ShowChanges(Staged.SelectedFileStatusItem, staged: true);
            }
            else
            {
                SelectedDiff.ViewPatch(string.Empty);
            }
        });
    }

    private void Unstaged_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_changingSelection || Unstaged.SelectedFileStatusItem is not FileStatusItem item)
        {
            return;
        }

        _changingSelection = true;
        Staged.ClearSelected();
        _changingSelection = false;
        UpdateStageButtons();
        ShowChanges(item, staged: false);
    }

    private void Staged_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_changingSelection || Staged.SelectedFileStatusItem is not FileStatusItem item)
        {
            return;
        }

        _changingSelection = true;
        Unstaged.ClearSelected();
        _changingSelection = false;
        UpdateStageButtons();
        ShowChanges(item, staged: true);
    }

    private void StageClick(object? sender, EventArgs e) => StageSelected();

    private void Unstaged_DoubleClick(object? sender, EventArgs e) => StageSelected();

    private void toolStageAllItem_Click(object? sender, EventArgs e)
    {
        Stage([.. Unstaged.GitItemFilteredStatuses.Where(CanStage)]);
        Unstaged.SetFilter(string.Empty);
    }

    private void UnstageFilesClick(object? sender, EventArgs e) => UnstageSelected();

    private void Staged_DoubleClick(object? sender, EventArgs e) => UnstageSelected();

    private void toolUnstageAllItem_Click(object? sender, EventArgs e)
    {
        Unstage(Staged.GitItemFilteredStatuses);
        Staged.SetFilter(string.Empty);
    }

    private static bool CanStage(GitItemStatus item)
        => !item.IsAssumeUnchanged && !item.IsSkipWorktree;

    private void StageSelected()
        => Stage([.. Unstaged.SelectedGitItems.Where(CanStage)]);

    private void UnstageSelected()
        => Unstage(Staged.SelectedGitItems);

    private void Stage(IReadOnlyList<GitItemStatus> items)
        => RunIndexOperation(items, stage: true);

    private void Unstage(IReadOnlyList<GitItemStatus> items)
        => RunIndexOperation(items, stage: false);

    private void RunIndexOperation(IReadOnlyList<GitItemStatus> items, bool stage)
    {
        if (_indexOperationInProgress || items.Count == 0 || Module.IsBareRepository())
        {
            return;
        }

        _indexOperationInProgress = true;
        UpdateStageButtons();

        CancellationToken cancellationToken = _indexOperationSequence.Next();
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
            if (!success)
            {
                FormStatus.ShowErrorDialog(this, UICommands, Text ?? string.Empty, output);
            }

            ReloadChanges(preferStaged: stage);
        });
    }

    private void btnResetAllChanges_Click(object sender, EventArgs e) => ResetChanges(onlyWorkTree: false);

    private void btnResetUnstagedChanges_Click(object sender, EventArgs e) => ResetChanges(onlyWorkTree: true);

    private void ResetChanges(bool onlyWorkTree)
    {
        UICommands.StartResetChangesDialog(this, Unstaged.GitItemStatuses, onlyWorkTree);
        ReloadChanges(preferStaged: false);
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

        bool hasMessage = !AppSettings.UseFormCommitMessage || !string.IsNullOrWhiteSpace(Message.Text);
        Commit.IsEnabled = actionsEnabled && hasMessage;
        CommitAndPush.IsEnabled = Commit.IsEnabled;
        CommitAndPush.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(PushForced ? _commitAndForcePush.Text : _commitAndPush.Text);
    }

    private void Message_TextChanged(object? sender, EventArgs e)
    {
        if (!_assigningInitialMessage)
        {
            _messageEditedByUser = true;
            _messageConsumed = false;
        }

        UpdateStageButtons();
        UpdateCursorPosition();
    }

    private void Message_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            this.InvokeAndForget(() => CheckForStagedAndCommitAsync(push: false));
        }
    }

    private void Message_SelectionChanged(object? sender, EventArgs e) => UpdateCursorPosition();

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

    private void CommitClick(object? sender, EventArgs e)
        => this.InvokeAndForget(() => CheckForStagedAndCommitAsync(push: false));

    private void CommitAndPushClick(object? sender, EventArgs e)
        => this.InvokeAndForget(() => CheckForStagedAndCommitAsync(push: true));

    private async Task CheckForStagedAndCommitAsync(bool push)
    {
        if (_commitInProgress)
        {
            return;
        }

        bool createAmendCommit = Amend.IsChecked == true;
        bool allowEmpty = createAmendCommit;
        bool pushForced = PushForced;

        if (createAmendCommit && !AppSettings.DontConfirmAmend
            && MessageBoxes.Show(this, _amendCommit.Text, _amendCommitCaption.Text, WinFormsShims.MessageBoxButtons.YesNo, WinFormsShims.MessageBoxIcon.Warning) != WinFormsShims.DialogResult.Yes)
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
            _commitKind = CommitKind.Normal;
            _assigningInitialMessage = true;
            Message.Text = string.Empty;
            _assigningInitialMessage = false;
            _messageConsumed = true;
            Amend.IsEnabled = true;
            Amend.IsChecked = false;
            noVerifyToolStripMenuItem.IsChecked = false;
            ApplyCommitKind();

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
                if (!Regex.IsMatch(GetTextToValidate(message), AppSettings.CommitValidationRegEx)
                    && !ConfirmInvalidMessage(_commitMsgRegExNotMatched.Text))
                {
                    return false;
                }
            }
            catch (ArgumentException)
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

    private void ShowChanges(FileStatusItem? item, bool staged)
    {
        _selectedDiffItem = item;
        _selectedDiffItemStaged = staged;
        if (item is null)
        {
            SelectedDiff.ViewPatch(string.Empty);
            return;
        }

        SelectedDiff.InvokeAndForget(() => SelectedDiff.ViewChangesAsync(
            item,
            OpenWithDiffTool,
            _viewChangesSequence.Next()));

        void OpenWithDiffTool()
        {
            IEnumerable<FileStatusItem> items = staged ? Staged.SelectedItems : Unstaged.SelectedItems;
            foreach (FileStatusItem selectedItem in items)
            {
                GitRevision?[] revisions = [selectedItem.SecondRevision, selectedItem.FirstRevision];
                UICommands.OpenWithDifftool(
                    this,
                    revisions,
                    selectedItem.Item.Name,
                    selectedItem.Item.OldName,
                    RevisionDiffKind.DiffAB,
                    selectedItem.Item.IsTracked);
            }
        }
    }

    private void PopulateCommitMessageHistory()
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

        flyout.Items.Add(ShowOnlyMyMessagesToolStripMenuItem);
    }

    private void PopulateCommitTemplates()
    {
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

        MenuItem conventional = new() { Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_conventionalCommit.Text) };
        foreach (string keyword in HeaderCommitTypes)
        {
            MenuItem item = new() { Header = keyword };
            item.Click += (_, _) => ApplyConventionalCommitPrefix(keyword);
            conventional.Items.Add(item);
        }

        conventional.Items.Add(new Separator());
        foreach (string footer in FooterKeywords)
        {
            MenuItem item = new() { Header = footer };
            item.Click += (_, _) => AppendCommitFooter($"{footer}: ");
            conventional.Items.Add(item);
        }

        MenuItem skipCi = new() { Header = "[skip ci]" };
        skipCi.Click += (_, _) => AppendCommitFooter("[skip ci]");
        conventional.Items.Add(skipCi);
        flyout.Items.Add(conventional);
    }

    private void ApplyConventionalCommitPrefix(string keyword)
    {
        string message = Message.Text ?? string.Empty;
        string[] lines = message.Split(Delimiters.NewLines, StringSplitOptions.None);
        string title = lines[0];
        string? existingKeyword = HeaderCommitTypes.FirstOrDefault(type =>
            title.StartsWith(type + ":", StringComparison.Ordinal)
            || title.StartsWith(type + "(", StringComparison.Ordinal)
            || title.StartsWith(type + "!", StringComparison.Ordinal));
        lines[0] = existingKeyword is null
            ? $"{keyword}: {title}"
            : keyword + title[existingKeyword.Length..];
        Message.Text = string.Join(Environment.NewLine, lines);
        Message.CaretIndex = Math.Min(Message.Text.Length, keyword.Length + 2);
        Message.Focus();
    }

    private void AppendCommitFooter(string footer)
    {
        string message = (Message.Text ?? string.Empty).TrimEnd();
        Message.Text = string.IsNullOrEmpty(message)
            ? footer
            : $"{message}{Environment.NewLine}{Environment.NewLine}{footer}";
        Message.CaretIndex = Message.Text.Length;
        Message.Focus();
    }

    private void ReplaceMessage(string message, bool regexEnabled = false)
    {
        if (regexEnabled)
        {
            try
            {
                foreach (Match match in TemplateReplaceRegex.Matches(message))
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

        Message.Text = message;
        Message.CaretIndex = message.Length;
        Message.Focus();
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

        ResetSoft.IsEnabled = amend && !Module.RevParse(ResetSoftRevision).IsZero;
        UpdateStageButtons();
    }

    private void ResetSoftClick(object? sender, EventArgs e)
    {
        if (!AppSettings.DontConfirmAmend
            && MessageBoxes.Show(this, _amendResetSoft.Text, _amendCommitCaption.Text, WinFormsShims.MessageBoxButtons.YesNo, WinFormsShims.MessageBoxIcon.Warning) != WinFormsShims.DialogResult.Yes)
        {
            return;
        }

        try
        {
            Module.GitExecutable.RunCommand(Commands.Reset(ResetMode.Soft, ResetSoftRevision));
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

    private void SolveMergeConflictsClick(object? sender, EventArgs e)
    {
        if (UICommands.StartResolveConflictsDialog(this, offerCommit: false))
        {
            ReloadChanges();
        }
    }

    private void createBranchToolStripButton_Click(object? sender, EventArgs e)
    {
        if (UICommands.StartCreateBranchDialog(this))
        {
            ThreadHelper.FileAndForget(UpdateBranchNameDisplayAsync);
        }
    }

    private void modifyCommitMessageButton_Click(object? sender, EventArgs e)
    {
        _commitKind = CommitKind.Normal;
        ApplyCommitKind();
        Message.Focus();
    }

    private void StageInSuperproject_CheckedChanged(object? sender, EventArgs e)
    {
        if (StageInSuperproject.IsVisible)
        {
            AppSettings.StageInSuperprojectAfterCommit = StageInSuperproject.IsChecked == true;
        }
    }

    private void toolAuthor_TextChanged(object? sender, EventArgs e) => UpdateAuthorInfo();

    private void gpgSignCommitChanged(object? sender, EventArgs e)
    {
        toolStripGpgKeyTextBox.IsVisible = gpgSignCommitToolStripComboBox.SelectedIndex == 3;
        Commit.Icon = gpgSignCommitToolStripComboBox.SelectedIndex >= 2
            ? Properties.Images.Key
            : Properties.Images.RepoStateClean;
    }

    private void CommitOptionChanged(object? sender, EventArgs e)
    {
        if (_updatingCommitOptions)
        {
            return;
        }

        AppSettings.CloseCommitDialogAfterCommit = closeDialogAfterEachCommitToolStripMenuItem.IsChecked == true;
        AppSettings.CloseCommitDialogAfterLastCommit = closeDialogAfterAllFilesCommittedToolStripMenuItem.IsChecked == true;
        AppSettings.RefreshArtificialCommitOnApplicationActivated = refreshDialogOnFormFocusToolStripMenuItem.IsChecked == true;
        AppSettings.CommitDialogSelectStagedOnEnterMessage.Value = tsmiSelectStagedOnEnterMessage.IsChecked == true;
    }

    private void ShowOnlyMyMessagesToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AppSettings.CommitDialogShowOnlyMyMessages = ShowOnlyMyMessagesToolStripMenuItem.IsChecked == true;
    }

    private void UpdateAuthorInfo()
    {
        string author = toolAuthor.Text ?? string.Empty;
        ThreadHelper.FileAndForget(async () =>
        {
            string userName = Module.GetEffectiveSetting(SettingKeyString.UserName, defaultValue: string.Empty);
            string userEmail = Module.GetEffectiveSetting(SettingKeyString.UserEmail, defaultValue: string.Empty);
            string committer = $"{_commitCommitterInfo.Text} {userName} <{userEmail}>";
            await this.SwitchToMainThreadAsync();
            commitAuthorStatus.Text = string.IsNullOrWhiteSpace(author)
                ? committer
                : $"{committer}  {_commitAuthorInfo.Text} {author}";
        });
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
        else if (string.IsNullOrEmpty(currentBranch.TrackingRemote) || string.IsNullOrEmpty(currentBranch.MergeWith))
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

    private void UICommands_PostRepositoryChanged(object? sender, GitUIEventArgs e)
    {
        this.InvokeAndForget(() => ReloadChanges());
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_subscribedToRepositoryChanges)
        {
            UICommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
        }

        _refreshSequence.Dispose();
        _viewChangesSequence.Dispose();
        _indexOperationSequence.Dispose();
        _commitSequence.Dispose();

        if (!_messageConsumed
            && _commitMessageManager is not null
            && _commitKind is CommitKind.Normal or CommitKind.Amend)
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

        base.OnClosed(e);
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        ToolTip.SetTip(toolStageAllItem, _stageAll.Text);
        ToolTip.SetTip(toolUnstageAllItem, _unstageAll.Text);
        ToolTip.SetTip(modifyCommitMessageButton, _modifyCommitMessageButtonToolTip.Text);
        ToolTip.SetTip(commitAuthorStatus, _commitCommitterToolTip.Text);
        UpdateStageButtons();
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormCommit form)
    {
        internal SpellChecker.EditNetSpell Message => form.Message;
        internal MenuFlyout CommitMessageFlyout => (MenuFlyout)form.commitMessageToolStripMenuItem.Flyout!;
        internal MenuFlyout CommitTemplatesFlyout => (MenuFlyout)form.commitTemplatesToolStripMenuItem.Flyout!;
        internal Task ClosePersistenceTask => form._closePersistenceTask;

        internal ArgumentString CreateCommitArguments(bool amend, bool allowEmpty)
            => form.CreateCommitArguments(amend, allowEmpty);

        internal bool IsCommitMessageValid(string message) => form.IsCommitMessageValid(message);
        internal void PopulateCommitMessageHistory() => form.PopulateCommitMessageHistory();
        internal void PopulateCommitTemplates() => form.PopulateCommitTemplates();
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
