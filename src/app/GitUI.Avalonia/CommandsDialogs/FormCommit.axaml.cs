using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs;

// Reduced first increment: changed-file loading and patch preview are functional. Staging,
// commit-message editing, and commit execution arrive in the following FormCommit increments.
public sealed partial class FormCommit : GitModuleForm
{
    private readonly CancellationTokenSequence _refreshSequence = new();
    private readonly CancellationTokenSequence _viewChangesSequence = new();
    private bool _changingSelection;
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
        UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
        _subscribedToRepositoryChanges = true;

        Message.Text = commitMessage ?? string.Empty;
        ReloadChanges();

        InitializeComplete();
    }

    private void ReloadChanges()
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
            _changingSelection = false;

            if (Unstaged.SelectedItem is not null)
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
        ShowChanges(item, staged: true);
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
        this.InvokeAndForget(ReloadChanges);
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_subscribedToRepositoryChanges)
        {
            UICommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
        }

        _refreshSequence.Dispose();
        _viewChangesSequence.Dispose();
        base.OnClosed(e);
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
