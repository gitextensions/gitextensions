using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Git.hub;
using GitCommands;
using GitUI.UserControls.Settings;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog;

public partial class FormUpdates : GitExtensionsDialog
{
    #region Translation
    private readonly TranslationString _newVersionAvailable = new("There is a new version {0} of Git Extensions available");
    private readonly TranslationString _noUpdatesFound = new("No updates found");
    private readonly TranslationString _downloadingUpdate = new("Downloading update...");
    private readonly TranslationString _errorHeading = new("Download Failed");
    private readonly TranslationString _errorMessage = new("Failed to download an update.");
    #endregion

    private IWin32Window? _ownerWindow;
    private Version _currentVersion;
    private bool _updateFound;
    private string _netRuntimeDownloadUrl = string.Empty;
    private string _updateUrl = string.Empty;
    private string _newVersion = string.Empty;
    private Version? _requiredNetRuntimeVersion;

    public FormUpdates(Version currentVersion)
        : base(commands: null, enablePositionRestore: false)
    {
        _currentVersion = currentVersion;

        InitializeComponent();
        InitializeComplete();

        progressBar1.Visible = true;
        progressBar1.Style = ProgressBarStyle.Marquee;

        linkRequiredDotNetRuntime.Visible = false;
    }

    public void SearchForUpdatesAndShow(IWin32Window ownerWindow, bool alwaysShow)
    {
        _ownerWindow = ownerWindow;
        ThreadHelper.FileAndForget(SearchForUpdates);
        if (alwaysShow)
        {
            ShowDialog(ownerWindow);
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // We need to override ProcessCmdKey as mnemonics on labels do not behave the same as buttons
        if (keyData == (Keys.Alt | Keys.L))
        {
            LaunchUrl(LaunchType.ChangeLog);
        }
        else if (keyData == (Keys.Alt | Keys.D))
        {
            LaunchUrl(LaunchType.DirectDownload);
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void SearchForUpdates()
    {
        try
        {
            Client github = new();
            Repository gitExtRepo = github.getRepository("gitextensions", "gitextensions");

            GitHubReference configData = gitExtRepo?.GetRef("heads/configdata");

            GitHubTree tree = configData?.GetTree();
            if (tree is null)
            {
                return;
            }

            GitHubTreeEntry releases = tree.Tree.FirstOrDefault(entry => "GitExtensions.releases".Equals(entry.Path, StringComparison.InvariantCultureIgnoreCase));

            if (releases?.Blob.Value is not null)
            {
                CheckForNewerVersion(releases.Blob.Value.GetContent());
            }
        }
        catch (Exception ex) when (ex.Message.Contains("rate limit", StringComparison.OrdinalIgnoreCase))
        {
            // GitHub API rate limiting - suppress the exception and do not show it to the user.
            // Nothing we can do here, ignore it.
            Done();
        }
        catch (InvalidAsynchronousStateException)
        {
            // InvalidAsynchronousStateException (The destination thread no longer exists) is thrown
            // if a UI component gets disposed or the UI thread EXITs while a 'check for updates' thread
            // is in the middle of its run... Ignore it, likely the user has closed the app
        }
        catch (Exception ex)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await this.SwitchToMainThreadAsync();
                    if (Visible)
                    {
                        ExceptionUtils.ShowException(this, ex, string.Empty, true);
                    }
                });
            Done();
        }
    }

    private void CheckForNewerVersion(string releases)
    {
        IEnumerable<ReleaseVersion> versions = ReleaseVersion.Parse(releases);
        IEnumerable<ReleaseVersion> updates = ReleaseVersion.GetNewerVersions(_currentVersion, AppSettings.CheckForReleaseCandidates, versions);

        ReleaseVersion update = updates.OrderBy(version => version.ApplicationVersion).LastOrDefault();
        if (update is not null)
        {
            _updateFound = true;
            _updateUrl = AdaptFromX64ToCurrentProcessArchitecture(update.DownloadPage);
            _requiredNetRuntimeVersion = update.RequiredNetRuntimeVersion;
            _newVersion = update.ApplicationVersion.ToString();
            Done();
            return;
        }

        _updateUrl = string.Empty;
        _requiredNetRuntimeVersion = null;
        _updateFound = false;
        Done();

        return;

        string AdaptFromX64ToCurrentProcessArchitecture(string link)
            => RuntimeInformation.OSArchitecture == Architecture.X64
                ? link
                : link.Replace("-x64-", $"-{RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()}-");
    }

    private void Done()
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            await this.SwitchToMainThreadAsync();

            progressBar1.Visible = false;

