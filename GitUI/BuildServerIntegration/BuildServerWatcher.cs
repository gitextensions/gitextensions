using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitUI.HelperDialogs;
using GitUI.RevisionGridClasses;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitUI.BuildServerIntegration
{
    public class BuildServerWatcher : IBuildServerWatcher, IDisposable
    {
        private readonly RevisionGrid revisionGrid;
        private readonly DvcsGraph revisions;
        private GitModule Module => revisionGrid.Module;

        public int BuildStatusImageColumnIndex { get; private set; }
        public int BuildStatusMessageColumnIndex { get; private set; }

        private IDisposable buildStatusCancellationToken;
        private IBuildServerAdapter buildServerAdapter;

        private readonly object buildServerCredentialsLock = new object();

        public BuildServerWatcher(RevisionGrid revisionGrid, DvcsGraph revisions)
        {
            this.revisionGrid = revisionGrid;
            this.revisions = revisions;
            BuildStatusImageColumnIndex = -1;
            BuildStatusMessageColumnIndex = -1;
        }

        public void LaunchBuildServerInfoFetchOperation()
        {
            CancelBuildStatusFetchOperation();

            DisposeBuildServerAdapter();

            // Extract the project name from the last part of the directory path. It is assumed that it matches the project name in the CI build server.
            GetBuildServerAdapter().ContinueWith(task =>
            {
                if (revisions.IsDisposed)
                {
                    return;
                }

                buildServerAdapter = task.Result;

                UpdateUI();

                if (buildServerAdapter == null)
                    return;

                var scheduler = NewThreadScheduler.Default;
                var fullDayObservable = buildServerAdapter.GetFinishedBuildsSince(scheduler, DateTime.Today - TimeSpan.FromDays(3));
                var fullObservable = buildServerAdapter.GetFinishedBuildsSince(scheduler);
                var fromNowObservable = buildServerAdapter.GetFinishedBuildsSince(scheduler, DateTime.Now);
                var runningBuildsObservable = buildServerAdapter.GetRunningBuilds(scheduler);

                var cancellationToken = new CompositeDisposable
                {
                    fullDayObservable.OnErrorResumeNext(fullObservable)
                                     .OnErrorResumeNext(Observable.Empty<BuildInfo>()
                                                                  .DelaySubscription(TimeSpan.FromMinutes(1))
                                                                  .OnErrorResumeNext(fromNowObservable)
                                                                  .Retry()
                                                                  .Repeat())
                                     .ObserveOn(SynchronizationContext.Current)
                                     .Subscribe(OnBuildInfoUpdate),

                    runningBuildsObservable.OnErrorResumeNext(Observable.Empty<BuildInfo>()
                                                                        .DelaySubscription(TimeSpan.FromSeconds(10)))
                                           .Retry()
                                           .Repeat()
                                           .ObserveOn(SynchronizationContext.Current)
                                           .Subscribe(OnBuildInfoUpdate)
                };

                buildStatusCancellationToken = cancellationToken;
            },
            TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void CancelBuildStatusFetchOperation()
        {
            var cancellationToken = Interlocked.Exchange(ref buildStatusCancellationToken, null);

            cancellationToken?.Dispose();
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "http://stackoverflow.com/questions/1065168/does-disposing-streamreader-close-the-stream")]
        public IBuildServerCredentials GetBuildServerCredentials(IBuildServerAdapter buildServerAdapter, bool useStoredCredentialsIfExisting)
        {
            lock (buildServerCredentialsLock)
            {
                IBuildServerCredentials buildServerCredentials = new BuildServerCredentials { UseGuestAccess = true };
                var foundInConfig = false;

                const string CredentialsConfigName = "Credentials";
                const string UseGuestAccessKey = "UseGuestAccess";
                const string UsernameKey = "Username";
                const string PasswordKey = "Password";
                using (var stream = GetBuildServerOptionsIsolatedStorageStream(buildServerAdapter, FileAccess.Read, FileShare.Read))
                {
                    if (stream.Position < stream.Length)
                    {
                        var protectedData = new byte[stream.Length];

                        stream.Read(protectedData, 0, (int)stream.Length);
                        try
                        {
                            byte[] unprotectedData = ProtectedData.Unprotect(protectedData, null,
                                DataProtectionScope.CurrentUser);
                            using (var memoryStream = new MemoryStream(unprotectedData))
                            {
                                ConfigFile credentialsConfig = new ConfigFile("", false);

                                using (var textReader = new StreamReader(memoryStream, Encoding.UTF8))
                                {
                                    credentialsConfig.LoadFromString(textReader.ReadToEnd());
                                }

                                var section = credentialsConfig.FindConfigSection(CredentialsConfigName);

                                if (section != null)
                                {
                                    buildServerCredentials.UseGuestAccess = section.GetValueAsBool(UseGuestAccessKey,
                                        true);
                                    buildServerCredentials.Username = section.GetValue(UsernameKey);
                                    buildServerCredentials.Password = section.GetValue(PasswordKey);
                                    foundInConfig = true;

                                    if (useStoredCredentialsIfExisting)
                                    {
                                        return buildServerCredentials;
                                    }
                                }
                            }
                        }
                        catch (CryptographicException)
                        {
                            // As per MSDN, the ProtectedData.Unprotect method is per user,
                            // it will throw the CryptographicException if the current user
                            // is not the one who protected the data.

                            // Set this variable to false so the user can reset the credentials.
                            useStoredCredentialsIfExisting = false;
                        }
                    }
                }

                if (!useStoredCredentialsIfExisting || !foundInConfig)
                {
                    buildServerCredentials = ShowBuildServerCredentialsForm(buildServerAdapter.UniqueKey, buildServerCredentials);

                    if (buildServerCredentials != null)
                    {
                        ConfigFile credentialsConfig = new ConfigFile("", true);

                        var section = credentialsConfig.FindOrCreateConfigSection(CredentialsConfigName);

                        section.SetValueAsBool(UseGuestAccessKey, buildServerCredentials.UseGuestAccess);
                        section.SetValue(UsernameKey, buildServerCredentials.Username);
                        section.SetValue(PasswordKey, buildServerCredentials.Password);

                        using (var stream = GetBuildServerOptionsIsolatedStorageStream(buildServerAdapter, FileAccess.Write, FileShare.None))
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                using (var textWriter = new StreamWriter(memoryStream, Encoding.UTF8))
                                {
                                    textWriter.Write(credentialsConfig.GetAsString());
                                }

                                var protectedData = ProtectedData.Protect(memoryStream.ToArray(), null, DataProtectionScope.CurrentUser);
                                stream.Write(protectedData, 0, protectedData.Length);
                            }
                        }

                        return buildServerCredentials;
                    }
                }

                return null;
            }
        }

        private IBuildServerCredentials ShowBuildServerCredentialsForm(string buildServerUniqueKey, IBuildServerCredentials buildServerCredentials)
        {
            if (revisionGrid.InvokeRequired)
            {
                return (IBuildServerCredentials)revisionGrid.Invoke(new Func<IBuildServerCredentials>(() => ShowBuildServerCredentialsForm(buildServerUniqueKey, buildServerCredentials)));
            }

            using (var form = new FormBuildServerCredentials(buildServerUniqueKey))
            {
                form.BuildServerCredentials = buildServerCredentials;

                if (form.ShowDialog(revisionGrid) == DialogResult.OK)
                {
                    return buildServerCredentials;
                }
            }

            return null;
        }

        private void AddBuildStatusColumns()
        {
            if (BuildStatusImageColumnIndex == -1)
            {
                var buildStatusImageColumn = new DataGridViewImageColumn
                                                 {
                                                     AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                                                     Width = 16,
                                                     ReadOnly = true,
                                                     Resizable = DataGridViewTriState.False,
                                                     SortMode = DataGridViewColumnSortMode.NotSortable
                                                 };
                BuildStatusImageColumnIndex = revisions.Columns.Add(buildStatusImageColumn);
            }

            if (BuildStatusMessageColumnIndex == -1 && Module.EffectiveSettings.BuildServer.ShowBuildSummaryInGrid.ValueOrDefault)
            {
                var buildMessageTextBoxColumn = new DataGridViewTextBoxColumn
                                                {
                                                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                                                    ReadOnly = true,
                                                    FillWeight = 50,
                                                    SortMode = DataGridViewColumnSortMode.NotSortable
                                                };

                BuildStatusMessageColumnIndex = revisions.Columns.Add(buildMessageTextBoxColumn);
            }
        }

        private void OnBuildInfoUpdate(BuildInfo buildInfo)
        {
            if (buildStatusCancellationToken == null)
                return;

            foreach (var commitHash in buildInfo.CommitHashList)
            {
                var index = revisions.TryGetRevisionIndex(commitHash);
                if (index.HasValue)
                {
                    var rowData = revisions.GetRowData(index.Value);
                    if (rowData.BuildStatus == null ||
                        buildInfo.StartDate >= rowData.BuildStatus.StartDate)
                    {
                        rowData.BuildStatus = buildInfo;
                        if (index.Value < revisions.RowCount)
                        {
                            if (BuildStatusImageColumnIndex != -1 &&
                                revisions.Rows[index.Value].Cells[BuildStatusImageColumnIndex].Displayed)
                                revisions.UpdateCellValue(BuildStatusImageColumnIndex, index.Value);
                            if (BuildStatusMessageColumnIndex != -1 &&
                                revisions.Rows[index.Value].Cells[BuildStatusMessageColumnIndex].Displayed)
                                revisions.UpdateCellValue(BuildStatusMessageColumnIndex, index.Value);
                        }
                    }
                }
            }
        }

        private Task<IBuildServerAdapter> GetBuildServerAdapter()
        {
            return Task<IBuildServerAdapter>.Factory.StartNew(() =>
            {
                if (!Module.EffectiveSettings.BuildServer.EnableIntegration.ValueOrDefault)
                    return null;
                var buildServerType = Module.EffectiveSettings.BuildServer.Type.ValueOrDefault;
                if (string.IsNullOrEmpty(buildServerType))
                    return null;
                var exports = ManagedExtensibility.GetExports<IBuildServerAdapter, IBuildServerTypeMetadata>();
                var export = exports.SingleOrDefault(x => x.Metadata.BuildServerType == buildServerType);

                if (export != null)
                {
                    try
                    {
                        var canBeLoaded = export.Metadata.CanBeLoaded;
                        if (!canBeLoaded.IsNullOrEmpty())
                        {
                            Debug.Write(export.Metadata.BuildServerType + " adapter could not be loaded: " + canBeLoaded);
                            return null;
                        }
                        var buildServerAdapter = export.Value;
                        buildServerAdapter.Initialize(this, Module.EffectiveSettings.BuildServer.TypeSettings, sha1 => revisionGrid.GetRevision(sha1) != null);
                        return buildServerAdapter;
                    }
                    catch (InvalidOperationException ex)
                    {
                        Debug.Write(ex);
                        // Invalid arguments, do not return a build server adapter
                    }
                }

                return null;
            });
        }

        private void UpdateUI()
        {
            var columnsAreVisible = buildServerAdapter != null;

            if (columnsAreVisible)
            {
                AddBuildStatusColumns();
            }

            if (BuildStatusImageColumnIndex != -1)
                revisions.Columns[BuildStatusImageColumnIndex].Visible = columnsAreVisible;

            if (BuildStatusMessageColumnIndex != -1)
                revisions.Columns[BuildStatusMessageColumnIndex].Visible = columnsAreVisible && Module.EffectiveSettings.BuildServer.ShowBuildSummaryInGrid.ValueOrDefault;
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

                DisposeBuildServerAdapter();
            }
        }

        private void DisposeBuildServerAdapter()
        {
            if (buildServerAdapter != null)
            {
                buildServerAdapter.Dispose();
                buildServerAdapter = null;
            }
        }

        private static IsolatedStorageFileStream GetBuildServerOptionsIsolatedStorageStream(IBuildServerAdapter buildServerAdapter, FileAccess fileAccess, FileShare fileShare)
        {
            var fileName = string.Format("BuildServer-{0}.options", Convert.ToBase64String(Encoding.UTF8.GetBytes(buildServerAdapter.UniqueKey)));
            return new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, fileAccess, fileShare);
        }
    }
}