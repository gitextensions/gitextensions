using Avalonia.Controls;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.Compat;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Reduced twin: the normal whole-file commit flow is functional. Advanced commit modes,
// message helpers, signing options, scripts UI, and push arrive in later increments.
public sealed partial class FormCommit : GitModuleForm
{
    private readonly TranslationString _commitAndPush = new("Commit && &push");
    private readonly TranslationString _enterCommitMessage = new("Please enter commit message");
    private readonly TranslationString _enterCommitMessageCaption = new("Commit message");
    private readonly TranslationString _mergeConflicts = new("There are unresolved merge conflicts, solve merge conflicts before committing.");
    private readonly TranslationString _mergeConflictsCaption = new("Merge conflicts");
    private readonly TranslationString _stageAll = new("Stage all");
    private readonly TranslationString _unstageAll = new("Unstage all");
    private readonly ICommitMessageManager _commitMessageManager = null!;
    private readonly WinFormsShims.Control _commitMessageManagerOwner = null!;
    private readonly CancellationTokenSequence _commitSequence = new();
    private readonly CancellationTokenSequence _indexOperationSequence = new();
    private readonly CancellationTokenSequence _refreshSequence = new();
    private readonly CancellationTokenSequence _viewChangesSequence = new();
    private bool _changingSelection;
    private bool _commitInProgress;
    private bool _indexOperationInProgress;
    private bool _subscribedToRepositoryChanges;

    public FormCommit()
    {
        InitializeComponent();
    }

