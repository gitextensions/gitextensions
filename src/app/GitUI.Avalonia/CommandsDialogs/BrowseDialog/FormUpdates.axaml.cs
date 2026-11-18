using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.Compat;
using GitUI.UserControls.Settings;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.BrowseDialog;

// The shared release configuration describes Windows MSI packages, so Linux and macOS expose
// release links without attempting to execute the installer.
public partial class FormUpdates : GitExtensionsDialog
{
    private const string ReleasesConfigUrl =
        "https://raw.githubusercontent.com/gitextensions/gitextensions/configdata/GitExtensions.releases";

    private static readonly HttpClient HttpClient = new();

    #region Translation
    private readonly TranslationString _newVersionAvailable = new("There is a new version {0} of Git Extensions available");
    private readonly TranslationString _noUpdatesFound = new("No updates found");
    private readonly TranslationString _downloadingUpdate = new("Downloading update...");
    private readonly TranslationString _errorHeading = new("Download Failed");
    private readonly TranslationString _errorMessage = new("Failed to download an update.");
    #endregion

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private IWin32Window? _ownerWindow;
    private readonly Func<CancellationToken, Task<string>> _loadReleases;

    // Avalonia's designer constructs views before the application initializes ThreadHelper.
    private readonly TaskManager _operations = GitUI.Compat.DesignTimeTaskManager.Create();
    private bool _alwaysShow;
    private Version _currentVersion;
    private bool _updateFound;
    private string _netRuntimeDownloadUrl = string.Empty;
    private string _updateUrl = string.Empty;
    private string _newVersion = string.Empty;
    private Version? _requiredNetRuntimeVersion;

    public FormUpdates()
        : this(typeof(FormUpdates).Assembly.GetName().Version ?? new Version())
    {
    }

    public FormUpdates(Version currentVersion)
        : base(commands: null, enablePositionRestore: false)
    {
        _currentVersion = currentVersion;
        _loadReleases = static cancellationToken => HttpClient.GetStringAsync(ReleasesConfigUrl, cancellationToken);

        InitializeComponent();
        linkChangeLog.Click += linkChangeLog_LinkClicked;
        linkDirectDownload.Click += linkDirectDownload_LinkClicked;
        linkRequiredDotNetRuntime.InfoClicked += linkRequiredDotNetRuntime_InfoClicked;
        linkRequiredDotNetRuntime.LinkClicked += linkRequiredDotNetRuntime_LinkClicked;
        btnUpdateNow.Click += btnUpdateNow_Click;
        progressBar1.IsVisible = true;
        progressBar1.IsIndeterminate = true;
        linkRequiredDotNetRuntime.IsVisible = false;
        InitializeComplete();
    }

