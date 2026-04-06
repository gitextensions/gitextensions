using GitCommands;
using GitUI.CommandsDialogs;

namespace GitUI.Infrastructure.Telemetry;

internal class FormBrowseDiagnosticsReporter
{
    private readonly FormBrowse _owner;

    public FormBrowseDiagnosticsReporter(FormBrowse owner)
    {
        _owner = owner;
    }

    public void Report()
    {
        Dictionary<string, string> properties = new()
            {
                // layout
                { "ShowLeftPanel".FormatKey(), _owner.MainSplitContainer.Panel1Collapsed.ToString() },
                { nameof(AppSettings.ShowSplitViewLayout.Value).FormatKey(), AppSettings.ShowSplitViewLayout.Value.ToString() },
                { nameof(AppSettings.CommitInfoPosition).FormatKey(), AppSettings.CommitInfoPosition.ToString() },

                // revision grid
                { nameof(AppSettings.ShowAuthorAvatarColumn.Value).FormatKey(), AppSettings.ShowAuthorAvatarColumn.Value.ToString() },
                { nameof(AppSettings.ShowAuthorNameColumn.Value).FormatKey(), AppSettings.ShowAuthorNameColumn.Value.ToString() },
                { nameof(AppSettings.ShowBuildStatusIconColumn.Value).FormatKey(), AppSettings.ShowBuildStatusIconColumn.Value.ToString() },
                { nameof(AppSettings.ShowBuildStatusTextColumn.Value).FormatKey(), AppSettings.ShowBuildStatusTextColumn.Value.ToString() },

                // commit info panel
                { nameof(AppSettings.ShowAuthorAvatarInCommitInfo.Value).FormatKey(), AppSettings.ShowAuthorAvatarInCommitInfo.Value.ToString() },
                { nameof(AppSettings.ShowGpgInformation.Value).FormatKey(), AppSettings.ShowGpgInformation.Value.ToString() },

                // other
                { nameof(AppSettings.ShowAheadBehindData.Value).FormatKey(), AppSettings.ShowAheadBehindData.Value.ToString() },
                { nameof(AppSettings.CurrentTranslation).FormatKey(), AppSettings.CurrentTranslation },
                { nameof(AppSettings.ShowGitStatusInBrowseToolbar.Value).FormatKey(), AppSettings.ShowGitStatusInBrowseToolbar.Value.ToString() },
                { nameof(AppSettings.ShowGitStatusForArtificialCommits.Value).FormatKey(), AppSettings.ShowGitStatusForArtificialCommits.Value.ToString() },
                { nameof(AppSettings.RevisionGraphShowArtificialCommits.Value).FormatKey(), AppSettings.RevisionGraphShowArtificialCommits.Value.ToString() },
            };

        DiagnosticsClient.TrackEvent($"{_owner.GetType().Name}Start", properties);
    }
}
