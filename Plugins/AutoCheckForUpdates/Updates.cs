using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AutoCheckForUpdates
{
    public partial class Updates : Form
    {
        public bool AutoClose;
        public string CurrentVersion;
        public bool UpdateFound;
        public string UpdateUrl;
        private const string FilesUrl = "http://gitextensions.googlecode.com/files/GitExtensions";
        private readonly SynchronizationContext syncContext;

        public Updates(string currentVersion)
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

        private void CloseClick(object sender, EventArgs e)
        {
            Close();
        }

        private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(link.Text);
        }

        private void SearchForUpdates()
        {
            var webClient = new WebClient { Proxy = WebRequest.DefaultWebProxy };
            webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webClient.Encoding = Encoding.UTF8;
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_DownloadStringCompleted);
            webClient.DownloadStringAsync(new Uri(@"http://code.google.com/p/gitextensions/"));
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
                    // search for string like "http://gitextensions.googlecode.com/files/GitExtensions170SetupComplete.msi"
                    var regEx = new Regex(FilesUrl + @"[0-9][0-9][0-9]SetupComplete.msi");

                    var matches = regEx.Matches(response);

                    foreach (Match match in matches)
                    {
                        if (match.Value.Equals(FilesUrl + CurrentVersion + "SetupComplete.msi"))
                            continue;

                        UpdateFound = true;
                        UpdateUrl = match.Value;
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
                MessageBox.Show(ex.Message, "Exception");
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
    }
}