using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitCommands.BuildServerIntegration;
using GitUI.HelperDialogs;
using GitUI.RevisionGridClasses;
using Nini.Config;

namespace GitUI.BuildServerIntegration
{
    public class BuildServerWatcher : IBuildServerWatcher, IDisposable
    {
        private readonly RevisionGrid revisionGrid;
        private readonly DvcsGraph revisions;

        private int buildStatusImageColumnIndex = -1;
        private int buildStatusMessageColumnIndex = -1;

        private IDisposable buildStatusCancellationToken;
        private IBuildServerAdapter buildServerAdapter;

        public BuildServerWatcher(RevisionGrid revisionGrid, DvcsGraph revisions)
        {
            this.revisionGrid = revisionGrid;
            this.revisions = revisions;

            AddBuildStatusColumns();
        }

        public void LaunchBuildServerInfoFetchOperation()
        {
            CancelBuildStatusFetchOperation();

            // Extract the project name from the last part of the directory path. It is assumed that it matches the project name in the CI build server.
            buildServerAdapter = GetBuildServerAdapter();

            UpdateUI();

            if (buildServerAdapter == null)
                return;

            var scheduler = NewThreadScheduler.Default;
            var fullDayObservable = buildServerAdapter.GetFinishedBuildsSince(scheduler, DateTime.Today - TimeSpan.FromDays(3));
            var fullObservable = buildServerAdapter.GetFinishedBuildsSince(scheduler);
            var fromNowObservable = buildServerAdapter.GetFinishedBuildsSince(scheduler, DateTime.Now);
            var runningBuildsObservable = buildServerAdapter.GetRunningBuilds(scheduler);

            var cancellationToken = new CompositeDisposable();

            buildStatusCancellationToken = cancellationToken;

            cancellationToken.Add(
                fullDayObservable
                    .Concat(fullObservable)
                    .Concat(Observable
                                .Empty<BuildInfo>()
                                .Delay(TimeSpan.FromMinutes(1))
                                .Concat(fromNowObservable)
                                .Repeat())
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(OnBuildInfoUpdate));

            cancellationToken.Add(
                runningBuildsObservable
                    .Concat(Observable.Empty<BuildInfo>().Delay(TimeSpan.FromSeconds(10)))
                    .Repeat()
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(OnBuildInfoUpdate));
        }

        public void CancelBuildStatusFetchOperation()
        {
            var cancellationToken = Interlocked.Exchange(ref buildStatusCancellationToken, null);

            if (cancellationToken != null)
            {
                cancellationToken.Dispose();
            }
        }

        public bool GetBuildServerCredentials(IBuildServerAdapter buildServerAdapter, bool firstTime, out string username, out string password)
        {
            username = null;
            password = null;

            IniConfigSource buildServerConfigSource;
            const string CredentialsConfigName = "Credentials";
            const string UsernameKey = "Username";
            const string PasswordKey = "Password";
            using (var stream = GetBuildServerOptionsIsolatedStorageStream(buildServerAdapter, FileAccess.Read, FileShare.Read))
            {
                var protectedData = new byte[stream.Length];

                stream.Read(protectedData, 0, (int)stream.Length);

                byte[] unprotectedData = ProtectedData.Unprotect(protectedData, null, DataProtectionScope.CurrentUser);
                using (var memoryStream = new MemoryStream(unprotectedData))
                {
                    using (var textReader = new StreamReader(memoryStream, Encoding.UTF8))
                    {
                        buildServerConfigSource = new IniConfigSource(textReader);
                    }

                    var credentialsConfig = buildServerConfigSource.Configs[CredentialsConfigName];

                    if (credentialsConfig != null)
                    {
                        username = credentialsConfig.GetString(UsernameKey);
                        password = credentialsConfig.GetString(PasswordKey);

                        if (firstTime)
                        {
                            return true;
                        }
                    }
                }
            }

            if (!firstTime)
            {
                using (var form = new FormBuildServerCredentials(buildServerAdapter.UniqueKey))
                {
                    form.UserName = username;
                    form.Password = password;

                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        username = form.UserName;
                        password = form.Password;

                        var credentialsConfig = buildServerConfigSource.Configs[CredentialsConfigName] ??
                                                buildServerConfigSource.AddConfig(CredentialsConfigName);

                        credentialsConfig.Set(UsernameKey, username);
                        credentialsConfig.Set(PasswordKey, password);

                        using (var stream = GetBuildServerOptionsIsolatedStorageStream(buildServerAdapter, FileAccess.Write, FileShare.None))
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                using (var textWriter = new StreamWriter(memoryStream, Encoding.UTF8))
                                {
                                    buildServerConfigSource.Save(textWriter);
                                }

                                var protectedData = ProtectedData.Protect(memoryStream.ToArray(), null, DataProtectionScope.CurrentUser);
                                stream.Write(protectedData, 0, protectedData.Length);
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
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

        private void OnBuildInfoUpdate(BuildInfo buildInfo)
        {
            if (buildStatusCancellationToken == null)
                return;

            foreach (var commitHash in buildInfo.CommitHashList)
            {
                string graphRevision;
                int row = revisionGrid.SearchRevision(commitHash, out graphRevision);
                if (row >= 0)
                {
                    var rowData = revisions.GetRowData(row);
                    if (rowData.BuildStatus == null ||
                        buildInfo.StartDate >= rowData.BuildStatus.StartDate)
                    {
                        rowData.BuildStatus = buildInfo;

                        // Ensure that the Build Report tab page visibility is refreshed.
                        if (revisionGrid.GetSelectedRevisions().Contains(rowData))
                        {
                            // HACK: Since there is no INotifyPropertyChanged mechanism in Revision,
                            // we have to rely on the knowledge that FormBrowse listens to the 
                            // SelectionChanged event of RevisionGrid in order to show/hide
                            // the Build Report tab page.
                            var selectedIds = revisions.SelectedIds;
                            revisions.ClearSelection();
                            revisions.SelectedIds = selectedIds;
                        }

                        revisions.UpdateCellValue(4, row);
                        revisions.UpdateCellValue(5, row);
                    }
                }
            }
        }

        private IBuildServerAdapter GetBuildServerAdapter()
        {
            var fileName = Path.Combine(revisionGrid.Module.GitWorkingDir, ".buildserver");
            if (File.Exists(fileName))
            {
                var buildServerConfigSource = new IniConfigSource(fileName);
                var buildServerConfig = buildServerConfigSource.Configs["General"];

                if (buildServerConfig != null)
                {
                    var buildServerType = (BuildServerType)Enum.Parse(typeof(BuildServerType), buildServerConfig.GetString("ActiveBuildServerType", BuildServerType.None.ToString()));
                    var config = buildServerConfigSource.Configs[buildServerType.ToString()];

                    try
                    {
                        switch (buildServerType)
                        {
                            case BuildServerType.TeamCity:
                                return new TeamCityAdapter(this, config);
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
            var columnsAreVisible = buildServerAdapter != null;
            revisions.Columns[buildStatusImageColumnIndex].Visible = columnsAreVisible;
            revisions.Columns[buildStatusMessageColumnIndex].Visible = columnsAreVisible;
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

        private static IsolatedStorageFileStream GetBuildServerOptionsIsolatedStorageStream(IBuildServerAdapter buildServerAdapter, FileAccess fileAccess, FileShare fileShare)
        {
            var fileName = string.Format("BuildServer-{0}.options", Convert.ToBase64String(Encoding.UTF8.GetBytes(buildServerAdapter.UniqueKey)));
            return new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, fileAccess, fileShare);
        }
    }
}