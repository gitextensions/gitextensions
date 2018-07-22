using System;
using System.Diagnostics;
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
using GitCommands.Remotes;
using GitUI.HelperDialogs;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace GitUI.BuildServerIntegration
{
    public sealed class BuildServerWatcher : IBuildServerWatcher, IDisposable
    {
        private readonly CancellationTokenSequence _launchCancellation = new CancellationTokenSequence();
        private readonly object _buildServerCredentialsLock = new object();
        private readonly RevisionGridControl _revisionGrid;
        private readonly RevisionDataGridView _revisionGridView;
        private readonly Func<GitModule> _module;
        private readonly IRepoNameExtractor _repoNameExtractor;
        private IDisposable _buildStatusCancellationToken;
        private IBuildServerAdapter _buildServerAdapter;

        internal BuildStatusColumnProvider ColumnProvider { get; }

        public BuildServerWatcher(RevisionGridControl revisionGrid, RevisionDataGridView revisionGridView, Func<GitModule> module)
        {
            _revisionGrid = revisionGrid;
            _revisionGridView = revisionGridView;
            _module = module;

            _repoNameExtractor = new RepoNameExtractor(_module);
            ColumnProvider = new BuildStatusColumnProvider(revisionGrid, revisionGridView, _module);
        }

        public async Task LaunchBuildServerInfoFetchOperationAsync()
        {
            await TaskScheduler.Default;

            CancelBuildStatusFetchOperation();

            var launchToken = _launchCancellation.Next();

            var buildServerAdapter = await GetBuildServerAdapterAsync().ConfigureAwait(false);

            await _revisionGridView.SwitchToMainThreadAsync(launchToken);

            _buildServerAdapter?.Dispose();
            _buildServerAdapter = buildServerAdapter;

            await TaskScheduler.Default;

            if (buildServerAdapter == null || launchToken.IsCancellationRequested)
            {
                return;
            }

            var scheduler = NewThreadScheduler.Default;

            // Run this first as it (may) force start queries
            var runningBuildsObservable = buildServerAdapter.GetRunningBuilds(scheduler);

            var fullDayObservable = buildServerAdapter.GetFinishedBuildsSince(scheduler, DateTime.Today - TimeSpan.FromDays(3));
            var fullObservable = buildServerAdapter.GetFinishedBuildsSince(scheduler);
            var fromNowObservable = buildServerAdapter.GetFinishedBuildsSince(scheduler, DateTime.Now);

            var cancellationToken = new CompositeDisposable
                    {
                        fullDayObservable.OnErrorResumeNext(fullObservable)
                                         .OnErrorResumeNext(Observable.Empty<BuildInfo>()
                                                                      .DelaySubscription(TimeSpan.FromMinutes(1))
                                                                      .OnErrorResumeNext(fromNowObservable)
                                                                      .Retry()
                                                                      .Repeat())
                                         .ObserveOn(MainThreadScheduler.Instance)
                                         .Subscribe(OnBuildInfoUpdate),

                        runningBuildsObservable.OnErrorResumeNext(Observable.Empty<BuildInfo>()
                                                                            .DelaySubscription(TimeSpan.FromSeconds(10)))
                                               .Retry()
                                               .Repeat()
                                               .ObserveOn(MainThreadScheduler.Instance)
                                               .Subscribe(OnBuildInfoUpdate)
                    };

            await _revisionGridView.SwitchToMainThreadAsync(launchToken);

            CancelBuildStatusFetchOperation();
            _buildStatusCancellationToken = cancellationToken;
        }

        public void CancelBuildStatusFetchOperation()
        {
            var cancellationToken = Interlocked.Exchange(ref _buildStatusCancellationToken, null);

            cancellationToken?.Dispose();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "http://stackoverflow.com/questions/1065168/does-disposing-streamreader-close-the-stream")]
        public IBuildServerCredentials GetBuildServerCredentials(IBuildServerAdapter buildServerAdapter, bool useStoredCredentialsIfExisting)
        {
            lock (_buildServerCredentialsLock)
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
                                var credentialsConfig = new ConfigFile("", false);

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
                    buildServerCredentials = ThreadHelper.JoinableTaskFactory.Run(() => ShowBuildServerCredentialsFormAsync(buildServerAdapter.UniqueKey, buildServerCredentials));

                    if (buildServerCredentials != null)
                    {
                        var credentialsConfig = new ConfigFile("", true);

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

        public string ReplaceVariables(string projectNames)
        {
            var (repoProject, repoName) = _repoNameExtractor.Get();

            if (repoProject.IsNotNullOrWhitespace())
            {
                projectNames = projectNames.Replace("{cRepoProject}", repoProject);
            }

            if (repoName.IsNotNullOrWhitespace())
            {
                projectNames = projectNames.Replace("{cRepoShortName}", repoName);
            }

            return projectNames;
        }

        private async Task<IBuildServerCredentials> ShowBuildServerCredentialsFormAsync(string buildServerUniqueKey, IBuildServerCredentials buildServerCredentials)
        {
            await _revisionGrid.SwitchToMainThreadAsync();

            using (var form = new FormBuildServerCredentials(buildServerUniqueKey))
            {
                form.BuildServerCredentials = buildServerCredentials;

                if (form.ShowDialog(_revisionGrid) == DialogResult.OK)
                {
                    return buildServerCredentials;
                }
            }

            return null;
        }

        private void OnBuildInfoUpdate(BuildInfo buildInfo)
        {
            if (_buildStatusCancellationToken == null)
            {
                return;
            }

            foreach (var commitHash in buildInfo.CommitHashList)
            {
                var index = _revisionGridView.TryGetRevisionIndex(commitHash);

                if (!index.HasValue)
                {
                    continue;
                }

                var revision = _revisionGridView.GetRevision(index.Value);

                if (revision == null)
                {
                    continue;
                }

                if (revision.BuildStatus == null || buildInfo.StartDate >= revision.BuildStatus.StartDate)
                {
                    revision.BuildStatus = buildInfo;

                    if (index.Value < _revisionGridView.RowCount)
                    {
                        if (_revisionGridView.Rows[index.Value].Cells[ColumnProvider.Index].Displayed)
                        {
                            _revisionGridView.UpdateCellValue(ColumnProvider.Index, index.Value);
                        }
                    }
                }
            }
        }

        [ItemCanBeNull]
        private async Task<IBuildServerAdapter> GetBuildServerAdapterAsync()
        {
            await TaskScheduler.Default;

            var buildServerSettings = _module().EffectiveSettings.BuildServer;

            if (!buildServerSettings.EnableIntegration.ValueOrDefault)
            {
                return null;
            }

            var buildServerType = buildServerSettings.Type.ValueOrDefault;
            if (string.IsNullOrEmpty(buildServerType))
            {
                return null;
            }

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

                    buildServerAdapter.Initialize(this, buildServerSettings.TypeSettings, objectId => _revisionGrid.GetRevision(objectId) != null);
                    return buildServerAdapter;
                }
                catch (InvalidOperationException ex)
                {
                    Debug.Write(ex);

                    // Invalid arguments, do not return a build server adapter
                }
            }

            return null;
        }

        public void Dispose()
        {
            CancelBuildStatusFetchOperation();

            _buildServerAdapter?.Dispose();
            _launchCancellation.Dispose();
        }

        private static IsolatedStorageFileStream GetBuildServerOptionsIsolatedStorageStream(IBuildServerAdapter buildServerAdapter, FileAccess fileAccess, FileShare fileShare)
        {
            var fileName = string.Format("BuildServer-{0}.options", Convert.ToBase64String(Encoding.UTF8.GetBytes(buildServerAdapter.UniqueKey)));
            return new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, fileAccess, fileShare);
        }
    }
}