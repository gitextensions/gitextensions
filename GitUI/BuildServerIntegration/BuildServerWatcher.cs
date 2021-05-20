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
using GitCommands.Settings;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.HelperDialogs;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using GitUIPluginInterfaces.Settings;
using Microsoft.VisualStudio.Threading;

namespace GitUI.BuildServerIntegration
{
    public sealed class BuildServerWatcher : IBuildServerWatcher, IDisposable
    {
        private static readonly TimeSpan ShortPollInterval = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan LongPollInterval = TimeSpan.FromSeconds(120);
        private readonly CancellationTokenSequence _launchCancellation = new();
        private readonly object _buildServerCredentialsLock = new();
        private readonly RevisionGridControl _revisionGrid;
        private readonly RevisionDataGridView _revisionGridView;
        private readonly Func<GitModule> _module;
        private readonly IRepoNameExtractor _repoNameExtractor;
        private IDisposable? _buildStatusCancellationToken;
        private IBuildServerAdapter? _buildServerAdapter;

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

            if (buildServerAdapter is null || launchToken.IsCancellationRequested)
            {
                return;
            }

            var scheduler = NewThreadScheduler.Default;

            // Run this first as it (may) force start queries
            var runningBuildsObservable = buildServerAdapter.GetRunningBuilds(scheduler);

            var fullDayObservable = buildServerAdapter.GetFinishedBuildsSince(scheduler, DateTime.Today - TimeSpan.FromDays(3));
            var fullObservable = buildServerAdapter.GetFinishedBuildsSince(scheduler);

            bool anyRunningBuilds = false;
            var delayObservable = Observable.Defer(() => Observable.Empty<BuildInfo>()
                                                                   .DelaySubscription(anyRunningBuilds ? ShortPollInterval : LongPollInterval));

            var shouldLookForNewlyFinishedBuilds = false;
            DateTime nowFrozen = DateTime.Now;

            // All finished builds have already been retrieved,
            // so looking for new finished builds make sense only if running builds have been found previously
            var fromNowObservable = Observable.If(() => shouldLookForNewlyFinishedBuilds,
                buildServerAdapter.GetFinishedBuildsSince(scheduler, nowFrozen)
                            .Finally(() => shouldLookForNewlyFinishedBuilds = false));

            CompositeDisposable cancellationToken = new()
                    {
                        fullDayObservable.OnErrorResumeNext(fullObservable)
                                         .OnErrorResumeNext(Observable.Empty<BuildInfo>()
                                                                      .DelaySubscription(LongPollInterval)
                                                                      .OnErrorResumeNext(fromNowObservable)
                                                                      .Retry()
                                                                      .Repeat())
                                         .ObserveOn(MainThreadScheduler.Instance)
                                         .Subscribe(OnBuildInfoUpdate),

                        runningBuildsObservable.Do(buildInfo =>
                                                    {
                                                        anyRunningBuilds = true;
                                                        shouldLookForNewlyFinishedBuilds = true;
                                                    })
                                               .OnErrorResumeNext(delayObservable)
                                               .Retry()
                                               .Finally(() => anyRunningBuilds = false)
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

        public IBuildServerCredentials? GetBuildServerCredentials(IBuildServerAdapter buildServerAdapter, bool useStoredCredentialsIfExisting)
        {
            lock (_buildServerCredentialsLock)
            {
                IBuildServerCredentials? buildServerCredentials = new BuildServerCredentials { UseGuestAccess = true };
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
                            using MemoryStream memoryStream = new(unprotectedData);
                            ConfigFile credentialsConfig = new("", false);

                            using (var textReader = new StreamReader(memoryStream, Encoding.UTF8))
                            {
                                credentialsConfig.LoadFromString(textReader.ReadToEnd());
                            }

                            var section = credentialsConfig.FindConfigSection(CredentialsConfigName);

                            if (section is not null)
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

                    if (buildServerCredentials is not null)
                    {
                        ConfigFile credentialsConfig = new("", true);

                        var section = credentialsConfig.FindOrCreateConfigSection(CredentialsConfigName);

                        section.SetValueAsBool(UseGuestAccessKey, buildServerCredentials.UseGuestAccess);
                        section.SetValue(UsernameKey, buildServerCredentials.Username);
                        section.SetValue(PasswordKey, buildServerCredentials.Password);

                        using var stream = GetBuildServerOptionsIsolatedStorageStream(buildServerAdapter, FileAccess.Write, FileShare.None);
                        using MemoryStream memoryStream = new();
                        using (var textWriter = new StreamWriter(memoryStream, Encoding.UTF8))
                        {
                            textWriter.Write(credentialsConfig.GetAsString());
                        }

                        var protectedData = ProtectedData.Protect(memoryStream.ToArray(), null, DataProtectionScope.CurrentUser);
                        stream.Write(protectedData, 0, protectedData.Length);

                        return buildServerCredentials;
                    }
                }

                return null;
            }
        }

