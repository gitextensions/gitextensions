﻿using GitCommands;
using GitCommands.Settings;
using GitUIPluginInterfaces.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class DetailedSettingsPage : DistributedSettingsPage
    {
        public DetailedSettingsPage(IServiceProvider serviceProvider)
           : base(serviceProvider)
        {
            InitializeComponent();
            InitializeComplete();
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(DetailedSettingsPage));
        }

        protected override void OnRuntimeLoad()
        {
            base.OnRuntimeLoad();

            // align 1st columns across all tables
            tlpnlEmailSettings.AdjustWidthToSize(0, addLogMessages);
        }

        protected override void SettingsToPage()
        {
            chkMergeGraphLanesHavingCommonParent.Checked = AppSettings.MergeGraphLanesHavingCommonParent.Value;
            chkRenderGraphWithDiagonals.Checked = AppSettings.RenderGraphWithDiagonals.Value;
            chkStraightenGraphDiagonals.Checked = AppSettings.StraightenGraphDiagonals.Value;

            IDetailedSettings detailedSettings = GetCurrentSettings()
                .Detailed();

            SmtpServer.Text = detailedSettings.SmtpServer;
            SmtpServerPort.Text = detailedSettings.SmtpPort.ToString();
            chkUseSSL.Checked = detailedSettings.SmtpUseSsl;

            chkRemotesFromServer.Checked = detailedSettings.GetRemoteBranchesDirectlyFromRemote;
            addLogMessages.Checked = detailedSettings.AddMergeLogMessages;
            nbMessages.Text = detailedSettings.MergeLogMessagesCount.ToString();

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            AppSettings.MergeGraphLanesHavingCommonParent.Value = chkMergeGraphLanesHavingCommonParent.Checked;
            AppSettings.RenderGraphWithDiagonals.Value = chkRenderGraphWithDiagonals.Checked;
            AppSettings.StraightenGraphDiagonals.Value = chkStraightenGraphDiagonals.Checked;

            IDetailedSettings detailedSettings = GetCurrentSettings()
                .Detailed();

            detailedSettings.SmtpServer = SmtpServer.Text;

            if (int.TryParse(SmtpServerPort.Text, out int port))
            {
                detailedSettings.SmtpPort = port;
            }

            detailedSettings.SmtpUseSsl = chkUseSSL.Checked;

            detailedSettings.GetRemoteBranchesDirectlyFromRemote = chkRemotesFromServer.Checked;
            detailedSettings.AddMergeLogMessages = addLogMessages.Checked;

            if (int.TryParse(nbMessages.Text, out int messagesCount))
            {
                detailedSettings.MergeLogMessagesCount = messagesCount;
            }

            base.PageToSettings();
        }

        private void chkUseSSL_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkUseSSL.Checked)
            {
                if (SmtpServerPort.Text == "587")
                {
                    SmtpServerPort.Text = "465";
                }
            }
            else
            {
                if (SmtpServerPort.Text == "465")
                {
                    SmtpServerPort.Text = "587";
                }
            }
        }
    }
}
