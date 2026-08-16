using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.UserControls.Settings;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.BrowseDialog;

// Twin of CommandsDialogs/BrowseDialog/FormUpdates. The shared release configuration still
// describes Windows MSI packages, so Linux and macOS deliberately expose release links
// without attempting to execute that installer.
public sealed partial class FormUpdates : GitExtensionsDialog
{
    private const string ReleasesPage = "https://github.com/gitextensions/gitextensions/releases";
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
    private readonly Version _currentVersion;
    private readonly Func<CancellationToken, Task<string>> _loadReleases;

    // Avalonia's designer constructs views before the application initializes ThreadHelper.
    private readonly TaskManager _operations = GitUI.Compat.DesignTimeTaskManager.Create();
    private bool _alwaysShow;
    private bool _updateFound;
    private string _netRuntimeDownloadUrl = string.Empty;
    private string _newVersion = string.Empty;
    private string _updateUrl = string.Empty;
    private Version? _requiredNetRuntimeVersion;
    private Window? _ownerWindow;

    public FormUpdates()
        : this(AppSettings.AppVersion)
    {
    }

    public FormUpdates(Version currentVersion)
        : this(currentVersion, LoadReleasesAsync)
    {
    }

    internal FormUpdates(
        Version currentVersion,
        Func<CancellationToken, Task<string>> loadReleases)
        : base(commands: null, enablePositionRestore: false)
    {
        _currentVersion = currentVersion;
        _loadReleases = loadReleases;

        InitializeComponent();
        WireEvents();
        InitializeComplete();
    }

