﻿using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using Git.hub;
using GitCommands;
using GitCommands.Config;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormUpdates : GitExtensionsForm
    {
        #region Translation
        private readonly TranslationString _newVersionAvailable = new("There is a new version {0} of Git Extensions available");
        private readonly TranslationString _noUpdatesFound = new("No updates found");
        private readonly TranslationString _downloadingUpdate = new("Downloading update...");
        private readonly TranslationString _errorHeading = new("Download Failed");
        private readonly TranslationString _errorMessage = new("Failed to download an update.");
        #endregion

        public IWin32Window? OwnerWindow;
        public Version CurrentVersion { get; }
        public bool UpdateFound;
        public string UpdateUrl = "";
        public string NewVersion = "";

        public FormUpdates(Version currentVersion)
        {
            CurrentVersion = currentVersion;

            InitializeComponent();
            InitializeComplete();

            progressBar1.Visible = true;
            progressBar1.Style = ProgressBarStyle.Marquee;
        }

        public void SearchForUpdatesAndShow(IWin32Window ownerWindow, bool alwaysShow)
        {
            OwnerWindow = ownerWindow;
            new Thread(SearchForUpdates).Start();
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

                var configData = gitExtRepo?.GetRef("heads/configdata");

                var tree = configData?.GetTree();
                if (tree is null)
                {
                    return;
                }

                var releases = tree.Tree.FirstOrDefault(entry => "GitExtensions.releases".Equals(entry.Path, StringComparison.InvariantCultureIgnoreCase));

                if (releases?.Blob.Value is not null)
                {
                    CheckForNewerVersion(releases.Blob.Value.GetContent());
                }
            }
            catch (InvalidAsynchronousStateException)
            {
                // InvalidAsynchronousStateException (The destination thread no longer exists) is thrown
                // if a UI component gets disposed or the UI thread EXITs while a 'check for updates' thread
                // is in the middle of its run... Ignore it, likely the user has closed the app
            }
            catch (NullReferenceException)
            {
                // We had a number of NRE reports.
                // Most likely scenario is that GitHub is API rate limiting unauthenticated requests that lead to failures in Git.hub library.
                // Nothing we can do here, ignore it.
            }
            catch (Exception ex)
            {
                this.InvokeSync(() =>
                    {
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
            var versions = ReleaseVersion.Parse(releases);
            var updates = ReleaseVersion.GetNewerVersions(CurrentVersion, AppSettings.CheckForReleaseCandidates, versions);

            var update = updates.OrderBy(version => version.Version).LastOrDefault();
            if (update is not null)
            {
                UpdateFound = true;
                UpdateUrl = update.DownloadPage;
                NewVersion = update.Version.ToString();
                Done();
                return;
            }

            UpdateUrl = "";
            UpdateFound = false;
            Done();
        }

        private void Done()
        {
            this.InvokeSync(() =>
            {
                progressBar1.Visible = false;

                if (UpdateFound)
                {
                    btnUpdateNow.Visible = true;
                    UpdateLabel.Text = string.Format(_newVersionAvailable.Text, NewVersion);
                    linkChangeLog.Visible = true;
                    linkDirectDownload.Visible = true;

                    if (!Visible)
                    {
                        ShowDialog(OwnerWindow);
                    }
                }
                else
                {
                    UpdateLabel.Text = _noUpdatesFound.Text;
                }
            });
        }

        private void LaunchUrl(LaunchType launchType)
        {
            switch (launchType)
            {
                case LaunchType.ChangeLog:
                    OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/blob/master/GitUI/Resources/ChangeLog.md");
                    break;
                case LaunchType.DirectDownload:
                    OsShellUtil.OpenUrlInDefaultBrowser(UpdateUrl);
                    break;
            }
        }

        private void linkChangeLog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LaunchUrl(LaunchType.ChangeLog);
        }

        private void btnUpdateNow_Click(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                linkChangeLog.Visible = false;
                progressBar1.Visible = true;
                btnUpdateNow.Enabled = false;
                UpdateLabel.Text = _downloadingUpdate.Text;
                string fileName = Path.GetFileName(UpdateUrl);

                try
                {
#pragma warning disable SYSLIB0014 // 'WebClient' is obsolete
                    using WebClient webClient = new();
#pragma warning restore SYSLIB0014 // 'WebClient' is obsolete
                    await webClient.DownloadFileTaskAsync(new Uri(UpdateUrl), Environment.GetEnvironmentVariable("TEMP") + "\\" + fileName);
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

                    progressBar1.Visible = false;
                    Close();
                    Application.Exit();
                }
                catch (Win32Exception)
                {
                }
            }).FileAndForget();
        }

        private void linkDirectDownload_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LaunchUrl(LaunchType.DirectDownload);
        }
    }

    internal enum LaunchType
    {
        ChangeLog,
        DirectDownload
    }

    public enum ReleaseType
    {
        Major,
        HotFix,
        ReleaseCandidate
    }

    public class ReleaseVersion
    {
        public Version Version { get; }
        public ReleaseType ReleaseType { get; }
        public string DownloadPage { get; }

        public ReleaseVersion(Version version, ReleaseType releaseType, string downloadPage)
        {
            Version = version;
            ReleaseType = releaseType;
            DownloadPage = downloadPage;
        }

        public static ReleaseVersion? FromSection(IConfigSection section)
        {
            Version ver;
            try
            {
                ver = new Version(section.SubSection);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }

            Enum.TryParse(section.GetValue("ReleaseType"), true, out ReleaseType releaseType);

            return new ReleaseVersion(ver, releaseType, section.GetValue("DownloadPage"));
        }

        public static IEnumerable<ReleaseVersion> Parse(string versionsStr)
        {
            ConfigFile cfg = new("", true);
            cfg.LoadFromString(versionsStr);
            var sections = cfg.GetConfigSections("Version");
            sections = sections.Concat(cfg.GetConfigSections("RCVersion"));

            return sections.Select(FromSection).WhereNotNull();
        }

        public static IEnumerable<ReleaseVersion> GetNewerVersions(
            Version currentVersion,
            bool checkForReleaseCandidates,
            IEnumerable<ReleaseVersion> availableVersions)
        {
            var versions = availableVersions.Where(version =>
                    version.ReleaseType == ReleaseType.Major ||
                    version.ReleaseType == ReleaseType.HotFix ||
                    (checkForReleaseCandidates && version.ReleaseType == ReleaseType.ReleaseCandidate));

            return versions.Where(version => version.Version.CompareTo(currentVersion) > 0);
        }
    }
}