    public void SearchForUpdatesAndShow(IWin32Window ownerWindow, bool alwaysShow)
    {
        _ownerWindow = ownerWindow;
        _alwaysShow = alwaysShow;
        if (_ownerWindow is Window owner)
        {
            owner.Closed += OwnerWindow_Closed;
        }

        SearchForUpdates();

        if (alwaysShow)
        {
            ShowDialog(ownerWindow);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (ProcessCmdKey(KeysMapper.ToKeys(e)))
        {
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    protected bool ProcessCmdKey(WinFormsShims.Keys keyData)
    {
        // We need to override ProcessCmdKey as mnemonics on labels do not behave the same as buttons
        if (keyData == (WinFormsShims.Keys.Alt | WinFormsShims.Keys.L))
        {
            LaunchUrl(LaunchType.ChangeLog);
            return true;
        }

        if (keyData == (WinFormsShims.Keys.Alt | WinFormsShims.Keys.D))
        {
            LaunchUrl(LaunchType.DirectDownload);
            return true;
        }

        return false;
    }

    private void SearchForUpdates()
    {
        CancellationToken cancellationToken = _cancellationTokenSource.Token;
        _operations.FileAndForget(async () =>
        {
            try
            {
                await TaskScheduler.Default;
                string releases = await _loadReleases(cancellationToken);
                CheckForNewerVersion(releases);
                await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                Done();
            }
            catch (OperationCanceledException)
            {
                // OperationCanceledException is thrown
                // if a UI component gets disposed or the UI thread EXITs while a 'check for updates' thread
                // is in the middle of its run... Ignore it, likely the user has closed the app
            }
            catch (HttpRequestException exception)
                when (exception.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests)
            {
                // GitHub rate limiting - suppress the exception and do not show it to the user.
                // Nothing we can do here, ignore it.
                await DisplayFailureAsync(exception, suppressMessage: true, cancellationToken);
            }
            catch (Exception exception)
            {
                await DisplayFailureAsync(exception, suppressMessage: !_alwaysShow, cancellationToken);
            }
        });
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(
            nameof(FormUpdates),
            nameof(linkRequiredDotNetRuntime),
            "Text",
            linkRequiredDotNetRuntime.Text!);
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        string source = linkRequiredDotNetRuntime.Text!;
        linkRequiredDotNetRuntime.Text = translation.TranslateItem(
            nameof(FormUpdates),
            nameof(linkRequiredDotNetRuntime),
            "Text",
            () => source) ?? source;
        linkRequiredDotNetRuntime.ToolTipText = ToolTip.GetTip(linkRequiredDotNetRuntime) as string;
    }

    protected override void OnClosed(EventArgs e)
    {
        _cancellationTokenSource.Cancel();
        DetachOwner();
        _operations.JoinPendingOperations();
        _cancellationTokenSource.Dispose();
        base.OnClosed(e);
    }

    private void CheckForNewerVersion(string releases)
    {
        IEnumerable<ReleaseVersion> versions = ReleaseVersion.Parse(releases);
        ReleaseVersion? update = ReleaseVersion
            .GetNewerVersions(_currentVersion, AppSettings.CheckForReleaseCandidates, versions)
            .OrderBy(version => version.ApplicationVersion)
            .LastOrDefault();

        _updateFound = update is not null;
        if (update is null)
        {
            _updateUrl = string.Empty;
            _requiredNetRuntimeVersion = null;
            _newVersion = string.Empty;
            return;
        }

        _updateUrl = AdaptFromX64ToCurrentProcessArchitecture(update.DownloadPage);
        _requiredNetRuntimeVersion = update.RequiredNetRuntimeVersion;
        _newVersion = update.ApplicationVersion.ToString();
        return;

        string AdaptFromX64ToCurrentProcessArchitecture(string link)
            => RuntimeInformation.OSArchitecture == Architecture.X64
                ? link
                : link.Replace(
                    "-x64-",
                    $"-{RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()}-",
                    StringComparison.Ordinal);
    }

    private void Done()
    {
        progressBar1.IsVisible = false;
        if (!_updateFound)
        {
            UpdateLabel.Text = _noUpdatesFound.Text;
            if (!_alwaysShow)
            {
                DetachOwner();
            }

            return;
        }

        UpdateLabel.Text = string.Format(_newVersionAvailable.Text, _newVersion);
        linkChangeLog.IsVisible = true;
        linkDirectDownload.IsVisible = true;

        if (OperatingSystem.IsWindows()
            && UpdateRequired(
                _requiredNetRuntimeVersion,
                UserEnvironmentInformation.GetDotnetDesktopRuntimeVersions()))
        {
            DisplayNetRuntimeLink(linkRequiredDotNetRuntime.Text!, _requiredNetRuntimeVersion!);
        }

        if (CanInstallUpdate(OperatingSystem.IsWindows(), AppSettings.IsPortable()))
        {
            btnUpdateNow.IsVisible = true;
            btnUpdateNow.Focus();
        }
        else
        {
            linkDirectDownload.Focus();
        }

        if (!_alwaysShow && _ownerWindow is Window { IsVisible: true } owner)
        {
            // Do not await the modal lifetime from the owner task. The search then leaves the
            // task collection before FormUpdates.OnClosed joins outstanding operations.
            _ = ShowDialog(owner);
        }
        else if (!_alwaysShow)
        {
            DetachOwner();
        }
    }

    private async Task DisplayFailureAsync(
        Exception exception,
        bool suppressMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        progressBar1.IsVisible = false;
        UpdateLabel.Text = _noUpdatesFound.Text;
        if (!suppressMessage && IsVisible)
        {
            ExceptionUtils.ShowException(this, exception, string.Empty, true);
        }

        if (!_alwaysShow)
        {
            DetachOwner();
        }
    }

    private void DisplayNetRuntimeLink(string format, Version requiredNetRuntimeVersion)
    {
        if (requiredNetRuntimeVersion is null || !OperatingSystem.IsWindows())
        {
            linkRequiredDotNetRuntime.IsVisible = false;
            return;
        }

        string versionText1 = requiredNetRuntimeVersion.ToString(fieldCount: 2);
        string versionText2 = requiredNetRuntimeVersion.ToString(fieldCount: 3);
        string versionText3 = requiredNetRuntimeVersion.ToString(fieldCount: 1);
        linkRequiredDotNetRuntime.Text = string.Format(format, versionText1, versionText2, versionText3);

        // The aka.ms/dotnet-core-applaunch URL expects the architecture and RID in lowercase (e.g. x64, arm64).
        string arch = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();
        _netRuntimeDownloadUrl =
            $"https://aka.ms/dotnet-core-applaunch?missing_runtime=true&arch={arch}&rid=win-{arch}&apphost_version={versionText2}&gui=true";
        linkRequiredDotNetRuntime.IsVisible = true;
    }

    private void LaunchUrl(LaunchType launchType)
    {
        const string releases = @"https://github.com/gitextensions/gitextensions/releases";
        string? url = launchType switch
        {
            LaunchType.ChangeLog => releases,
            LaunchType.DirectDownload when OperatingSystem.IsWindows() && !AppSettings.IsPortable()
                => _updateUrl,
            LaunchType.DirectDownload => releases,
            LaunchType.DotNetRuntime => _netRuntimeDownloadUrl,
            LaunchType.LocalDotNetRuntime
                => "https://github.com/gitextensions/gitextensions/wiki/.NET-Desktop-Runtime",
            _ => null,
        };

        if (!string.IsNullOrWhiteSpace(url))
        {
            OsShellUtil.OpenUrlInDefaultBrowser(url);
        }
    }

    private void linkChangeLog_LinkClicked(object? sender, EventArgs e)
        => LaunchUrl(LaunchType.ChangeLog);

    private void linkDirectDownload_LinkClicked(object? sender, EventArgs e)
        => LaunchUrl(LaunchType.DirectDownload);

    private void linkRequiredDotNetRuntime_InfoClicked(object? sender, EventArgs e)
        => LaunchUrl(LaunchType.LocalDotNetRuntime);

    private void linkRequiredDotNetRuntime_LinkClicked(object? sender, EventArgs e)
        => LaunchUrl(LaunchType.DotNetRuntime);

    private void btnUpdateNow_Click(object? sender, EventArgs e)
    {
        if (!CanInstallUpdate(OperatingSystem.IsWindows(), AppSettings.IsPortable()))
        {
            LaunchUrl(LaunchType.DirectDownload);
            return;
        }

        linkChangeLog.IsVisible = false;
        progressBar1.IsVisible = true;
        progressBar1.IsIndeterminate = true;
        btnUpdateNow.IsEnabled = false;
        UpdateLabel.Text = _downloadingUpdate.Text;

        CancellationToken cancellationToken = _cancellationTokenSource.Token;
        _operations.FileAndForget(async () =>
        {
            try
            {
                await TaskScheduler.Default;
                string fileName = Path.GetFileName(new Uri(_updateUrl).LocalPath);
                string installerPath = Path.Combine(Path.GetTempPath(), fileName);
                using HttpResponseMessage response = await HttpClient.GetAsync(
                    _updateUrl,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                response.EnsureSuccessStatusCode();
                await using Stream source = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using FileStream destination = File.Create(installerPath);
                await source.CopyToAsync(destination, cancellationToken);

                string msiexecPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.System),
                    "msiexec.exe");
                ProcessStartInfo startInfo = new(msiexecPath)
                {
                    UseShellExecute = false,
                };
                startInfo.ArgumentList.Add("/i");
                startInfo.ArgumentList.Add(installerPath);
                startInfo.ArgumentList.Add("/qb");
                startInfo.ArgumentList.Add("LAUNCH=1");
                Process.Start(startInfo);

                await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                progressBar1.IsVisible = false;
                Dispatcher.UIThread.Post(() =>
                {
                    Close();
                    if (Avalonia.Application.Current?.ApplicationLifetime
                        is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.Shutdown();
                    }
                });
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                try
                {
                    await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                progressBar1.IsVisible = false;
                btnUpdateNow.IsEnabled = true;
                MessageBoxes.Show(
                    this,
                    _errorMessage.Text + Environment.NewLine + exception.Message,
                    _errorHeading.Text,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Warning);
            }
        });
    }

    private void OwnerWindow_Closed(object? sender, EventArgs e)
    {
        _cancellationTokenSource.Cancel();
        if (IsVisible)
        {
            Close();
        }
    }

    private void DetachOwner()
    {
        if (_ownerWindow is Window owner)
        {
            owner.Closed -= OwnerWindow_Closed;
        }

        _ownerWindow = null;
    }

    private static bool CanInstallUpdate(bool isWindows, bool isPortable)
        => isWindows && !isPortable;

    private static bool UpdateRequired(Version? required, IEnumerable<Version> installed)
    {
        if (required is null)
        {
            return false;
        }

        IEnumerable<Version> matchingMajor = installed.Where(version => version.Major == required.Major);
        return !matchingMajor.Any(version => version >= required);
    }

    internal enum LaunchType
    {
        ChangeLog,
        DirectDownload,
        DotNetRuntime,
        LocalDotNetRuntime
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly FormUpdates _form;

        public TestAccessor(FormUpdates form)
        {
            _form = form;
        }

        public SettingsLinkLabel linkRequiredNetRuntime => _form.linkRequiredDotNetRuntime;

        public string NetRuntimeDownloadUrl => _form._netRuntimeDownloadUrl;

        public void DisplayNetRuntimeLink(string format, Version requiredNetRuntimeVersion)
            => _form.DisplayNetRuntimeLink(format, requiredNetRuntimeVersion);

        public void LaunchUrl(LaunchType launchType) => _form.LaunchUrl(launchType);
    }
}
