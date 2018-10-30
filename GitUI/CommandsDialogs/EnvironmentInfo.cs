using System;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    public partial class EnvironmentInfo : UserControl
    {
        public EnvironmentInfo()
        {
            InitializeComponent();

            environmentIssueInfo.Text = UserEnvironmentInformation.GetInformation().Replace("-", "");
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
