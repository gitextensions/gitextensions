using GitCommands;
using GitCommands.Settings;

namespace GitExtensions.UITests;

/// <summary>
/// Restores <see cref="AppSettings"/> changed for CommitInfo / FormBrowse integration tests.
/// </summary>
internal sealed class CommitInfoPanelTestSettingsScope : IDisposable
{
    private readonly bool _commitInfoShowContainedInBranchesLocal;
    private readonly bool _commitInfoShowContainedInBranchesRemote;
    private readonly bool _commitInfoShowContainedInBranchesRemoteIfNoLocal;
    private readonly bool _commitInfoShowContainedInTags;
    private readonly bool _commitInfoShowTagThisCommitDerivesFrom;
    private readonly bool _showAnnotatedTagsMessages;
    private readonly CommitInfoPosition _commitInfoPosition;
    private readonly bool _revisionGraphShowArtificialCommits;

    private CommitInfoPanelTestSettingsScope(
        bool disableAsyncCommitInfoData,
        CommitInfoPosition? commitInfoPosition,
        bool? revisionGraphShowArtificialCommits)
    {
        _commitInfoShowContainedInBranchesLocal = AppSettings.CommitInfoShowContainedInBranchesLocal;
        _commitInfoShowContainedInBranchesRemote = AppSettings.CommitInfoShowContainedInBranchesRemote;
        _commitInfoShowContainedInBranchesRemoteIfNoLocal = AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal;
        _commitInfoShowContainedInTags = AppSettings.CommitInfoShowContainedInTags;
        _commitInfoShowTagThisCommitDerivesFrom = AppSettings.CommitInfoShowTagThisCommitDerivesFrom;
        _showAnnotatedTagsMessages = AppSettings.ShowAnnotatedTagsMessages;
        _commitInfoPosition = AppSettings.CommitInfoPosition;
        _revisionGraphShowArtificialCommits = AppSettings.RevisionGraphShowArtificialCommits;

        if (disableAsyncCommitInfoData)
        {
            AppSettings.CommitInfoShowContainedInBranchesLocal = false;
            AppSettings.CommitInfoShowContainedInBranchesRemote = false;
            AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal = false;
            AppSettings.CommitInfoShowContainedInTags = false;
            AppSettings.CommitInfoShowTagThisCommitDerivesFrom = false;
            AppSettings.ShowAnnotatedTagsMessages = false;
        }

        if (commitInfoPosition is not null)
        {
            AppSettings.CommitInfoPosition = commitInfoPosition.Value;
        }

        if (revisionGraphShowArtificialCommits is not null)
        {
            AppSettings.RevisionGraphShowArtificialCommits = revisionGraphShowArtificialCommits.Value;
        }
    }

    public static CommitInfoPanelTestSettingsScope ForMinimalCommitInfoDataLoad()
        => new(disableAsyncCommitInfoData: true, commitInfoPosition: null, revisionGraphShowArtificialCommits: null);

    public static CommitInfoPanelTestSettingsScope ForBrowseRepositorySwitch()
        => new(
            disableAsyncCommitInfoData: true,
            commitInfoPosition: CommitInfoPosition.BelowList,
            revisionGraphShowArtificialCommits: false);

    public void Dispose()
    {
        AppSettings.CommitInfoShowContainedInBranchesLocal = _commitInfoShowContainedInBranchesLocal;
        AppSettings.CommitInfoShowContainedInBranchesRemote = _commitInfoShowContainedInBranchesRemote;
        AppSettings.CommitInfoShowContainedInBranchesRemoteIfNoLocal = _commitInfoShowContainedInBranchesRemoteIfNoLocal;
        AppSettings.CommitInfoShowContainedInTags = _commitInfoShowContainedInTags;
        AppSettings.CommitInfoShowTagThisCommitDerivesFrom = _commitInfoShowTagThisCommitDerivesFrom;
        AppSettings.ShowAnnotatedTagsMessages = _showAnnotatedTagsMessages;
        AppSettings.CommitInfoPosition = _commitInfoPosition;
        AppSettings.RevisionGraphShowArtificialCommits = _revisionGraphShowArtificialCommits;
    }
}
