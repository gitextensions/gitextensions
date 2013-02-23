using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitCommands.BuildServerIntegration;
using GitUI.RevisionGridClasses;
using Nini.Config;

namespace GitUI.BuildServerIntegration
{
    public class BuildServerWatcher : IDisposable
    {
        private readonly RevisionGrid revisionGrid;
        private readonly DvcsGraph revisions;

        private int buildStatusImageColumnIndex = -1;
        private int buildStatusMessageColumnIndex = -1;

        private IDisposable buildStatusCancellationToken;
        private IBuildServerAdapter _buildServerAdapter;

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
                                                    ReadOnly = true,
                                                    SortMode = DataGridViewColumnSortMode.NotSortable
                                                };

            buildStatusImageColumnIndex = revisions.Columns.Add(buildStatusImageColumn);
            buildStatusMessageColumnIndex = revisions.Columns.Add(buildMessageTextBoxColumn);
        }

        public void LaunchBuildServerInfoFetchOperation()
        {
            CancelBuildStatusFetchOperation();

            // Extract the project name from the last part of the directory path. It is assumed that it matches the project name in the CI build server.
            var projectName = revisionGrid.Module.GitWorkingDir.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).Last();
            _buildServerAdapter = GetBuildServerAdapter(projectName);

            UpdateUI();

            if (_buildServerAdapter == null)
                return;

            var fullDayObservable = _buildServerAdapter.CreateObservable(NewThreadScheduler.Default, DateTime.Now.Date);
            var fullObservable = _buildServerAdapter.CreateObservable(NewThreadScheduler.Default);
            var fromNowObservable = _buildServerAdapter.CreateObservable(NewThreadScheduler.Default, DateTime.Now);

            buildStatusCancellationToken =
                fullDayObservable.Concat(fullObservable)
                                 .Concat(Observable.Empty<BuildInfo>()
                                                   .Delay(TimeSpan.FromMinutes(1))
                                                   .Concat(fromNowObservable)
                                                   .Repeat())
                                 .ObserveOn(SynchronizationContext.Current)
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

        private IBuildServerAdapter GetBuildServerAdapter(string projectName)
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
                                return new TeamCityAdapter(projectName, buildServerConfigSource.Configs[buildServerType.ToString()]);
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
            var columnsAreVisible = _buildServerAdapter != null;
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