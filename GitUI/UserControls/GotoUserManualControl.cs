using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;

namespace GitUI.UserControls
{
    public partial class GotoUserManualControl : GitExtensionsControl
    {
        /// <summary>
        /// TODO: make customizable
        /// </summary>
        private const string ManualLocation = @"file:///D:/data2/projects/gitextensions/GitExtensionsDoc/build/singlehtml";

        public GotoUserManualControl()
        {
            InitializeComponent();
            Translate();
        }

        string _manualSectionAnchorName;
        public string ManualSectionAnchorName
        {
            get { return _manualSectionAnchorName; }
            set { _manualSectionAnchorName = value; UpdateTooltip(); }
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
            return string.Format("{0}/index.html#{1}", ManualLocation, ManualSectionAnchorName);
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
