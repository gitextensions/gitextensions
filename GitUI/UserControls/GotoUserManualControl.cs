using System;
using System.Windows.Forms;
using ResourceManager;

namespace GitUI.UserControls
{
    public partial class GotoUserManualControl : GitExtensionsControl
    {
        private readonly TranslationString _gotoUserManualControlTooltip =
            new TranslationString("Read more about this feature at {0}");

        public GotoUserManualControl()
        {
            InitializeComponent();
            InitializeComplete();
        }

        private bool _isLoaded;

        private void GotoUserManualControl_Load(object sender, EventArgs e)
        {
            _isLoaded = true;
            UpdateTooltip();
        }

        private string _manualSectionAnchorName;
        public string ManualSectionAnchorName
        {
            get { return _manualSectionAnchorName; }
            set
            {
                _manualSectionAnchorName = value;
                if (_isLoaded)
                {
                    UpdateTooltip();
                }
            }
        }

        private string _manualSectionSubfolder;
        public string ManualSectionSubfolder
        {
            get { return _manualSectionSubfolder; }
            set
            {
                _manualSectionSubfolder = value;
                if (_isLoaded)
                {
                    UpdateTooltip();
                }
            }
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

        private void linkLabelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenManual();
        }
    }
}
