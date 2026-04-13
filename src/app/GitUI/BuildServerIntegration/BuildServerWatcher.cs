using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using GitCommands;
using GitCommands.Config;
using GitCommands.Remotes;
using GitCommands.Settings;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.HelperDialogs;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft.VisualStudio.Threading;

namespace GitUI.BuildServerIntegration;

public sealed class BuildServerWatcher : IBuildServerWatcher, IDisposable
{
    private static readonly TimeSpan ShortPollInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan LongPollInterval = TimeSpan.FromSeconds(120);
    private readonly CancellationTokenSequence _launchCancellation = new();
    private readonly Lock _buildServerCredentialsLock = new();
    private readonly RevisionGridControl _revisionGrid;
    private readonly RevisionDataGridView _revisionGridView;
    private readonly IRevisionGridInfo _revisionGridInfo;
    private readonly Func<IGitModule> _module;
    private readonly IRepoNameExtractor _repoNameExtractor;
    private IDisposable? _buildStatusCancellationToken;
    private IBuildServerAdapter? _buildServerAdapter;
    private readonly Lock _observerLock = new();

    internal BuildStatusColumnProvider ColumnProvider { get; }

    public BuildServerWatcher(RevisionGridControl revisionGrid, RevisionDataGridView revisionGridView, IRevisionGridInfo revisionGridInfo, Func<IGitModule> module)
    {
        _revisionGrid = revisionGrid;
        _revisionGridView = revisionGridView;
        _revisionGridInfo = revisionGridInfo;
        _module = module;

        _repoNameExtractor = new RepoNameExtractor(_module);
        ColumnProvider = new BuildStatusColumnProvider(revisionGrid, revisionGridView, _module);
    }

    public async Task LaunchBuildServerInfoFetchOperationAsync()
    {
        await TaskScheduler.Default;

        CancelBuildStatusFetchOperation();

        CancellationToken launchToken = _launchCancellation.Next();

        IBuildServerAdapter? buildServerAdapter = await GetBuildServerAdapterAsync().ConfigureAwait(false);

        await _revisionGridView.SwitchToMainThreadAsync(launchToken);

        _buildServerAdapter?.Dispose();
        _buildServerAdapter = buildServerAdapter;

        // When a build server adapter is available (including auto-detected),
        // ensure the column visibility and width reflect the user's display preferences
        if (buildServerAdapter is not null)
        {
            bool showIcon = AppSettings.ShowBuildStatusIconColumn;
            bool showText = AppSettings.ShowBuildStatusTextColumn;
            ColumnProvider.Column.Visible = showIcon || showText;

            if (showIcon && !showText)
            {
                ColumnProvider.Column.Width = DpiUtil.Scale(16);
            }
        }

        await TaskScheduler.Default;

        if (buildServerAdapter is null || launchToken.IsCancellationRequested)
        {
            return;
        }

        NewThreadScheduler scheduler = NewThreadScheduler.Default;

        // Run this first as it (may) force start queries
        IObservable<BuildInfo> runningBuildsObservable = buildServerAdapter.GetRunningBuilds(scheduler);

        IObservable<BuildInfo> fullDayObservable = buildServerAdapter.GetFinishedBuildsSince(scheduler, DateTime.Today - TimeSpan.FromDays(3));
        IObservable<BuildInfo> fullObservable = buildServerAdapter.GetFinishedBuildsSince(scheduler);

        bool anyRunningBuilds = false;
        IObservable<BuildInfo> delayObservable = Observable.Defer(() => Observable.Empty<BuildInfo>()
                                                               .DelaySubscription(anyRunningBuilds ? ShortPollInterval : LongPollInterval));

        bool shouldLookForNewlyFinishedBuilds = false;
        DateTime nowFrozen = DateTime.Now;

        // All finished builds have already been retrieved,
        // so looking for new finished builds make sense only if running builds have been found previously
        IObservable<BuildInfo> fromNowObservable = Observable.If(() => shouldLookForNewlyFinishedBuilds,
            buildServerAdapter.GetFinishedBuildsSince(scheduler, nowFrozen)
                        .Finally(() => shouldLookForNewlyFinishedBuilds = false));

        CancelBuildStatusFetchOperation();
        lock (_observerLock)
        {
            _buildStatusCancellationToken = new CompositeDisposable
                {
                    fullDayObservable.OnErrorResumeNext(fullObservable)
                                     .OnErrorResumeNext(Observable.Empty<BuildInfo>()
                                                                  .DelaySubscription(LongPollInterval)
                                                                  .OnErrorResumeNext(fromNowObservable)
                                                                  .Retry()
                                                                  .Repeat())
                                     .ObserveOn(MainThreadScheduler.Instance)
                                     .Subscribe(UpdateAndReportExceptions),

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
                                           .Subscribe(UpdateAndReportExceptions)
                };
        }

        await _revisionGridView.SwitchToMainThreadAsync(launchToken);

        return;

        void UpdateAndReportExceptions(BuildInfo buildInfo)
        {
            TaskManager.HandleExceptions(() => OnBuildInfoUpdate(buildInfo), Application.OnThreadException);
        }
    }

