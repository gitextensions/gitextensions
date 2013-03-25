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
        private string ManualLocation;

        /// <summary>
        /// TODO: make customizable
        /// </summary>
        private ManualType ManualType;

        public GotoUserManualControl()
        {
            InitializeComponent();
            Translate();

            // set online manual
            ManualLocation = @"https://gitextensions.readthedocs.org/en/latest";
            ManualType = ManualType.StandardHtml;

            // set local singlehtml help / TODO: put manual to GitExt setup
            ////ManualLocation = @"file:///D:/data2/projects/gitextensions/GitExtensionsDoc/build/singlehtml";
            ////ManualType = ManualType.SingleHtml;
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
        /// <summary>
        /// only needed when ManualType is StandardHtml (not needed for SingleHtml)
        /// </summary>
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
            switch (ManualType)
            {
                case UserControls.ManualType.SingleHtml:
                    return string.Format("{0}/index.html#{1}", ManualLocation, ManualSectionAnchorName);

                case UserControls.ManualType.StandardHtml:
                    return string.Format("{0}/{1}/#{2}", ManualLocation, ManualSectionSubfolder, ManualSectionAnchorName);

                default:
                    throw new NotImplementedException();
            }
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

    public enum ManualType
    {
        /// <summary>
        /// all in one html page
        /// </summary>
        SingleHtml,

        /// <summary>
        /// html with subfolders
        /// </summary>
        StandardHtml
    }
}
