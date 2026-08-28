using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using GitCommands;
using GitCommands.Config;
using GitCommands.Remotes;
using GitCommands.Settings;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtUtils.GitUI;
using GitUI.CommandsDialogs;
using GitUI.HelperDialogs;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft.VisualStudio.Threading;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.BuildServerIntegration;

public sealed class BuildServerWatcher : IBuildServerWatcher, IDisposable
{
    private static readonly TimeSpan ShortPollInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan LongPollInterval = TimeSpan.FromSeconds(120);

    private readonly CancellationTokenSequence _launchCancellation = new();
    private readonly Lock _buildServerCredentialsLock = new();
    private readonly BuildServerCredentialStore _credentialStore;
    private readonly RevisionGridControl _revisionGrid;
    private readonly IRevisionGridInfo _revisionGridInfo;
    private readonly Func<IGitModule> _module;
    private readonly IRepoNameExtractor _repoNameExtractor;
    private IDisposable? _buildStatusCancellationToken;
    private IBuildServerAdapter? _buildServerAdapter;
    private readonly Lock _observerLock = new();

    internal BuildStatusColumnProvider ColumnProvider { get; }

    public BuildServerWatcher(RevisionGridControl revisionGrid, IRevisionGridInfo revisionGridInfo, Func<IGitModule> module)
    {
        _revisionGrid = revisionGrid;
        _revisionGridInfo = revisionGridInfo;
        _module = module;

        _credentialStore = new BuildServerCredentialStore(() => _module().WorkingDir);
        _repoNameExtractor = new RepoNameExtractor(_module);
        ColumnProvider = new BuildStatusColumnProvider(OpenBuildReport);
    }

    public async Task LaunchBuildServerInfoFetchOperationAsync()
    {
        await TaskScheduler.Default;

        CancelBuildStatusFetchOperation();
        CancellationToken launchToken = _launchCancellation.Next();
        IBuildServerAdapter? buildServerAdapter = await GetBuildServerAdapterAsync().ConfigureAwait(false);

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(launchToken);

        _buildServerAdapter?.Dispose();

        // When a build server adapter is available (including auto-detected),
        // ensure the column visibility and width reflect the user's display preferences
        _buildServerAdapter = buildServerAdapter;
        ColumnProvider.Column.IsAvailable = buildServerAdapter is not null;
        _revisionGrid.ApplyColumnSettings();

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
        IObservable<BuildInfo> fromNowObservable = Observable.If(
            () => shouldLookForNewlyFinishedBuilds,
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

                runningBuildsObservable.Do(_ =>
                    {
                        anyRunningBuilds = true;
                        shouldLookForNewlyFinishedBuilds = true;
                    })
                    .OnErrorResumeNext(delayObservable)
                    .Retry()
                    .Finally(() => anyRunningBuilds = false)
                    .Repeat()
                    .ObserveOn(MainThreadScheduler.Instance)
                    .Subscribe(UpdateAndReportExceptions),
            };
        }

        void UpdateAndReportExceptions(BuildInfo buildInfo)
            => TaskManager.HandleExceptions(() => OnBuildInfoUpdate(buildInfo), WinFormsShims.Application.OnThreadException);
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
            BuildServerCredentials? storedCredentials = _credentialStore.Get(buildServerAdapter.UniqueKey);
            BuildServerCredentials credentials;
            if (storedCredentials is not null)
            {
                credentials = Clone(storedCredentials);
                if (useStoredCredentialsIfExisting)
                {
                    return credentials;
                }
            }
            else
            {
                credentials = new BuildServerCredentials { BuildServerCredentialsType = BuildServerCredentialsType.Guest };
            }

            IBuildServerCredentials? enteredCredentials = ThreadHelper.JoinableTaskFactory.Run(
                () => ShowBuildServerCredentialsFormAsync(buildServerAdapter.UniqueKey, credentials));
            if (enteredCredentials is null)
            {
                return null;
            }

            credentials = Clone(enteredCredentials);
            _credentialStore.Save(buildServerAdapter.UniqueKey, credentials);
            return Clone(credentials);
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

