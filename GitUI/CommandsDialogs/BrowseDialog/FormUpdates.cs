using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using GitCommands.Config;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormUpdates : Form
    {
        public bool AutoClose;
        public Version CurrentVersion;
        public bool UpdateFound;
        public string UpdateUrl;
        private const string FilesUrl = "gitextensions.googlecode.com/files/GitExtensions";
        private readonly SynchronizationContext syncContext;

        public FormUpdates(Version currentVersion)
        {
            syncContext = SynchronizationContext.Current;
            InitializeComponent();
            UpdateFound = false;
            link.Visible = false;
            UpdateLabel.Text = "Searching for updates";
            progressBar1.Visible = true;
            CurrentVersion = currentVersion;
            UpdateUrl = "";
            progressBar1.Style = ProgressBarStyle.Marquee;
        }

        private void CloseButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(link.Text);
            }
            catch (System.ComponentModel.Win32Exception)
            {
            }
        }

        private void SearchForUpdates()
        {
            var webClient = new WebClient { Proxy = WebRequest.DefaultWebProxy };
            webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webClient.Encoding = Encoding.UTF8;
            webClient.DownloadProgressChanged += webClient_DownloadProgressChanged;
            webClient.DownloadStringCompleted += webClient_DownloadStringCompleted;
            webClient.DownloadStringAsync(new Uri(@"https://raw.github.com/gitextensions/gitextensions/configdata/GitExtensions.releases"));
        }

        void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            syncContext.Send(o =>
            {
                progressBar1.Style = ProgressBarStyle.Continuous;
                progressBar1.Maximum = 100;
                progressBar1.Value = e.ProgressPercentage;
            }, this);
        }

        void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    string response = e.Result;
                    var versions = ReleaseVersion.Parse(response);
                    var updates = versions.Where(version => version.Version.CompareTo(CurrentVersion) > 0);
                    
                    var update = updates.OrderBy(version => version.Version).LastOrDefault();
                    if (update != null)
                    {

                        UpdateFound = true;
                        UpdateUrl = update.DownloadPage;
                        Done();
                        return;
                    }
                }
                UpdateUrl = "";
                UpdateFound = false;
                Done();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Exception");
                Done();
            }
        }


        private void Done()
        {
            syncContext.Send(o =>
            {
                progressBar1.Visible = false;
                link.Text = UpdateUrl;

                if (UpdateFound)
                {
                    link.Visible = true;
                    linkChangeLog.Visible = true;

                    UpdateLabel.Text = "There is a new version available";
                }
                else
                {
                    UpdateLabel.Text = "No updates found";
                    if (AutoClose)
                        Close();
                }
            }, this);
        }

        private void UpdatesShown(object sender, EventArgs e)
        {
            new Thread(SearchForUpdates).Start();
        }

        private void linkChangeLog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/gitextensions/gitextensions/blob/master/GitUI/Resources/ChangeLog.md");
        }
    }

    public class ReleaseVersion
    {
        public Version Version;
        public string ReleaseType;
        public string DownloadPage;

        public static ReleaseVersion FromSection(ConfigSection section)
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

            return new ReleaseVersion()
            {
                Version = ver,
                ReleaseType = section.GetValue("ReleaseType"),
                DownloadPage = section.GetValue("DownloadPage")
            };

        }

        public static IEnumerable<ReleaseVersion> Parse(string versionsStr)
        {
            ConfigFile cfg = new ConfigFile("", true);
            cfg.LoadFromString(versionsStr);
            var sections = cfg.GetConfigSections("Version");

            return sections.Select(section => ReleaseVersion.FromSection(section)).Where(version => version != null);
        }

    }

}