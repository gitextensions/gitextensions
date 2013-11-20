using System;
using System.Windows.Forms;
using ResourceManager.Translation;

namespace GitUI.UserControls
{
    public partial class GotoUserManualControl : GitExtensionsControl
    {
        private readonly TranslationString _gotoUserManualControlTooltip =
            new TranslationString("Read more about this feature at {0}");

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
            string caption = string.Format(_gotoUserManualControlTooltip.Text, GetUrl());
            toolTip1.SetToolTip(pictureBoxHelpIcon, caption);
            toolTip1.SetToolTip(linkLabelHelp, caption);
        }

        private void OpenManual()
        {
            string url = GetUrl();
            OsShellUtil.OpenUrlInDefaultBrowser(url);
        }

        private string GetUrl()
        {
            return UserManual.UserManual.UrlFor(ManualSectionSubfolder, ManualSectionAnchorName);
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
