using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;

namespace GitUI.UserControls
{
    public partial class GotoUserManualControl : GitExtensionsControl
    {
        public GotoUserManualControl()
        {
            InitializeComponent();
            Translate();
        }

        bool isLoaded = false;

        private void GotoUserManualControl_Load(object sender, EventArgs e)
        {
            isLoaded = true;
            UpdateTooltip();
        }

        string _manualSectionAnchorName;
        public string ManualSectionAnchorName
        {
            get { return _manualSectionAnchorName; }
            set { _manualSectionAnchorName = value; if (isLoaded) { UpdateTooltip(); } }
        }

        string _manualSectionSubfolder;
        public string ManualSectionSubfolder
        {
            get { return _manualSectionSubfolder; }
            set { _manualSectionSubfolder = value; if (isLoaded) { UpdateTooltip(); } }
        }

        private void UpdateTooltip()
        {
            string caption = string.Format("Read more about this feature at {0}", GetUrl());
            toolTip1.SetToolTip(labelHelpIcon, caption);
            toolTip1.SetToolTip(linkLabelHelp, caption);
        }

        private void OpenManual()
        {
            string url = GetUrl();
            OpenUrlInDefaultBrowser(url);
        }

        private string GetUrl()
        {
            return UserManual.UserManual.UrlFor(ManualSectionSubfolder, ManualSectionAnchorName);
        }

        /// <summary>
        /// opens urls even with anchor
        /// </summary>
        /// <returns></returns>
        private void OpenUrlInDefaultBrowser(string url)
        {
            // does not work with anchors: http://stackoverflow.com/questions/2404449/process-starturl-with-anchor-in-the-url
            ////Process.Start(url);

            var browserRegistryString  = Registry.ClassesRoot.OpenSubKey(@"\http\shell\open\command\").GetValue("").ToString();
            var defaultBrowserPath = System.Text.RegularExpressions.Regex.Match(browserRegistryString, @"(\"".*?\"")").Captures[0].ToString();
            Process.Start(defaultBrowserPath, url);
        }

        private void labelHelpIcon_Click(object sender, EventArgs e)
        {
            OpenManual();
        }

        private void linkLabelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenManual();
        }
    }
}
