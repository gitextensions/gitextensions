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

        public string ManualSectionAnchorName { get; set; }

        private void OpenManual()
        {
            string url = string.Format("{0}/index.html#{1}", ManualLocation, ManualSectionAnchorName);
            OpenUrlInDefaultBrowser(url);
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