    public FormCommit(
        IGitUICommands commands,
        CommitKind commitKind = CommitKind.Normal,
        GitRevision? editedCommit = null,
        string? commitMessage = null)
        : base(commands, enablePositionRestore: true)
    {
        InitializeComponent();

        Unstaged.SelectedIndexChanged += Unstaged_SelectedIndexChanged;
        Staged.SelectedIndexChanged += Staged_SelectedIndexChanged;
        Unstaged.DoubleClick += Unstaged_DoubleClick;
        Staged.DoubleClick += Staged_DoubleClick;
        toolStageItem.Click += StageClick;
        toolStageAllItem.Click += toolStageAllItem_Click;
        toolUnstageItem.Click += UnstageFilesClick;
        toolUnstageAllItem.Click += toolUnstageAllItem_Click;
        Commit.Click += CommitClick;
        Message.TextChanged += Message_TextChanged;
        UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
        _subscribedToRepositoryChanges = true;

        _commitMessageManagerOwner = new WinFormsShims.Control();
        _commitMessageManager = new CommitMessageManager(
            _commitMessageManagerOwner,
            Module.WorkingDirGitDir,
            Module.CommitEncoding,
            commitMessage);
        Message.Text = commitMessage ?? string.Empty;
        ReloadChanges();

        InitializeComplete();
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

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            _changingSelection = true;
            Unstaged.SetDiffs(unstaged);
            Staged.SetDiffs(staged);
            commitStagedCount.Text = staged.Length.ToString();
            _indexOperationInProgress = false;
            UpdateStageButtons();
            _changingSelection = false;

            if (preferStaged && Staged.SelectedItem is not null)
            {
                Unstaged.ClearSelected();
                ShowChanges(Staged.SelectedItem, staged: true);
            }
            else if (Unstaged.SelectedItem is not null)
            {
                Staged.ClearSelected();
                ShowChanges(Unstaged.SelectedItem, staged: false);
            }
            else if (Staged.SelectedItem is not null)
            {
                ShowChanges(Staged.SelectedItem, staged: true);
            }
            else
            {
                SelectedDiff.ViewPatch(string.Empty);
            }
        });
    }

    private void Unstaged_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_changingSelection || Unstaged.SelectedItem is not GitItemStatus item)
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
        if (_changingSelection || Staged.SelectedItem is not GitItemStatus item)
        {
            return;
        }

        _changingSelection = true;
        Unstaged.ClearSelected();
        _changingSelection = false;
        UpdateStageButtons();
        ShowChanges(item, staged: true);
    }

    private void StageClick(object? sender, EventArgs e)
    {
        if (Unstaged.SelectedItem is GitItemStatus { IsAssumeUnchanged: false, IsSkipWorktree: false } item)
        {
            Stage([item]);
        }
    }

    private void Unstaged_DoubleClick(object? sender, EventArgs e)
    {
        if (Unstaged.SelectedItem is GitItemStatus item)
        {
            Stage([item]);
        }
    }

    private void toolStageAllItem_Click(object? sender, EventArgs e)
    {
        Stage([.. Unstaged.GitItemStatuses.Where(item => !item.IsAssumeUnchanged && !item.IsSkipWorktree)]);
    }

    private void UnstageFilesClick(object? sender, EventArgs e)
    {
        if (Staged.SelectedItem is GitItemStatus item)
        {
            Unstage([item]);
        }
    }

    private void Staged_DoubleClick(object? sender, EventArgs e)
    {
        UnstageFilesClick(sender, e);
    }

    private void toolUnstageAllItem_Click(object? sender, EventArgs e)
    {
        Unstage(Staged.GitItemStatuses);
    }

    private void Stage(IReadOnlyList<GitItemStatus> items)
    {
        RunIndexOperation(items, stage: true);
    }

    private void Unstage(IReadOnlyList<GitItemStatus> items)
    {
        RunIndexOperation(items, stage: false);
    }

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

    private void UpdateStageButtons()
    {
        bool actionsEnabled = !_indexOperationInProgress && !_commitInProgress;
        bool canStage = actionsEnabled
            && Unstaged.SelectedItem is { IsAssumeUnchanged: false, IsSkipWorktree: false };
        bool canUnstage = actionsEnabled && Staged.SelectedItem is not null;
        toolStageItem.IsEnabled = canStage;
        toolStageAllItem.IsEnabled = actionsEnabled
            && Unstaged.GitItemStatuses.Any(item => !item.IsAssumeUnchanged && !item.IsSkipWorktree);
        toolUnstageItem.IsEnabled = canUnstage;
        toolUnstageAllItem.IsEnabled = actionsEnabled && Staged.GitItemStatuses.Count > 0;
        Commit.IsEnabled = actionsEnabled
            && Staged.GitItemStatuses.Count > 0
            && !string.IsNullOrWhiteSpace(Message.Text);
    }

    private void Message_TextChanged(object? sender, EventArgs e)
    {
        UpdateStageButtons();
    }

    private void CommitClick(object? sender, EventArgs e)
    {
        string message = Message.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(message))
        {
            MessageBoxes.Show(this, _enterCommitMessage.Text, _enterCommitMessageCaption.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Asterisk);
            return;
        }

        if (Module.InTheMiddleOfConflictedMerge())
        {
            MessageBoxes.Show(this, _mergeConflicts.Text, _mergeConflictsCaption.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        if (_commitInProgress || Staged.GitItemStatuses.Count == 0)
        {
            return;
        }

        _commitInProgress = true;
        UpdateStageButtons();

        CancellationToken cancellationToken = _commitSequence.Next();
        ThreadHelper.FileAndForget(async () =>
        {
            await _commitMessageManager.WriteCommitMessageToFileAsync(
                message,
                CommitMessageType.Normal,
                usingCommitTemplate: false,
                ensureCommitMessageSecondLineEmpty: AppSettings.EnsureCommitMessageSecondLineEmpty,
                cancellationToken);

            ArgumentString commitArguments = Commands.Commit(
                amend: false,
                signOff: false,
                author: string.Empty,
                useExplicitCommitMessage: true,
                _commitMessageManager.CommitMessagePath,
                Module.GetPathForGitExecution);

            await this.SwitchToMainThreadAsync(cancellationToken);

            bool success = FormProcess.ShowDialog(
                this,
                UICommands,
                arguments: commitArguments,
                Module.WorkingDir,
                input: null,
                useDialogSettings: true);

            if (!success)
            {
                _commitInProgress = false;
                UpdateStageButtons();
                return;
            }

            AppSettings.LastCommitMessage = message;
            await _commitMessageManager.ResetCommitMessageAsync();
            await this.SwitchToMainThreadAsync(cancellationToken);

            UICommands.RepoChangedNotifier.Notify();
            DialogResult = WinFormsShims.DialogResult.OK;
            if (IsVisible)
            {
                Close();
            }
        });
    }

    private void ShowChanges(GitItemStatus item, bool staged)
    {
        CancellationToken cancellationToken = _viewChangesSequence.Next();
        IGitModule module = Module;
        ObjectId firstId = staged ? module.GetCurrentCheckout() : ObjectId.IndexId;
        ObjectId secondId = staged ? ObjectId.IndexId : ObjectId.WorkTreeId;

        ThreadHelper.FileAndForget(async () =>
        {
            (Patch? patch, string? errorMessage) = await module.GetSingleDiffAsync(
                firstId,
                secondId,
                item.Name,
                item.OldName,
                extraDiffArguments: "",
                module.FilesEncoding,
                cacheResult: true,
                isTracked: item.IsTracked,
                useGitColoring: false,
                GitCommandConfiguration.Default,
                cancellationToken);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            FileStatusList selectedList = staged ? Staged : Unstaged;
            if (ReferenceEquals(selectedList.SelectedItem, item))
            {
                SelectedDiff.ViewPatch(patch?.Text ?? errorMessage);
            }
        });
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
        _commitMessageManagerOwner?.Dispose();
        base.OnClosed(e);
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        ToolTip.SetTip(toolStageAllItem, _stageAll.Text);
        ToolTip.SetTip(toolUnstageAllItem, _unstageAll.Text);
        if (CommitAndPush.Content is TextBlock commitAndPushText)
        {
            commitAndPushText.Text = AvaloniaTranslationUtils.ToAvaloniaMnemonics(_commitAndPush.Text);
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