    public void SearchForUpdatesAndShow(IWin32Window ownerWindow, bool alwaysShow)
    {
        _ownerWindow = ownerWindow as Window;
        _alwaysShow = alwaysShow;
        if (_ownerWindow is not null)
        {
            _ownerWindow.Closed += OwnerWindow_Closed;
        }

        CancellationToken cancellationToken = _cancellationTokenSource.Token;
        _operations.FileAndForget(async () =>
        {
            try
            {
                await TaskScheduler.Default;
                string releases = await _loadReleases(cancellationToken);
                ReleaseVersion? update = GetNewestUpdate(releases);
                await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                DisplayResult(update);
            }
            catch (OperationCanceledException)
            {
            }
            catch (HttpRequestException exception)
                when (exception.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests)
            {
                await DisplayFailureAsync(exception, suppressMessage: true, cancellationToken);
            }
            catch (Exception exception)
            {
                await DisplayFailureAsync(exception, suppressMessage: !alwaysShow, cancellationToken);
            }
        });

        if (alwaysShow)
        {
            ShowDialog(ownerWindow);
        }
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

    private static Task<string> LoadReleasesAsync(CancellationToken cancellationToken)
        => HttpClient.GetStringAsync(ReleasesConfigUrl, cancellationToken);

    private void WireEvents()
    {
        linkChangeLog.Click += linkChangeLog_LinkClicked;
        linkDirectDownload.Click += linkDirectDownload_LinkClicked;
        linkRequiredDotNetRuntime.InfoClicked += linkRequiredDotNetRuntime_InfoClicked;
        linkRequiredDotNetRuntime.LinkClicked += linkRequiredDotNetRuntime_LinkClicked;
        btnUpdateNow.Click += btnUpdateNow_Click;
        progressBar1.IsVisible = true;
        progressBar1.IsIndeterminate = true;
        linkRequiredDotNetRuntime.IsVisible = false;
    }

    private ReleaseVersion? GetNewestUpdate(string releases)
    {
        IEnumerable<ReleaseVersion> versions = ReleaseVersion.Parse(releases);
        return ReleaseVersion
            .GetNewerVersions(_currentVersion, AppSettings.CheckForReleaseCandidates, versions)
            .OrderBy(version => version.ApplicationVersion)
            .LastOrDefault();
    }

    private void DisplayResult(ReleaseVersion? update)
    {
        progressBar1.IsVisible = false;
        _updateFound = update is not null;
        if (update is null)
        {
            _updateUrl = string.Empty;
            _requiredNetRuntimeVersion = null;
            UpdateLabel.Text = _noUpdatesFound.Text;
            if (!_alwaysShow)
            {
                DetachOwner();
            }

            return;
        }

        _updateUrl = AdaptFromX64ToCurrentProcessArchitecture(update.DownloadPage);
        _requiredNetRuntimeVersion = update.RequiredNetRuntimeVersion;
        _newVersion = update.ApplicationVersion.ToString();
        UpdateLabel.Text = string.Format(_newVersionAvailable.Text, _newVersion);
        linkChangeLog.IsVisible = true;
        linkDirectDownload.IsVisible = true;

        if (OperatingSystem.IsWindows()
            && UpdateRequired(
                _requiredNetRuntimeVersion,
                GetDotnetDesktopRuntimeVersions()))
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

        if (!_alwaysShow && _ownerWindow is { IsVisible: true } owner)
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

    private static string AdaptFromX64ToCurrentProcessArchitecture(string link)
        => RuntimeInformation.OSArchitecture == Architecture.X64
            ? link
            : link.Replace(
                "-x64-",
                $"-{RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()}-",
                StringComparison.Ordinal);

    private void DisplayNetRuntimeLink(string format, Version? requiredNetRuntimeVersion)
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
        string? url = launchType switch
        {
            LaunchType.ChangeLog => ReleasesPage,
            LaunchType.DirectDownload when OperatingSystem.IsWindows() && !AppSettings.IsPortable()
                => _updateUrl,
            LaunchType.DirectDownload => ReleasesPage,
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
        if (_ownerWindow is not null)
        {
            _ownerWindow.Closed -= OwnerWindow_Closed;
            _ownerWindow = null;
        }
    }

    private static bool CanInstallUpdate(bool isWindows, bool isPortable)
        => isWindows && !isPortable;

    private static IEnumerable<Version> GetDotnetDesktopRuntimeVersions()
    {
        if (!OperatingSystem.IsWindows())
        {
            return [];
        }

        try
        {
            string? dotnetHostPath = GetDotnetHostPath();
            if (dotnetHostPath is null)
            {
                return [];
            }

            ProcessStartInfo startInfo = new(dotnetHostPath)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };
            startInfo.ArgumentList.Add("--list-runtimes");
            using Process? process = Process.Start(startInfo);
            if (process is null)
            {
                return [];
            }

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => line.StartsWith("Microsoft.WindowsDesktop.App ", StringComparison.Ordinal))
                .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1))
                .Select(version => Version.TryParse(version, out Version? parsed) ? parsed : null)
                .WhereNotNull()
                .ToArray();
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException)
        {
            return [];
        }
    }

    private static string? GetDotnetHostPath()
    {
        string executableName = OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";
        string? hostPath = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
        if (!string.IsNullOrWhiteSpace(hostPath)
            && Path.IsPathFullyQualified(hostPath)
            && File.Exists(hostPath))
        {
            return hostPath;
        }

        string? dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (!string.IsNullOrWhiteSpace(dotnetRoot))
        {
            hostPath = Path.Combine(dotnetRoot, executableName);
            if (File.Exists(hostPath))
            {
                return hostPath;
            }
        }

        if (OperatingSystem.IsWindows())
        {
            hostPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "dotnet",
                executableName);
            if (File.Exists(hostPath))
            {
                return hostPath;
            }
        }

        string? path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        foreach (string pathEntry in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            string directory = pathEntry.Trim().Trim('"');
            if (!Path.IsPathFullyQualified(directory))
            {
                continue;
            }

            hostPath = Path.Combine(directory, executableName);
            if (File.Exists(hostPath))
            {
                return Path.GetFullPath(hostPath);
            }
        }

        return null;
    }

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

        public Button UpdateNow => _form.btnUpdateNow;

        public HyperlinkButton ChangeLog => _form.linkChangeLog;

        public HyperlinkButton DirectDownload => _form.linkDirectDownload;

        public SettingsLinkLabel RequiredNetRuntime => _form.linkRequiredDotNetRuntime;

        public TextBlock UpdateText => _form.UpdateLabel;

        public string NetRuntimeDownloadUrl => _form._netRuntimeDownloadUrl;

        public string UpdateUrl => _form._updateUrl;

        public bool UpdateFound => _form._updateFound;

        public void DisplayNetRuntimeLink(string format, Version? requiredNetRuntimeVersion)
            => _form.DisplayNetRuntimeLink(format, requiredNetRuntimeVersion);

        public void DisplayReleases(string releases)
            => _form.DisplayResult(_form.GetNewestUpdate(releases));

        public Task JoinOperationsAsync(CancellationToken cancellationToken = default)
            => _form._operations.JoinPendingOperationsAsync(cancellationToken);

        public static bool CanInstall(bool isWindows, bool isPortable)
            => CanInstallUpdate(isWindows, isPortable);
    }
}
