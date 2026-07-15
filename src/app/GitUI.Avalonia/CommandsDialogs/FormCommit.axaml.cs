using Avalonia.Controls;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs;

// Reduced twin: changed-file loading, patch preview, and whole-file staging are functional.
// Commit-message editing and commit execution arrive in the following FormCommit increment.
public sealed partial class FormCommit : GitModuleForm
{
    private readonly TranslationString _stageAll = new("Stage all");
    private readonly TranslationString _unstageAll = new("Unstage all");
    private readonly CancellationTokenSequence _indexOperationSequence = new();
    private readonly CancellationTokenSequence _refreshSequence = new();
    private readonly CancellationTokenSequence _viewChangesSequence = new();
    private bool _changingSelection;
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
        UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
        _subscribedToRepositoryChanges = true;

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
        bool canStage = !_indexOperationInProgress
            && Unstaged.SelectedItem is { IsAssumeUnchanged: false, IsSkipWorktree: false };
        bool canUnstage = !_indexOperationInProgress && Staged.SelectedItem is not null;
        toolStageItem.IsEnabled = canStage;
        toolStageAllItem.IsEnabled = !_indexOperationInProgress
            && Unstaged.GitItemStatuses.Any(item => !item.IsAssumeUnchanged && !item.IsSkipWorktree);
        toolUnstageItem.IsEnabled = canUnstage;
        toolUnstageAllItem.IsEnabled = !_indexOperationInProgress && Staged.GitItemStatuses.Count > 0;
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
        base.OnClosed(e);
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        ToolTip.SetTip(toolStageAllItem, _stageAll.Text);
        ToolTip.SetTip(toolUnstageAllItem, _unstageAll.Text);
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