        public string ReplaceVariables(string projectNames)
        {
            var (repoProject, repoName) = _repoNameExtractor.Get();

            if (!string.IsNullOrWhiteSpace(repoProject))
            {
                projectNames = projectNames.Replace("{cRepoProject}", repoProject);
            }

            if (!string.IsNullOrWhiteSpace(repoName))
            {
                projectNames = projectNames.Replace("{cRepoShortName}", repoName);
            }

            return projectNames;
        }

        private async Task<IBuildServerCredentials?> ShowBuildServerCredentialsFormAsync(string buildServerUniqueKey, IBuildServerCredentials buildServerCredentials)
        {
            await _revisionGrid.SwitchToMainThreadAsync();

            using FormBuildServerCredentials form = new(buildServerUniqueKey);
            form.BuildServerCredentials = buildServerCredentials;

            if (form.ShowDialog(_revisionGrid) == DialogResult.OK)
            {
                return buildServerCredentials;
            }

            return null;
        }

        private void OnBuildInfoUpdate(BuildInfo buildInfo)
        {
            if (_buildStatusCancellationToken is null)
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

                if (revision is null)
                {
                    continue;
                }

                if (revision.BuildStatus is null || buildInfo.StartDate >= revision.BuildStatus.StartDate)
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

        private async Task<IBuildServerAdapter?> GetBuildServerAdapterAsync()
        {
            await TaskScheduler.Default;

            IBuildServerSettings buildServerSettings = _module().GetEffectiveSettings()
                .BuildServer();

            if (!buildServerSettings.EnableIntegration)
            {
                return null;
            }

            var buildServerType = buildServerSettings.Type;
            if (string.IsNullOrEmpty(buildServerType))
            {
                return null;
            }

            var exports = ManagedExtensibility.GetExports<IBuildServerAdapter, IBuildServerTypeMetadata>();
            var export = exports.SingleOrDefault(x => x.Metadata.BuildServerType == buildServerType);

            if (export is not null)
            {
                try
                {
                    var canBeLoaded = export.Metadata.CanBeLoaded;
                    if (!string.IsNullOrEmpty(canBeLoaded))
                    {
                        Debug.Write(export.Metadata.BuildServerType + " adapter could not be loaded: " + canBeLoaded);
                        return null;
                    }

                    var buildServerAdapter = export.Value;

                    buildServerAdapter.Initialize(this, _module().GetEffectiveSettings().ByPath(buildServerSettings.Type!),
                        () =>
                        {
                            // To run the `StartSettingsDialog()` in the UI Thread
                            _revisionGrid.Invoke((Action)(() =>
                            {
                                _revisionGrid.UICommands.StartSettingsDialog(typeof(BuildServerIntegrationSettingsPage));
                            }));
                        }, objectId => _revisionGrid.GetRevision(objectId) is not null);
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
