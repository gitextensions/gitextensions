using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace AutoCheckForUpdates
{
    delegate void DoneCallback();

    public partial class Updates : Form
    {
        public Updates(string currentVersion)
        {
            InitializeComponent();
            UpdateFound = false;
            link.Visible = false;
            UpdateLabel.Text = "Searching for updates";
            progressBar1.Visible = true;
            this.CurrentVersion = currentVersion;
            UpdateUrl = "";
            progressBar1.Style = ProgressBarStyle.Marquee;
        }

        private void Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(link.Text);
        }

        public bool UpdateFound;
        public bool AutoClose = false;
        public string CurrentVersion;
        public string UpdateUrl;

        private void SearchForUpdates()
        {
            try
            {
                WebClient webClient = new WebClient();
                webClient.Proxy = WebRequest.DefaultWebProxy;
                webClient.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                webClient.Encoding = System.Text.Encoding.UTF8;

                string response = webClient.DownloadString(@"http://code.google.com/p/gitextensions/");

                //find for string like "http://gitextensions.googlecode.com/files/GitExtensions170SetupComplete.msi"
                Regex regEx = new Regex(@"http://gitextensions.googlecode.com/files/GitExtensions[0-9][0-9][0-9]SetupComplete.msi");

                MatchCollection matches = regEx.Matches(response);

                foreach (Match match in matches)
                {
                    if (!match.Value.Equals("http://gitextensions.googlecode.com/files/GitExtensions" + CurrentVersion + "SetupComplete.msi"))
                    {
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
            if (link.InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                DoneCallback d = new DoneCallback(Done);
                this.Invoke(d, new object[] {});
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
                        this.Close();
                }

            }
        }

        private void Updates_Load(object sender, EventArgs e)
        {
        }

        private void Updates_Shown(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(SearchForUpdates));
            thread.Start();
        }
    }
}
