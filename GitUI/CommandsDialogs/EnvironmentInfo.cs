using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    public partial class EnvironmentInfo : UserControl
    {
        public EnvironmentInfo()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || GitModuleForm.IsUnitTestActive)
            {
                UserEnvironmentInformation.Initialise(
                "9999999999999999999999999999999999abcdef", true);
            }

            InitializeComponent();

            environmentIssueInfo.Text = UserEnvironmentInformation.GetInformation().Replace("- ", "");
        }

        public ToolTip ToolTip { get; set; }

        public void SetCopyButtonTooltip(string tooltip)
        {
            ToolTip?.SetToolTip(copyButton, tooltip);
        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            UserEnvironmentInformation.CopyInformation();
        }
    }
}