    private async Task<IBuildServerCredentials?> ShowBuildServerCredentialsFormAsync(
        string buildServerUniqueKey,
        IBuildServerCredentials buildServerCredentials)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        using FormBuildServerCredentials form = new(buildServerUniqueKey)
        {
            BuildServerCredentials = buildServerCredentials,
        };
        WinFormsShims.IWin32Window? owner = Avalonia.Controls.TopLevel.GetTopLevel(_revisionGrid) as WinFormsShims.IWin32Window;
        return form.ShowDialog(owner) == DialogResult.OK ? form.BuildServerCredentials : null;
    }

    internal void OnBuildInfoUpdate(BuildInfo buildInfo)
    {
        lock (_observerLock)
        {
            if (_buildStatusCancellationToken is null)
            {
                return;
            }
        }

        bool changed = false;
        foreach (ObjectId commitHash in buildInfo.CommitHashList)
        {
            GitRevision? revision = _revisionGrid.GetRevision(commitHash);
            if (revision is null || (revision.BuildStatus is not null && buildInfo.StartDate < revision.BuildStatus.StartDate))
            {
                continue;
            }

            revision.BuildStatus = buildInfo;
            changed = true;
        }

        if (changed)
        {
            _revisionGrid.RefreshRealizedRows();
        }
    }

    private async Task<IBuildServerAdapter?> GetBuildServerAdapterAsync()
    {
        await TaskScheduler.Default;

        SettingsSource effectiveSettings = _module().GetEffectiveSettings();
        string? buildServerName = BuildServerSettings.ServerName[effectiveSettings];
        if (!string.IsNullOrEmpty(buildServerName))
        {
            // A build server type is explicitly configured.
            // Only bail out if integration has been explicitly disabled.
            if (BuildServerSettings.IntegrationEnabled[effectiveSettings] is false)
            {
                return null;
            }
        }
        else
        {
            // Nothing configured. Auto-detect only when the user hasn't touched
            // integration settings at all (both ServerName and IntegrationEnabled are unset).
            if (BuildServerSettings.IntegrationEnabled[effectiveSettings] is not null)
            {
                return null;
            }

            buildServerName = TryAutoDetectBuildServerType(BuildServerSettings.GetSettingsSource(effectiveSettings));
            if (string.IsNullOrEmpty(buildServerName))
            {
                return null;
            }
        }

        // When explicitly configured, let the matching detector populate settings from remotes
        TryPopulateSettingsForBuildServer(buildServerName, BuildServerSettings.GetSettingsSource(effectiveSettings));
        Lazy<IBuildServerAdapter, IBuildServerTypeMetadata>? export = ManagedExtensibility
            .GetExports<IBuildServerAdapter, IBuildServerTypeMetadata>()
            .SingleOrDefault(item => item.Metadata.BuildServerType == buildServerName);
        if (export is null)
        {
            return null;
        }

        try
        {
            if (!string.IsNullOrEmpty(export.Metadata.CanBeLoaded))
            {
                Debug.Write($"{export.Metadata.BuildServerType} adapter could not be loaded: {export.Metadata.CanBeLoaded}");
                return null;
            }

            IBuildServerAdapter adapter = export.Value;

            // To run the `StartSettingsDialog()` in the UI Thread
            adapter.Initialize(
                this,
                BuildServerSettings.GetSettingsSource(effectiveSettings),
                () => ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    _revisionGrid.UICommands.StartPluginSettingsDialog(
                        Avalonia.Controls.TopLevel.GetTopLevel(_revisionGrid) as WinFormsShims.IWin32Window);
                }),
                objectId => _revisionGridInfo.GetRevision(objectId) is not null);
            return adapter;
        }
        catch (InvalidOperationException ex)
        {
            Debug.Write(ex);
            return null;
        }
    }

    /// <summary>
    ///  Attempts to detect the build server type from the repository's remote URLs
    ///  by querying registered <see cref="IBuildServerAutoDetector"/> exports.
    ///  When detected, writes adapter-specific settings to <paramref name="settingsSource"/>
    ///  (if not already set) so the adapter can use them without re-parsing.
    ///  Respects <see cref="AppSettings.PrioritizedBuildServerRemoteNames"/> for remote ordering,
    ///  so that forks resolve to the upstream project's CI rather than the fork's.
    /// </summary>
    private string? TryAutoDetectBuildServerType(SettingsSource? settingsSource = null)
    {
        try
        {
            List<string> remoteUrls = GetOrderedRemoteUrls();
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
    private void TryPopulateSettingsForBuildServer(string buildServerName, SettingsSource settingsSource)
    {
        try
        {
            List<string> remoteUrls = GetOrderedRemoteUrls();
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
        string[] prioritizedNames = AppSettings.PrioritizedBuildServerRemoteNames
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IEnumerable<string> orderedRemotes = module.GetRemoteNames().OrderBy(remoteName =>
        {
            int index = Array.FindIndex(
                prioritizedNames,
                prioritizedName => string.Equals(prioritizedName, remoteName, StringComparison.OrdinalIgnoreCase));
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

    public void OnRepositoryChanged()
        => _buildServerAdapter?.OnRepositoryChanged();

    private void OpenBuildReport(GitRevision revision)
        => OsShellUtil.OpenUrlInDefaultBrowser(revision.BuildStatus?.Url);

    private static BuildServerCredentials Clone(IBuildServerCredentials credentials)
        => new()
        {
            BuildServerCredentialsType = credentials.BuildServerCredentialsType,
            Username = credentials.Username,
            Password = credentials.Password,
            BearerToken = credentials.BearerToken,
        };
}
