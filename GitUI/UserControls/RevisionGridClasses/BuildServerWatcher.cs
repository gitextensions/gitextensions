using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitCommands.BuildServerIntegration;
using GitUI.RevisionGridClasses;
using Nini.Config;

namespace GitUI.UserControls.RevisionGridClasses
{
    public class BuildServerWatcher : IDisposable
    {
        private readonly RevisionGrid revisionGrid;
        private readonly DvcsGraph revisions;

        private int buildStatusImageColumnIndex = -1;
        private int buildStatusMessageColumnIndex = -1;

        private IDisposable buildStatusCancellationToken;
        private IBuildServerWatcher buildServerWatcher;

        public BuildServerWatcher(RevisionGrid revisionGrid, DvcsGraph revisions)
        {
            this.revisionGrid = revisionGrid;
            this.revisions = revisions;

            AddBuildStatusColumns();
        }

        private void AddBuildStatusColumns()
        {
            var buildStatusImageColumn = new DataGridViewImageColumn
                                             {
                                                 AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                                                 Width = 16,
                                                 ReadOnly = true,
                                                 SortMode = DataGridViewColumnSortMode.NotSortable
                                             };
            var buildMessageTextBoxColumn = new DataGridViewTextBoxColumn
                                                {
                                                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                                                    HeaderText = "Build Status",
                                                    ReadOnly = true,
                                                    SortMode = DataGridViewColumnSortMode.NotSortable
                                                };

            buildStatusImageColumnIndex = revisions.Columns.Add(buildStatusImageColumn);
            buildStatusMessageColumnIndex = revisions.Columns.Add(buildMessageTextBoxColumn);
        }

        public void LaunchBuildServerInfoFetchOperation()
        {
            CancelBuildStatusFetchOperation();

            var projectName = revisionGrid.Module.GitWorkingDir.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).Last();
            buildServerWatcher = GetBuildServerWatcher(projectName);

            UpdateUI();

            if (buildServerWatcher == null)
                return;

            var fullDayObservable = buildServerWatcher.CreateObservable(DateTime.Now.Date);
            var fullObservable = buildServerWatcher.CreateObservable();
            var fromNowObservable = buildServerWatcher.CreateObservable(DateTime.Now);

            buildStatusCancellationToken =
                fullDayObservable.Concat(fullObservable)
                                 .Concat(Observable.Empty<BuildInfo>().Delay(TimeSpan.FromMinutes(1))
                                                   .Concat(fromNowObservable)
                                                   .Repeat())
                                 .Subscribe(item =>
                                     {
                                         if (buildStatusCancellationToken == null)
                                             return;

                                         string graphRevision;
                                         int row = revisionGrid.SearchRevision(item.CommitHash, out graphRevision);
                                         if (row >= 0)
                                         {
                                             var rowData = revisions.GetRowData(row);
                                             if (rowData.BuildStatus == null ||
                                                 item.StartDate > rowData.BuildStatus.StartDate)
                                             {
                                                 rowData.BuildStatus = item;

                                                 revisions.UpdateCellValue(4, row);
                                                 revisions.UpdateCellValue(5, row);
                                             }
                                         }
                                     });
        }

        private IBuildServerWatcher GetBuildServerWatcher(string projectName)
        {
            var fileName = Path.Combine(revisionGrid.Module.GitWorkingDir, ".buildserver");
            if (File.Exists(fileName))
            {
                var buildServerConfigSource = new IniConfigSource(fileName);
                var buildServerConfig = buildServerConfigSource.Configs["General"];

                if (buildServerConfig != null)
                {
                    var buildServerType = (BuildServerType)Enum.Parse(typeof(BuildServerType), buildServerConfig.GetString("ActiveBuildServerType", BuildServerType.None.ToString()));
                    try
                    {
                        switch (buildServerType)
                        {
                            case BuildServerType.TeamCity:
                                return new TeamCityBuildWatcher(projectName, buildServerConfigSource.Configs[buildServerType.ToString()]);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // Invalid arguments, do not return a build server adapter
                    }
                }
            }

            return null;
        }

        private void UpdateUI()
        {
            var columnsAreVisible = buildServerWatcher != null;
            revisions.Columns[buildStatusImageColumnIndex].Visible = columnsAreVisible;
            revisions.Columns[buildStatusMessageColumnIndex].Visible = columnsAreVisible;
        }

        public void CancelBuildStatusFetchOperation()
        {
            var cancellationToken = Interlocked.Exchange(ref buildStatusCancellationToken, null);

            if (cancellationToken != null)
            {
                cancellationToken.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                CancelBuildStatusFetchOperation();
            }
        }
    }
}