            if (_updateFound)
            {
                UpdateLabel.Text = string.Format(_newVersionAvailable.Text, _newVersion);
                linkChangeLog.Visible = true;
                linkDirectDownload.Visible = true;

                if (UpdateRequired(_requiredNetRuntimeVersion, UserEnvironmentInformation.GetDotnetDesktopRuntimeVersions()))
                {
                    DisplayNetRuntimeLink(format: linkRequiredDotNetRuntime.Text, _requiredNetRuntimeVersion);
                }

                if (AppSettings.IsPortable())
                {
                    linkDirectDownload.Focus();
                }
                else
                {
                    btnUpdateNow.Visible = true;
                    btnUpdateNow.Focus();
                }

                if (!Visible)
                {
                    await ShowDialogAsync(_ownerWindow);
                }
            }
            else
            {
                UpdateLabel.Text = _noUpdatesFound.Text;
            }
        });
    }

    private void DisplayNetRuntimeLink(string format, Version requiredNetRuntimeVersion)
    {
        if (requiredNetRuntimeVersion is null)
        {
            linkRequiredDotNetRuntime.Visible = false;
            return;
        }

        string versionText1 = requiredNetRuntimeVersion.ToString(fieldCount: 2);
        string versionText2 = requiredNetRuntimeVersion.ToString(fieldCount: 3);
        string versionText3 = requiredNetRuntimeVersion.ToString(fieldCount: 1);
        linkRequiredDotNetRuntime.Text = string.Format(format, versionText1, versionText2, versionText3);

        int start = linkRequiredDotNetRuntime.Text.IndexOf(versionText2, StringComparison.Ordinal);
        int length = versionText2.Length;
        linkRequiredDotNetRuntime.LinkArea = new LinkArea(start, length);

        _netRuntimeDownloadUrl = $@"https://aka.ms/dotnet-core-applaunch?missing_runtime=true&arch={RuntimeInformation.OSArchitecture}&rid=win-{RuntimeInformation.OSArchitecture}&apphost_version={requiredNetRuntimeVersion.ToString(fieldCount: 3)}&gui=true";

        linkRequiredDotNetRuntime.Visible = true;
    }

    private void LaunchUrl(LaunchType launchType)
    {
        const string releases = @"https://github.com/gitextensions/gitextensions/releases";
        switch (launchType)
        {
            case LaunchType.ChangeLog:
                OsShellUtil.OpenUrlInDefaultBrowser(releases);
                break;

            case LaunchType.DirectDownload:
                if (AppSettings.IsPortable())
                {
                    OsShellUtil.OpenUrlInDefaultBrowser(releases);
                }
                else
                {
                    OsShellUtil.OpenUrlInDefaultBrowser(_updateUrl);
                }

                break;

            case LaunchType.DotNetRuntime:
                if (!string.IsNullOrWhiteSpace(_netRuntimeDownloadUrl))
                {
                    OsShellUtil.OpenUrlInDefaultBrowser(_netRuntimeDownloadUrl);
                }

                break;

            case LaunchType.LocalDotNetRuntime:
                OsShellUtil.OpenUrlInDefaultBrowser("https://github.com/gitextensions/gitextensions/wiki/.NET-Desktop-Runtime");
                break;
        }
    }

    private void linkChangeLog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => LaunchUrl(LaunchType.ChangeLog);

    private void linkDirectDownload_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => LaunchUrl(LaunchType.DirectDownload);

    private void linkRequiredDotNetRuntime_InfoClicked(object sender, EventArgs e) => LaunchUrl(LaunchType.LocalDotNetRuntime);

    private void linkRequiredDotNetRuntime_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => LaunchUrl(LaunchType.DotNetRuntime);

    private void btnUpdateNow_Click(object sender, EventArgs e)
    {
        linkChangeLog.Visible = false;
        progressBar1.Visible = true;
        btnUpdateNow.Enabled = false;
        UpdateLabel.Text = _downloadingUpdate.Text;

        ThreadHelper.FileAndForget(async () =>
        {
            string fileName = Path.GetFileName(_updateUrl);
            try
            {
#pragma warning disable SYSLIB0014 // 'WebClient' is obsolete
                using WebClient webClient = new();
#pragma warning restore SYSLIB0014 // 'WebClient' is obsolete
                await webClient.DownloadFileTaskAsync(new Uri(_updateUrl), Environment.GetEnvironmentVariable("TEMP") + "\\" + fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _errorMessage.Text + Environment.NewLine + ex.Message, _errorHeading.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Process process = new();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = "msiexec.exe";
                process.StartInfo.Arguments = string.Format("/i \"{0}\\{1}\" /qb LAUNCH=1", Environment.GetEnvironmentVariable("TEMP"), fileName);
                process.Start();

                await this.SwitchToMainThreadAsync();
                progressBar1.Visible = false;
                Close();
                Application.Exit();
            }
            catch (Win32Exception)
            {
            }
        });
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

        public SettingsLinkLabel linkRequiredNetRuntime => _form.linkRequiredDotNetRuntime;

        public string NetRuntimeDownloadUrl => _form._netRuntimeDownloadUrl;

        public void DisplayNetRuntimeLink(string format, Version requiredNetRuntimeVersion) => _form.DisplayNetRuntimeLink(format, requiredNetRuntimeVersion);

        public void LaunchUrl(LaunchType launchType) => _form.LaunchUrl(launchType);
    }
}
