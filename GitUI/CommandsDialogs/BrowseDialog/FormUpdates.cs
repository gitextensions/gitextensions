using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormUpdates : Form
    {
        public bool AutoClose;
        public string CurrentVersion;
        public bool UpdateFound;
        public string UpdateUrl;
        private const string FilesUrl = "gitextensions.googlecode.com/files/GitExtensions";
        private readonly SynchronizationContext syncContext;

        public FormUpdates(string currentVersion)
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
                    var regEx = new Regex(FilesUrl + @"(?<ver>\d{3,6})SetupComplete.msi");
                    
                    var matches = regEx.Matches(response);

                    foreach (Match match in matches)
                    {
                        int ver=int.Parse(match.Groups["ver"].Value);
                        int cver=int.Parse(CurrentVersion);

                            
                        if (ver <= cver)
                            continue;
                        
                        UpdateFound = true;
                        UpdateUrl = "http://" + match.Value;
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
}