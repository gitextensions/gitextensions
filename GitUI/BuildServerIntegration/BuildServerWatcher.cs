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
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft.VisualStudio.Threading;

namespace GitUI.BuildServerIntegration
{
    public sealed class BuildServerWatcher : IBuildServerWatcher, IDisposable
    {
        private static readonly TimeSpan ShortPollInterval = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan LongPollInterval = TimeSpan.FromSeconds(120);
        private readonly object _buildServerCredentialsLock = new();
        private readonly Func<GitModule> _module;
        private readonly IRepoNameExtractor _repoNameExtractor;
        private IDisposable? _buildStatusCancellationToken;
        private IBuildServerAdapter? _buildServerAdapter;

        public BuildServerWatcher(Func<GitModule> module)
        {
            _module = module;
            _repoNameExtractor = new RepoNameExtractor(_module);
        }

        public async Task LaunchBuildServerInfoFetchOperationAsync(Action<BuildInfo> onBuildInfoUpdate, Action openSettings, Func<ObjectId, bool>? isCommitInRevisionGrid = null)
        {
            CancelBuildStatusFetchOperation();

            _buildServerAdapter?.Dispose();
            _buildServerAdapter = await GetBuildServerAdapterAsync(openSettings, isCommitInRevisionGrid)
                .ConfigureAwait(false);

            if (_buildServerAdapter is null)
            {
                return;
            }

            var scheduler = NewThreadScheduler.Default;

            // Run this first as it (may) force start queries
            var runningBuildsObservable = _buildServerAdapter.GetRunningBuilds(scheduler);

            var fullDayObservable = _buildServerAdapter.GetFinishedBuildsSince(scheduler, DateTime.Today - TimeSpan.FromDays(3));
            var fullObservable = _buildServerAdapter.GetFinishedBuildsSince(scheduler);

            bool anyRunningBuilds = false;
            var delayObservable = Observable.Defer(() => Observable.Empty<BuildInfo>()
                                                                   .DelaySubscription(anyRunningBuilds ? ShortPollInterval : LongPollInterval));

            var shouldLookForNewlyFinishedBuilds = false;
            DateTime nowFrozen = DateTime.Now;

            // All finished builds have already been retrieved,
            // so looking for new finished builds make sense only if running builds have been found previously
            var fromNowObservable = Observable.If(() => shouldLookForNewlyFinishedBuilds,
                _buildServerAdapter.GetFinishedBuildsSince(scheduler, nowFrozen)
                            .Finally(() => shouldLookForNewlyFinishedBuilds = false));

            var cancellationToken = new CompositeDisposable
                    {
                        fullDayObservable.OnErrorResumeNext(fullObservable)
                                         .OnErrorResumeNext(Observable.Empty<BuildInfo>()
                                                                      .DelaySubscription(LongPollInterval)
                                                                      .OnErrorResumeNext(fromNowObservable)
                                                                      .Retry()
                                                                      .Repeat())
                                         .ObserveOn(MainThreadScheduler.Instance)
                                         .Subscribe(x =>
                                         {
                                             if (_buildStatusCancellationToken is null)
                                             {
                                                 return;
                                             }

                                             onBuildInfoUpdate(x);
                                         }),

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
                                               .Subscribe(x =>
                                               {
                                                   if (_buildStatusCancellationToken is null)
                                                   {
                                                       return;
                                                   }

                                                   onBuildInfoUpdate(x);
                                               })
                    };

            CancelBuildStatusFetchOperation();
            _buildStatusCancellationToken = cancellationToken;
        }

        public void CancelBuildStatusFetchOperation()
        {
            var cancellationToken = Interlocked.Exchange(ref _buildStatusCancellationToken, null);

            cancellationToken?.Dispose();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "http://stackoverflow.com/questions/1065168/does-disposing-streamreader-close-the-stream")]
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
                            using var memoryStream = new MemoryStream(unprotectedData);
                            var credentialsConfig = new ConfigFile("", false);

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
                        var credentialsConfig = new ConfigFile("", true);

                        var section = credentialsConfig.FindOrCreateConfigSection(CredentialsConfigName);

                        section.SetValueAsBool(UseGuestAccessKey, buildServerCredentials.UseGuestAccess);
                        section.SetValue(UsernameKey, buildServerCredentials.Username);
                        section.SetValue(PasswordKey, buildServerCredentials.Password);

                        using var stream = GetBuildServerOptionsIsolatedStorageStream(buildServerAdapter, FileAccess.Write, FileShare.None);
                        using var memoryStream = new MemoryStream();
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

        private Task<IBuildServerCredentials?> ShowBuildServerCredentialsFormAsync(string buildServerUniqueKey, IBuildServerCredentials buildServerCredentials)
        {
            using var form = new FormBuildServerCredentials(buildServerUniqueKey)
            {
                BuildServerCredentials = buildServerCredentials
            };

            if (form.ShowDialog() == DialogResult.OK)
            {
                return Task.FromResult<IBuildServerCredentials?>(buildServerCredentials);
            }

            return Task.FromResult<IBuildServerCredentials?>(null);
        }

        private async Task<IBuildServerAdapter?> GetBuildServerAdapterAsync(Action openSettings, Func<ObjectId, bool>? isCommitInRevisionGrid = null)
        {
            await TaskScheduler.Default;

            var buildServerSettings = _module().EffectiveSettings.BuildServer;

            if (!buildServerSettings.EnableIntegration.Value)
            {
                return null;
            }

            var buildServerType = buildServerSettings.Type.Value;
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

                    buildServerAdapter.Initialize(this, buildServerSettings.TypeSettings, openSettings, isCommitInRevisionGrid);
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
        }

        private static IsolatedStorageFileStream GetBuildServerOptionsIsolatedStorageStream(IBuildServerAdapter buildServerAdapter, FileAccess fileAccess, FileShare fileShare)
        {
            var fileName = string.Format("BuildServer-{0}.options", Convert.ToBase64String(Encoding.UTF8.GetBytes(buildServerAdapter.UniqueKey)));
            return new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, fileAccess, fileShare);
        }
    }
}
