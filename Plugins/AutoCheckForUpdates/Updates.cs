using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AutoCheckForUpdates
{
    internal delegate void DoneCallback();

    public partial class Updates : Form
    {
        public bool AutoClose;
        public string CurrentVersion;
        public bool UpdateFound;
        public string UpdateUrl;
        private const string FilesUrl = "http://gitextensions.googlecode.com/files/GitExtensions";

        public Updates(string currentVersion)
        {
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
            try
            {
                var webClient = new WebClient {Proxy = WebRequest.DefaultWebProxy};
                webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
                webClient.Encoding = Encoding.UTF8;

                var response = webClient.DownloadString(@"http://code.google.com/p/gitextensions/");

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
            if (link.InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                DoneCallback d = Done;
                Invoke(d, new object[] {});
            }
            else
            {
                progressBar1.Visible = false;
                link.Text = UpdateUrl;
                link.Visible = true;
                if (UpdateFound)
                {
                    UpdateLabel.Text = "There is a new version available";
                }
                else
                {
                    UpdateLabel.Text = "No updates found";
                    if (AutoClose)
                        Close();
                }
            }
        }

        private void UpdatesShown(object sender, EventArgs e)
        {
            new Thread(SearchForUpdates).Start();
        }
    }
}