    public void CancelBuildStatusFetchOperation()
    {
        IDisposable? cancellationToken = Interlocked.Exchange(ref _buildStatusCancellationToken, null);

        cancellationToken?.Dispose();
    }

    public IBuildServerCredentials? GetBuildServerCredentials(IBuildServerAdapter buildServerAdapter, bool useStoredCredentialsIfExisting)
    {
        lock (_buildServerCredentialsLock)
        {
            IBuildServerCredentials? buildServerCredentials = new BuildServerCredentials { BuildServerCredentialsType = BuildServerCredentialsType.Guest };
            bool foundInConfig = false;

            const string CredentialsConfigName = "Credentials";
            const string UseGuestAccessKey = "UseGuestAccess";
            const string BuildServerCredentialsTypeKey = "BuildServerCredentialsType";
            const string UsernameKey = "Username";
            const string PasswordKey = "Password";
            const string BearerTokenKey = "BearerToken";
            using (IsolatedStorageFileStream stream = GetBuildServerOptionsIsolatedStorageStream(buildServerAdapter, FileAccess.Read, FileShare.Read))
            {
                if (stream.Position < stream.Length)
                {
                    byte[] protectedData = new byte[stream.Length];

                    stream.ReadExactly(protectedData, 0, (int)stream.Length);
                    try
                    {
                        byte[] unprotectedData = ProtectedData.Unprotect(protectedData, null,
                            DataProtectionScope.CurrentUser);
                        using MemoryStream memoryStream = new(unprotectedData);
                        ConfigFile credentialsConfig = new(fileName: "");

                        using (StreamReader textReader = new(memoryStream, Encoding.UTF8))
                        {
                            credentialsConfig.LoadFromString(textReader.ReadToEnd());
                        }

                        IConfigSection? section = credentialsConfig.FindConfigSection(CredentialsConfigName);

                        if (section is not null)
                        {
                            string? buildServerCredentialsType = section.GetValue(BuildServerCredentialsTypeKey);
                            if (!string.IsNullOrWhiteSpace(buildServerCredentialsType))
                            {
                                if (!Enum.TryParse(buildServerCredentialsType, ignoreCase: true, out BuildServerCredentialsType credentialsType))
                                {
                                    credentialsType = BuildServerCredentialsType.Guest;
                                }

                                buildServerCredentials.BuildServerCredentialsType = credentialsType;
                            }
                            else
                            {
                                buildServerCredentials.BuildServerCredentialsType =
                                    section.GetValueAsBool(UseGuestAccessKey, true)
                                        ? BuildServerCredentialsType.Guest
                                        : BuildServerCredentialsType.UsernameAndPassword;
                            }

                            buildServerCredentials.Username = section.GetValue(UsernameKey);
                            buildServerCredentials.Password = section.GetValue(PasswordKey);
                            buildServerCredentials.BearerToken = section.GetValue(BearerTokenKey);
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
                    ConfigFile credentialsConfig = new(fileName: "");

                    IConfigSection section = credentialsConfig.FindOrCreateConfigSection(CredentialsConfigName);

                    section.SetValue(BuildServerCredentialsTypeKey, buildServerCredentials.BuildServerCredentialsType.ToString());
                    section.SetValue(UsernameKey, buildServerCredentials.Username);
                    section.SetValue(PasswordKey, buildServerCredentials.Password);
                    section.SetValue(BearerTokenKey, buildServerCredentials.BearerToken);

                    using IsolatedStorageFileStream stream = GetBuildServerOptionsIsolatedStorageStream(buildServerAdapter, FileAccess.Write, FileShare.None);
                    using MemoryStream memoryStream = new();
                    using (StreamWriter textWriter = new(memoryStream, Encoding.UTF8))
                    {
                        textWriter.Write(credentialsConfig.GetAsString());
                    }

                    byte[] protectedData = ProtectedData.Protect(memoryStream.ToArray(), null, DataProtectionScope.CurrentUser);
                    stream.Write(protectedData, 0, protectedData.Length);

                    return buildServerCredentials;
                }
            }

            return null;
        }
    }

    public string ReplaceVariables(string projectNames)
    {
        (string repoProject, string repoName) = _repoNameExtractor.Get();

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

        DialogResult result = await form.ShowDialogAsync(_revisionGrid);
        if (result == DialogResult.OK)
        {
            return buildServerCredentials;
        }

        return null;
    }

    private void OnBuildInfoUpdate(BuildInfo buildInfo)
    {
        lock (_observerLock)
        {
            if (_buildStatusCancellationToken is null)
            {
                return;
            }
        }

        foreach (ObjectId commitHash in buildInfo.CommitHashList)
        {
            int? index = _revisionGridView.TryGetRevisionIndex(commitHash);

            if (!index.HasValue)
            {
                continue;
            }

            GitRevision? revision = _revisionGridView.GetRevision(index.Value);

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

        IBuildServerSettings buildServerSettings = _module().GetEffectiveSettings().GetBuildServerSettings();

        string? buildServerName = buildServerSettings.ServerName;

        if (!string.IsNullOrEmpty(buildServerName))
        {
            // A build server type is explicitly configured.
            // Only proceed if integration is enabled or hasn't been explicitly disabled.
            if (buildServerSettings.IntegrationEnabled == false)
            {
                return null;
            }
        }
        else
        {
            // Nothing configured. Auto-detect only when the user hasn't touched
            // integration settings at all (both ServerName and IntegrationEnabled are unset).
            if (buildServerSettings.IntegrationEnabled is not null)
            {
                return null;
            }

            buildServerName = TryAutoDetectBuildServerType(buildServerSettings.SettingsSource);
            if (string.IsNullOrEmpty(buildServerName))
            {
                return null;
            }
        }

        // When explicitly configured, let the matching detector populate settings from remotes
        TryPopulateSettingsForBuildServer(buildServerName, buildServerSettings.SettingsSource);

        IEnumerable<Lazy<IBuildServerAdapter, IBuildServerTypeMetadata>> exports = ManagedExtensibility.GetExports<IBuildServerAdapter, IBuildServerTypeMetadata>();
        Lazy<IBuildServerAdapter, IBuildServerTypeMetadata>? export = exports.SingleOrDefault(x => x.Metadata.BuildServerType == buildServerName);

        if (export is not null)
        {
            try
            {
                string? canBeLoaded = export.Metadata.CanBeLoaded;
                if (!string.IsNullOrEmpty(canBeLoaded))
                {
                    Debug.Write(export.Metadata.BuildServerType + " adapter could not be loaded: " + canBeLoaded);
                    return null;
                }

                IBuildServerAdapter buildServerAdapter = export.Value;

                buildServerAdapter.Initialize(this, buildServerSettings.SettingsSource,
                    () =>
                    {
                        // To run the `StartSettingsDialog()` in the UI Thread
                        ThreadHelper.JoinableTaskFactory.Run(async () =>
                            {
                                await _revisionGrid.SwitchToMainThreadAsync();
                                _revisionGrid.UICommands.StartSettingsDialog(typeof(BuildServerIntegrationSettingsPage));
                            });
                    },
                    objectId => _revisionGridInfo.GetRevision(objectId) is not null);
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

    /// <summary>
    ///  Attempts to detect the build server type from the repository's remote URLs
    ///  by querying registered <see cref="IBuildServerAutoDetector"/> exports.
    ///  When detected, writes adapter-specific settings to <paramref name="settingsSource"/>
    ///  (if not already set) so the adapter can use them without re-parsing.
    ///  Respects <see cref="AppSettings.PrioritizedBuildServerRemoteNames"/> for remote ordering,
    ///  so that forks resolve to the upstream project's CI rather than the fork's.
    /// </summary>
    private string? TryAutoDetectBuildServerType(GitExtensions.Extensibility.Settings.SettingsSource? settingsSource = null)
    {
        try
        {
            List<string> remoteUrls = GetOrderedRemoteUrls();
            if (remoteUrls.Count == 0)
            {
                return null;
            }

            foreach (Lazy<IBuildServerAutoDetector> export in ManagedExtensibility.GetExports<IBuildServerAutoDetector>())
            {
                if (export.Value.TryDetect(remoteUrls, settingsSource))
                {
                    return export.Value.BuildServerType;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Write($"Auto-detect build server failed: {ex}");
        }

        return null;
    }

    /// <summary>
    ///  For an explicitly configured build server, runs the matching auto-detector
    ///  to populate adapter-specific settings from remote URLs.
    /// </summary>
    private void TryPopulateSettingsForBuildServer(string buildServerName, GitExtensions.Extensibility.Settings.SettingsSource settingsSource)
    {
        try
        {
            List<string> remoteUrls = GetOrderedRemoteUrls();
            if (remoteUrls.Count == 0)
            {
                return;
            }

            foreach (Lazy<IBuildServerAutoDetector> export in ManagedExtensibility.GetExports<IBuildServerAutoDetector>())
            {
                if (export.Value.BuildServerType == buildServerName)
                {
                    export.Value.TryDetect(remoteUrls, settingsSource);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Write($"Populate build server settings failed: {ex}");
        }
    }

    /// <summary>
    ///  Collects remote URLs from the current module, ordered by
    ///  <see cref="AppSettings.PrioritizedBuildServerRemoteNames"/>.
    /// </summary>
    private List<string> GetOrderedRemoteUrls()
    {
        IGitModule module = _module();
        IReadOnlyList<string> remoteNames = module.GetRemoteNames();

        string[] prioritizedNames = AppSettings.PrioritizedBuildServerRemoteNames
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        IEnumerable<string> orderedRemotes = remoteNames
            .OrderBy(r =>
            {
                int index = Array.FindIndex(prioritizedNames, n => string.Equals(n, r, StringComparison.OrdinalIgnoreCase));
                return index >= 0 ? index : prioritizedNames.Length;
            });

        List<string> remoteUrls = [];
        foreach (string remoteName in orderedRemotes)
        {
            string remoteUrl = module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remoteName));
            if (!string.IsNullOrWhiteSpace(remoteUrl))
            {
                remoteUrls.Add(remoteUrl);
            }
        }

        return remoteUrls;
    }

    public void Dispose()
    {
        CancelBuildStatusFetchOperation();

        _buildServerAdapter?.Dispose();
        _launchCancellation.Dispose();
    }

    private static IsolatedStorageFileStream GetBuildServerOptionsIsolatedStorageStream(IBuildServerAdapter buildServerAdapter, FileAccess fileAccess, FileShare fileShare)
    {
        string fileName = string.Format("BuildServer-{0}.options", Convert.ToBase64String(Encoding.UTF8.GetBytes(buildServerAdapter.UniqueKey)));
        return new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, fileAccess, fileShare);
    }

    public void OnRepositoryChanged()
    {
        _buildServerAdapter?.OnRepositoryChanged();
    }
}
