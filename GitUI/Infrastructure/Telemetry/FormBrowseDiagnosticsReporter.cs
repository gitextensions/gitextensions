using System.Collections.Generic;
using GitCommands;
using GitUI.CommandsDialogs;

namespace GitUI.Infrastructure.Telemetry
{
    internal class FormBrowseDiagnosticsReporter
    {
        private readonly FormBrowse _owner;

        public FormBrowseDiagnosticsReporter(FormBrowse owner)
        {
            _owner = owner;
        }

        public void Report()
        {
            var properties = new Dictionary<string, string>
                {
                    // layout
                    { "ShowLeftPanel".FormatKey(), _owner.MainSplitContainer.Panel1Collapsed.ToString() },
                    { nameof(AppSettings.ShowSplitViewLayout).FormatKey(), AppSettings.ShowSplitViewLayout.ToString() },
                    { nameof(AppSettings.CommitInfoPosition).FormatKey(), AppSettings.CommitInfoPosition.ToString() },

                    // revision grid
                    { nameof(AppSettings.ShowAuthorAvatarColumn).FormatKey(), AppSettings.ShowAuthorAvatarColumn.ToString() },
                    { nameof(AppSettings.ShowAuthorNameColumn).FormatKey(), AppSettings.ShowAuthorNameColumn.ToString() },
                    { nameof(AppSettings.ShowBuildStatusIconColumn).FormatKey(), AppSettings.ShowBuildStatusIconColumn.ToString() },
                    { nameof(AppSettings.ShowBuildStatusTextColumn).FormatKey(), AppSettings.ShowBuildStatusTextColumn.ToString() },

                    // commit info panel
                    { nameof(AppSettings.ShowAuthorAvatarInCommitInfo).FormatKey(), AppSettings.ShowAuthorAvatarInCommitInfo.ToString() },
                    { nameof(AppSettings.ShowGpgInformation).FormatKey(), AppSettings.ShowGpgInformation.ValueOrDefault.ToString() },

                    // other
                    { nameof(AppSettings.ShowAheadBehindData).FormatKey(), AppSettings.ShowAheadBehindData.ToString() },
                    { nameof(AppSettings.CurrentTranslation).FormatKey(), AppSettings.CurrentTranslation },
                    { nameof(AppSettings.ShowGitStatusInBrowseToolbar).FormatKey(), AppSettings.ShowGitStatusInBrowseToolbar.ToString() },
                    { nameof(AppSettings.ShowGitStatusForArtificialCommits).FormatKey(), AppSettings.ShowGitStatusForArtificialCommits.ToString() },
                    { nameof(AppSettings.RevisionGraphShowWorkingDirChanges).FormatKey(), AppSettings.RevisionGraphShowWorkingDirChanges.ToString() },
                };

            DiagnosticsClient.TrackEvent($"{_owner.GetType().Name}Start", properties);
        }
    }
}