using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using Git.hub;
using GitCommands.Config;
using GitCommands;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormUpdates : GitExtensionsForm
    {
        #region Translation
        private readonly TranslationString _newVersionAvailable =
            new TranslationString("There is a new version {0} of Git Extensions available");
        private readonly TranslationString _noUpdatesFound =
            new TranslationString("No updates found");
        #endregion

        public IWin32Window OwnerWindow;
        public Version CurrentVersion;
        public bool UpdateFound;
        public string UpdateUrl;
        public string NewVersion;

        public FormUpdates(Version currentVersion)
        {
            InitializeComponent();
            Translate();
            UpdateFound = false;
            progressBar1.Visible = true;
            CurrentVersion = currentVersion;
            UpdateUrl = "";
            NewVersion = "";
            progressBar1.Style = ProgressBarStyle.Marquee;
        }

        private void CloseButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        public void SearchForUpdatesAndShow(IWin32Window aOwnerWindow, bool alwaysShow)
        {
            OwnerWindow = aOwnerWindow;
            new Thread(SearchForUpdates).Start();
            if (alwaysShow)
                ShowDialog(aOwnerWindow);
        }

        private void SearchForUpdates()
        {
            try
            {
                Client github = new Client();
                Repository gitExtRepo = github.getRepository("gitextensions", "gitextensions");
                if (gitExtRepo == null)
                    return;

                var configData = gitExtRepo.GetRef("heads/configdata");
                if (configData == null)
                    return;

                var tree = configData.GetTree();
                if (tree == null)
                    return;

                var releases = tree.Tree.Where(entry => "GitExtensions.releases".Equals(entry.Path, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (releases != null && releases.Blob.Value != null)
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
            catch (Exception ex)
            {
                this.InvokeSync(state =>
                    {
                        if (Visible)
                        {
                            ExceptionUtils.ShowException(this, ex, string.Empty, true);
                        }
                    }, null);
                Done();
            }

        }

        void CheckForNewerVersion(string releases)
        {
            var versions = ReleaseVersion.Parse(releases);
            var updates = ReleaseVersion.GetNewerVersions(CurrentVersion, AppSettings.CheckForReleaseCandidates, versions);

            var update = updates.OrderBy(version => version.Version).LastOrDefault();
            if (update != null)
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
            this.InvokeSync(o =>
            {
                progressBar1.Visible = false;

                if (UpdateFound)
                {
                    btnDownloadNow.Enabled = true;
                    UpdateLabel.Text = string.Format(_newVersionAvailable.Text, NewVersion);
                    linkChangeLog.Visible = true;

                    if (!Visible)
                        ShowDialog(OwnerWindow);
                }
                else
                {
                    UpdateLabel.Text = _noUpdatesFound.Text;
                }
            }, this);
        }

        private void linkChangeLog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/gitextensions/gitextensions/blob/master/GitUI/Resources/ChangeLog.md");
        }

        private void btnDownloadNow_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(UpdateUrl);
            }
            catch (System.ComponentModel.Win32Exception)
            {
            }
        }
    }

    public enum ReleaseType
    {
        Major,
        HotFix,
        ReleaseCandidate
    }

    public class ReleaseVersion
    {
        public Version Version;
        public ReleaseType ReleaseType;
        public string DownloadPage;

        public static ReleaseVersion FromSection(IConfigSection section)
        {
            Version ver;
            try
            {
                ver = new Version(section.SubSection);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }

            var version = new ReleaseVersion()
            {
                Version = ver,
                ReleaseType = ReleaseType.Major,
                DownloadPage = section.GetValue("DownloadPage")
            };

            Enum.TryParse<ReleaseType>(section.GetValue("ReleaseType"), true, out version.ReleaseType);

            return version;

        }

        public static IEnumerable<ReleaseVersion> Parse(string versionsStr)
        {
            ConfigFile cfg = new ConfigFile("", true);
            cfg.LoadFromString(versionsStr);
            var sections = cfg.GetConfigSections("Version");
            sections = sections.Concat(cfg.GetConfigSections("RCVersion"));

            return sections.Select(FromSection).Where(version => version != null);
        }

        public static IEnumerable<ReleaseVersion> GetNewerVersions(
            Version currentVersion,
            bool checkForReleaseCandidates,
            IEnumerable<ReleaseVersion> availableVersions)
        {
            var versions = availableVersions.Where(version =>
                    version.ReleaseType == ReleaseType.Major ||
                    version.ReleaseType == ReleaseType.HotFix ||
                    checkForReleaseCandidates && version.ReleaseType == ReleaseType.ReleaseCandidate);

            return versions.Where(version => version.Version.CompareTo(currentVersion) > 0);
        }

    }

}